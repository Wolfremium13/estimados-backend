using LanguageExt;
using LanguageExt.Common;
using static Common.Estimation.EstimationSession.Domain.Errors.EstimationSessionErrors;

namespace Common.Estimation.EstimationSession.Domain.Models;

public record SessionId
{
    private SessionId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static Either<Error, SessionId> Create(Guid value)
    {
        return value == Guid.Empty
            ? Either<Error, SessionId>.Left(Error.New(new ClientValidationException("Session ID cannot be empty.")))
            : Either<Error, SessionId>.Right(new SessionId(value));
    }

    public static Either<Error, SessionId> Create(string value)
    {
        return Guid.TryParse(value, out var guid)
            ? Create(guid)
            : Either<Error, SessionId>.Left(
                Error.New(new ClientValidationException("Session ID must be a valid GUID.")));
    }

    public override string ToString() => Value.ToString();
}