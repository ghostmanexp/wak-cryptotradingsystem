using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace RatesService.Application.Services
{
    public interface ICoinMarketCapClient
    {
        Task<CoinMarketCapResponse> GetLatestListingsAsync();
    }

    public class CoinMarketCapClient : ICoinMarketCapClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public CoinMarketCapClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["CoinMarketCap:ApiKey"] ?? throw new ArgumentNullException("CoinMarketCap:ApiKey");
            _httpClient.DefaultRequestHeaders.Add("X-CMC_PRO_API_KEY", _apiKey);
        }

        public async Task<CoinMarketCapResponse> GetLatestListingsAsync()
        {
            var response = await _httpClient.GetAsync("v1/cryptocurrency/listings/latest?convert=USD");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CoinMarketCapResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }

    public class CoinMarketCapResponse
    {
        public List<CryptoCurrency> Data { get; set; }
    }

    public class CryptoCurrency
    {
        public string Symbol { get; set; }
        public Quote Quote { get; set; }
    }

    public class Quote
    {
        public UsdQuote USD { get; set; }
    }

    public class UsdQuote
    {
        public decimal Price { get; set; }
    }
}