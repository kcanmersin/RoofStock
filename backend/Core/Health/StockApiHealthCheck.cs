using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public class StockApiHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly string _testSymbol;
    private readonly string _apiKey;
    private readonly string _baseUrl;

    public StockApiHealthCheck(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _testSymbol = configuration["StockApiSettings:TestSymbol"] ?? "AAPL";
        _apiKey = Environment.GetEnvironmentVariable("STOCKAPI_APIKEY") ?? configuration["StockApiSettings:ApiKey"];
        _baseUrl = Environment.GetEnvironmentVariable("STOCKAPI_BASEURL") ?? configuration["StockApiSettings:BaseUrl"];

    }
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var requestUrl = $"{_baseUrl.TrimEnd('/')}/quote?symbol={_testSymbol}&token={_apiKey}";

        var details = new Dictionary<string, object>
    {
        { "Url", requestUrl },
        { "ApiKey", _apiKey },
        { "TestSymbol", _testSymbol }
    };

        try
        {
            var response = await _httpClient.GetAsync(requestUrl, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(content))
                {
                    return HealthCheckResult.Healthy("Stock API is running and accessible.", details);
                }
                else
                {
                    return HealthCheckResult.Unhealthy("Stock API is accessible, but the response was empty.", data: details);
                }
            }
            else
            {
                return HealthCheckResult.Unhealthy($"Stock API is not accessible. Status code: {response.StatusCode}", data: details);
            }
        }
        catch (HttpRequestException ex)
        {
            return HealthCheckResult.Unhealthy($"Stock API is not accessible. Exception: {ex.Message}", data: details);
        }
    }

}