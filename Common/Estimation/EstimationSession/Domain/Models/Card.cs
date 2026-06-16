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
    public const string Axe = "Axe";
    private const string Diagram = "Diagram";
    private const string AI = "AI";
    private const string CoffeeCup = "Coffee Cup";

    private static readonly System.Collections.Generic.HashSet<string> ValidCards =
        new(StringComparer.OrdinalIgnoreCase)
        {
            One, Two, Three, Five, Eight, Axe, Diagram, AI, CoffeeCup
        };

    private Card(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public bool IsNumeric => Value is One or Two or Three or Five or Eight;
    public bool IsSpecial => !IsNumeric;
    public bool IsAxe => Value == Axe;
    public bool IsDiagram => Value == Diagram;
    public bool IsAI => Value == AI;
    public bool IsCoffeeCup => Value == CoffeeCup;

    public static Either<Error, Card> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Either<Error, Card>.Left(Error.New(new ClientValidationException("Card value is required.")));

        var trimmed = value.Trim();

        if (string.Equals(trimmed, "Hacha", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(trimmed, "Axe", StringComparison.OrdinalIgnoreCase))
        {
            return Either<Error, Card>.Right(new Card(Axe));
        }

        if (string.Equals(trimmed, "Diagrama", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(trimmed, "Diagram", StringComparison.OrdinalIgnoreCase))
        {
            return Either<Error, Card>.Right(new Card(Diagram));
        }

        if (string.Equals(trimmed, "IA", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(trimmed, "AI", StringComparison.OrdinalIgnoreCase))
        {
            return Either<Error, Card>.Right(new Card(AI));
        }

        if (string.Equals(trimmed, "Taza de Café", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(trimmed, "Taza de Cafe", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(trimmed, "Café", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(trimmed, "Cafe", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(trimmed, "Coffee Cup", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(trimmed, "Coffee", StringComparison.OrdinalIgnoreCase))
        {
            return Either<Error, Card>.Right(new Card(CoffeeCup));
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