using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ResourceBooking.Domain.Entities;

namespace ResourceBooking.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(u => u.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");
    }
}
