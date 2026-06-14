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
using static Common.Estimation.RoomAccess.Domain.Errors.RoomAccessErrors;

namespace Common.Test.Estimation.RoomAccess.Application.UseCases;

public class CreateRoomUseCaseShould
{
    private readonly IEstimationRoomRepository _repository = Substitute.For<IEstimationRoomRepository>();
    private readonly CreateRoomUseCase _useCase;

    public CreateRoomUseCaseShould()
    {
        _useCase = new CreateRoomUseCase(_repository);
    }

    [Fact]
    public async Task CreateRoomSuccessfully()
    {
        _repository.Save(Arg.Any<EstimationRoom>()).Returns(Unit.Default);

        var result = await _useCase.Execute(new CreateRoomCommand("Carlos"));

        result.IsRight.ShouldBeTrue();
        result.IfRight(success =>
        {
            success.RoomId.ShouldNotBe(Guid.Empty);
            success.ModeratorName.ShouldBe("Carlos");
            _repository.Received(1).Save(Arg.Any<EstimationRoom>());
        });
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task FailWhenModeratorNameIsInvalid(string? invalidName)
    {
        var result = await _useCase.Execute(new CreateRoomCommand(invalidName!));

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error =>
        {
            error.ToException().ShouldBeOfType<ClientValidationException>();
            _repository.DidNotReceive().Save(Arg.Any<EstimationRoom>());
        });
    }
}
