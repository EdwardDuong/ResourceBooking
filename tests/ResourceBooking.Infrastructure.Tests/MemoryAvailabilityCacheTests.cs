using Microsoft.Extensions.Caching.Memory;
using ResourceBooking.Application.Bookings.Queries.GetAvailability;
using ResourceBooking.Infrastructure.Caching;
using Xunit;

namespace ResourceBooking.Infrastructure.Tests;

public class MemoryAvailabilityCacheTests
{
    private readonly MemoryAvailabilityCache _cache = new(new MemoryCache(new MemoryCacheOptions()));

    [Fact]
    public async Task GetAsync_BeforeAnySet_ReturnsNull()
    {
        var result = await _cache.GetAsync(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.UtcNow), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_ThenGetAsync_ReturnsTheSameValue()
    {
        var resourceId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow);
        var availability = new ResourceAvailabilityDto(resourceId, date, []);

        await _cache.SetAsync(resourceId, date, availability, CancellationToken.None);
        var result = await _cache.GetAsync(resourceId, date, CancellationToken.None);

        Assert.Equal(availability, result);
    }

    [Fact]
    public async Task InvalidateAsync_RemovesACachedEntry()
    {
        var resourceId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow);
        await _cache.SetAsync(resourceId, date, new ResourceAvailabilityDto(resourceId, date, []), CancellationToken.None);

        await _cache.InvalidateAsync(resourceId, date, CancellationToken.None);
        var result = await _cache.GetAsync(resourceId, date, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_DoesNotAffectADifferentDate()
    {
        var resourceId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow);
        var otherDate = date.AddDays(1);
        await _cache.SetAsync(resourceId, date, new ResourceAvailabilityDto(resourceId, date, []), CancellationToken.None);

        var result = await _cache.GetAsync(resourceId, otherDate, CancellationToken.None);

        Assert.Null(result);
    }
}
