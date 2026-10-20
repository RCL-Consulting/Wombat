using System.Globalization;
using Wombat.Application.Common.Email;

namespace Wombat.Application.Common.Email.Templates;

public static class InvitationEmail
{
    /// <summary>The mail's subject (S12).</summary>
    public const string Subject = "Your Wombat invitation";

    /// <param name="roleAndScope">
    /// What the invitation is for, in the pages' words: the role's label and its scope, "Institutional admin at Kgosi Kgari
    /// Teaching Hospital" (<c>InvitationScopeWords.RoleAndScope</c>; T373, flow 11, S12). Until T373 the mail said the
    /// role's key, "as InstitutionalAdmin", in its words and in a tag.
    /// </param>
    /// <param name="expiresOn">The link's last day on the South African calendar: it works until the end of it (F7).</param>
    /// <param name="deliveryKey">
    /// What the mail worker hands back with the mail's outcome: the invitation and its link
    /// (<c>Invitation.DeliveryKey</c>), so that the invitations list can say whether it arrived (T283).
    /// </param>
    public static EmailMessage Build(
        string toEmail, string roleAndScope, string registrationUrl, DateOnly expiresOn, string deliveryKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleAndScope);
        ArgumentException.ThrowIfNullOrWhiteSpace(deliveryKey);

        var lastDay = expiresOn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var invited = $"You are invited to Wombat as {roleAndScope}.";
        var works = $"Register with this link; it works until {lastDay}.";

        var html = EmailTemplateBase.WrapHtml(Subject, $"""
            <p>{System.Net.WebUtility.HtmlEncode(invited)}</p>
            <p>{System.Net.WebUtility.HtmlEncode(works)}</p>
            <p><a class="btn" href="{System.Net.WebUtility.HtmlEncode(registrationUrl)}">Register</a></p>
            <p>Or copy this link into your browser:<br><code>{System.Net.WebUtility.HtmlEncode(registrationUrl)}</code></p>
            """);

        var text = $"""
            {invited}

            {works}
            {registrationUrl}
            """;

        // No role in the tags either (S12): the mail's own words are its only record of what it is for.
        return new EmailMessage(
            To: toEmail,
            Subject: Subject,
            HtmlBody: html,
            TextBody: text,
            Tags: ["invitation"],
            DeliveryKey: deliveryKey);
    }
}
