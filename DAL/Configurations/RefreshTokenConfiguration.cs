using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");

            builder.HasKey(rt => rt.RefreshTokenId);

            builder.HasOne(rt => rt.User)
                   .WithMany(u => u.RefreshTokens)
                   .HasForeignKey(rt => rt.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(rt => rt.TokenHash)
                   .HasMaxLength(128)
                   .IsRequired();

            builder.Property(rt => rt.ExpiresAt)
                   .HasColumnType("datetime2")
                   .IsRequired();

            builder.Property(rt => rt.CreatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()")
                   .IsRequired();

            builder.Property(rt => rt.RevokedAt)
                   .HasColumnType("datetime2");

            builder.HasIndex(rt => rt.TokenHash).IsUnique();
            builder.HasIndex(rt => rt.UserId);
        }
    }
}
