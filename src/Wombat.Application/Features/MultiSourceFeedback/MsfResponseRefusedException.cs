namespace Wombat.Application.Features.MultiSourceFeedback;

/// <summary>
/// Why a respondent's link, or what they sent through it, was refused. (T202)
/// </summary>
public enum MsfResponseRefusal
{
    /// <summary>No invitation's stored token matches the link: mistyped, or replaced by a newer link (T132).</summary>
    LinkNotRecognised,

    /// <summary>The invitation's own expiry date has passed.</summary>
    LinkExpired,

    /// <summary>The invitation was revoked.</summary>
    LinkRevoked,

    /// <summary>A response was already submitted through the link. A link takes one.</summary>
    LinkUsed,

    /// <summary>The campaign is not open: closed, withdrawn or released.</summary>
    CampaignNotOpen,

    /// <summary>The link is good, but the answers sent through it are not complete.</summary>
    AnswersIncomplete
}

/// <summary>
/// A refusal meant for the respondent: the one reader of an MSF link, who is not signed in. (T202)
/// </summary>
/// <remarks>
/// <para>
/// Its message is written to be shown to them as it stands, and <see cref="Reason" /> decides what they are answered
/// with (<see cref="MsfResponseRefusals.Describe" />): a status the respondent's browser can act on, and a page or body
/// that says what happened. Until T202 every one of these was a bare <see cref="InvalidOperationException" />, which the
/// Api did not handle, so a used or dead link answered 500 with no body.
/// </para>
/// <para>
/// Derived from <see cref="InvalidOperationException" /> so that nothing that already catches a refusal changes. The Api
/// maps this type and no other: an <see cref="InvalidOperationException" /> that is not one of these is a fault, and
/// stays a 500.
/// </para>
/// </remarks>
public sealed class MsfResponseRefusedException : InvalidOperationException
{
    public MsfResponseRefusedException(MsfResponseRefusal reason, string message)
        : base(message)
    {
        Reason = reason;
    }

    /// <summary>A refusal the database made first: a second response through a link that has just taken one (T205).</summary>
    public MsfResponseRefusedException(MsfResponseRefusal reason, string message, Exception innerException)
        : base(message, innerException)
    {
        Reason = reason;
    }

    public MsfResponseRefusal Reason { get; }

    /// <summary>
    /// The question an <see cref="MsfResponseRefusal.AnswersIncomplete" /> refusal is about, when it is about one: the
    /// respondent's page marks that question as the one to put right (T205).
    /// </summary>
    public int? QuestionId { get; init; }
}

/// <summary>
/// What each refusal answers with, wherever a respondent meets it: an HTTP status and a title. One mapping for both
/// places a respondent answers, the web app's respondent page and the Api's respond endpoint, so the two cannot drift.
/// (T202, T205)
/// </summary>
public static class MsfResponseRefusals
{
    /// <summary>
    /// A link that names nothing is 404; a link that did name an invitation but can no longer be used is 410, because it
    /// will never work again; answers that do not complete the form are 400.
    /// </summary>
    /// <remarks>
    /// Every value has an answer, and each is a 4xx: a refusal is the respondent's news, never a fault. A value added
    /// later without one throws here, which a test asks of every value.
    /// </remarks>
    public static (int Status, string Title) Describe(MsfResponseRefusal reason) => reason switch
    {
        MsfResponseRefusal.LinkNotRecognised => (404, "Feedback link not recognised"),
        MsfResponseRefusal.LinkExpired => (410, "Feedback link expired"),
        MsfResponseRefusal.LinkRevoked => (410, "Feedback link revoked"),
        MsfResponseRefusal.LinkUsed => (410, "Feedback link already used"),
        MsfResponseRefusal.CampaignNotOpen => (410, "Feedback request closed"),
        MsfResponseRefusal.AnswersIncomplete => (400, "The response is not complete"),
        _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, "An MSF refusal with no answer.")
    };
}
