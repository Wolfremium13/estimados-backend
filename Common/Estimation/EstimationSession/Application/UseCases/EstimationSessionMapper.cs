using Common.Estimation.EstimationSession.Application.Contracts;

namespace Common.Estimation.EstimationSession.Application.UseCases;

public static class EstimationSessionMapper
{
    public static EstimationSessionDto ToDto(Domain.Models.EstimationSession session)
    {
        string? consensusValue = null;
        session.ConsensusValue.IfSome(c => consensusValue = c.Value);

        var votesList = new List<ParticipantVoteDto>();
        foreach (var v in session.GetVotes())
        {
            string? cardValue = null;
            v.Card.IfSome(c => cardValue = c.Value);
            votesList.Add(new ParticipantVoteDto(v.Name.Value, cardValue));
        }

        return new EstimationSessionDto(
            session.Id.Value,
            session.RoomId.Value,
            session.StoryDescription,
            session.CurrentState.ToString(),
            consensusValue,
            session.HasDiscrepancy,
            session.FlaggedSpecialCards.Select(c => c.Value).ToList().AsReadOnly(),
            votesList.AsReadOnly()
        );
    }
}