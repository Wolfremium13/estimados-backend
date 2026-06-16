using Common.Estimation.EstimationSession.Domain.Models;
using Common.Estimation.RoomAccess.Domain.Models;
using Common.Test.Estimation.EstimationSession.Domain.Builders;
using Shouldly;
using Xunit;
using static Common.Estimation.EstimationSession.Domain.Errors.EstimationSessionErrors;

namespace Common.Test.Estimation.EstimationSession.Domain.Models;

public class EstimationSessionShould
{
    [Fact]
    public void BeCreatedCorrectlyWithDefaultState()
    {
        var sessionId = new SessionIdBuilder().Build();
        var roomId = RoomId.Create(Guid.NewGuid()).Match(r => r, _ => throw new Exception());
        var storyDesc = "  Implement BDD Scenarios  ";

        var result =
            Common.Estimation.EstimationSession.Domain.Models.EstimationSession.Create(sessionId, roomId, storyDesc);

        result.IsRight.ShouldBeTrue();
        result.IfRight(session =>
        {
            session.Id.ShouldBe(sessionId);
            session.RoomId.ShouldBe(roomId);
            session.StoryDescription.ShouldBe("Implement BDD Scenarios");
            session.CurrentState.ShouldBe(SessionState.StoryPresentation);
            session.ConsensusValue.IsNone.ShouldBeTrue();
            session.HasDiscrepancy.ShouldBeFalse();
            session.FlaggedSpecialCards.ShouldBeEmpty();
        });
    }

    [Fact]
    public void FailCreationWhenStoryDescriptionIsEmpty()
    {
        var sessionId = new SessionIdBuilder().Build();
        var roomId = RoomId.Create(Guid.NewGuid()).Match(r => r, _ => throw new Exception());

        var result = Common.Estimation.EstimationSession.Domain.Models.EstimationSession.Create(sessionId, roomId, "");

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error => error.ToException().ShouldBeOfType<ClientValidationException>());
    }

    [Fact]
    public void TransitionToClarificationSuccessfully()
    {
        var session = new EstimationSessionBuilder().Build();

        var result = session.TransitionToClarification();

        result.IsRight.ShouldBeTrue();
        session.CurrentState.ShouldBe(SessionState.ClarificationDiscussion);
    }

    [Fact]
    public void FailTransitionToClarificationWhenNotInStoryPresentation()
    {
        var session = new EstimationSessionBuilder()
            .WithState(SessionState.ClarificationDiscussion)
            .Build();

        var result = session.TransitionToClarification();

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error => error.ToException().ShouldBeOfType<InvalidStateTransitionException>());
    }

    [Fact]
    public void TransitionToPrivateEstimationSuccessfully()
    {
        var session = new EstimationSessionBuilder()
            .WithState(SessionState.ClarificationDiscussion)
            .Build();

        var result = session.TransitionToPrivateEstimation();

        result.IsRight.ShouldBeTrue();
        session.CurrentState.ShouldBe(SessionState.PrivateEstimation);
    }

    [Fact]
    public void FailTransitionToPrivateEstimationFromInvalidState()
    {
        var session = new EstimationSessionBuilder().Build();

        var result = session.TransitionToPrivateEstimation();

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error => error.ToException().ShouldBeOfType<InvalidStateTransitionException>());
    }

    [Fact]
    public void CastVoteSuccessfullyDuringPrivateEstimation()
    {
        var session = new EstimationSessionBuilder()
            .WithState(SessionState.PrivateEstimation)
            .Build();
        var voterName = ParticipantName.Create("Carlos").Match(n => n, _ => throw new Exception());
        var voterRole = ParticipantRole.Create(ParticipantRole.Developer).Match(r => r, _ => throw new Exception());
        var card = new CardBuilder().WithValue("5").Build();

        var result = session.CastVote(voterName, voterRole, card);

        result.IsRight.ShouldBeTrue();

        var votes = session.GetVotes();
        votes.Count.ShouldBe(1);
        votes.ShouldContain(v => v.Name == voterName && v.Card.IsNone);
    }

    [Fact]
    public void FailCastingVoteWhenNotInPrivateEstimation()
    {
        var session = new EstimationSessionBuilder()
            .WithState(SessionState.ClarificationDiscussion)
            .Build();
        var voterName = ParticipantName.Create("Carlos").Match(n => n, _ => throw new Exception());
        var voterRole = ParticipantRole.Create(ParticipantRole.Developer).Match(r => r, _ => throw new Exception());
        var card = new CardBuilder().WithValue("5").Build();

        var result = session.CastVote(voterName, voterRole, card);

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error => error.ToException().ShouldBeOfType<InvalidStateTransitionException>());
    }

    [Fact]
    public void FailCastingVoteWhenVoterIsProductOwner()
    {
        var session = new EstimationSessionBuilder()
            .WithState(SessionState.PrivateEstimation)
            .Build();
        var voterName = ParticipantName.Create("Ana").Match(n => n, _ => throw new Exception());
        var voterRole = ParticipantRole.Create(ParticipantRole.ProductOwner).Match(r => r, _ => throw new Exception());
        var card = new CardBuilder().WithValue("5").Build();

        var result = session.CastVote(voterName, voterRole, card);

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error => error.ToException().ShouldBeOfType<OnlyDevelopersCanVoteException>());
    }

    [Fact]
    public void UpdateVoteWhenDeveloperCastsVoteMultipleTimes()
    {
        var session = new EstimationSessionBuilder()
            .WithState(SessionState.PrivateEstimation)
            .Build();
        var voterName = ParticipantName.Create("Carlos").Match(n => n, _ => throw new Exception());
        var voterRole = ParticipantRole.Create(ParticipantRole.Developer).Match(r => r, _ => throw new Exception());
        var card1 = new CardBuilder().WithValue("5").Build();
        var card2 = new CardBuilder().WithValue("3").Build();

        session.CastVote(voterName, voterRole, card1);
        var result = session.CastVote(voterName, voterRole, card2);

        result.IsRight.ShouldBeTrue();

        session.RevealVotes();
        var votes = session.GetVotes();
        votes.ShouldContain(v => v.Name == voterName && v.Card == card2);
    }

    [Fact]
    public void FailRevealVotesWhenNoVotesCast()
    {
        var session = new EstimationSessionBuilder()
            .WithState(SessionState.PrivateEstimation)
            .Build();

        var result = session.RevealVotes();

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error => error.ToException().ShouldBeOfType<NoVotesCastException>());
    }

    [Fact]
    public void RegisterConsensusAndRevealVotesSuccessfully()
    {
        var session = new EstimationSessionBuilder()
            .WithState(SessionState.PrivateEstimation)
            .WithVote("Carlos", ParticipantRole.Developer, "5")
            .WithVote("Ana", ParticipantRole.Developer, "5")
            .Build();

        var result = session.RevealVotes();

        result.IsRight.ShouldBeTrue();
        session.CurrentState.ShouldBe(SessionState.SimultaneousReveal);
        session.ConsensusValue.IfSome(card => card.Value.ShouldBe("5"));
        session.HasDiscrepancy.ShouldBeFalse();

        var expectedCard = Card.Create("5").Match(c => c, _ => throw new Exception());
        var votes = session.GetVotes();
        votes.ShouldContain(v => v.Name.Value == "Carlos" && v.Card == expectedCard);
        votes.ShouldContain(v => v.Name.Value == "Ana" && v.Card == expectedCard);
    }

    [Fact]
    public void DetectDiscrepancyAndRevealVotesSuccessfully()
    {
        var session = new EstimationSessionBuilder()
            .WithState(SessionState.PrivateEstimation)
            .WithVote("Carlos", ParticipantRole.Developer, "3")
            .WithVote("Ana", ParticipantRole.Developer, "5")
            .Build();

        var result = session.RevealVotes();

        result.IsRight.ShouldBeTrue();
        session.CurrentState.ShouldBe(SessionState.SimultaneousReveal);
        session.ConsensusValue.IsNone.ShouldBeTrue();
        session.HasDiscrepancy.ShouldBeTrue();
    }

    [Fact]
    public void HaltSessionWhenDeveloperVotesWithHacha()
    {
        var session = new EstimationSessionBuilder()
            .WithState(SessionState.PrivateEstimation)
            .WithVote("Carlos", ParticipantRole.Developer, "5")
            .WithVote("Ana", ParticipantRole.Developer, "Axe")
            .Build();

        var result = session.RevealVotes();

        result.IsRight.ShouldBeTrue();
        session.CurrentState.ShouldBe(SessionState.Halted);
        session.FlaggedSpecialCards.ShouldContain(c => c.Value == "Axe");
        session.ConsensusValue.IsNone.ShouldBeTrue();
        session.HasDiscrepancy.ShouldBeFalse();
    }

    [Fact]
    public void FlagSpecialCardsOnReveal()
    {
        var session = new EstimationSessionBuilder()
            .WithState(SessionState.PrivateEstimation)
            .WithVote("Carlos", ParticipantRole.Developer, "5")
            .WithVote("Ana", ParticipantRole.Developer, "Diagram")
            .WithVote("Bob", ParticipantRole.Developer, "AI")
            .WithVote("Clara", ParticipantRole.Developer, "Coffee Cup")
            .Build();

        var result = session.RevealVotes();

        result.IsRight.ShouldBeTrue();
        session.CurrentState.ShouldBe(SessionState.SimultaneousReveal);
        session.FlaggedSpecialCards.ShouldContain(c => c.Value == "Diagram");
        session.FlaggedSpecialCards.ShouldContain(c => c.Value == "AI");
        session.FlaggedSpecialCards.ShouldContain(c => c.Value == "Coffee Cup");
    }

    [Fact]
    public void SupportReEstimationByClearingVotesAndResettingState()
    {
        var session = new EstimationSessionBuilder()
            .WithState(SessionState.SimultaneousReveal)
            .WithVote("Carlos", ParticipantRole.Developer, "3")
            .WithVote("Ana", ParticipantRole.Developer, "5")
            .Build();

        var result = session.TransitionToPrivateEstimation();

        result.IsRight.ShouldBeTrue();
        session.CurrentState.ShouldBe(SessionState.PrivateEstimation);
        session.ConsensusValue.IsNone.ShouldBeTrue();
        session.HasDiscrepancy.ShouldBeFalse();
        session.FlaggedSpecialCards.ShouldBeEmpty();
        session.GetVotes().ShouldBeEmpty();
    }

    [Fact]
    public void TransitionToConsensusManagementSuccessfully()
    {
        var session = new EstimationSessionBuilder()
            .WithState(SessionState.SimultaneousReveal)
            .WithVote("Carlos", ParticipantRole.Developer, "5")
            .Build();

        var result = session.TransitionToConsensusManagement();

        result.IsRight.ShouldBeTrue();
        session.CurrentState.ShouldBe(SessionState.ConsensusManagement);
    }

    [Fact]
    public void RecalculateConsensusWhenVoteIsRemovedDuringSimultaneousReveal()
    {
        var session = new EstimationSessionBuilder()
            .WithState(SessionState.PrivateEstimation)
            .WithVote("Carlos", ParticipantRole.Developer, "3")
            .WithVote("Ana", ParticipantRole.Developer, "5")
            .Build();

        session.RevealVotes();
        session.HasDiscrepancy.ShouldBeTrue();
        session.ConsensusValue.IsNone.ShouldBeTrue();

        var voterName = ParticipantName.Create("Ana").Match(n => n, _ => throw new Exception());
        session.RemoveVote(voterName);

        session.HasDiscrepancy.ShouldBeFalse();
        session.ConsensusValue.IfSome(card => card.Value.ShouldBe("3"));
    }

    [Fact]
    public void RestoreFromHaltedStateWhenHachaVoteIsRemoved()
    {
        var session = new EstimationSessionBuilder()
            .WithState(SessionState.PrivateEstimation)
            .WithVote("Carlos", ParticipantRole.Developer, "5")
            .WithVote("Ana", ParticipantRole.Developer, "Axe")
            .Build();

        session.RevealVotes();
        session.CurrentState.ShouldBe(SessionState.Halted);

        var voterName = ParticipantName.Create("Ana").Match(n => n, _ => throw new Exception());
        session.RemoveVote(voterName);

        session.CurrentState.ShouldBe(SessionState.SimultaneousReveal);
        session.ConsensusValue.IfSome(card => card.Value.ShouldBe("5"));
        session.HasDiscrepancy.ShouldBeFalse();
    }
}