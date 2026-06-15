using Common.Estimation.EstimationSession.Application.Contracts;
using Common.Estimation.EstimationSession.Domain.Models;
using Common.Estimation.EstimationSession.Domain.Ports;
using LanguageExt;
using LanguageExt.Common;

namespace Common.Estimation.EstimationSession.Application.UseCases;

public class TransitionToConsensusManagementUseCase(IEstimationSessionRepository sessionRepository)
    : ITransitionToConsensusManagementUseCase
{
    public async Task<Either<Error, Unit>> Execute(TransitionToConsensusManagementCommand command)
    {
        return await (
            from sId in SessionId.Create(command.RoomId).ToAsync()
            from session in sessionRepository.FindById(sId).ToAsync()
            from _ in session.TransitionToConsensusManagement().ToAsync()
            from saveResult in sessionRepository.Save(session).ToAsync()
            select saveResult
        );
    }
}