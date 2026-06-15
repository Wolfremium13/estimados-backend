using Common.Estimation.EstimationSession.Application.Contracts;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Wolfremium.Estimados.Hubs;
using static Wolfremium.Estimados.Controllers.EstimationSessionErrorsWeb;

namespace Wolfremium.Estimados.Controllers.V1.EstimationSession;

[ApiController]
[Route("v1/rooms/{roomId:guid}/session/reveal")]
[Tags("Estimation Session")]
public class EstimationSessionRevealController(
    IRevealVotesUseCase revealVotesUseCase,
    IHubContext<RoomHub> hubContext
) : ControllerBase
{
    [HttpPost]
    [Produces("application/json")]
    [EndpointSummary("Reveal votes in an estimation session")]
    [EndpointDescription("Triggers the simultaneous reveal of all votes in the session.")]
    [ProducesResponseType(typeof(EstimationSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IResult> Reveal([FromRoute] Guid roomId)
    {
        return await (
            from dto in revealVotesUseCase.Execute(new RevealVotesCommand(roomId)).ToAsync()
            from notify in NotifyReveal(roomId, dto).ToAsync()
            select dto
        ).Match<IResult>(
            success => Results.Ok(success),
            error => Results.Problem(MapToProblemDetails(error, HttpContext))
        );
    }

    private async Task<Either<Error, Unit>> NotifyReveal(Guid roomId, EstimationSessionDto dto)
    {
        try
        {
            var group = hubContext.Clients.Group($"room_{roomId}");
            if (dto.CurrentState == "Halted")
            {
                await group.SendAsync("OnSessionHalted", "The story is too complex and must be split.");
            }
            else
            {
                await group.SendAsync("OnVotesRevealed", dto);
            }

            return Unit.Default;
        }
        catch (Exception ex)
        {
            return Error.New(ex);
        }
    }
}