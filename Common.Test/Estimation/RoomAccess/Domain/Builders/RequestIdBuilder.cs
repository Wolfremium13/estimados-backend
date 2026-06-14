using System;
using Common.Estimation.RoomAccess.Domain.Models;

namespace Common.Test.Estimation.RoomAccess.Domain.Builders;

public class RequestIdBuilder
{
    private Guid _value = Guid.NewGuid();

    public RequestId Build()
    {
        return RequestId.Create(_value).Match(
            id => id,
            error => throw new InvalidOperationException($"Failed to build RequestId: {error.Message}")
        );
    }

    public RequestIdBuilder WithValue(Guid value)
    {
        _value = value;
        return this;
    }
}
