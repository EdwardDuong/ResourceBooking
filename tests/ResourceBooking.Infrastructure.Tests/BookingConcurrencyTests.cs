using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ResourceBooking.Domain.Entities;
using ResourceBooking.Domain.Exceptions;
using ResourceBooking.Infrastructure.Persistence;
using ResourceBooking.Infrastructure.Persistence.Repositories;
using Xunit;

namespace ResourceBooking.Infrastructure.Tests;

/// <summary>
/// Exercises the correctness guarantee ADR-0001 is built around: two
/// bookings for the same resource and slot must never both succeed. SQLite
/// (rather than a real SQL Server instance) is used here so the suite has no
/// external dependency; it enforces the same unique-index semantics the
/// production schema relies on - see BookingConfiguration and
/// UniqueConstraintViolationDetector.
/// </summary>
public class BookingConcurrencyTests : IDisposable
{
    private readonly string _dbPath;
    private readonly string _connectionString;

    public BookingConcurrencyTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"resourcebooking-concurrency-{Guid.NewGuid()}.db");
        _connectionString = $"Data Source={_dbPath};Default Timeout=5";

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    [Fact]
    public async Task AddAsync_TwoConcurrentBookingsForSameSlot_ExactlyOneSucceeds()
    {
        var resourceId = await SeedResourceAsync();

        var slot = new TimeSlot(new DateTimeOffset(2026, 3, 2, 9, 0, 0, TimeSpan.FromHours(11)));
        var firstBooking = new Booking(resourceId, Guid.NewGuid(), slot);
        var secondBooking = new Booking(resourceId, Guid.NewGuid(), slot);

        var firstAttempt = SafeAddAsync(firstBooking);
        var secondAttempt = SafeAddAsync(secondBooking);
        var results = await Task.WhenAll(firstAttempt, secondAttempt);

        Assert.Single(results, succeeded => succeeded);
        Assert.Single(results, succeeded => !succeeded);

        await using var verificationContext = CreateContext();
        var persistedCount = await verificationContext.Bookings
            .CountAsync(b => b.ResourceId == resourceId && b.SlotStart == slot.Start);
        Assert.Equal(1, persistedCount);
    }

    [Fact]
    public async Task AddAsync_SlotFreedByCancellation_CanBeRebooked()
    {
        var resourceId = await SeedResourceAsync();

        var slot = new TimeSlot(new DateTimeOffset(2026, 3, 2, 9, 15, 0, TimeSpan.FromHours(11)));
        var original = new Booking(resourceId, Guid.NewGuid(), slot);
        original.Cancel();

        await using (var context = CreateContext())
        {
            context.Bookings.Add(original);
            await context.SaveChangesAsync();
        }

        var rebooked = new Booking(resourceId, Guid.NewGuid(), slot);
        await using var repositoryContext = CreateContext();
        var repository = new BookingRepository(repositoryContext);

        await repository.AddAsync(rebooked, CancellationToken.None);

        await using var verificationContext = CreateContext();
        var activeCount = await verificationContext.Bookings
            .CountAsync(b => b.ResourceId == resourceId && b.SlotStart == slot.Start);
        Assert.Equal(2, activeCount);
    }

    private async Task<bool> SafeAddAsync(Booking booking)
    {
        await using var context = CreateContext();
        var repository = new BookingRepository(context);

        try
        {
            await repository.AddAsync(booking, CancellationToken.None);
            return true;
        }
        catch (BookingConflictException)
        {
            return false;
        }
    }

    private async Task<Guid> SeedResourceAsync()
    {
        await using var context = CreateContext();
        var resource = new Resource("Conference Room A");
        context.Resources.Add(resource);
        await context.SaveChangesAsync();
        return resource.Id;
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
}
