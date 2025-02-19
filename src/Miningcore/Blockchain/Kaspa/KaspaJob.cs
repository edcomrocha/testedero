using System.Collections.Concurrent;
using System.Text;
using Miningcore.Contracts;
using Miningcore.Crypto;
using Miningcore.Crypto.Hashing.Algorithms;
using Miningcore.Extensions;
using Miningcore.Stratum;
using Miningcore.Time;
using Miningcore.Util;
using NBitcoin;
using BigInteger = System.Numerics.BigInteger;
using kaspad = Miningcore.Blockchain.Kaspa.Kaspad;

namespace Miningcore.Blockchain.Kaspa;

public class KaspaXoShiRo256PlusPlus
{
    private readonly ulong[] s = new ulong[4];

    public KaspaXoShiRo256PlusPlus(Span<byte> prePowHash)
    {
        Contract.Requires<ArgumentException>(prePowHash.Length >= 32);

        for(var i = 0; i < 4; i++)
            s[i] = BitConverter.ToUInt64(prePowHash.Slice(i * 8, 8));
    }

    public ulong Uint64()
    {
        var result = RotateLeft64(s[0] + s[3], 23) + s[0];
        var t = s[1] << 17;
        s[2] ^= s[0];
        s[3] ^= s[1];
        s[1] ^= s[2];
        s[0] ^= s[3];
        s[2] ^= t;
        s[3] = RotateLeft64(s[3], 45);
        return result;
    }

    private static ulong RotateLeft64(ulong value, int offset)
    {
        return (value << offset) | (value >> (64 - offset));
    }

    private static ulong RotateRight64(ulong value, int offset)
    {
        return (value >> offset) | (value << (64 - offset));
    }
}
public class KaspaJob
{
    private readonly ConcurrentDictionary<string, bool> submissions = new(StringComparer.OrdinalIgnoreCase);

    protected IHashAlgorithm blockHeaderHasher = new Blake2b(Encoding.UTF8.GetBytes(KaspaConstants.CoinbaseBlockHash));
    protected IMasterClock clock;
    protected IHashAlgorithm coinbaseHasher = new CShake256(null, Encoding.UTF8.GetBytes(KaspaConstants.CoinbaseProofOfWorkHash));

    private object[] jobParams;
    protected IHashAlgorithm shareHasher = new CShake256(null, Encoding.UTF8.GetBytes(KaspaConstants.CoinbaseHeavyHash));
    public kaspad.RpcBlock BlockTemplate { get; private set; }
    public double Difficulty { get; private set; }
    public string JobId { get; protected set; }
    public uint256 blockTargetValue { get; protected set; }

    protected bool RegisterSubmit(string nonce)
    {
        var key = new StringBuilder()
            .Append(nonce)
            .ToString();

        return submissions.TryAdd(key, true);
    }

    protected virtual ushort[][] GenerateMatrix(Span<byte> prePowHash)
    {
        var matrix = new ushort[64][];
        for(var i = 0; i < 64; i++)
            matrix[i] = new ushort[64];

        var generator = new KaspaXoShiRo256PlusPlus(prePowHash);
        while(true)
        {
            for(var i = 0; i < 64; i++)
            for(var j = 0; j < 64; j += 16)
            {
                var val = generator.Uint64();
                for(var shift = 0; shift < 16; shift++)
                    matrix[i][j + shift] = (ushort) ((val >> (4 * shift)) & 0x0F);
            }
            if(ComputeRank(matrix) == 64)
                return matrix;
        }
    }

    protected virtual int ComputeRank(ushort[][] matrix)
    {
        var Eps = 0.000000001;
        var B = matrix.Select(row => row.Select(val => (double) val).ToArray()).ToArray();
        var rank = 0;
        var rowSelected = new bool[64];
        for(var i = 0; i < 64; i++)
        {
            int j;
            for(j = 0; j < 64; j++)
                if(!rowSelected[j] && Math.Abs(B[j][i]) > Eps)
                    break;
            if(j != 64)
            {
                rank++;
                rowSelected[j] = true;
                var pivot = B[j][i];
                for(var p = i + 1; p < 64; p++)
                    B[j][p] /= pivot;
                for(var k = 0; k < 64; k++)
                    if(k != j && Math.Abs(B[k][i]) > Eps)
                        for(var p = i + 1; p < 64; p++)
                            B[k][p] -= B[j][p] * B[k][i];
            }
        }
        return rank;
    }

    protected virtual Span<byte> ComputeCoinbase(Span<byte> prePowHash, Span<byte> data)
    {
        var matrix = GenerateMatrix(prePowHash);
        var vector = new ushort[64];
        var product = new ushort[64];
        for(var i = 0; i < 32; i++)
        {
            vector[2 * i] = (ushort) (data[i] >> 4);
            vector[2 * i + 1] = (ushort) (data[i] & 0x0F);
        }

        for(var i = 0; i < 64; i++)
        {
            ushort sum = 0;
            for(var j = 0; j < 64; j++)
                sum += (ushort) (matrix[i][j] * vector[j]);
            product[i] = (ushort) (sum >> 10);
        }

        var res = new byte[32];
        for(var i = 0; i < 32; i++)
            res[i] = (byte) (data[i] ^ ((byte) (product[2 * i] << 4) | (byte) product[2 * i + 1]));

        return (Span<byte>) res;
    }

    protected virtual Span<byte> SerializeCoinbase(Span<byte> prePowHash, long timestamp, ulong nonce)
    {
        Span<byte> hashBytes = stackalloc byte[32];

        using(var stream = new MemoryStream())
        {
            stream.Write(prePowHash);
            stream.Write(BitConverter.GetBytes((ulong) timestamp));
            stream.Write(new byte[32]); // 32 zero bytes padding
            stream.Write(BitConverter.GetBytes(nonce));

            coinbaseHasher.Digest(stream.ToArray(), hashBytes);

            return (Span<byte>) hashBytes.ToArray();
        }
    }

    protected virtual Span<byte> SerializeHeader(kaspad.RpcBlockHeader header, bool isPrePow = true, bool isLittleEndian = false)
    {
        var nonce = isPrePow ? 0 : header.Nonce;
        var timestamp = isPrePow ? 0 : header.Timestamp;
        Span<byte> hashBytes = stackalloc byte[32];
        //var blockHashBytes = Encoding.UTF8.GetBytes(KaspaConstants.CoinbaseBlockHash);

        using(var stream = new MemoryStream())
        {
            var versionBytes = isLittleEndian ? BitConverter.GetBytes((ushort) header.Version).ReverseInPlace() : BitConverter.GetBytes((ushort) header.Version);
            stream.Write(versionBytes);
            var parentsBytes = isLittleEndian ? BitConverter.GetBytes((ulong) header.Parents.Count).ReverseInPlace() : BitConverter.GetBytes((ulong) header.Parents.Count);
            stream.Write(parentsBytes);

            foreach(var parent in header.Parents)
            {
                var parentHashesBytes = isLittleEndian ? BitConverter.GetBytes((ulong) parent.ParentHashes.Count).ReverseInPlace() : BitConverter.GetBytes((ulong) parent.ParentHashes.Count);
                stream.Write(parentHashesBytes);

                foreach(var parentHash in parent.ParentHashes)
                    stream.Write(parentHash.HexToByteArray());
            }

            stream.Write(header.HashMerkleRoot.HexToByteArray());
            stream.Write(header.AcceptedIdMerkleRoot.HexToByteArray());
            stream.Write(header.UtxoCommitment.HexToByteArray());

            var timestampBytes = isLittleEndian ? BitConverter.GetBytes((ulong) timestamp).ReverseInPlace() : BitConverter.GetBytes((ulong) timestamp);
            stream.Write(timestampBytes);
            var bitsBytes = isLittleEndian ? BitConverter.GetBytes(header.Bits).ReverseInPlace() : BitConverter.GetBytes(header.Bits);
            stream.Write(bitsBytes);
            var nonceBytes = isLittleEndian ? BitConverter.GetBytes(nonce).ReverseInPlace() : BitConverter.GetBytes(nonce);
            stream.Write(nonceBytes);
            var daaScoreBytes = isLittleEndian ? BitConverter.GetBytes(header.DaaScore).ReverseInPlace() : BitConverter.GetBytes(header.DaaScore);
            stream.Write(daaScoreBytes);
            var blueScoreBytes = isLittleEndian ? BitConverter.GetBytes(header.BlueScore).ReverseInPlace() : BitConverter.GetBytes(header.BlueScore);
            stream.Write(blueScoreBytes);

            var blueWork = header.BlueWork.PadLeft(header.BlueWork.Length + header.BlueWork.Length % 2, '0');
            var blueWorkBytes = blueWork.HexToByteArray();

            var blueWorkLengthBytes = isLittleEndian ? BitConverter.GetBytes((ulong) blueWorkBytes.Length).ReverseInPlace() : BitConverter.GetBytes((ulong) blueWorkBytes.Length);
            stream.Write(blueWorkLengthBytes);
            stream.Write(blueWorkBytes);

            stream.Write(header.PruningPoint.HexToByteArray());

            blockHeaderHasher.Digest(stream.ToArray(), hashBytes);

            return (Span<byte>) hashBytes.ToArray();
        }
    }

    protected virtual (string, ulong[]) SerializeJobParamsData(Span<byte> prePowHash, bool isLittleEndian = false)
    {
        var preHashU64s = new ulong[4];
        var preHashStrings = "";

        for(var i = 0; i < 4; i++)
        {
            var slice = isLittleEndian ? prePowHash.Slice(i * 8, 8).ToNewReverseArray() : prePowHash.Slice(i * 8, 8);

            preHashStrings += slice.ToHexString().PadLeft(16, '0');
            preHashU64s[i] = BitConverter.ToUInt64(slice);
        }

        return (preHashStrings, preHashU64s);
    }

    protected virtual Share ProcessShareInternal(StratumConnection worker, string nonce)
    {
        var context = worker.ContextAs<KaspaWorkerContext>();

        BlockTemplate.Header.Nonce = Convert.ToUInt64(nonce, 16);

        var prePowHashBytes = SerializeHeader(BlockTemplate.Header);
        var coinbaseBytes = SerializeCoinbase(prePowHashBytes, BlockTemplate.Header.Timestamp, BlockTemplate.Header.Nonce);
        Span<byte> hashCoinbaseBytes = stackalloc byte[32];
        shareHasher.Digest(ComputeCoinbase(prePowHashBytes, coinbaseBytes), hashCoinbaseBytes);

        var targetHashCoinbaseBytes = new Target(new BigInteger(hashCoinbaseBytes.ToNewReverseArray(), true, true));
        var hashCoinbaseBytesValue = targetHashCoinbaseBytes.ToUInt256();
        //throw new StratumException(StratumError.LowDifficultyShare, $"nonce: {nonce} ||| BigInteger: {targetHashCoinbaseBytes.ToBigInteger()} ||| Target: {hashCoinbaseBytesValue} - [stratum: {KaspaUtils.DifficultyToTarget(context.Difficulty)} - blockTemplate: {blockTargetValue}] ||| BigToCompact: {KaspaUtils.BigToCompact(targetHashCoinbaseBytes.ToBigInteger())} - [stratum: {KaspaUtils.BigToCompact(KaspaUtils.DifficultyToTarget(context.Difficulty))} - blockTemplate: {BlockTemplate.Header.Bits}] ||| shareDiff: {(double) new BigRational(KaspaConstants.Diff1b, targetHashCoinbaseBytes.ToBigInteger()) / KaspaConstants.ShareMultiplier} - [stratum: {context.Difficulty} - blockTemplate: {KaspaUtils.TargetToDifficulty(KaspaUtils.CompactToBig(BlockTemplate.Header.Bits)) / KaspaConstants.ShareMultiplier}] ||| AdjustShareDifficulty: {context.Difficulty * KaspaConstants.Pow2xDiff1TargetNumZero * (double) KaspaConstants.MinHash}");

        // calc share-diff
        var shareDiff = (double) new BigRational(KaspaConstants.Diff1b, targetHashCoinbaseBytes.ToBigInteger()) / KaspaConstants.ShareMultiplier;
        // diff check
        var stratumDifficulty = context.Difficulty;
        var ratio = shareDiff / stratumDifficulty;

        // check if the share meets the much harder block difficulty (block candidate)
        var isBlockCandidate = hashCoinbaseBytesValue <= blockTargetValue;
        //var isBlockCandidate = true;

        // test if share meets at least workers current difficulty
        if(!isBlockCandidate && ratio < 0.99)
        {
            // check if share matched the previous difficulty from before a vardiff retarget
            if(context.VarDiff?.LastUpdate != null && context.PreviousDifficulty.HasValue)
            {
                ratio = shareDiff / context.PreviousDifficulty.Value;

                if(ratio < 0.99)
                    throw new StratumException(StratumError.LowDifficultyShare, $"low difficulty share ({shareDiff})");

                // use previous difficulty
                stratumDifficulty = context.PreviousDifficulty.Value;
            }

            else
            {
                throw new StratumException(StratumError.LowDifficultyShare, $"low difficulty share ({shareDiff})");
            }
        }

        var result = new Share
        {
            BlockHeight = (long) BlockTemplate.Header.DaaScore,
            NetworkDifficulty = Difficulty,
            Difficulty = context.Difficulty / KaspaConstants.ShareMultiplier
        };

        if(isBlockCandidate)
        {
            var hashBytes = SerializeHeader(BlockTemplate.Header, false);

            result.IsBlockCandidate = true;
            result.BlockHash = hashBytes.ToHexString();
        }

        return result;
    }

    public object[] GetJobParams()
    {
        return jobParams;
    }

    public virtual Share ProcessShare(StratumConnection worker, string nonce)
    {
        Contract.RequiresNonNull(worker);
        Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(nonce));

        var context = worker.ContextAs<KaspaWorkerContext>();

        // We don't need "0x"
        nonce = nonce.StartsWith("0x") ? nonce.Substring(2) : nonce;

        // Add extranonce to nonce if enabled and submitted nonce is shorter than expected (16 - <extranonce length> characters)
        if(nonce.Length <= KaspaConstants.NonceLength - context.ExtraNonce1.Length)
            nonce = context.ExtraNonce1.PadRight(KaspaConstants.NonceLength - context.ExtraNonce1.Length, '0') + nonce;

        // dupe check
        if(!RegisterSubmit(nonce))
            throw new StratumException(StratumError.DuplicateShare, "duplicate share");

        return ProcessShareInternal(worker, nonce);
    }

    public void Init(kaspad.RpcBlock blockTemplate, string jobId)
    {
        Contract.RequiresNonNull(blockTemplate);
        Contract.RequiresNonNull(jobId);

        JobId = jobId;
        var target = new Target(KaspaUtils.CompactToBig(blockTemplate.Header.Bits));
        Difficulty = KaspaUtils.TargetToDifficulty(target.ToBigInteger()) / KaspaConstants.ShareMultiplier;
        blockTargetValue = target.ToUInt256();
        BlockTemplate = blockTemplate;

        var (largeJob, regularJob) = SerializeJobParamsData(SerializeHeader(blockTemplate.Header));
        jobParams = new object[]
        {
            JobId, largeJob + BitConverter.GetBytes(blockTemplate.Header.Timestamp).ToHexString().PadLeft(16, '0'), regularJob, blockTemplate.Header.Timestamp
        };
    }
}
