using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");

            builder.HasKey(p => p.PaymentId);

            builder.Property(p => p.PaymentMethod)
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(p => p.AmountTendered)
                   .HasColumnType("decimal(10,2)")
                   .IsRequired();

            builder.Property(p => p.ChangeGiven)
                   .HasColumnType("decimal(10,2)")
                   .HasDefaultValue(0);

            builder.Property(p => p.ReferenceNumber)
                   .HasMaxLength(100);

            builder.Property(p => p.PaidAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("GETDATE()");

            builder.HasOne(p => p.Transaction)
                   .WithMany(t => t.Payments)
                   .HasForeignKey(p => p.TransactionId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}