namespace Wombat.Domain.Institutions;

public sealed class Speciality
{
    public int Id { get; set; }
    public int CollegeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public College College { get; set; } = null!;
    public ICollection<SubSpeciality> SubSpecialities { get; set; } = [];
}
