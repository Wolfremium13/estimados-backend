using System.Text.Json.Serialization;
using Common.Estimation.EstimationSession.Application.Contracts;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Wolfremium.Estimados.Hubs;
using static Wolfremium.Estimados.Controllers.EstimationSessionErrorsWeb;

namespace Wolfremium.Estimados.Controllers.V1.EstimationSession;

[ApiController]
[Route("v1/rooms/{roomId:guid}/session/vote")]
[Tags("Estimation Session")]
public class EstimationSessionVoteController(
    ICastVoteUseCase castVoteUseCase,
    IHubContext<RoomHub> hubContext
) : ControllerBase
{
    [HttpPost]
    [Produces("application/json")]
    [EndpointSummary("Cast a vote in an estimation session")]
    [EndpointDescription("Registers a developer's secret vote in the estimation session.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IResult> Vote([FromRoute] Guid roomId, [FromBody] VotePayload payload)
    {
        return await (
            from _ in castVoteUseCase.Execute(new CastVoteCommand(roomId, payload.ParticipantName, payload.CardValue))
                .ToAsync()
            from notify in NotifyVoteCast(roomId, payload.ParticipantName).ToAsync()
            select Results.Ok()
        ).Match<IResult>(
            success => success,
            error => Results.Problem(MapToProblemDetails(error, HttpContext))
        );
    }

    private async Task<Either<Error, Unit>> NotifyVoteCast(Guid roomId, string participantName)
    {
        try
        {
            await hubContext.Clients.Group($"room_{roomId}").SendAsync("OnVoteCast", participantName);
            return Unit.Default;
        }
        catch (Exception ex)
        {
            return Error.New(ex);
        }
    }
}

public record VotePayload(
    [property: JsonPropertyName("participantName")]
    string ParticipantName,
    [property: JsonPropertyName("cardValue")]
    string CardValue
);