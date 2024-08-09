using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

    public class CurrencyConversionService
    {
        private readonly HttpClient _httpClient;

        public CurrencyConversionService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<decimal> ConvertFromUSD(decimal amount, string toCurrency)
{
    if (toCurrency.ToUpper() == "USD")
    {
        return amount;
    }

    var response = await _httpClient.GetAsync($"https://api.frankfurter.app/latest?amount={amount}&from=USD&to={toCurrency}");

    if (!response.IsSuccessStatusCode)
    {
        throw new Exception("Currency conversion failed");
    }

    var responseContent = await response.Content.ReadAsStringAsync();
    var conversionResult = JsonSerializer.Deserialize<CurrencyConversionResult>(responseContent);

    if (conversionResult != null && conversionResult.Rates != null && conversionResult.Rates.ContainsKey(toCurrency.ToUpper()))
    {
        return conversionResult.Rates[toCurrency.ToUpper()];
    }
    else
    {
        throw new Exception($"{toCurrency} rate not found in the conversion result.");
    }
}


        public async Task<decimal> ConvertToUSD(decimal amount, string fromCurrency)
        {
        if (fromCurrency.ToUpper() == "USD")
        {
            return amount;
        }

        var response = await _httpClient.GetAsync($"https://api.frankfurter.app/latest?amount={amount}&from={fromCurrency}&to=USD");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Currency conversion failed");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var conversionResult = JsonSerializer.Deserialize<CurrencyConversionResult>(responseContent);

            if (conversionResult != null && conversionResult.Rates != null && conversionResult.Rates.ContainsKey("USD"))
            {
                // The API returns the amount converted to USD directly
                return conversionResult.Rates["USD"];
            }
            else
            {
                throw new Exception("USD rate not found in the conversion result.");
            }
        }
    }
