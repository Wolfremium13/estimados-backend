using Common.Estimation.EstimationSession.Domain.Models;
using Common.Estimation.RoomAccess.Domain.Models;

namespace Common.Test.Estimation.EstimationSession.Domain.Builders;

public class EstimationSessionBuilder
{
    private readonly List<(string Name, string Role, string CardValue)> _votes = new();
    private Guid _id = Guid.NewGuid();
    private Guid _roomId = Guid.NewGuid();
    private SessionState _state = SessionState.StoryPresentation;
    private string _storyDescription = "Implement the domain layer";

    public Common.Estimation.EstimationSession.Domain.Models.EstimationSession Build()
    {
        var sessionId = SessionId.Create(_id)
            .Match(s => s, error => throw new InvalidOperationException(error.Message));
        var roomId = RoomId.Create(_roomId).Match(r => r, error => throw new InvalidOperationException(error.Message));

        var session = Common.Estimation.EstimationSession.Domain.Models.EstimationSession
            .Create(sessionId, roomId, _storyDescription).Match(
                s => s,
                error => throw new InvalidOperationException($"Failed to build EstimationSession: {error.Message}")
            );

        if (_state != SessionState.StoryPresentation)
        {
            session.TransitionToClarification().Match(
                u => u,
                error => throw new InvalidOperationException($"Failed to transition to Clarification: {error.Message}")
            );

            if (_state != SessionState.ClarificationDiscussion)
            {
                session.TransitionToPrivateEstimation().Match(
                    u => u,
                    error => throw new InvalidOperationException(
                        $"Failed to transition to PrivateEstimation: {error.Message}")
                );

                foreach (var vote in _votes)
                {
                    var pName = ParticipantName.Create(vote.Name)
                        .Match(n => n, e => throw new InvalidOperationException(e.Message));
                    var pRole = ParticipantRole.Create(vote.Role)
                        .Match(r => r, e => throw new InvalidOperationException(e.Message));
                    var card = Card.Create(vote.CardValue)
                        .Match(c => c, e => throw new InvalidOperationException(e.Message));

                    session.CastVote(pName, pRole, card).Match(
                        u => u,
                        error => throw new InvalidOperationException($"Failed to cast vote: {error.Message}")
                    );
                }

                if (_state != SessionState.PrivateEstimation)
                {
                    session.RevealVotes().Match(
                        u => u,
                        error => throw new InvalidOperationException($"Failed to reveal votes: {error.Message}")
                    );

                    if (_state == SessionState.ConsensusManagement)
                    {
                        session.TransitionToConsensusManagement().Match(
                            u => u,
                            error => throw new InvalidOperationException(
                                $"Failed to transition to ConsensusManagement: {error.Message}")
                        );
                    }
                }
            }
        }

        return session;
    }

    public EstimationSessionBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public EstimationSessionBuilder WithRoomId(Guid roomId)
    {
        _roomId = roomId;
        return this;
    }

    public EstimationSessionBuilder WithStoryDescription(string storyDescription)
    {
        _storyDescription = storyDescription;
        return this;
    }

    public EstimationSessionBuilder WithState(SessionState state)
    {
        _state = state;
        return this;
    }

    public EstimationSessionBuilder WithVote(string name, string role, string cardValue)
    {
        _votes.Add((name, role, cardValue));
        return this;
    }
}