using LanguageExt;
using LanguageExt.Common;

namespace Common.Estimation.RoomAccess.Application.Contracts;

public interface IDisconnectParticipantUseCase
{
    Task<Either<Error, Unit>> Execute(DisconnectParticipantCommand command);
}

public record DisconnectParticipantCommand(Guid RoomId, string ParticipantName);
