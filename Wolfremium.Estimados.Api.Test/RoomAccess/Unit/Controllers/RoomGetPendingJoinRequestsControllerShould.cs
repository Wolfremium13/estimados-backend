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

public class RoomGetPendingJoinRequestsControllerShould
{
    private readonly RoomGetPendingJoinRequestsController _controller;

    private readonly ILogger<RoomGetPendingJoinRequestsController> _logger =
        Substitute.For<ILogger<RoomGetPendingJoinRequestsController>>();

    private readonly IGetPendingJoinRequestsUseCase _useCase = Substitute.For<IGetPendingJoinRequestsUseCase>();

    public RoomGetPendingJoinRequestsControllerShould()
    {
        _controller = new RoomGetPendingJoinRequestsController(_useCase, _logger);

        var context = new DefaultHttpContext();
        context.Request.Path = "/v1/rooms/join-requests";
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = context
        };
    }

    [Fact]
    public async Task ReturnOkWithPendingRequestsOnSuccess()
    {
        var roomId = Guid.NewGuid();
        var dtos = new List<PendingJoinRequestDto>
        {
            new(Guid.NewGuid().ToString(), "Ana", "Developer"),
            new(Guid.NewGuid().ToString(), "Pedro", "Product Owner")
        }.AsReadOnly();
        _useCase.Execute(Arg.Any<GetPendingJoinRequestsQuery>()).Returns(dtos);

        var result = await _controller.Get(roomId);

        var okResult = result.ShouldBeOfType<Ok<IReadOnlyCollection<PendingJoinRequestDto>>>();
        okResult.Value.ShouldNotBeNull();
        okResult.Value.Count.ShouldBe(2);
        await _useCase.Received(1)
            .Execute(Arg.Is<GetPendingJoinRequestsQuery>(q => q.RoomId == roomId));
    }

    [Fact]
    public async Task ReturnProblemDetailsOnFailure()
    {
        var roomId = Guid.NewGuid();
        var error = Error.New(new RoomNotFoundException("Room not found"));
        _useCase.Execute(Arg.Any<GetPendingJoinRequestsQuery>()).Returns(error);

        var result = await _controller.Get(roomId);

        var problemResult = result.ShouldBeOfType<ProblemHttpResult>();
        problemResult.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
        problemResult.ProblemDetails.Detail.ShouldBe("Room not found");
    }
}