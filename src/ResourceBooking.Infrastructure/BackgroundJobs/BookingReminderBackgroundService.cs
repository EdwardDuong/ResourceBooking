using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ResourceBooking.Application.Reminders.Commands.SendDueReminders;

namespace ResourceBooking.Infrastructure.BackgroundJobs;

/// <summary>
/// Scans for due reminders on a fixed interval. Runs in the same process as
/// the API - fine at this scale, but means reminders stop if the API isn't
/// running; a real deployment would move this to its own worker/job so
/// scaling or restarting the API doesn't affect reminder delivery.
/// </summary>
public class BookingReminderBackgroundService : BackgroundService
{
    private static readonly TimeSpan ScanInterval = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan ReminderWindow = TimeSpan.FromMinutes(30);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingReminderBackgroundService> _logger;

    public BookingReminderBackgroundService(
        IServiceScopeFactory scopeFactory, ILogger<BookingReminderBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(ScanInterval);

        do
        {
            await ScanOnceAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ScanOnceAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        try
        {
            var sentCount = await sender.Send(new SendDueRemindersCommand(ReminderWindow), cancellationToken);
            if (sentCount > 0)
            {
                _logger.LogInformation("Sent {Count} booking reminder(s).", sentCount);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // A scan failure shouldn't kill the background service for the
            // rest of the process's lifetime - log it and try again next tick.
            _logger.LogError(ex, "Booking reminder scan failed.");
        }
    }
}
