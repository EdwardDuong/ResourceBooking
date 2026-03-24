using ResourceBooking.Application.Bookings.Queries.GetAvailability;

namespace ResourceBooking.Application.Common.Interfaces;

/// <summary>
/// Caches computed availability grids. A short TTL is the safety net;
/// callers that mutate bookings are responsible for calling InvalidateAsync
/// for the affected resource/date so readers don't see stale data before
/// the TTL expires - see CreateBookingCommandHandler and
/// CancelBookingCommandHandler.
/// </summary>
public interface IAvailabilityCache
{
    Task<ResourceAvailabilityDto?> GetAsync(Guid resourceId, DateOnly date, CancellationToken cancellationToken);

    Task SetAsync(
        Guid resourceId, DateOnly date, ResourceAvailabilityDto availability, CancellationToken cancellationToken);

    Task InvalidateAsync(Guid resourceId, DateOnly date, CancellationToken cancellationToken);
}
