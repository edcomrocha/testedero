using System.Numerics;

namespace Miningcore.Blockchain.Beam;

public static class BeamUtils
{
    public static double UnpackedDifficulty(long packedDifficulty)
    {
        uint leadingBit = 1 << 24;
        var order = packedDifficulty >> 24;
        var result = (leadingBit | (packedDifficulty & (leadingBit - 1))) * Math.Pow(2, order - 24);
        return Math.Abs(result);
    }

    public static long PackedDifficulty(double unpackedDifficulty)
    {
        long bits = 32 - BitOperations.LeadingZeroCount(Convert.ToUInt32(Math.Round(unpackedDifficulty, MidpointRounding.ToEven)));
        var correctedOrder = bits - 24 - 1;
        var mantissa = (long) (unpackedDifficulty * Math.Pow(2, -correctedOrder) - Math.Pow(2, 24));
        var order = 24 + correctedOrder;
        return mantissa | (order << 24);
    }
}
