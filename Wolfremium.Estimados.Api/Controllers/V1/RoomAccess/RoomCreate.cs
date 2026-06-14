using System.Text.Json.Serialization;
using Common.Estimation.RoomAccess.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using static Wolfremium.Estimados.Controllers.RoomAccessErrorsWeb;

namespace Wolfremium.Estimados.Controllers.V1.RoomAccess;

[ApiController]
[Route("v1/rooms")]
[Tags("Room Access")]
public class RoomCreate(ICreateRoomUseCase createRoomUseCase) : ControllerBase
{
    [HttpPost]
    [Produces("application/json")]
    [EndpointSummary("Create a new estimation room")]
    [EndpointDescription("Creates a new estimation room with a unique UUID, registering the creator as the Moderator.")]
    [ProducesResponseType(typeof(RoomCreateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IResult> Create([FromQuery(Name = "moderatorName")] string moderatorName) =>
        await (
            from info in createRoomUseCase.Execute(new CreateRoomCommand(moderatorName)).ToAsync()
            select new RoomCreateResponse(info.RoomId, info.ModeratorName)
        ).Match<IResult>(
            success => Results.Ok(success),
            error => Results.Problem(MapToProblemDetails(error, HttpContext))
        );
}

public record RoomCreateResponse(
    [property: JsonPropertyName("roomId")] Guid RoomId,
    [property: JsonPropertyName("moderatorName")] string ModeratorName
);
