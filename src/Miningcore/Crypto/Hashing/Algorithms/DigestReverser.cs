namespace Miningcore.Crypto.Hashing.Algorithms;

[Identifier("reverse")]
public class DigestReverser : IHashAlgorithm
{

    public DigestReverser(IHashAlgorithm upstream)
    {
        this.Upstream = upstream;
    }

    public IHashAlgorithm Upstream { get; }

    public void Digest(ReadOnlySpan<byte> data, Span<byte> result, params object[] extra)
    {
        Upstream.Digest(data, result, extra);
        result.Reverse();
    }
}
