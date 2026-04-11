namespace Wombat.Domain.Forms;

public sealed class FormCriterion
{
    public int Id { get; set; }
    public int FormId { get; set; }
    public int Order { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public string? HelpText { get; set; }
    public bool IsRequired { get; set; } = true;

    public AssessmentForm Form { get; set; } = null!;
}
