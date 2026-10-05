using Wombat.Application.Features.Curricula.Quota;
using Wombat.Application.Features.Dashboards.Oversight;
using Wombat.Application.Features.Programme.Waiting;

namespace Wombat.Application.Features.Dashboards.SubSpecialityAdmin;

/// <summary>
/// The Sub-speciality admin's Home (T358, flow 06; R2-Home a5): Waiting for assessors, Registrars and Targets by EPA, each
/// read as Sub-speciality admin, in the sub-speciality's scope at the admin's institution.
/// </summary>
/// <remarks>
/// Until T358 it was "Pending reviews" (a count of the programme's backlog that linked nowhere), "Trainees in programme"
/// ("4 active / 1 inactive") and "Curriculum coverage — Semester 2, 2026". The backlog is now its list's first five, each
/// row its link, and no "inactive" is counted anywhere (Q3, Q10).
/// </remarks>
/// <param name="Waiting">Waiting for assessors' first five and its counts; null when the role gives no scope.</param>
/// <param name="Registrars">Programme trainees' first five and how many it holds.</param>
/// <param name="Coverage">The same registrars' Targets by EPA.</param>
public sealed record SubSpecialityAdminDashboardSummaryDto(
    WaitingForAssessorsDto? Waiting,
    RegistrarsCardDto Registrars,
    CurriculumCoverage Coverage);
