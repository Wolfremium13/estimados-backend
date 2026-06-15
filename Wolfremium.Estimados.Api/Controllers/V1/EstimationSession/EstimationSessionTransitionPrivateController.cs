using Common.Estimation.EstimationSession.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using static Wolfremium.Estimados.Controllers.EstimationSessionErrorsWeb;

namespace Wolfremium.Estimados.Controllers.V1.EstimationSession;

[ApiController]
[Route("v1/rooms/{roomId:guid}/session/transition/private-estimation")]
[Tags("Estimation Session")]
public class EstimationSessionTransitionPrivateController(
    ITransitionToPrivateEstimationUseCase useCase
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
            select Results.Ok()
        ).Match<IResult>(
            success => success,
            error => Results.Problem(MapToProblemDetails(error, HttpContext))
        );
    }
}