using Common.Estimation.RoomAccess.Application.Contracts;
using LanguageExt.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using Shouldly;
using Wolfremium.Estimados.Controllers.V1.RoomAccess;
using Wolfremium.Estimados.Hubs;
using Xunit;
using static Common.Estimation.RoomAccess.Domain.Errors.RoomAccessErrors;

using Microsoft.Extensions.Logging;

namespace Wolfremium.Estimados.Api.Test.RoomAccess.Unit.Controllers;

public class RoomRequestJoinShould
{
    private readonly IClientProxy _clientProxy = Substitute.For<IClientProxy>();
    private readonly ILogger<RoomRequestJoin> _logger = Substitute.For<ILogger<RoomRequestJoin>>();
    private readonly RoomRequestJoin _controller;
    private readonly IHubClients _hubClients = Substitute.For<IHubClients>();
    private readonly IHubContext<RoomHub> _hubContext = Substitute.For<IHubContext<RoomHub>>();
    private readonly IRequestToJoinUseCase _useCase = Substitute.For<IRequestToJoinUseCase>();

    public RoomRequestJoinShould()
    {
        _hubContext.Clients.Returns(_hubClients);
        _hubClients.Group(Arg.Any<string>()).Returns(_clientProxy);

        _controller = new RoomRequestJoin(_useCase, _hubContext, _logger);

        var context = new DefaultHttpContext();
        context.Request.Path = "/v1/rooms/join-requests";
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = context
        };
    }

    [Fact]
    public async Task ReturnOkAndNotifyHubOnSuccess()
    {
        var roomId = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        var info = new JoinRequestInfo(requestId, roomId, "Ana", "Developer");
        _useCase.Execute(Arg.Any<RequestToJoinCommand>()).Returns(info);

        var payload = new RoomJoinRequestPayload("Ana", "Developer");
        var result = await _controller.RequestJoin(roomId, payload);

        var okResult = result.ShouldBeOfType<Ok<RoomJoinRequestResponse>>();
        okResult.Value.ShouldNotBeNull();
        okResult.Value.RequestId.ShouldBe(requestId);
        okResult.Value.RoomId.ShouldBe(roomId);
        okResult.Value.ParticipantName.ShouldBe("Ana");
        okResult.Value.ParticipantRole.ShouldBe("Developer");

        await _useCase.Received(1).Execute(Arg.Is<RequestToJoinCommand>(c =>
            c.RoomId == roomId && c.ParticipantName == "Ana" && c.ParticipantRole == "Developer"));
        await _clientProxy.Received(1).SendCoreAsync(
            "OnJoinRequestReceived",
            Arg.Is<object[]>(args =>
                args.Length == 3 && (Guid)args[0] == requestId && (string)args[1] == "Ana" &&
                (string)args[2] == "Developer"),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task ReturnNotFoundWhenRoomDoesNotExist()
    {
        var roomId = Guid.NewGuid();
        var error = Error.New(new RoomNotFoundException("Room not found"));
        _useCase.Execute(Arg.Any<RequestToJoinCommand>()).Returns(error);

        var payload = new RoomJoinRequestPayload("Ana", "Developer");
        var result = await _controller.RequestJoin(roomId, payload);

        var problemResult = result.ShouldBeOfType<ProblemHttpResult>();
        problemResult.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
        problemResult.ProblemDetails.Detail.ShouldBe("Room not found");

        await _clientProxy.DidNotReceive().SendCoreAsync(
            Arg.Any<string>(),
            Arg.Any<object[]>(),
            Arg.Any<CancellationToken>()
        );
    }
}