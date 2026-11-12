namespace laba;

public sealed class ReplayRandom : Random
{
    private readonly Random _inner;

    public int Seed { get; }

    public int Draws { get; private set; }

    public ReplayRandom(int seed, int draws = 0)
    {
        if (draws < 0)
            throw new ArgumentOutOfRangeException(nameof(draws));

        Seed = seed;
        _inner = new Random(seed);

        for (var i = 0; i < draws; i++)
            _inner.NextDouble();

        Draws = draws;
    }

    public override int Next(int maxValue)
    {
        if (maxValue < 0)
            throw new ArgumentOutOfRangeException(nameof(maxValue));

        if (maxValue == 0)
            return 0;

        Draws++;

        return (int)(_inner.NextDouble() * maxValue);
    }
}