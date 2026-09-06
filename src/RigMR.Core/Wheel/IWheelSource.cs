namespace RigMR.Wheel;

public interface IWheelSource : IDisposable
{
    string Name { get; }
    bool TryPoll(out WheelSample sample);
}

public readonly struct WheelSample
{
    public long TimestampTicks { get; init; }
    public double CenteredDeg { get; init; }
    public string Device { get; init; }
}

public sealed class SyntheticWheel : IWheelSource
{
    public string Name => "synthetic";
    private readonly Func<long, double> _angle;
    private readonly Func<long>? _clock;

    public SyntheticWheel(Func<long, double> angleAtTicks, Func<long>? clock = null)
    {
        _angle = angleAtTicks;
        _clock = clock;
    }

    public static SyntheticWheel Sine(double amplitudeDeg, double hz, double rangeDeg = 900)
    {
        var start = DateTime.UtcNow.Ticks;
        return new SyntheticWheel(t =>
        {
            var s = (t - start) / (double)TimeSpan.TicksPerSecond;
            return amplitudeDeg * Math.Sin(2 * Math.PI * hz * s);
        });
    }

    public bool TryPoll(out WheelSample sample)
    {
        var t = _clock?.Invoke() ?? DateTime.UtcNow.Ticks;
        sample = new WheelSample
        {
            TimestampTicks = t,
            CenteredDeg = _angle(t),
            Device = Name
        };
        return true;
    }

    public void Dispose() { }
}
