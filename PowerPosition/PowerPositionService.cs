using Microsoft.Extensions.Options;
using PowerPosition.Interfaces;
using PowerPosition.Models;

namespace PowerPosition
{
    internal class PowerPositionService : IPowerPositionService
    {
        private readonly ILogger<PowerPositionService> _logger;
        private readonly Settings _settings;
        private readonly PowerPositionService _powerPositionService;
        public PowerPositionService(ILogger<PowerPositionService> logger, IOptions<Settings> settings)
        {
            _logger = logger;
            _settings = settings.Value;
            _powerPositionService = this;
        }
        public Task GenerateAsync()
        {
            throw new NotImplementedException();
        }
    }
}
