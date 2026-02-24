using MediatR;

namespace ResourceBooking.Application.Bookings.Commands.CancelBooking;

public record CancelBookingCommand(Guid BookingId) : IRequest;
