using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Data.Entity;

namespace Core.Data.Configuration
{
    internal class OrderProcessConfiguration : IEntityTypeConfiguration<OrderProcess>
    {
        public void Configure(EntityTypeBuilder<OrderProcess> builder)
        {
            builder.HasKey(op => op.Id);

            builder.HasOne(op => op.Order)
                   .WithOne(o => o.OrderProcess) 
                   .HasForeignKey<OrderProcess>(op => op.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(op => op.Status)
                   .HasConversion<int>()
                   .HasDefaultValue(OrderProcessStatus.Pending);

            builder.ToTable("OrderProcesses");
        }
    }
}
