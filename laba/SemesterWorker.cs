using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace laba;

public class SemesterWorker : BackgroundService
{
    private readonly ISimulationEngine _engine;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<SemesterWorker> _logger;

    public SemesterWorker(
        ISimulationEngine engine, 
        IHostApplicationLifetime lifetime, 
        ILogger<SemesterWorker> logger)
    {
        _engine = engine;
        _lifetime = lifetime;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Симуляция запущена. Нажмите любую клавишу для прерывания...");

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            var result = _engine.Step();

            Console.WriteLine($"[День {result.Day:D3}] Общее удовольствие: {result.TotalPleasure} | Среднее в день: {result.AveragePleasurePerDay:F2}");

            if (result.Expelled)
            {
                Console.WriteLine("\n!!! Студент попался и был отчислен !!! Приложение останавливается.");
                _lifetime.StopApplication();
                break;
            }

            if (result.Day < 100) continue;
            Console.WriteLine("\nСеместр успешно завершён!");
            _lifetime.StopApplication();
            break;
        }
    }
}