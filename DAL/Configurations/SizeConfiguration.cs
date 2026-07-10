using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations;

public sealed class SizeConfiguration : IEntityTypeConfiguration<Size>
{
    public void Configure(EntityTypeBuilder<Size> builder)
    {
        builder.HasKey(x => x.SizeId);

        builder.Property(x => x.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .HasPrecision(0);

        builder.Property(x => x.UpdatedAt)
            .HasPrecision(0);

        builder.HasMany(x => x.SizeTranslations)
            .WithOne(x => x.Size)
            .HasForeignKey(x => x.SizeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ProductVariants)
            .WithOne(x => x.Size)
            .HasForeignKey(x => x.SizeId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
