using LanguageExt;
using LanguageExt.Common;

namespace Common.Estimation.EstimationSession.Application.Contracts;

public interface IStartEstimationSessionUseCase
{
    Task<Either<Error, EstimationSessionDto>> Execute(StartEstimationSessionCommand command);
}

public record StartEstimationSessionCommand(Guid RoomId, string StoryDescription);

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

public record ParticipantVoteDto(string Name, string? Card);