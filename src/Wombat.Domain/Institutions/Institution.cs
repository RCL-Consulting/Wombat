namespace Wombat.Domain.Institutions;

public sealed class Institution
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortCode { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    // Specialities are owned by College (national), not Institution — see T091.
    // Institution-scoped concepts (trainees, activity types, forms) carry a direct InstitutionId.
}
