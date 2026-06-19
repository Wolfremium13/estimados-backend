using LanguageExt;
using LanguageExt.Common;

namespace Common.Estimation.RoomAccess.Application.Contracts;

public record GetPendingJoinRequestsQuery(Guid RoomId);

public record PendingJoinRequestDto(string RequestId, string Name, string Role);

public interface IGetPendingJoinRequestsUseCase
{
    Task<Either<Error, IReadOnlyCollection<PendingJoinRequestDto>>> Execute(GetPendingJoinRequestsQuery query);
}