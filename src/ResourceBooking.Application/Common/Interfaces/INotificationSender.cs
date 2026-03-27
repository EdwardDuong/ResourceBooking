namespace ResourceBooking.Application.Common.Interfaces;

/// <summary>
/// Delivers a booking reminder to a user. The only implementation today
/// (LoggingNotificationSender) writes to the application log rather than
/// actually emailing anyone - there's no mail provider wired up. Behind
/// this interface, that's a config/DI change, not a change to
/// SendDueRemindersCommandHandler or the background scanner that calls it.
/// </summary>
public interface INotificationSender
{
    Task SendBookingReminderAsync(
        string recipientEmail, Guid bookingId, DateTimeOffset slotStart, CancellationToken cancellationToken);
}
