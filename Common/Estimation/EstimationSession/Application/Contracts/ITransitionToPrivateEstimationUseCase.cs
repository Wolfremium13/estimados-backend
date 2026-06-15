using LanguageExt;
using LanguageExt.Common;

namespace Common.Estimation.EstimationSession.Application.Contracts;

public interface ITransitionToPrivateEstimationUseCase
{
    Task<Either<Error, Unit>> Execute(TransitionToPrivateEstimationCommand command);
}

public record TransitionToPrivateEstimationCommand(Guid RoomId);