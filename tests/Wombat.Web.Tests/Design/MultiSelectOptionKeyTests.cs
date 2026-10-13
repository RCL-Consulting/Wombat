using System.Text.RegularExpressions;
using FluentAssertions;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// A choice rendered from data for one person is keyed by what it offers (<c>@key</c>), so the selection a browser shows is
/// the one the form holds: each option of a <c>&lt;select multiple&gt;</c>, and each person's radio group in the panel
/// page's seat picker. (T257; T364, Q5; DESIGN.md § Form system)
/// </summary>
/// <remarks>
/// <para>
/// Unkeyed, Blazor reuses elements by position when the list changes. The panel form's Members list left out the chair,
/// and its External members list every member; a browser ignores a change to the <c>selected</c> attribute of an option
/// the user has clicked, so the element reused for the next person kept showing selected, although the form did not hold
/// them, and the next click sent them. Checked in Chrome: select Naidoo and Mokoena under Members, replay the positional
/// update that choosing Naidoo as chair makes, and Mokoena and Botha show selected; keyed, only Mokoena does.
/// </para>
/// <para>
/// T364 replaced both multi-selects with the seat picker (Q5): one radio group a committee member, whose row turns into
/// "Chair · chosen above" when that person is chosen as chair. A row reused by position for the next person would carry
/// the radio the user last checked there. So the rule moved with the control: a person's group (a <c>fieldset.choice-row</c>
/// whose legend is rendered from data) is keyed. The multi-select rule stays for any multi-select a page adds; none is left
/// today. bUnit renders markup, not elements that live on, so the rules are checked in the source.
/// </para>
/// </remarks>
public sealed partial class MultiSelectOptionKeyTests
{
    [Fact]
    public void EveryOptionOfAMultipleSelect_RenderedFromData_IsKeyed()
    {
        var unkeyed = RazorFiles()
            .SelectMany(file => MultipleSelectsIn(file.Path, file.Text))
            .SelectMany(select => UnkeyedOptions(select.Markup).Select(option => $"{select.File}: {option}"))
            .ToList();

        unkeyed.Should().BeEmpty(
            "an option of a <select multiple> rendered from data carries @key=\"…\" naming what it offers, or a browser " +
            "shows as selected someone the form does not hold once the list changes under a selection the user made");
    }

    [Fact]
    public void EveryPersonsRadioGroup_RenderedFromData_IsKeyed()
    {
        var groups = RazorFiles().SelectMany(file => PersonGroupsIn(file.Path, file.Text)).ToList();

        groups.Select(group => Path.GetFileName(group.File)).Should().Contain(
            "PanelEdit.razor", "guard: the scan finds the seat picker's groups, or it proves nothing");
        groups.Where(group => !group.Keyed).Select(group => $"{group.File}: {group.StartTag}").Should().BeEmpty(
            "a radio group rendered for one person carries @key=\"…\" naming them, or a row reused for the next person " +
            "shows the seat the user last chose there");
    }

    [Fact]
    public void TheScans_FindWhatIsUnkeyed_AndPassWhatIsKeyed()
    {
        // The scans' own rules, shown failing, so a scan that matched nothing would not pass every page.
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

        const string unkeyedGroup = """
            <fieldset class="form-group choice-row">
              <legend class="visually-hidden">@PanelWords.SeatFor(name)</legend>
            </fieldset>
            """;
        const string keyedGroup = """
            <fieldset class="form-group choice-row" @key="candidate.UserId">
              <legend class="visually-hidden">@PanelWords.SeatFor(name)</legend>
            </fieldset>
            """;
        const string fixedGroup = """
            <fieldset class="form-group choice-row" id="panel-scope">
              <legend>Scope</legend>
            </fieldset>
            """;

        PersonGroupsIn("unkeyed", unkeyedGroup).Should().ContainSingle().Which.Keyed.Should().BeFalse();
        PersonGroupsIn("keyed", keyedGroup).Should().ContainSingle().Which.Keyed.Should().BeTrue();
        PersonGroupsIn("fixed", fixedGroup).Should().BeEmpty("a group with a written legend is one field, never reused");
    }

    private static IEnumerable<(string Path, string Text)> RazorFiles()
    {
        var web = Path.Combine(SolutionRoot(), "src", "Wombat.Web");
        return Directory.EnumerateFiles(web, "*.razor", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Select(path => (Path.GetRelativePath(web, path), File.ReadAllText(path)));
    }

    private static IEnumerable<(string File, string Markup)> MultipleSelectsIn(string file, string text)
        => Select().Matches(text)
            .Select(match => match.Value)
            .Where(markup => MultipleAttribute().IsMatch(OpeningTag(markup)))
            .Select(markup => (file, markup));

    /// <summary>A <c>fieldset.choice-row</c> whose legend is rendered from data: one person's group.</summary>
    private static IEnumerable<(string File, string StartTag, bool Keyed)> PersonGroupsIn(string file, string text)
        => ChoiceRow().Matches(text)
            .Where(match => match.Groups["legend"].Value.Contains('@'))
            .Select(match => (file, match.Groups["tag"].Value, match.Groups["tag"].Value.Contains("@key=", StringComparison.Ordinal)));

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

    [GeneratedRegex(@"(?<tag><fieldset\b[^>]*\bchoice-row\b[^>]*>)\s*<legend\b[^>]*>(?<legend>[^<]*)</legend>", RegexOptions.Singleline)]
    private static partial Regex ChoiceRow();
}
