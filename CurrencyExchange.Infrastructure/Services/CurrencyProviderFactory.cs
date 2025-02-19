using CurrencyExchange.Application.Interfaces;
using static CurrencyExchange.Application.Common.Enumerator;

namespace CurrencyExchange.Infrastructure.Services
{
    public class CurrencyProviderFactory : ICurrencyProviderFactory
    {
        private readonly ICurrencyConverter _currencyConverter;
        public CurrencyProviderFactory(ICurrencyConverter currencyConverter)
        {
            _currencyConverter = currencyConverter;
        }
        public ICurrencyConverter GetProvider(Providers ProviderName)
        {
            if (ProviderName == Providers.frankfurter)
            {
                return _currencyConverter;
            }
            throw new NotImplementedException() ;
        }
    }
}
