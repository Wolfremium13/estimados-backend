using Common.Estimation.EstimationSession.Application.Contracts;
using Common.Estimation.EstimationSession.Domain.Models;
using Common.Estimation.EstimationSession.Domain.Ports;
using Common.Estimation.RoomAccess.Domain.Models;
using Common.Estimation.RoomAccess.Domain.Ports;
using LanguageExt;
using LanguageExt.Common;
using static Common.Estimation.EstimationSession.Domain.Errors.EstimationSessionErrors;

namespace Common.Estimation.EstimationSession.Application.UseCases;

public class CastVoteUseCase(
    IEstimationRoomRepository roomRepository,
    IEstimationSessionRepository sessionRepository
) : ICastVoteUseCase
{
    public async Task<Either<Error, Unit>> Execute(CastVoteCommand command)
    {
        return await (
            from rId in RoomId.Create(command.RoomId).ToAsync()
            from room in roomRepository.FindById(rId).ToAsync()
            from participant in FindParticipant(room, command.ParticipantName).ToAsync()
            from sId in SessionId.Create(command.RoomId).ToAsync()
            from session in sessionRepository.FindById(sId).ToAsync()
            from card in Card.Create(command.CardValue).ToAsync()
            from _ in session.CastVote(participant.Name, participant.Role, card).ToAsync()
            from saveResult in sessionRepository.Save(session).ToAsync()
            select saveResult
        );
    }

    private static Either<Error, Participant> FindParticipant(EstimationRoom room, string name)
    {
        var participant = room.ActiveParticipants.FirstOrDefault(p =>
            string.Equals(p.Name.Value, name, StringComparison.OrdinalIgnoreCase));

        return participant != null
            ? Either<Error, Participant>.Right(participant)
            : Either<Error, Participant>.Left(Error.New(
                new ClientValidationException("Participant not found in room.")));
    }
}