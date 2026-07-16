using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations;

public sealed class ModifierOptionConfiguration : IEntityTypeConfiguration<ModifierOption>
{
    public void Configure(EntityTypeBuilder<ModifierOption> builder)
    {
        builder.ToTable("ModifierOptions");

        builder.HasKey(x => x.ModifierOptionId);

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.PriceAdd)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.AllowQuantity)
            .HasDefaultValue(false);

        builder.Property(x => x.IsDefault)
            .HasDefaultValue(false);

        builder.Property(x => x.SortOrder)
            .HasDefaultValue(0);

        builder.Property(x => x.CreatedAt)
            .HasPrecision(0);

        builder.Property(x => x.UpdatedAt)
            .HasPrecision(0);

        builder.HasOne(x => x.ModifierGroup)
            .WithMany(x => x.ModifierOptions)
            .HasForeignKey(x => x.ModifierGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ModifierOptionTranslations)
            .WithOne(x => x.ModifierOption)
            .HasForeignKey(x => x.ModifierOptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}