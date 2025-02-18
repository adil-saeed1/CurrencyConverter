using System.Text.Json;
using Microsoft.Extensions.Logging;
using CurrencyExchange.Application.Models;
using Microsoft.Extensions.Caching.Distributed;
using CurrencyExchange.Application.Interfaces;
using System.Net.Http;

namespace CurrencyExchange.Infrastructure.Services
{
    public class FrankFrutImplementation : ICurrencyConverter
    {
        private readonly IDistributedCache _cache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<FrankFrutImplementation> _logger;

        public FrankFrutImplementation(IDistributedCache cache, IHttpClientFactory httpClientFactory, ILogger<FrankFrutImplementation> logger)
        {
            _cache = cache;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        public async Task<Dictionary<string, decimal>> GetLatestRates(string baseCurrency)
        {
            var cacheKey = $"latest-rate-{baseCurrency}";
            var cachedRates = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedRates))
            {
                _logger.LogInformation("Returning cached exchange rates for {baseCurrency}", baseCurrency);
                return JsonSerializer.Deserialize<Dictionary<string, decimal>>(cachedRates);
            }

            var client = _httpClientFactory.CreateClient("FrankfurterClient");
            var response = await client.GetAsync($"latest?base={baseCurrency}");
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var rates = JsonSerializer.Deserialize<JsonElement>(jsonResponse).GetProperty("rates").Deserialize<Dictionary<string, decimal>>();

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(rates), cacheOptions);

            _logger.LogInformation("Fetched and cached latest exchange rates for {baseCurrency}", baseCurrency);
            return rates;

        }
        public async Task<decimal> ConvertCurrency(CurrencyConvertReq request)
        {

            var cacheKey = $"converted-rate-{request.FromCurrency}&symbols={request.ToCurrency}";
            var cachedConvertedRate = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedConvertedRate))
            {
                _logger.LogInformation("Returning cached converted rate for {baseCurrency}", request.FromCurrency);
                return Convert.ToDecimal(cachedConvertedRate);
            }

            var client = _httpClientFactory.CreateClient("FrankfurterClient");
            var response = await client.GetAsync($"latest?base={request.FromCurrency}&symbols={request.ToCurrency}");
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var rates = JsonSerializer.Deserialize<JsonElement>(jsonResponse).GetProperty("rates").Deserialize<Dictionary<string, decimal>>();

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            };
            rates.TryGetValue(request.ToCurrency,out decimal rate);
            await _cache.SetStringAsync(cacheKey,Convert.ToString(request.Amount * rate), cacheOptions);
            return request.Amount* rate;
        }
        public async Task<Dictionary<DateTime, Dictionary<string, decimal>>> GetHistoricalRates(string baseCurrency, DateTime startDate, DateTime endDate, int page, int pageSize)
        {
            var cacheKey = $"rate-history-{startDate:yyyy-MM-dd}..{endDate:yyyy-MM-dd}?base={baseCurrency}";
            var cachedHistory = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedHistory))
            {
                _logger.LogInformation("Returning cacehed history rates for {baseCurrency}", baseCurrency);
                var  cahcedData = JsonSerializer.Deserialize<Dictionary<DateTime, Dictionary<string, decimal>>>(cachedHistory) ;
                return cahcedData.Skip((page - 1) * pageSize).Take(pageSize).ToDictionary(k => k.Key, v => v.Value);
            }

            var client = _httpClientFactory.CreateClient("FrankfurterClient");
            var response = await client.GetAsync($"{startDate:yyyy-MM-dd}..{endDate:yyyy-MM-dd}?base={baseCurrency}");
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var rates = JsonSerializer.Deserialize<JsonElement>(jsonResponse).GetProperty("rates").Deserialize<Dictionary<DateTime, Dictionary<string, decimal>>>();
           
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(rates), cacheOptions);
            return rates.Skip((page - 1) * pageSize).Take(pageSize).ToDictionary(k => k.Key, v => v.Value);
        }

    }
}
