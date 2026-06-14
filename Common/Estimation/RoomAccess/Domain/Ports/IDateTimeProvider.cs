namespace Common.Estimation.RoomAccess.Domain.Ports;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}