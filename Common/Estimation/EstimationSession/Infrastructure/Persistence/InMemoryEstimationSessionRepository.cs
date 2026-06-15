using System.Collections.Concurrent;
using Common.Estimation.EstimationSession.Domain.Models;
using Common.Estimation.EstimationSession.Domain.Ports;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using static Common.Estimation.EstimationSession.Domain.Errors.EstimationSessionErrors;

namespace Common.Estimation.EstimationSession.Infrastructure.Persistence;

public class InMemoryEstimationSessionRepository(
    ILogger<InMemoryEstimationSessionRepository> logger
) : IEstimationSessionRepository
{
    private static readonly ConcurrentDictionary<Guid, Domain.Models.EstimationSession> Sessions = new();

    public Task<Either<Error, Domain.Models.EstimationSession>> FindById(SessionId sessionId)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("In-memory repository: Finding session {SessionId}...", sessionId.Value);
        }

        if (Sessions.TryGetValue(sessionId.Value, out var session))
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "In-memory repository: Found session {SessionId}. State: {State}.",
                    session.Id.Value, session.CurrentState);
            }

            return Task.FromResult<Either<Error, Domain.Models.EstimationSession>>(session);
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("In-memory repository: Session {SessionId} was not found.", sessionId.Value);
        }

        return Task.FromResult<Either<Error, Domain.Models.EstimationSession>>(
            Error.New(new SessionNotFoundException($"Session with ID {sessionId.Value} was not found."))
        );
    }

    public Task<Either<Error, Unit>> Save(Domain.Models.EstimationSession session)
    {
        Sessions[session.Id.Value] = session;

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("In-memory repository: Saved session {SessionId}. State: {State}.",
                session.Id.Value, session.CurrentState);
        }

        return Task.FromResult<Either<Error, Unit>>(Unit.Default);
    }

    public static void Clear()
    {
        Sessions.Clear();
    }
}