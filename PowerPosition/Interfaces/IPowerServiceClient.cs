using Services;

namespace PowerPosition.Interfaces
{
    public interface IPowerServiceClient
    {
        Task<IEnumerable<PowerTrade>> GetTradesAsync(DateTime tradeDate);
    }
}
