namespace laba;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Warning);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<IStrategy, AlwaysAttendStrategy>();

                services.AddSingleton<ISimulationEngine, SimulationEngine>();

                services.AddHostedService<SemesterWorker>();
                services.AddHostedService<KeyboardListenerService>();
            });

        var host = builder.Build();
        await host.RunAsync();
    }
}