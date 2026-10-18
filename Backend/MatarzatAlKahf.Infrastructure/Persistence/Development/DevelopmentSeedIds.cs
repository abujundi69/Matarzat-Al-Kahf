using System.Security.Cryptography;
using System.Text;

namespace MatarzatAlKahf.Infrastructure.Persistence.Development;

internal static class DevelopmentSeedIds
{
    private const string NamespacePrefix = "MatarzatAlKahf.DevelopmentSeed:";

    public static Guid For(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        byte[] hash = SHA256.HashData(
            Encoding.UTF8.GetBytes(NamespacePrefix + key));
        Span<byte> guidBytes = stackalloc byte[16];
        hash.AsSpan(0, guidBytes.Length).CopyTo(guidBytes);

        guidBytes[7] = (byte)((guidBytes[7] & 0x0F) | 0x50);
        guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80);

        return new Guid(guidBytes);
    }
}
