using Common.Estimation.RoomAccess.Domain.Models;
using LanguageExt;

namespace Common.Estimation.EstimationSession.Domain.Models;

public record ParticipantVote(ParticipantName Name, Option<Card> Card);