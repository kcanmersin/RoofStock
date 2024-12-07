using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Shared;
using MediatR;

namespace Core.Features.BuyStock
{
    public class BuyStockCommand : IRequest<Result<BuyStockResponse>>
    {
        public Guid UserId { get; set; }
        public string StockSymbol { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}