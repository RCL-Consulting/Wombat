using System.Security.Cryptography;
using System.Text;

namespace Wombat.Application.Common.Security;

public interface IInvitationTokenService
{
    string GenerateToken();
    string HashToken(string token);
    bool VerifyToken(string token, string expectedHash);

    /// <summary>
    /// Whether this could be a token <see cref="GenerateToken" /> made, judged by its shape alone. A link whose token is
    /// not is refused before anything is read to verify it (T205).
    /// </summary>
    bool IsWellFormed(string? token);
}

public sealed class InvitationTokenService : IInvitationTokenService
{
    private const int TokenBytesLength = 32;

    /// <summary>
    /// The length of every token <see cref="GenerateToken" /> makes: <see cref="TokenBytesLength" /> bytes in unpadded
    /// base64url.
    /// </summary>
    public const int TokenLength = (TokenBytesLength * 4 + 2) / 3;

    public string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(TokenBytesLength);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public bool IsWellFormed(string? token)
        => token is { Length: TokenLength } && token.All(IsBase64UrlCharacter);

    private static bool IsBase64UrlCharacter(char character)
        => char.IsAsciiLetterOrDigit(character) || character is '-' or '_';

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
