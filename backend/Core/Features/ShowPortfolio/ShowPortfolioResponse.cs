using Core.Data.Entity;

namespace Core.Features.ShowPortfolio
{
    public class ShowPortfolioResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<StockHoldingItemDetail> StockHoldingItems { get; set; } = new List<StockHoldingItemDetail>();
        public List<OrderDetail> Orders { get; set; } = new List<OrderDetail>();
        public List<StockPriceAlertDetail> StockPriceAlerts { get; set; } = new List<StockPriceAlertDetail>();
        public List<TransactionDetail> Transactions { get; set; } = new List<TransactionDetail>();
        public decimal TotalPortfolioValue { get; set; }
        public decimal Change { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal AvailableBalance { get; set; }

        public class StockHoldingItemDetail
        {
            public string StockSymbol { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal HoldingValue { get; set; }
            public decimal Change { get; set; }
        }

        public class OrderDetail
        {
            public Guid OrderId { get; set; }
            public string StockSymbol { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public decimal TargetPrice { get; set; }
            public OrderType OrderType { get; set; }
            public OrderProcessStatus Status { get; set; }
            public DateTime CreatedDate { get; set; }
        }

        public class StockPriceAlertDetail
        {
            public Guid AlertId { get; set; }
            public string StockSymbol { get; set; } = string.Empty;
            public decimal TargetPrice { get; set; }
            public AlertType AlertType { get; set; }
            public bool IsTriggered { get; set; }
            public DateTime? TriggeredDate { get; set; }
            public DateTime CreatedDate { get; set; }
        }

        public class TransactionDetail
        {
            public Guid Id { get; set; }
            public decimal Amount { get; set; }
            public string Type { get; set; }  // Positive/Negative
            public string Description { get; set; }
            public DateTime CreatedDate { get; set; }
        }
    }
}
