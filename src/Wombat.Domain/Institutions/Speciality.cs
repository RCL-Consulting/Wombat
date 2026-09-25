namespace Wombat.Domain.Institutions;

public sealed class Speciality
{
    public int Id { get; set; }
    public int CollegeId { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The key a seeder finds this row by, such as <c>cpsa:paediatrics</c>, or null for a row an administrator made
    /// (T221). Written when a seeder creates the row and never changed after: no command writes it, so neither a rename
    /// nor a move to another College can lose it.
    /// </summary>
    public string? SeedKey { get; init; }

    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public College College { get; set; } = null!;
    public ICollection<SubSpeciality> SubSpecialities { get; set; } = [];
}
