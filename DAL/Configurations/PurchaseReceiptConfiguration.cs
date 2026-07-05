using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations
{
    public class PurchaseReceiptConfiguration : IEntityTypeConfiguration<PurchaseReceipt>
    {
        public void Configure(EntityTypeBuilder<PurchaseReceipt> builder)
        {
            builder.ToTable("PurchaseReceipts");

            builder.HasKey(p => p.ReceiptId);

            builder.Property(p => p.ReceiptTypeId)
                   .IsRequired();

            builder.Property(p => p.SupplierId);

            builder.Property(p => p.InvoiceNumber)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(p => p.InvoiceDate)
                   .HasColumnType("date")
                   .IsRequired();

            builder.Property(p => p.Category)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(p => p.Description)
                   .HasMaxLength(500);

            builder.Property(p => p.Subtotal)
                   .HasColumnType("decimal(18,2)")
                   .HasDefaultValue(0m);

            builder.Property(p => p.VatRate)
                   .HasColumnType("decimal(5,2)")
                   .HasDefaultValue(0m);

            builder.Property(p => p.VatAmount)
                   .HasColumnType("decimal(18,2)")
                   .HasDefaultValue(0m);

            builder.Property(p => p.GrandTotal)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(p => p.Notes)
                   .HasMaxLength(1000);

            builder.Property(p => p.ImagePath)
                   .HasMaxLength(500);

            builder.Property(p => p.CreatedBy)
                   .IsRequired();

            builder.Property(p => p.CreatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()");

            builder.HasOne(p => p.ReceiptType)
                   .WithMany(r => r.PurchaseReceipts)
                   .HasForeignKey(p => p.ReceiptTypeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Supplier)
                   .WithMany(s => s.PurchaseReceipts)
                   .HasForeignKey(p => p.SupplierId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(p => p.CreatedByUser)
                   .WithMany()
                   .HasForeignKey(p => p.CreatedBy)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(p => p.InvoiceDate);
            builder.HasIndex(p => p.SupplierId);
            builder.HasIndex(p => p.ReceiptTypeId);
            builder.HasIndex(p => p.Category);
            builder.HasIndex(p => p.InvoiceNumber);
        }
    }
}
