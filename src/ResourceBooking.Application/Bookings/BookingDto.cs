using ResourceBooking.Domain.Enums;

namespace ResourceBooking.Application.Bookings;

public record BookingDto(Guid Id, Guid ResourceId, DateTimeOffset SlotStart, BookingStatus Status);
