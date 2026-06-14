using System;
using Xunit;
using Shouldly;
using Common.Estimation.RoomAccess.Domain.Models;
using Common.Test.Estimation.RoomAccess.Domain.Builders;
using static Common.Estimation.RoomAccess.Domain.Errors.RoomAccessErrors;

namespace Common.Test.Estimation.RoomAccess.Domain.Models;

public class EstimationRoomShould
{
    [Fact]
    public void BeCreatedCorrectlyWithModeratorAsActiveParticipant()
    {
        var roomId = new RoomIdBuilder().Build();
        var moderatorName = new ParticipantNameBuilder().WithValue("Carlos").Build();

        var result = EstimationRoom.Create(roomId, moderatorName);

        result.IsRight.ShouldBeTrue();
        result.IfRight(success =>
        {
            success.Id.ShouldBe(roomId);
            success.ModeratorName.ShouldBe(moderatorName);
            success.IsActive.ShouldBeTrue();
            success.ActiveParticipants.ShouldContain(p => p.Name.Value == "Carlos" && p.Role.Value == ParticipantRole.Moderador);
        });
    }

    [Fact]
    public void AddJoinRequestSuccessfullyWhenActive()
    {
        var room = new EstimationRoomBuilder().Build();
        var request = new JoinRequestBuilder().Build();

        var result = room.AddJoinRequest(request);

        result.IsRight.ShouldBeTrue();
        room.JoinRequests.ShouldContain(request);
    }

    [Fact]
    public void FailAddingJoinRequestWhenClosed()
    {
        var room = new EstimationRoomBuilder().AsClosed().Build();
        var request = new JoinRequestBuilder().Build();

        var result = room.AddJoinRequest(request);

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error => error.ToException().ShouldBeOfType<RoomClosedException>());
        room.JoinRequests.ShouldNotContain(request);
    }

    [Fact]
    public void FailAddingDuplicateJoinRequest()
    {
        var requestId = Guid.NewGuid();
        var request1 = new JoinRequestBuilder().WithId(requestId).WithName("Ana").Build();
        var request2 = new JoinRequestBuilder().WithId(requestId).WithName("Bob").Build();
        var room = new EstimationRoomBuilder().WithJoinRequest(request1).Build();

        var result = room.AddJoinRequest(request2);

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error => error.ToException().ShouldBeOfType<ClientValidationException>());
    }

    [Fact]
    public void ApproveJoinRequestAndAddParticipantCorrectly()
    {
        var requestId = Guid.NewGuid();
        var request = new JoinRequestBuilder().WithId(requestId).WithName("Ana").WithRole(ParticipantRole.Developer).Build();
        var room = new EstimationRoomBuilder().WithJoinRequest(request).Build();

        var result = room.ApproveJoinRequest(new RequestIdBuilder().WithValue(requestId).Build());

        result.IsRight.ShouldBeTrue();
        request.Status.ShouldBe(JoinRequestStatus.Approved);
        room.ActiveParticipants.ShouldContain(p => p.Name.Value == "Ana" && p.Role.Value == ParticipantRole.Developer);
    }

    [Fact]
    public void FailApprovingRequestWhenClosed()
    {
        var requestId = Guid.NewGuid();
        var request = new JoinRequestBuilder().WithId(requestId).Build();
        var room = new EstimationRoomBuilder().WithJoinRequest(request).AsClosed().Build();

        var result = room.ApproveJoinRequest(new RequestIdBuilder().WithValue(requestId).Build());

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error => error.ToException().ShouldBeOfType<RoomClosedException>());
    }

    [Fact]
    public void FailApprovingNonExistentRequest()
    {
        var room = new EstimationRoomBuilder().Build();

        var result = room.ApproveJoinRequest(new RequestIdBuilder().Build());

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error => error.ToException().ShouldBeOfType<RoomNotFoundException>());
    }

    [Fact]
    public void RejectJoinRequestCorrectly()
    {
        var requestId = Guid.NewGuid();
        var request = new JoinRequestBuilder().WithId(requestId).WithName("Ana").Build();
        var room = new EstimationRoomBuilder().WithJoinRequest(request).Build();

        var result = room.RejectJoinRequest(new RequestIdBuilder().WithValue(requestId).Build());

        result.IsRight.ShouldBeTrue();
        request.Status.ShouldBe(JoinRequestStatus.Rejected);
        room.ActiveParticipants.ShouldNotContain(p => p.Name.Value == "Ana");
    }

    [Fact]
    public void FailRejectingRequestWhenClosed()
    {
        var requestId = Guid.NewGuid();
        var request = new JoinRequestBuilder().WithId(requestId).Build();
        var room = new EstimationRoomBuilder().WithJoinRequest(request).AsClosed().Build();

        var result = room.RejectJoinRequest(new RequestIdBuilder().WithValue(requestId).Build());

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error => error.ToException().ShouldBeOfType<RoomClosedException>());
    }

    [Fact]
    public void FailRejectingNonExistentRequest()
    {
        var room = new EstimationRoomBuilder().Build();

        var result = room.RejectJoinRequest(new RequestIdBuilder().Build());

        result.IsLeft.ShouldBeTrue();
        result.IfLeft(error => error.ToException().ShouldBeOfType<RoomNotFoundException>());
    }

    [Fact]
    public void CloseRoomAndRejectAllPendingRequests()
    {
        var requestApproved = new JoinRequestBuilder().WithName("Ana").Build();
        var requestPending = new JoinRequestBuilder().WithName("Bob").Build();
        requestApproved.Approve();
        
        var room = new EstimationRoomBuilder()
            .WithJoinRequest(requestApproved)
            .WithJoinRequest(requestPending)
            .Build();

        var result = room.Close();

        result.IsRight.ShouldBeTrue();
        room.IsActive.ShouldBeFalse();
        requestApproved.Status.ShouldBe(JoinRequestStatus.Approved);
        requestPending.Status.ShouldBe(JoinRequestStatus.Rejected);
    }
}
