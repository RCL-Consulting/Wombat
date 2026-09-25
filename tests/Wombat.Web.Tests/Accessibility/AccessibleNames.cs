using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;

namespace Wombat.Web.Tests.Accessibility;

/// <summary>
/// Works out the accessible name of each control on a rendered page (T176). A control with no name renders and
/// works, but a screen reader announces it as "combo box" or "spin button" and nothing else. The edit row on the
/// curriculum items page had three like that. Nothing fails at build or run time, so only a check over the rendered
/// markup catches it.
/// </summary>
/// <remarks>
/// This follows the parts of the W3C accessible name computation that Wombat's markup uses, in its order:
/// <c>aria-labelledby</c>, then <c>aria-label</c>, then the control's <c>&lt;label for&gt;</c> or wrapping
/// <c>&lt;label&gt;</c>, then, for a button or a link, its own text. Text inside <c>aria-hidden="true"</c> does not count.
/// A <c>title</c> or a <c>placeholder</c> does not count either: neither is a label a sighted user can rely on.
/// Pair it with <see cref="IdReferences" />, which finds a label that points at no control.
/// </remarks>
internal static partial class AccessibleNames
{
    /// <summary>The controls a user fills in or presses: every input except a hidden one, and selects, textareas and buttons.</summary>
    private const string ControlSelector = "input:not([type=hidden]), select, textarea, button";

    /// <summary>Every control on the page, each with its accessible name (empty when it has none).</summary>
    public static IReadOnlyList<(IElement Control, string Name)> Controls(IRenderedFragment cut)
        => cut.FindAll(ControlSelector)
            .Select(control => (control, NameOf(cut, control)))
            .ToList();

    /// <summary>One line per control that has no accessible name. Empty means every control is named.</summary>
    public static IReadOnlyList<string> Unnamed(IRenderedFragment cut)
        => Controls(cut)
            .Where(entry => entry.Name.Length == 0)
            .Select(entry => Describe(entry.Control))
            .ToList();

    /// <summary>The accessible name of <paramref name="control" />, or an empty string when it has none.</summary>
    public static string NameOf(IRenderedFragment cut, IElement control)
    {
        if (control.GetAttribute("aria-labelledby") is { } labelledBy)
        {
            var name = Collapse(string.Join(' ', labelledBy
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
                .Select(id => cut.FindAll($"[id='{id}']").FirstOrDefault())
                .Where(element => element is not null)
                .Select(element => VisibleText(element!))));
            if (name.Length > 0)
            {
                return name;
            }
        }

        if (Collapse(control.GetAttribute("aria-label") ?? string.Empty) is { Length: > 0 } ariaLabel)
        {
            return ariaLabel;
        }

        var labels = new List<IElement>();
        if (!string.IsNullOrEmpty(control.Id))
        {
            labels.AddRange(cut.FindAll("label[for]").Where(label => label.GetAttribute("for") == control.Id));
        }

        if (control.Closest("label") is { } wrapping)
        {
            labels.Add(wrapping);
        }

        if (Collapse(string.Join(' ', labels.Select(VisibleText))) is { Length: > 0 } labelled)
        {
            return labelled;
        }

        // A button and a link are named by their own text when nothing else names them (T239: a row's "View" link).
        return control.LocalName is "button" or "a" ? Collapse(VisibleText(control)) : string.Empty;
    }

    /// <summary>The text a screen reader would read from an element: everything but what is marked aria-hidden.</summary>
    private static string VisibleText(IElement element)
        => string.Concat(element.ChildNodes.Select(node => node switch
        {
            IElement child when child.GetAttribute("aria-hidden") == "true" => string.Empty,
            IElement child => " " + VisibleText(child) + " ",
            _ => node.TextContent
        }));

    private static string Collapse(string text) => Whitespace().Replace(text, " ").Trim();

    private static string Describe(IElement control)
    {
        var type = control.GetAttribute("type") is { } value ? $" type=\"{value}\"" : string.Empty;
        var id = string.IsNullOrEmpty(control.Id) ? "no id" : $"id \"{control.Id}\"";
        return $"<{control.LocalName}{type}> ({id}) has no accessible name";
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
