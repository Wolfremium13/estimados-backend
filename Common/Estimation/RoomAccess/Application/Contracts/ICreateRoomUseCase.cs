using System;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;

namespace Common.Estimation.RoomAccess.Application.Contracts;

public interface ICreateRoomUseCase
{
    Task<Either<Error, CreatedRoomInfo>> Execute(CreateRoomCommand command);
}

public record CreateRoomCommand(string ModeratorName);

public record CreatedRoomInfo(Guid RoomId, string ModeratorName);
