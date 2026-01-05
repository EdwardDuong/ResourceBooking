namespace ResourceBooking.Domain.Exceptions;

/// <summary>
/// Base type for exceptions raised by domain invariants.
/// Kept distinct from infrastructure/framework exceptions so the API layer
/// can translate them into stable, predictable error responses.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }
}
