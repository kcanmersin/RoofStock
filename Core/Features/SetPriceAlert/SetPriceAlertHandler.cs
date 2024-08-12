using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Data;
using Core.Data.Entity;
using Core.Features.GivePriceAlert;
using Core.Shared;
using MediatR;

namespace Core.Features.SetPriceAlert
{
    public class SetPriceAlertHandler : IRequestHandler<SetPriceAlertCommand, Result<SetPriceAlertResponse>>
    {
        private readonly ApplicationDbContext _context;

        public SetPriceAlertHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<SetPriceAlertResponse>> Handle(SetPriceAlertCommand request, CancellationToken cancellationToken)
        {
            var alert = new StockPriceAlert
            {
                UserId = request.UserId,
                StockSymbol = request.StockSymbol,
                TargetPrice = request.TargetPrice,
                IsTriggered = false,
                CreatedDate = DateTime.UtcNow
            };

            _context.StockPriceAlerts.Add(alert);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(new SetPriceAlertResponse
            {
                IsSuccess = true,
                Message = $"Price alert set for {request.StockSymbol} at {request.TargetPrice:C}."
            });
        }
    }
}