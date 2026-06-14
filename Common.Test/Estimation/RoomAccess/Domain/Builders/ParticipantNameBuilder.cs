using Common.Estimation.RoomAccess.Domain.Models;

namespace Common.Test.Estimation.RoomAccess.Domain.Builders;

public class ParticipantNameBuilder
{
    private string _value = "Carlos";

    public ParticipantName Build()
    {
        return ParticipantName.Create(_value).Match(
            name => name,
            error => throw new InvalidOperationException($"Failed to build ParticipantName: {error.Message}")
        );
    }

    public ParticipantNameBuilder WithValue(string value)
    {
        _value = value;
        return this;
    }
}