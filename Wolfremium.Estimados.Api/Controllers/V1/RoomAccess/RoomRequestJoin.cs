using System.Text.Json.Serialization;
using Common.Estimation.RoomAccess.Application.Contracts;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Wolfremium.Estimados.Hubs;
using static Wolfremium.Estimados.Controllers.RoomAccessErrorsWeb;

namespace Wolfremium.Estimados.Controllers.V1.RoomAccess;

[ApiController]
[Route("v1/rooms")]
[Tags("Room Access")]
public class RoomRequestJoin(
    IRequestToJoinUseCase requestToJoinUseCase,
    IHubContext<RoomHub> hubContext,
    ILogger<RoomRequestJoin> logger
) : ControllerBase
{
    [HttpPost("{roomId:guid}/join-requests")]
    [Produces("application/json")]
    [EndpointSummary("Request to join an estimation room")]
    [EndpointDescription(
        "Submits a pending request to join the room. The room moderator will be notified in real-time via SignalR.")]
    [ProducesResponseType(typeof(RoomJoinRequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IResult> RequestJoin(
        [FromRoute] Guid roomId,
        [FromBody] RoomJoinRequestPayload payload
    )
    {
        return await (
            from info in requestToJoinUseCase.Execute(new RequestToJoinCommand(roomId, payload.Name, payload.Role))
                .ToAsync()
            from _ in NotifyModerator(info).ToAsync()
            select new RoomJoinRequestResponse(info.RequestId, info.RoomId, info.ParticipantName, info.ParticipantRole)
        ).Match<IResult>(
            success => Results.Ok(success),
            error => Results.Problem(MapToProblemDetails(error, HttpContext))
        );
    }

    private async Task<Either<Error, Unit>> NotifyModerator(JoinRequestInfo info)
    {
        try
        {
            var groupName = $"room_{info.RoomId}";

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation(
                    "Web API: Sending notification OnJoinRequestReceived to room {RoomId} for participant {ParticipantName} ({ParticipantRole}).",
                    info.RoomId, info.ParticipantName, info.ParticipantRole);


            await hubContext.Clients.Group(groupName).SendAsync("OnJoinRequestReceived", info.RequestId,
                info.ParticipantName, info.ParticipantRole);
            return Unit.Default;
        }
        catch (Exception ex)
        {
            return Error.New(ex);
        }
    }
}

public record RoomJoinRequestPayload(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("role")] string Role
);

public record RoomJoinRequestResponse(
    [property: JsonPropertyName("requestId")]
    Guid RequestId,
    [property: JsonPropertyName("roomId")] Guid RoomId,
    [property: JsonPropertyName("participantName")]
    string ParticipantName,
    [property: JsonPropertyName("participantRole")]
    string ParticipantRole
);