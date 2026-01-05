using ResourceBooking.Domain.Entities;
using Xunit;

namespace ResourceBooking.Domain.Tests;

public class ResourceTests
{
    [Fact]
    public void Constructor_WithValidName_CreatesActiveResource()
    {
        var resource = new Resource("Conference Room A");

        Assert.True(resource.IsActive);
        Assert.Equal("Conference Room A", resource.Name);
        Assert.NotEqual(Guid.Empty, resource.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithInvalidName_Throws(string? name)
    {
        Assert.Throws<ArgumentException>(() => new Resource(name!));
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var resource = new Resource("Conference Room A");

        resource.Deactivate();

        Assert.False(resource.IsActive);
    }
}
