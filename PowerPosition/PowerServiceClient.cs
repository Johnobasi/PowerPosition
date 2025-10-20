using PowerPosition.Interfaces;
using Services;

namespace PowerPosition
{
    public class PowerServiceClient(PowerService powerService, ILogger<PowerServiceClient> logger) : IPowerServiceClient
    {
        private readonly PowerService _powerService = powerService;
        private readonly ILogger<PowerServiceClient> _logger = logger;

        public async Task<IEnumerable<PowerTrade>> GetTradesAsync(DateTime tradeDate)
        {
            _logger.LogInformation("Getting trades from client");
            return await _powerService.GetTradesAsync(tradeDate);
        }
    }
}
