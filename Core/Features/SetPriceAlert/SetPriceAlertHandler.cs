using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Data;
using Core.Data.Entity;
using Core.Features.GivePriceAlert;
using Core.Shared;
using MediatR;
using Core.Service.StockApi;

namespace Core.Features.SetPriceAlert
{
    public class SetPriceAlertHandler : IRequestHandler<SetPriceAlertCommand, Result<SetPriceAlertResponse>>
    {
        private readonly ApplicationDbContext _context;
        private readonly IStockApiService _stockApiService;

        public SetPriceAlertHandler(ApplicationDbContext context, IStockApiService stockApiService)
        {
            _context = context;
            _stockApiService = stockApiService;
        }

        public async Task<Result<SetPriceAlertResponse>> Handle(SetPriceAlertCommand request, CancellationToken cancellationToken)
        {
            // Retrieve the current stock price
            var currentPrice = await _stockApiService.GetStockPriceAsync(request.StockSymbol);

            var alert = new StockPriceAlert
            {
                UserId = request.UserId,
                StockSymbol = request.StockSymbol,
                TargetPrice = request.TargetPrice,
                AlertType = request.AlertType,
                IsTriggered = false,
                CreatedDate = DateTime.UtcNow
            };

            _context.StockPriceAlerts.Add(alert);
            await _context.SaveChangesAsync(cancellationToken);

            // Determine the alert type as a string
            string alertTypeDescription = request.AlertType == AlertType.Rise ? "rise" : "fall";

            return Result.Success(new SetPriceAlertResponse
            {
                IsSuccess = true,
                Message = $"Price alert set for {request.StockSymbol} at {request.TargetPrice:C}. Current price is {currentPrice:C}. Alert type is for a {alertTypeDescription}."
            });
        }
    }
}
