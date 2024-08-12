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

            builder.HasOne(o => o.OrderProcess)
                   .WithOne(op => op.Order)
                   .HasForeignKey<OrderProcess>(op => op.OrderId);

            builder.ToTable("Orders");
        }
    }
}
