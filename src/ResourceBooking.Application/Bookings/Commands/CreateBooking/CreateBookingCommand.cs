using MediatR;

namespace ResourceBooking.Application.Bookings.Commands.CreateBooking;

public record CreateBookingCommand(
    Guid ResourceId,
    Guid RequestedByUserId,
    DateTimeOffset SlotStart) : IRequest<Guid>;
