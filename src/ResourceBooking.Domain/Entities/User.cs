using ResourceBooking.Domain.Enums;

namespace ResourceBooking.Domain.Entities;

/// <summary>
/// An account that can authenticate and make bookings. Password hashing is
/// an infrastructure concern (see Application.Common.Interfaces.IPasswordHasher)
/// - this entity only ever stores the resulting hash, never a plaintext password.
/// </summary>
public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    private User() { }

    public User(string email, string passwordHash, UserRole role = UserRole.Member)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be empty.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));
        }

        Id = Guid.NewGuid();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        Role = role;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }
}
