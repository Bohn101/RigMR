using System.IO.MemoryMappedFiles;

namespace RigMR.Telemetry;

/// <summary>
/// Conservative reader for AMS2 / pCARS2 shared memory ($pcars2$).
/// Enable in-game: Options - System - Shared Memory = Project CARS 2.
/// </summary>
public sealed class Ams2SharedMemoryReader : IDisposable
{
    public const string MapName = "$pcars2$";

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
            var version = _view.ReadInt32(0);
            if (version < 1) return false;
            const int UnfilteredSteerOffset = 12;
            snap = new Ams2Snapshot
            {
                Version = version,
                UnfilteredSteer = ReadFloatSafe(_view, UnfilteredSteerOffset),
                Live = true
            };
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static float ReadFloatSafe(MemoryMappedViewAccessor view, int offset)
    {
        try { return view.ReadSingle(offset); }
        catch { return float.NaN; }
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
    public float UnfilteredSteer { get; init; }
    public bool Live { get; init; }
}
