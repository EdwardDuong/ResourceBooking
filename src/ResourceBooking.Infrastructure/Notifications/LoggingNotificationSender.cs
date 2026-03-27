using Microsoft.Extensions.Logging;
using ResourceBooking.Application.Common.Interfaces;

namespace ResourceBooking.Infrastructure.Notifications;

public class LoggingNotificationSender : INotificationSender
{
    private readonly ILogger<LoggingNotificationSender> _logger;

    public LoggingNotificationSender(ILogger<LoggingNotificationSender> logger)
    {
        _logger = logger;
    }

    public Task SendBookingReminderAsync(
        string recipientEmail, Guid bookingId, DateTimeOffset slotStart, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Reminder: booking {BookingId} for {RecipientEmail} starts at {SlotStart}",
            bookingId, recipientEmail, slotStart);

        return Task.CompletedTask;
    }
}
