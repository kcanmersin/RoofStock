using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Data.Entity;

namespace Core.Data.Configuration
{
    internal class StockHoldingConfiguration : IEntityTypeConfiguration<StockHolding>
    {
        public void Configure(EntityTypeBuilder<StockHolding> builder)
        {
            builder.HasKey(sh => sh.Id);

            builder.HasOne(sh => sh.User)
                   .WithMany(u => u.StockHoldings)
                   .HasForeignKey(sh => sh.UserId);

            builder.Property(sh => sh.StockSymbol)
                   .IsRequired()
                   .HasMaxLength(6);

            builder.Property(sh => sh.TotalPurchasePrice)
                   .HasColumnType("decimal(18,2)");

            builder.ToTable("StockHoldings");
        }
    }
}
