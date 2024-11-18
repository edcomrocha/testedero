using Miningcore.Crypto.Hashing.Algorithms;

namespace Miningcore.Blockchain.Kaspa.Custom.Karlsencoin;

public class KarlsencoinJob : KaspaJob
{
    public KarlsencoinJob()
    {
        coinbaseHasher = new Blake3();
    }
}
