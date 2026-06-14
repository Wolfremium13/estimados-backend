using System;
using Common.Estimation.RoomAccess.Domain.Models;

namespace Common.Test.Estimation.RoomAccess.Domain.Builders;

public class ParticipantRoleBuilder
{
    private string _value = ParticipantRole.Developer;

    public ParticipantRole Build()
    {
        return ParticipantRole.Create(_value).Match(
            role => role,
            error => throw new InvalidOperationException($"Failed to build ParticipantRole: {error.Message}")
        );
    }

    public ParticipantRoleBuilder WithValue(string value)
    {
        _value = value;
        return this;
    }
}
