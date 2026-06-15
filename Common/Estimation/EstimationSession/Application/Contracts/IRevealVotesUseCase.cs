using LanguageExt;
using LanguageExt.Common;

namespace Common.Estimation.EstimationSession.Application.Contracts;

public interface IRevealVotesUseCase
{
    Task<Either<Error, EstimationSessionDto>> Execute(RevealVotesCommand command);
}

public record RevealVotesCommand(Guid RoomId);