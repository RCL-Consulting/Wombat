namespace Wombat.Application.Features.Dashboards.InstitutionalAdmin;

/// <summary>
/// Her Home (T376, flow 12, Q7, Q8, K24; boards H01–H05): the waiting line, People, Curriculum and Your institution, each
/// read for her institution only.
/// </summary>
/// <param name="UsersByRole">Each role held by one or more accounts at her institution, in <c>WombatRoles.All</c>'s order,
/// with its count: Home counts every role held (round 2's PROPOSED, settled), so the counts may sum past
/// <paramref name="AccountCount" />.</param>
/// <param name="AccountCount">Her institution's accounts, each once, those with no role included (K24: "15 accounts.").</param>
/// <param name="OnlyAccountIsYours">Her own is the institution's one account (1.10: "1 account: yours.").</param>
/// <param name="WaitingCount">Accounts at her institution waiting to be admitted (Pending trainee, Q8): the line above the
/// cards when above 0.</param>
/// <param name="AdoptedSpecialityCount">The specialities her institution has an active adoption in (T291 item 4: not the
/// College's catalogue).</param>
/// <param name="AdoptedSubSpecialityCount">The sub-specialities her institution has an active adoption for.</param>
/// <param name="Adoptions">Her institution's active adoptions, one a discipline, for the Curriculum card's line.</param>
/// <param name="Institution">Her institution's record (Q3); null when the claim names none, or one that is gone.</param>
public sealed record InstitutionalAdminDashboardSummaryDto(
    IReadOnlyList<RoleCountItem> UsersByRole,
    int AccountCount,
    bool OnlyAccountIsYours,
    int WaitingCount,
    int AdoptedSpecialityCount,
    int AdoptedSubSpecialityCount,
    IReadOnlyList<AdoptedCurriculumItem> Adoptions,
    InstitutionCardItem? Institution);

/// <summary>A role's key and how many accounts at her institution hold it.</summary>
public sealed record RoleCountItem(string Role, int Count);

/// <summary>
/// One active adoption, as the Curriculum card names it: "Paediatrics / Paediatrics, from the CPSA's Paediatric EPA
/// Curriculum (11.1)." (H01).
/// </summary>
public sealed record AdoptedCurriculumItem(
    string SpecialityName,
    string SubSpecialityName,
    string CollegeShortCode,
    string CurriculumName,
    string CurriculumVersion);

/// <summary>Her institution's record as Your institution reads it (Q3, H01): Status is set by a Wombat administrator.</summary>
public sealed record InstitutionCardItem(int Id, string Name, string ShortCode, string? ContactEmail, bool IsActive);
