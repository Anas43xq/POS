using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations;

public sealed class ModifierOptionTranslationConfiguration : IEntityTypeConfiguration<ModifierOptionTranslation>
{
    public void Configure(EntityTypeBuilder<ModifierOptionTranslation> builder)
    {
        builder.HasKey(x => x.ModifierOptionTranslationId);

        builder.Property(x => x.LanguageCode)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasPrecision(0);

        builder.HasOne(x => x.ModifierOption)
            .WithMany(x => x.ModifierOptionTranslations)
            .HasForeignKey(x => x.ModifierOptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
        {
            x.ModifierOptionId,
            x.LanguageCode
        }).IsUnique();

        builder.HasIndex(x => x.LanguageCode);
    }
}