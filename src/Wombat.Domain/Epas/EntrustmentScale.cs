namespace Wombat.Domain.Epas;

public sealed class EntrustmentScale
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<EntrustmentLevel> Levels { get; set; } = [];
    public ICollection<Wombat.Domain.Forms.AssessmentForm> Forms { get; set; } = [];
}
