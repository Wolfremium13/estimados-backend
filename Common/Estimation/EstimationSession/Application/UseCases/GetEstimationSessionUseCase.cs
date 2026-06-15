using Common.Estimation.EstimationSession.Application.Contracts;
using Common.Estimation.EstimationSession.Domain.Models;
using Common.Estimation.EstimationSession.Domain.Ports;
using LanguageExt;
using LanguageExt.Common;

namespace Common.Estimation.EstimationSession.Application.UseCases;

public class GetEstimationSessionUseCase(IEstimationSessionRepository sessionRepository) : IGetEstimationSessionUseCase
{
    public async Task<Either<Error, EstimationSessionDto>> Execute(GetEstimationSessionQuery query)
    {
        return await (
            from sId in SessionId.Create(query.RoomId).ToAsync()
            from session in sessionRepository.FindById(sId).ToAsync()
            select EstimationSessionMapper.ToDto(session)
        );
    }
}