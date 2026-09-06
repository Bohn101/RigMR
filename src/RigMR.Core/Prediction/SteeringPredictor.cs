namespace RigMR.Prediction;

/// <summary>
/// Unwrap-aware constant-acceleration predictor for a bounded steering wheel.
/// Compensates compositor / render lead time so a mixed-reality wheel mask
/// does not lag the physical rim.
/// </summary>
public sealed class SteeringPredictor
{
    public SteeringPredictorOptions Options { get; }

    private double _unwrappedDeg;
    private double _lastRawDeg;
    private double _omega;
    private double _alpha;
    private long _lastTicks;
    private bool _hasSample;
    private double _secondsSinceReversal = 10;
    private double _jitterEma;
    private int _samples;

    public SteeringPredictor(SteeringPredictorOptions? options = null)
    {
        Options = options ?? new SteeringPredictorOptions();
    }

    public SteeringPrediction Push(double rawCenteredDeg, long timestampTicks)
    {
        rawCenteredDeg = Options.Invert ? -rawCenteredDeg : rawCenteredDeg;
        rawCenteredDeg += Options.CenterOffsetDeg;

        if (!_hasSample)
        {
            _unwrappedDeg = rawCenteredDeg;
            _lastRawDeg = rawCenteredDeg;
            _lastTicks = timestampTicks;
            _hasSample = true;
            _samples = 1;
            return Current(rawCenteredDeg);
        }

        var dt = (timestampTicks - _lastTicks) / (double)TimeSpan.TicksPerSecond;
        if (dt <= 1e-6)
        {
            return Current(rawCenteredDeg);
        }

        dt = Math.Min(dt, 0.05);

        var delta = UnwrapDelta(rawCenteredDeg - _lastRawDeg, Options.RangeDeg);
        _unwrappedDeg += delta;

        var instOmega = delta / dt;
        var omegaCutoff = Math.Clamp(Options.VelocityCutoffHz, 1, 200);
        var accCutoff = Math.Clamp(Options.AccelerationCutoffHz, 1, 200);
        var omegaAlpha = 1.0 - Math.Exp(-2.0 * Math.PI * omegaCutoff * dt);
        var accAlpha = 1.0 - Math.Exp(-2.0 * Math.PI * accCutoff * dt);

        var prevOmega = _omega;
        _omega += omegaAlpha * (instOmega - _omega);

        var instAlpha = (_omega - prevOmega) / dt;
        instAlpha = Math.Clamp(instAlpha, -Options.MaxAlphaDegPerSec2, Options.MaxAlphaDegPerSec2);
        _alpha += accAlpha * (instAlpha - _alpha);
        _alpha = Math.Clamp(_alpha, -Options.MaxAlphaDegPerSec2, Options.MaxAlphaDegPerSec2);

        if (ChangedDirection(prevOmega, _omega))
        {
            _secondsSinceReversal = 0;
            _alpha *= 0.15;
        }
        else
        {
            _secondsSinceReversal += dt;
        }

        var residual = instOmega - _omega;
        _jitterEma = 0.92 * _jitterEma + 0.08 * residual * residual;

        _lastRawDeg = rawCenteredDeg;
        _lastTicks = timestampTicks;
        _samples++;
        return Current(rawCenteredDeg);
    }

    public SteeringPrediction Current(double rawCenteredDeg)
    {
        var tau = EffectiveTauSeconds();
        var predictedUnwrapped = _unwrappedDeg + _omega * tau + 0.5 * _alpha * tau * tau;
        var predicted = WrapToRange(predictedUnwrapped, Options.RangeDeg);
        var raw = WrapToRange(_unwrappedDeg, Options.RangeDeg);

        return new SteeringPrediction
        {
            RawDeg = raw,
            PredictedDeg = predicted,
            OmegaDegPerSec = _omega,
            AlphaDegPerSec2 = _alpha,
            Confidence = Confidence(),
            PredictMs = tau * 1000.0,
            Samples = _samples
        };
    }

    public void Reset()
    {
        _unwrappedDeg = _lastRawDeg = _omega = _alpha = _jitterEma = 0;
        _hasSample = false;
        _samples = 0;
        _secondsSinceReversal = 10;
    }

    private double EffectiveTauSeconds()
    {
        var tau = Math.Max(0, Options.PredictMs) / 1000.0;
        var reversalWindow = Math.Max(0.008, Options.ReversalMs / 1000.0);
        if (_secondsSinceReversal < reversalWindow)
        {
            var t = _secondsSinceReversal / reversalWindow;
            var s = t * t * (3 - 2 * t);
            tau *= s;
        }

        if (Math.Abs(_omega) > 1 && Math.Abs(_alpha) > 1)
        {
            var timeToZero = -_omega / _alpha;
            if (timeToZero > 0 && timeToZero < tau)
            {
                tau = timeToZero * 0.55;
            }
        }

        return tau;
    }

    private double Confidence()
    {
        var jitter = Math.Sqrt(Math.Max(0, _jitterEma));
        var jitterTerm = 1.0 / (1.0 + jitter / 80.0);
        var reversalTerm = Math.Clamp(_secondsSinceReversal / 0.12, 0, 1);
        var warmup = Math.Clamp(_samples / 30.0, 0, 1);
        return Math.Clamp(jitterTerm * (0.35 + 0.65 * reversalTerm) * warmup, 0, 1);
    }

    internal static double UnwrapDelta(double delta, double rangeDeg)
    {
        var half = rangeDeg * 0.5;
        while (delta > half) delta -= rangeDeg;
        while (delta < -half) delta += rangeDeg;
        return delta;
    }

    internal static double WrapToRange(double unwrapped, double rangeDeg)
    {
        var half = rangeDeg * 0.5;
        var x = unwrapped + half;
        var m = x % rangeDeg;
        if (m < 0) m += rangeDeg;
        return m - half;
    }

    private static bool ChangedDirection(double a, double b)
    {
        const double eps = 8.0;
        if (Math.Abs(a) < eps || Math.Abs(b) < eps) return false;
        return Math.Sign(a) != Math.Sign(b);
    }
}

public sealed class SteeringPredictorOptions
{
    public double PredictMs { get; set; } = 30;
    public double RangeDeg { get; set; } = 900;
    public double VelocityCutoffHz { get; set; } = 40;
    public double AccelerationCutoffHz { get; set; } = 20;
    public double MaxAlphaDegPerSec2 { get; set; } = 180_000;
    public double ReversalMs { get; set; } = 40;
    public double CenterOffsetDeg { get; set; }
    public bool Invert { get; set; }
}

public readonly struct SteeringPrediction
{
    public double RawDeg { get; init; }
    public double PredictedDeg { get; init; }
    public double OmegaDegPerSec { get; init; }
    public double AlphaDegPerSec2 { get; init; }
    public double Confidence { get; init; }
    public double PredictMs { get; init; }
    public int Samples { get; init; }
}
