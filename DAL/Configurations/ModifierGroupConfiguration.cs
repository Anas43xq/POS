using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations;

public sealed class ModifierGroupConfiguration : IEntityTypeConfiguration<ModifierGroup>
{
    public void Configure(EntityTypeBuilder<ModifierGroup> builder)
    {
        builder.HasKey(x => x.ModifierGroupId);

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.GroupType)
            .HasColumnType("tinyint")
            .IsRequired();

        builder.Property(x => x.IsRequired)
            .HasDefaultValue(false);

        builder.Property(x => x.MinSelections)
            .HasDefaultValue(0);

        builder.Property(x => x.MaxSelections)
            .HasDefaultValue(1);

        builder.Property(x => x.SortOrder)
            .HasDefaultValue(0);

        builder.Property(x => x.CreatedAt)
            .HasPrecision(0);

        builder.Property(x => x.UpdatedAt)
            .HasPrecision(0);

        builder.HasMany(x => x.ModifierOptions)
            .WithOne(x => x.ModifierGroup)
            .HasForeignKey(x => x.ModifierGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ModifierGroupTranslations)
            .WithOne(x => x.ModifierGroup)
            .HasForeignKey(x => x.ModifierGroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}