using MediatR;

namespace ResourceBooking.Application.Bookings.Queries.GetMyBookings;

public record GetMyBookingsQuery(Guid UserId) : IRequest<IReadOnlyList<BookingDto>>;
