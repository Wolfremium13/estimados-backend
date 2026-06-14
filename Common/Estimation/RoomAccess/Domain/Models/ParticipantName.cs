using LanguageExt;
using LanguageExt.Common;
using static Common.Estimation.RoomAccess.Domain.Errors.RoomAccessErrors;

namespace Common.Estimation.RoomAccess.Domain.Models;

public record ParticipantName
{
    private ParticipantName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Either<Error, ParticipantName> Create(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? Either<Error, ParticipantName>.Left(
                Error.New(new ClientValidationException("Name is required and cannot be empty.")))
            : Either<Error, ParticipantName>.Right(new ParticipantName(value.Trim()));
    }

    public override string ToString()
    {
        return Value;
    }
}