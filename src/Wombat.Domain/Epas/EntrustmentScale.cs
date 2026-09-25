namespace Wombat.Domain.Epas;

public sealed class EntrustmentScale
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The key a seeder finds this row by, such as <c>cpsa:scale:v11.1</c>, or null for a scale an administrator made
    /// (T221). Written when a seeder creates the row and never changed after: no command writes it, so a rename cannot
    /// lose it.
    /// </summary>
    public string? SeedKey { get; init; }

    public string? Description { get; set; }

    public ICollection<EntrustmentLevel> Levels { get; set; } = [];
}
