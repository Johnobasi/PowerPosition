using Microsoft.Extensions.Options;
using PowerPosition.Interfaces;
using PowerPosition.Models;
using Services;
using System.Globalization;
using System.Text;

namespace PowerPosition
{
    public class PowerPositionService(ILogger<PowerPositionService> logger, IOptions<Settings> settings, IPowerServiceClient powerService) : IPowerPositionService
    {
        private readonly ILogger<PowerPositionService> _logger = logger;
        private readonly Settings _settings = settings.Value;
        private readonly IPowerServiceClient _powerServiceClient = powerService;

        public async Task GenerateAsync()
        {

            try
            {

                _logger.LogInformation("=== Starting Power Position generation cycle ===");

                DateTime currentLondonTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/London"));
                DateTime tradeDate = currentLondonTime.Date.AddDays(1);
                _logger.LogInformation("Local London time: {CurrentTime}. Processing trade date: {TradeDate:yyyy-MM-dd}.",
                    currentLondonTime, tradeDate);
                
                var powerTrades = await _powerServiceClient.GetTradesAsync(tradeDate) ?? new List<PowerTrade>();
                if (!powerTrades.Any())
                {
                    _logger.LogWarning("No trades were returned for {TradeDate}. " +
                        "CSV will still be generated with zero volumes.", tradeDate.ToString("yyyy-MM-dd HH:mm:ss"));
                }
                else
                {
                    _logger.LogInformation("Fetched {TradeCount} trades successfully for {TradeDate:yyyy-MM-dd}.",
                        powerTrades.Count(), tradeDate);
                }

                var allTradePeriods = powerTrades.SelectMany(t => t.Periods).ToList();
                _logger.LogDebug("Flattened all trade periods: total {PeriodCount} records before aggregation.", 
                    allTradePeriods.Count);

                var hourlyAggregatedPositions = allTradePeriods
                    .GroupBy(p => p.Period)
                    .Select(g => new { Period = g.Key, Volume = g.Sum(x => x.Volume) })
                    .OrderBy(x => x.Period)
                    .ToList();

                // Period 1 starts at 23:00 previous day
                DateTime powerDayStartTime = new DateTime(tradeDate.Year, tradeDate.Month, tradeDate.Day, 23, 0, 0).AddDays(-1);

                var csvBuilder = new StringBuilder();
                csvBuilder.AppendLine("Local Time,Volume");

                foreach (var item in hourlyAggregatedPositions)
                {
                    var periodStartTime = powerDayStartTime.AddHours(item.Period -1);
                    var hourlyVolume = Math.Round(item.Volume, 2, MidpointRounding.AwayFromZero);
                    csvBuilder.AppendLine($"{periodStartTime:HH:mm},{hourlyVolume.ToString(CultureInfo.InvariantCulture)}");
                }

                // Write to CSV
                _logger.LogDebug("CSV content built with {RowCount} rows (including header).", hourlyAggregatedPositions.Count + 1);
                Directory.CreateDirectory(_settings.OutputDirectory);
                var reportFileName = $"PowerPosition_{tradeDate:yyyyMMdd}_{currentLondonTime:HHmm}.csv";
                var reportFilePath = Path.Combine(_settings.OutputDirectory, reportFileName);

                await File.WriteAllTextAsync(reportFilePath, csvBuilder.ToString(), Encoding.UTF8);
                _logger.LogInformation("Generated CSV file: {filePath}", reportFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Power Position report");
                throw;
            }            
        }
    }
}
