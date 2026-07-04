using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations
{
    public class RecentTransactionViewConfiguration : IEntityTypeConfiguration<RecentTransactionView>
    {
        public void Configure(EntityTypeBuilder<RecentTransactionView> builder)
        {
            builder.HasNoKey();
            builder.ToView("vw_RecentTransactions");

            builder.Property(x => x.TransactionId).HasColumnName("TransactionId");
            builder.Property(x => x.ReceiptNumber).HasColumnName("ReceiptNumber");
            builder.Property(x => x.GrandTotal).HasColumnName("GrandTotal");
            builder.Property(x => x.PaymentMethod).HasColumnName("PaymentMethod");
            builder.Property(x => x.TransactionDate).HasColumnName("TransactionDate");
            builder.Property(x => x.CashierName).HasColumnName("CashierName");
            builder.Property(x => x.Status).HasColumnName("Status");
        }
    }
}
