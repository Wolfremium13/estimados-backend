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
    [InlineData("Axe", false)]
    [InlineData("Diagram", false)]
    [InlineData("AI", false)]
    [InlineData("Coffee Cup", false)]
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
    [InlineData("hacha", "Axe")]
    [InlineData("diagrama", "Diagram")]
    [InlineData("ia", "AI")]
    [InlineData("TAZA DE CAFÉ", "Coffee Cup")]
    [InlineData("axe", "Axe")]
    [InlineData("diagram", "Diagram")]
    [InlineData("ai", "AI")]
    [InlineData("coffee cup", "Coffee Cup")]
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
    [InlineData("coffee")]
    public void NormalizeCoffeeCupWithoutAccent(string input)
    {
        var result = Card.Create(input);

        result.IsRight.ShouldBeTrue();
        result.IfRight(card => card.Value.ShouldBe("Coffee Cup"));
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
        var axe = Card.Create("Axe").Match(c => c, _ => throw new Exception());
        var diagram = Card.Create("Diagram").Match(c => c, _ => throw new Exception());
        var ai = Card.Create("AI").Match(c => c, _ => throw new Exception());
        var coffee = Card.Create("Coffee Cup").Match(c => c, _ => throw new Exception());

        axe.IsAxe.ShouldBeTrue();
        diagram.IsDiagram.ShouldBeTrue();
        ai.IsAI.ShouldBeTrue();
        coffee.IsCoffeeCup.ShouldBeTrue();
    }
}