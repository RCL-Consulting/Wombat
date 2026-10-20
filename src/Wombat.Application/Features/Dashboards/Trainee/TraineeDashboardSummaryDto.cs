using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.EntrustmentDecisions;

namespace Wombat.Application.Features.Dashboards.Trainee;

/// <summary>
/// The Trainee's Home, in one read behind <c>DashboardFrame</c>, so a failure is Home's one load error (T355, R1; note 3):
/// Your targets, Needs you, Recent decisions and My authorisations. Recent activities and Upcoming deadlines are retired
/// (Q3; T298): a date is shown on the row it belongs to.
/// </summary>
/// <param name="CurriculumTargets">
/// The trainee's curriculum progress for the current period: the same read model the progress page uses (T130). For a
/// trainee whose programme has ended, the one they ended on, marked as ended (T252). Null for a pending trainee or one
/// with no profile at all.
/// </param>
/// <param name="NeedsYou">
/// Every row of the caller's Needs you, most recently updated first: their drafts and the work returned to them, read by
/// the code <c>ListNeedsYouQuery</c> reads them with (<c>NeedsYou.ReadAsync</c>; T297's rule, restated by T342). Home's
/// card lists the first <c>NeedsYouListed</c> and counts them all.
/// </param>
/// <param name="RecentDecisions">
/// What someone else decided on the caller's own requests, newest decision first, the first
/// <c>RecentDecisionsListed</c> (<c>DecidedOnYours</c>, T355, B1; E6), each with its count line (E5).
/// </param>
/// <param name="Standing">
/// The standing against Annexure A in summary mode (<c>EntrustmentStandingReader</c>, note 3): no latest rating is read.
/// As on the day the targets are read for (the programme's last day once it has ended). Null for a pending trainee or one
/// with no profile.
/// </param>
public sealed record TraineeDashboardSummaryDto(
    TraineeCurriculumProgressSummaryDto? CurriculumTargets,
    IReadOnlyList<ActivitySummaryDto> NeedsYou,
    IReadOnlyList<ActivitySummaryDto> RecentDecisions,
    EntrustmentStandingDto? Standing,
    bool IsPendingTrainee)
{
    /// <summary>
    /// A pending registrar's wait card (T373, flow 11, P01–P07): where and as what she waits, who invited her, and what
    /// admission will do. Null for a trainee, and for a pending trainee whose account could not be read.
    /// </summary>
    public PendingAdmissionDto? Pending { get; init; }
}

/// <summary>
/// What the pending registrar's Home says while she waits (T373, flow 11, F3, F8, M2, S5; build-contracts § 3).
/// </summary>
/// <param name="Email">Her address as stored, which the last step names ("An email tells you, at ‹address›, …").</param>
/// <param name="InstitutionName">Her institution; null for an account of none.</param>
/// <param name="SpecialityName">The invited speciality, which the lead names ("the Paediatrics programme"); null with no invited scope (check rule 4).</param>
/// <param name="InvitedAs">"Paediatrics / Paediatrics": the Programme line's first part; null with no invited scope, and no Programme line is shown (S5).</param>
/// <param name="CurriculumName">The curriculum adopted at her institution for her invited sub-speciality (M2); null when none is.</param>
/// <param name="CurriculumVersion">Its version, with <paramref name="CurriculumName" />.</param>
/// <param name="InvitedByName">
/// The Institutional admin who invited her (F3): only when the invitation she registered from was issued as one
/// (<c>Invitation.IssuedAsRole</c>), and the issuer, read now, exists and is not deactivated or erased. Null otherwise.
/// </param>
/// <param name="InvitedOn">The invitation's issue day on the South African calendar (check rule 7: not the day she registered), with <paramref name="InvitedByName" />.</param>
public sealed record PendingAdmissionDto(
    string Email,
    string? InstitutionName,
    string? SpecialityName,
    string? InvitedAs,
    string? CurriculumName,
    string? CurriculumVersion,
    string? InvitedByName,
    DateOnly? InvitedOn);
