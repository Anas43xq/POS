using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations
{
    public class ShiftConfiguration : IEntityTypeConfiguration<Shift>
    {
    public void Configure(EntityTypeBuilder<Shift> builder)
        {
            builder.ToTable("Shifts");

            builder.HasKey(s => s.ShiftId);

            builder.Property(s => s.Status)
                   .HasColumnType("tinyint")
                   .HasDefaultValue(ShiftStatus.Open);

            builder.Property(s => s.OpeningCash)
                   .HasColumnType("decimal(18,2)")
                   .HasDefaultValue(0);

            builder.Property(s => s.ClosingCash)
                   .HasColumnType("decimal(18,2)");

            builder.Property(s => s.ExpectedCash)
                   .HasColumnType("decimal(18,2)");

            builder.Property(s => s.CashDifference)
                   .HasColumnType("decimal(18,2)");

            builder.Property(s => s.OpenedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()");

            builder.Property(s => s.ClosedAt)
                   .HasColumnType("datetime2");

            builder.HasOne(s => s.User)
                   .WithMany(u => u.Shifts)
                   .HasForeignKey(s => s.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Indices for efficient queries
            builder.HasIndex(s => s.UserId);
            builder.HasIndex(s => s.Status);
            
            // Composite index for common query pattern: finding open shifts for a user
            builder.HasIndex(s => new { s.UserId, s.Status })
                   .HasDatabaseName("IX_Shifts_UserId_Status");
        }
    }
}