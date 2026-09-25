using System.Text.RegularExpressions;
using FluentAssertions;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// An option of a <c>&lt;select multiple&gt;</c> rendered from data is keyed by what it offers (<c>@key</c>), so the
/// selection a browser shows is the one the form holds. (T257; DESIGN.md § Form system)
/// </summary>
/// <remarks>
/// Unkeyed, Blazor reuses option elements by position when the list changes: the panel form's Members list leaves out
/// the chair, and its External members list leaves out every member. A browser ignores a change to the <c>selected</c>
/// attribute of an option the user has clicked, so the element reused for the next person kept showing selected, although
/// the form did not hold them, and the next click sent them. Checked in Chrome: select Naidoo and Mokoena under Members,
/// replay the positional update that choosing Naidoo as chair makes, and Mokoena and Botha show selected; keyed, only
/// Mokoena does. bUnit renders markup, not elements that live on, so the rule is checked in the source.
/// </remarks>
public sealed partial class MultiSelectOptionKeyTests
{
    [Fact]
    public void EveryOptionOfAMultipleSelect_RenderedFromData_IsKeyed()
    {
        var selects = MultipleSelects();

        selects.Should().HaveCountGreaterThanOrEqualTo(2, "guard: the scan finds the panel form's Members and External members");

        var unkeyed = selects
            .SelectMany(select => UnkeyedOptions(select.Markup).Select(option => $"{select.File}: {option}"))
            .ToList();

        unkeyed.Should().BeEmpty(
            "an option of a <select multiple> rendered from data carries @key=\"…\" naming what it offers, or a browser " +
            "shows as selected someone the form does not hold once the list changes under a selection the user made");
    }

    [Fact]
    public void TheScan_FindsAnUnkeyedOption_AndPassesAKeyedOne()
    {
        // The scan's own rule, shown failing, so a scan that matched nothing would not pass every page.
        const string unkeyed = """
            <select id="panel-members" class="form-select" multiple size="6" @onchange="OnMembersChanged">
              @foreach (var candidate in _candidates)
              {
                <option value="@candidate.UserId" selected="@isSelected">@candidate.Name</option>
              }
            </select>
            """;
        const string keyed = """
            <select id="panel-members" class="form-select" multiple size="6" @onchange="OnMembersChanged">
              @foreach (var candidate in _candidates)
              {
                <option @key="candidate.UserId" value="@candidate.UserId" selected="@isSelected">@candidate.Name</option>
              }
            </select>
            """;
        const string singleSelect = """
            <select id="panel-chair" class="form-select">
              @foreach (var candidate in _candidates)
              {
                <option value="@candidate.UserId">@candidate.Name</option>
              }
            </select>
            """;
        const string staticOption = """
            <select id="tools" class="form-select" multiple>
              <option value="">None</option>
            </select>
            """;

        MultipleSelectsIn("unkeyed", unkeyed).SelectMany(select => UnkeyedOptions(select.Markup)).Should().ContainSingle();
        MultipleSelectsIn("keyed", keyed).Should().ContainSingle();
        MultipleSelectsIn("keyed", keyed).SelectMany(select => UnkeyedOptions(select.Markup)).Should().BeEmpty();
        MultipleSelectsIn("single", singleSelect).Should().BeEmpty("a single select shows its value, which Blazor sets");
        MultipleSelectsIn("static", staticOption).SelectMany(select => UnkeyedOptions(select.Markup))
            .Should().BeEmpty("an option written out, not rendered from data, is never reused for another");
    }

    private static IReadOnlyList<(string File, string Markup)> MultipleSelects()
    {
        var web = Path.Combine(SolutionRoot(), "src", "Wombat.Web");
        return Directory.EnumerateFiles(web, "*.razor", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .SelectMany(path => MultipleSelectsIn(Path.GetRelativePath(web, path), File.ReadAllText(path)))
            .ToList();
    }

    private static IEnumerable<(string File, string Markup)> MultipleSelectsIn(string file, string text)
        => Select().Matches(text)
            .Select(match => match.Value)
            .Where(markup => MultipleAttribute().IsMatch(OpeningTag(markup)))
            .Select(markup => (file, markup));

    /// <summary>The select's own tag: everything before its first option or loop.</summary>
    private static string OpeningTag(string markup)
    {
        var end = markup.Length;
        foreach (var marker in new[] { "<option", "@foreach", "@for", "@if" })
        {
            var at = markup.IndexOf(marker, StringComparison.Ordinal);
            if (at >= 0 && at < end)
            {
                end = at;
            }
        }

        return markup[..end];
    }

    private static IReadOnlyList<string> UnkeyedOptions(string markup)
        => Option().Matches(markup)
            .Select(match => match.Value)
            .Where(option => option.Contains('@') && !option.Contains("@key=", StringComparison.Ordinal))
            .ToList();

    private static string SolutionRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && directory.GetFiles("Wombat.sln").Length == 0)
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Could not find Wombat.sln.");
    }

    [GeneratedRegex(@"<select\b.*?</select>", RegexOptions.Singleline)]
    private static partial Regex Select();

    [GeneratedRegex(@"\smultiple\b")]
    private static partial Regex MultipleAttribute();

    [GeneratedRegex(@"<option\b[^>]*>")]
    private static partial Regex Option();
}
