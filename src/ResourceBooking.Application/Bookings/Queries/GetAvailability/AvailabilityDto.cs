namespace ResourceBooking.Application.Bookings.Queries.GetAvailability;

public record AvailabilitySlotDto(DateTimeOffset SlotStart, bool IsAvailable);

public record ResourceAvailabilityDto(
    Guid ResourceId,
    DateOnly Date,
    IReadOnlyList<AvailabilitySlotDto> Slots);
