using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Data.Entity;

namespace Core.Data.Configuration
{
    internal class StockHoldingItemConfiguration : IEntityTypeConfiguration<StockHoldingItem>
    {
        public void Configure(EntityTypeBuilder<StockHoldingItem> builder)
        {
            builder.HasKey(shi => shi.Id);

            builder.HasOne(shi => shi.OrderProcess)
                   .WithMany()
                   .HasForeignKey(shi => shi.OrderProcessId)
                   .IsRequired(false); //  nullable  because stockholdingitem might be bought without an order

            builder.Property(shi => shi.StockSymbol)
                   .IsRequired()
                   .HasMaxLength(6);

            builder.Property(shi => shi.UnitPrice)
                   .HasColumnType("decimal(18,2)");

            builder.Property(shi => shi.Type)
                   .IsRequired();

            builder.ToTable("StockHoldingItems");
        }
    }
}
