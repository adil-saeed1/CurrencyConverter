using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CurrencyExchange.Application.Models;
using CurrencyExchange.Application.Interfaces;

namespace CurrencyConverter.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExchangeController : ControllerBase
    {
        private readonly ILogger<ExchangeController> _logger;
        private readonly ICurrencyConverter _service;

        public ExchangeController(ICurrencyConverter currencyConverter, ILogger<ExchangeController> logger)
        {
            _service = currencyConverter;
            _logger = logger;
        }
        [HttpGet("/api/latest")]
        [Authorize(Roles = "Admin,Guest")]
        public async Task<IActionResult> GetLatestRates(string baseCurrency)
        {
            var rates = await _service.GetLatestRates(baseCurrency);
            return Ok(rates);
        }
        [Authorize(Policy = "AdminPolicy")]
        [HttpPost("/api/convert")]
        public IActionResult ConvertCurrency([FromBody] CurrencyConvertReq request)
        {
            var result = _service.ConvertCurrency(request);
            return Ok(result);
        }
        [Authorize(Policy = "AdminPolicy")]
        [HttpGet("/api/history")]
        public async Task<IActionResult> GetHistoricalRates(string baseCurrency, DateTime startDate, DateTime endDate, int page, int pageSize)
        {
            var rates = await _service.GetHistoricalRates(baseCurrency, startDate, endDate, page, pageSize);
            return Ok(rates);
        }
    }
}
