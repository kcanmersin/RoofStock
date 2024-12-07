namespace Core.Features.SellStock
{
    public class SellStockResponse
    {
        public bool IsSuccess { get; set; }
        public string StockSymbol { get; set; }
        public int Quantity { get; set; }
        public decimal NewBalance { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal TotalProceeds { get; set; } 
    }
}
