using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Shouldly;
using NSubstitute;
using LanguageExt;
using Common.Estimation.RoomAccess.Application.Contracts;
using Wolfremium.Estimados.Controllers.V1.RoomAccess;
using static Common.Estimation.RoomAccess.Domain.Errors.RoomAccessErrors;

namespace Wolfremium.Estimados.Api.Test.RoomAccess.Unit.Controllers;

public class RoomCreateShould
{
    private readonly ICreateRoomUseCase _useCase = Substitute.For<ICreateRoomUseCase>();
    private readonly RoomCreate _controller;

    public RoomCreateShould()
    {
        _controller = new RoomCreate(_useCase);
        
        var context = new DefaultHttpContext();
        context.Request.Path = "/v1/rooms";
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = context
        };
    }

    [Fact]
    public async Task ReturnOkWithResponseDetailsOnSuccess()
    {
        var roomId = Guid.NewGuid();
        var info = new CreatedRoomInfo(roomId, "Carlos");
        _useCase.Execute(Arg.Any<CreateRoomCommand>()).Returns(info);

        var result = await _controller.Create("Carlos");

        var okResult = result.ShouldBeOfType<Ok<RoomCreateResponse>>();
        okResult.Value.ShouldNotBeNull();
        okResult.Value.RoomId.ShouldBe(roomId);
        okResult.Value.ModeratorName.ShouldBe("Carlos");
        await _useCase.Received(1).Execute(Arg.Is<CreateRoomCommand>(c => c.ModeratorName == "Carlos"));
    }

    [Fact]
    public async Task ReturnProblemDetailsOnFailure()
    {
        var error = LanguageExt.Common.Error.New(new ClientValidationException("Name is required"));
        _useCase.Execute(Arg.Any<CreateRoomCommand>()).Returns(error);

        var result = await _controller.Create("");

        var problemResult = result.ShouldBeOfType<ProblemHttpResult>();
        problemResult.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        problemResult.ProblemDetails.Detail.ShouldBe("Name is required");
    }
}
