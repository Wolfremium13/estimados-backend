using System.Collections.Concurrent;
using Common.Estimation.EstimationSession.Application.Contracts;
using Common.Estimation.RoomAccess.Application.Contracts;
using Microsoft.AspNetCore.SignalR;

namespace Wolfremium.Estimados.Hubs;

public class RoomHub(
    IDisconnectModeratorUseCase disconnectModeratorUseCase,
    IDisconnectParticipantUseCase disconnectParticipantUseCase,
    ICastVoteUseCase castVoteUseCase,
    IRevealVotesUseCase revealVotesUseCase,
    IResetVotesUseCase resetVotesUseCase,
    IHubContext<RoomHub> hubContext,
    ILogger<RoomHub> logger
) : Hub
{
    private static readonly ConcurrentDictionary<string, Guid> ModeratorConnections = new();
    private static readonly ConcurrentDictionary<string, (Guid RoomId, string Name)> ParticipantConnections = new();
    private static readonly ConcurrentDictionary<string, CancellationTokenSource> DisconnectionDelays = new();

    public async Task JoinRoomAsModerator(Guid roomId)
    {
        if (ModeratorConnections.Any(kvp => kvp.Value == roomId && kvp.Key != Context.ConnectionId))
        {
            throw new HubException("A moderator is already connected to this room.");
        }

        var roomIdStr = $"room_{roomId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, roomIdStr);
        ModeratorConnections[Context.ConnectionId] = roomId;

        var key = $"moderator_{roomId}";
        if (DisconnectionDelays.TryRemove(key, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("SignalR Hub: Cancelled pending disconnection for moderator of room {RoomId}.",
                    roomId);
        }

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("SignalR Hub: Connection {ConnectionId} joined room {RoomId} as Moderator.",
                Context.ConnectionId, roomId);
    }

    public async Task JoinRoomAsParticipantWithName(Guid roomId, string name)
    {
        if (ParticipantConnections.Any(kvp =>
                kvp.Value.RoomId == roomId && kvp.Value.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                kvp.Key != Context.ConnectionId))
        {
            throw new HubException("A participant with this name is already connected to this room.");
        }

        var roomIdStr = $"room_{roomId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, roomIdStr);
        ParticipantConnections[Context.ConnectionId] = (roomId, name);

        var key = $"{roomId}_{name}";
        if (DisconnectionDelays.TryRemove(key, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation(
                    "SignalR Hub: Cancelled pending disconnection for participant {Name} in room {RoomId}.", name,
                    roomId);
        }

        // Notify that the participant is online
        await Clients.Group(roomIdStr).SendAsync("OnParticipantConnectionStatusChanged", name, true);

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("SignalR Hub: Connection {ConnectionId} joined room {RoomId} as Participant {Name}.",
                Context.ConnectionId, roomId, name);
    }

    public async Task JoinRoomAsParticipant(Guid roomId)
    {
        var roomIdStr = $"room_{roomId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, roomIdStr);

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation(
                "SignalR Hub: Connection {ConnectionId} joined room {RoomId} as anonymous Participant.",
                Context.ConnectionId, roomId);
    }

    public async Task CastVote(Guid roomId, string participantName, string cardValue)
    {
        var result = await castVoteUseCase.Execute(new CastVoteCommand(roomId, participantName, cardValue));
        await result.Match(
            async success => { await Clients.Group($"room_{roomId}").SendAsync("OnVoteCast", participantName); },
            error => { throw new HubException(error.Message); }
        );
    }

    public async Task RevealVotes(Guid roomId)
    {
        var result = await revealVotesUseCase.Execute(new RevealVotesCommand(roomId));
        await result.Match(
            async dto =>
            {
                if (dto.CurrentState == "Halted")
                {
                    await Clients.Group($"room_{roomId}")
                        .SendAsync("OnSessionHalted", "The story is too complex and must be split.");
                }
                else
                {
                    await Clients.Group($"room_{roomId}").SendAsync("OnVotesRevealed", dto);
                }
            },
            error => { throw new HubException(error.Message); }
        );
    }

    public async Task RestartEstimation(Guid roomId)
    {
        var result = await resetVotesUseCase.Execute(new ResetVotesCommand(roomId));
        await result.Match(
            async success => { await Clients.Group($"room_{roomId}").SendAsync("OnVotesRestarted"); },
            error => { throw new HubException(error.Message); }
        );
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (ModeratorConnections.TryRemove(Context.ConnectionId, out var roomId))
        {
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation(
                    "SignalR Hub: Moderator connection {ConnectionId} disconnected from room {RoomId}. Waiting to see if they reconnect...",
                    Context.ConnectionId, roomId);

            var key = $"moderator_{roomId}";
            var cts = new CancellationTokenSource();
            DisconnectionDelays[key] = cts;

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(3000, cts.Token); // 3 seconds delay for moderator refresh

                    DisconnectionDelays.TryRemove(key, out _);

                    if (logger.IsEnabled(LogLevel.Information))
                        logger.LogInformation(
                            "SignalR Hub: Moderator disconnection delay elapsed. Closing room {RoomId}.", roomId);

                    _ = await disconnectModeratorUseCase.Execute(new DisconnectModeratorCommand(roomId));
                    await hubContext.Clients.Group($"room_{roomId}").SendAsync("OnRoomClosed");
                }
                catch (TaskCanceledException)
                {
                    if (logger.IsEnabled(LogLevel.Information))
                        logger.LogInformation("SignalR Hub: Moderator reconnected to room {RoomId}.", roomId);
                }
                finally
                {
                    cts.Dispose();
                }
            });
        }

        if (ParticipantConnections.TryRemove(Context.ConnectionId, out var connectionInfo))
        {
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation(
                    "SignalR Hub: Participant {Name} connection {ConnectionId} disconnected from room {RoomId}. Waiting to see if they reconnect...",
                    connectionInfo.Name, Context.ConnectionId, connectionInfo.RoomId);

            // Notify immediately that the participant is offline
            await Clients.Group($"room_{connectionInfo.RoomId}")
                .SendAsync("OnParticipantConnectionStatusChanged", connectionInfo.Name, false);

            var key = $"{connectionInfo.RoomId}_{connectionInfo.Name}";
            var cts = new CancellationTokenSource();
            DisconnectionDelays[key] = cts;

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(3000, cts.Token); // Reduced to 3 seconds for faster feedback

                    DisconnectionDelays.TryRemove(key, out _);

                    if (logger.IsEnabled(LogLevel.Information))
                        logger.LogInformation(
                            "SignalR Hub: Disconnection delay elapsed. Removing participant {Name} from room {RoomId}.",
                            connectionInfo.Name, connectionInfo.RoomId);

                    var result =
                        await disconnectParticipantUseCase.Execute(
                            new DisconnectParticipantCommand(connectionInfo.RoomId, connectionInfo.Name));
                    await result.Match(
                        async success =>
                        {
                            await hubContext.Clients.Group($"room_{connectionInfo.RoomId}")
                                .SendAsync("OnSessionUpdated");
                        },
                        error =>
                        {
                            logger.LogError(
                                "SignalR Hub: Failed to disconnect participant {Name} from room {RoomId}: {Message}",
                                connectionInfo.Name, connectionInfo.RoomId, error.Message);
                            return Task.CompletedTask;
                        }
                    );
                }
                catch (TaskCanceledException)
                {
                    // Reconnected successfully, notify they are back online
                    await hubContext.Clients.Group($"room_{connectionInfo.RoomId}")
                        .SendAsync("OnParticipantConnectionStatusChanged", connectionInfo.Name, true);
                }
                finally
                {
                    cts.Dispose();
                }
            });
        }

        await base.OnDisconnectedAsync(exception);
    }
}