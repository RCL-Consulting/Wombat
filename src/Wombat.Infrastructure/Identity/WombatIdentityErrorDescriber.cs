using Microsoft.AspNetCore.Identity;

namespace Wombat.Infrastructure.Identity;

/// <summary>
/// Identity's words for a password rule, written as Wombat writes them: one short sentence per rule, in one order, under
/// one heading, the same in the list shown before typing and in every refusal (T339, flow 02, C2 and E11).
/// </summary>
/// <remarks>
/// <para>
/// Until T339 a refused password read as Identity's own descriptions ("Passwords must have at least one uppercase
/// ('A'-'Z')."), in the order Identity's validator happened to find them, on the change-password and register pages and,
/// joined with "; ", on the administrator's reset card. The pages now list the rules up front and a refusal names only
/// the rules broken, so each rule has one sentence that reads both ways, under "The new password needs:".
/// </para>
/// <para>
/// Registered as Identity's describer (<c>AddErrorDescriber</c>), so any <see cref="IdentityError" /> for a rule carries
/// these words, whoever reads it: the administrator's reset reads them off the errors
/// (<c>UserAdministrationService.ResetPasswordAsync</c>). The pages in Wombat.Web read them from
/// <see cref="Describe" />, which needs no describer, so a page rendered with Identity's default describer still says them.
/// </para>
/// </remarks>
public sealed class WombatIdentityErrorDescriber : IdentityErrorDescriber
{
    /// <summary>What every list of the rules is headed with, before typing and in a refusal.</summary>
    public const string Heading = "The new password needs:";

    private const string Digit = "A digit (0 to 9).";

    private const string Upper = "An upper-case letter.";

    private const string Lower = "A lower-case letter.";

    private const string Symbol = "A symbol, such as ! or #.";

    /// <summary>The rules' error codes, in the one order every list of them follows.</summary>
    public static IReadOnlyList<string> RuleOrder { get; } =
    [
        nameof(PasswordTooShort),
        nameof(PasswordRequiresUniqueChars),
        nameof(PasswordRequiresDigit),
        nameof(PasswordRequiresUpper),
        nameof(PasswordRequiresLower),
        nameof(PasswordRequiresNonAlphanumeric)
    ];

    /// <summary>Whether <paramref name="code" /> is a password rule's.</summary>
    public static bool IsRule(string? code) => code is not null && RuleOrder.Contains(code, StringComparer.Ordinal);

    /// <summary>The sentence for the rule <paramref name="code" /> names, from the rules set; null for any other code.</summary>
    public static string? Describe(string? code, PasswordOptions rules)
    {
        ArgumentNullException.ThrowIfNull(rules);

        return code switch
        {
            nameof(PasswordTooShort) => TooShort(rules.RequiredLength),
            nameof(PasswordRequiresUniqueChars) => UniqueChars(rules.RequiredUniqueChars),
            nameof(PasswordRequiresDigit) => Digit,
            nameof(PasswordRequiresUpper) => Upper,
            nameof(PasswordRequiresLower) => Lower,
            nameof(PasswordRequiresNonAlphanumeric) => Symbol,
            _ => null
        };
    }

    /// <summary>Every rule the options set, in <see cref="RuleOrder" />: the list a page shows before anything is typed.</summary>
    public static IReadOnlyList<string> All(PasswordOptions rules)
    {
        ArgumentNullException.ThrowIfNull(rules);

        var codes = new List<string>();
        if (rules.RequiredLength > 0)
        {
            codes.Add(nameof(PasswordTooShort));
        }

        if (rules.RequiredUniqueChars > 1)
        {
            codes.Add(nameof(PasswordRequiresUniqueChars));
        }

        if (rules.RequireDigit)
        {
            codes.Add(nameof(PasswordRequiresDigit));
        }

        if (rules.RequireUppercase)
        {
            codes.Add(nameof(PasswordRequiresUpper));
        }

        if (rules.RequireLowercase)
        {
            codes.Add(nameof(PasswordRequiresLower));
        }

        if (rules.RequireNonAlphanumeric)
        {
            codes.Add(nameof(PasswordRequiresNonAlphanumeric));
        }

        return codes.Select(code => Describe(code, rules)!).ToList();
    }

    /// <summary>
    /// The rules among <paramref name="codes" />, each once, in <see cref="RuleOrder" /> whatever order they came in: what a
    /// refusal lists under <see cref="Heading" />. Codes that are not a rule's are left out.
    /// </summary>
    public static IReadOnlyList<string> Broken(IEnumerable<string>? codes, PasswordOptions rules)
    {
        var sent = (codes ?? []).ToHashSet(StringComparer.Ordinal);
        return RuleOrder.Where(sent.Contains).Select(code => Describe(code, rules)!).ToList();
    }

    public override IdentityError PasswordTooShort(int length)
        => new() { Code = nameof(PasswordTooShort), Description = TooShort(length) };

    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars)
        => new() { Code = nameof(PasswordRequiresUniqueChars), Description = UniqueChars(uniqueChars) };

    public override IdentityError PasswordRequiresDigit()
        => new() { Code = nameof(PasswordRequiresDigit), Description = Digit };

    public override IdentityError PasswordRequiresUpper()
        => new() { Code = nameof(PasswordRequiresUpper), Description = Upper };

    public override IdentityError PasswordRequiresLower()
        => new() { Code = nameof(PasswordRequiresLower), Description = Lower };

    public override IdentityError PasswordRequiresNonAlphanumeric()
        => new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = Symbol };

    private static string TooShort(int length) => $"At least {length} characters.";

    private static string UniqueChars(int uniqueChars) => $"At least {uniqueChars} different characters.";
}
