using Common.Estimation.EstimationSession.Application.Contracts;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Wolfremium.Estimados.Hubs;
using static Wolfremium.Estimados.Controllers.EstimationSessionErrorsWeb;

namespace Wolfremium.Estimados.Controllers.V1.EstimationSession;

[ApiController]
[Route("v1/rooms/{roomId:guid}/session/close")]
[Tags("Estimation Session")]
public class EstimationSessionCloseController(
    ICloseEstimationSessionUseCase closeEstimationSessionUseCase,
    IHubContext<RoomHub> hubContext
) : ControllerBase
{
    [HttpPost]
    [Produces("application/json")]
    [EndpointSummary("Close the estimation session")]
    [EndpointDescription("Closes and deletes the current estimation session for the room, resetting the state of all participants.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IResult> Close([FromRoute] Guid roomId)
    {
        return await (
            from _ in closeEstimationSessionUseCase
                .Execute(new CloseEstimationSessionCommand(roomId)).ToAsync()
            from __ in NotifySessionUpdated(roomId).ToAsync()
            select Unit.Default
        ).Match<IResult>(
            success => Results.Ok(),
            error => Results.Problem(MapToProblemDetails(error, HttpContext))
        );
    }

    private async Task<Either<Error, Unit>> NotifySessionUpdated(Guid roomId)
    {
        try
        {
            await hubContext.Clients.Group($"room_{roomId}").SendAsync("OnSessionUpdated");
            return Unit.Default;
        }
        catch (Exception ex)
        {
            return Error.New(ex);
        }
    }
}
