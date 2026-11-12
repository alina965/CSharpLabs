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
    
    public HistoryEntry[] Export()
    {
        return _records
            .OrderBy(pair => pair.Key.Item1)
            .ThenBy(pair => pair.Key.Item2)
            .Select(pair => new HistoryEntry(
                pair.Key.Item1,
                pair.Key.Item2,
                pair.Value.Attended,
                pair.Value.Asked))
            .ToArray();
    }

    public static StudentHistory Restore(HistoryEntry[] entries)
    {
        var history = new StudentHistory();

        foreach (var entry in entries)
        {
            history.Record(
                entry.Day,
                entry.Subject,
                entry.Attended,
                entry.Asked);
        }

        return history;
    }
}
