using Core.Data.Entity;
using Core.Data.Entity.EntityBases;
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
    public class Order : EntityBase
    {
        public string StockSymbol { get; set; }
        public int Quantity { get; set; }
        public decimal TargetPrice { get; set; }

    }


}
