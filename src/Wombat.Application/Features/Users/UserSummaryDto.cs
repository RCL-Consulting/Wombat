namespace Wombat.Application.Features.Users;

/// <summary>
/// One row of Users. In a file of its own since T376 (flow 12, build-contracts § 0) so the Users lane (U) and the user page
/// lane (P) change their own DTOs without meeting in one file.
/// </summary>
/// <param name="InstitutionName">The person's institution's name; null for an account with none (the page writes
/// <see cref="PersonScopeWords.NoInstitution" />, U14).</param>
/// <param name="Roles">The keys held, in <c>WombatRoleLabels.InOrder</c>'s order (T376, Q14); empty for a person with none.</param>
/// <param name="Scope">The person's one scope in words (<see cref="PersonScopeWords" />, S1).</param>
/// <param name="IsDeactivated">"Locked out": the administrative lock, never Identity's self-lifting brute-force lockout
/// (T376, K25; build-contracts § 1).</param>
/// <param name="HasEndedProgramme">The person holds no role and a trainee programme of theirs has ended (S2, D54): the No
/// role cell then says a completed or withdrawn registrar keeps an account and its records.</param>
public sealed record UserSummaryDto(
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    int? InstitutionId,
    string? InstitutionName,
    IReadOnlyList<string> Roles,
    string Scope,
    bool IsDeactivated,
    bool HasEndedProgramme = false);

/// <summary>One page of Users (T376, flow 12, S6, F9).</summary>
/// <param name="Rows">The page's people, by last name (S7).</param>
/// <param name="TotalCount">Everyone in the caller's scope: "10" in the heading, "of 10" after a filter (U02).</param>
/// <param name="MatchCount">Everyone the filter matched, across every page: the pager's total (F9).</param>
/// <param name="Page">The page answered, from 1.</param>
/// <param name="PageSize">The page's size: 10, 20, 50 or 100 (Q22).</param>
public sealed record UserListDto(
    IReadOnlyList<UserSummaryDto> Rows,
    int TotalCount,
    int MatchCount,
    int Page,
    int PageSize);
