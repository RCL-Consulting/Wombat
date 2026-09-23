using Wombat.Application.Features.Curricula.Quota;

namespace Wombat.Application.Features.Dashboards.SpecialityAdmin;

/// <param name="CurriculumCoverage">
/// Each EPA's target for the current period, as "n of m trainees met it" (T130). It replaces a mean of lifetime
/// percentages that counted exempt trainees as nought and, once progress was per semester, passed 100%.
/// </param>
public sealed record SpecialityAdminDashboardSummaryDto(
    int PendingReviewCount,
    int ActiveTraineeCount,
    int InactiveTraineeCount,
    CurriculumCoverage CurriculumCoverage);
