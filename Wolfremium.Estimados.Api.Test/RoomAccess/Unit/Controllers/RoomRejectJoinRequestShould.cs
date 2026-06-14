using Common.Estimation.RoomAccess.Application.Contracts;
using LanguageExt.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Wolfremium.Estimados.Controllers.V1.RoomAccess;
using Wolfremium.Estimados.Hubs;
using Xunit;
using static Common.Estimation.RoomAccess.Domain.Errors.RoomAccessErrors;

namespace Wolfremium.Estimados.Api.Test.RoomAccess.Unit.Controllers;

public class RoomRejectJoinRequestShould
{
    private readonly IClientProxy _clientProxy = Substitute.For<IClientProxy>();
    private readonly RoomRejectJoinRequest _controller;
    private readonly IHubClients _hubClients = Substitute.For<IHubClients>();
    private readonly IHubContext<RoomHub> _hubContext = Substitute.For<IHubContext<RoomHub>>();
    private readonly ILogger<RoomRejectJoinRequest> _logger = Substitute.For<ILogger<RoomRejectJoinRequest>>();
    private readonly IRejectJoinRequestUseCase _useCase = Substitute.For<IRejectJoinRequestUseCase>();

    public RoomRejectJoinRequestShould()
    {
        _hubContext.Clients.Returns(_hubClients);
        _hubClients.Group(Arg.Any<string>()).Returns(_clientProxy);

        _controller = new RoomRejectJoinRequest(_useCase, _hubContext, _logger);

        var context = new DefaultHttpContext();
        context.Request.Path = "/v1/rooms/reject";
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = context
        };
    }

    [Fact]
    public async Task ReturnOkOnSuccess()
    {
        var roomId = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        _useCase.Execute(Arg.Any<RejectJoinRequestCommand>()).Returns(LanguageExt.Unit.Default);

        var result = await _controller.Reject(roomId, requestId);

        result.ShouldBeOfType<Ok>();
        await _useCase.Received(1)
            .Execute(Arg.Is<RejectJoinRequestCommand>(c => c.RoomId == roomId && c.RequestId == requestId));
        await _clientProxy.Received(1).SendCoreAsync(
            "OnJoinRequestRejected",
            Arg.Is<object[]>(args => args.Length == 1 && (Guid)args[0] == requestId),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task ReturnProblemDetailsOnFailure()
    {
        var roomId = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        var error = Error.New(new RoomNotFoundException("Request not found"));
        _useCase.Execute(Arg.Any<RejectJoinRequestCommand>()).Returns(error);

        var result = await _controller.Reject(roomId, requestId);

        var problemResult = result.ShouldBeOfType<ProblemHttpResult>();
        problemResult.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
        problemResult.ProblemDetails.Detail.ShouldBe("Request not found");

        await _clientProxy.DidNotReceive().SendCoreAsync(
            Arg.Any<string>(),
            Arg.Any<object[]>(),
            Arg.Any<CancellationToken>()
        );
    }
}