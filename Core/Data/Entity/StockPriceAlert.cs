using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Data.Entity.EntityBases;
using Core.Data.Entity.User;

namespace Core.Data.Entity
{
 internal class StockPriceAlert : EntityBase
    {
        public Guid UserId { get; set; }
        public virtual AppUser User { get; set; }

        public string StockSymbol { get; set; } 
        public decimal TargetPrice { get; set; } 
        public bool IsTriggered { get; set; } = false;
        public DateTime? TriggeredDate { get; set; }

        //alert type
        public AlertType AlertType { get; set; }


    }
    public enum AlertType
    {
        Fall,
        Rise
    }
}