using PowerPosition.Interfaces;
using PowerPosition.Models;
using Serilog;
using Services;

namespace PowerPosition
{
    public class Program
    {
        public static void Main(string[] args)
        {

            try
            {
                var builder = Host.CreateApplicationBuilder(args);

                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(builder.Configuration)
                    .CreateLogger();


                Log.Information("Starting PowerPosition window service...");
                builder.Services.Configure<Settings>(builder.Configuration.GetSection(nameof(Settings)));
                builder.Services.AddTransient<PowerService>();
                builder.Services.AddTransient<IPowerPositionService, PowerPositionService>();
                builder.Services.AddHostedService<Worker>();

                builder.Logging.ClearProviders();
                builder.Logging.AddSerilog();

                var host = builder.Build();
                host.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "PowerPosition window service terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}