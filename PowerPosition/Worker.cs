using Microsoft.Extensions.Options;
using PowerPosition.Interfaces;
using PowerPosition.Models;

namespace PowerPosition
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IPowerPositionService _powerPositionService;
        private readonly Settings _settings;

        public Worker(ILogger<Worker> logger, IPowerPositionService service, IOptions<Settings> options)
        {
            _logger = logger;
            _powerPositionService = service;
            _settings = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Window service started with extraction interval: {Interval} minute(s)",
                _settings.ExtractIntervalMinutes);

            await RunSafeAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var nextRunTime = DateTimeOffset.Now.AddMinutes(_settings.ExtractIntervalMinutes);
                    _logger.LogInformation(
                        "Next extract scheduled for {NextRunTime} (in {Interval} minute(s))",
                        nextRunTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        _settings.ExtractIntervalMinutes);

                    _logger.LogDebug("Window service entering sleep state until next scheduled run...");


                    await Task.Delay(TimeSpan.FromMinutes(_settings.ExtractIntervalMinutes), stoppingToken);
                    await RunSafeAsync();

                    _logger.LogInformation("Woke up at {CurrentTime}, initiating next extract.",
                      DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                }
                catch (TaskCanceledException)
                {
                    _logger.LogWarning("Task delay canceled — window service is shutting down gracefully...");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in Window service loop");
                }
                _logger.LogInformation("Window service execution stopped at {Time}", DateTimeOffset.Now);
            }
        }

        private async Task RunSafeAsync()
        {
            try
            {
                _logger.LogInformation("Starting scheduled extract at {time}", DateTimeOffset.Now);
                await _powerPositionService.GenerateAsync();
                _logger.LogInformation("Extract completed successfully at {time}", DateTimeOffset.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during power position generation");
            }
        }
    }
}
