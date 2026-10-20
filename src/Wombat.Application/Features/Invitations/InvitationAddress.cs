using Wombat.Domain.Invitations;

namespace Wombat.Application.Features.Invitations;

/// <summary>
/// Invitations to one address, whatever its spelling (T373, flow 11, S4; build-contracts § 0). An invitation stores the
/// address as typed (trimmed, <c>IssueInvitation</c>), and Postgres compares text by case, so until T373 an invitation to
/// <c>Molefe@kgk…</c> and an account at <c>molefe@kgk…</c> were two addresses to every check. Each check that asks whether
/// two invitations, or an invitation and an account, name one address reads through this: issue's second-invitation
/// refusal (Q10), the accept's sweep, the used invitation a pending registrar's Home and the Waiting list read, and
/// Expired's leaving rule. A page shows the stored spelling.
/// </summary>
/// <remarks>
/// A seam (T373 fix pass, B4): the query lowers in SQL, under the database's locale, and <see cref="Invitation.NormaliseAddress" />
/// lowers in C#, invariantly. They agree on every ASCII address. On a character they disagree about (some non-ASCII local
/// parts; <c>İ</c>, or <c>I</c> under a Turkish locale) the match silently misses: Q10's second-invitation refusal does not
/// fire, and the sweep and Expired's leaving rule treat the two spellings as two addresses. A reader that indexes by the
/// C# form what the SQL returned must not assume every row came back (<c>ListExpiredUnusedInvitations</c>).
/// </remarks>
public static class InvitationAddress
{
    /// <summary>
    /// The invitations whose address is <paramref name="email" /> once both are trimmed and lower-cased
    /// (<see cref="Invitation.NormaliseAddress" />). Translated to SQL as <c>lower("Email") = @normalised</c>; the stored
    /// address is trimmed already.
    /// </summary>
    public static IQueryable<Invitation> ToAddress(this IQueryable<Invitation> invitations, string email)
    {
        ArgumentNullException.ThrowIfNull(invitations);
        var normalised = Invitation.NormaliseAddress(email);
        return invitations.Where(invitation => invitation.Email.ToLower() == normalised);
    }
}
