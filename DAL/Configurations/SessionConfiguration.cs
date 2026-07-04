using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations
{
    public class SessionConfiguration : IEntityTypeConfiguration<Session>
    {
        public void Configure(EntityTypeBuilder<Session> builder)
        {
            builder.ToTable("Sessions");

            builder.HasKey(s => s.SessionId);

            builder.HasOne(s => s.User)
                   .WithMany(u => u.Sessions)
                   .HasForeignKey(s => s.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(s => s.LoginAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()")
                   .IsRequired();

            builder.Property(s => s.LogoutAt)
                   .HasColumnType("datetime2");

            builder.HasIndex(s => s.UserId);
            builder.HasIndex(s => s.LoginAt);
        }
    }
}