using Microsoft.Extensions.Options;
using PowerPosition.Models;
using Services;

namespace PowerPosition
{
    public class Worker(ILogger<Worker> logger, PowerPositionService service, IOptions<Settings> options) : BackgroundService
    {
        private readonly ILogger<Worker> _logger = logger;
        private readonly PowerPositionService _powerPositionService = service;
        private readonly Settings _settings = options.Value;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Window service started with extraction interval: {Interval} minute(s)",
                _settings.ExtractIntervalMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                var nextRunTime = DateTimeOffset.Now.AddMinutes(_settings.ExtractIntervalMinutes);
                _ = Task.Run(async () =>
                {
                    try
                    {
                        _logger.LogInformation("Starting scheduled extract at {Time}", DateTimeOffset.Now);
                        await _powerPositionService.GenerateAsync();
                        _logger.LogInformation("Extract completed successfully at {Time}", DateTimeOffset.Now);



                        _logger.LogInformation(
                            "Next extract scheduled for {NextRunTime} (in {Interval} minute(s))",
                            nextRunTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            _settings.ExtractIntervalMinutes);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during background extract");
                    }
                });

                await Task.Delay(TimeSpan.FromMinutes(_settings.ExtractIntervalMinutes), stoppingToken);
            }
        }
    }
}
