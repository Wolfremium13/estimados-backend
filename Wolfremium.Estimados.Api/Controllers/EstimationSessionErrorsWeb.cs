using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using static Common.Estimation.EstimationSession.Domain.Errors.EstimationSessionErrors;

namespace Wolfremium.Estimados.Controllers;

public static class EstimationSessionErrorsWeb
{
    public static ProblemDetails MapToProblemDetails(Error error, HttpContext context)
    {
        var exception = error.Exception.Case;

        var statusCode = exception switch
        {
            ClientValidationException => StatusCodes.Status400BadRequest,
            InvalidStateTransitionException => StatusCodes.Status400BadRequest,
            OnlyDevelopersCanVoteException => StatusCodes.Status400BadRequest,
            NoVotesCastException => StatusCodes.Status400BadRequest,
            InvalidCardException => StatusCodes.Status400BadRequest,
            SessionNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        return new ProblemDetails
        {
            Status = statusCode,
            Title = exception?.GetType().Name ?? "Error",
            Detail = error.Message,
            Instance = context.Request.Path
        };
    }
}