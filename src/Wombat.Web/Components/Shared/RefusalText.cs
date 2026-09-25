using FluentValidation;

namespace Wombat.Web.Components.Shared;

/// <summary>
/// What a page shows when a request is refused: the refusal in its own words (T213).
/// </summary>
/// <remarks>
/// <para>
/// A command's validator refuses through the validation pipeline with a FluentValidation <see cref="ValidationException" />,
/// whose <see cref="Exception.Message" /> is written for a log: "Validation failed: -- PresentUserIds: A committee decision
/// needs … Severity: Error". The committee review page printed that to a chair whose attendance fell short, and the panel
/// page to an administrator whose panel had one member. The page shows each failure's message instead, once, in the
/// order the validator gave them. The exception itself is unchanged: the audit row still records the full message,
/// property names and all.
/// </para>
/// <para>
/// Every other refusal a handler throws is already a sentence written for the person who clicked, so its message is
/// shown as it is.
/// </para>
/// </remarks>
public static class RefusalText
{
    public static string Of(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        if (exception is ValidationException validation)
        {
            var messages = validation.Errors
                .Select(failure => failure.ErrorMessage?.Trim())
                .Where(message => !string.IsNullOrEmpty(message))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            if (messages.Length > 0)
            {
                return string.Join(" ", messages);
            }
        }

        return exception.Message;
    }
}
