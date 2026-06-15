namespace Common.Estimation.EstimationSession.Domain.Errors;

public static class EstimationSessionErrors
{
    public class ClientValidationException(string message) : Exception(message);

    public class InvalidStateTransitionException(string message) : Exception(message);

    public class OnlyDevelopersCanVoteException(string message) : Exception(message);

    public class NoVotesCastException(string message) : Exception(message);

    public class InvalidCardException(string message) : Exception(message);

    public class SessionNotFoundException(string message) : Exception(message);
}