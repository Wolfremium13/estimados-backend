using Common.Estimation.EstimationSession.Application.Contracts;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Wolfremium.Estimados.Hubs;
using static Wolfremium.Estimados.Controllers.EstimationSessionErrorsWeb;

namespace Wolfremium.Estimados.Controllers.V1.EstimationSession;

[ApiController]
[Route("v1/rooms/{roomId:guid}/session/transition/private-estimation")]
[Route("v1/rooms/{roomId:guid}/session/transition/private")]
[Tags("Estimation Session")]
public class EstimationSessionTransitionPrivateController(
    ITransitionToPrivateEstimationUseCase useCase,
    IHubContext<RoomHub> hubContext
) : ControllerBase
{
    [HttpPost]
    [Produces("application/json")]
    [EndpointSummary("Transition session to Private Estimation")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IResult> Transition([FromRoute] Guid roomId)
    {
        return await (
            from _ in useCase.Execute(new TransitionToPrivateEstimationCommand(roomId)).ToAsync()
            from notify in NotifySessionUpdated(roomId).ToAsync()
            select Results.Ok()
        ).Match<IResult>(
            success => success,
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