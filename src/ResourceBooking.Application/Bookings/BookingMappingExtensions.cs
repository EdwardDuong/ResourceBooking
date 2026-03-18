using ResourceBooking.Domain.Entities;

namespace ResourceBooking.Application.Bookings;

public static class BookingMappingExtensions
{
    public static BookingDto ToDto(this Booking booking) =>
        new(booking.Id, booking.ResourceId, booking.SlotStart, booking.Status);
}
