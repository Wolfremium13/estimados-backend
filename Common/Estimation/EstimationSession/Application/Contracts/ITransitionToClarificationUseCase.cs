using LanguageExt;
using LanguageExt.Common;

namespace Common.Estimation.EstimationSession.Application.Contracts;

public interface ITransitionToClarificationUseCase
{
    Task<Either<Error, Unit>> Execute(TransitionToClarificationCommand command);
}

public record TransitionToClarificationCommand(Guid RoomId);