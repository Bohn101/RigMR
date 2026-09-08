namespace RigMR.Wheel;

/// <summary>
/// Fanatec Podium DD1 (driver 457): Axis 1 is ±32767 across the current SEN lock.
/// theta = (axis / 32767) * (senDeg / 2).
/// </summary>
public static class FanatecAxis
{
    public const int FullScale = 32767;
    public const double AutoIdleSenDeg = 1080;

    public static double AxisToDegrees(int axis, double senDeg)
    {
        if (senDeg <= 0) throw new ArgumentOutOfRangeException(nameof(senDeg));
        var a = axis;
        if (a < -FullScale) a = -FullScale;
        if (a > FullScale) a = FullScale;
        return (a / (double)FullScale) * (senDeg * 0.5);
    }

    public static double ResolveSen(double? configuredSen, bool autoIdle) =>
        configuredSen is > 0 ? configuredSen.Value : (autoIdle ? AutoIdleSenDeg : 540);
}
