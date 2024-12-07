using System;
using MediatR;
using Core.Shared;
using Core.Data.Entity;

namespace Core.Features.GiveOrder
{
    public class OrderCommand : IRequest<Result<OrderResponse>>
    {
        public Guid UserId { get; set; }
        public string StockSymbol { get; set; }
        public int Quantity { get; set; }
        public decimal TargetPrice { get; set; }
        public OrderType OrderType { get; set; }

        public OrderCommand(Guid userId, string stockSymbol, int quantity, decimal targetPrice, OrderType orderType)
        {
            UserId = userId;
            StockSymbol = stockSymbol;
            Quantity = quantity;
            TargetPrice = targetPrice;
            OrderType = orderType;
        }
    }
}
