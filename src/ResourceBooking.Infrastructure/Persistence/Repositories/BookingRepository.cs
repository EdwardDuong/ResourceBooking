using Microsoft.EntityFrameworkCore;
using ResourceBooking.Application.Common.Interfaces;
using ResourceBooking.Domain.Entities;
using ResourceBooking.Domain.Enums;
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

    public Task UpdateAsync(Booking booking, CancellationToken cancellationToken) =>
        _dbContext.SaveChangesAsync(cancellationToken);

    public async Task<IReadOnlyList<DateTimeOffset>> GetTakenSlotStartsAsync(
        Guid resourceId, DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken cancellationToken)
    {
        // Status filtering happens after the range query rather than in the
        // SQL Where clause: the SQLite provider can't translate a comparison
        // against an enum backed by a string value converter (it collapses
        // to the underlying int and throws at query-compile time). The
        // range query itself stays server-side - see BookingConfiguration
        // for why SlotStart is stored as UTC DateTime rather than
        // DateTimeOffset, which is what makes the range comparison
        // translatable at all under SQLite.
        var bookingsInRange = await _dbContext.Bookings
            .Where(b => b.ResourceId == resourceId && b.SlotStart >= fromUtc && b.SlotStart < toUtc)
            .ToListAsync(cancellationToken);

        return bookingsInRange
            .Where(b => b.Status != BookingStatus.Cancelled)
            .Select(b => b.SlotStart)
            .ToList();
    }
}
