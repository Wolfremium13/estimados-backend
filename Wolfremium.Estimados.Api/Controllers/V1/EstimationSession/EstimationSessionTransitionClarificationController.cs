using Common.Estimation.EstimationSession.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using static Wolfremium.Estimados.Controllers.EstimationSessionErrorsWeb;

namespace Wolfremium.Estimados.Controllers.V1.EstimationSession;

[ApiController]
[Route("v1/rooms/{roomId:guid}/session/transition/clarification")]
[Tags("Estimation Session")]
public class EstimationSessionTransitionClarificationController(
    ITransitionToClarificationUseCase useCase
) : ControllerBase
{
    [HttpPost]
    [Produces("application/json")]
    [EndpointSummary("Transition session to Clarification Discussion")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IResult> Transition([FromRoute] Guid roomId)
    {
        return await (
            from _ in useCase.Execute(new TransitionToClarificationCommand(roomId)).ToAsync()
            select Results.Ok()
        ).Match<IResult>(
            success => success,
            error => Results.Problem(MapToProblemDetails(error, HttpContext))
        );
    }
}