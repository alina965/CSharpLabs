using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace laba;

public class DbMode
{
    public static async Task<SimulationEngine> RestoreAsync(
        SemesterRepository repository,
        Guid sessionId,
        IStrategy strategy)
    {
        var state = await repository.LoadAsync(sessionId);

        if (state.Expelled || state.Day >= 100)
            throw new InvalidOperationException("Этот семестр уже завершён.");

        return SimulationEngine.Restore(strategy, state);
    }

    public static async Task RunAsync(string[] args)
    {
        var resume = args[0] == "--db-resume";

        if (resume &&
            (args.Length != 2 || !Guid.TryParse(args[1], out _)))
        {
            throw new ArgumentException("Использование: --db-resume <идентификатор семестра>");
        }

        if (!resume && args.Length != 1)
        {
            throw new ArgumentException("Использование: --db-new");
        }
        
        var connectionString =
            Environment.GetEnvironmentVariable("LABA_DB")
            ?? throw new InvalidOperationException(
                "Задай переменную окружения LABA_DB.");

        await using var dataSource =
            NpgsqlDataSource.Create(connectionString);
        
        var repository = new SemesterRepository(dataSource);
        await repository.InitializeAsync();
        
        IStrategy strategy = new AlwaysAttendStrategy();

        Guid sessionId;
        SimulationEngine engine;
        
        if (resume)
        {
            sessionId = Guid.Parse(args[1]);

            engine = await RestoreAsync(repository, sessionId, strategy);
            var state = engine.Export();

            Console.WriteLine(
                $"Восстановлен день {state.Day}. " +
                $"Следующий день: {state.Day + 1}.");
        }
        else
        {
            sessionId = Guid.NewGuid();
            engine = new SimulationEngine(strategy);

            await repository.SaveAsync(sessionId, engine.Export());
        }
        
        Console.WriteLine($"Семестр: {sessionId}");
        Console.WriteLine("Для остановки нажми любую клавишу.");

        var builder = Host.CreateApplicationBuilder();
        
        builder.Services.AddSingleton(engine);
        builder.Services.AddSingleton(repository);
        builder.Services.AddSingleton(new DbSession(sessionId));

        builder.Services.AddHostedService<DbSemesterWorker>();
        builder.Services.AddHostedService<KeyboardListenerService>();

        using var host = builder.Build();

        await host.RunAsync();
    }
}
