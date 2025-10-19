using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PowerPosition.Interfaces;
using PowerPosition.Models;
using Services;

namespace PowerPosition.Test
{
    public class PowerPositionServiceTests
    {
        private readonly Mock<ILogger<IPowerPositionService>> _loggerMock;
        private readonly Mock<IOptions<Settings>> _settingsMock;
        private readonly Mock<PowerService> _powerServiceMock;
        public PowerPositionServiceTests()
        {
            _loggerMock = new Mock<ILogger<IPowerPositionService>>();
            _settingsMock = new Mock<IOptions<Settings>>();
            _powerServiceMock = new Mock<PowerService>();
        }

        [Fact]
        public void Test1()
        {

        }

        public static IEnumerable<object[]> TradeDateTestData =>
        [
            // London time, expected trade date
            [new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 15)], // Morning UTC
            [new DateTime(2024, 1, 15, 23, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 15)], // Evening UTC
            [new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 15)],  // Midnight UTC
        ];

        public static IEnumerable<object[]> PeriodToTimeMappingTestData =>
        [
            [1, "23:00"],  // Period 1 = 23:00 previous day
            [2, "00:00"],  // Period 2 = 00:00
            [3, "01:00"],  // Period 3 = 01:00
            [24, "22:00"], // Period 24 = 22:00
        ];

        public static IEnumerable<object[]> VolumeAggregationTestData =>
        [
            // periods data, expected aggregated volumes per period
            [
                new[]
                {
                    new { Period = 1, Volume = 100.5 },
                    new { Period = 1, Volume = 200.3 },
                    new { Period = 2, Volume = 150.0 }
                },
                new[]
                {
                    new { Period = 1, ExpectedVolume = 300.8 },
                    new { Period = 2, ExpectedVolume = 150.0 }
                }
            ],
            [
                new[]
                {
                    new { Period = 1, Volume = 50.123 },
                    new { Period = 1, Volume = 50.124 },
                    new { Period = 1, Volume = 50.125 },
                    new { Period = 3, Volume = 75.5 }
                },
                new[]
                {
                    new { Period = 1, ExpectedVolume = 150.37 }, // Rounded from 150.372
                    new { Period = 3, ExpectedVolume = 75.5 }
                }
            ]
        ];

        public static IEnumerable<object[]> EmptyTradesTestData =>
        [
            [null!],                    // Null trades
            [Array.Empty<PowerTrade>()] // Empty trades
        ];
    }
}