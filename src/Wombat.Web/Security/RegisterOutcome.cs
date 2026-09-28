using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Wombat.Application.Features.Invitations;

namespace Wombat.Web.Security;

/// <summary>
/// The register page's address with a refusal in it, the codes a refusal travels as, and the words the page shows for
/// each (T285).
/// </summary>
/// <remarks>
/// <para>
/// The register form posts to <see cref="SubmitPath" />, which redirects back to the page when it refuses. Until T285 the
/// redirect carried the sentence, which the page printed as it arrived, and the endpoint's catch put the message of
/// whatever it caught into the address: a crafted link could put any words on the page, and a database fault's own text
/// could reach it. Now the address carries codes (<see cref="CodesFor" />), the page chooses the sentences
/// (<see cref="Describe" />), and an exception that is not a refusal is logged, never put in an address.
/// </para>
/// <para>
/// A refusal of the invitation travels as its <see cref="InvitationRefusal" />; a password Identity refused, as Identity's
/// own codes, which the page describes from the rules this app sets, as the change-password page does
/// (<see cref="ChangePasswordOutcome" />).
/// </para>
/// </remarks>
public static class RegisterOutcome
{
    /// <summary>The register page.</summary>
    public const string PagePath = "/account/register";

    /// <summary>The endpoint the page's form posts to.</summary>
    public const string SubmitPath = "/account/register/submit";

    /// <summary>The endpoint's log category: a fault is logged, and the page says registration could not be completed.</summary>
    public const string LogCategory = "Wombat.Web.Account.Register";

    /// <summary>The form was posted without an invitation token.</summary>
    public const string TokenMissing = "TokenMissing";

    /// <summary>The password and its confirmation differ.</summary>
    public const string ConfirmationMismatch = "ConfirmationMismatch";

    /// <summary>A name or the password was left blank, or a name is longer than the command allows.</summary>
    public const string DetailsInvalid = "DetailsInvalid";

    /// <summary>The account was created, but could not be read back to sign it in.</summary>
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

    /// <summary>A refusal that is not the person's to put right, or a code the page does not know.</summary>
    public const string Failed = "Failed";

    /// <summary>What the page says for <see cref="TokenMissing" />, and when it is opened without a token.</summary>
    public const string TokenMissingMessage = "The invitation token is missing.";

    /// <summary>What the page says for <see cref="ConfirmationMismatch" />.</summary>
    public const string ConfirmationMismatchMessage = "The password confirmation does not match.";

    /// <summary>What the page says for <see cref="DetailsInvalid" />.</summary>
    public const string DetailsInvalidMessage =
        "Enter your first name and last name, each of at most 100 characters, and a password.";

    /// <summary>What the page says for <see cref="UserNotLoaded" />.</summary>
    public const string UserNotLoadedMessage = "The invited user could not be loaded after registration.";

    /// <summary>What the page says for <see cref="Failed" /> and for any code it does not know.</summary>
    public const string GeneralRefusal = "Registration could not be completed. Try again.";

    /// <summary>
    /// The page, for the invitation <paramref name="token" /> names, refusing with <paramref name="codes" /> (each once;
    /// <see cref="Failed" /> when there are none). With no token, the page says only that it is missing.
    /// </summary>
    public static string Url(string? token, IEnumerable<string> codes)
    {
        var distinct = codes.Where(code => !string.IsNullOrWhiteSpace(code)).Distinct(StringComparer.Ordinal).ToList();
        if (distinct.Count == 0)
        {
            distinct.Add(Failed);
        }

        var query = distinct.Select(code => $"error={Uri.EscapeDataString(code)}");
        if (!string.IsNullOrWhiteSpace(token))
        {
            query = query.Prepend($"token={Uri.EscapeDataString(token)}");
        }

        return $"{PagePath}?{string.Join("&", query)}";
    }

    /// <summary>
    /// The codes a failed registration or preview travels as: a refusal's own, a validator's <see cref="DetailsInvalid" />,
    /// and <see cref="Failed" /> for anything else, which the caller logs.
    /// </summary>
    public static IReadOnlyList<string> CodesFor(Exception exception) => exception switch
    {
        InvitationRefusedException { Reason: InvitationRefusal.AccountNotCreated } refused =>
            refused.ErrorCodes.Count > 0 ? refused.ErrorCodes : [Failed],
        InvitationRefusedException refused => [CodeOf(refused.Reason)],
        ValidationException => [DetailsInvalid],
        _ => [Failed]
    };

    /// <summary>Whether <paramref name="exception" /> is a refusal the page describes, rather than a fault to log.</summary>
    public static bool IsRefusal(Exception exception) => exception is InvitationRefusedException or ValidationException;

    /// <summary>
    /// The sentences the page shows for the codes it was sent back with, each once: its own for each code above, in the
    /// order sent, and <see cref="GeneralRefusal" /> for any code this list does not name; then, when a password rule was
    /// broken, the rules' heading and each rule broken, in the one order the rules are always listed in
    /// (<see cref="PasswordRuleMessages" />, T339, flow 02, E11), whatever order Identity sent them in.
    /// </summary>
    public static IReadOnlyList<string> Describe(
        IEnumerable<string>? codes,
        IdentityErrorDescriber describer,
        PasswordOptions rules)
        => Refusal(codes, describer, rules).Flat;

    /// <summary>What <see cref="Describe" /> says, with the rules broken apart from the other reasons, for a page to list.</summary>
    public static PasswordRefusal Refusal(
        IEnumerable<string>? codes,
        IdentityErrorDescriber describer,
        PasswordOptions rules)
    {
        ArgumentNullException.ThrowIfNull(describer);
        ArgumentNullException.ThrowIfNull(rules);

        var sent = (codes ?? []).Where(code => !string.IsNullOrWhiteSpace(code)).ToList();
        var sentences = new List<string>();
        foreach (var code in sent.Where(code => !PasswordRuleMessages.IsRule(code)))
        {
            var sentence = code switch
            {
                TokenMissing => TokenMissingMessage,
                ConfirmationMismatch => ConfirmationMismatchMessage,
                DetailsInvalid => DetailsInvalidMessage,
                UserNotLoaded => UserNotLoadedMessage,
                InvitationInvalid => InvitationRefusals.Describe(InvitationRefusal.Invalid),
                InvitationRevoked => InvitationRefusals.Describe(InvitationRefusal.Revoked),
                InvitationUsed => InvitationRefusals.Describe(InvitationRefusal.Used),
                InvitationExpired => InvitationRefusals.Describe(InvitationRefusal.Expired),
                AccountExists => InvitationRefusals.Describe(InvitationRefusal.AccountExists),
                AddressNotAccepted => InvitationRefusals.Describe(InvitationRefusal.AddressNotAccepted),
                _ => GeneralRefusal
            };

            if (!sentences.Contains(sentence, StringComparer.Ordinal))
            {
                sentences.Add(sentence);
            }
        }

        return new PasswordRefusal(null, sentences, PasswordRuleMessages.Broken(sent, rules));
    }

    private static string CodeOf(InvitationRefusal reason) => reason switch
    {
        InvitationRefusal.Invalid => InvitationInvalid,
        InvitationRefusal.Revoked => InvitationRevoked,
        InvitationRefusal.Used => InvitationUsed,
        InvitationRefusal.Expired => InvitationExpired,
        InvitationRefusal.AccountExists => AccountExists,
        InvitationRefusal.AddressNotAccepted => AddressNotAccepted,
        _ => Failed
    };
}
