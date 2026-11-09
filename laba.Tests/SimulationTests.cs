using laba;

namespace laba.Tests;

public class SimulationTests
{
    private sealed class ControlledRandom(int value) : Random
    {
        public override int Next(int maxValue) => value % maxValue;
    }

    private sealed class TestStrategy(bool attend, bool skipAfterFirstDay = false) : IStrategy
    {
        public int Calls { get; private set; }

        public bool[] DecideDay(int day, IReadOnlyStudentHistory history)
        {
            Calls++;
            return Enumerable.Repeat(attend && (!skipAfterFirstDay || day == 1), 6).ToArray();
        }
    }

    private static Teacher[] Teachers(TeacherRule rule) =>
        Enum.GetValues<Subject>().Select(subject => new Teacher(subject, rule)).ToArray();

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, false)]
    public void RandomRule_UsesControlledRandom(int randomValue, bool expected)
    {
        var teacher = new Teacher(Subject.Calculus, new TeacherRule(AskRuleKind.Random));

        Assert.Equal(expected,
            teacher.DecideAsk(new bool[6], new ControlledRandom(randomValue)));
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    public void XorRule_MatchesTruthTable(bool a, bool b, bool expected)
    {
        var yesterday = new[] { a, false, false, false, false, b };
        var teacher = new Teacher(Subject.Calculus,
            new TeacherRule(AskRuleKind.XorYesterdayAb, Subject.Calculus, Subject.English));

        Assert.Equal(expected, teacher.DecideAsk(yesterday, new ControlledRandom(0)));
    }

    [Fact]
    public void XorRule_RejectsEqualSubjects()
    {
        Assert.Throws<ArgumentException>(() => new TeacherRule(
            AskRuleKind.XorYesterdayAb, Subject.Calculus, Subject.Calculus));
    }

    [Theory]
    [InlineData(true, false, 1, false)]
    [InlineData(true, true, 1, false)]
    [InlineData(false, false, 7, false)]
    [InlineData(false, true, 0, true)]
    public void Step_HandlesAttendanceAndQuestionCombinations(
        bool attended, bool asked, int expectedPleasure, bool expectedExpulsion)
    {
        var strategy = new TestStrategy(attended);
        var engine = new SimulationEngine(strategy,
            new ControlledRandom(asked ? 0 : 1),
            Teachers(new TeacherRule(AskRuleKind.Random)));

        var result = engine.Step();

        Assert.Equal(expectedPleasure, result.TotalPleasure);
        Assert.Equal(expectedExpulsion, result.Expelled);
    }

    [Fact]
    public void Expulsion_ResetsPleasureAndStopsSimulation()
    {
        var strategy = new TestStrategy(attend: true, skipAfterFirstDay: true);
        var engine = new SimulationEngine(strategy, new ControlledRandom(0),
            Teachers(new TeacherRule(AskRuleKind.Random)));

        Assert.Equal(1, engine.Step().TotalPleasure);
        var expelled = engine.Step();

        Assert.True(expelled.Expelled);
        Assert.Equal(0, expelled.TotalPleasure);
        Assert.Same(expelled, engine.Step());
        Assert.Equal(2, strategy.Calls);
    }
}
