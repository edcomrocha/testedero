using Miningcore.Contracts;
using Miningcore.Native;

namespace Miningcore.Crypto.Hashing.Algorithms;

[Identifier("neoscrypt")]
public unsafe class NeoScrypt : IHashAlgorithm
{

    private readonly uint profile;

    public NeoScrypt(uint profile)
    {
        this.profile = profile;
    }

    public void Digest(ReadOnlySpan<byte> data, Span<byte> result, params object[] extra)
    {
        Contract.Requires<ArgumentException>(data.Length == 80);
        Contract.Requires<ArgumentException>(result.Length >= 32);

        fixed(byte* input = data)
        {
            fixed(byte* output = result)
            {
                Multihash.neoscrypt(input, output, (uint) data.Length, profile);
            }
        }
    }
}
