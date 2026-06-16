using Common.Estimation.RoomAccess.Application.Contracts;
using Common.Estimation.RoomAccess.Domain.Models;
using Common.Estimation.RoomAccess.Domain.Ports;
using Common.Estimation.EstimationSession.Domain.Models;
using Common.Estimation.EstimationSession.Domain.Ports;
using LanguageExt;
using LanguageExt.Common;

namespace Common.Estimation.RoomAccess.Application.UseCases;

public class DisconnectParticipantUseCase(
    IEstimationRoomRepository roomRepository,
    IEstimationSessionRepository sessionRepository
) : IDisconnectParticipantUseCase
{
    public async Task<Either<Error, Unit>> Execute(DisconnectParticipantCommand command)
    {
        return await (
            from roomId in RoomId.Create(command.RoomId).ToAsync()
            from participantName in ParticipantName.Create(command.ParticipantName).ToAsync()
            from room in roomRepository.FindById(roomId).ToAsync()
            from _ in room.RemoveParticipant(participantName).ToAsync()
            from __ in roomRepository.Save(room).ToAsync()
            from ___ in RemoveVoteIfExists(roomId, participantName).ToAsync()
            select Unit.Default
        );
    }

    private async Task<Either<Error, Unit>> RemoveVoteIfExists(RoomId roomId, ParticipantName participantName)
    {
        var sessionIdEither = SessionId.Create(roomId.Value);
        if (sessionIdEither.IsLeft)
            return Unit.Default;

        var sessionId = sessionIdEither.Match(id => id, _ => null!);
        var sessionResult = await sessionRepository.FindById(sessionId);

        return await sessionResult.MatchAsync(
            async session =>
            {
                session.RemoveVote(participantName);
                var saveResult = await sessionRepository.Save(session);
                return saveResult;
            },
            error => Task.FromResult<Either<Error, Unit>>(Unit.Default)
        );
    }
}
