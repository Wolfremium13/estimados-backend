using LanguageExt;
using LanguageExt.Common;

namespace Common.Estimation.RoomAccess.Domain.Models;

public enum JoinRequestStatus
{
    Pending,
    Approved,
    Rejected
}

public class JoinRequest
{
    private JoinRequest(RequestId id, ParticipantName name, ParticipantRole role, JoinRequestStatus status)
    {
        Id = id;
        Name = name;
        Role = role;
        Status = status;
    }

    public RequestId Id { get; }
    public ParticipantName Name { get; }
    public ParticipantRole Role { get; }
    public JoinRequestStatus Status { get; private set; }

    public static Either<Error, JoinRequest> Create(RequestId id, ParticipantName name, ParticipantRole role)
    {
        return new JoinRequest(id, name, role, JoinRequestStatus.Pending);
    }

    public void Approve()
    {
        Status = JoinRequestStatus.Approved;
    }

    public void Reject()
    {
        Status = JoinRequestStatus.Rejected;
    }
}