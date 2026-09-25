namespace Wombat.Application.Common.Email;

/// <param name="Tags">What kind of mail this is, for the logs: every line about a mail prints them.</param>
/// <param name="DeliveryKey">
/// What the mail worker hands back with this mail's outcome, to whoever records it
/// (<see cref="IEmailDeliveryObserver" />); null when nobody asks. Not a tag, and never logged: an MSF link's key names
/// the respondent's invitation (<c>MsfInvitation.DeliveryKey</c>), and a log line pairs it with the address. (T251)
/// </param>
public sealed record EmailMessage(
    string To,
    string Subject,
    string HtmlBody,
    string TextBody,
    string? Cc = null,
    IReadOnlyList<string>? Tags = null,
    string? DeliveryKey = null);
