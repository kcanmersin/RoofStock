using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Data.Entity;

namespace Core.Data.Configuration
{
    public class OrderProcessConfiguration : IEntityTypeConfiguration<OrderProcess>
    {
        public void Configure(EntityTypeBuilder<OrderProcess> builder)
        {
            builder.HasKey(op => op.Id);

            builder.HasOne(op => op.Order)
                   .WithMany()
                   .HasForeignKey(op => op.OrderId)
                   .OnDelete(DeleteBehavior.Cascade); 

            builder.Property(op => op.Status)
                   .HasConversion<int>()  
                   .HasDefaultValue(OrderProcessStatus.Pending); 

            builder.HasIndex(op => op.OrderId);

            builder.ToTable("OrderProcesses");
        }
    }
}
