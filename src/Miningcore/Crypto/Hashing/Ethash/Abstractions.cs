using NLog;

namespace Miningcore.Crypto.Hashing.Ethash;

public interface IEthashLight : IDisposable
{
    string AlgoName { get; }
    void Setup(int totalCache, ulong hardForkBlock, string dagDir = null);
    Task<IEthashCache> GetCacheAsync(ILogger logger, ulong block, CancellationToken ct);
}
public interface IEthashCache : IDisposable
{
    bool Compute(ILogger logger, byte[] hash, ulong nonce, out byte[] mixDigest, out byte[] result);
}
