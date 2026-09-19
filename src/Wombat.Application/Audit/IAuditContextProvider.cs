namespace Wombat.Application.Audit;

/// <summary>
/// Provides actor context (current user, IP, user agent) for audit entries.
/// The Infrastructure layer registers an HTTP-context-backed implementation.
/// Returns nulls safely when there is no HTTP context (e.g. background jobs).
///
/// Scoped per MediatR dispatch: an HTTP request in the minimal-API endpoints, and a fresh
/// DI scope per Send in ScopedSender. That is what makes <see cref="DeclareInstitution"/>
/// safe to hold as mutable state.
/// </summary>
public interface IAuditContextProvider
{
    string? UserId { get; }
    string? UserDisplay { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }

    /// <summary>
    /// The acting user's institution, from their institution_id claim. Null when the actor has no
    /// institution: a global Administrator, a background job, an unauthenticated request.
    ///
    /// The audit log is read under institution scope, so every row needs a scope to be read under.
    /// Before T101 the pipeline stamped none, which left every command row unscoped and therefore
    /// readable by every InstitutionalAdmin. Do not drop this: an unstamped row is invisible to
    /// scoped admins now, so removing it does not fail loudly — it silently empties their audit log.
    ///
    /// A handler that knows better may override this for its own dispatch with
    /// <see cref="DeclareInstitution"/>.
    /// </summary>
    int? InstitutionId { get; }

    /// <summary>
    /// Declares, for the rest of this dispatch, which institution the command's audit row belongs to,
    /// overriding the principal-derived <see cref="InstitutionId"/>.
    ///
    /// For the handful of commands whose institution is knowable from the data they touch but not from
    /// the caller: the anonymous ones. AcceptInvitationCommand is the case that forced this — it runs
    /// from an <c>.AllowAnonymous()</c> endpoint that signs the new user in only after the command
    /// returns, so the principal is empty, and if the browser still carries someone else's auth cookie
    /// it is the wrong person entirely (eleven such rows in the dev database name the previously
    /// registered user as actor). Either way the row misses the admin who issued the invitation.
    ///
    /// Declaring is opt-in for the handler, not for every future command: a command that says nothing
    /// keeps the principal-derived stamp, which is right for the authenticated ones. The value must be
    /// resolved by the handler from server-side data — never read off the request payload, which on
    /// these endpoints is attacker-supplied. Last declaration in the dispatch wins; a nested Send
    /// shares the scope, so a handler declares for the command it is handling and no other.
    /// </summary>
    void DeclareInstitution(int institutionId);
}
