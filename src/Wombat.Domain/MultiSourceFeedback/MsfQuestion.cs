namespace Wombat.Domain.MultiSourceFeedback;

public sealed class MsfQuestion
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    public int Order { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public MsfQuestionType Type { get; set; }
    public int? ScaleId { get; set; }
    public bool Required { get; set; }

    public MsfTemplate Template { get; set; } = null!;
}
