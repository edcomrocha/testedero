using System.Text;
using Miningcore.Crypto.Hashing.Algorithms;

namespace Miningcore.Blockchain.Kaspa.Custom.Pyrin;

public class PyrinJob : KaspaJob
{
    public PyrinJob(long blockHeight)
    {
        if(blockHeight >= PyrinConstants.Blake3ForkHeight)
        {
            var coinbaseBlockHash = KaspaConstants.CoinbaseBlockHash;
            var hashBytes = Encoding.UTF8.GetBytes(coinbaseBlockHash.PadRight(32, '\0')).Take(32).ToArray();
            blockHeaderHasher = new Blake3(hashBytes);
            coinbaseHasher = new Blake3();
            shareHasher = new Blake3();
        }
    }
}
