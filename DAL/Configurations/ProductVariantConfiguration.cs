using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations;

public sealed class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.HasKey(x => x.VariantId);

        builder.Property(x => x.UnitPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .HasPrecision(0);

        builder.Property(x => x.UpdatedAt)
            .HasPrecision(0);

        builder.HasOne(x => x.Product)
            .WithMany(x => x.ProductVariants)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Size)
            .WithMany(x => x.ProductVariants)
            .HasForeignKey(x => x.SizeId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(x => x.TransactionItems)
            .WithOne(x => x.ProductVariant)
            .HasForeignKey(x => x.VariantId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => new
        {
            x.ProductId,
            x.SizeId
        })
        .IncludeProperties(x => new
        {
            x.UnitPrice,
            x.IsActive
        });

        builder.HasIndex(x => x.SizeId);
    }
}
