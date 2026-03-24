using MediatR;
using ResourceBooking.Application.Common.Interfaces;
using ResourceBooking.Domain.Entities;
using ResourceBooking.Domain.Exceptions;

namespace ResourceBooking.Application.Bookings.Commands.CreateBooking;

public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, Guid>
{
    private readonly IResourceRepository _resourceRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IAvailabilityCache _availabilityCache;

    public CreateBookingCommandHandler(
        IResourceRepository resourceRepository,
        IBookingRepository bookingRepository,
        IAvailabilityCache availabilityCache)
    {
        _resourceRepository = resourceRepository;
        _bookingRepository = bookingRepository;
        _availabilityCache = availabilityCache;
    }

    public async Task<Guid> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var resource = await _resourceRepository.GetByIdAsync(request.ResourceId, cancellationToken)
            ?? throw new NotFoundException(nameof(Resource), request.ResourceId);

        if (!resource.IsActive)
        {
            throw new ResourceInactiveException(resource.Id);
        }

        var slot = new TimeSlot(request.SlotStart);
        var booking = new Booking(request.ResourceId, request.RequestedByUserId, slot);

        // Throws BookingConflictException if the slot is already taken - see
        // BookingRepository.AddAsync and ADR-0001.
        await _bookingRepository.AddAsync(booking, cancellationToken);

        await _availabilityCache.InvalidateAsync(
            booking.ResourceId, DateOnly.FromDateTime(booking.SlotStart.UtcDateTime), cancellationToken);

        return booking.Id;
    }
}
