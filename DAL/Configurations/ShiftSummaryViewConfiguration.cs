using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations
{
    public class ShiftSummaryViewConfiguration : IEntityTypeConfiguration<ShiftSummaryView>
    {
        public void Configure(EntityTypeBuilder<ShiftSummaryView> builder)
        {
            builder.HasNoKey();
            builder.ToView("vw_ShiftSummary");

            builder.Property(x => x.ShiftId).HasColumnName("ShiftId");
            builder.Property(x => x.CashierName).HasColumnName("CashierName");
            builder.Property(x => x.OpenTime).HasColumnName("OpenTime");
            builder.Property(x => x.OpeningCash).HasColumnName("OpeningCash");
            builder.Property(x => x.CloseTime).HasColumnName("CloseTime");
            builder.Property(x => x.ClosingCash).HasColumnName("ClosingCash");
            builder.Property(x => x.ExpectedCash).HasColumnName("ExpectedCash");
            builder.Property(x => x.CashDifference).HasColumnName("CashDifference");
            builder.Property(x => x.Status)
                .HasColumnName("Status")
                .HasConversion<byte>();
        }
    }
}
