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
/// Its message is written to be shown to them as it stands, and <see cref="Reason" /> is what the Api's respond
/// endpoint answers with: a status the respondent's browser can act on and a body that says what happened. Until T202
/// every one of these was a bare <see cref="InvalidOperationException" />, which the Api did not handle, so a used or
/// dead link answered 500 with no body.
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

    public MsfResponseRefusal Reason { get; }
}
