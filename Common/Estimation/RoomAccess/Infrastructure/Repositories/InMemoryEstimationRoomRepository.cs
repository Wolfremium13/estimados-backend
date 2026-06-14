using System.Collections.Concurrent;
using Common.Estimation.RoomAccess.Domain.Models;
using Common.Estimation.RoomAccess.Domain.Ports;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static Common.Estimation.RoomAccess.Domain.Errors.RoomAccessErrors;

namespace Common.Estimation.RoomAccess.Infrastructure.Repositories;

public class InMemoryEstimationRoomRepository(
    ILogger<InMemoryEstimationRoomRepository>? logger = null,
    IHostEnvironment? env = null
) : IEstimationRoomRepository
{
    private static readonly ConcurrentDictionary<Guid, EstimationRoom> Rooms = new();

    public Task<Either<Error, EstimationRoom>> FindById(RoomId roomId)
    {
        if (Rooms.TryGetValue(roomId.Value, out var room))
        {
            return Task.FromResult<Either<Error, EstimationRoom>>(room);
        }

        return Task.FromResult<Either<Error, EstimationRoom>>(
            Error.New(new RoomNotFoundException($"Room with ID {roomId.Value} was not found."))
        );
    }

    public Task<Either<Error, Unit>> Save(EstimationRoom room)
    {
        Rooms[room.Id.Value] = room;

        var isDev = env?.IsDevelopment() ??
                    (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development");
        if (isDev)
        {
            logger?.LogInformation("In-memory repository: Saved room {RoomId} with moderator {ModeratorName}.",
                room.Id.Value, room.ModeratorName.Value);
        }

        return Task.FromResult<Either<Error, Unit>>(Unit.Default);
    }

    public static void Clear()
    {
        Rooms.Clear();
    }
}