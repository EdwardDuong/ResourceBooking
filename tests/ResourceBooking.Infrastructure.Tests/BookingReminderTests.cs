using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ResourceBooking.Application.Common.Interfaces;
using ResourceBooking.Application.Reminders.Commands.SendDueReminders;
using ResourceBooking.Domain.Entities;
using ResourceBooking.Domain.Enums;
using ResourceBooking.Infrastructure.Persistence;
using ResourceBooking.Infrastructure.Persistence.Repositories;
using Xunit;

namespace ResourceBooking.Infrastructure.Tests;

public class BookingReminderTests : IDisposable
{
    private readonly string _dbPath;
    private readonly string _connectionString;

    public BookingReminderTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"resourcebooking-reminders-{Guid.NewGuid()}.db");
        _connectionString = $"Data Source={_dbPath};Default Timeout=5";

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    [Fact]
    public async Task GetBookingsDueForReminderAsync_ExcludesOutOfWindowSentAndCancelledBookings()
    {
        var resourceId = await SeedResourceAsync();

        // A fixed, already slot-aligned base time rather than real UtcNow:
        // offsetting real "now" by a few minutes and rounding down to the
        // 15-minute grid can collapse two distinct offsets into the same
        // slot depending on what minute the test happens to run at.
        var now = new DateTimeOffset(2026, 6, 1, 9, 0, 0, TimeSpan.Zero);

        // Window below is 60 minutes; each candidate needs a distinct,
        // 15-minute-aligned SlotStart (the unique index is on
        // ResourceId+SlotStart) so three within-window candidates need
        // offsets of 15/30/45, with the fourth (tooFarOut) past the window.
        var due = new Booking(resourceId, Guid.NewGuid(), new TimeSlot(now.AddMinutes(15)));
        var alreadyReminded = new Booking(resourceId, Guid.NewGuid(), new TimeSlot(now.AddMinutes(30)));
        alreadyReminded.MarkReminderSent();
        var cancelled = new Booking(resourceId, Guid.NewGuid(), new TimeSlot(now.AddMinutes(45)));
        cancelled.Cancel();
        var tooFarOut = new Booking(resourceId, Guid.NewGuid(), new TimeSlot(now.AddHours(3)));

        await using (var context = CreateContext())
        {
            context.Bookings.AddRange(due, alreadyReminded, cancelled, tooFarOut);
            await context.SaveChangesAsync();
        }

        await using var queryContext = CreateContext();
        var repository = new BookingRepository(queryContext);
        var result = await repository.GetBookingsDueForReminderAsync(
            now, now.AddMinutes(60), CancellationToken.None);

        var onlyResult = Assert.Single(result);
        Assert.Equal(due.Id, onlyResult.Id);
    }

    [Fact]
    public async Task SendDueRemindersCommandHandler_SendsOnceThenSkipsOnNextScan()
    {
        var resourceId = await SeedResourceAsync();
        var userId = await SeedUserAsync("attendee@example.com");
        var now = DateTimeOffset.UtcNow;

        await using (var context = CreateContext())
        {
            var booking = new Booking(resourceId, userId, new TimeSlot(AlignToSlot(now.AddMinutes(15))));
            context.Bookings.Add(booking);
            await context.SaveChangesAsync();
        }

        var sender = new FakeNotificationSender();

        var firstRun = await SendReminders(sender);
        var secondRun = await SendReminders(sender);

        Assert.Equal(1, firstRun);
        Assert.Equal(0, secondRun);
        Assert.Single(sender.SentTo, email => email == "attendee@example.com");
    }

    private async Task<int> SendReminders(INotificationSender sender)
    {
        await using var context = CreateContext();
        var handler = new SendDueRemindersCommandHandler(
            new BookingRepository(context), new UserRepository(context), sender);

        return await handler.Handle(new SendDueRemindersCommand(TimeSpan.FromMinutes(30)), CancellationToken.None);
    }

    private static DateTimeOffset AlignToSlot(DateTimeOffset value)
    {
        var alignedMinute = value.Minute - (value.Minute % 15);
        return new DateTimeOffset(value.Year, value.Month, value.Day, value.Hour, alignedMinute, 0, value.Offset);
    }

    private async Task<Guid> SeedResourceAsync()
    {
        await using var context = CreateContext();
        var resource = new Resource("Conference Room A");
        context.Resources.Add(resource);
        await context.SaveChangesAsync();
        return resource.Id;
    }

    private async Task<Guid> SeedUserAsync(string email)
    {
        await using var context = CreateContext();
        var user = new User(email, "hashed-password", UserRole.Member);
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user.Id;
    }

    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connectionString)
            .Options;

        return new AppDbContext(options);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }

    private class FakeNotificationSender : INotificationSender
    {
        public List<string> SentTo { get; } = [];

        public Task SendBookingReminderAsync(
            string recipientEmail, Guid bookingId, DateTimeOffset slotStart, CancellationToken cancellationToken)
        {
            SentTo.Add(recipientEmail);
            return Task.CompletedTask;
        }
    }
}
