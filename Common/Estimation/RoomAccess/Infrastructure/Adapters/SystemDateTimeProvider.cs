using Common.Estimation.RoomAccess.Domain.Ports;

namespace Common.Estimation.RoomAccess.Infrastructure.Adapters;

public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}