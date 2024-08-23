using Microsoft.EntityFrameworkCore;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Core.Data.Entity;
using Core.Data;

namespace Core.Service.StockRecommendationService
{
    internal class StockRecommendationService
    {
        private readonly ApplicationDbContext _context;
        private readonly InferenceSession _session;

        public StockRecommendationService(ApplicationDbContext context)
        {
            _context = context;

            var modelPath = Path.Combine(Environment.CurrentDirectory, "Models", "stock_recommendation_model.onnx");
            _session = new InferenceSession(modelPath);
        }

        public async Task<List<StockRecommendation>> GetRecommendationsAsync(Guid userId)
        {
            var transactions = await _context.StockHoldings
                .Where(t => t.UserId == userId)
                .ToListAsync();

            var tickers = transactions.Select(t => t.StockSymbol).Distinct().ToList();

            var recommendations = tickers.Select(ticker => new StockRecommendation
            {
                Ticker = ticker,
                Score = Predict(userId.ToString(), ticker)
            })
            .OrderByDescending(x => x.Score)
            .Take(3) 
            .ToList();

            return recommendations;
        }

        private float Predict(string userId, string ticker)
        {
            float userIdEncoded = EncodeUserId(userId);
            float tickerEncoded = EncodeTicker(ticker);

            var inputData = new DenseTensor<float>(new[] { userIdEncoded, tickerEncoded }, new[] { 1, 2 });

            var input = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("float_input", inputData)
            };

            using var results = _session.Run(input);

            var score = results.First().AsEnumerable<float>().First();
            return score;
        }

        private float EncodeUserId(string userId)
        {
            return (float)userId.GetHashCode();
        }

        private float EncodeTicker(string ticker)
        {
            return (float)ticker.GetHashCode();
        }
    }

    public class StockRecommendation
    {
        public string Ticker { get; set; }
        public float Score { get; set; }
    }
}
