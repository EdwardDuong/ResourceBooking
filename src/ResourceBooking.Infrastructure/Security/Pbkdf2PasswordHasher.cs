using System.Security.Cryptography;
using ResourceBooking.Application.Common.Interfaces;

namespace ResourceBooking.Infrastructure.Security;

/// <summary>
/// PBKDF2-SHA256 password hashing via the BCL (no external dependency).
/// Stored format is "{iterations}.{saltBase64}.{hashBase64}" so the work
/// factor can be increased later without invalidating hashes created under
/// the old one - Verify reads the iteration count from the stored value
/// rather than assuming the current default.
/// </summary>
public class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int SaltSizeBytes = 16;
    private const int HashSizeBytes = 32;
    private const int Iterations = 210_000;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);
        var hash = Derive(password, salt, Iterations);

        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string hash)
    {
        var parts = hash.Split('.', 3);
        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[1]);
        var expectedHash = Convert.FromBase64String(parts[2]);
        var actualHash = Derive(password, salt, iterations);

        return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
    }

    private static byte[] Derive(string password, byte[] salt, int iterations) =>
        Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, HashSizeBytes);
}
