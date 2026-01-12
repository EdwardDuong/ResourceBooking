using ResourceBooking.Domain.Entities;

namespace ResourceBooking.Application.Common.Interfaces;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Attempts to persist a new booking. Implementations are responsible for
    /// surfacing a conflict (e.g. via a unique constraint violation) as a
    /// well-defined application-level result rather than letting a raw DB
    /// exception escape - see ADR-0001.
    /// </summary>
    Task AddAsync(Booking booking, CancellationToken cancellationToken);
}
