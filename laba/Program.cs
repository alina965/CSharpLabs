namespace laba;

public class Program
{
    public static void Main()
    {
        var strategy = new AlwaysAttendStrategy();
        var simulator = new SemesterSimulator();
        var rng = new Random();

        var result = simulator.Run(strategy, rng);

        Console.WriteLine("Общее удовольствие: " + result.TotalPleasure);
        Console.WriteLine("Среднее удовольствие в день: " + result.AveragePleasurePerDay);
    }
}
