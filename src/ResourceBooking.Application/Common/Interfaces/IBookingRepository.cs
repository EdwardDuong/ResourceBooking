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

    /// <summary>
    /// Persists changes made to a Booking previously loaded via
    /// GetByIdAsync (e.g. Confirm/Cancel).
    /// </summary>
    Task UpdateAsync(Booking booking, CancellationToken cancellationToken);

    /// <summary>
    /// Slot start times already taken (Pending/Confirmed/Completed) for the
    /// given resource within [fromUtc, toUtc) - used to compute availability.
    /// </summary>
    Task<IReadOnlyList<DateTimeOffset>> GetTakenSlotStartsAsync(
        Guid resourceId, DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken cancellationToken);

    /// <summary>
    /// A user's bookings, most recent slot first. Includes cancelled
    /// bookings - the caller decides whether to show them.
    /// </summary>
    Task<IReadOnlyList<Booking>> GetByUserAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Active (Pending/Confirmed) bookings whose slot falls in
    /// (nowUtc, reminderWindowEndUtc] and haven't had a reminder sent yet.
    /// </summary>
    Task<IReadOnlyList<Booking>> GetBookingsDueForReminderAsync(
        DateTimeOffset nowUtc, DateTimeOffset reminderWindowEndUtc, CancellationToken cancellationToken);
}
