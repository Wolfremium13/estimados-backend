using Xunit;
using Shouldly;
using Common.Estimation.RoomAccess.Domain.Models;
using Common.Test.Estimation.RoomAccess.Domain.Builders;

namespace Common.Test.Estimation.RoomAccess.Domain.Models;

public class JoinRequestShould
{
    [Fact]
    public void BeCreatedWithPendingStatus()
    {
        var id = new RequestIdBuilder().Build();
        var name = new ParticipantNameBuilder().Build();
        var role = new ParticipantRoleBuilder().Build();

        var result = JoinRequest.Create(id, name, role);

        result.IsRight.ShouldBeTrue();
        result.IfRight(success =>
        {
            success.Id.ShouldBe(id);
            success.Name.ShouldBe(name);
            success.Role.ShouldBe(role);
            success.Status.ShouldBe(JoinRequestStatus.Pending);
        });
    }

    [Fact]
    public void TransitionToApprovedCorrectly()
    {
        var request = new JoinRequestBuilder().Build();

        request.Approve();

        request.Status.ShouldBe(JoinRequestStatus.Approved);
    }

    [Fact]
    public void TransitionToRejectedCorrectly()
    {
        var request = new JoinRequestBuilder().Build();

        request.Reject();

        request.Status.ShouldBe(JoinRequestStatus.Rejected);
    }
}
