using LanguageExt;
using LanguageExt.Common;

namespace Common.Estimation.EstimationSession.Application.Contracts;

public interface ICastVoteUseCase
{
    Task<Either<Error, Unit>> Execute(CastVoteCommand command);
}

public record CastVoteCommand(Guid RoomId, string ParticipantName, string CardValue);