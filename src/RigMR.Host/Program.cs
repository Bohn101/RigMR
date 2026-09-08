using System.Net;
using System.Net.Sockets;
using RigMR.Prediction;
using RigMR.Profiles;
using RigMR.Protocol;
using RigMR.Telemetry;
using RigMR.Wheel;

namespace RigMR.Host;

internal static class Program
{
    private static int Main(string[] args)
    {
        var opt = Cli.Parse(args);
        Console.WriteLine("RigMR host");
        Console.WriteLine($"  predict-ms={opt.PredictMs}  range={opt.RangeDeg}  port={opt.Port}");
        Console.WriteLine($"  profile={opt.ProfilePath ?? "(none)"}  synthetic={opt.Synthetic}");

        RigProfile? profile = null;
        if (opt.ProfilePath is not null && File.Exists(opt.ProfilePath))
        {
            profile = RigProfile.Load(opt.ProfilePath);
            Console.WriteLine($"  loaded wheel={profile.Wheel.Name} thickness={profile.Wheel.ThicknessMm}mm");
        }

        var range = profile?.Wheel.RangeDeg ?? opt.RangeDeg;
        var predOpt = new SteeringPredictorOptions
        {
            PredictMs = profile?.Calibration.PredictMs ?? opt.PredictMs,
            RangeDeg = range,
            Invert = profile?.Wheel.Invert ?? opt.Invert,
            CenterOffsetDeg = profile?.Wheel.CenterOffsetDeg ?? 0
        };
        var predictor = new SteeringPredictor(predOpt);

        using IWheelSource wheel = opt.Synthetic
            ? SyntheticWheel.Sine(amplitudeDeg: 90, hz: 0.7, rangeDeg: predOpt.RangeDeg)
            : new WindowsWheelSource(profile?.Wheel.PreferredDeviceSubstring);

        using var ams2 = new Ams2SharedMemoryReader();
        var ams2Open = ams2.TryOpen();
        Console.WriteLine(ams2Open
            ? "  AMS2 shared memory: connected (steer is -1..+1, left negative)"
            : "  AMS2 shared memory: not found (start AMS2 with pCARS2 SHM)");

        using var udp = new UdpClient();
        var endpoint = new IPEndPoint(IPAddress.Loopback, opt.Port);

        Console.WriteLine("  broadcasting pose on UDP 127.0.0.1:{0}  (Ctrl+C to stop)", opt.Port);
        Console.WriteLine();

        var lastPrint = 0L;
        while (true)
        {
            if (!wheel.TryPoll(out var sample))
            {
                Thread.Sleep(2);
                continue;
            }

            var pred = predictor.Push(sample.CenteredDeg, sample.TimestampTicks);
            float ams2Steer = float.NaN;
            float ams2Filt = float.NaN;
            ushort flags = PosePacket.FlagWheel | PosePacket.FlagPredicted;
            if (ams2.TryRead(out var snap) && snap.Live)
            {
                ams2Steer = snap.UnfilteredSteer;
                ams2Filt = snap.Steer;
                flags |= PosePacket.FlagAms2;
            }

            var packet = new PosePacket
            {
                Version = 1,
                Flags = flags,
                UnixMicroseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000,
                RawDeg = (float)pred.RawDeg,
                PredictedDeg = (float)pred.PredictedDeg,
                OmegaDegPerSec = (float)pred.OmegaDegPerSec,
                AlphaDegPerSec2 = (float)pred.AlphaDegPerSec2,
                Confidence = (float)pred.Confidence,
                PredictMs = (float)pred.PredictMs,
                Ams2Steer = ams2Steer
            };

            var bytes = packet.ToBytes();
            udp.Send(bytes, bytes.Length, endpoint);

            var now = DateTime.UtcNow.Ticks;
            if (now - lastPrint > TimeSpan.TicksPerMillisecond * 100)
            {
                lastPrint = now;
                var ams2Text = float.IsNaN(ams2Steer)
                    ? "-"
                    : $"{ams2Steer,6:0.00}/{ams2Filt,6:0.00}";
                Console.Write($"\r raw={pred.RawDeg,7:0.00} pred={pred.PredictedDeg,7:0.00}  ams2={ams2Text}     ");
            }

            Thread.Sleep(1);
        }
    }
}

internal sealed class Cli
{
    public double PredictMs { get; init; } = 30;
    public double RangeDeg { get; init; } = 540;
    public int Port { get; init; } = 24721;
    public bool Synthetic { get; init; }
    public bool Invert { get; init; }
    public string? ProfilePath { get; init; }

    public static Cli Parse(string[] args)
    {
        double predict = 30, range = 540;
        var port = 24721;
        var synth = false;
        var invert = false;
        string? profile = null;
        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--predict-ms": predict = double.Parse(args[++i]); break;
                case "--range-deg": range = double.Parse(args[++i]); break;
                case "--port": port = int.Parse(args[++i]); break;
                case "--synthetic": synth = true; break;
                case "--invert": invert = true; break;
                case "--profile": profile = args[++i]; break;
                case "--help":
                case "-h":
                    Console.WriteLine("RigMR.Host [--predict-ms 30] [--range-deg 540] [--port 24721] [--synthetic] [--invert] [--profile path]");
                    Environment.Exit(0);
                    break;
            }
        }
        return new Cli
        {
            PredictMs = predict,
            RangeDeg = range,
            Port = port,
            Synthetic = synth,
            Invert = invert,
            ProfilePath = profile
        };
    }
}

internal sealed class WindowsWheelSource : IWheelSource
{
    public string Name => _hint ?? "directinput";
    private readonly string? _hint;
    private bool _warned;

    public WindowsWheelSource(string? deviceSubstring) => _hint = deviceSubstring;

    public bool TryPoll(out WheelSample sample)
    {
        if (!_warned)
        {
            Console.WriteLine("  no native DirectInput backend in this build -- use --synthetic or plug in SharpDX on Windows");
            _warned = true;
        }
        sample = default;
        return false;
    }

    public void Dispose() { }
}
