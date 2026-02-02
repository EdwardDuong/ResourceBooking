namespace ResourceBooking.Domain.Exceptions;

/// <summary>
/// Raised when a booking cannot be created because the requested slot on the
/// requested resource is already taken. Distinguished from validation errors
/// so the API layer can map it to 409 Conflict rather than 400 Bad Request.
/// </summary>
public sealed class BookingConflictException : DomainException
{
    public Guid ResourceId { get; }
    public DateTimeOffset SlotStart { get; }

    public BookingConflictException(Guid resourceId, DateTimeOffset slotStart)
        : base($"Resource {resourceId} is already booked for the slot starting at {slotStart:O}.")
    {
        ResourceId = resourceId;
        SlotStart = slotStart;
    }
}
