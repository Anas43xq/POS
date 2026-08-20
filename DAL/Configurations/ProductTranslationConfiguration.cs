using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations;

public sealed class ProductTranslationConfiguration : IEntityTypeConfiguration<ProductTranslation>
{
    public void Configure(EntityTypeBuilder<ProductTranslation> builder)
    {
        builder.HasKey(x => x.ProductTranslationId);

        builder.Property(x => x.LanguageCode)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(255);

        builder.Property(x => x.CreatedAt)
            .HasPrecision(0);

        builder.HasOne(x => x.Product)
            .WithMany(x => x.ProductTranslations)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
        {
            x.ProductId,
            x.LanguageCode
        }).IsUnique();

        builder.HasIndex(x => x.LanguageCode);
    }
}
