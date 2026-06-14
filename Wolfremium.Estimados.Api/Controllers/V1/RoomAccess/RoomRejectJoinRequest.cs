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
public class RoomRejectJoinRequest(
    IRejectJoinRequestUseCase rejectJoinRequestUseCase,
    IHubContext<RoomHub> hubContext,
    ILogger<RoomRejectJoinRequest>? logger = null,
    IHostEnvironment? env = null
) : ControllerBase
{
    [HttpPost("{roomId:guid}/join-requests/{requestId:guid}/reject")]
    [Produces("application/json")]
    [EndpointSummary("Reject a pending request to join the room")]
    [EndpointDescription("Rejects a user's join request. The applicant is notified in real-time via SignalR.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IResult> Reject(
        [FromRoute] Guid roomId,
        [FromRoute] Guid requestId
    ) =>
        await (
            from _ in rejectJoinRequestUseCase.Execute(new RejectJoinRequestCommand(roomId, requestId)).ToAsync()
            from __ in NotifyApplicantRejected(roomId, requestId).ToAsync()
            select Unit.Default
        ).Match<IResult>(
            success => Results.Ok(),
            error => Results.Problem(MapToProblemDetails(error, HttpContext))
        );

    private async Task<Either<Error, Unit>> NotifyApplicantRejected(Guid roomId, Guid requestId)
    {
        try
        {
            var groupName = $"room_{roomId}";

            var isDev = env?.IsDevelopment() ??
                        (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development");
            if (isDev)
            {
                logger?.LogInformation(
                    "Web API: Sending notification OnJoinRequestRejected to room {RoomId} for request {RequestId}.",
                    roomId, requestId);
            }

            await hubContext.Clients.Group(groupName).SendAsync("OnJoinRequestRejected", requestId);
            return Unit.Default;
        }
        catch (Exception ex)
        {
            return Error.New(ex);
        }
    }
}