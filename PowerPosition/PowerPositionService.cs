using Microsoft.Extensions.Options;
using PowerPosition.Interfaces;
using PowerPosition.Models;

namespace PowerPosition
{
    internal class PowerPositionService : IPowerPositionService
    {
        private readonly ILogger<PowerPositionService> _logger;
        private readonly Settings _settings;
        public PowerPositionService(ILogger<PowerPositionService> logger, IOptions<Settings> settings)
        {
            _logger = logger;
            _settings = settings.Value;
        }
        public Task GenerateAsync()
        {
            throw new NotImplementedException();
        }
    }
}
