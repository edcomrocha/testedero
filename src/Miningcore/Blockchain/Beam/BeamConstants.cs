using System.Globalization;
using System.Numerics;

namespace Miningcore.Blockchain.Beam;

public class BeamConstants
{
    public const string WalletDaemonCategory = "wallet";
    public const string ExplorerDaemonCategory = "explorer";

    public const string WalletDaemonRpcLocation = "api/wallet";
    public const string ExplorerDaemonRpcStatusLocation = "status";

    public const short BeamRpcLoginSuccess = 0;
    public const short BeamRpcLoginFailure = -32003;

    public const short BeamRpcMethodNotFound = -32601;
    public const short BeamRpcJobNotFound = -32008; // stale
    public const short BeamRpcShareBadNonce = -32007;
    public const short BeamRpcShareBadSolution = -32004;
    public const short BeamRpcDuplicateShare = -32006;
    public const short BeamRpcInvalidShare = -32008;
    public const short BeamRpcLowDifficultyShare = -32009;
    public const short BeamRpcUnauthorizedWorker = -32003;
    public const short BeamRpcNotSubscribed = -32003;
    public const short BeamRpcShareAccepted = 1;

    // BEAM smallest unit is called GROTH
    public const decimal SmallestUnit = 100000000;

    public const short NonceSize = 16;
    public const short SolutionSize = 208;

    public const int PayoutMinBlockConfirmations = 240;

    public const ulong EmisssionFirstEpochHeight = 525600;
    public const decimal EmisssionFirstEpochBlockReward = 40.0m;
    public const ulong EmisssionSecondEpochHeight = 2102400;
    public const decimal EmisssionSecondEpochBlockReward = 25m;

    public const ulong EmisssionThirdEpochHeight = 3153600;
    public const ulong EraLength = 2102401;
    public const double DisinflationRateQuotient = 5.0;
    public const double DisinflationRateDivisor = 10.0;

    public const decimal BaseRewardInitial = 80.0m;

    public static readonly BigInteger BigMaxValue = BigInteger.Pow(2, 256);
    public static readonly double Pow2x32 = Math.Pow(2, 32);
    public static readonly BigInteger Diff1b = BigInteger.Parse("ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff", NumberStyles.HexNumber);
    public static readonly BigInteger ZCashDiff1b = BigInteger.Parse("0007ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff", NumberStyles.HexNumber);
}
public static class BeamExplorerCommands
{
    public const string GetStatus = "status";
    public const string GetBlock = "block";
}
public static class BeamWalletCommands
{
    // method available only since wallet API v6.1, so upgrade your node in order to enjoy that feature
    // https://github.com/BeamMW/beam/wiki/Beam-wallet-protocol-API-v6.1
    public const string GetVersion = "get_version";

    public const string GetBlockHeaders = "block_details";
    public const string GetBalance = "wallet_status";
    public const string ValidateAddress = "validate_address";
    public const string GetListAddresses = "addr_list";

    /// <summary>
    ///     Returns an transactionId or an error.
    /// </summary>
    public const string SendTransaction = "tx_send";

    public const string GetListTransactions = "tx_list";
    public const string GetTransactionStatus = "tx_status";
}
