using Common.Estimation.RoomAccess.Application.Contracts;
using Common.Estimation.RoomAccess.Application.UseCases;
using Common.Estimation.RoomAccess.Domain.Models;
using Common.Estimation.RoomAccess.Domain.Ports;
using LanguageExt;
using NSubstitute;
using Shouldly;
using Xunit;
using static Common.Estimation.RoomAccess.Domain.Errors.RoomAccessErrors;

namespace Common.Test.Estimation.RoomAccess.Application.UseCases;

public class CreateRoomUseCaseShould
{
    private readonly IGuidGenerator _guidGenerator = Substitute.For<IGuidGenerator>();
    private readonly IEstimationRoomRepository _repository = Substitute.For<IEstimationRoomRepository>();
    private readonly CreateRoomUseCase _useCase;

    public CreateRoomUseCaseShould()
    {
        _guidGenerator.NewGuid().Returns(Guid.NewGuid());
        _useCase = new CreateRoomUseCase(_repository, _guidGenerator);
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