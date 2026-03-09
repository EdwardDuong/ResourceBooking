using ResourceBooking.Domain.Entities;
using ResourceBooking.Domain.Enums;
using Xunit;

namespace ResourceBooking.Domain.Tests;

public class UserTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesMemberByDefault()
    {
        var user = new User("Jane.Doe@Example.com", "hashed-value");

        Assert.Equal("jane.doe@example.com", user.Email);
        Assert.Equal(UserRole.Member, user.Role);
        Assert.NotEqual(Guid.Empty, user.Id);
    }

    [Fact]
    public void Constructor_WithExplicitRole_UsesIt()
    {
        var user = new User("admin@example.com", "hashed-value", UserRole.Admin);

        Assert.Equal(UserRole.Admin, user.Role);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidEmail_Throws(string? email)
    {
        Assert.Throws<ArgumentException>(() => new User(email!, "hashed-value"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidPasswordHash_Throws(string? passwordHash)
    {
        Assert.Throws<ArgumentException>(() => new User("user@example.com", passwordHash!));
    }
}
