using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Core.Data.Entity;

namespace API.Contracts
{
    public class SetPriceAlertRequest
    {
        [DefaultValue("3aa42229-1c0f-4630-8c1a-db879ecd0427")]
        public Guid UserId { get; set; }
        [DefaultValue("AAPL")]
        public string StockSymbol { get; set; } = string.Empty;
        [DefaultValue(100.00)]
        public decimal TargetPrice { get; set; }
        [DefaultValue("Rise")]
        public AlertType AlertType { get; set; }


    }
}