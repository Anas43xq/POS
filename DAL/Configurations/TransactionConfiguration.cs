using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("Transactions");

            builder.HasKey(t => t.TransactionId);

            builder.Property(t => t.ReceiptNumber)
                   .HasMaxLength(30)
                   .IsRequired();

            builder.HasIndex(t => t.ReceiptNumber)
                   .IsUnique();

            builder.Property(t => t.Subtotal)
                   .HasColumnType("decimal(10,2)")
                   .HasDefaultValue(0);

            builder.Property(t => t.TaxTotal)
                   .HasColumnType("decimal(10,2)")
                   .HasDefaultValue(0);

            builder.Property(t => t.GrandTotal)
                   .HasColumnType("decimal(10,2)")
                   .HasDefaultValue(0);

            builder.Property(t => t.Status)
                   .HasColumnType("tinyint")
                   .HasConversion<byte>()
                   .HasDefaultValue(TransactionStatus.Pending);

            builder.Property(t => t.Notes)
                   .HasMaxLength(500);

            builder.Property(t => t.TransactionDate)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(t => t.CreatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("GETDATE()");

            builder.HasOne(t => t.Shift)
                   .WithMany(s => s.Transactions)
                   .HasForeignKey(t => t.ShiftId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Cashier)
                   .WithMany(u => u.Transactions)
                   .HasForeignKey(t => t.CashierId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}