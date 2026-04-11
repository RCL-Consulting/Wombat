namespace Wombat.Domain.Curricula;

public sealed class CurriculumItem
{
    public int Id { get; set; }
    public int CurriculumId { get; set; }
    public int EpaId { get; set; }
    public int RequiredCount { get; set; }
    public int MinimumLevelOrder { get; set; }
    public int WindowMonths { get; set; }
    public double? Weight { get; set; }

    public Curriculum Curriculum { get; set; } = null!;
    public Wombat.Domain.Epas.Epa Epa { get; set; } = null!;
}
