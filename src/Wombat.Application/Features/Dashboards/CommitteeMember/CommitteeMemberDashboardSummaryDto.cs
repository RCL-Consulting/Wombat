namespace Wombat.Application.Features.Dashboards.CommitteeMember;

public sealed record CommitteeMemberDashboardSummaryDto(
    IReadOnlyList<TraineeNearCompletionItem> TraineesNearCompletion,
    IReadOnlyList<ProgrammeProgressItem> ProgrammeProgress);

public sealed record TraineeNearCompletionItem(
    string TraineeUserId,
    string TraineeName,
    double OverallCompletionPercent);

public sealed record ProgrammeProgressItem(
    string EpaTitle,
    double AverageCompletionPercent);
