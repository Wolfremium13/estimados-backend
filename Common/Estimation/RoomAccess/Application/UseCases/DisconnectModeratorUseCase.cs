using Common.Estimation.RoomAccess.Application.Contracts;
using Common.Estimation.RoomAccess.Domain.Models;
using Common.Estimation.RoomAccess.Domain.Ports;
using LanguageExt;
using LanguageExt.Common;

namespace Common.Estimation.RoomAccess.Application.UseCases;

public class DisconnectModeratorUseCase(IEstimationRoomRepository roomRepository) : IDisconnectModeratorUseCase
{
    public async Task<Either<Error, Unit>> Execute(DisconnectModeratorCommand command)
    {
        return await (
            from roomId in RoomId.Create(command.RoomId).ToAsync()
            from room in roomRepository.FindById(roomId).ToAsync()
            from _ in room.Close().ToAsync()
            from __ in roomRepository.Save(room).ToAsync()
            select Unit.Default
        );
    }
}