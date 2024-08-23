using Core.Service.StockRecommendationService;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Features.GetStockRecommendations
{
    internal class GetStockRecommendationsHandler : IRequestHandler<GetStockRecommendationsCommand, List<StockRecommendationResponse>>
    {
        private readonly StockRecommendationService _stockRecommendationService;

        public GetStockRecommendationsHandler(StockRecommendationService stockRecommendationService)
        {
            _stockRecommendationService = stockRecommendationService;
        }

        public async Task<List<StockRecommendationResponse>> Handle(GetStockRecommendationsCommand request, CancellationToken cancellationToken)
        {
            var recommendations = await _stockRecommendationService.GetRecommendationsAsync(request.UserId);

            return recommendations.Select(r => new StockRecommendationResponse
            {
                Ticker = r.Ticker,
                Score = r.Score
            }).ToList();
        }
    }
}
