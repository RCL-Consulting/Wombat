namespace Wombat.Application.Features.Dashboards.SubSpecialityAdmin;

public sealed record SubSpecialityAdminDashboardSummaryDto(
    int PendingReviewCount,
    int ActiveTraineeCount,
    int InactiveTraineeCount,
    IReadOnlyList<EpaCoverageItem> CurriculumCoverage);

public sealed record EpaCoverageItem(
    string EpaTitle,
    double AverageCompletionPercent);
