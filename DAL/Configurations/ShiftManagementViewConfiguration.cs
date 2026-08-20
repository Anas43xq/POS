using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations
{
    public class ShiftManagementViewConfiguration : IEntityTypeConfiguration<ShiftManagementView>
    {
        public void Configure(EntityTypeBuilder<ShiftManagementView> builder)
        {
            builder.HasNoKey();
            builder.ToView("vw_ShiftManagement");

            builder.Property(x => x.ShiftId).HasColumnName("ShiftId");
            builder.Property(x => x.UserId).HasColumnName("UserId");
            builder.Property(x => x.CashierName).HasColumnName("CashierName");
            builder.Property(x => x.OpenedAt).HasColumnName("OpenedAt");
            builder.Property(x => x.ClosedAt).HasColumnName("ClosedAt");
            builder.Property(x => x.OpeningCash).HasColumnName("OpeningCash");
            builder.Property(x => x.ClosingCash).HasColumnName("ClosingCash");
            builder.Property(x => x.ExpectedCash).HasColumnName("ExpectedCash");
            builder.Property(x => x.CashDifference).HasColumnName("CashDifference");
            builder.Property(x => x.Status).HasColumnName("Status").HasConversion<byte>();
            builder.Property(x => x.StatusLabel).HasColumnName("StatusLabel");
            builder.Property(x => x.DurationHours).HasColumnName("DurationHours");
        }
    }
}
