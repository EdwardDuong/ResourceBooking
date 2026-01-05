using ResourceBooking.Domain.Enums;

namespace ResourceBooking.Domain.Entities;

/// <summary>
/// A single reservation of a Resource for one TimeSlot.
/// Conflict-prevention logic (rejecting overlapping bookings) is added in the
/// application/infrastructure layers alongside its tests - see ADR-0001.
/// </summary>
public class Booking
{
    public Guid Id { get; private set; }
    public Guid ResourceId { get; private set; }
    public Guid RequestedByUserId { get; private set; }
    public DateTimeOffset SlotStart { get; private set; }
    public BookingStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? CancelledAtUtc { get; private set; }

    private Booking() { }

    public Booking(Guid resourceId, Guid requestedByUserId, TimeSlot slot)
    {
        Id = Guid.NewGuid();
        ResourceId = resourceId;
        RequestedByUserId = requestedByUserId;
        SlotStart = slot.Start;
        Status = BookingStatus.Pending;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void Confirm()
    {
        if (Status != BookingStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot confirm a booking in status {Status}.");
        }

        Status = BookingStatus.Confirmed;
    }

    public void Cancel()
    {
        if (Status is BookingStatus.Cancelled or BookingStatus.Completed)
        {
            throw new InvalidOperationException($"Cannot cancel a booking in status {Status}.");
        }

        Status = BookingStatus.Cancelled;
        CancelledAtUtc = DateTimeOffset.UtcNow;
    }
}
