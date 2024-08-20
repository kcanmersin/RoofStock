using Core.Data.Entity.EntityBases;
using Core.Data.Entity.User;
using System;

namespace Core.Data.Entity
{
    internal class Order : EntityBase
    {
        public string StockSymbol { get; set; }
        public int Quantity { get; set; }
        public decimal TargetPrice { get; set; }

        public Guid UserId { get; set; }
        public virtual AppUser User { get; set; }

              
        public virtual OrderProcess OrderProcess { get; set; }
        // Buy or Sell order type
        public OrderType OrderType { get; set; } 
    }

    public enum OrderType
    {
        Buy,
        Sell
    }
}
