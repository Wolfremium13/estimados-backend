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

public class RejectJoinRequestUseCaseShould
{
    private readonly IEstimationRoomRepository _repository = Substitute.For<IEstimationRoomRepository>();
    private readonly RejectJoinRequestUseCase _useCase;
    private readonly Guid _roomId = Guid.NewGuid();
    private readonly Guid _requestId = Guid.NewGuid();
    private readonly EstimationRoom _room;

    public RejectJoinRequestUseCaseShould()
    {
        _useCase = new RejectJoinRequestUseCase(_repository);
        
        var request = new JoinRequestBuilder().WithId(_requestId).WithName("Bob").WithRole("Product Owner").Build();
        _room = new EstimationRoomBuilder().WithId(_roomId).WithModeratorName("Carlos").WithJoinRequest(request).Build();
    }

    [Fact]
    public async Task RejectRequestSuccessfully()
    {
        _repository.FindById(Arg.Any<RoomId>()).Returns(_room);
        _repository.Save(_room).Returns(Unit.Default);

        var result = await _useCase.Execute(new RejectJoinRequestCommand(_roomId, _requestId));

        result.IsRight.ShouldBeTrue();
        result.IfRight(success =>
        {
            success.ShouldBe(Unit.Default);
            _room.JoinRequests.ShouldContain(r => r.Id.Value == _requestId && r.Status == JoinRequestStatus.Rejected);
            _room.ActiveParticipants.ShouldNotContain(p => p.Name.Value == "Bob");
            _repository.Received(1).Save(_room);
        });
    }

    [Fact]
    public async Task FailWhenRoomNotFound()
    {
        _repository.FindById(Arg.Any<RoomId>()).Returns(LanguageExt.Common.Error.New(new RoomNotFoundException("Not found")));

        var result = await _useCase.Execute(new RejectJoinRequestCommand(_roomId, _requestId));

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error =>
        {
            error.ToException().ShouldBeOfType<RoomNotFoundException>();
            _repository.DidNotReceive().Save(Arg.Any<EstimationRoom>());
        });
    }

    [Fact]
    public async Task FailWhenRequestNotFound()
    {
        _repository.FindById(Arg.Any<RoomId>()).Returns(_room);
        var invalidRequestId = Guid.NewGuid();

        var result = await _useCase.Execute(new RejectJoinRequestCommand(_roomId, invalidRequestId));

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error =>
        {
            error.ToException().ShouldBeOfType<RoomNotFoundException>();
            _repository.DidNotReceive().Save(Arg.Any<EstimationRoom>());
        });
    }

    [Fact]
    public async Task FailWhenRoomIsClosed()
    {
        var request = new JoinRequestBuilder().WithId(_requestId).WithName("Bob").WithRole("Product Owner").Build();
        var closedRoom = new EstimationRoomBuilder().WithId(_roomId).WithModeratorName("Carlos").WithJoinRequest(request).AsClosed().Build();
        _repository.FindById(Arg.Any<RoomId>()).Returns(closedRoom);

        var result = await _useCase.Execute(new RejectJoinRequestCommand(_roomId, _requestId));

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error =>
        {
            error.ToException().ShouldBeOfType<RoomClosedException>();
            _repository.DidNotReceive().Save(Arg.Any<EstimationRoom>());
        });
    }
}
