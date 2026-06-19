using Common.Estimation.RoomAccess.Application.Contracts;
using Common.Estimation.RoomAccess.Domain.Models;
using Common.Estimation.RoomAccess.Domain.Ports;
using LanguageExt;
using LanguageExt.Common;
using static Common.Estimation.RoomAccess.Domain.Errors.RoomAccessErrors;

namespace Common.Estimation.RoomAccess.Application.UseCases;

public class GetPendingJoinRequestsUseCase(IEstimationRoomRepository roomRepository) : IGetPendingJoinRequestsUseCase
{
    public async Task<Either<Error, IReadOnlyCollection<PendingJoinRequestDto>>> Execute(
        GetPendingJoinRequestsQuery query)
    {
        return await (
            from rId in RoomId.Create(query.RoomId).ToAsync()
            from room in roomRepository.FindById(rId).ToAsync()
            from _ in room.IsActive
                ? Either<Error, Unit>.Right(Unit.Default).ToAsync()
                : Either<Error, Unit>
                    .Left(Error.New(new RoomNotFoundException($"Room with ID {query.RoomId} is not active."))).ToAsync()
            select (IReadOnlyCollection<PendingJoinRequestDto>)room.JoinRequests
                .Where(r => r.Status == JoinRequestStatus.Pending)
                .Select(r => new PendingJoinRequestDto(r.Id.Value.ToString(), r.Name.Value, r.Role.Value))
                .ToList()
                .AsReadOnly()
        );
    }
}