using Wombat.Domain.Audit;

namespace Wombat.Application.Features.Audit;

/// <summary>
/// One row of the audit log (T400, flow 18, Q8, T19, T20, K8, N10): what was done, when, by whom and with what result.
/// The subject and the address are the entry's, not the row's (N10).
/// </summary>
/// <param name="Id">The entry's id, which its Detail opens.</param>
/// <param name="OccurredAt">When, in UTC; the page prints it in SAST.</param>
/// <param name="Category">The category; the page prints its label (K15).</param>
/// <param name="Action">The action's stored name ("CreateActivityCommand", "Login").</param>
/// <param name="Success">Whether it succeeded.</param>
/// <param name="ErrorMessage">A failure's stored text: a refusal's words, or "The action failed." (K10).</param>
/// <param name="ActorKind">Who acted, by kind (K8).</param>
/// <param name="PersonName">
/// A person's name as read now (Q15, M3: the live "First Last", an erased person's pseudonym, an id with no name); for a row
/// with no user id the name stored with it, if any; null for the MSF respondent.
/// </param>
/// <param name="ActorUserId">The person's user id, as stored.</param>
/// <param name="InstitutionId">The row's institution, null for none.</param>
/// <param name="InstitutionName">That institution's name, read now; null for none.</param>
public sealed record AuditLogRowDto(
    Guid Id,
    DateTime OccurredAt,
    AuditCategory Category,
    string Action,
    bool Success,
    string? ErrorMessage,
    AuditActorKind ActorKind,
    string? PersonName,
    string? ActorUserId,
    int? InstitutionId,
    string? InstitutionName);

/// <summary>An institution the Administrator's Institution filter offers, by name (T19).</summary>
public sealed record AuditInstitutionOptionDto(int Id, string Name);

/// <summary>
/// One audit entry, as its page reads it (T400, flow 18, Q9, T4): What happened, Who, What it was done to, and the payload
/// for the Administrator alone.
/// </summary>
/// <param name="ActorIpAddress">The address as stored, cut to /24 or /48 (K9).</param>
/// <param name="SpecialityName">The row's speciality, by name; null for none.</param>
/// <param name="SummaryJson">The payload as stored; null for anyone but the Administrator (R2-E01).</param>
public sealed record AuditEntryDetailDto(
    Guid Id,
    DateTime OccurredAt,
    AuditCategory Category,
    string Action,
    bool Success,
    string? ErrorMessage,
    AuditActorKind ActorKind,
    string? PersonName,
    string? ActorUserId,
    string? ActorIpAddress,
    int? InstitutionId,
    string? InstitutionName,
    string? SubjectType,
    Guid? SubjectId,
    int? SpecialityId,
    string? SpecialityName,
    string? SummaryJson);
