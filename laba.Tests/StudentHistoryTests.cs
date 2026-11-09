using laba;

namespace laba.Tests;

public class StudentHistoryTests
{
    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, null)]
    public void WasAsked_IsKnownOnlyWhenStudentAttended(
        bool attended, bool asked, bool? expected)
    {
        var history = new StudentHistory();

        history.Record(1, Subject.Calculus, attended, asked);

        Assert.Equal(attended, history.Attended(1, Subject.Calculus));
        Assert.Equal(expected, history.WasAsked(1, Subject.Calculus));
        Assert.Null(history.WasAsked(2, Subject.Calculus));
    }
}
