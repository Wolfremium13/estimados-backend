using Common.Estimation.RoomAccess.Application.Contracts;
using LanguageExt.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Wolfremium.Estimados.Controllers.V1.RoomAccess;
using Xunit;
using static Common.Estimation.RoomAccess.Domain.Errors.RoomAccessErrors;

namespace Wolfremium.Estimados.Api.Test.RoomAccess.Unit.Controllers;

public class RoomGetParticipantsControllerShould
{
    private readonly RoomGetParticipantsController _controller;

    private readonly ILogger<RoomGetParticipantsController> _logger =
        Substitute.For<ILogger<RoomGetParticipantsController>>();

    private readonly IGetRoomParticipantsUseCase _useCase = Substitute.For<IGetRoomParticipantsUseCase>();

    public RoomGetParticipantsControllerShould()
    {
        _controller = new RoomGetParticipantsController(_useCase, _logger);

        var context = new DefaultHttpContext();
        context.Request.Path = "/v1/rooms/participants";
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = context
        };
    }

    [Fact]
    public async Task ReturnOkWithParticipantsOnSuccess()
    {
        var roomId = Guid.NewGuid();
        var dtos = new List<ParticipantDto>
        {
            new("Carlos", "Moderator"),
            new("Ana", "Developer")
        }.AsReadOnly();
        _useCase.Execute(Arg.Any<GetRoomParticipantsQuery>()).Returns(dtos);

        var result = await _controller.Get(roomId);

        var okResult = result.ShouldBeOfType<Ok<IReadOnlyCollection<ParticipantDto>>>();
        okResult.Value.ShouldNotBeNull();
        okResult.Value.Count.ShouldBe(2);
        await _useCase.Received(1)
            .Execute(Arg.Is<GetRoomParticipantsQuery>(q => q.RoomId == roomId));
    }

    [Fact]
    public async Task ReturnProblemDetailsOnFailure()
    {
        var roomId = Guid.NewGuid();
        var error = Error.New(new RoomNotFoundException("Room not found"));
        _useCase.Execute(Arg.Any<GetRoomParticipantsQuery>()).Returns(error);

        var result = await _controller.Get(roomId);

        var problemResult = result.ShouldBeOfType<ProblemHttpResult>();
        problemResult.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
        problemResult.ProblemDetails.Detail.ShouldBe("Room not found");
    }
}