using Microsoft.EntityFrameworkCore;
using ResourceBooking.Application.Common.Interfaces;
using ResourceBooking.Domain.Entities;
using ResourceBooking.Domain.Exceptions;

namespace ResourceBooking.Infrastructure.Persistence.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _dbContext;

    public BookingRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _dbContext.Bookings.SingleOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task AddAsync(Booking booking, CancellationToken cancellationToken)
    {
        await _dbContext.Bookings.AddAsync(booking, cancellationToken);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (UniqueConstraintViolationDetector.IsSlotConflict(ex))
        {
            throw new BookingConflictException(booking.ResourceId, booking.SlotStart);
        }
    }
}
