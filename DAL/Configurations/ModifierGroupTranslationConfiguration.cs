using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations;

public sealed class ModifierGroupTranslationConfiguration : IEntityTypeConfiguration<ModifierGroupTranslation>
{
    public void Configure(EntityTypeBuilder<ModifierGroupTranslation> builder)
    {
        builder.HasKey(x => x.ModifierGroupTranslationId);

        builder.Property(x => x.LanguageCode)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasPrecision(0);

        builder.HasOne(x => x.ModifierGroup)
            .WithMany(x => x.ModifierGroupTranslations)
            .HasForeignKey(x => x.ModifierGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
        {
            x.ModifierGroupId,
            x.LanguageCode
        }).IsUnique();

        builder.HasIndex(x => x.LanguageCode);
    }
}