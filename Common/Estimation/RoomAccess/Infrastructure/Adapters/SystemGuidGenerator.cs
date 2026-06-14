using Common.Estimation.RoomAccess.Domain.Ports;

namespace Common.Estimation.RoomAccess.Infrastructure.Adapters;

public class SystemGuidGenerator : IGuidGenerator
{
    public Guid NewGuid() => Guid.NewGuid();
}