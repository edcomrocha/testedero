using Miningcore.Contracts;
using Miningcore.Native;

namespace Miningcore.Crypto.Hashing.Algorithms;

[Identifier("cshake256")]
public unsafe class CShake256 : IHashAlgorithm
{

    public CShake256(byte[] dataName = null, byte[] dataCustom = null)
    {
        this.dataName = dataName;
        this.dataCustom = dataCustom;
    }

    public byte[] dataName { get; protected set; }
    public byte[] dataCustom { get; protected set; }

    public void Digest(ReadOnlySpan<byte> data, Span<byte> result, params object[] extra)
    {
        Contract.Requires<ArgumentException>(result.Length >= 32);

        fixed(byte* input = data)
        {
            fixed(byte* output = result)
            {
                fixed(byte* name = dataName)
                {
                    var nameLength = dataName == null ? 0 : dataName.Length;
                    fixed(byte* custom = dataCustom)
                    {
                        var customLength = dataCustom == null ? 0 : dataCustom.Length;
                        Multihash.cshake256(input, (uint) data.Length, name, (uint) nameLength, custom, (uint) customLength, output, (uint) result.Length);
                    }
                }
            }
        }
    }
}
