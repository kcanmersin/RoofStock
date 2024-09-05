using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Core.Data.Entity;

namespace Core.Data.Configuration
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.HasKey(t => t.Id);

            builder.HasOne(t => t.User)
                   .WithMany(u => u.Transactions)
                   .HasForeignKey(t => t.UserId);

            builder.HasOne(t => t.StockHoldingItem)//nullable
                   .WithMany()
                   .HasForeignKey(t => t.StockHoldingItemId)
                   .IsRequired(false);

            builder.Property(t => t.Amount)
                   .HasColumnType("decimal(18,2)");

            builder.Property(t => t.Type)
                   .IsRequired();

            builder.Property(t => t.Description)
                   .HasMaxLength(500);

            builder.ToTable("Transactions");
        }
    }
}
