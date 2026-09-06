using RigMR.Prediction;
using RigMR.Protocol;
using Xunit;

namespace RigMR.Tests;

public class SteeringPredictorTests
{
    [Fact]
    public void ConstantVelocity_PredictionLeadsByTau()
    {
        var p = new SteeringPredictor(new SteeringPredictorOptions
        {
            PredictMs = 30,
            RangeDeg = 900,
            VelocityCutoffHz = 80,
            AccelerationCutoffHz = 40
        });

        const double omega = 200;
        const double dt = 0.001;
        long ticks = 0;
        SteeringPrediction last = default;
        for (var i = 0; i < 400; i++)
        {
            ticks += (long)(dt * TimeSpan.TicksPerSecond);
            last = p.Push(omega * i * dt, ticks);
        }

        Assert.InRange(last.OmegaDegPerSec, omega * 0.9, omega * 1.1);
        Assert.InRange(last.PredictedDeg - last.RawDeg, 4.5, 7.5);
        Assert.True(last.Confidence > 0.5);
    }

    [Fact]
    public void PredictionBeatsDelayedRaw_OnSineWave()
    {
        const double amp = 80;
        const double hz = 1.2;
        const double tau = 0.030;
        const double dt = 0.002;

        var p = new SteeringPredictor(new SteeringPredictorOptions
        {
            PredictMs = tau * 1000,
            RangeDeg = 900,
            VelocityCutoffHz = 50,
            AccelerationCutoffHz = 25
        });

        double errRaw = 0, errPred = 0;
        var n = 0;
        long ticks = 0;
        for (var i = 0; i < 1500; i++)
        {
            var t = i * dt;
            ticks += (long)(dt * TimeSpan.TicksPerSecond);
            var theta = amp * Math.Sin(2 * Math.PI * hz * t);
            var future = amp * Math.Sin(2 * Math.PI * hz * (t + tau));
            var pred = p.Push(theta, ticks);
            if (i < 250) continue;
            errRaw += Math.Abs(future - pred.RawDeg);
            errPred += Math.Abs(future - pred.PredictedDeg);
            n++;
        }

        errRaw /= n;
        errPred /= n;
        Assert.True(errPred < errRaw * 0.55,
            $"predicted MAE {errPred:0.00} should beat delayed raw MAE {errRaw:0.00}");
    }

    [Fact]
    public void Reversal_DoesNotKeepIntegratingOldAcceleration()
    {
        var p = new SteeringPredictor(new SteeringPredictorOptions
        {
            PredictMs = 30,
            RangeDeg = 900
        });

        long ticks = 0;
        const double dt = 0.002;
        for (var i = 0; i < 80; i++)
        {
            ticks += (long)(dt * TimeSpan.TicksPerSecond);
            p.Push(i * 2.0, ticks);
        }
        SteeringPrediction last = default;
        for (var i = 0; i < 40; i++)
        {
            ticks += (long)(dt * TimeSpan.TicksPerSecond);
            last = p.Push(160 - i * 4.0, ticks);
        }

        Assert.True(Math.Abs(last.PredictedDeg - last.RawDeg) < 25,
            $"overshoot {last.PredictedDeg - last.RawDeg:0.0} deg");
    }

    [Fact]
    public void Unwrap_Handles900DegreeWrap()
    {
        var d = SteeringPredictor.UnwrapDelta(890 - (-890), 900);
        Assert.InRange(d, -30, 30);
        var wrapped = SteeringPredictor.WrapToRange(460, 900);
        Assert.InRange(wrapped, -450, 450);
    }

    [Fact]
    public void PosePacket_RoundTrip()
    {
        var pkt = new PosePacket
        {
            Version = 1,
            Flags = PosePacket.FlagWheel | PosePacket.FlagPredicted,
            UnixMicroseconds = 1_700_000_000_123,
            RawDeg = 12.5f,
            PredictedDeg = 18.25f,
            OmegaDegPerSec = 210.5f,
            AlphaDegPerSec2 = -40f,
            Confidence = 0.87f,
            PredictMs = 30f,
            Ams2Steer = -0.25f
        };
        var bytes = pkt.ToBytes();
        Assert.Equal(PosePacket.Size, bytes.Length);
        Assert.True(PosePacket.TryRead(bytes, out var back));
        Assert.Equal(pkt.RawDeg, back.RawDeg);
        Assert.Equal(pkt.PredictedDeg, back.PredictedDeg);
        Assert.Equal(pkt.Confidence, back.Confidence);
        Assert.Equal(pkt.UnixMicroseconds, back.UnixMicroseconds);
    }
}
