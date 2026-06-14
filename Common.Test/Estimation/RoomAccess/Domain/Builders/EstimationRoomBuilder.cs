using Common.Estimation.RoomAccess.Domain.Models;

namespace Common.Test.Estimation.RoomAccess.Domain.Builders;

public class EstimationRoomBuilder
{
    private readonly List<JoinRequest> _joinRequests = new();
    private Guid _id = Guid.NewGuid();
    private bool _isActive = true;
    private string _moderatorName = "Carlos";

    public EstimationRoom Build()
    {
        var roomId = RoomId.Create(_id).Match(r => r, error => throw new InvalidOperationException(error.Message));
        var name = ParticipantName.Create(_moderatorName)
            .Match(n => n, error => throw new InvalidOperationException(error.Message));

        var room = EstimationRoom.Create(roomId, name).Match(
            r => r,
            error => throw new InvalidOperationException($"Failed to build EstimationRoom: {error.Message}")
        );

        foreach (var req in _joinRequests) room.AddJoinRequest(req);

        if (!_isActive) room.Close();

        return room;
    }

    public EstimationRoomBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public EstimationRoomBuilder WithModeratorName(string name)
    {
        _moderatorName = name;
        return this;
    }

    public EstimationRoomBuilder WithJoinRequest(JoinRequest request)
    {
        _joinRequests.Add(request);
        return this;
    }

    public EstimationRoomBuilder AsClosed()
    {
        _isActive = false;
        return this;
    }
}