namespace Wombat.Application.Features.Dashboards.SpecialityAdmin;

public sealed record SpecialityAdminDashboardSummaryDto(
    int PendingReviewCount,
    int ActiveTraineeCount,
    int InactiveTraineeCount,
    IReadOnlyList<EpaCoverageItem> CurriculumCoverage);

public sealed record EpaCoverageItem(
    string EpaTitle,
    double AverageCompletionPercent);
