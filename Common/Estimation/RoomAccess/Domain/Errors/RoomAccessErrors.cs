namespace Common.Estimation.RoomAccess.Domain.Errors;

public static class RoomAccessErrors
{
    public class ClientValidationException(string message) : Exception(message);

    public class RoomNotFoundException(string message) : Exception(message);

    public class InvalidRoleException(string message) : Exception(message);

    public class AccessDeniedException(string message) : Exception(message);

    public class RoomClosedException(string message) : Exception(message);

    public class ParticipantNotFoundException(string message) : Exception(message);
}