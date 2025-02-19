using System.Globalization;
using System.Text.RegularExpressions;
using BigInteger = System.Numerics.BigInteger;

// ReSharper disable InconsistentNaming

namespace Miningcore.Blockchain.Kaspa;

public static class KaspaConstants
{
    public const string WalletDaemonCategory = "wallet";

    public const int Diff1TargetNumZero = 31;
    public const int ExtranoncePlaceHolderLength = 8;
    public const uint ShareMultiplier = 1;

    // KAS smallest unit is called SOMPI: https://github.com/kaspanet/kaspad/blob/master/util/amount.go
    public const decimal SmallestUnit = 100000000;

    // List of KAS prefixes: https://github.com/kaspanet/kaspad/blob/master/util/address.go
    public const string ChainPrefixDevnet = "kaspadev";
    public const string ChainPrefixSimnet = "kaspasim";
    public const string ChainPrefixTestnet = "kaspatest";
    public const string ChainPrefixMainnet = "kaspa";

    public const string CoinbaseBlockHash = "BlockHash";
    public const string CoinbaseProofOfWorkHash = "ProofOfWorkHash";
    public const string CoinbaseHeavyHash = "HeavyHash";

    public const string ProtobufDaemonRpcServiceName = "protowire.RPC";
    public const string ProtobufWalletRpcServiceName = "kaspawalletd.kaspawalletd";

    public const byte PubKeyAddrID = 0x00;
    public const byte PubKeyECDSAAddrID = 0x01;
    public const byte ScriptHashAddrID = 0x08;
    public const int PublicKeySize = 32;
    public const int PublicKeySizeECDSA = 33;
    public const int Blake2bSize256 = 32;
    public static readonly BigInteger Diff1b = BigInteger.Parse("00000000ffff0000000000000000000000000000000000000000000000000000", NumberStyles.HexNumber);
    public static BigInteger Diff1 = BigInteger.Pow(2, 256);
    public static BigInteger Diff1Target = BigInteger.Pow(2, 255) - 1;
    public static readonly double Pow2xDiff1TargetNumZero = Math.Pow(2, Diff1TargetNumZero);
    public static BigInteger BigOne = BigInteger.One;
    public static BigInteger OneLsh256 = BigInteger.One << 256;
    public static BigInteger MinHash = BigInteger.Divide(Diff1, Diff1Target);
    public static BigInteger BigGig = BigInteger.Pow(10, 9);
    public static int NonceLength = 16;

    public static readonly Regex RegexUserAgentBzMiner = new("bzminer", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    public static readonly Regex RegexUserAgentIceRiverMiner = new("iceriverminer", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    public static readonly Regex RegexUserAgentGodMiner = new("godminer", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    public static readonly Dictionary<byte, string> KaspaAddressType = new()
    {
        {
            PubKeyAddrID, "Public Key Address"
        },
        {
            PubKeyECDSAAddrID, "Public Key ECDSA Address"
        },
        {
            ScriptHashAddrID, "Script Hash Address"
        }
    };
}
public static class KarlsencoinConstants
{
    // List of KLS prefixes: https://github.com/karlsen-network/karlsend/blob/master/util/address.go
    public const string ChainPrefixDevnet = "karlsendev";
    public const string ChainPrefixSimnet = "karlsensim";
    public const string ChainPrefixTestnet = "karlsentest";
    public const string ChainPrefixMainnet = "karlsen";
}
public static class PyrinConstants
{
    // List of KLS prefixes: https://github.com/Pyrinpyi/pyipad/blob/master/util/address.go
    public const string ChainPrefixDevnet = "pyipadev";
    public const string ChainPrefixSimnet = "pyrinsim";
    public const string ChainPrefixTestnet = "pyrintest";
    public const string ChainPrefixMainnet = "pyrin";

    public const long Blake3ForkHeight = 1484741;
}
public enum KaspaBech32Prefix
{
    Unknown = 0,
    KaspaMain,
    KaspaDev,
    KaspaTest,
    KaspaSim
}
