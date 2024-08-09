using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        // GET: api/ExternalApiStock/price?symbol=AAPL
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
    }

}