using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using Common.Estimation.RoomAccess.Domain.Models;

namespace Common.Estimation.RoomAccess.Domain.Ports;

public interface IEstimationRoomRepository
{
    Task<Either<Error, EstimationRoom>> FindById(RoomId roomId);
    Task<Either<Error, Unit>> Save(EstimationRoom room);
}
