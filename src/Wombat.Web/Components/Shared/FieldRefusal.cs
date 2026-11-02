using FluentValidation;
using Wombat.Application.Common;

namespace Wombat.Web.Components.Shared;

/// <summary>
/// A refusal a page shows under one of its fields (T392, flow 15, F4, S6): the field it is about, by the command's property
/// name, and the sentence. Read from a <see cref="FieldRefusalException" /> (a command's own refusal) or from a validator's
/// <see cref="ValidationException" /> whose failures are all about one property; any other exception is no field's, and the
/// page shows its own fixed failure sentence in its alert (T329), never the exception's text.
/// </summary>
/// <remarks>
/// <para>
/// The EPA's page, the item's page and the trainee profile share it (build-contracts § 0). Each page maps
/// <see cref="Field" /> to its own control: the key is the command's property (<c>"SubSpecialityId"</c>, <c>"Code"</c>,
/// <c>"EpaId"</c>, <c>"CurriculumId"</c>), and a page whose control is named otherwise says which in its own code. The
/// refusal renders as <c>div.validation-message role="alert"</c> with <see cref="Id" />, the control <c>aria-invalid</c>, the
/// focus in it, and <see cref="DescribedBy" /> putting the refusal first, then the help (flow 14's N17).
/// </para>
/// </remarks>
public sealed record FieldRefusal(string Field, string Message)
{
    /// <summary>
    /// The field refusal <paramref name="exception" /> carries, or null when it is no field's: a
    /// <see cref="FieldRefusalException" />, or a <see cref="ValidationException" /> whose failures name one property (their
    /// messages joined, once each, in the validator's order).
    /// </summary>
    public static FieldRefusal? Of(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        if (exception is FieldRefusalException refused)
        {
            return new FieldRefusal(refused.Field, refused.Message);
        }

        if (exception is ValidationException validation)
        {
            var failures = validation.Errors
                .Where(failure => !string.IsNullOrWhiteSpace(failure.ErrorMessage))
                .ToList();
            var properties = failures.Select(failure => failure.PropertyName).Distinct(StringComparer.Ordinal).ToList();
            if (properties.Count == 1 && !string.IsNullOrWhiteSpace(properties[0]))
            {
                var message = string.Join(' ', failures.Select(failure => failure.ErrorMessage.Trim()).Distinct(StringComparer.Ordinal));
                return new FieldRefusal(properties[0], message);
            }
        }

        return null;
    }

    /// <summary>Whether this refusal is about <paramref name="field" /> (the command's property name).</summary>
    public bool Is(string field) => string.Equals(Field, field, StringComparison.Ordinal);

    /// <summary>The id of the refusal rendered under the control whose id is <paramref name="inputId" />.</summary>
    public static string Id(string inputId) => $"{FieldHelp.IdPart(inputId)}-refusal";

    /// <summary>
    /// The control's <c>aria-describedby</c>: the refusal first when <paramref name="refused" />, then the help text's id when
    /// there is help, then any other region (a counter); null when nothing describes it.
    /// </summary>
    public static string? DescribedBy(string inputId, bool refused, string? helpText, params string?[] regionIds)
    {
        var ids = new List<string>();
        if (refused)
        {
            ids.Add(Id(inputId));
        }

        if (FieldHelp.DescribedBy(inputId, helpText, regionIds) is { } rest)
        {
            ids.AddRange(rest.Split(' ').Where(id => !ids.Contains(id, StringComparer.Ordinal)));
        }

        return ids.Count == 0 ? null : string.Join(' ', ids);
    }
}
