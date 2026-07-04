using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Roles");

            builder.HasKey(r => r.RoleId);

            builder.HasIndex(r => r.RoleName)
                   .IsUnique();

            builder.Property(r => r.RoleName)
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(r => r.Description)
                   .HasMaxLength(255);

            builder.Property(u => u.CreatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()");

                     // Permissions mapping removed per request (permissions feature disabled)
        }
    }
}