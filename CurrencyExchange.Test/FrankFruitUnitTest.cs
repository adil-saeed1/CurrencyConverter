using Moq;
using Xunit;
using Moq.Protected;
using NUnit.Framework;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using CurrencyExchange.Infrastructure.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace CurrencyExchange.Test
{
    public class FrankFrutImplementationUnitTest
    {
        private readonly Mock<IDistributedCache> _cacheMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<ILogger<FrankFrutImplementation>> _loggerMock;
        private readonly FrankFrutImplementation _service;

        public FrankFrutImplementationUnitTest()
        {
            _cacheMock = new Mock<IDistributedCache>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _loggerMock = new Mock<ILogger<FrankFrutImplementation>>();
            _service = new FrankFrutImplementation(_cacheMock.Object, _httpClientFactoryMock.Object, _loggerMock.Object);
        }
        [Fact]
        public async Task GetLatestRates_SimpleApiCall_ReturnsExpectedResult()
        {
            // Arrange
            string baseCurrency = "USD";
            var apiResponse = new { rates = new Dictionary<string, decimal> { { "EUR", 0.85m }, { "GBP", 0.75m } } };
            var jsonResponse = JsonSerializer.Serialize(apiResponse);

            var httpResponseMessage = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            };

            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<System.Threading.CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            var httpClient = new HttpClient(httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://api.frankfurter.app/")
            };

            _httpClientFactoryMock.Setup(f => f.CreateClient("FrankfurterClient")).Returns(httpClient);

            // Act
            var result = await _service.GetLatestRates(baseCurrency);

            // Assert
            Assert.That(result,Is.Not.Null);
            Assert.Equals(apiResponse.rates.Count, result.Count);
            Assert.Equals(apiResponse.rates, result);
        }

       
    }
}
