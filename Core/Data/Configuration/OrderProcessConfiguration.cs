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
                   .HasForeignKey(op => op.OrderId);

            builder.Property(op => op.Result)
                   .IsRequired();

            builder.ToTable("OrderProcesses");
        }
    }
}
