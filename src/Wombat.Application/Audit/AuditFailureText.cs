using FluentValidation;

namespace Wombat.Application.Audit;

/// <summary>
/// What a failed command's audit row stores (T400, flow 18, K10; P1, decided 2026-11-11): a refusal's own words, and for any
/// other exception the fixed <see cref="ActionFailed" />, whose own text goes to the application log instead.
/// </summary>
/// <remarks>
/// <para>
/// The audit log shows a failed row's text under its Failed badge, to an institution's administrator too, so it must be a
/// sentence written for a reader. Until T400 the pipeline stored every exception's message: a refusal's sentence, but also
/// EF Core's and LINQ's ("Sequence contains no elements."), written for a developer.
/// </para>
/// <para>
/// A refusal is:
/// <list type="bullet">
/// <item>a FluentValidation <see cref="ValidationException" />, stored as its validators' messages, each once, in order
/// (as <c>RefusalText.Of</c> reduces them on a page), never its "Validation failed: -- Property: …" log form;</item>
/// <item>an <see cref="UnauthorizedAccessException" /> <b>thrown by Wombat's own code</b>, whose message a handler writes
/// for the caller (the operator's F1, 2026-11-11): the framework throws the same type for file access, with a path in its
/// text ("Access to the path '…' is denied."), and that is not a refusal;</item>
/// <item>an <see cref="InvalidOperationException" />, typed or plain, <b>thrown by Wombat's own code</b>: its throw site's
/// declaring type in an assembly whose name starts "Wombat.". About twenty typed refusals derive from it and about seventy
/// files throw it plain with a sentence for the reader; the framework throws it too, with text for a developer, and that
/// is not a refusal.</item>
/// </list>
/// Rows already stored are not rewritten.
/// </para>
/// </remarks>
public static class AuditFailureText
{
    /// <summary>A failed row's text when its exception is no refusal (K10).</summary>
    public const string ActionFailed = "The action failed.";

    private const string WombatAssemblyPrefix = "Wombat.";

    /// <summary>The text the row stores for <paramref name="exception" />: the refusal's words, or <see cref="ActionFailed" />.</summary>
    public static string Of(Exception exception) => RefusalOf(exception) ?? ActionFailed;

    /// <summary>The refusal's words, or null when <paramref name="exception" /> is no refusal.</summary>
    public static string? RefusalOf(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        switch (exception)
        {
            case ValidationException validation:
                var messages = validation.Errors
                    .Select(failure => failure.ErrorMessage?.Trim())
                    .Where(message => !string.IsNullOrEmpty(message))
                    .Distinct(StringComparer.Ordinal)
                    .ToArray();
                return messages.Length > 0 ? string.Join(" ", messages) : null;

            case UnauthorizedAccessException unauthorized when ThrownByWombat(unauthorized):
                return Words(unauthorized.Message);

            case InvalidOperationException invalid when ThrownByWombat(invalid):
                return Words(invalid.Message);

            default:
                return null;
        }
    }

    /// <summary>
    /// Whether the exception was thrown by Wombat's own code: the method it was thrown from (the stack trace's first frame,
    /// which a rethrow through an <c>await</c> keeps) is declared in a Wombat assembly. An async method's frame is its state
    /// machine's, a nested type of the same assembly. An exception never thrown has no throw site, and is no refusal.
    /// </summary>
    private static bool ThrownByWombat(Exception exception)
    {
        var assembly = exception.TargetSite?.DeclaringType?.Assembly.GetName().Name;
        return assembly is not null && assembly.StartsWith(WombatAssemblyPrefix, StringComparison.Ordinal);
    }

    private static string? Words(string? message) => string.IsNullOrWhiteSpace(message) ? null : message.Trim();
}
