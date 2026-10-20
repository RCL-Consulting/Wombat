using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Wombat.Application.Features.Invitations;

namespace Wombat.Web.Security;

/// <summary>
/// The register page's paths, the codes a refused registration travels as, and the words the page shows for each (T285;
/// T373, flow 11).
/// </summary>
/// <remarks>
/// <para>
/// The invitation mail's link opens <see cref="StartPath" />, which keeps the token in the register cookie
/// (<see cref="RegisterCookie" />) and answers 303 to <see cref="PagePath" /> with no query (S2). The page's form posts to
/// <see cref="SubmitPath" />, which reads the token from the cookie, and on a refusal writes the codes and the two names
/// into it and redirects to the page, again with no query (S1). Until T373 the token and the codes travelled in the
/// address (<c>?token=…&amp;error=…</c>), and the names were dropped.
/// </para>
/// <para>
/// Until T285 the redirect carried the sentence, which the page printed as it arrived, and the endpoint's catch put the
/// message of whatever it caught into the address. A refusal still travels as codes (<see cref="CodesFor" />), the page
/// still chooses the sentences, and an exception that is not a refusal is logged and read as <see cref="Failed" />.
/// </para>
/// </remarks>
public static class RegisterOutcome
{
    /// <summary>The register page. Also the register cookie's path, and the only <c>returnTo</c> sign-out accepts (F5).</summary>
    public const string PagePath = "/account/register";

    /// <summary>
    /// What the invitation mail's link opens (S2): it keeps the token in the register cookie and answers 303 to the page.
    /// <c>InvitationLinks.RegistrationUrl</c> writes this path; a test holds them equal.
    /// </summary>
    public const string StartPath = "/account/register/start";

    /// <summary>The endpoint the page's form posts to.</summary>
    public const string SubmitPath = "/account/register/submit";

    /// <summary>The endpoint's log category: a fault is logged, and the page says registration could not be completed.</summary>
    public const string LogCategory = "Wombat.Web.Account.Register";

    /// <summary>The password and its confirmation differ.</summary>
    public const string ConfirmationMismatch = "ConfirmationMismatch";

    /// <summary>The first name was left blank (the browser's <c>required</c> is passed by a name of spaces).</summary>
    public const string FirstNameMissing = "FirstNameMissing";

    /// <summary>The first name is longer than the command allows (S21: no <c>maxlength</c>, so the validator says it).</summary>
    public const string FirstNameTooLong = "FirstNameTooLong";

    /// <summary>The last name was left blank.</summary>
    public const string LastNameMissing = "LastNameMissing";

    /// <summary>The last name is longer than the command allows (G06).</summary>
    public const string LastNameTooLong = "LastNameTooLong";

    /// <summary>The password was left blank.</summary>
    public const string PasswordMissing = "PasswordMissing";

    /// <summary>The account was created, but could not be read back to sign it in (G15).</summary>
    public const string UserNotLoaded = "UserNotLoaded";

    /// <summary>No invitation matches the token (<see cref="InvitationRefusal.Invalid" />).</summary>
    public const string InvitationInvalid = "InvitationInvalid";

    /// <summary>The invitation was revoked (<see cref="InvitationRefusal.Revoked" />).</summary>
    public const string InvitationRevoked = "InvitationRevoked";

    /// <summary>The invitation has been accepted already (<see cref="InvitationRefusal.Used" />).</summary>
    public const string InvitationUsed = "InvitationUsed";

    /// <summary>The invitation has expired (<see cref="InvitationRefusal.Expired" />).</summary>
    public const string InvitationExpired = "InvitationExpired";

    /// <summary>An account already holds the invited address (<see cref="InvitationRefusal.AccountExists" />).</summary>
    public const string AccountExists = "AccountExists";

    /// <summary>Identity will not take the invited address as a user name (<see cref="InvitationRefusal.AddressNotAccepted" />).</summary>
    public const string AddressNotAccepted = "AddressNotAccepted";

    /// <summary>
    /// The form posted was shown for another invitation than the one the cookie holds now: this browser opened a second
    /// link since (T373 fix pass, B1). Nothing is accepted.
    /// </summary>
    public const string OtherInvitation = "OtherInvitation";

    /// <summary>A fault, a refusal that is not the person's to put right, or a code the page does not know (G14).</summary>
    public const string Failed = "Failed";

    /// <summary>G13's first sentence: a GET of the page with neither the cookie nor a token (S7).</summary>
    public const string TokenMissingMessage = "The invitation token is missing.";

    /// <summary>What the password confirmation's field says (G05), before its next step.</summary>
    public const string ConfirmationMismatchMessage = "The password confirmation does not match.";

    /// <summary>G05's next step: both password fields are emptied.</summary>
    public const string ConfirmationMismatchNextStep = "Type the password again in both fields.";

    /// <summary>G15's first sentence.</summary>
    public const string UserNotLoadedMessage = "The invited user could not be loaded after registration.";

    /// <summary>
    /// G14's first sentence. Until T373 it was "Registration could not be completed. Try again." (<c>GeneralRefusal</c>):
    /// the Try again button says the rest, and F10's one transaction makes "Nothing was saved." true.
    /// </summary>
    public const string NotCompletedMessage = "Registration could not be completed. Nothing was saved.";

    /// <summary>What Password says for <see cref="PasswordMissing" />.</summary>
    public const string PasswordMissingMessage = "Enter a password.";

    /// <summary>The validator's longest name, which the command allows (<c>AcceptInvitationCommandValidator</c>).</summary>
    public const int NameMaximumLength = 100;

    /// <summary>
    /// The codes a failed registration travels as: a refusal's own; a validator's, one for each field it refused; and
    /// <see cref="Failed" /> for anything else, which the caller logs.
    /// </summary>
    public static IReadOnlyList<string> CodesFor(Exception exception) => exception switch
    {
        InvitationRefusedException { Reason: InvitationRefusal.AccountNotCreated } refused =>
            refused.ErrorCodes.Count > 0 ? refused.ErrorCodes : [Failed],
        InvitationRefusedException refused => [CodeOf(refused.Reason)],
        ValidationException validation => ValidationCodes(validation),
        _ => [Failed]
    };

    /// <summary>Whether <paramref name="exception" /> is a refusal the page describes, rather than a fault to log.</summary>
    public static bool IsRefusal(Exception exception) => exception is InvitationRefusedException or ValidationException;

    /// <summary>The code for <paramref name="reason" />; <see cref="Failed" /> for Identity's own refusal, which has codes of its own.</summary>
    public static string CodeOf(InvitationRefusal reason) => reason switch
    {
        InvitationRefusal.Invalid => InvitationInvalid,
        InvitationRefusal.Revoked => InvitationRevoked,
        InvitationRefusal.Used => InvitationUsed,
        InvitationRefusal.Expired => InvitationExpired,
        InvitationRefusal.AccountExists => AccountExists,
        InvitationRefusal.AddressNotAccepted => AddressNotAccepted,
        _ => Failed
    };

    /// <summary>The refusal of the invitation <paramref name="code" /> stands for; null when it is no invitation's.</summary>
    public static InvitationRefusal? InvitationRefusalOf(string code) => code switch
    {
        InvitationInvalid => InvitationRefusal.Invalid,
        InvitationRevoked => InvitationRefusal.Revoked,
        InvitationUsed => InvitationRefusal.Used,
        InvitationExpired => InvitationRefusal.Expired,
        AccountExists => InvitationRefusal.AccountExists,
        AddressNotAccepted => InvitationRefusal.AddressNotAccepted,
        _ => null
    };

    /// <summary>
    /// Whether <paramref name="code" /> is one the form puts beside a field (S15): a name's, the password's, a broken
    /// password rule, or the confirmation's. Any other code is a card.
    /// </summary>
    public static bool IsFieldCode(string code)
        => code is ConfirmationMismatch or FirstNameMissing or FirstNameTooLong or LastNameMissing or LastNameTooLong
               or PasswordMissing
           || PasswordRuleMessages.IsRule(code);

    /// <summary>What a name field says for its code: "Last name: enter 100 characters or fewer." (G06), "Enter your last name.".</summary>
    public static string? NameMessage(string label, IEnumerable<string> codes, string missingCode, string tooLongCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        var sent = codes.ToList();
        if (sent.Contains(tooLongCode, StringComparer.Ordinal))
        {
            return $"{label}: enter {NameMaximumLength} characters or fewer.";
        }

        return sent.Contains(missingCode, StringComparer.Ordinal) ? $"Enter your {label.ToLowerInvariant()}." : null;
    }

    /// <summary>
    /// What Password says for the rules its refusal broke (G04): the rules' heading and each rule broken, in the one order
    /// they are always listed in, "The new password needs: At least 12 characters." Null when no rule was broken.
    /// </summary>
    public static string? PasswordRulesMessage(IEnumerable<string> codes, PasswordOptions rules)
    {
        ArgumentNullException.ThrowIfNull(rules);
        var broken = PasswordRuleMessages.Broken(codes, rules);
        return broken.Count == 0 ? null : $"{PasswordRuleMessages.Heading} {string.Join(" ", broken)}";
    }

    /// <summary>What Confirm password says when the two differed (G05).</summary>
    public static string ConfirmationFieldMessage => $"{ConfirmationMismatchMessage} {ConfirmationMismatchNextStep}";

    /// <summary>A validator's refusal, one code a field and reason, in the form's order; <see cref="Failed" /> for anything else.</summary>
    private static IReadOnlyList<string> ValidationCodes(ValidationException validation)
    {
        var codes = new List<string>();
        foreach (var failure in validation.Errors)
        {
            // FluentValidation's code for MaximumLength, the one length rule the command has.
            var tooLong = string.Equals(failure.ErrorCode, "MaximumLengthValidator", StringComparison.Ordinal);
            var code = failure.PropertyName switch
            {
                nameof(AcceptInvitationCommand.FirstName) => tooLong ? FirstNameTooLong : FirstNameMissing,
                nameof(AcceptInvitationCommand.LastName) => tooLong ? LastNameTooLong : LastNameMissing,
                nameof(AcceptInvitationCommand.Password) => PasswordMissing,
                _ => Failed
            };

            if (!codes.Contains(code, StringComparer.Ordinal))
            {
                codes.Add(code);
            }
        }

        return codes.Count == 0 ? [Failed] : codes;
    }
}
