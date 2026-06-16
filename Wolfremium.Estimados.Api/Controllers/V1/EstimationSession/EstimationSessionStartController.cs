using Common.Estimation.EstimationSession.Application.Contracts;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Wolfremium.Estimados.Hubs;
using static Wolfremium.Estimados.Controllers.EstimationSessionErrorsWeb;

namespace Wolfremium.Estimados.Controllers.V1.EstimationSession;

[ApiController]
[Route("v1/rooms/{roomId:guid}/session/start")]
[Tags("Estimation Session")]
public class EstimationSessionStartController(
    IStartEstimationSessionUseCase startEstimationSessionUseCase,
    IHubContext<RoomHub> hubContext
) : ControllerBase
{
    [HttpPost]
    [Produces("application/json")]
    [EndpointSummary("Start an estimation session")]
    [EndpointDescription("Starts a new estimation session for the specified room with a story description.")]
    [ProducesResponseType(typeof(EstimationSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IResult> Start([FromRoute] Guid roomId,
        [FromQuery(Name = "storyDescription")] string storyDescription)
    {
        return await (
            from dto in startEstimationSessionUseCase
                .Execute(new StartEstimationSessionCommand(roomId, storyDescription)).ToAsync()
            from _ in NotifySessionUpdated(roomId).ToAsync()
            select dto
        ).Match<IResult>(
            success => Results.Ok(success),
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