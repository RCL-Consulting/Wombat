namespace Wombat.Domain.MultiSourceFeedback;

public sealed class MsfResponseAnswer
{
    /// <summary>
    /// The longest comment an answer holds: the column's length. A longer one is refused as the respondent's to shorten,
    /// not left to fail the save as a fault (T205). The respondent's page caps its text boxes at the same number.
    /// </summary>
    public const int LongTextMaxLength = 4000;

    /// <summary>
    /// A comment with every line break as one line feed, as the text box it was typed in counted it. A browser posts each
    /// line break of a text box as CRLF (two characters) while the box's <c>maxlength</c> counts one, so a comment the box
    /// accepted could otherwise measure over <see cref="LongTextMaxLength" /> and be refused (T205).
    /// </summary>
    public static string? NormaliseLineBreaks(string? comment)
        => comment?.ReplaceLineEndings("\n");

    public int Id { get; set; }
    public int ResponseId { get; set; }
    public int QuestionId { get; set; }
    public int? ScaleValue { get; set; }
    public string? LongText { get; set; }

    public MsfResponse Response { get; set; } = null!;
    public MsfQuestion Question { get; set; } = null!;
}
