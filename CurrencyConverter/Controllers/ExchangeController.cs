using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CurrencyExchange.Application.Models;
using CurrencyExchange.Application.Interfaces;
using static CurrencyExchange.Application.Common.Enumerator;

namespace CurrencyConverter.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/Exchange")]
    public class ExchangeController : ControllerBase
    {
        private readonly ILogger<ExchangeController> _logger;
        private readonly ICurrencyProviderFactory _currencyProviderFactory;

        public ExchangeController(ICurrencyProviderFactory currencyProviderFactory, ILogger<ExchangeController> logger)
        {
            _currencyProviderFactory = currencyProviderFactory;
            _logger = logger;
        }
        [HttpGet("latest")]
        [Authorize(Roles = "Admin,Guest")]
        public async Task<IActionResult> GetLatestRates(string baseCurrency)
        {
            var provider = _currencyProviderFactory.GetProvider(Providers.frankfurter);
            var rates = await provider.GetLatestRates(baseCurrency);
            return Ok(rates);
        }
        [Authorize(Roles = "Admin,Guest")]
        [HttpPost("convert")]
        public async Task<IActionResult> ConvertCurrency([FromBody] CurrencyConvertReq request)
        {
            var provider = _currencyProviderFactory.GetProvider(Providers.frankfurter);
            var result = await provider.ConvertCurrency(request);
            return Ok(new { ConvertedRate = result });
        }
        [Authorize(Roles = "Admin,Guest")]
        [HttpGet("history")]
        public async Task<IActionResult> GetHistoricalRates(string baseCurrency, DateTime startDate, DateTime endDate, int page, int pageSize)
        {
            var provider = _currencyProviderFactory.GetProvider(Providers.frankfurter);
            var rates = await provider.GetHistoricalRates(baseCurrency, startDate, endDate, page, pageSize);
            return Ok(rates);
        }
    }
}
