namespace ResourceBooking.Domain.Exceptions;

/// <summary>
/// Raised on a failed login attempt. Deliberately doesn't distinguish
/// "no such user" from "wrong password" in its message - that distinction
/// is exactly what an attacker enumerating emails would want to know.
/// </summary>
public sealed class InvalidCredentialsException : DomainException
{
    public InvalidCredentialsException()
        : base("Invalid email or password.")
    {
    }
}
