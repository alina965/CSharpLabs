namespace laba;

public record TeacherState(
    Subject Subject,
    AskRuleKind Kind,
    Subject A,
    Subject B);
    
public record HistoryEntry(
    int Day,
    Subject Subject,
    bool Attended,
    bool? Asked);
    
public record SemesterState(
    int Day,
    int TotalPleasure,
    bool Expelled,
    TeacherState[] Teachers,
    bool[] YesterdayAsked,
    HistoryEntry[] History,
    int RandomSeed,
    int RandomDraws);