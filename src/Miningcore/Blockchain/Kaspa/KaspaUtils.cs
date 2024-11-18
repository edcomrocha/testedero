using System.Text;
using Miningcore.Crypto;
using Miningcore.Crypto.Hashing.Algorithms;
using Miningcore.Util;
using BigInteger = System.Numerics.BigInteger;

namespace Miningcore.Blockchain.Kaspa;

public static class KaspaUtils
{
    public static (KaspaAddressUtility, Exception) ValidateAddress(string address, string network, string coinSymbol = "KAS")
    {
        if(string.IsNullOrEmpty(address))
            return (null, new ArgumentException("Empty address..."));

        KaspaBech32Prefix networkBech32Prefix;

        switch(network.ToLower())
        {
            case "devnet":
                networkBech32Prefix = KaspaBech32Prefix.KaspaDev;

                break;
            case "simnet":
                networkBech32Prefix = KaspaBech32Prefix.KaspaSim;

                break;
            case "testnet":
                networkBech32Prefix = KaspaBech32Prefix.KaspaTest;

                break;
            default:
                networkBech32Prefix = KaspaBech32Prefix.KaspaMain;

                break;
        }

        try
        {
            var kaspaAddressUtility = new KaspaAddressUtility(coinSymbol);
            kaspaAddressUtility.DecodeAddress(address, networkBech32Prefix);

            return (kaspaAddressUtility, null);
        }
        catch(Exception ex)
        {
            return (null, ex);
        }
    }

    public static BigInteger DifficultyToTarget(double difficulty)
    {
        return BigInteger.Divide(KaspaConstants.Diff1Target, new BigInteger(difficulty));
    }

    public static BigInteger CalculateTarget(uint bits)
    {
        (uint mant, int expt) result;

        var unshiftedExpt = bits >> 24;
        if(unshiftedExpt <= 3)
        {
            result.mant = (bits & 0xFFFFFF) >> (8 * (3 - (int) unshiftedExpt));
            result.expt = 0;
        }
        else
        {
            result.mant = bits & 0xFFFFFF;
            result.expt = 8 * ((int) (bits >> 24) - 3);
        }

        // The mantissa is signed but may not be negative
        if(result.mant > 0x7FFFFF)
            return BigInteger.Zero;
        return BigInteger.Pow(result.mant, result.expt);
    }

    public static double TargetToDifficulty(BigInteger target)
    {
        return (double) new BigRational(KaspaConstants.Diff1Target, target);
    }

    public static double DifficultyToHashrate(double diff)
    {
        return (double) new BigRational(BigInteger.Multiply(BigInteger.Multiply(KaspaConstants.MinHash, KaspaConstants.BigGig), new BigInteger(diff)), KaspaConstants.Diff1);
    }

    public static double BigDiffToLittle(BigInteger diff)
    {
        var numerator = new BigInteger(2);
        numerator = numerator << 254;

        var final = BigInteger.Divide(numerator, diff);

        var tempA = BigInteger.Pow(2, 30);
        final = BigInteger.Divide(final, tempA);

        return (double) final;
    }

    public static BigInteger CompactToBig(uint compact)
    {
        var mantissa = compact & 0x007FFFFF;
        var isNegative = (compact & 0x00800000) != 0;
        var exponent = compact >> 24;

        BigInteger result;

        if(exponent <= 3)
        {
            mantissa >>= (int) (8 * (3 - exponent));
            result = new BigInteger(mantissa);
        }
        else
        {
            result = new BigInteger(mantissa);
            result <<= (int) (8 * (exponent - 3));
        }

        if(isNegative)
            result = BigInteger.Negate(result);

        return result;
    }

    public static uint BigToCompact(BigInteger n)
    {
        if(n.Sign == 0)
            return 0;

        var exponent = n.ToByteArray().Length;
        uint mantissa;

        if(exponent <= 3)
        {
            mantissa = (uint) n;
            mantissa <<= 8 * (3 - exponent);
        }
        else
        {
            var tmp = BigInteger.Divide(n, BigInteger.Pow(256, exponent - 3));
            mantissa = (uint) tmp;
        }

        if((mantissa & 0x00800000) != 0)
        {
            mantissa >>= 8;
            exponent++;
        }

        var compact = (uint) (exponent << 24) | mantissa;

        if(n.Sign < 0)
            compact |= 0x00800000;

        return compact;
    }

    public static double CalcWork(uint bits)
    {
        var difficultyNum = CompactToBig(bits);

        if(difficultyNum.Sign <= 0)
            return (double) BigInteger.Zero;

        return (double) new BigRational(KaspaConstants.OneLsh256, BigInteger.Add(difficultyNum, KaspaConstants.BigOne));
    }

    public static byte[] HashBlake2b(byte[] serializedScript)
    {
        IHashAlgorithm scriptHasher = new Blake2b();
        Span<byte> hashBytes = stackalloc byte[32];
        scriptHasher.Digest(serializedScript, hashBytes);

        return hashBytes.ToArray();
    }
}
public interface KaspaIAddress
{
    string EncodeAddress();
    byte[] ScriptAddress();
    KaspaBech32Prefix Prefix();
    byte Version();
    bool IsForPrefix(KaspaBech32Prefix prefix);
}
public class KaspaAddressPublicKey : KaspaIAddress
{
    private readonly KaspaBech32Prefix prefix;
    private readonly byte[] publicKey;

    public KaspaAddressPublicKey(byte[] publicKey, KaspaBech32Prefix prefix)
    {
        if(publicKey.Length != KaspaConstants.PublicKeySize)
            throw new ArgumentException($"Public key must be {KaspaConstants.PublicKeySize} bytes", nameof(publicKey));

        this.prefix = prefix;
        this.publicKey = publicKey.ToArray();
    }

    public byte version { get; } = KaspaConstants.PubKeyAddrID;

    public string EncodeAddress()
    {
        return KaspaBech32.Encode(prefix.ToString(), publicKey, version);
    }

    public byte[] ScriptAddress()
    {
        return publicKey.ToArray();
    }

    public KaspaBech32Prefix Prefix()
    {
        return prefix;
    }

    public byte Version()
    {
        return version;
    }

    public bool IsForPrefix(KaspaBech32Prefix prefix)
    {
        return this.prefix == prefix;
    }

    public override string ToString()
    {
        return EncodeAddress();
    }
}
public class KaspaAddressPublicKeyECDSA : KaspaIAddress
{
    private readonly KaspaBech32Prefix prefix;
    private readonly byte[] publicKey;

    public KaspaAddressPublicKeyECDSA(byte[] publicKey, KaspaBech32Prefix prefix)
    {
        if(publicKey.Length != KaspaConstants.PublicKeySizeECDSA)
            throw new ArgumentException($"Public key must be {KaspaConstants.PublicKeySizeECDSA} bytes", nameof(publicKey));

        this.prefix = prefix;
        this.publicKey = publicKey.ToArray();
    }

    public byte version { get; } = KaspaConstants.PubKeyECDSAAddrID;

    public string EncodeAddress()
    {
        return KaspaBech32.Encode(prefix.ToString(), publicKey, version);
    }

    public byte[] ScriptAddress()
    {
        return publicKey.ToArray();
    }

    public KaspaBech32Prefix Prefix()
    {
        return prefix;
    }

    public byte Version()
    {
        return version;
    }

    public bool IsForPrefix(KaspaBech32Prefix prefix)
    {
        return this.prefix == prefix;
    }

    public override string ToString()
    {
        return EncodeAddress();
    }
}
public class KaspaAddressScriptHash : KaspaIAddress
{
    private readonly byte[] hash;
    private readonly KaspaBech32Prefix prefix;

    public KaspaAddressScriptHash(byte[] serializedScript, KaspaBech32Prefix prefix)
    {
        var scriptHash = KaspaUtils.HashBlake2b(serializedScript);
        if(scriptHash.Length != KaspaConstants.Blake2bSize256)
            throw new ArgumentException($"Script hash must be {KaspaConstants.Blake2bSize256} bytes", nameof(scriptHash));

        this.prefix = prefix;
        hash = scriptHash.ToArray();
    }

    public byte version { get; } = KaspaConstants.ScriptHashAddrID;

    public string EncodeAddress()
    {
        return KaspaBech32.Encode(prefix.ToString(), hash, version);
    }

    public byte[] ScriptAddress()
    {
        return hash.ToArray();
    }

    public KaspaBech32Prefix Prefix()
    {
        return prefix;
    }

    public byte Version()
    {
        return version;
    }

    public bool IsForPrefix(KaspaBech32Prefix prefix)
    {
        return this.prefix == prefix;
    }

    public override string ToString()
    {
        return EncodeAddress();
    }
}
public class KaspaAddressUtility
{

    private readonly Dictionary<string, KaspaBech32Prefix> stringsToBech32Prefixes;

    public KaspaAddressUtility(string coinSymbol = "KAS")
    {
        CoinSymbol = coinSymbol;

        // Build address pattern based on network type and coin symbol
        switch(CoinSymbol)
        {
            case "KLS":
                stringsToBech32Prefixes = new Dictionary<string, KaspaBech32Prefix>
                {
                    {
                        KarlsencoinConstants.ChainPrefixMainnet, KaspaBech32Prefix.KaspaMain
                    },
                    {
                        KarlsencoinConstants.ChainPrefixDevnet, KaspaBech32Prefix.KaspaDev
                    },
                    {
                        KarlsencoinConstants.ChainPrefixTestnet, KaspaBech32Prefix.KaspaTest
                    },
                    {
                        KarlsencoinConstants.ChainPrefixSimnet, KaspaBech32Prefix.KaspaSim
                    }
                };

                break;
            case "PYI":
                stringsToBech32Prefixes = new Dictionary<string, KaspaBech32Prefix>
                {
                    {
                        PyrinConstants.ChainPrefixMainnet, KaspaBech32Prefix.KaspaMain
                    },
                    {
                        PyrinConstants.ChainPrefixDevnet, KaspaBech32Prefix.KaspaDev
                    },
                    {
                        PyrinConstants.ChainPrefixTestnet, KaspaBech32Prefix.KaspaTest
                    },
                    {
                        PyrinConstants.ChainPrefixSimnet, KaspaBech32Prefix.KaspaSim
                    }
                };

                break;
            default:
                stringsToBech32Prefixes = new Dictionary<string, KaspaBech32Prefix>
                {
                    {
                        KaspaConstants.ChainPrefixMainnet, KaspaBech32Prefix.KaspaMain
                    },
                    {
                        KaspaConstants.ChainPrefixDevnet, KaspaBech32Prefix.KaspaDev
                    },
                    {
                        KaspaConstants.ChainPrefixTestnet, KaspaBech32Prefix.KaspaTest
                    },
                    {
                        KaspaConstants.ChainPrefixSimnet, KaspaBech32Prefix.KaspaSim
                    }
                };

                break;
        }
    }

    public KaspaIAddress KaspaAddress { get; private set; }
    public string CoinSymbol { get; }

    public string EncodeAddress(KaspaBech32Prefix prefix, byte[] payload, byte version)
    {
        return KaspaBech32.Encode(PrefixToString(prefix), payload, version);
    }

    public void DecodeAddress(string addr, KaspaBech32Prefix expectedPrefix)
    {
        var (prefixString, decoded, version, error) = KaspaBech32.Decode(addr);
        if(error != null)
            throw new ArgumentException($"Decoded address is of unknown format: {error}");

        var prefix = ParsePrefix(prefixString);
        if(expectedPrefix != KaspaBech32Prefix.Unknown && expectedPrefix != prefix)
            throw new ArgumentException($"Decoded address is of wrong network. Expected {expectedPrefix.ToString()} but got {prefix.ToString()}");

        switch(version)
        {
            case KaspaConstants.PubKeyAddrID:
                KaspaAddress = new KaspaAddressPublicKey(decoded, prefix);

                break;
            case KaspaConstants.PubKeyECDSAAddrID:
                KaspaAddress = new KaspaAddressPublicKeyECDSA(decoded, prefix);

                break;
            case KaspaConstants.ScriptHashAddrID:
                KaspaAddress = new KaspaAddressScriptHash(KaspaUtils.HashBlake2b(decoded), prefix);

                break;
            default:
                throw new InvalidOperationException("Unknown address type");
        }
    }

    public KaspaBech32Prefix ParsePrefix(string prefixString)
    {
        if(!stringsToBech32Prefixes.TryGetValue(prefixString, out var prefix))
            throw new ArgumentException($"Could not parse prefix {prefixString}");

        return prefix;
    }

    public string PrefixToString(KaspaBech32Prefix prefix)
    {
        foreach(var (key, value) in stringsToBech32Prefixes)
            if(prefix == value)
                return key;

        return string.Empty;
    }
}
public static class KaspaBech32
{
    private const string Charset = "qpzry9x8gf2tvdw0s3jn54khce6mua7l";
    private const int ChecksumLength = 8;

    private static readonly ConversionType FiveToEightBits = new()
    {
        FromBits = 5,
        ToBits = 8,
        Pad = false
    };
    private static readonly ConversionType EightToFiveBits = new()
    {
        FromBits = 8,
        ToBits = 5,
        Pad = true
    };

    private static readonly long[] Generator =
    {
        0x98f2bc8e61, 0x79b76d99e2, 0xf33e5fb3c4, 0xae2eabe2a8, 0x1e4f43e470
    };

    public static string Encode(string prefix, byte[] payload, byte version)
    {
        var data = new byte[payload.Length + 1];
        data[0] = version;
        Array.Copy(payload, 0, data, 1, payload.Length);

        var converted = ConvertBits(data, EightToFiveBits);

        return EncodeInternal(prefix, converted);
    }

    public static (string, byte[], byte, Exception) Decode(string encoded)
    {
        try
        {
            var (prefix, decoded) = DecodeInternal(encoded);
            var converted = ConvertBits(decoded, FiveToEightBits);
            var version = converted[0];
            var payload = converted.Skip(1).ToArray();
            return (prefix, payload, version, null);
        }
        catch(Exception ex)
        {
            return (null, null, 0, ex);
        }
    }

    private static string EncodeInternal(string prefix, byte[] data)
    {
        var checksum = CalculateChecksum(prefix, data);
        var combined = data.Concat(checksum).ToArray();

        var base32String = EncodeToBase32(combined);

        return $"{prefix}:{base32String}";
    }

    private static (string, byte[]) DecodeInternal(string encoded)
    {
        if(encoded.Length < ChecksumLength + 2)
            throw new Exception($"Invalid bech32 string length {encoded.Length}");

        foreach(var c in encoded)
            if(c < 33 || c > 126)
                throw new Exception($"Invalid character in string: '{c}'");

        var lower = encoded.ToLower();
        var upper = encoded.ToUpper();

        if(encoded != lower && encoded != upper)
            throw new Exception("String not all lowercase or all uppercase");

        encoded = lower;

        var colonIndex = encoded.LastIndexOf(':');
        if(colonIndex < 1 || colonIndex + ChecksumLength + 1 > encoded.Length)
            throw new Exception("Invalid index of ':'");

        var prefix = encoded.Substring(0, colonIndex);
        var data = encoded.Substring(colonIndex + 1);

        var decoded = DecodeFromBase32(data);

        if(!VerifyChecksum(prefix, decoded))
        {
            var checksum = encoded.Substring(encoded.Length - ChecksumLength);
            var expected = EncodeToBase32(CalculateChecksum(prefix, decoded.Take(decoded.Length - ChecksumLength).ToArray()));

            throw new Exception($"Checksum failed. Expected {expected}, got {checksum}");
        }

        return (prefix, decoded.Take(decoded.Length - ChecksumLength).ToArray());
    }

    private static byte[] DecodeFromBase32(string base32String)
    {
        var decoded = new List<byte>(base32String.Length);
        foreach(var c in base32String)
        {
            var index = Charset.IndexOf(c);
            if(index < 0)
                throw new Exception($"Invalid character not part of charset: {c}");

            decoded.Add((byte) index);
        }
        return decoded.ToArray();
    }

    private static string EncodeToBase32(byte[] data)
    {
        var result = new StringBuilder(data.Length);
        foreach(var b in data)
        {
            if(b >= Charset.Length)
                return "";

            result.Append(Charset[b]);
        }
        return result.ToString();
    }

    private static byte[] ConvertBits(byte[] data, ConversionType conversionType)
    {
        var regrouped = new List<byte>();
        byte nextByte = 0;
        byte filledBits = 0;

        foreach(var b in data)
        {
            var shiftedB = (byte) (b << (8 - conversionType.FromBits));
            var remainingFromBits = conversionType.FromBits;

            while(remainingFromBits > 0)
            {
                var remainingToBits = (byte) (conversionType.ToBits - filledBits);
                var toExtract = remainingFromBits < remainingToBits ? remainingFromBits : remainingToBits;

                nextByte = (byte) ((nextByte << toExtract) | (shiftedB >> (8 - toExtract)));

                shiftedB = (byte) (shiftedB << toExtract);
                remainingFromBits -= toExtract;
                filledBits += toExtract;

                if(filledBits == conversionType.ToBits)
                {
                    regrouped.Add(nextByte);
                    filledBits = 0;
                    nextByte = 0;
                }
            }
        }

        if(conversionType.Pad && filledBits > 0)
        {
            nextByte = (byte) (nextByte << (conversionType.ToBits - filledBits));
            regrouped.Add(nextByte);
        }

        return regrouped.ToArray();
    }

    private static byte[] CalculateChecksum(string prefix, byte[] payload)
    {
        var prefixLower5Bits = PrefixToUint5Array(prefix);
        var payloadInts = PayloadToInts(payload);
        int[] templateZeroes =
        {
            0, 0, 0, 0, 0, 0, 0, 0
        };

        var concat = prefixLower5Bits.Concat(new[]
        {
            0
        }).Concat(payloadInts).Concat(templateZeroes).ToArray();
        var polyModResult = PolyMod(concat);

        var res = new byte[ChecksumLength];
        for(var i = 0; i < ChecksumLength; i++)
            res[i] = (byte) ((polyModResult >> (5 * (ChecksumLength - 1 - i))) & 31);

        return res;
    }

    private static bool VerifyChecksum(string prefix, byte[] payload)
    {
        var prefixLower5Bits = PrefixToUint5Array(prefix);
        var payloadInts = PayloadToInts(payload);

        var dataToVerify = prefixLower5Bits.Concat(new[]
        {
            0
        }).Concat(payloadInts).ToArray();
        return PolyMod(dataToVerify) == 0;
    }

    private static int[] PrefixToUint5Array(string prefix)
    {
        var prefixLower5Bits = new int[prefix.Length];
        for(var i = 0; i < prefix.Length; i++)
        {
            var c = prefix[i];
            var charLower5Bits = c & 31;
            prefixLower5Bits[i] = charLower5Bits;
        }

        return prefixLower5Bits;
    }

    private static int[] PayloadToInts(byte[] payload)
    {
        return payload.Select(b => (int) b).ToArray();
    }

    private static long PolyMod(int[] values)
    {
        long checksum = 1;
        foreach(var value in values)
        {
            var topBits = checksum >> 35;
            checksum = ((checksum & 0x07ffffffff) << 5) ^ value;

            for(var i = 0; i < Generator.Length; i++)
                if(((topBits >> i) & 1) == 1)
                    checksum ^= Generator[i];
        }

        return checksum ^ 1;
    }

    private class ConversionType
    {
        public byte FromBits { get; set; }
        public byte ToBits { get; set; }
        public bool Pad { get; set; }
    }
}
