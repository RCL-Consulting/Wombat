using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.Dashboards.Oversight;

namespace Wombat.Application.Features.Dashboards.CommitteeMember;

/// <summary>
/// The Committee member's Home (T358, flow 06; R2-Home c1–c11): Registrars, the institution's current registrars fewest
/// EPAs met first, and Targets by EPA, each EPA's "n of m registrars met".
/// </summary>
/// <remarks>
/// Before T130 the card listed trainees "approaching completion": at least 80% of a lifetime total nobody published. Until
/// T358 it was "Targets this period", every trainee by name with "semester 1/5 · yearly 0/1" and no link; it is now
/// Programme trainees' first five, each a link to the registrar's page, in My progress's figures.
/// </remarks>
/// <param name="Registrars">Programme trainees' first five and how many it holds.</param>
/// <param name="Coverage">The same registrars' Targets by EPA (one roster read, so the two cards count the same people).</param>
public sealed record CommitteeMemberDashboardSummaryDto(RegistrarsCardDto Registrars, CurriculumCoverage Coverage);
