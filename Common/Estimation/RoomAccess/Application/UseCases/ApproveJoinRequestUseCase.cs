using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using Common.Estimation.RoomAccess.Domain.Models;
using Common.Estimation.RoomAccess.Domain.Ports;
using Common.Estimation.RoomAccess.Application.Contracts;

namespace Common.Estimation.RoomAccess.Application.UseCases;

public class ApproveJoinRequestUseCase(IEstimationRoomRepository roomRepository) : IApproveJoinRequestUseCase
{
    public async Task<Either<Error, Unit>> Execute(ApproveJoinRequestCommand command)
    {
        return await (
            from roomId in RoomId.Create(command.RoomId).ToAsync()
            from requestId in RequestId.Create(command.RequestId).ToAsync()
            from room in roomRepository.FindById(roomId).ToAsync()
            from _ in room.ApproveJoinRequest(requestId).ToAsync()
            from __ in roomRepository.Save(room).ToAsync()
            select Unit.Default
        );
    }
}
