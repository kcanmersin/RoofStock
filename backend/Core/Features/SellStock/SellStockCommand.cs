using Core.Shared;
using MediatR;

namespace Core.Features.SellStock
{
    public class SellStockCommand : IRequest<Result<SellStockResponse>>
    {
        public Guid UserId { get; set; }
        public string StockSymbol { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
