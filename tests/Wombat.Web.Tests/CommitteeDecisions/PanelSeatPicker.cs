using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;

namespace Wombat.Web.Tests.CommitteeDecisions;

/// <summary>
/// Reads and sets the panel page's seat picker (T364, Q5): one radio group a committee member, named "Seat for {name}",
/// with the chair's row read-only, "Chair · chosen above".
/// </summary>
internal static partial class PanelSeatPicker
{
    /// <summary>The person's seat as the picker shows it: "none", "member", "external", or "chair" on the chair's row.</summary>
    public static string SeatOf(IRenderedFragment cut, string name)
    {
        var row = RowOf(cut, name);
        if (row.QuerySelector("fieldset") is not { } fieldset)
        {
            return Squash(row.QuerySelector("td")!.TextContent).StartsWith("Chair", StringComparison.Ordinal) ? "chair" : "?";
        }

        return fieldset.QuerySelectorAll("input[type=radio]")
            .Single(radio => radio.HasAttribute("checked"))
            .GetAttribute("value")!;
    }

    /// <summary>Chooses a seat for the person: "none", "member" or "external".</summary>
    public static void Choose(IRenderedFragment cut, string name, string seat)
        => RowOf(cut, name).QuerySelector($"input[type=radio][value={seat}]")!.Change(seat);

    /// <summary>Seats the person with this user id, found by their name in the Chair select's options.</summary>
    public static void SeatById(IRenderedFragment cut, string userId, string seat) => Choose(cut, NameOf(cut, userId), seat);

    /// <summary>The person's name, as the Chair select offers them ("Thandi Zulu (zulu@test)").</summary>
    public static string NameOf(IRenderedFragment cut, string userId)
    {
        var text = cut.Find("#panel-chair").QuerySelectorAll("option")
            .Single(option => option.GetAttribute("value") == userId).TextContent.Trim();
        return text[..text.LastIndexOf(" (", StringComparison.Ordinal)].Trim();
    }

    /// <summary>
    /// Chooses the new panel's scope by its radio; where the form offers one scope only, says it as text, and it is that
    /// scope, there is nothing to choose.
    /// </summary>
    public static void ChooseScope(IRenderedFragment cut, Wombat.Domain.CommitteeDecisions.DecisionPanelScope scope)
    {
        if (cut.FindAll("#panel-scope input[type=radio]").Count == 0 &&
            string.Equals(cut.Find("#panel-scope").TextContent.Trim(), scope.ToString(), StringComparison.Ordinal))
        {
            return;
        }

        cut.Find($"#panel-scope-{scope.ToString().ToLowerInvariant()}").Change(scope.ToString());
    }

    /// <summary>The scopes the new-panel form offers: its radios' values, or the one scope it says as text.</summary>
    public static IReadOnlyList<string> ScopesOffered(IRenderedFragment cut)
    {
        var radios = cut.FindAll("#panel-scope input[type=radio]");
        return radios.Count > 0
            ? radios.Select(radio => radio.GetAttribute("value")!).ToArray()
            : [cut.Find("#panel-scope").TextContent.Trim()];
    }

    /// <summary>The people the picker lists, by name, in its order.</summary>
    public static IReadOnlyList<string> Listed(IRenderedFragment cut)
        => cut.FindAll("#panel-seats tbody tr")
            .Select(row => row.QuerySelector(".person-cell")!.ChildNodes.First().TextContent.Trim())
            .ToArray();

    /// <summary>Every person the picker seats, as "name:seat", in its order.</summary>
    public static IReadOnlyList<string> Seats(IRenderedFragment cut)
        => Listed(cut).Select(name => $"{name}:{SeatOf(cut, name)}").ToArray();

    /// <summary>The values the Chair select offers, its "Choose a chair" left out.</summary>
    public static IReadOnlyList<string?> ChairOptions(IRenderedFragment cut)
        => cut.Find("#panel-chair").QuerySelectorAll("option")
            .Select(option => option.GetAttribute("value"))
            .Where(value => !string.IsNullOrEmpty(value))
            .ToArray();

    public static string Squash(string text) => Whitespace().Replace(text, " ").Trim();

    private static IElement RowOf(IRenderedFragment cut, string name)
        => cut.FindAll("#panel-seats tbody tr")
            .Single(row => string.Equals(row.QuerySelector(".person-cell")!.ChildNodes.First().TextContent.Trim(), name, StringComparison.Ordinal));

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
