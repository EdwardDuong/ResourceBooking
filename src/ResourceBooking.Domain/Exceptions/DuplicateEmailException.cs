namespace ResourceBooking.Domain.Exceptions;

/// <summary>
/// Raised when registration is attempted with an email that's already taken.
/// </summary>
public sealed class DuplicateEmailException : DomainException
{
    public DuplicateEmailException(string email)
        : base($"An account with email '{email}' already exists.")
    {
    }
}
