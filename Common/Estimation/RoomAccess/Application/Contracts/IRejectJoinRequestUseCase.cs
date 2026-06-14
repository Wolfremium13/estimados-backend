using LanguageExt;
using LanguageExt.Common;

namespace Common.Estimation.RoomAccess.Application.Contracts;

public interface IRejectJoinRequestUseCase
{
    Task<Either<Error, Unit>> Execute(RejectJoinRequestCommand command);
}

public record RejectJoinRequestCommand(Guid RoomId, Guid RequestId);