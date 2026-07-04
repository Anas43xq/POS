using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.UserId);

            builder.HasIndex(u => u.Username)
                   .IsUnique();

            builder.Property(u => u.FullName)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(u => u.Username)
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(u => u.PasswordHash)
                   .HasMaxLength(255)
                   .IsRequired();

            builder.HasOne(u => u.Role)
                   .WithMany(r => r.Users)
                   .HasForeignKey(u => u.RoleId);
        }
    }
}