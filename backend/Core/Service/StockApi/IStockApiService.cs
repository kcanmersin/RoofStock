using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Service.StockApi
{
    public interface IStockApiService
    {
        Task<decimal> GetStockPriceAsync(string symbol);
        Task<List<MarketNewsResponse>> GetMarketNewsAsync(string category, int? minId = null);
        Task<List<CompanyNewsResponse>> GetCompanyNewsAsync(string symbol, string fromDate, string toDate);
    }
}