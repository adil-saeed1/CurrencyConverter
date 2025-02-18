using static CurrencyExchange.Application.Common.Enumerator;

namespace CurrencyExchange.Application.Interfaces
{
    public interface ICurrencyProviderFactory
    {
        ICurrencyConverter GetProvider(Providers ProviderName);
    }
}
