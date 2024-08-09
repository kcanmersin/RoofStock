using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Service.StockApi
{
    public interface IStockApiService
    {
        Task<decimal> GetStockPriceAsync(string symbol);
    }
}