using Microsoft.Extensions.Options;
using PowerPosition.Interfaces;
using PowerPosition.Models;

namespace PowerPosition
{
    public class Worker(ILogger<Worker> logger, IPowerPositionService service, IOptions<Settings> options) : BackgroundService
    {
        private readonly ILogger<Worker> _logger = logger;
        private readonly IPowerPositionService _powerPositionService = service;
        private readonly Settings _settings = options.Value;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Windows service started with extraction interval: {Interval} minute(s)",
                _settings.ExtractIntervalMinutes);

            _logger.LogInformation("Starting initial extract at {Time}", 
                DateTimeOffset.Now);

            await _powerPositionService.GenerateAsync();
            _logger.LogInformation("Initial extract completed at {Time}",
                DateTimeOffset.Now);

            var interval = TimeSpan.FromMinutes(_settings.ExtractIntervalMinutes);
            using var timer = new PeriodicTimer(interval);

            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    _logger.LogInformation("Starting scheduled extract at {Time}", DateTimeOffset.Now);
                    await _powerPositionService.GenerateAsync();
                    _logger.LogInformation("Extract completed at {Time}", DateTimeOffset.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during background extract");
                }
            }
        }
    }
}
