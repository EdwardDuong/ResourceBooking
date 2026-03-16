using MediatR;

namespace ResourceBooking.Application.Bookings.Commands.CancelBooking;

public record CancelBookingCommand(Guid BookingId, Guid RequestingUserId, bool RequestingUserIsAdmin) : IRequest;
