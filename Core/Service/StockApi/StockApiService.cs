using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Core.Service.StockApi;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

public class StockApiService : IStockApiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
private readonly IMemoryCache _memoryCache;
    public StockApiService(HttpClient httpClient, IConfiguration configuration, IMemoryCache memoryCache)
    {
        _httpClient = httpClient;
        _apiKey = Environment.GetEnvironmentVariable("STOCKAPI_APIKEY")
                  ?? configuration["StockApiSettings:ApiKey"];
        _memoryCache = memoryCache;
        

    }

    public async Task<decimal> GetStockPriceAsync(string symbol)
    {
        var requestUri = $"quote?symbol={symbol}&token={_apiKey}";

        var response = await _httpClient.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var stockData = JsonSerializer.Deserialize<StockApiResponse>(content);

        return stockData?.c ?? 0;
    }


      public async Task<List<MarketNewsResponse>> GetMarketNewsAsync(string category, int? minId = null)
    {
        var cacheKey = $"MarketNews_{category}_{minId}";

        if (_memoryCache.TryGetValue(cacheKey, out List<MarketNewsResponse> cachedNews))
        {
            return cachedNews;
        }

        var requestUri = $"news?category={category}&token={_apiKey}";

        if (minId.HasValue)
        {
            requestUri += $"&minId={minId.Value}";
        }

        var response = await _httpClient.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var newsData = JsonSerializer.Deserialize<List<MarketNewsResponse>>(content);

        if (newsData != null)
        {
            _memoryCache.Set(cacheKey, newsData, TimeSpan.FromMinutes(10)); 
        }

        return newsData ?? new List<MarketNewsResponse>();
    }
    public async Task<List<CompanyNewsResponse>> GetCompanyNewsAsync(string symbol, string fromDate, string toDate)
    {
        var requestUri = $"company-news?symbol={symbol}&from={fromDate}&to={toDate}&token={_apiKey}";

        var response = await _httpClient.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var newsData = JsonSerializer.Deserialize<List<CompanyNewsResponse>>(content);

        return newsData ?? new List<CompanyNewsResponse>();
    }
}

// Market News Response Model
public class MarketNewsResponse
{
    [JsonPropertyName("category")]
    public string Category { get; set; }

    [JsonPropertyName("datetime")]
    public long Datetime { get; set; }

    [JsonPropertyName("headline")]
    public string Headline { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("image")]
    public string Image { get; set; }

    [JsonPropertyName("related")]
    public string Related { get; set; }

    [JsonPropertyName("source")]
    public string Source { get; set; }

    [JsonPropertyName("summary")]
    public string Summary { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }
}

// Company News Response Model
public class CompanyNewsResponse
{
    [JsonPropertyName("category")]
    public string Category { get; set; }

    [JsonPropertyName("datetime")]
    public long Datetime { get; set; }

    [JsonPropertyName("headline")]
    public string Headline { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("image")]
    public string Image { get; set; }

    [JsonPropertyName("related")]
    public string Related { get; set; }

    [JsonPropertyName("source")]
    public string Source { get; set; }

    [JsonPropertyName("summary")]
    public string Summary { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }
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
