using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations
{
    public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
    {
        public void Configure(EntityTypeBuilder<Supplier> builder)
        {
            builder.ToTable("Suppliers");

            builder.HasKey(s => s.SupplierId);

            builder.Property(s => s.CompanyName)
                   .HasMaxLength(200)
                   .IsRequired();

            builder.Property(s => s.TRN)
                   .HasMaxLength(50);

            builder.Property(s => s.Address)
                   .HasMaxLength(300);

            builder.Property(s => s.Phone)
                   .HasMaxLength(50);

            builder.Property(s => s.Email)
                   .HasMaxLength(255);

            builder.Property(s => s.Notes)
                   .HasMaxLength(500);

            builder.Property(s => s.CreatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()");

            builder.HasIndex(s => s.CompanyName)
                   .IsUnique();

            builder.HasIndex(s => s.TRN);
        }
    }
}
