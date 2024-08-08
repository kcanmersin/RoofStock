using Core.Data.Entity.EntityBase;
using Core.Data.Entity.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data.Entity
{
    /*
     Table stock_holdings {
  id uuid [pk]
  user_id uuid [ref: > users.id]
  stock_symbol varchar
  quantity int
  total_purchase_price decimal
}*/
    internal class StockHolding : IEntityBase
    {
        public Guid UserId { get; set; }
        public virtual AppUser User { get; set; }

        public string StockSymbol {get; set; }
        public int Quantity { get; set; }
        public decimal TotalPurchasePrice { get; set; }

    }
}
