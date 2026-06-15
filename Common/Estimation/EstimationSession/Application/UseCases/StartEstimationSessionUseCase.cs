using Common.Estimation.EstimationSession.Application.Contracts;
using Common.Estimation.EstimationSession.Domain.Models;
using Common.Estimation.EstimationSession.Domain.Ports;
using Common.Estimation.RoomAccess.Domain.Models;
using Common.Estimation.RoomAccess.Domain.Ports;
using LanguageExt;
using LanguageExt.Common;

namespace Common.Estimation.EstimationSession.Application.UseCases;

public class StartEstimationSessionUseCase(
    IEstimationRoomRepository roomRepository,
    IEstimationSessionRepository sessionRepository
) : IStartEstimationSessionUseCase
{
    public async Task<Either<Error, EstimationSessionDto>> Execute(StartEstimationSessionCommand command)
    {
        return await (
            from rId in RoomId.Create(command.RoomId).ToAsync()
            from room in roomRepository.FindById(rId).ToAsync()
            from sessionId in SessionId.Create(command.RoomId).ToAsync()
            from session in CreateSession(sessionId, room.Id, command.StoryDescription).ToAsync()
            from _ in sessionRepository.Save(session).ToAsync()
            select EstimationSessionMapper.ToDto(session)
        );
    }

    private static Either<Error, Domain.Models.EstimationSession> CreateSession(SessionId id, RoomId roomId,
        string description)
    {
        return Domain.Models.EstimationSession.Create(id, roomId, description);
    }
}