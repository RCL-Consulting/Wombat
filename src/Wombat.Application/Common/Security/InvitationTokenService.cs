using System.Security.Cryptography;
using System.Text;

namespace Wombat.Application.Common.Security;

public interface IInvitationTokenService
{
    string GenerateToken();
    string HashToken(string token);
    bool VerifyToken(string token, string expectedHash);

    /// <summary>
    /// A new token that names the one row holding its hash: a random selector, stored in the clear under a unique index,
    /// followed by a random secret. Only the hash of the whole token is stored (<see cref="HashToken" />), and it is
    /// checked in constant time (<see cref="VerifyToken" />) against the single row the selector finds. (T163)
    /// </summary>
    /// <remarks>
    /// For a link a stranger holds, answered on a public page: an MSF respondent's. Until T163 that link was a
    /// <see cref="GenerateToken" /> token, and the only way to find its row was to load every invitation and hash the
    /// token against each.
    /// </remarks>
    SelectorToken GenerateSelectorToken();

    /// <summary>
    /// The selector a token <see cref="GenerateSelectorToken" /> could have made begins with, judged by the token's shape
    /// alone; null for anything else. A link whose token has none is refused before anything is read (T205, T163).
    /// </summary>
    string? SelectorOf(string? token);
}

/// <summary>
/// A token from <see cref="IInvitationTokenService.GenerateSelectorToken" />: what the link carries, the selector it
/// begins with, and the hash that is stored. (T163)
/// </summary>
public sealed record SelectorToken(string Token, string Selector, string Hash)
{
    /// <summary>Never the token: it is the link's whole authority, and a record prints every property it has.</summary>
    public override string ToString() => $"{nameof(SelectorToken)} {{ {nameof(Selector)} = {Selector} }}";
}

public sealed class InvitationTokenService : IInvitationTokenService
{
    private const int TokenBytesLength = 32;
    private const int SelectorBytesLength = 12;

    /// <summary>
    /// The length of every token <see cref="GenerateToken" /> makes: <see cref="TokenBytesLength" /> bytes in unpadded
    /// base64url.
    /// </summary>
    public const int TokenLength = (TokenBytesLength * 4 + 2) / 3;

    /// <summary>
    /// The length of a selector: <see cref="SelectorBytesLength" /> bytes (96 bits) in base64url, which needs no padding.
    /// Enough that two links never share one, and not a secret: the database holds it in the clear.
    /// </summary>
    public const int SelectorLength = SelectorBytesLength * 4 / 3;

    /// <summary>
    /// The length of every token <see cref="GenerateSelectorToken" /> makes: its selector, then a secret as long as a
    /// <see cref="GenerateToken" /> token.
    /// </summary>
    public const int SelectorTokenLength = SelectorLength + TokenLength;

    public string GenerateToken() => RandomBase64Url(TokenBytesLength);

    public SelectorToken GenerateSelectorToken()
    {
        var selector = RandomBase64Url(SelectorBytesLength);
        var token = selector + RandomBase64Url(TokenBytesLength);
        return new SelectorToken(token, selector, HashToken(token));
    }

    public string? SelectorOf(string? token)
        => token is { Length: SelectorTokenLength } && token.All(IsBase64UrlCharacter)
            ? token[..SelectorLength]
            : null;

    private static string RandomBase64Url(int byteCount)
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(byteCount))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

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
