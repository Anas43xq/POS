using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations
{
    public class PurchaseReceiptTypeConfiguration : IEntityTypeConfiguration<PurchaseReceiptType>
    {
        public void Configure(EntityTypeBuilder<PurchaseReceiptType> builder)
        {
            builder.ToTable("PurchaseReceiptTypes");

            builder.HasKey(p => p.ReceiptTypeId);

            builder.Property(p => p.Name)
                   .HasMaxLength(30)
                   .IsRequired();

            builder.HasIndex(p => p.Name)
                   .IsUnique();
        }
    }
}
