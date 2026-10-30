using System.Security.Cryptography;
using System.Text;

namespace Saksbehandling.Api.Auth;

/// <summary>
/// Derives an opaque per-user reference from the numeric user id, so routes
/// don't expose the raw sequential id directly.
/// </summary>
public static class UserReferenceHasher
{
    public static string Hash(int id)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(id.ToString()));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
