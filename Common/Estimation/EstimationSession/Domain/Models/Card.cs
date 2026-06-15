using LanguageExt;
using LanguageExt.Common;
using static Common.Estimation.EstimationSession.Domain.Errors.EstimationSessionErrors;

namespace Common.Estimation.EstimationSession.Domain.Models;

public record Card
{
    private const string One = "1";
    private const string Two = "2";
    private const string Three = "3";
    private const string Five = "5";
    private const string Eight = "8";
    public const string Hacha = "Hacha";
    private const string Diagrama = "Diagrama";
    private const string IA = "IA";
    private const string TazaDeCafe = "Taza de Café";

    private static readonly System.Collections.Generic.HashSet<string> ValidCards =
        new(StringComparer.OrdinalIgnoreCase)
        {
            One, Two, Three, Five, Eight, Hacha, Diagrama, IA, TazaDeCafe
        };

    private Card(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public bool IsNumeric => Value is One or Two or Three or Five or Eight;
    public bool IsSpecial => !IsNumeric;
    public bool IsHacha => Value == Hacha;
    public bool IsDiagrama => Value == Diagrama;
    public bool IsIA => Value == IA;
    public bool IsTazaDeCafe => Value == TazaDeCafe;

    public static Either<Error, Card> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Either<Error, Card>.Left(Error.New(new ClientValidationException("Card value is required.")));

        var trimmed = value.Trim();

        if (string.Equals(trimmed, "Taza de Cafe", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(trimmed, "Café", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(trimmed, "Cafe", StringComparison.OrdinalIgnoreCase))
        {
            return Either<Error, Card>.Right(new Card(TazaDeCafe));
        }

        string? matchedValue = null;
        foreach (var valid in ValidCards)
        {
            if (string.Equals(valid, trimmed, StringComparison.OrdinalIgnoreCase))
            {
                matchedValue = valid;
                break;
            }
        }

        if (matchedValue == null)
            return Either<Error, Card>.Left(
                Error.New(new InvalidCardException($"Card value '{trimmed}' is not valid.")));

        return Either<Error, Card>.Right(new Card(matchedValue));
    }

    public override string ToString() => Value;
}