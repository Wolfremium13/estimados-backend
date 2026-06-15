using LanguageExt;
using LanguageExt.Common;

namespace Common.Estimation.EstimationSession.Application.Contracts;

public interface ITransitionToConsensusManagementUseCase
{
    Task<Either<Error, Unit>> Execute(TransitionToConsensusManagementCommand command);
}

public record TransitionToConsensusManagementCommand(Guid RoomId);