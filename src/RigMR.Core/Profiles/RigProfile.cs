using System.Text.Json;
using System.Text.Json.Serialization;

namespace RigMR.Profiles;

public sealed class RigProfile
{
    public string Name { get; set; } = "default";
    public WheelProfile Wheel { get; set; } = new();
    public MaskProfile Dash { get; set; } = new() { Enabled = false };
    public MaskProfile Cockpit { get; set; } = new() { Enabled = true };
    public Calibration Calibration { get; set; } = new();

    public static RigProfile Load(string path) =>
        JsonSerializer.Deserialize<RigProfile>(File.ReadAllText(path), Json) ?? new RigProfile();

    public void Save(string path)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        File.WriteAllText(path, JsonSerializer.Serialize(this, Json));
    }

    public static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}

public sealed class WheelProfile
{
    public string Name { get; set; } = "generic-gt";
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string Kind { get; set; } = "gt";
    public string? PngPath { get; set; }
    public double RangeDeg { get; set; } = 900;
    public double ThicknessMm { get; set; } = 18;
    public double PivotHeightMm { get; set; } = 0;
    public double TiltVerticalDeg { get; set; } = 0;
    public double TiltHorizontalDeg { get; set; } = 0;
    public bool Invert { get; set; }
    public double CenterOffsetDeg { get; set; }
    public string? PreferredDeviceSubstring { get; set; }
}

public sealed class MaskProfile
{
    public bool Enabled { get; set; } = true;
    public string? PngPath { get; set; }
    public List<Node3> Nodes { get; set; } = new();
}

public sealed class Node3
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
}

public sealed class Calibration
{
    public double HeadToWheelM { get; set; } = 0.42;
    public double WheelWorldX { get; set; }
    public double WheelWorldY { get; set; }
    public double WheelWorldZ { get; set; }
    public double PredictMs { get; set; } = 30;
}
