using Miningcore.Contracts;
using Miningcore.Native;

namespace Miningcore.Crypto.Hashing.Algorithms;

[Identifier("lyra2-rev3")]
public unsafe class Lyra2Rev3 : IHashAlgorithm
{
    public void Digest(ReadOnlySpan<byte> data, Span<byte> result, params object[] extra)
    {
        Contract.Requires<ArgumentException>(data.Length == 80);
        Contract.Requires<ArgumentException>(result.Length >= 32);

        fixed(byte* input = data)
        {
            fixed(byte* output = result)
            {
                Multihash.lyra2rev3(input, output);
            }
        }
    }
}
