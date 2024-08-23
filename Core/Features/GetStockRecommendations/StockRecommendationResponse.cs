using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Features.GetStockRecommendations
{
    public class StockRecommendationResponse
    {
        public string Ticker { get; set; }
        public float Score { get; set; }
    }

}
