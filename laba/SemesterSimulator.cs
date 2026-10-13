namespace laba;

public class SemesterResult
{
    public int TotalPleasure { get; init; }
    public double AveragePleasurePerDay { get; init; }
    public bool Expelled { get; set; }
}

public class SemesterSimulator
{
    private const int Days = 100;
    private const int SubjectCount = 6;

    public SemesterResult Run(IStrategy strategy, Random rng)
    {
        var teachers = CreateTeachers(rng);
        var yesterdayAsked = new bool[SubjectCount];
        var history = new StudentHistory();
        var pleasure = 0;

        for (var day = 1; day <= Days; day++)
        {
            var attend = strategy.DecideDay(day, history);
            var todayAsked = new bool[SubjectCount];

            for (var i = 0; i < SubjectCount; i++)
            {
                var subject = (Subject)i;
                var asked = teachers[i].DecideAsk(yesterdayAsked, rng);
                todayAsked[i] = asked;

                var went = attend[i];

                switch (went)
                {
                    case false when asked:
                    {
                        var expelled = new SemesterResult
                        {
                            TotalPleasure = 0,
                            AveragePleasurePerDay = 0,
                            Expelled = true
                        };
                        return expelled;
                    }
                    case false:
                        pleasure++;
                        break;
                }

                bool? seenAsk = null;
                if (went)
                {
                    seenAsk = asked;
                }

                history.Record(day, subject, went, seenAsk);
            }

            pleasure++;
            yesterdayAsked = todayAsked;
        }

        var result = new SemesterResult
        {
            TotalPleasure = pleasure,
            AveragePleasurePerDay = pleasure / (double)Days,
            Expelled = false
        };
        return result;
    }

    private static Teacher[] CreateTeachers(Random rng)
    {
        var teachers = new Teacher[SubjectCount];

        for (var i = 0; i < SubjectCount; i++)
        {
            var subject = (Subject)i;
            var rule = TeacherRule.CreateRandom(rng);
            teachers[i] = new Teacher(subject, rule);
        }

        return teachers;
    }
}