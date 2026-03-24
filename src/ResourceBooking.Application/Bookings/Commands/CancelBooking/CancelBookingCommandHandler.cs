using MediatR;
using ResourceBooking.Application.Common.Interfaces;
using ResourceBooking.Domain.Entities;
using ResourceBooking.Domain.Exceptions;

namespace ResourceBooking.Application.Bookings.Commands.CancelBooking;

public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IAvailabilityCache _availabilityCache;

    public CancelBookingCommandHandler(IBookingRepository bookingRepository, IAvailabilityCache availabilityCache)
    {
        _bookingRepository = bookingRepository;
        _availabilityCache = availabilityCache;
    }

    public async Task Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(request.BookingId, cancellationToken)
            ?? throw new NotFoundException(nameof(Booking), request.BookingId);

        if (booking.RequestedByUserId != request.RequestingUserId && !request.RequestingUserIsAdmin)
        {
            throw new ForbiddenException("You can only cancel your own bookings.");
        }

        booking.Cancel();

        await _bookingRepository.UpdateAsync(booking, cancellationToken);

        await _availabilityCache.InvalidateAsync(
            booking.ResourceId, DateOnly.FromDateTime(booking.SlotStart.UtcDateTime), cancellationToken);
    }
}
