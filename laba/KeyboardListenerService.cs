using Microsoft.Extensions.Hosting;

namespace laba;

public class KeyboardListenerService : BackgroundService
{
    private readonly IHostApplicationLifetime _lifetime;

    public KeyboardListenerService(IHostApplicationLifetime lifetime)
    {
        _lifetime = lifetime;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (Console.KeyAvailable)
                {
                    Console.ReadKey(intercept: true);
                    Console.WriteLine("\n[Получен сигнал от пользователя] Выполнение прервано нажатием клавиши.");
                    _lifetime.StopApplication();
                    break;
                }

                Thread.Sleep(200);
            }
        }, stoppingToken);
    }
}