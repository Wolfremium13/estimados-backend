using Common.Estimation.RoomAccess.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using static Wolfremium.Estimados.Controllers.RoomAccessErrorsWeb;

namespace Wolfremium.Estimados.Controllers.V1.RoomAccess;

[ApiController]
[Route("v1/rooms")]
[Tags("Room Access")]
public class RoomGetParticipantsController(
    IGetRoomParticipantsUseCase getRoomParticipantsUseCase,
    ILogger<RoomGetParticipantsController> logger
) : ControllerBase
{
    [HttpGet("{roomId:guid}/participants")]
    [Produces("application/json")]
    [EndpointSummary("Get active participants of a room")]
    [ProducesResponseType(typeof(IReadOnlyCollection<ParticipantDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IResult> Get([FromRoute] Guid roomId)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Web API: Fetching participants for room {RoomId}.", roomId);

        return await (
            from dtos in getRoomParticipantsUseCase.Execute(new GetRoomParticipantsQuery(roomId)).ToAsync()
            select dtos
        ).Match<IResult>(
            success => Results.Ok(success),
            error => Results.Problem(MapToProblemDetails(error, HttpContext))
        );
    }
}