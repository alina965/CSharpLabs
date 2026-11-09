namespace laba;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class Program
{
    public static async Task Main(string[] args)
    {
        if (args.Contains("--console"))
        {
            var engine = new SimulationEngine(new AlwaysAttendStrategy());
            DayResult result = null!;
            for (var day = 0; day < 100; day++)
            {
                result = engine.Step();
                if (result.Expelled) break;
            }
            Console.WriteLine($"Дней: {result.Day}. Общее удовольствие: {result.TotalPleasure}. Среднее в день: {result.AveragePleasurePerDay:F2}. Отчислен: {result.Expelled}");
            return;
        }
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

        using var host = builder.Build();
        await host.RunAsync();
    }
}
