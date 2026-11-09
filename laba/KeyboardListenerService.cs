using Microsoft.Extensions.Hosting;

namespace laba;

public class KeyboardListenerService(IHostApplicationLifetime lifetime) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (Console.IsInputRedirected) return;

        while (!stoppingToken.IsCancellationRequested)
        {
            if (Console.KeyAvailable)
            {
                Console.ReadKey(intercept: true);
                Console.WriteLine("\n[Получен сигнал от пользователя] Выполнение прервано нажатием клавиши.");
                lifetime.StopApplication();
                break;
            }

            await Task.Delay(200, stoppingToken);
        }
    }
}
