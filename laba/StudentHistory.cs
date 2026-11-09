namespace laba;

public class StudentHistory : IReadOnlyStudentHistory
{
    private readonly Dictionary<(int, Subject), (bool Attended, bool? Asked)> _records = new();

    public void Record(int day, Subject subject, bool attended, bool? wasAsked)
    {
        _records[(day, subject)] = (attended, attended ? wasAsked : null);
    }
    
    public bool Attended(int day, Subject subject)
    {
        return _records.TryGetValue((day, subject), out var record) && record.Attended;
    }

    public bool? WasAsked(int day, Subject subject)
    {
        return _records.TryGetValue((day, subject), out var record) ? record.Asked : null;
    }
}
