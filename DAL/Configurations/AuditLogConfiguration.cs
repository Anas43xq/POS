using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");

            builder.HasKey(a => a.AuditLogId);

            builder.Property(a => a.ActionType)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(a => a.EntityName)
                   .HasMaxLength(100);

            builder.Property(a => a.OccurredAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()");

            builder.HasOne(a => a.User)
                   .WithMany(u => u.AuditLogs)
                   .HasForeignKey(a => a.UserId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(a => a.UserId);
            builder.HasIndex(a => a.OccurredAt);
            builder.HasIndex(a => a.ActionType);
        }
    }
}