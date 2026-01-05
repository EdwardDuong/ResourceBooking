namespace ResourceBooking.Domain.Entities;

/// <summary>
/// A discretized, fixed-width time slot. Bookings are anchored to slots rather
/// than arbitrary start/end times so that slot uniqueness can be enforced by a
/// database constraint instead of a locking/serialization strategy.
/// See docs/adr/0001-time-slot-concurrency-strategy.md for the reasoning.
/// </summary>
public readonly record struct TimeSlot
{
    public static readonly TimeSpan Duration = TimeSpan.FromMinutes(15);

    public DateTimeOffset Start { get; }
    public DateTimeOffset End => Start + Duration;

    public TimeSlot(DateTimeOffset start)
    {
        if (start.Minute % (int)Duration.TotalMinutes != 0 || start.Second != 0 || start.Millisecond != 0)
        {
            throw new ArgumentException(
                $"Slot start must align to {Duration.TotalMinutes}-minute boundaries.", nameof(start));
        }

        Start = start;
    }
}
