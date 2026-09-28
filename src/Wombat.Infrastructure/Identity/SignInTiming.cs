using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;

namespace Wombat.Infrastructure.Identity;

/// <summary>
/// A password check that checks nothing, for the sign-in refusals that would otherwise answer without one, so every
/// refusal takes as long as a wrong password's (T287's timing half, in T339, flow 02, E5).
/// </summary>
/// <remarks>
/// <para>
/// A wrong password costs one password hash, which is slow on purpose. Until T339 three refusals cost none: an address no
/// account has (Identity finds no user and stops), an account that signs in only through its institution (the endpoint
/// refuses it before any check), and a locked account (Identity answers a lockout before it checks the password). Each
/// read the same as a wrong password, but answered sooner, so a stopwatch could tell which addresses have accounts, and
/// which are locked.
/// </para>
/// <para>
/// So those paths verify the password typed against a hash of a random password no one knows, made once, by the app's
/// own hasher (<see cref="UserManager{TUser}.PasswordHasher" />), so it costs what a real check costs. The answer is
/// thrown away.
/// </para>
/// <para>
/// Used by the sign-in page's endpoint and by the link page's (<see cref="ExternalLoginHandler.LinkAndSignInAsync" />),
/// which refuses the same four ways in the same words. In Infrastructure, beside the handler, since the review of the t339
/// branch: until then it lived in the web host, and the link path answered those refusals with no hash.
/// </para>
/// <para>
/// One difference is left, and accepted: a wrong password also writes the account's failed-attempt count
/// (<c>AccessFailedAsync</c>), one database write the other refusals do not make. It is far smaller than a hash, and
/// equalising it would mean writing to an account that does not exist, or to one no password was tried against.
/// </para>
/// </remarks>
public sealed class SignInTiming
{
    private static readonly WombatIdentityUser Nobody = new();

    private readonly object _gate = new();
    private string? _hash;

    /// <summary>Checks <paramref name="password" /> against the hash no password matches, as long as a real check takes.</summary>
    public void Equalize(IPasswordHasher<WombatIdentityUser> hasher, string? password)
    {
        ArgumentNullException.ThrowIfNull(hasher);

        _ = hasher.VerifyHashedPassword(Nobody, HashOf(hasher), password ?? string.Empty);
    }

    private string HashOf(IPasswordHasher<WombatIdentityUser> hasher)
    {
        if (_hash is { } made)
        {
            return made;
        }

        lock (_gate)
        {
            return _hash ??= hasher.HashPassword(Nobody, Convert.ToHexString(RandomNumberGenerator.GetBytes(32)));
        }
    }
}
