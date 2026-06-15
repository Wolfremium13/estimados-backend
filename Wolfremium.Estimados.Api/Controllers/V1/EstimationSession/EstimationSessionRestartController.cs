using Common.Estimation.EstimationSession.Application.Contracts;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Wolfremium.Estimados.Hubs;
using static Wolfremium.Estimados.Controllers.EstimationSessionErrorsWeb;

namespace Wolfremium.Estimados.Controllers.V1.EstimationSession;

[ApiController]
[Route("v1/rooms/{roomId:guid}/session/restart")]
[Tags("Estimation Session")]
public class EstimationSessionRestartController(
    IResetVotesUseCase resetVotesUseCase,
    IHubContext<RoomHub> hubContext
) : ControllerBase
{
    [HttpPost]
    [Produces("application/json")]
    [EndpointSummary("Restart estimation session")]
    [EndpointDescription("Resets all cast votes and transitions session state back to private estimation.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IResult> Restart([FromRoute] Guid roomId)
    {
        return await (
            from _ in resetVotesUseCase.Execute(new ResetVotesCommand(roomId)).ToAsync()
            from notify in NotifyRestart(roomId).ToAsync()
            select Results.Ok()
        ).Match<IResult>(
            success => success,
            error => Results.Problem(MapToProblemDetails(error, HttpContext))
        );
    }

    private async Task<Either<Error, Unit>> NotifyRestart(Guid roomId)
    {
        try
        {
            await hubContext.Clients.Group($"room_{roomId}").SendAsync("OnVotesRestarted");
            return Unit.Default;
        }
        catch (Exception ex)
        {
            return Error.New(ex);
        }
    }
}