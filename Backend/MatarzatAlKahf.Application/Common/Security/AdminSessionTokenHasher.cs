using System.Security.Cryptography;
using System.Text;

namespace MatarzatAlKahf.Application.Common.Security;

/// <summary>
/// Hashes an administrative token identifier before it is stored. The raw JWT identifier never
/// reaches the database, so a database read cannot be replayed as a live session lookup.
/// </summary>
public static class AdminSessionTokenHasher
{
    public static string Hash(string tokenId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenId);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(tokenId)));
    }
}
