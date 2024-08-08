using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Data.Entity.EntityBase;
namespace Core.Data.Entity
{
    /*
     Table orderProccess{
    id uuid [pk]
    orderId uuid [ref: > order.id]
    result bool
}
     */
    internal class OrderProcess : IEntityBase
    {
        public Guid OrderId { get; set; }
        public Order Order { get; set; }
        public bool Result { get; set; }
    }
}
