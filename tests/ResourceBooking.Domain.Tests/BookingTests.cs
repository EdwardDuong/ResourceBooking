using ResourceBooking.Domain.Entities;
using ResourceBooking.Domain.Enums;
using Xunit;

namespace ResourceBooking.Domain.Tests;

public class BookingTests
{
    private static TimeSlot ArbitrarySlot() =>
        new(new DateTimeOffset(2026, 2, 4, 9, 0, 0, TimeSpan.FromHours(11)));

    [Fact]
    public void Constructor_CreatesPendingBooking()
    {
        var resourceId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var booking = new Booking(resourceId, userId, ArbitrarySlot());

        Assert.Equal(resourceId, booking.ResourceId);
        Assert.Equal(userId, booking.RequestedByUserId);
        Assert.Equal(BookingStatus.Pending, booking.Status);
        Assert.Null(booking.CancelledAtUtc);
        Assert.NotEqual(Guid.Empty, booking.Id);
    }

    [Fact]
    public void Confirm_WhenPending_SetsStatusToConfirmed()
    {
        var booking = new Booking(Guid.NewGuid(), Guid.NewGuid(), ArbitrarySlot());

        booking.Confirm();

        Assert.Equal(BookingStatus.Confirmed, booking.Status);
    }

    [Fact]
    public void Confirm_WhenAlreadyConfirmed_Throws()
    {
        var booking = new Booking(Guid.NewGuid(), Guid.NewGuid(), ArbitrarySlot());
        booking.Confirm();

        Assert.Throws<InvalidOperationException>(() => booking.Confirm());
    }

    [Fact]
    public void Cancel_WhenPending_SetsStatusToCancelledAndStampsTimestamp()
    {
        var booking = new Booking(Guid.NewGuid(), Guid.NewGuid(), ArbitrarySlot());

        booking.Cancel();

        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        Assert.NotNull(booking.CancelledAtUtc);
    }

    [Fact]
    public void Cancel_WhenConfirmed_Succeeds()
    {
        var booking = new Booking(Guid.NewGuid(), Guid.NewGuid(), ArbitrarySlot());
        booking.Confirm();

        booking.Cancel();

        Assert.Equal(BookingStatus.Cancelled, booking.Status);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_Throws()
    {
        var booking = new Booking(Guid.NewGuid(), Guid.NewGuid(), ArbitrarySlot());
        booking.Cancel();

        Assert.Throws<InvalidOperationException>(() => booking.Cancel());
    }

    [Fact]
    public void MarkReminderSent_WhenNotYetSent_StampsTimestamp()
    {
        var booking = new Booking(Guid.NewGuid(), Guid.NewGuid(), ArbitrarySlot());

        booking.MarkReminderSent();

        Assert.NotNull(booking.ReminderSentAtUtc);
    }

    [Fact]
    public void MarkReminderSent_WhenAlreadySent_Throws()
    {
        var booking = new Booking(Guid.NewGuid(), Guid.NewGuid(), ArbitrarySlot());
        booking.MarkReminderSent();

        Assert.Throws<InvalidOperationException>(() => booking.MarkReminderSent());
    }
}
