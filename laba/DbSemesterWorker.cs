using Microsoft.Extensions.Hosting;

namespace laba;

public record DbSession(Guid Id);

public class DbSemesterWorker(
    SimulationEngine engine,
    SemesterRepository repository,
    DbSession session,
    IHostApplicationLifetime lifetime) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        try
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

            while (await timer.WaitForNextTickAsync(ct))
            {
                ct.ThrowIfCancellationRequested();

                var result = engine.Step();

                using var saveTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                
                await repository.SaveAsync(
                    session.Id,
                    engine.Export(),
                    saveTimeout.Token);
                
                Console.WriteLine(
                    $"День {result.Day}: " +
                    $"удовольствие {result.TotalPleasure}, " +
                    $"среднее {result.AveragePleasurePerDay:F2}");

                if (result.Expelled || result.Day >= 100)
                {
                    Console.WriteLine(
                        result.Expelled
                            ? "Студент отчислен."
                            : "Семестр завершён.");

                    lifetime.StopApplication();
                    return; 
                }
            }
        }
        catch (OperationCanceledException)
            when (ct.IsCancellationRequested)
        {
            
        }
    }
}