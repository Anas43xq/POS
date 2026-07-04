using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DAL.Configurations
{
    public class TaxRateConfiguration : IEntityTypeConfiguration<TaxRate>
    {
        public void Configure(EntityTypeBuilder<TaxRate> builder)
        {
            builder.ToTable("TaxRates");

            builder.HasKey(t => t.TaxRateId);

            builder.Property(t => t.Name)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(t => t.Rate)
                   .HasColumnType("decimal(5,4)")
                   .IsRequired();

            builder.Property(t => t.CreatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(t => t.UpdatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("GETDATE()");

        }
    }
}