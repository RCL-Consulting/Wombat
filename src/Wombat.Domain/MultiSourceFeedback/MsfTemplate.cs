namespace Wombat.Domain.MultiSourceFeedback;

public sealed class MsfTemplate
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? SpecialityId { get; set; }
    public bool AllowPatientResponses { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<MsfQuestion> Questions { get; set; } = [];
}
