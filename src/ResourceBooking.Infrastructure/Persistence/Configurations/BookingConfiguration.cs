using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ResourceBooking.Domain.Entities;

namespace ResourceBooking.Infrastructure.Persistence.Configurations;

/// <summary>
/// The unique index on (ResourceId, SlotStart) is the enforcement mechanism
/// for ADR-0001: two bookings for the same resource and slot can never both
/// be inserted, regardless of request timing, without any explicit locking.
/// </summary>
public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.ResourceId)
            .IsRequired();

        builder.Property(b => b.RequestedByUserId)
            .IsRequired();

        // Stored as UTC DateTime rather than DateTimeOffset: SQLite (used by
        // the test suite) can't translate range comparisons on
        // DateTimeOffset columns, only equality. All slot times are UTC
        // anyway, so nothing is lost by normalizing at the persistence
        // boundary instead of carrying an offset nothing uses.
        builder.Property(b => b.SlotStart)
            .IsRequired()
            .HasConversion(
                toStore => toStore.UtcDateTime,
                fromStore => new DateTimeOffset(fromStore, TimeSpan.Zero));

        builder.Property(b => b.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(b => b.CreatedAtUtc)
            .IsRequired();

        builder.Property(b => b.ReminderSentAtUtc);

        // Filtered so a cancelled booking doesn't permanently lock its slot -
        // only Pending/Confirmed/Completed rows compete for uniqueness.
        builder.HasIndex(b => new { b.ResourceId, b.SlotStart })
            .IsUnique()
            .HasFilter("[Status] <> 'Cancelled'")
            .HasDatabaseName("IX_Bookings_ResourceId_SlotStart");

        builder.HasOne<Resource>()
            .WithMany()
            .HasForeignKey(b => b.ResourceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
