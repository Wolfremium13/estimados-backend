using System;
using System.Threading.Tasks;
using Xunit;
using Shouldly;
using NSubstitute;
using LanguageExt;
using Common.Estimation.RoomAccess.Domain.Models;
using Common.Estimation.RoomAccess.Domain.Ports;
using Common.Estimation.RoomAccess.Application.Contracts;
using Common.Estimation.RoomAccess.Application.UseCases;
using Common.Test.Estimation.RoomAccess.Domain.Builders;
using static Common.Estimation.RoomAccess.Domain.Errors.RoomAccessErrors;

namespace Common.Test.Estimation.RoomAccess.Application.UseCases;

public class RequestToJoinUseCaseShould
{
    private readonly IEstimationRoomRepository _repository = Substitute.For<IEstimationRoomRepository>();
    private readonly RequestToJoinUseCase _useCase;
    private readonly Guid _roomId = Guid.NewGuid();
    private readonly EstimationRoom _room;

    public RequestToJoinUseCaseShould()
    {
        _useCase = new RequestToJoinUseCase(_repository);
        _room = new EstimationRoomBuilder().WithId(_roomId).WithModeratorName("Carlos").Build();
    }

    [Fact]
    public async Task RequestToJoinSuccessfully()
    {
        _repository.FindById(Arg.Any<RoomId>()).Returns(_room);
        _repository.Save(_room).Returns(Unit.Default);

        var result = await _useCase.Execute(new RequestToJoinCommand(_roomId, "Ana", "Developer"));

        result.IsRight.ShouldBeTrue();
        result.IfRight(success =>
        {
            success.RequestId.ShouldNotBe(Guid.Empty);
            success.RoomId.ShouldBe(_roomId);
            success.ParticipantName.ShouldBe("Ana");
            success.ParticipantRole.ShouldBe("Developer");
            _repository.Received(1).Save(_room);
        });
    }

    [Fact]
    public async Task FailWhenRoomNotFound()
    {
        _repository.FindById(Arg.Any<RoomId>()).Returns(LanguageExt.Common.Error.New(new RoomNotFoundException("Not found")));

        var result = await _useCase.Execute(new RequestToJoinCommand(_roomId, "Ana", "Developer"));

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error =>
        {
            error.ToException().ShouldBeOfType<RoomNotFoundException>();
            _repository.DidNotReceive().Save(Arg.Any<EstimationRoom>());
        });
    }

    [Fact]
    public async Task FailWhenParticipantNameIsInvalid()
    {
        var result = await _useCase.Execute(new RequestToJoinCommand(_roomId, "", "Developer"));

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error =>
        {
            error.ToException().ShouldBeOfType<ClientValidationException>();
            _repository.DidNotReceive().Save(Arg.Any<EstimationRoom>());
        });
    }

    [Fact]
    public async Task FailWhenParticipantRoleIsInvalid()
    {
        var result = await _useCase.Execute(new RequestToJoinCommand(_roomId, "Ana", "Manager"));

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error =>
        {
            error.ToException().ShouldBeOfType<InvalidRoleException>();
            _repository.DidNotReceive().Save(Arg.Any<EstimationRoom>());
        });
    }

    [Fact]
    public async Task FailWhenRoomIsClosed()
    {
        var closedRoom = new EstimationRoomBuilder().WithId(_roomId).WithModeratorName("Carlos").AsClosed().Build();
        _repository.FindById(Arg.Any<RoomId>()).Returns(closedRoom);

        var result = await _useCase.Execute(new RequestToJoinCommand(_roomId, "Ana", "Developer"));

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error =>
        {
            error.ToException().ShouldBeOfType<RoomClosedException>();
            _repository.DidNotReceive().Save(Arg.Any<EstimationRoom>());
        });
    }
}
