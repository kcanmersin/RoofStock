using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Core.Service.StockApi;
using Microsoft.Extensions.Configuration;

public class StockApiService : IStockApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public StockApiService(HttpClient httpClient, IConfiguration configuration)
    {
        // Ortam deðiþkeninden veya appsettings'ten API anahtarýný alýyoruz
        _httpClient = httpClient;
        _apiKey = Environment.GetEnvironmentVariable("STOCKAPI_APIKEY")
                  ?? configuration["StockApiSettings:ApiKey"];
    }

    public async Task<decimal> GetStockPriceAsync(string symbol)
    {
        // API anahtarýný doðru þekilde request URI'ya yerleþtiriyoruz
        var requestUri = $"quote?symbol={symbol}&token={_apiKey}";

        var response = await _httpClient.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var stockData = JsonSerializer.Deserialize<StockApiResponse>(content);

        return stockData?.c ?? 0;
    }
}

public class StockApiResponse
{
    public decimal c { get; set; } // Current price
    public decimal d { get; set; } // Change
    public decimal dp { get; set; } // Percent change
    public decimal h { get; set; } // High price of the day
    public decimal l { get; set; } // Low price of the day
    public decimal o { get; set; } // Open price of the day
    public decimal pc { get; set; } // Previous close price
}
