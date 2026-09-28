using Microsoft.AspNetCore.Identity;
using Wombat.Infrastructure.Identity;

namespace Wombat.Web.Security;

/// <summary>
/// The password rules as the pages show them: one heading, one order and one sentence per rule, in the list shown before
/// typing and in every refusal, on change password, register and the administrator's reset card (T265, T285; T339, flow
/// 02, C2 and E11).
/// </summary>
/// <remarks>
/// The sentences are <see cref="WombatIdentityErrorDescriber" />'s, which Identity's errors carry too, so the reset card,
/// which reads the errors, says the same. Until T339 the pages showed Identity's own descriptions ("Passwords must have at
/// least one uppercase ('A'-'Z').") in the order its validator sent the codes, and listed no rule until one was broken.
/// </remarks>
public static class PasswordRuleMessages
{
    /// <summary>What heads every list of the rules: "The new password needs:".</summary>
    public const string Heading = WombatIdentityErrorDescriber.Heading;

    /// <summary>What the new-password field says when a rule was broken; the refusal names which.</summary>
    public const string FieldMessage = "The new password does not meet the rules below.";

    /// <summary>The sentence for <paramref name="code" />, when it is a password rule's; null for any other code.</summary>
    public static string? Describe(string? code, PasswordOptions rules) => WombatIdentityErrorDescriber.Describe(code, rules);

    /// <summary>Whether <paramref name="code" /> is a password rule's.</summary>
    public static bool IsRule(string? code) => WombatIdentityErrorDescriber.IsRule(code);

    /// <summary>
    /// Every rule <paramref name="rules" /> sets, one sentence each, in the one order: the list a page shows under
    /// <see cref="Heading" /> before anything is typed. With the rules Wombat sets, all six.
    /// </summary>
    public static IReadOnlyList<string> All(PasswordOptions rules) => WombatIdentityErrorDescriber.All(rules);

    /// <summary>
    /// The rules among <paramref name="codes" />, each once, in the one order <see cref="All" /> lists them, whatever order
    /// Identity sent them in: what a refusal lists under <see cref="Heading" />. Codes that are not a rule's are left out.
    /// </summary>
    public static IReadOnlyList<string> Broken(IEnumerable<string>? codes, PasswordOptions rules)
        => WombatIdentityErrorDescriber.Broken(codes, rules);
}
