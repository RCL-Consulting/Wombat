using System.Security.Claims;
using Wombat.Application.Common.Extensions;
using Wombat.Domain.DataRights;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.DataRights;

/// <summary>
/// The one place that decides who may review a data-rights request. (T112)
/// </summary>
/// <remarks>
/// <para>
/// Before T112 this rule existed six times, hand-written in each handler, and every copy gated on
/// role alone: <c>Administrator || Coordinator</c> for review and download, <c>Administrator ||
/// SpecialityAdmin</c> for rectification. With no institution comparison anywhere, a Coordinator at
/// one hospital could list, open and download another hospital's subject access reports — and
/// approve their <b>erasure</b>, which is destructive rather than merely disclosing.
/// </para>
/// <para>
/// The two role sets are preserved exactly as they were. Which roles ought to hold this power is a
/// product question — a statutory data-rights officer is usually a named institutional role, not
/// "any Coordinator" — and answering it here would be a silent change to who can do what. T112
/// scopes the existing holders; choosing the holders is left open in the task file.
/// </para>
/// </remarks>
public static class DataRightsAuthorization
{
    /// <summary>Roles that may triage, approve, reject and download a request.</summary>
    private static readonly string[] ReviewRoles = [WombatRoles.Coordinator];

    /// <summary>Roles that may apply and complete a rectification.</summary>
    private static readonly string[] RectificationRoles = [WombatRoles.SpecialityAdmin];

    public static bool CanReview(ClaimsPrincipal principal, DataRightsRequest request)
        => CanAct(principal, request, ReviewRoles);

    public static bool CanRectify(ClaimsPrincipal principal, DataRightsRequest request)
        => CanAct(principal, request, RectificationRoles);

    /// <summary>The requester reading their own request, which is the point of the feature.</summary>
    public static bool IsRequester(ClaimsPrincipal principal, DataRightsRequest request)
    {
        var callerUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return !string.IsNullOrEmpty(callerUserId) &&
               string.Equals(request.RequesterUserId, callerUserId, StringComparison.Ordinal);
    }

    public static void DemandReviewAccess(ClaimsPrincipal principal, DataRightsRequest request)
    {
        if (!CanReview(principal, request))
        {
            throw new UnauthorizedAccessException(RefusalMessage);
        }
    }

    public static void DemandRectificationAccess(ClaimsPrincipal principal, DataRightsRequest request)
    {
        if (!CanRectify(principal, request))
        {
            throw new UnauthorizedAccessException(RefusalMessage);
        }
    }

    /// <summary>
    /// The requester, or a reviewer in scope. For request METADATA — status, type, decision note —
    /// which a reviewer must see to triage it.
    /// </summary>
    public static void DemandReadAccess(ClaimsPrincipal principal, DataRightsRequest request)
    {
        if (!CanReview(principal, request) && !IsRequester(principal, request))
        {
            throw new UnauthorizedAccessException(RefusalMessage);
        }
    }

    /// <summary>
    /// The exported bundle itself: the data subject, or a global Administrator. Reviewers are
    /// deliberately NOT admitted here, unlike <see cref="DemandReadAccess" />.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Scoping the request row does not scope the bundle it releases. The access report is assembled
    /// by person, not by institution — every activity where the subject is subject OR creator, their
    /// committee reviews, their progress, their audit trail. An assessor who covered a rotation at
    /// another hospital has rows stamped to THAT hospital; a trainee who transferred keeps their old
    /// ones. Those rows are the subject's personal data and belong in their report, but they are also
    /// other institutions' clinical records, and a reviewer in scope for the REQUEST cannot open them
    /// individually.
    /// </para>
    /// <para>
    /// The subject downloading their own data is the entire point of the feature and stays complete
    /// and unfiltered. Nothing is lost by removing the reviewer: the only download link in the
    /// product is on the data subject's own profile page. The reviewer approves; the subject
    /// collects.
    /// </para>
    /// </remarks>
    public static void DemandExportAccess(ClaimsPrincipal principal, DataRightsRequest request)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(request);

        if (!principal.IsAdministrator() && !IsRequester(principal, request))
        {
            throw new UnauthorizedAccessException(RefusalMessage);
        }
    }

    /// <summary>
    /// Whether this caller may review requests at all, before any particular request is loaded — the
    /// list path's first gate. It deliberately does NOT answer "which ones": the query filters by
    /// institution separately, so that a reviewer with no institution claim sees an empty list rather
    /// than everyone's.
    /// </summary>
    public static bool CanReviewAnything(ClaimsPrincipal principal)
        => principal.IsAdministrator() || HasAnyRole(principal, ReviewRoles);

    /// <summary>
    /// One message for every refusal, so a caller cannot tell "no such request" from "not yours" or
    /// "wrong institution" by reading the error.
    /// </summary>
    public const string RefusalMessage = "You are not authorized to access this data-rights request.";

    private static bool CanAct(ClaimsPrincipal principal, DataRightsRequest request, string[] roles)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(request);

        if (principal.IsAdministrator())
        {
            return true;
        }

        if (!HasAnyRole(principal, roles))
        {
            return false;
        }

        // A null stamp matches nobody but a global Administrator. Erasure is irreversible, so an
        // unplaceable request withholds the power rather than spreading it.
        return request.InstitutionId is int institutionId &&
               principal.GetInstitutionId() == institutionId;
    }

    private static bool HasAnyRole(ClaimsPrincipal principal, string[] roles)
    {
        foreach (var role in roles)
        {
            if (principal.IsInRole(role))
            {
                return true;
            }
        }

        return false;
    }
}
