using Common.Estimation.EstimationSession.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using static Wolfremium.Estimados.Controllers.EstimationSessionErrorsWeb;

namespace Wolfremium.Estimados.Controllers.V1.EstimationSession;

[ApiController]
[Route("v1/rooms/{roomId:guid}/session")]
[Tags("Estimation Session")]
public class EstimationSessionGetController(IGetEstimationSessionUseCase useCase) : ControllerBase
{
    [HttpGet]
    [Produces("application/json")]
    [EndpointSummary("Get the current estimation session")]
    [ProducesResponseType(typeof(EstimationSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IResult> Get([FromRoute] Guid roomId)
    {
        return await (
            from dto in useCase.Execute(new GetEstimationSessionQuery(roomId)).ToAsync()
            select dto
        ).Match<IResult>(
            success => Results.Ok(success),
            error => Results.Problem(MapToProblemDetails(error, HttpContext))
        );
    }
}