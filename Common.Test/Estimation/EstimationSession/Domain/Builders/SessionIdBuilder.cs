using Common.Estimation.EstimationSession.Domain.Models;

namespace Common.Test.Estimation.EstimationSession.Domain.Builders;

public class SessionIdBuilder
{
    private Guid _value = Guid.NewGuid();

    public SessionId Build()
    {
        return SessionId.Create(_value).Match(
            s => s,
            error => throw new InvalidOperationException($"Failed to build SessionId: {error.Message}")
        );
    }

    public SessionIdBuilder WithValue(Guid value)
    {
        _value = value;
        return this;
    }
}