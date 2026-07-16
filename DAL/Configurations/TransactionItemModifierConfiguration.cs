using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations;

public sealed class TransactionItemModifierConfiguration : IEntityTypeConfiguration<TransactionItemModifier>
{
    public void Configure(EntityTypeBuilder<TransactionItemModifier> builder)
    {
        builder.ToTable("TransactionItemModifiers");

        builder.HasKey(x => x.TransactionItemModifierId);

        builder.Property(x => x.GroupName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.OptionName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Quantity)
            .HasDefaultValue(1);

        builder.Property(x => x.PriceAdd)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder.Property(x => x.LineTotal)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder.HasOne(x => x.TransactionItem)
            .WithMany()
            .HasForeignKey(x => x.TransactionItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ModifierOption)
            .WithMany()
            .HasForeignKey(x => x.ModifierOptionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.ModifierGroup)
            .WithMany()
            .HasForeignKey(x => x.ModifierGroupId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}