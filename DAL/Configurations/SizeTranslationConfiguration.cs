using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations;

public sealed class SizeTranslationConfiguration : IEntityTypeConfiguration<SizeTranslation>
{
    public void Configure(EntityTypeBuilder<SizeTranslation> builder)
    {
        builder.HasKey(x => x.SizeTranslationId);

        builder.Property(x => x.LanguageCode)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasPrecision(0);

        builder.HasOne(x => x.Size)
            .WithMany(x => x.SizeTranslations)
            .HasForeignKey(x => x.SizeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
        {
            x.SizeId,
            x.LanguageCode
        }).IsUnique();

        builder.HasIndex(x => x.LanguageCode);
    }
}
