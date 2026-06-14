using LanguageExt;
using LanguageExt.Common;
using static Common.Estimation.RoomAccess.Domain.Errors.RoomAccessErrors;

namespace Common.Estimation.RoomAccess.Domain.Models;

public record RoomId
{
    private RoomId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static Either<Error, RoomId> Create(Guid value)
    {
        return value == Guid.Empty
            ? Either<Error, RoomId>.Left(Error.New(new ClientValidationException("Room ID cannot be empty.")))
            : Either<Error, RoomId>.Right(new RoomId(value));
    }

    public static Either<Error, RoomId> Create(string value)
    {
        return Guid.TryParse(value, out var guid)
            ? Create(guid)
            : Either<Error, RoomId>.Left(Error.New(new ClientValidationException("Room ID must be a valid GUID.")));
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}