using System.Collections.Concurrent;
using Common.Estimation.EstimationSession.Application.Contracts;
using Common.Estimation.RoomAccess.Application.Contracts;
using Microsoft.AspNetCore.SignalR;

namespace Wolfremium.Estimados.Hubs;

public class RoomHub(
    IDisconnectModeratorUseCase disconnectModeratorUseCase,
    ICastVoteUseCase castVoteUseCase,
    IRevealVotesUseCase revealVotesUseCase,
    IResetVotesUseCase resetVotesUseCase,
    ILogger<RoomHub> logger
) : Hub
{
    private static readonly ConcurrentDictionary<string, Guid> ModeratorConnections = new();
    private static readonly ConcurrentDictionary<string, Guid> ParticipantConnections = new();

    public async Task JoinRoomAsModerator(Guid roomId)
    {
        var roomIdStr = $"room_{roomId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, roomIdStr);
        ModeratorConnections[Context.ConnectionId] = roomId;

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("SignalR Hub: Connection {ConnectionId} joined room {RoomId} as Moderator.",
                Context.ConnectionId, roomId);
    }

    public async Task JoinRoomAsParticipant(Guid roomId)
    {
        var roomIdStr = $"room_{roomId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, roomIdStr);
        ParticipantConnections[Context.ConnectionId] = roomId;

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("SignalR Hub: Connection {ConnectionId} joined room {RoomId} as Participant.",
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
                    "SignalR Hub: Moderator connection {ConnectionId} disconnected from room {RoomId}. Closing room...",
                    Context.ConnectionId, roomId);

            _ = await disconnectModeratorUseCase.Execute(new DisconnectModeratorCommand(roomId));
            await Clients.Group($"room_{roomId}").SendAsync("OnRoomClosed");
        }

        if (ParticipantConnections.TryRemove(Context.ConnectionId, out var pRoomId))
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation(
                    "SignalR Hub: Participant connection {ConnectionId} disconnected from room {RoomId}.",
                    Context.ConnectionId, pRoomId);

        await base.OnDisconnectedAsync(exception);
    }
}