using Miningcore.Contracts;
using Miningcore.Native;

namespace Miningcore.Crypto.Hashing.Algorithms;

[Identifier("blake2b")]
public unsafe class Blake2b : IHashAlgorithm
{

    public Blake2b(byte[] dataKey = null)
    {
        this.dataKey = dataKey;
    }

    public byte[] dataKey { get; protected set; }

    public void Digest(ReadOnlySpan<byte> data, Span<byte> result, params object[] extra)
    {
        Contract.Requires<ArgumentException>(result.Length >= 32);

        fixed(byte* input = data)
        {
            fixed(byte* output = result)
            {
                fixed(byte* key = dataKey)
                {
                    var keyLength = dataKey == null ? 0 : dataKey.Length;
                    Multihash.blake2b(input, output, (uint) data.Length, result.Length, key, (uint) keyLength);
                }
            }
        }
    }
}
