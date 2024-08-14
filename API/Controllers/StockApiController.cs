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
        public async Task<IActionResult> GetStockPrice(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
            {
                return BadRequest("Stock symbol is required.");
            }

            try
            {
                var price = await _stockApiService.GetStockPriceAsync(symbol);
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
                var news = await _stockApiService.GetMarketNewsAsync(request.Category, request.MinId);
                return Ok(news);
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
                return Ok(news);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error retrieving company news: {ex.Message}");
            }
        }
    }
}