using System.IO.MemoryMappedFiles;

namespace RigMR.Telemetry;

/// <summary>
/// AMS2 / pCARS2 shared memory ($pcars2$).
/// Options - System - Shared Memory = Project CARS 2.
/// Offsets from the published pCARS2 / AMS2 map (participant block is 6400 bytes).
/// </summary>
public sealed class Ams2SharedMemoryReader : IDisposable
{
    public const string MapName = "$pcars2$";

    public const int OffsetVersion = 0;
    public const int OffsetBuild = 4;
    public const int OffsetGameState = 8;
    public const int OffsetUnfilteredThrottle = 6428;
    public const int OffsetUnfilteredBrake = 6432;
    public const int OffsetUnfilteredSteering = 6436;
    public const int OffsetUnfilteredClutch = 6440;
    public const int OffsetSpeed = 6848;
    public const int OffsetRpm = 6852;
    public const int OffsetSteering = 6872;
    public const int OffsetGear = 6876;

    private MemoryMappedFile? _mmf;
    private MemoryMappedViewAccessor? _view;

    public bool TryOpen()
    {
        try
        {
            _mmf = MemoryMappedFile.OpenExisting(MapName);
            _view = _mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
            return true;
        }
        catch (FileNotFoundException)
        {
            return false;
        }
        catch (PlatformNotSupportedException)
        {
            return false;
        }
    }

    public bool TryRead(out Ams2Snapshot snap)
    {
        snap = default;
        if (_view is null) return false;
        try
        {
            var version = _view.ReadInt32(OffsetVersion);
            if (version < 1) return false;
            snap = new Ams2Snapshot
            {
                Version = version,
                Build = _view.ReadInt32(OffsetBuild),
                GameState = _view.ReadInt32(OffsetGameState),
                UnfilteredSteer = _view.ReadSingle(OffsetUnfilteredSteering),
                Steer = _view.ReadSingle(OffsetSteering),
                SpeedMps = _view.ReadSingle(OffsetSpeed),
                Rpm = _view.ReadSingle(OffsetRpm),
                Gear = _view.ReadInt32(OffsetGear),
                Live = true
            };
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        _view?.Dispose();
        _mmf?.Dispose();
    }
}

public readonly struct Ams2Snapshot
{
    public int Version { get; init; }
    public int Build { get; init; }
    public int GameState { get; init; }
    /// <summary>Driver input, -1..+1 (left negative).</summary>
    public float UnfilteredSteer { get; init; }
    /// <summary>Filtered/in-car steer, -1..+1.</summary>
    public float Steer { get; init; }
    public float SpeedMps { get; init; }
    public float Rpm { get; init; }
    public int Gear { get; init; }
    public bool Live { get; init; }
}
