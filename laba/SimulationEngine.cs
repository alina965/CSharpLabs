namespace laba;

public interface ISimulationEngine
{
    DayResult Step();
    void Reset();
}

public class SimulationEngine : ISimulationEngine
{
    private const int SubjectCount = 6;

    private readonly IStrategy _strategy;
    private readonly Random _rng;

    private Teacher[] _teachers = null!;
    private bool[] _yesterdayAsked = null!;
    private StudentHistory _history = null!;
    private int _currentDay;
    private int _totalPleasure;

    public SimulationEngine(IStrategy strategy)
    {
        _strategy = strategy;
        _rng = new Random();
        Reset();
    }

    public void Reset()
    {
        _teachers = CreateTeachers(_rng);
        _yesterdayAsked = new bool[SubjectCount];
        _history = new StudentHistory();
        _currentDay = 0;
        _totalPleasure = 0;
    }

    public DayResult Step()
    {
        _currentDay++;

        var attend = _strategy.DecideDay(_currentDay, _history);
        var todayAsked = new bool[SubjectCount];

        for (var i = 0; i < SubjectCount; i++)
        {
            var subject = (Subject)i;
            var asked = _teachers[i].DecideAsk(_yesterdayAsked, _rng);
            todayAsked[i] = asked;

            var went = attend[i];

            if (!went && asked)
            {
                _totalPleasure = 0;
                return new DayResult
                {
                    Day = _currentDay,
                    TotalPleasure = 0,
                    AveragePleasurePerDay = 0,
                    Expelled = true
                };
            }

            if (!went)
            {
                _totalPleasure++;
            }

            bool? seenAsk = went ? asked : null;
            _history.Record(_currentDay, subject, went, seenAsk);
        }

        _totalPleasure++;
        _yesterdayAsked = todayAsked;

        return new DayResult
        {
            Day = _currentDay,
            TotalPleasure = _totalPleasure,
            AveragePleasurePerDay = (double)_totalPleasure / _currentDay,
            Expelled = false
        };
    }

    private static Teacher[] CreateTeachers(Random rng)
    {
        var teachers = new Teacher[SubjectCount];
        for (var i = 0; i < SubjectCount; i++)
        {
            teachers[i] = new Teacher((Subject)i, TeacherRule.CreateRandom(rng));
        }
        return teachers;
    }
}