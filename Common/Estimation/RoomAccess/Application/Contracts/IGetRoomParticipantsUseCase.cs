using LanguageExt;
using LanguageExt.Common;

namespace Common.Estimation.RoomAccess.Application.Contracts;

public record GetRoomParticipantsQuery(Guid RoomId);

public record ParticipantDto(string Name, string Role);

public interface IGetRoomParticipantsUseCase
{
    Task<Either<Error, IReadOnlyCollection<ParticipantDto>>> Execute(GetRoomParticipantsQuery query);
}