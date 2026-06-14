using Common.Estimation.RoomAccess.Application.Contracts;
using Common.Estimation.RoomAccess.Domain.Models;
using Common.Estimation.RoomAccess.Domain.Ports;
using LanguageExt;
using LanguageExt.Common;

namespace Common.Estimation.RoomAccess.Application.UseCases;

public class CreateRoomUseCase(
    IEstimationRoomRepository roomRepository,
    IGuidGenerator guidGenerator
) : ICreateRoomUseCase
{
    public async Task<Either<Error, CreatedRoomInfo>> Execute(CreateRoomCommand command)
    {
        return await (
            from name in ParticipantName.Create(command.ModeratorName).ToAsync()
            from roomId in RoomId.Create(guidGenerator.NewGuid()).ToAsync()
            from room in CreateRoom(roomId, name).ToAsync()
            from _ in roomRepository.Save(room).ToAsync()
            select new CreatedRoomInfo(room.Id.Value, room.ModeratorName.Value)
        );
    }

    private static Either<Error, EstimationRoom> CreateRoom(RoomId id, ParticipantName name)
    {
        return EstimationRoom.Create(id, name);
    }
}