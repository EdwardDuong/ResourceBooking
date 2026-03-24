using MediatR;
using ResourceBooking.Application.Common.Interfaces;
using ResourceBooking.Domain.Entities;
using ResourceBooking.Domain.Exceptions;

namespace ResourceBooking.Application.Bookings.Queries.GetAvailability;

/// <summary>
/// Availability is computed for a full UTC day (00:00-24:00) rather than
/// against resource-specific operating hours - there is no concept of
/// business hours in the domain model yet, so every slot is considered
/// bookable except the ones already taken.
/// </summary>
public class GetAvailabilityQueryHandler : IRequestHandler<GetAvailabilityQuery, ResourceAvailabilityDto>
{
    private readonly IResourceRepository _resourceRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IAvailabilityCache _availabilityCache;

    public GetAvailabilityQueryHandler(
        IResourceRepository resourceRepository,
        IBookingRepository bookingRepository,
        IAvailabilityCache availabilityCache)
    {
        _resourceRepository = resourceRepository;
        _bookingRepository = bookingRepository;
        _availabilityCache = availabilityCache;
    }

    public async Task<ResourceAvailabilityDto> Handle(
        GetAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var cached = await _availabilityCache.GetAsync(request.ResourceId, request.Date, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        _ = await _resourceRepository.GetByIdAsync(request.ResourceId, cancellationToken)
            ?? throw new NotFoundException(nameof(Resource), request.ResourceId);

        var dayStart = new DateTimeOffset(request.Date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var dayEnd = dayStart.AddDays(1);

        var takenSlots = await _bookingRepository.GetTakenSlotStartsAsync(
            request.ResourceId, dayStart, dayEnd, cancellationToken);
        var takenSlotSet = takenSlots.ToHashSet();

        var slots = new List<AvailabilitySlotDto>();
        for (var slotStart = dayStart; slotStart < dayEnd; slotStart += TimeSlot.Duration)
        {
            slots.Add(new AvailabilitySlotDto(slotStart, IsAvailable: !takenSlotSet.Contains(slotStart)));
        }

        var availability = new ResourceAvailabilityDto(request.ResourceId, request.Date, slots);
        await _availabilityCache.SetAsync(request.ResourceId, request.Date, availability, cancellationToken);
        return availability;
    }
}
