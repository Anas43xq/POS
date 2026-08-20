using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations;

public sealed class CategoryTranslationConfiguration : IEntityTypeConfiguration<CategoryTranslation>
{
    public void Configure(EntityTypeBuilder<CategoryTranslation> builder)
    {
        builder.HasKey(x => x.CategoryTranslationId);

        builder.Property(x => x.LanguageCode)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasPrecision(0);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.CategoryTranslations)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
        {
            x.CategoryId,
            x.LanguageCode
        }).IsUnique();

        builder.HasIndex(x => x.LanguageCode);
    }
}
