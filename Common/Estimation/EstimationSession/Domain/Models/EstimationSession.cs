using Common.Estimation.RoomAccess.Domain.Models;
using LanguageExt;
using LanguageExt.Common;
using static Common.Estimation.EstimationSession.Domain.Errors.EstimationSessionErrors;

namespace Common.Estimation.EstimationSession.Domain.Models;

public class EstimationSession
{
    private readonly List<Card> _flaggedSpecialCards = new();
    private readonly Dictionary<ParticipantName, Card> _votes = new();

    private EstimationSession(SessionId id, RoomId roomId, string storyDescription)
    {
        Id = id;
        RoomId = roomId;
        StoryDescription = storyDescription;
        CurrentState = SessionState.StoryPresentation;
        ConsensusValue = Option<Card>.None;
        HasDiscrepancy = false;
    }

    public SessionId Id { get; }
    public RoomId RoomId { get; }
    public string StoryDescription { get; private set; }
    public SessionState CurrentState { get; private set; }
    public Option<Card> ConsensusValue { get; private set; }
    public bool HasDiscrepancy { get; private set; }
    public IReadOnlyCollection<Card> FlaggedSpecialCards => _flaggedSpecialCards.AsReadOnly();

    public static Either<Error, EstimationSession> Create(SessionId id, RoomId roomId, string storyDescription)
    {
        if (string.IsNullOrWhiteSpace(storyDescription))
            return Either<Error, EstimationSession>.Left(
                Error.New(new ClientValidationException("Story description cannot be empty.")));

        return new EstimationSession(id, roomId, storyDescription.Trim());
    }

    public Either<Error, Unit> TransitionToClarification()
    {
        if (CurrentState != SessionState.StoryPresentation)
            return Either<Error, Unit>.Left(Error.New(new InvalidStateTransitionException(
                $"Cannot transition to ClarificationDiscussion from {CurrentState}.")));

        CurrentState = SessionState.ClarificationDiscussion;
        return Either<Error, Unit>.Right(Unit.Default);
    }

    public Either<Error, Unit> TransitionToPrivateEstimation()
    {
        if (CurrentState is not (SessionState.ClarificationDiscussion or
            SessionState.SimultaneousReveal or
            SessionState.ConsensusManagement or
            SessionState.Halted))
        {
            return Either<Error, Unit>.Left(Error.New(new InvalidStateTransitionException(
                $"Cannot transition to PrivateEstimation from {CurrentState}.")));
        }

        _votes.Clear();
        _flaggedSpecialCards.Clear();
        ConsensusValue = Option<Card>.None;
        HasDiscrepancy = false;

        CurrentState = SessionState.PrivateEstimation;
        return Either<Error, Unit>.Right(Unit.Default);
    }

    public Either<Error, Unit> TransitionToConsensusManagement()
    {
        if (CurrentState != SessionState.SimultaneousReveal)
            return Either<Error, Unit>.Left(Error.New(new InvalidStateTransitionException(
                $"Cannot transition to ConsensusManagement from {CurrentState}.")));

        CurrentState = SessionState.ConsensusManagement;
        return Either<Error, Unit>.Right(Unit.Default);
    }

    public Either<Error, Unit> CastVote(ParticipantName voterName, ParticipantRole voterRole, Card card)
    {
        if (CurrentState != SessionState.PrivateEstimation)
            return Either<Error, Unit>.Left(Error.New(new InvalidStateTransitionException(
                "Cannot cast a vote when session is not in PrivateEstimation state.")));

        if (voterRole.Value != ParticipantRole.Developer)
            return Either<Error, Unit>.Left(Error.New(new OnlyDevelopersCanVoteException(
                $"Role '{voterRole.Value}' is not permitted to vote. Only developers can vote.")));

        _votes[voterName] = card;
        return Either<Error, Unit>.Right(Unit.Default);
    }

    public Either<Error, Unit> RevealVotes()
    {
        if (CurrentState != SessionState.PrivateEstimation)
            return Either<Error, Unit>.Left(Error.New(new InvalidStateTransitionException(
                $"Cannot reveal votes when session is in {CurrentState} state.")));

        if (_votes.Count == 0)
            return Either<Error, Unit>.Left(Error.New(new NoVotesCastException(
                "Cannot reveal votes because no developer has voted yet.")));

        if (_votes.Values.Any(c => c.IsHacha))
        {
            CurrentState = SessionState.Halted;
            _flaggedSpecialCards.Add(Card.Create(Card.Hacha).Match(c => c, _ => throw new InvalidOperationException()));
            ConsensusValue = Option<Card>.None;
            HasDiscrepancy = false;
            return Either<Error, Unit>.Right(Unit.Default);
        }

        var specialCards = _votes.Values.Where(c => c.IsSpecial).Distinct().ToList();
        foreach (var sc in specialCards)
        {
            _flaggedSpecialCards.Add(sc);
        }

        var uniqueVotes = _votes.Values.Distinct().ToList();
        if (uniqueVotes.Count == 1)
        {
            ConsensusValue = Option<Card>.Some(uniqueVotes[0]);
            HasDiscrepancy = false;
        }
        else
        {
            ConsensusValue = Option<Card>.None;
            HasDiscrepancy = true;
        }

        CurrentState = SessionState.SimultaneousReveal;
        return Either<Error, Unit>.Right(Unit.Default);
    }

    public IReadOnlyCollection<ParticipantVote> GetVotes()
    {
        return _votes.Select(kv => new ParticipantVote(
            kv.Key,
            CurrentState is SessionState.SimultaneousReveal or SessionState.ConsensusManagement or SessionState.Halted
                ? Option<Card>.Some(kv.Value)
                : Option<Card>.None
        )).ToList().AsReadOnly();
    }
}