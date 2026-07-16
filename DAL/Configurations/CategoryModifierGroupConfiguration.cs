using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations;

public sealed class CategoryModifierGroupConfiguration : IEntityTypeConfiguration<CategoryModifierGroup>
{
    public void Configure(EntityTypeBuilder<CategoryModifierGroup> builder)
    {
        builder.ToTable("CategoryModifierGroups");

        builder.HasKey(x => new { x.CategoryId, x.ModifierGroupId });

        builder.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ModifierGroup)
            .WithMany(x => x.CategoryModifierGroups)
            .HasForeignKey(x => x.ModifierGroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}