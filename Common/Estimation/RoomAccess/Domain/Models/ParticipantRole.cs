using LanguageExt;
using LanguageExt.Common;
using static Common.Estimation.RoomAccess.Domain.Errors.RoomAccessErrors;

namespace Common.Estimation.RoomAccess.Domain.Models;

public record ParticipantRole
{
    public const string Moderador = "Moderador";
    public const string Developer = "Developer";
    public const string ProductOwner = "Product Owner";

    private static readonly System.Collections.Generic.HashSet<string> ValidRoles =
        new(StringComparer.OrdinalIgnoreCase)
        {
            Moderador, Developer, ProductOwner
        };

    private ParticipantRole(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public bool IsModerator => Value == Moderador;

    public static Either<Error, ParticipantRole> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Either<Error, ParticipantRole>.Left(
                Error.New(new ClientValidationException("Role is required and cannot be empty.")));

        var trimmedValue = value.Trim();
        if (!ValidRoles.Contains(trimmedValue))
            return Either<Error, ParticipantRole>.Left(Error.New(
                new InvalidRoleException($"Role must be one of: {Moderador}, {Developer}, {ProductOwner}.")));

        var normalized = string.Equals(trimmedValue, Moderador, StringComparison.OrdinalIgnoreCase) ? Moderador :
            string.Equals(trimmedValue, Developer, StringComparison.OrdinalIgnoreCase) ? Developer : ProductOwner;

        return Either<Error, ParticipantRole>.Right(new ParticipantRole(normalized));
    }

    public override string ToString()
    {
        return Value;
    }
}