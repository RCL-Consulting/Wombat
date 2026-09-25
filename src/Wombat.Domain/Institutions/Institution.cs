namespace Wombat.Domain.Institutions;

public sealed class Institution
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortCode { get; set; } = string.Empty;

    /// <summary>
    /// The key a seeder finds this row by, <c>demo</c> for the Demo Institution, or null for an institution an
    /// administrator made (T229). Written when a seeder creates the row and never changed after: no command writes it, so
    /// a new name or short code cannot lose it.
    /// </summary>
    public string? SeedKey { get; init; }

    public string? ContactEmail { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    // Specialities are owned by College (national), not Institution — see T091.
    // Institution-scoped concepts (trainees, activity types, forms) carry a direct InstitutionId.
}
