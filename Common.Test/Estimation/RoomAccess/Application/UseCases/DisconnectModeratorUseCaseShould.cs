using Common.Estimation.RoomAccess.Application.Contracts;
using Common.Estimation.RoomAccess.Application.UseCases;
using Common.Estimation.RoomAccess.Domain.Models;
using Common.Estimation.RoomAccess.Domain.Ports;
using Common.Test.Estimation.RoomAccess.Domain.Builders;
using LanguageExt;
using LanguageExt.Common;
using NSubstitute;
using Shouldly;
using Xunit;
using static Common.Estimation.RoomAccess.Domain.Errors.RoomAccessErrors;

namespace Common.Test.Estimation.RoomAccess.Application.UseCases;

public class DisconnectModeratorUseCaseShould
{
    private readonly IEstimationRoomRepository _repository = Substitute.For<IEstimationRoomRepository>();
    private readonly EstimationRoom _room;
    private readonly Guid _roomId = Guid.NewGuid();
    private readonly DisconnectModeratorUseCase _useCase;

    public DisconnectModeratorUseCaseShould()
    {
        _useCase = new DisconnectModeratorUseCase(_repository);
        _room = new EstimationRoomBuilder().WithId(_roomId).WithModeratorName("Carlos").Build();
    }

    [Fact]
    public async Task CloseRoomSuccessfully()
    {
        _repository.FindById(Arg.Any<RoomId>()).Returns(_room);
        _repository.Save(_room).Returns(Unit.Default);

        var result = await _useCase.Execute(new DisconnectModeratorCommand(_roomId));

        result.IsRight.ShouldBeTrue();
        result.IfRight(success =>
        {
            success.ShouldBe(Unit.Default);
            _room.IsActive.ShouldBeFalse();
            _repository.Received(1).Save(_room);
        });
    }

    [Fact]
    public async Task FailWhenRoomNotFound()
    {
        _repository.FindById(Arg.Any<RoomId>()).Returns(Error.New(new RoomNotFoundException("Not found")));

        var result = await _useCase.Execute(new DisconnectModeratorCommand(_roomId));

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error =>
        {
            error.ToException().ShouldBeOfType<RoomNotFoundException>();
            _repository.DidNotReceive().Save(Arg.Any<EstimationRoom>());
        });
    }
}