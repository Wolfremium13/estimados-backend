using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using static Common.Estimation.RoomAccess.Domain.Errors.RoomAccessErrors;

namespace Wolfremium.Estimados.Controllers;

public static class RoomAccessErrorsWeb
{
    public static ProblemDetails MapToProblemDetails(Error error, HttpContext context)
    {
        var exception = error.Exception.Case;

        var statusCode = exception switch
        {
            ClientValidationException => StatusCodes.Status400BadRequest,
            InvalidRoleException => StatusCodes.Status400BadRequest,
            RoomClosedException => StatusCodes.Status400BadRequest,
            RoomNotFoundException => StatusCodes.Status404NotFound,
            AccessDeniedException => StatusCodes.Status403Forbidden,
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