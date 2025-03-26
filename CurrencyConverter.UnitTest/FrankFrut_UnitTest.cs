using Moq;
using Moq.Protected;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using CurrencyConverter.Infrastructure.Services;

namespace CurrencyConverter.Tests
{
    public class FrankFrut_UnitTest
    {
        private  Mock<IDistributedCache> _cacheMock;
        private  Mock<IHttpClientFactory> _httpClientFactoryMock;
        private  Mock<ILogger<FrankFrutImplementation>> _loggerMock;
        private  FrankFrutImplementation _service;
      
        [SetUp]
        public void Setup()
        {
            _cacheMock = new Mock<IDistributedCache>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _loggerMock = new Mock<ILogger<FrankFrutImplementation>>();
            _service = new FrankFrutImplementation(_cacheMock.Object, _httpClientFactoryMock.Object, _loggerMock.Object);
         
        }

        [Test]
        public void GetLatestRates_ShouldReturnMockedRates()
        {
            string baseCurrency = "USD";
            var apiResponse = new { rates = new Dictionary<string, decimal> {  { "GBP", 0.75m } } };
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
            var result =  _service.GetLatestRates(baseCurrency).Result;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.AreEqual(apiResponse.rates, result);

        }
    }
}