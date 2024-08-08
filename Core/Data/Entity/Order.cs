using Core.Data.Entity.EntityBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Data.Entity
{
    /* 
     * 
     * Table order{
  id uuid [pk]
  stock_symbol varchar
  quantity int
  targetPrice decimal
 */
    internal class Order : IEntityBase
    {
        public string StockSymbol { get; set; }
        public int Quantity { get; set; }
        public decimal TargetPrice { get; set; }
    }
}
