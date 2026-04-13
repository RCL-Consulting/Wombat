namespace Wombat.Domain.MultiSourceFeedback;

public sealed class MsfResponseAnswer
{
    public int Id { get; set; }
    public int ResponseId { get; set; }
    public int QuestionId { get; set; }
    public int? ScaleValue { get; set; }
    public string? LongText { get; set; }

    public MsfResponse Response { get; set; } = null!;
    public MsfQuestion Question { get; set; } = null!;
}
