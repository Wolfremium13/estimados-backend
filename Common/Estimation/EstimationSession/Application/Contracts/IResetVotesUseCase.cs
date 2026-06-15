using LanguageExt;
using LanguageExt.Common;

namespace Common.Estimation.EstimationSession.Application.Contracts;

public interface IResetVotesUseCase
{
    Task<Either<Error, Unit>> Execute(ResetVotesCommand command);
}

public record ResetVotesCommand(Guid RoomId);