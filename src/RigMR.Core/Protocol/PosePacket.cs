using System.Buffers.Binary;
using System.Text;

namespace RigMR.Protocol;

public struct PosePacket
{
    public const int Size = 48;
    public static readonly uint Magic = 0x524D4752;

    public ushort Version;
    public ushort Flags;
    public long UnixMicroseconds;
    public float RawDeg;
    public float PredictedDeg;
    public float OmegaDegPerSec;
    public float AlphaDegPerSec2;
    public float Confidence;
    public float PredictMs;
    public float Ams2Steer;

    public const ushort FlagWheel = 1;
    public const ushort FlagAms2 = 2;
    public const ushort FlagPredicted = 4;

    public byte[] ToBytes()
    {
        var buf = new byte[Size];
        Write(buf);
        return buf;
    }

    public void Write(Span<byte> dest)
    {
        if (dest.Length < Size) throw new ArgumentException("buffer too small");
        BinaryPrimitives.WriteUInt32LittleEndian(dest[0..], Magic);
        BinaryPrimitives.WriteUInt16LittleEndian(dest[4..], Version == 0 ? (ushort)1 : Version);
        BinaryPrimitives.WriteUInt16LittleEndian(dest[6..], Flags);
        BinaryPrimitives.WriteInt64LittleEndian(dest[8..], UnixMicroseconds);
        BinaryPrimitives.WriteSingleLittleEndian(dest[16..], RawDeg);
        BinaryPrimitives.WriteSingleLittleEndian(dest[20..], PredictedDeg);
        BinaryPrimitives.WriteSingleLittleEndian(dest[24..], OmegaDegPerSec);
        BinaryPrimitives.WriteSingleLittleEndian(dest[28..], AlphaDegPerSec2);
        BinaryPrimitives.WriteSingleLittleEndian(dest[32..], Confidence);
        BinaryPrimitives.WriteSingleLittleEndian(dest[36..], PredictMs);
        BinaryPrimitives.WriteSingleLittleEndian(dest[40..], Ams2Steer);
        BinaryPrimitives.WriteUInt32LittleEndian(dest[44..], 0);
    }

    public static bool TryRead(ReadOnlySpan<byte> src, out PosePacket packet)
    {
        packet = default;
        if (src.Length < Size) return false;
        if (BinaryPrimitives.ReadUInt32LittleEndian(src) != Magic) return false;
        packet = new PosePacket
        {
            Version = BinaryPrimitives.ReadUInt16LittleEndian(src[4..]),
            Flags = BinaryPrimitives.ReadUInt16LittleEndian(src[6..]),
            UnixMicroseconds = BinaryPrimitives.ReadInt64LittleEndian(src[8..]),
            RawDeg = BinaryPrimitives.ReadSingleLittleEndian(src[16..]),
            PredictedDeg = BinaryPrimitives.ReadSingleLittleEndian(src[20..]),
            OmegaDegPerSec = BinaryPrimitives.ReadSingleLittleEndian(src[24..]),
            AlphaDegPerSec2 = BinaryPrimitives.ReadSingleLittleEndian(src[28..]),
            Confidence = BinaryPrimitives.ReadSingleLittleEndian(src[32..]),
            PredictMs = BinaryPrimitives.ReadSingleLittleEndian(src[36..]),
            Ams2Steer = BinaryPrimitives.ReadSingleLittleEndian(src[40..]),
        };
        return true;
    }

    public override string ToString()
    {
        var sb = new StringBuilder(128);
        sb.Append($"raw={RawDeg,7:0.00} pred={PredictedDeg,7:0.00} w={OmegaDegPerSec,8:0.0} conf={Confidence:0.00}");
        return sb.ToString();
    }
}
