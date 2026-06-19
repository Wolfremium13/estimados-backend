using Common.Estimation.RoomAccess.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using static Wolfremium.Estimados.Controllers.RoomAccessErrorsWeb;

namespace Wolfremium.Estimados.Controllers.V1.RoomAccess;

[ApiController]
[Route("v1/rooms")]
[Tags("Room Access")]
public class RoomGetPendingJoinRequestsController(
    IGetPendingJoinRequestsUseCase getPendingJoinRequestsUseCase,
    ILogger<RoomGetPendingJoinRequestsController> logger
) : ControllerBase
{
    [HttpGet("{roomId:guid}/join-requests")]
    [Produces("application/json")]
    [EndpointSummary("Get pending join requests of a room")]
    [ProducesResponseType(typeof(IReadOnlyCollection<PendingJoinRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IResult> Get([FromRoute] Guid roomId)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Web API: Fetching pending join requests for room {RoomId}.", roomId);

        return await (
            from dtos in getPendingJoinRequestsUseCase.Execute(new GetPendingJoinRequestsQuery(roomId)).ToAsync()
            select dtos
        ).Match<IResult>(
            success => Results.Ok(success),
            error => Results.Problem(MapToProblemDetails(error, HttpContext))
        );
    }
}