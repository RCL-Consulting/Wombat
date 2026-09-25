using Microsoft.AspNetCore.Identity;

namespace Wombat.Web.Security;

/// <summary>
/// Identity's sentence for each password rule a new password broke, from its error code and the rules this app sets: what
/// the change-password and register pages show for a refused password (T265, T285).
/// </summary>
public static class PasswordRuleMessages
{
    /// <summary>The sentence for <paramref name="code" />, when it is a password rule's; null for any other code.</summary>
    public static string? Describe(string? code, IdentityErrorDescriber describer, PasswordOptions rules) => code switch
    {
        nameof(IdentityErrorDescriber.PasswordTooShort) => describer.PasswordTooShort(rules.RequiredLength).Description,
        nameof(IdentityErrorDescriber.PasswordRequiresUniqueChars) =>
            describer.PasswordRequiresUniqueChars(rules.RequiredUniqueChars).Description,
        nameof(IdentityErrorDescriber.PasswordRequiresNonAlphanumeric) => describer.PasswordRequiresNonAlphanumeric().Description,
        nameof(IdentityErrorDescriber.PasswordRequiresDigit) => describer.PasswordRequiresDigit().Description,
        nameof(IdentityErrorDescriber.PasswordRequiresLower) => describer.PasswordRequiresLower().Description,
        nameof(IdentityErrorDescriber.PasswordRequiresUpper) => describer.PasswordRequiresUpper().Description,
        _ => null
    };
}
