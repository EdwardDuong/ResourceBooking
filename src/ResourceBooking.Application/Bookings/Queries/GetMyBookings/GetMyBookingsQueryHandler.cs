using MediatR;
using ResourceBooking.Application.Common.Interfaces;

namespace ResourceBooking.Application.Bookings.Queries.GetMyBookings;

public class GetMyBookingsQueryHandler : IRequestHandler<GetMyBookingsQuery, IReadOnlyList<BookingDto>>
{
    private readonly IBookingRepository _bookingRepository;

    public GetMyBookingsQueryHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<IReadOnlyList<BookingDto>> Handle(
        GetMyBookingsQuery request, CancellationToken cancellationToken)
    {
        var bookings = await _bookingRepository.GetByUserAsync(request.UserId, cancellationToken);
        return bookings.Select(b => b.ToDto()).ToList();
    }
}
