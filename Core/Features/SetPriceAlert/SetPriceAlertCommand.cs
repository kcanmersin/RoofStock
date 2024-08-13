using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Data.Entity;
using Core.Features.SetPriceAlert;
using Core.Shared;
using MediatR;

namespace Core.Features.GivePriceAlert
{
    public class SetPriceAlertCommand : IRequest<Result<SetPriceAlertResponse>>
    {
      public Guid UserId { get; set; }
        public string StockSymbol { get; set; } = string.Empty;
        public decimal TargetPrice { get; set; }
        public AlertType AlertType { get; set; }
    }
}