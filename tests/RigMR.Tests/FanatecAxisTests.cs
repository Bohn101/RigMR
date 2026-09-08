using RigMR.Wheel;
using Xunit;

namespace RigMR.Tests;

public class FanatecAxisTests
{
    [Theory]
    [InlineData(-32767, 540, -270)]
    [InlineData(-32767, 900, -450)]
    [InlineData(-32767, 1080, -540)]
    [InlineData(-32767, 2520, -1260)]
    [InlineData(0, 900, 0)]
    [InlineData(32767, 900, 450)]
    public void MeasuredStops(int axis, double sen, double expected)
    {
        Assert.Equal(expected, FanatecAxis.AxisToDegrees(axis, sen), 3);
    }

    [Fact]
    public void AutoIdleIs1080()
    {
        Assert.Equal(1080, FanatecAxis.ResolveSen(null, autoIdle: true));
        Assert.Equal(540, FanatecAxis.ResolveSen(null, autoIdle: false));
        Assert.Equal(900, FanatecAxis.ResolveSen(900, autoIdle: true));
    }
}
