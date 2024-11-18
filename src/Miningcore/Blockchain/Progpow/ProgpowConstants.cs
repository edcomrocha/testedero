using System.Globalization;
using Miningcore.Util;
using BigInteger = System.Numerics.BigInteger;

namespace Miningcore.Blockchain.Progpow;

public class FiroConstants
{
    public const int EpochLength = 1300;
    public const int ExtranoncePlaceHolderLength = 2;
    public static BigInteger BigMaxValue = BigInteger.Pow(2, 256);
    public static readonly BigInteger Diff1B = BigInteger.Parse("00000000ffff0000000000000000000000000000000000000000000000000000", NumberStyles.AllowHexSpecifier, null);
    public static readonly BigInteger Diff1 = BigInteger.Parse("00000000ffff0000000000000000000000000000000000000000000000000000", NumberStyles.HexNumber);
    public static double Multiplier = (double) new BigRational(BigMaxValue, Diff1);
}
public class RavencoinConstants
{
    public const int EpochLength = 7500;
    public const int ExtranoncePlaceHolderLength = 2;
    public static BigInteger BigMaxValue = BigInteger.Pow(2, 256);
    public static readonly BigInteger Diff1B = BigInteger.Parse("00000000ff000000000000000000000000000000000000000000000000000000", NumberStyles.AllowHexSpecifier, null);
    public static readonly BigInteger Diff1 = BigInteger.Parse("00000000ff000000000000000000000000000000000000000000000000000000", NumberStyles.HexNumber);
    public static double Multiplier = (double) new BigRational(BigMaxValue, Diff1);
}
