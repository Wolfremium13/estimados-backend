using LanguageExt;
using LanguageExt.Common;

namespace Common.Estimation.EstimationSession.Application.Contracts;

public interface IGetEstimationSessionUseCase
{
    Task<Either<Error, EstimationSessionDto>> Execute(GetEstimationSessionQuery query);
}

public record GetEstimationSessionQuery(Guid RoomId);