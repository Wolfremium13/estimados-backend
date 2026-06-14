using Common.Estimation.RoomAccess.Domain.Models;

namespace Common.Test.Estimation.RoomAccess.Domain.Builders;

public class RoomIdBuilder
{
    private Guid _value = Guid.NewGuid();

    public RoomId Build()
    {
        return RoomId.Create(_value).Match(
            id => id,
            error => throw new InvalidOperationException($"Failed to build RoomId: {error.Message}")
        );
    }

    public RoomIdBuilder WithValue(Guid value)
    {
        _value = value;
        return this;
    }
}