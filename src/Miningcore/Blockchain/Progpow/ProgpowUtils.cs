using System.Globalization;
using BigInteger = System.Numerics.BigInteger;

namespace Miningcore.Blockchain.Progpow;

public static class ProgpowUtils
{
    public static string FiroEncodeTarget(double difficulty)
    {
        difficulty = 1.0 / difficulty;

        BigInteger NewTarget;
        BigInteger DecimalDiff;
        BigInteger DecimalTarget;

        NewTarget = BigInteger.Multiply(FiroConstants.Diff1B, new BigInteger(difficulty));

        var StringDiff = difficulty.ToString(CultureInfo.InvariantCulture);
        var DecimalOffset = StringDiff.IndexOf(".");
        if(DecimalOffset > -1)
        {
            var Precision = StringDiff.Length - 1 - DecimalOffset;
            DecimalDiff = BigInteger.Parse(StringDiff.Substring(DecimalOffset + 1));
            DecimalTarget = BigInteger.Multiply(FiroConstants.Diff1B, DecimalDiff);

            var s = DecimalTarget.ToString();
            s = s.Substring(0, s.Length - Precision);

            DecimalTarget = BigInteger.Parse(s);
            NewTarget += DecimalTarget;
        }

        return string.Format("{0:x64}", NewTarget);
    }

    public static string RavencoinEncodeTarget(double difficulty)
    {
        difficulty = 1.0 / difficulty;

        BigInteger NewTarget;
        BigInteger DecimalDiff;
        BigInteger DecimalTarget;

        NewTarget = BigInteger.Multiply(RavencoinConstants.Diff1B, new BigInteger(difficulty));

        var StringDiff = difficulty.ToString(CultureInfo.InvariantCulture);
        var DecimalOffset = StringDiff.IndexOf(".");
        if(DecimalOffset > -1)
        {
            var Precision = StringDiff.Length - 1 - DecimalOffset;
            DecimalDiff = BigInteger.Parse(StringDiff.Substring(DecimalOffset + 1));
            DecimalTarget = BigInteger.Multiply(RavencoinConstants.Diff1B, DecimalDiff);

            var s = DecimalTarget.ToString();
            s = s.Substring(0, s.Length - Precision);

            DecimalTarget = BigInteger.Parse(s);
            NewTarget += DecimalTarget;
        }

        return string.Format("{0:x64}", NewTarget);
    }
}
