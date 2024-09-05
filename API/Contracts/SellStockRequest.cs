using System;
using System.ComponentModel;

namespace Core.Features.SellStock
{
    public class SellStockRequest
    {
        [DefaultValue("3aa42229-1c0f-4630-8c1a-db879ecd0427")]
        public Guid? UserId { get; set; }
        [DefaultValue("AAPL")]
        public string? StockSymbol { get; set; }
        [DefaultValue(1)]
        public int? Quantity { get; set; }
    }
}
