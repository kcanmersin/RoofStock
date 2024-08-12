using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Data.Entity.EntityBases;
namespace Core.Data.Entity
{
    /*
     Table orderProccess{
    id uuid [pk]
    orderId uuid [ref: > order.id]
    result bool
}
     */
    public class OrderProcess : EntityBase
    {
        public Guid OrderId { get; set; }
        public Order Order { get; set; }
        // public bool Result { get; set; }
        //status enum
        public OrderProcessStatus Status { get; set; }
    }

    public enum OrderProcessStatus
    {
    Pending,
    Completed,
    Failed,
    Canceled,
    InProgress
    }
}
