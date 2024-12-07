namespace Core.Features.BuyStock
{
    public class BuyStockResponse
    {
        public bool IsSuccess { get; set; }
        public string StockSymbol { get; set; }
        public int Quantity { get; set; }
        public decimal NewBalance { get; set; }
        public string Message { get; set; } = string.Empty;

        //total price
        public decimal TotalPrice { get; set; }


    }
}
