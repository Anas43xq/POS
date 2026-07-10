using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations
{
    public class TransactionItemConfiguration : IEntityTypeConfiguration<TransactionItem>
    {
        public void Configure(EntityTypeBuilder<TransactionItem> builder)
        {
            builder.ToTable("TransactionItems");

            builder.HasKey(t => t.TransactionItemId);

            builder.Property(t => t.ProductName)
                   .HasMaxLength(150)
                   .IsRequired();

            builder.Property(t => t.UnitPrice)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(t => t.Quantity)
                   .HasDefaultValue(1);

            builder.Property(t => t.TaxRate)
                   .HasColumnType("decimal(9,4)")
                   .IsRequired();



            builder.Property(t => t.LineSubtotal)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(t => t.LineTax)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(t => t.LineTotal)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.HasOne(t => t.Transaction)
                   .WithMany(t => t.TransactionItems)
                   .HasForeignKey(t => t.TransactionId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(t => t.ProductVariant)
                   .WithMany(pv => pv.TransactionItems)
                   .HasForeignKey(t => t.VariantId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}