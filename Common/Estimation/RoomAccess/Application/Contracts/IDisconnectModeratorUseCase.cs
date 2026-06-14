using LanguageExt;
using LanguageExt.Common;

namespace Common.Estimation.RoomAccess.Application.Contracts;

public interface IDisconnectModeratorUseCase
{
    Task<Either<Error, Unit>> Execute(DisconnectModeratorCommand command);
}

public record DisconnectModeratorCommand(Guid RoomId);