using System;
using System.Collections.Generic;

namespace Wolfremium.Estimados.WebApp.Models;

public record ParticipantDto(string Name, string Role);

public record ParticipantVoteDto(string Name, string? Card);

public record EstimationSessionDto(
    Guid SessionId,
    Guid RoomId,
    string StoryDescription,
    string CurrentState,
    string? ConsensusValue,
    bool HasDiscrepancy,
    IReadOnlyCollection<string> FlaggedSpecialCards,
    IReadOnlyCollection<ParticipantVoteDto> Votes
);

public record PendingRequestModel(Guid RequestId, string Name, string Role);
