using LanguageExt;
using LanguageExt.Common;

namespace Common.Estimation.EstimationSession.Application.Contracts;

public interface ICloseEstimationSessionUseCase
{
    Task<Either<Error, Unit>> Execute(CloseEstimationSessionCommand command);
}

public record CloseEstimationSessionCommand(Guid RoomId);
