namespace Wombat.Domain.Institutions;

/// <summary>
/// A national governing body that owns a discipline's EPAs and curricula — a constituent
/// College of the Colleges of Medicine of South Africa (CMSA), e.g. the College of
/// Paediatricians. Training institutions adopt a College's curriculum versions (see T091);
/// they do not author the national catalogue. Specialities are re-parented onto College in
/// T091 phase 2 — this entity is introduced additively in phase 1.
/// </summary>
public sealed class College
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortCode { get; set; } = string.Empty;

    /// <summary>
    /// The key a seeder finds this row by, such as <c>cpsa</c>, or null for a row an administrator made (T221). Written
    /// when a seeder creates the row and never changed after: no command writes it, so a rename cannot lose it.
    /// </summary>
    public string? SeedKey { get; init; }

    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public ICollection<Speciality> Specialities { get; set; } = [];
}
