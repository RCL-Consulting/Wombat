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
/// T112 kept the two role sets as they were. T387 settled who holds them (Q1, Q21; the statutory data-rights officer
/// stays open, T112, Q15): one set, <see cref="ReviewRoles" />, the Coordinator at the request's institution and the
/// global Administrator. Whoever may approve a Rectification records each correction and completes it; the Speciality
/// admin, who could record one but could open neither page, is dropped. And nobody decides or carries through their own
/// request, the Administrator included: it needs another reviewer.
/// </para>
/// </remarks>
public static class DataRightsAuthorization
{
    /// <summary>
    /// Roles that may decide a request in their institution, record its corrections and complete it (with the global
    /// Administrator, everywhere). T387 (Q1) folded the old RectificationRoles (SpecialityAdmin) into this one.
    /// </summary>
    private static readonly string[] ReviewRoles = [WombatRoles.Coordinator];

    /// <summary>
    /// The caller may approve or reject the request: in scope (<see cref="InScope" />) and not its requester (T387, Q21,
    /// F9). Whether it is still Submitted is the request's own rule, not this one.
    /// </summary>
    public static bool CanReview(ClaimsPrincipal principal, DataRightsRequest request)
        => InScope(principal, request) && !IsRequester(principal, request);

    /// <summary>
    /// The caller may record a correction on the request and complete it: the same people as <see cref="CanReview" />
    /// (T387, Q1), so one person approves, records and completes.
    /// </summary>
    public static bool CanRectify(ClaimsPrincipal principal, DataRightsRequest request)
        => CanReview(principal, request);

    /// <summary>
    /// The caller is the requester and would otherwise decide it (T387, Q21, F9): the page says who decides instead of
    /// offering Decide.
    /// </summary>
    public static DataRightsOwnRequest OwnRequest(ClaimsPrincipal principal, DataRightsRequest request)
    {
        if (!IsRequester(principal, request) || !InScope(principal, request))
        {
            return DataRightsOwnRequest.No;
        }

        return principal.IsAdministrator()
            ? DataRightsOwnRequest.AnotherAdministratorDecides
            : DataRightsOwnRequest.AnotherCoordinatorDecides;
    }

    /// <summary>The exported bundle: the data subject, or a global Administrator (<see cref="DemandExportAccess" />).</summary>
    public static bool CanDownload(ClaimsPrincipal principal, DataRightsRequest request)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(request);

        return principal.IsAdministrator() || IsRequester(principal, request);
    }

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
        if (!CanRead(principal, request))
        {
            throw new UnauthorizedAccessException(RefusalMessage);
        }
    }

    /// <summary>
    /// The requester, or a reviewer in scope, the reviewer's own request included: a Coordinator opens their own from the
    /// queue and reads who decides it (T387, Q21).
    /// </summary>
    public static bool CanRead(ClaimsPrincipal principal, DataRightsRequest request)
        => InScope(principal, request) || IsRequester(principal, request);

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
        if (!CanDownload(principal, request))
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

    /// <summary>
    /// A reviewer whose scope reaches the request: a global Administrator, or a holder of <see cref="ReviewRoles" /> at the
    /// request's institution. Says nothing of who asked: <see cref="CanReview" /> adds that.
    /// </summary>
    private static bool InScope(ClaimsPrincipal principal, DataRightsRequest request)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(request);

        if (principal.IsAdministrator())
        {
            return true;
        }

        if (!HasAnyRole(principal, ReviewRoles))
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
