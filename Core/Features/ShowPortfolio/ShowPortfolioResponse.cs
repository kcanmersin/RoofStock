using System;
using System.Collections.Generic;

namespace Core.Features.ShowPortfolio
{
    public class ShowPortfolioResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<StockHoldingItemDetail> StockHoldingItems { get; set; } = new List<StockHoldingItemDetail>();
        public decimal TotalPortfolioValue { get; set; }
        public decimal Change { get; set; }

        public decimal TotalBalance { get; set; }

        public decimal AvailableBalance { get; set; }

        public class StockHoldingItemDetail
        {
            public string StockSymbol { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
        }
    }
}
