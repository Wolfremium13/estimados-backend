using Common.Estimation.EstimationSession.Domain.Models;

namespace Common.Test.Estimation.EstimationSession.Domain.Builders;

public class CardBuilder
{
    private string _value = "5";

    public Card Build()
    {
        return Card.Create(_value).Match(
            c => c,
            error => throw new InvalidOperationException($"Failed to build Card: {error.Message}")
        );
    }

    public CardBuilder WithValue(string value)
    {
        _value = value;
        return this;
    }
}