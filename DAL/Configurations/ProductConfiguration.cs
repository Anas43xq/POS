using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");

            builder.HasKey(p => p.ProductId);

            builder.Property(p => p.Name)
                   .HasMaxLength(150)
                   .IsRequired();

            builder.Property(p => p.Description)
                   .HasMaxLength(255);

            builder.Property(p => p.UnitPrice)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(p => p.IsActive)
                   .HasDefaultValue(true);

            builder.Property(p => p.CreatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(p => p.UpdatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("GETDATE()");

            builder.HasOne(p => p.Category)
                   .WithMany(c => c.Products)
                   .HasForeignKey(p => p.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.TaxRate)
                   .WithMany(t => t.Products)
                   .HasForeignKey(p => p.TaxRateId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(p => p.TaxRateId)
            .HasColumnName("TaxRateId");
        }
    }
}   