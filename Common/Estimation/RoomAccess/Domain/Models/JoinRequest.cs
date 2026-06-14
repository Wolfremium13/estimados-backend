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
    public RequestId Id { get; }
    public ParticipantName Name { get; }
    public ParticipantRole Role { get; }
    public JoinRequestStatus Status { get; private set; }

    private JoinRequest(RequestId id, ParticipantName name, ParticipantRole role, JoinRequestStatus status)
    {
        Id = id;
        Name = name;
        Role = role;
        Status = status;
    }

    public static Either<Error, JoinRequest> Create(RequestId id, ParticipantName name, ParticipantRole role) =>
        new JoinRequest(id, name, role, JoinRequestStatus.Pending);

    public void Approve() => Status = JoinRequestStatus.Approved;

    public void Reject() => Status = JoinRequestStatus.Rejected;
}
