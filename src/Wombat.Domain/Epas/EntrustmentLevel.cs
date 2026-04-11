namespace Wombat.Domain.Epas;

public sealed class EntrustmentLevel
{
    public int Id { get; set; }
    public int ScaleId { get; set; }
    public int Order { get; set; }
    public string Label { get; set; } = string.Empty;
    public string? Description { get; set; }

    public EntrustmentScale Scale { get; set; } = null!;
}
