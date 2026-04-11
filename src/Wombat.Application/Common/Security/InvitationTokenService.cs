using System.Security.Cryptography;
using System.Text;

namespace Wombat.Application.Common.Security;

public interface IInvitationTokenService
{
    string GenerateToken();
    string HashToken(string token);
    bool VerifyToken(string token, string expectedHash);
}

public sealed class InvitationTokenService : IInvitationTokenService
{
    private const int TokenBytesLength = 32;

    public string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(TokenBytesLength);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public string HashToken(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }

    public bool VerifyToken(string token, string expectedHash)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(expectedHash))
        {
            return false;
        }

        byte[] expectedBytes;

        try
        {
            expectedBytes = Convert.FromHexString(expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }

        var candidateBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return CryptographicOperations.FixedTimeEquals(candidateBytes, expectedBytes);
    }
}
