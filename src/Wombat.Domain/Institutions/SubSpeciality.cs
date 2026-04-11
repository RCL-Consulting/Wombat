namespace Wombat.Domain.Institutions;

public sealed class SubSpeciality
{
    public int Id { get; set; }
    public int SpecialityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public Speciality Speciality { get; set; } = null!;
}
