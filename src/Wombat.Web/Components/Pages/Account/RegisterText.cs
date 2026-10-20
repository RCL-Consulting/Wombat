using Wombat.Application.Features.Invitations;
using Wombat.Web.Security;

namespace Wombat.Web.Components.Pages.Account;

/// <summary>What the register page shows: the form, or one of its cards (T373, flow 11, R3-G01–G18).</summary>
public enum RegisterCard
{
    /// <summary>G01–G06: the form, with a refusal under its field when the last submit met one.</summary>
    Form,

    /// <summary>G07: the invitation has been used.</summary>
    Used,

    /// <summary>G08: the invitation was revoked.</summary>
    Revoked,

    /// <summary>G09: no invitation matches the token (a link a resend replaced, or one cut short).</summary>
    Invalid,

    /// <summary>G10: the link's last day has passed on the South African calendar.</summary>
    Expired,

    /// <summary>G11: an account holds the address.</summary>
    AccountExists,

    /// <summary>G12: no account can be made for the address.</summary>
    AddressNotAccepted,

    /// <summary>G13: the page opened with neither the cookie nor a token (S7).</summary>
    LinkIncomplete,

    /// <summary>G14: a fault; F10's one transaction means nothing was saved.</summary>
    NotCompleted,

    /// <summary>G15: the account was created but could not be read back to sign it in.</summary>
    NotSignedIn,

    /// <summary>G16: the form was posted after the cookie's 30 minutes (S7).</summary>
    TimedOut,

    /// <summary>G18: someone is signed in on this browser (Q11, M4).</summary>
    SignedInElsewhere,

    /// <summary>
    /// The form posted was shown for another invitation than the cookie holds now: a second link was opened in this
    /// browser since (T373 fix pass, B1). No board draws it; G16's card would say a time had passed that had not.
    /// </summary>
    OtherInvitation
}

/// <summary>
/// The register page's words (T373, flow 11): each card's sentences as the boards draw them; the headings are the page's
/// own (<c>Register.Heading</c>), where the tab-title check reads them. Each refusal's first sentence is <see cref="InvitationRefusals.Describe" />'s or
/// <see cref="RegisterOutcome" />'s as built (build-contracts § 4), then the board's next step.
/// </summary>
public static class RegisterText
{
    /// <summary>The Email field's help: the address cannot be edited here (Q6).</summary>
    public const string EmailHelp = "The address this invitation is for. It cannot be changed here.";

    /// <summary>"Ask the person who invited you for a new invitation.": G08's and G10's next step.</summary>
    public const string AskForANewInvitation = "Ask the person who invited you for a new invitation.";

    /// <summary>G07's first sentence: <see cref="InvitationRefusals.Describe" />'s, with what it means.</summary>
    public const string UsedMessage = "This invitation has already been used: an account was registered from it.";

    public const string UsedNextStep = "If it was yours, sign in with that account.";

    public const string InvalidNextStep =
        "The link may be incomplete, or a newer invitation email may have replaced it. Open the link in the most recent " +
        "invitation email, or ask the person who invited you for a new invitation.";

    public const string AccountExistsNextStep = "Sign in with that account.";

    public const string LinkIncompleteNextStep =
        "Open the link from the invitation email again. If the email split it over two lines, copy all of it into the address bar.";

    public const string NotCompletedNextStep =
        "The invitation is still open in this browser for 30 minutes from when you opened its link.";

    public const string NotSignedInNextStep =
        "Your account was created. Sign in with this email address and the password you chose.";

    public const string TimedOutMessage =
        "This page held the invitation for 30 minutes, and that time has passed. Nothing was saved.";

    public const string TimedOutNextStep = "Try again: open the link in the invitation email once more.";

    public const string SignedInElsewhereNextStep = "To register it, sign out first. Your own account is not changed.";

    /// <summary>The other-invitation card's first sentence (B1).</summary>
    public const string OtherInvitationMessage =
        "Another invitation's link was opened in this browser after this form was shown, so nothing was registered.";

    /// <summary>What comes before the address the cookie holds now; the page writes the address, with its break after the @.</summary>
    public const string OtherInvitationHeldNow = "The invitation open in this browser now is for ";

    public const string OtherInvitationNextStep =
        "Continue to register it, or open the link in your own invitation email again.";

    /// <summary>The card for an invitation's refusal.</summary>
    public static RegisterCard CardFor(InvitationRefusal reason) => reason switch
    {
        InvitationRefusal.Used => RegisterCard.Used,
        InvitationRefusal.Revoked => RegisterCard.Revoked,
        InvitationRefusal.Invalid => RegisterCard.Invalid,
        InvitationRefusal.Expired => RegisterCard.Expired,
        InvitationRefusal.AccountExists => RegisterCard.AccountExists,
        InvitationRefusal.AddressNotAccepted => RegisterCard.AddressNotAccepted,
        _ => RegisterCard.NotCompleted
    };

    /// <summary>G10's first sentence: "This invitation has expired: its link worked until 2026-11-02." (F7).</summary>
    public static string ExpiredMessage(DateOnly? lastDay)
        => lastDay is { } day
            ? $"This invitation has expired: its link worked until {Iso(day)}."
            : InvitationRefusals.Describe(InvitationRefusal.Expired);

    /// <summary>The form's note: "Not expecting this invitation? Ignore it: its link works until 2026-11-02." (F7).</summary>
    public static string Note(DateOnly lastDay) => $"Not expecting this invitation? Ignore it: its link works until {Iso(lastDay)}.";

    /// <summary>", as Coordinator at Kgosi Kgari Teaching Hospital.": what follows the address in the form's notice (Q13).</summary>
    public static string AsRoleAndScope(InvitationPreviewDto preview)
    {
        ArgumentNullException.ThrowIfNull(preview);
        return $", as {InvitationScopeWords.RoleAndScope(preview.TargetRole, preview.InstitutionName, preview.CollegeName, preview.SpecialityName, preview.SubSpecialityName)}.";
    }

    private static string Iso(DateOnly day) => day.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
}
