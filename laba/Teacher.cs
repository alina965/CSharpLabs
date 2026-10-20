namespace laba;

public enum AskRuleKind
{
    Random,
    IfAskedYesterdayA,
    XorYesterdayAb
}

public class TeacherRule
{
    public AskRuleKind Kind { get; }
    public Subject A { get; }
    public Subject B { get; }

    private TeacherRule(AskRuleKind kind, Subject a = default, Subject b =  default)
    {
        Kind = kind;
        A = a;
        B = b;
    }

    public static TeacherRule CreateRandom(Random rng)
    {
        var kind = (AskRuleKind)rng.Next(3);
        var a = (Subject)rng.Next(6);

        Subject b;
        do
        {
            b = (Subject)rng.Next(6);
            
        } while (b == a);

        return kind switch
        {
            AskRuleKind.Random => new TeacherRule(kind),
            AskRuleKind.IfAskedYesterdayA => new TeacherRule(kind, a),
            _ => new TeacherRule(kind, a, b)
        };
    }
}

public class Teacher(Subject subj, TeacherRule rule)
{
    public Subject Subject = subj;
    private TeacherRule Rule { get; } = rule;

    public bool DecideAsk(bool[] yesterdayAsked, Random rng)
    {
        if (Rule.Kind == AskRuleKind.Random)
        {
            return rng.Next(2) == 0;
        }

        if (Rule.Kind == AskRuleKind.IfAskedYesterdayA)
        {
            return yesterdayAsked[(int)Rule.A];
        }
        
        return yesterdayAsked[(int)Rule.A] != yesterdayAsked[(int)Rule.B];
    }
}