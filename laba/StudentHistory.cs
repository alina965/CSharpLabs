namespace laba;

public class StudentHistory : IReadOnlyStudentHistory
{
    private readonly bool[,] _attended =  new bool[101, 6];
    private readonly bool?[,] _wasAsked =  new bool?[101, 6];

    public void Record(int day, Subject subject, bool attended, bool? wasAsked)
    {
        _attended[day, (int)subject] = attended;
        _wasAsked[day, (int)subject] = wasAsked;
    }
    
    public bool Attended(int day, Subject subject)
    {
        return _attended[day, (int)subject];
    }

    public bool? WasAsked(int day, Subject subject)
    {
        return _wasAsked[day, (int)subject];
    }
}