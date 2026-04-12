namespace Wombat.Domain.Activities;

public sealed class ProcedureCatalogueEntry
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}
