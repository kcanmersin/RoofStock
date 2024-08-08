using Core.Data.Entity.EntityBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data.Entity
{
    /*
     Table stockholdingitem{
 id uuid [pk]
  stock_symbol varchar
  quantity int
  unitprice decimal//USD
  type enum //al -sat
  orderProcessId   uuid [ref: > orderProccess.id]
}
     */
    internal class StockHoldingItem : IEntityBase
    {

        public string StockSymbol { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; } //USD
        public StockHoldingItemType Type { get; set; }
        public Guid OrderProcessId { get; set; }
        public OrderProcess OrderProcess { get; set; }
    }

    public enum StockHoldingItemType
    {
        Purchase,
        Sale
    }
}
