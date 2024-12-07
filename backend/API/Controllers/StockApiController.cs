using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Contracts;
using Core.Service.StockApi;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockApiController : ControllerBase
    {
        private readonly IStockApiService _stockApiService;

        public StockApiController(IStockApiService stockApiService)
        {
            _stockApiService = stockApiService;
        }

        // GET: api/StockApi/price?symbol=AAPL
        [HttpGet("price")]
        public async Task<IActionResult> GetStockPrice(string symbol = "AAPL")
        {
            if (string.IsNullOrEmpty(symbol))
            {
                return BadRequest("Stock symbol is required.");
            }

            try
            {
                var priceNullable = await _stockApiService.GetStockPriceAsync(symbol);
                if (priceNullable == null)
                {
                    return NotFound($"No price data found for symbol: {symbol}");
                }

                var price = (decimal)priceNullable; // Convert double? to decimal
                return Ok(new { Symbol = symbol, Price = price });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error retrieving stock price: {ex.Message}");
            }
        }

        // GET: api/StockApi/market-news?category=general
        [HttpGet("market-news")]
        public async Task<IActionResult> GetMarketNews([FromQuery] MarketNewsRequest request)
        {
            if (string.IsNullOrEmpty(request.Category))
            {
                return BadRequest("Category is required.");
            }

            try
            {
                // Default `MinId` to 0 if null, ensuring non-null value
                var news = await _stockApiService.GetMarketNewsAsync(request.Category, request.MinId ?? 0);

                // Apply pagination
                var pagedNews = news
                    .Skip((request.Page.GetValueOrDefault(1) - 1) * request.PageSize.GetValueOrDefault(10))
                    .Take(request.PageSize.GetValueOrDefault(10))
                    .ToList();

                var totalRecords = news.Count;

                var response = new
                {
                    Data = pagedNews,
                    TotalRecords = totalRecords,
                    Page = request.Page ?? 1,
                    PageSize = request.PageSize ?? 10,
                    TotalPages = (int)Math.Ceiling((double)totalRecords / (request.PageSize ?? 10))
                };

                return Ok(response);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error retrieving market news: {ex.Message}");
            }
        }

        // GET: api/StockApi/company-news?symbol=AAPL&from=2023-08-15&to=2023-08-20
        [HttpGet("company-news")]
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client, VaryByQueryKeys = new[] { "symbol", "from", "to" })]
        public async Task<IActionResult> GetCompanyNews([FromQuery] CompanyNewsRequest request)
        {
            if (string.IsNullOrEmpty(request.Symbol) || string.IsNullOrEmpty(request.From) || string.IsNullOrEmpty(request.To))
            {
                return BadRequest("Symbol, from date, and to date are required.");
            }

            try
            {
                var news = await _stockApiService.GetCompanyNewsAsync(request.Symbol, request.From, request.To);

                // Apply pagination
                var pagedNews = news
                    .Skip((request.Page.GetValueOrDefault(1) - 1) * request.PageSize.GetValueOrDefault(10))
                    .Take(request.PageSize.GetValueOrDefault(10))
                    .ToList();

                var totalRecords = news.Count;

                var response = new
                {
                    Data = pagedNews,
                    TotalRecords = totalRecords,
                    Page = request.Page ?? 1,
                    PageSize = request.PageSize ?? 10,
                    TotalPages = (int)Math.Ceiling((double)totalRecords / (request.PageSize ?? 10))
                };

                return Ok(response);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error retrieving company news: {ex.Message}");
            }
        }
    }
}
