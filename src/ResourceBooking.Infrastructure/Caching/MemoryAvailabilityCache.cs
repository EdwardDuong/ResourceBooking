using Microsoft.Extensions.Caching.Memory;
using ResourceBooking.Application.Bookings.Queries.GetAvailability;
using ResourceBooking.Application.Common.Interfaces;

namespace ResourceBooking.Infrastructure.Caching;

public class MemoryAvailabilityCache : IAvailabilityCache
{
    // Short TTL: this is a safety net for any invalidation path we missed,
    // not the primary correctness mechanism - CreateBookingCommandHandler
    // and CancelBookingCommandHandler invalidate explicitly on write.
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(30);

    private readonly IMemoryCache _cache;

    public MemoryAvailabilityCache(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<ResourceAvailabilityDto?> GetAsync(
        Guid resourceId, DateOnly date, CancellationToken cancellationToken)
    {
        _cache.TryGetValue(BuildKey(resourceId, date), out ResourceAvailabilityDto? cached);
        return Task.FromResult(cached);
    }

    public Task SetAsync(
        Guid resourceId, DateOnly date, ResourceAvailabilityDto availability, CancellationToken cancellationToken)
    {
        _cache.Set(BuildKey(resourceId, date), availability, Ttl);
        return Task.CompletedTask;
    }

    public Task InvalidateAsync(Guid resourceId, DateOnly date, CancellationToken cancellationToken)
    {
        _cache.Remove(BuildKey(resourceId, date));
        return Task.CompletedTask;
    }

    private static string BuildKey(Guid resourceId, DateOnly date) =>
        $"availability:{resourceId:N}:{date:yyyy-MM-dd}";
}
