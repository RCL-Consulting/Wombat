namespace Wombat.Application.Common.Options;

public sealed class DashboardThresholds
{
    public const string SectionName = "DashboardThresholds";

    public int AssessorDueDays { get; set; } = 7;
    public int CoordinatorStallDays { get; set; } = 7;
    public int CommitteeCompletionPercent { get; set; } = 80;
}
