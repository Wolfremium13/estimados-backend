using Common.Estimation.RoomAccess.Application.Contracts;
using Common.Estimation.RoomAccess.Domain.Models;
using Common.Estimation.RoomAccess.Domain.Ports;
using LanguageExt;
using LanguageExt.Common;

namespace Common.Estimation.RoomAccess.Application.UseCases;

public class RequestToJoinUseCase(
    IEstimationRoomRepository roomRepository,
    IGuidGenerator guidGenerator
) : IRequestToJoinUseCase
{
    public async Task<Either<Error, JoinRequestInfo>> Execute(RequestToJoinCommand command)
    {
        return await (
            from roomId in RoomId.Create(command.RoomId).ToAsync()
            from name in ParticipantName.Create(command.ParticipantName).ToAsync()
            from role in ParticipantRole.Create(command.ParticipantRole).ToAsync()
            from requestId in RequestId.Create(guidGenerator.NewGuid()).ToAsync()
            from request in CreateJoinRequest(requestId, name, role).ToAsync()
            from room in roomRepository.FindById(roomId).ToAsync()
            from _ in room.AddJoinRequest(request).ToAsync()
            from __ in roomRepository.Save(room).ToAsync()
            select new JoinRequestInfo(request.Id.Value, room.Id.Value, request.Name.Value, request.Role.Value)
        );
    }

    private Either<Error, JoinRequest> CreateJoinRequest(RequestId id, ParticipantName name, ParticipantRole role)
    {
        return JoinRequest.Create(id, name, role);
    }
}