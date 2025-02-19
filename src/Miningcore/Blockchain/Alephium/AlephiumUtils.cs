using BigInteger = System.Numerics.BigInteger;

namespace Miningcore.Blockchain.Alephium;

public static class AlephiumUtils
{
    public static string EncodeTarget(double difficulty)
    {
        var numerator = AlephiumConstants.Diff1Target * 1024;
        var denominator = (BigInteger) Math.Ceiling(difficulty * 1024);
        var target = BigInteger.Divide(numerator, denominator);
        var targetBytes = target.ToByteArray();
        Array.Reverse(targetBytes); // Reverse the byte order to match endianness
        var paddedTargetBytes = new byte[Math.Max(targetBytes.Length, 32)];
        Buffer.BlockCopy(targetBytes, 0, paddedTargetBytes, paddedTargetBytes.Length - targetBytes.Length, Math.Min(targetBytes.Length, paddedTargetBytes.Length));
        return BitConverter.ToString(paddedTargetBytes).Replace("-", "").ToLower();
    }

    public static (int fromGroup, int toGroup) BlockChainIndex(Span<byte> hashBytes)
    {
        var beforeLast = hashBytes[hashBytes.Length - 2];
        beforeLast = (byte) (beforeLast & 0xFF);
        var last = hashBytes[hashBytes.Length - 1];
        last = (byte) (last & 0xFF);
        var bigIndex = (beforeLast << 8) | last;
        var chainNum = AlephiumConstants.GroupSize * AlephiumConstants.GroupSize;
        var index = bigIndex % chainNum;
        double tmpFromGroup = index / AlephiumConstants.GroupSize;
        var fromGroup = (int) Math.Floor(tmpFromGroup);
        var toGroup = index % AlephiumConstants.GroupSize;
        return (fromGroup, toGroup);
    }

    public static double TranslateApiHashrate(string hashrate)
    {
        double result = 0;

        if(!string.IsNullOrEmpty(hashrate))
        {
            var hashrateWithUnit = hashrate.Split(" ");
            if(hashrateWithUnit.Length == 2)
            {
                result = (double) ConvertNumberFromApi(hashrateWithUnit[0]);

                switch(hashrateWithUnit[1].ToLower())
                {
                    case "zh/s":
                        result = result * (double) BigInteger.Parse("1000000000000000000000");
                        break;
                    case "eh/s":
                        result = result * (double) BigInteger.Parse("1000000000000000000");
                        break;
                    case "ph/s":
                        result = result * (double) BigInteger.Parse("1000000000000000");
                        break;
                    case "th/s":
                        result = result * (double) BigInteger.Parse("1000000000000");
                        break;
                    case "gh/s":
                        result = result * (double) BigInteger.Parse("1000000000");
                        break;
                    case "mh/s":
                        result = result * (double) BigInteger.Parse("1000000");
                        break;
                    case "kh/s":
                        result = result * (double) BigInteger.Parse("1000");
                        break;
                }
            }
        }

        return result;
    }

    public static string ConvertNumberForApi(decimal amount)
    {
        return Convert.ToString(Math.Round(amount));
    }

    public static decimal ConvertNumberFromApi(string amount)
    {
        return Convert.ToDecimal(amount);
    }

    public static long UnixTimeStampForApi(DateTime date)
    {
        var unixTimeStamp = date.ToUniversalTime().Ticks - new DateTime(1970, 1, 1, 0, 0, 0).Ticks;
        unixTimeStamp /= TimeSpan.TicksPerMillisecond;
        return unixTimeStamp;
    }
}
