using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Data.Entity;

namespace Core.Data.Configuration
{
    public class StockPriceAlertConfiguration : IEntityTypeConfiguration<StockPriceAlert>
    {
        public void Configure(EntityTypeBuilder<StockPriceAlert> builder)
        {
            builder.HasKey(spa => spa.Id);

            builder.Property(spa => spa.StockSymbol)
                   .IsRequired()
                   .HasMaxLength(10);

            builder.Property(spa => spa.TargetPrice)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(spa => spa.IsTriggered)
                   .IsRequired();

            builder.Property(spa => spa.TriggeredDate)
                   .IsRequired(false);

            builder.Property(spa => spa.AlertType)
                   .IsRequired();

            builder.HasOne(spa => spa.User)
                   .WithMany()
                   .HasForeignKey(spa => spa.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.ToTable("StockPriceAlerts");
        }
    }
}
