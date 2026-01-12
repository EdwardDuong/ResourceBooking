using Microsoft.EntityFrameworkCore;
using ResourceBooking.Domain.Entities;

namespace ResourceBooking.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext. Entity configurations (including the unique constraint
/// that enforces the no-double-booking rule from ADR-0001) land in the next
/// milestone alongside the migration and its tests.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<Booking> Bookings => Set<Booking>();
}
