using Common.Estimation.EstimationSession.Application.Contracts;
using Common.Estimation.EstimationSession.Domain.Models;
using Common.Estimation.EstimationSession.Domain.Ports;
using LanguageExt;
using LanguageExt.Common;

namespace Common.Estimation.EstimationSession.Application.UseCases;

public class CloseEstimationSessionUseCase(IEstimationSessionRepository sessionRepository) : ICloseEstimationSessionUseCase
{
    public async Task<Either<Error, Unit>> Execute(CloseEstimationSessionCommand command)
    {
        return await (
            from sId in SessionId.Create(command.RoomId).ToAsync()
            from _ in sessionRepository.Delete(sId).ToAsync()
            select Unit.Default
        );
    }
}
