using AngleSharp.Dom;
using Bunit;

namespace Wombat.Web.Tests.Accessibility;

/// <summary>
/// Finds the id references on a rendered page that name nothing a browser can use (T147). A
/// <c>&lt;label for&gt;</c> naming a missing id renders, looks right, and does nothing: clicking it focuses
/// nothing, and a screen reader announces the control it was meant for with no name. Nothing fails at
/// build or run time, so only a check over the rendered markup catches it.
/// </summary>
internal static class IdReferences
{
    /// <summary>HTML's labelable elements. A label <c>for</c> any other element labels nothing.</summary>
    private static readonly HashSet<string> Labelable =
        new(["button", "input", "meter", "output", "progress", "select", "textarea"], StringComparer.OrdinalIgnoreCase);

    private static readonly string[] AriaIdListAttributes = ["aria-labelledby", "aria-describedby"];

    /// <summary>
    /// Every broken reference on the page, one line each: a <c>&lt;label for&gt;</c> naming no element, or
    /// naming one that cannot be labelled (a <c>div</c>, a hidden input); an <c>aria-labelledby</c> or
    /// <c>aria-describedby</c> token naming no element; and an id used twice, which makes any reference to it
    /// ambiguous. Empty means every reference resolves.
    /// </summary>
    public static IReadOnlyList<string> Broken(IRenderedFragment cut)
    {
        var problems = new List<string>();

        var elementsById = cut.FindAll("[id]")
            .GroupBy(element => element.Id!, StringComparer.Ordinal)
            .ToList();

        problems.AddRange(elementsById
            .Where(group => group.Count() > 1)
            .Select(group => $"id \"{group.Key}\" is used {group.Count()} times"));

        var byId = elementsById.ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

        foreach (var label in cut.FindAll("label[for]"))
        {
            var target = label.GetAttribute("for")!;
            var name = label.TextContent.Trim();

            if (!byId.TryGetValue(target, out var control))
            {
                problems.Add($"<label for=\"{target}\"> (\"{name}\") names no element");
            }
            else if (!IsLabelable(control))
            {
                problems.Add($"<label for=\"{target}\"> (\"{name}\") names a <{control.LocalName}>, which cannot be labelled");
            }
        }

        foreach (var attribute in AriaIdListAttributes)
        {
            foreach (var element in cut.FindAll($"[{attribute}]"))
            {
                foreach (var target in element.GetAttribute(attribute)!.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!byId.ContainsKey(target))
                    {
                        problems.Add($"<{element.LocalName} {attribute}=\"{target}\"> names no element");
                    }
                }
            }
        }

        return problems;
    }

    private static bool IsLabelable(IElement element)
        => Labelable.Contains(element.LocalName)
           && !(element.LocalName == "input"
                && string.Equals(element.GetAttribute("type"), "hidden", StringComparison.OrdinalIgnoreCase));
}
