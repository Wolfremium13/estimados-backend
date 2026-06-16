using Common.Estimation.RoomAccess.Application.Contracts;
using Common.Estimation.RoomAccess.Application.UseCases;
using Common.Estimation.RoomAccess.Domain.Models;
using Common.Estimation.RoomAccess.Domain.Ports;
using Common.Estimation.EstimationSession.Domain.Models;
using Common.Estimation.EstimationSession.Domain.Ports;
using Common.Test.Estimation.RoomAccess.Domain.Builders;
using Common.Test.Estimation.EstimationSession.Domain.Builders;
using LanguageExt;
using LanguageExt.Common;
using NSubstitute;
using Shouldly;
using Xunit;
using static Common.Estimation.RoomAccess.Domain.Errors.RoomAccessErrors;

namespace Common.Test.Estimation.RoomAccess.Application.UseCases;

public class DisconnectParticipantUseCaseShould
{
    private readonly IEstimationRoomRepository _roomRepository = Substitute.For<IEstimationRoomRepository>();
    private readonly IEstimationSessionRepository _sessionRepository = Substitute.For<IEstimationSessionRepository>();
    private readonly EstimationRoom _room;
    private readonly Guid _roomId = Guid.NewGuid();
    private readonly DisconnectParticipantUseCase _useCase;

    public DisconnectParticipantUseCaseShould()
    {
        _useCase = new DisconnectParticipantUseCase(_roomRepository, _sessionRepository);
        _room = new EstimationRoomBuilder()
            .WithId(_roomId)
            .WithModeratorName("Carlos")
            .Build();
        
        var joinRequest = new JoinRequestBuilder()
            .WithName("Ana")
            .WithRole(ParticipantRole.Developer)
            .Build();
        
        _room.AddJoinRequest(joinRequest);
        _room.ApproveJoinRequest(joinRequest.Id);
    }

    [Fact]
    public async Task DisconnectParticipantSuccessfullyWhenSessionExists()
    {
        _roomRepository.FindById(Arg.Any<RoomId>()).Returns(_room);
        _roomRepository.Save(_room).Returns(Unit.Default);

        var session = new EstimationSessionBuilder()
            .WithRoomId(_roomId)
            .WithState(SessionState.PrivateEstimation)
            .WithVote("Ana", ParticipantRole.Developer, "5")
            .Build();

        _sessionRepository.FindById(Arg.Any<SessionId>()).Returns(session);
        _sessionRepository.Save(session).Returns(Unit.Default);

        var result = await _useCase.Execute(new DisconnectParticipantCommand(_roomId, "Ana"));

        result.IsRight.ShouldBeTrue();
        _room.ActiveParticipants.Any(p => p.Name.Value == "Ana").ShouldBeFalse();
        session.GetVotes().Any(v => v.Name.Value == "Ana").ShouldBeFalse();
        
        await _roomRepository.Received(1).Save(_room);
        await _sessionRepository.Received(1).Save(session);
    }

    [Fact]
    public async Task DisconnectParticipantSuccessfullyWhenSessionDoesNotExist()
    {
        _roomRepository.FindById(Arg.Any<RoomId>()).Returns(_room);
        _roomRepository.Save(_room).Returns(Unit.Default);

        _sessionRepository.FindById(Arg.Any<SessionId>()).Returns(Error.New(new Exception("Session not found")));

        var result = await _useCase.Execute(new DisconnectParticipantCommand(_roomId, "Ana"));

        result.IsRight.ShouldBeTrue();
        _room.ActiveParticipants.Any(p => p.Name.Value == "Ana").ShouldBeFalse();
        
        await _roomRepository.Received(1).Save(_room);
        await _sessionRepository.DidNotReceive().Save(Arg.Any<Common.Estimation.EstimationSession.Domain.Models.EstimationSession>());
    }

    [Fact]
    public async Task FailWhenRoomNotFound()
    {
        _roomRepository.FindById(Arg.Any<RoomId>()).Returns(Error.New(new RoomNotFoundException("Not found")));

        var result = await _useCase.Execute(new DisconnectParticipantCommand(_roomId, "Ana"));

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error =>
        {
            error.ToException().ShouldBeOfType<RoomNotFoundException>();
            _roomRepository.DidNotReceive().Save(Arg.Any<EstimationRoom>());
        });
    }
}
