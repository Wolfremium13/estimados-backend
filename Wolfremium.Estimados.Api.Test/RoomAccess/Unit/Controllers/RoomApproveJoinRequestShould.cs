using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Xunit;
using Shouldly;
using NSubstitute;
using LanguageExt;
using Common.Estimation.RoomAccess.Application.Contracts;
using Wolfremium.Estimados.Controllers.V1.RoomAccess;
using Wolfremium.Estimados.Hubs;
using static Common.Estimation.RoomAccess.Domain.Errors.RoomAccessErrors;

namespace Wolfremium.Estimados.Api.Test.RoomAccess.Unit.Controllers;

public class RoomApproveJoinRequestShould
{
    private readonly IApproveJoinRequestUseCase _useCase = Substitute.For<IApproveJoinRequestUseCase>();
    private readonly IHubContext<RoomHub> _hubContext = Substitute.For<IHubContext<RoomHub>>();
    private readonly IHubClients _hubClients = Substitute.For<IHubClients>();
    private readonly IClientProxy _clientProxy = Substitute.For<IClientProxy>();
    private readonly RoomApproveJoinRequest _controller;

    public RoomApproveJoinRequestShould()
    {
        _hubContext.Clients.Returns(_hubClients);
        _hubClients.Group(Arg.Any<string>()).Returns(_clientProxy);
        
        _controller = new RoomApproveJoinRequest(_useCase, _hubContext);
        
        var context = new DefaultHttpContext();
        context.Request.Path = "/v1/rooms/approve";
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
        _useCase.Execute(Arg.Any<ApproveJoinRequestCommand>()).Returns(LanguageExt.Unit.Default);

        var result = await _controller.Approve(roomId, requestId);

        result.ShouldBeOfType<Ok>();
        await _useCase.Received(1).Execute(Arg.Is<ApproveJoinRequestCommand>(c => c.RoomId == roomId && c.RequestId == requestId));
        await _clientProxy.Received(1).SendCoreAsync(
            "OnJoinRequestApproved",
            Arg.Is<object[]>(args => args.Length == 1 && (Guid)args[0] == requestId),
            Arg.Any<System.Threading.CancellationToken>()
        );
    }

    [Fact]
    public async Task ReturnProblemDetailsOnFailure()
    {
        var roomId = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        var error = LanguageExt.Common.Error.New(new RoomClosedException("Room is closed"));
        _useCase.Execute(Arg.Any<ApproveJoinRequestCommand>()).Returns(error);

        var result = await _controller.Approve(roomId, requestId);

        var problemResult = result.ShouldBeOfType<ProblemHttpResult>();
        problemResult.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        problemResult.ProblemDetails.Detail.ShouldBe("Room is closed");
        
        await _clientProxy.DidNotReceive().SendCoreAsync(
            Arg.Any<string>(),
            Arg.Any<object[]>(),
            Arg.Any<System.Threading.CancellationToken>()
        );
    }
}
