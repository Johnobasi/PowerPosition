using PowerPosition.Models;

namespace PowerPosition
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.Configure<Settings>(builder.Configuration.GetSection(nameof(Settings)));
            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.Run();
        }
    }
}