using System;
using LanguageExt;
using LanguageExt.Common;
using static Common.Estimation.RoomAccess.Domain.Errors.RoomAccessErrors;

namespace Common.Estimation.RoomAccess.Domain.Models;

public record RequestId
{
    public Guid Value { get; }

    private RequestId(Guid value) => Value = value;

    public static Either<Error, RequestId> Create(Guid value) =>
        value == Guid.Empty
            ? Either<Error, RequestId>.Left(Error.New(new ClientValidationException("Request ID cannot be empty.")))
            : Either<Error, RequestId>.Right(new RequestId(value));

    public static Either<Error, RequestId> Create(string value) =>
        Guid.TryParse(value, out var guid)
            ? Create(guid)
            : Either<Error, RequestId>.Left(Error.New(new ClientValidationException("Request ID must be a valid GUID.")));

    public override string ToString() => Value.ToString();
}
