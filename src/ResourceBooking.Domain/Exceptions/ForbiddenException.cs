namespace ResourceBooking.Domain.Exceptions;

/// <summary>
/// Raised when an authenticated user attempts an action on a resource they
/// don't own and aren't privileged enough to act on (e.g. cancelling
/// someone else's booking). Distinct from 401 - the caller is authenticated,
/// just not permitted.
/// </summary>
public sealed class ForbiddenException : DomainException
{
    public ForbiddenException(string message) : base(message)
    {
    }
}
