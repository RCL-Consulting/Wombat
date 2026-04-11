namespace Wombat.Domain.Forms;

public sealed class FormEpaLink
{
    public int Id { get; set; }
    public int FormId { get; set; }
    public int EpaId { get; set; }

    public AssessmentForm Form { get; set; } = null!;
    public Wombat.Domain.Epas.Epa Epa { get; set; } = null!;
}
