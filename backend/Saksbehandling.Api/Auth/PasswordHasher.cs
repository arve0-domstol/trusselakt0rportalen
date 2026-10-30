using System.Security.Cryptography;
using System.Text;

namespace Saksbehandling.Api.Auth;

/// <summary>
/// Hashes and verifies user passwords for storage and login.
/// </summary>
public static class PasswordHasher
{
    public static string Hash(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }

    public static bool Verify(string password, string hash) =>
        string.Equals(Hash(password), hash, StringComparison.OrdinalIgnoreCase);
}
