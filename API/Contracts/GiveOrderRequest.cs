using System;
using System.ComponentModel;
using Core.Data.Entity;

namespace Core.Features.GiveOrder
{
    public class GiveOrderRequest
    {
        [DefaultValue("3aa42229-1c0f-4630-8c1a-db879ecd0427")]
        public Guid? UserId { get; set; }

        [DefaultValue("AAPL")]
        public string? StockSymbol { get; set; }

        [DefaultValue(1)]
        public int? Quantity { get; set; }

        [DefaultValue(150.0)]
        public decimal? TargetPrice { get; set; }

        [DefaultValue(OrderType.Buy)]
        public OrderType? OrderType { get; set; }
    }
}
