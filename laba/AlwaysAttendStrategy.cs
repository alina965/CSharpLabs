namespace laba;

public class AlwaysAttendStrategy : IStrategy
{
    public bool[] DecideDay(int day, IReadOnlyStudentHistory history) => [true, true, true, true, true, true];
}