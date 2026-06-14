using LanguageExt;
using LanguageExt.Common;
using static Common.Estimation.RoomAccess.Domain.Errors.RoomAccessErrors;

namespace Common.Estimation.RoomAccess.Domain.Models;

public record Participant(ParticipantName Name, ParticipantRole Role);

public class EstimationRoom
{
    private readonly List<Participant> _activeParticipants = new();
    private readonly List<JoinRequest> _joinRequests = new();

    private EstimationRoom(RoomId id, ParticipantName moderatorName, bool isActive)
    {
        Id = id;
        ModeratorName = moderatorName;
        IsActive = isActive;

        var moderatorRole = ParticipantRole.Create(ParticipantRole.Moderador).Match(
            success => success,
            error => throw new InvalidOperationException($"Failed to create moderator role: {error.Message}")
        );
        _activeParticipants.Add(new Participant(moderatorName, moderatorRole));
    }

    public RoomId Id { get; }
    public ParticipantName ModeratorName { get; }
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<JoinRequest> JoinRequests => _joinRequests.AsReadOnly();
    public IReadOnlyCollection<Participant> ActiveParticipants => _activeParticipants.AsReadOnly();

    public static Either<Error, EstimationRoom> Create(RoomId id, ParticipantName moderatorName)
    {
        return new EstimationRoom(id, moderatorName, isActive: true);
    }

    public Either<Error, Unit> AddJoinRequest(JoinRequest request)
    {
        if (!IsActive)
        {
            return Either<Error, Unit>.Left(
                Error.New(new RoomClosedException("Cannot add join request. The room is closed.")));
        }

        if (_joinRequests.Any(r => r.Id == request.Id))
        {
            return Either<Error, Unit>.Left(Error.New(new ClientValidationException("Join request already exists.")));
        }

        _joinRequests.Add(request);
        return Either<Error, Unit>.Right(Unit.Default);
    }

    public Either<Error, Unit> ApproveJoinRequest(RequestId requestId)
    {
        if (!IsActive)
        {
            return Either<Error, Unit>.Left(
                Error.New(new RoomClosedException("Cannot approve request. The room is closed.")));
        }

        var request = _joinRequests.FirstOrDefault(r => r.Id == requestId);
        if (request == null)
        {
            return Either<Error, Unit>.Left(Error.New(new RoomNotFoundException("Join request not found.")));
        }

        request.Approve();
        _activeParticipants.Add(new Participant(request.Name, request.Role));
        return Either<Error, Unit>.Right(Unit.Default);
    }

    public Either<Error, Unit> RejectJoinRequest(RequestId requestId)
    {
        if (!IsActive)
        {
            return Either<Error, Unit>.Left(
                Error.New(new RoomClosedException("Cannot reject request. The room is closed.")));
        }

        var request = _joinRequests.FirstOrDefault(r => r.Id == requestId);
        if (request == null)
        {
            return Either<Error, Unit>.Left(Error.New(new RoomNotFoundException("Join request not found.")));
        }

        request.Reject();
        return Either<Error, Unit>.Right(Unit.Default);
    }

    public Either<Error, Unit> Close()
    {
        if (!IsActive)
        {
            return Either<Error, Unit>.Right(Unit.Default);
        }

        IsActive = false;

        foreach (var request in _joinRequests.Where(r => r.Status == JoinRequestStatus.Pending))
        {
            request.Reject();
        }

        return Either<Error, Unit>.Right(Unit.Default);
    }
}