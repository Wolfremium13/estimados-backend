using System;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;

namespace Common.Estimation.RoomAccess.Application.Contracts;

public interface IApproveJoinRequestUseCase
{
    Task<Either<Error, Unit>> Execute(ApproveJoinRequestCommand command);
}

public record ApproveJoinRequestCommand(Guid RoomId, Guid RequestId);
