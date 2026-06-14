using Common.Estimation.RoomAccess.Application.Contracts;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Wolfremium.Estimados.Hubs;
using static Wolfremium.Estimados.Controllers.RoomAccessErrorsWeb;

namespace Wolfremium.Estimados.Controllers.V1.RoomAccess;

[ApiController]
[Route("v1/rooms")]
[Tags("Room Access")]
public class RoomApproveJoinRequest(
    IApproveJoinRequestUseCase approveJoinRequestUseCase,
    IHubContext<RoomHub> hubContext,
    ILogger<RoomApproveJoinRequest> logger
) : ControllerBase
{
    [HttpPost("{roomId:guid}/join-requests/{requestId:guid}/approve")]
    [Produces("application/json")]
    [EndpointSummary("Approve a pending request to join the room")]
    [EndpointDescription("Approves a user's join request. The applicant is notified in real-time via SignalR.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IResult> Approve(
        [FromRoute] Guid roomId,
        [FromRoute] Guid requestId
    )
    {
        return await (
            from _ in approveJoinRequestUseCase.Execute(new ApproveJoinRequestCommand(roomId, requestId)).ToAsync()
            from __ in NotifyApplicantApproved(roomId, requestId).ToAsync()
            select Unit.Default
        ).Match<IResult>(
            success => Results.Ok(),
            error => Results.Problem(MapToProblemDetails(error, HttpContext))
        );
    }

    private async Task<Either<Error, Unit>> NotifyApplicantApproved(Guid roomId, Guid requestId)
    {
        try
        {
            var groupName = $"room_{roomId}";

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation(
                    "Web API: Sending notification OnJoinRequestApproved to room {RoomId} for request {RequestId}.",
                    roomId, requestId);

            await hubContext.Clients.Group(groupName).SendAsync("OnJoinRequestApproved", requestId);
            return Unit.Default;
        }
        catch (Exception ex)
        {
            return Error.New(ex);
        }
    }
}