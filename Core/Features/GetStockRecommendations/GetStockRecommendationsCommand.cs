using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Features.GetStockRecommendations
{
    public class GetStockRecommendationsCommand : IRequest<List<StockRecommendationResponse>>
    {
        public Guid UserId { get; set; }

        public GetStockRecommendationsCommand(Guid userId)
        {
            UserId = userId;
        }
    }
}
