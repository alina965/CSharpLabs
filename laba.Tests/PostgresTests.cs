using laba;
using Npgsql;

namespace laba.Tests;

public class PostgresTests : IAsyncLifetime
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly SemesterRepository _repository;
    private readonly Guid _sessionId = Guid.NewGuid();

    public PostgresTests()
    {
        var connectionString = Environment.GetEnvironmentVariable("LABA_TEST_DB")
            ?? throw new InvalidOperationException("Задай переменную LABA_TEST_DB.");

        _dataSource = NpgsqlDataSource.Create(connectionString);
        _repository = new SemesterRepository(_dataSource);
    }

    public async Task InitializeAsync()
    {
        await _repository.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        try
        {
            await _repository.DeleteAsync(_sessionId);
        }
        finally
        {
            await _dataSource.DisposeAsync();
        }
    }

    private static SimulationEngine CreateEngine(IStrategy strategy)
    {
        var teachers = Enum.GetValues<Subject>()
            .Select(subject => new Teacher(subject, new TeacherRule(AskRuleKind.Random)))
            .ToArray();

        return new SimulationEngine(strategy, new ReplayRandom(42), teachers);
    }

    private static void AssertSameState(SemesterState expected, SemesterState actual)
    {
        Assert.Equal(expected.Day, actual.Day);
        Assert.Equal(expected.TotalPleasure, actual.TotalPleasure);
        Assert.Equal(expected.Expelled, actual.Expelled);
        Assert.Equal(expected.Teachers, actual.Teachers);
        Assert.Equal(expected.YesterdayAsked, actual.YesterdayAsked);
        Assert.Equal(expected.History, actual.History);
        Assert.Equal(expected.RandomSeed, actual.RandomSeed);
        Assert.Equal(expected.RandomDraws, actual.RandomDraws);
    }

    [Fact]
    public async Task SaveAndRestore_PreservesState()
    {
        var engine = CreateEngine(new AlwaysAttendStrategy());
        for (var day = 0; day < 5; day++)
            engine.Step();

        var expected = engine.Export();
        await _repository.SaveAsync(_sessionId, expected);

        var restored = await DbMode.RestoreAsync(
            _repository, _sessionId, new AlwaysAttendStrategy());

        AssertSameState(expected, restored.Export());
    }

    [Fact]
    public async Task Resume_MatchesContinuousSimulation()
    {
        var continuous = CreateEngine(new AlwaysAttendStrategy());
        var interrupted = CreateEngine(new AlwaysAttendStrategy());

        for (var day = 0; day < 5; day++)
        {
            continuous.Step();
            interrupted.Step();
            await _repository.SaveAsync(_sessionId, interrupted.Export());
        }

        var restored = await DbMode.RestoreAsync(
            _repository, _sessionId, new AlwaysAttendStrategy());

        Assert.Equal(5, restored.Export().Day);

        for (var day = 6; day <= 100; day++)
        {
            continuous.Step();
            restored.Step();
            AssertSameState(continuous.Export(), restored.Export());
        }
    }

    [Fact]
    public async Task Resume_AfterHundredDays_IsForbidden()
    {
        var engine = CreateEngine(new AlwaysAttendStrategy());
        for (var day = 0; day < 100; day++)
            engine.Step();

        await _repository.SaveAsync(_sessionId, engine.Export());
        var saved = await _repository.LoadAsync(_sessionId);

        Assert.Equal(100, saved.Day);
        Assert.Equal(100, saved.TotalPleasure);
        Assert.False(saved.Expelled);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            DbMode.RestoreAsync(_repository, _sessionId, new AlwaysAttendStrategy()));
    }

    [Fact]
    public async Task Resume_AfterExpulsion_IsForbidden()
    {
        var engine = CreateEngine(new SkipAfterFirstDayStrategy());
        Assert.Equal(1, engine.Step().TotalPleasure);

        for (var day = 2; day <= 100; day++)
        {
            if (engine.Step().Expelled)
                break;
        }

        Assert.True(engine.Export().Expelled);

        await _repository.SaveAsync(_sessionId, engine.Export());
        var saved = await _repository.LoadAsync(_sessionId);

        Assert.True(saved.Expelled);
        Assert.Equal(0, saved.TotalPleasure);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            DbMode.RestoreAsync(_repository, _sessionId, new SkipAfterFirstDayStrategy()));
    }

    private sealed class SkipAfterFirstDayStrategy : IStrategy
    {
        public bool[] DecideDay(int day, IReadOnlyStudentHistory history) =>
            Enumerable.Repeat(day == 1, 6).ToArray();
    }
}
