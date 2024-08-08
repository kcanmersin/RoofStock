using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Data.Entity;

namespace Core.Data.Configuration
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.StockSymbol)
                   .IsRequired()
                   .HasMaxLength(10);

            builder.Property(o => o.TargetPrice)
                   .HasColumnType("decimal(18,2)");

            builder.ToTable("Orders");
        }
    }
}
