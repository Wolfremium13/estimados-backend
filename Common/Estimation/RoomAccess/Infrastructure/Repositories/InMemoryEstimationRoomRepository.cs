using System.Collections.Concurrent;
using Common.Estimation.RoomAccess.Domain.Models;
using Common.Estimation.RoomAccess.Domain.Ports;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using static Common.Estimation.RoomAccess.Domain.Errors.RoomAccessErrors;

namespace Common.Estimation.RoomAccess.Infrastructure.Repositories;

public class InMemoryEstimationRoomRepository(
    ILogger<InMemoryEstimationRoomRepository> logger
) : IEstimationRoomRepository
{
    private static readonly ConcurrentDictionary<Guid, EstimationRoom> Rooms = new();

    public Task<Either<Error, EstimationRoom>> FindById(RoomId roomId)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("In-memory repository: Finding room {RoomId}...", roomId.Value);

        if (Rooms.TryGetValue(roomId.Value, out var room))
        {
            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation(
                    "In-memory repository: Found room {RoomId}. Active: {IsActive}, Participants: {ParticipantCount}.",
                    room.Id.Value, room.IsActive, room.ActiveParticipants.Count);
            return Task.FromResult<Either<Error, EstimationRoom>>(room);
        }

        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("In-memory repository: Room {RoomId} was not found.", roomId.Value);
        return Task.FromResult<Either<Error, EstimationRoom>>(
            Error.New(new RoomNotFoundException($"Room with ID {roomId.Value} was not found."))
        );
    }

    public Task<Either<Error, Unit>> Save(EstimationRoom room)
    {
        var isNew = !Rooms.ContainsKey(room.Id.Value);
        Rooms[room.Id.Value] = room;

        if (logger.IsEnabled(LogLevel.Information))
        {
            if (isNew)
            {
                logger.LogInformation("In-memory repository: Created room {RoomId} with moderator {ModeratorName}.",
                    room.Id.Value, room.ModeratorName.Value);
            }
            else
            {
                var participantNames = string.Join(", ",
                    room.ActiveParticipants.Select(p => $"{p.Name.Value} ({p.Role.Value})"));
                var pendingRequests = string.Join(", ",
                    room.JoinRequests.Where(r => r.Status == JoinRequestStatus.Pending)
                        .Select(r => r.Id.Value.ToString()));

                logger.LogInformation(
                    "In-memory repository: Updated room {RoomId}. Active: {IsActive}, Active Participants: [{Participants}], Pending Requests: [{PendingRequests}].",
                    room.Id.Value, room.IsActive, participantNames, pendingRequests);
            }
        }

        return Task.FromResult<Either<Error, Unit>>(Unit.Default);
    }

    public static void Clear()
    {
        Rooms.Clear();
    }
}