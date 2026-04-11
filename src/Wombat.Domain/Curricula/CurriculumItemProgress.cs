namespace Wombat.Domain.Curricula;

public sealed class CurriculumItemProgress
{
    public int Id { get; set; }
    public int CurriculumItemId { get; set; }
    public string TraineeUserId { get; set; } = string.Empty;
    public int CountsSoFar { get; set; }
    public int MinimumLevelReachedCount { get; set; }
    public int? LastActivityId { get; set; }
    public DateTime LastUpdated { get; set; }
    public string CreditedActivityKeysJson { get; set; } = "[]";

    public CurriculumItem CurriculumItem { get; set; } = null!;
}
