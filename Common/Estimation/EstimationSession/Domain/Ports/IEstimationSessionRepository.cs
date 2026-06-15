using Common.Estimation.EstimationSession.Domain.Models;
using LanguageExt;
using LanguageExt.Common;

namespace Common.Estimation.EstimationSession.Domain.Ports;

public interface IEstimationSessionRepository
{
    Task<Either<Error, Models.EstimationSession>> FindById(SessionId sessionId);
    Task<Either<Error, Unit>> Save(Models.EstimationSession session);
}