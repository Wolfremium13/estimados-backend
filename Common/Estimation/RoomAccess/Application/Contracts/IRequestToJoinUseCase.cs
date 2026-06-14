using LanguageExt;
using LanguageExt.Common;

namespace Common.Estimation.RoomAccess.Application.Contracts;

public interface IRequestToJoinUseCase
{
    Task<Either<Error, JoinRequestInfo>> Execute(RequestToJoinCommand command);
}

public record RequestToJoinCommand(Guid RoomId, string ParticipantName, string ParticipantRole);

public record JoinRequestInfo(Guid RequestId, Guid RoomId, string ParticipantName, string ParticipantRole);