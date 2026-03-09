using ResourceBooking.Infrastructure.Security;
using Xunit;

namespace ResourceBooking.Infrastructure.Tests;

public class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _hasher = new();

    [Fact]
    public void Verify_WithCorrectPassword_ReturnsTrue()
    {
        var hash = _hasher.Hash("correct-horse-battery-staple");

        Assert.True(_hasher.Verify("correct-horse-battery-staple", hash));
    }

    [Fact]
    public void Verify_WithWrongPassword_ReturnsFalse()
    {
        var hash = _hasher.Hash("correct-horse-battery-staple");

        Assert.False(_hasher.Verify("wrong-password", hash));
    }

    [Fact]
    public void Hash_CalledTwiceForSamePassword_ProducesDifferentOutput()
    {
        var first = _hasher.Hash("same-password");
        var second = _hasher.Hash("same-password");

        Assert.NotEqual(first, second);
        Assert.True(_hasher.Verify("same-password", first));
        Assert.True(_hasher.Verify("same-password", second));
    }
}
