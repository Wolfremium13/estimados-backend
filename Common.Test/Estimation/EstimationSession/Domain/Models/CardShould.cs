using Common.Estimation.EstimationSession.Domain.Models;
using Shouldly;
using Xunit;
using static Common.Estimation.EstimationSession.Domain.Errors.EstimationSessionErrors;

namespace Common.Test.Estimation.EstimationSession.Domain.Models;

public class CardShould
{
    [Theory]
    [InlineData("1", true)]
    [InlineData("2", true)]
    [InlineData("3", true)]
    [InlineData("5", true)]
    [InlineData("8", true)]
    [InlineData("Hacha", false)]
    [InlineData("Diagrama", false)]
    [InlineData("IA", false)]
    [InlineData("Taza de Café", false)]
    public void BeCreatedForValidValues(string value, bool isNumeric)
    {
        var result = Card.Create(value);

        result.IsRight.ShouldBeTrue();
        result.IfRight(card =>
        {
            card.Value.ShouldBe(value);
            card.IsNumeric.ShouldBe(isNumeric);
            card.IsSpecial.ShouldBe(!isNumeric);
        });
    }

    [Fact]
    public void FailForInvalidValues()
    {
        var result = Card.Create("13");

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error => error.ToException().ShouldBeOfType<InvalidCardException>());
    }

    [Theory]
    [InlineData("hacha", "Hacha")]
    [InlineData("diagrama", "Diagrama")]
    [InlineData("ia", "IA")]
    [InlineData("TAZA DE CAFÉ", "Taza de Café")]
    public void BeCaseInsensitiveAndNormalize(string input, string expectedNormalized)
    {
        var result = Card.Create(input);

        result.IsRight.ShouldBeTrue();
        result.IfRight(card => card.Value.ShouldBe(expectedNormalized));
    }

    [Theory]
    [InlineData("Taza de Cafe")]
    [InlineData("taza de cafe")]
    [InlineData("Cafe")]
    [InlineData("Café")]
    public void NormalizeCoffeeCupWithoutAccent(string input)
    {
        var result = Card.Create(input);

        result.IsRight.ShouldBeTrue();
        result.IfRight(card => card.Value.ShouldBe("Taza de Café"));
    }

    [Fact]
    public void FailWhenValueIsEmpty()
    {
        var result = Card.Create("");

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error => error.ToException().ShouldBeOfType<ClientValidationException>());
    }

    [Fact]
    public void IdentifyPropertiesCorrectly()
    {
        var hacha = Card.Create("Hacha").Match(c => c, _ => throw new Exception());
        var diagrama = Card.Create("Diagrama").Match(c => c, _ => throw new Exception());
        var ia = Card.Create("IA").Match(c => c, _ => throw new Exception());
        var cafe = Card.Create("Taza de Café").Match(c => c, _ => throw new Exception());

        hacha.IsHacha.ShouldBeTrue();
        diagrama.IsDiagrama.ShouldBeTrue();
        ia.IsIA.ShouldBeTrue();
        cafe.IsTazaDeCafe.ShouldBeTrue();
    }
}