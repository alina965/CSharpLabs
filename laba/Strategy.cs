namespace laba;

public interface IStrategy
{
    bool[] DecideDay(int day, IReadOnlyStudentHistory history);
}

public interface IReadOnlyStudentHistory
{
    bool Attended (int day, Subject subject);
    bool? WasAsked (int day, Subject subject);
}