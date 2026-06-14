using System;
using Common.Estimation.RoomAccess.Domain.Models;

namespace Common.Test.Estimation.RoomAccess.Domain.Builders;

public class JoinRequestBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _name = "Ana";
    private string _role = ParticipantRole.Developer;

    public JoinRequest Build()
    {
        var id = RequestId.Create(_id).Match(r => r, error => throw new InvalidOperationException(error.Message));
        var name = ParticipantName.Create(_name).Match(n => n, error => throw new InvalidOperationException(error.Message));
        var role = ParticipantRole.Create(_role).Match(r => r, error => throw new InvalidOperationException(error.Message));

        return JoinRequest.Create(id, name, role).Match(
            request => request,
            error => throw new InvalidOperationException($"Failed to build JoinRequest: {error.Message}")
        );
    }

    public JoinRequestBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public JoinRequestBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public JoinRequestBuilder WithRole(string role)
    {
        _role = role;
        return this;
    }
}
