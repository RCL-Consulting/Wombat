using System.Text.RegularExpressions;
using FluentAssertions;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// T188: a fieldset that is one field is a <c>fieldset.form-group</c>, wherever it is. T177 made the activity form's
/// multi-choice fields form groups, so a legend among a section's fields reads as that field's label. The rule stopped
/// at ActivityForm, and the other groups sat among their pages' <c>FormField</c>s as plain fieldsets, each legend a
/// section title (1.1rem, semibold) in a grid of labels: the MSF campaign form's EPA list, the curriculum item forms'
/// Tools and minimum by training year, and the committee review's evidence picker and Present lists.
/// </summary>
/// <remarks>
/// A fieldset that holds fields (a <c>.form-grid</c> or a <c>&lt;FormField&gt;</c>: an activity form's section, the
/// curriculum item's edit row, a deferral) is a cluster, and keeps the section-size legend. Every other fieldset is one
/// field: a group of checkboxes or radios, or a field with no control to name. The scan reads every <c>&lt;fieldset</c>
/// in every <c>.razor</c> file, the <c>@code</c> block's render fragments included (the review's Present list), and
/// needs <c>form-group</c> written out in a field's class, not left to an expression.
/// </remarks>
public sealed partial class FieldGroupTests
{
    [Fact]
    public void EveryFieldLevelFieldset_IsAFormGroup()
    {
        var web = WebFile();
        var fields = new List<string>();
        var clusters = new List<string>();
        var offenders = new List<string>();

        foreach (var path in Directory.EnumerateFiles(web, "*.razor", SearchOption.AllDirectories))
        {
            var file = Path.GetFileName(path);
            foreach (var fieldset in FieldsetsIn(File.ReadAllText(path)))
            {
                if (fieldset.HoldsFields)
                {
                    clusters.Add(file);
                    continue;
                }

                fields.Add(file);
                if (!fieldset.IsFormGroup)
                {
                    offenders.Add($"{Path.GetRelativePath(web, path)}:{fieldset.Line}: {fieldset.StartTag}");
                }
            }
        }

        fields.Distinct().Should().Contain(
            ["ActivityForm.razor", "CampaignEdit.razor", "CurriculumItemsEdit.razor", "MsfRespond.razor", "ReviewDetail.razor",
             "StageMinimaEditor.razor"],
            "guard: the scan finds the field-level groups, or it proves nothing");
        // An activity form's section is a <section> named by its heading since T342 (flow 03): a card one deep, not a
        // fieldset, so the clusters left are the curriculum item's edit row and the deferral.
        clusters.Distinct().Should().Contain(
            ["CurriculumItemsEdit.razor", "ReviewDetail.razor"],
            "guard: the scan tells a fieldset of fields (a section, the edit row, a deferral) from a field");
        string.Join(Environment.NewLine, offenders).Should().BeEmpty(
            "a fieldset that is one field is a form group, so its legend reads as that field's label (DESIGN.md § Form " +
            "system, T188); every one is listed");
    }

    [Fact]
    public void EveryParagraphInAFieldLevelFieldset_IsTheSizeOfAFieldsHelp()
    {
        // T188 review. A FormField's help is a small.page-subtitle (0.9rem). The groups' help was a body-size p.muted, so
        // Tools' help stood larger than Weight's beside it, and the campaign form's EPA help sat over "Select a trainee
        // first." (a page-subtitle) at two sizes in one field. A group's help and status lines are page-subtitles; a
        // field-warning is a warning, drawn as one; and a validation-message is the group's own refusal (T342, flow 03),
        // drawn as every field's is.
        var web = WebFile();
        var paragraphs = 0;
        var offenders = new List<string>();

        foreach (var path in Directory.EnumerateFiles(web, "*.razor", SearchOption.AllDirectories))
        {
            foreach (var fieldset in FieldsetsIn(File.ReadAllText(path)).Where(fieldset => !fieldset.HoldsFields))
            {
                paragraphs += fieldset.Paragraphs.Count;
                offenders.AddRange(fieldset.Paragraphs
                    .Where(paragraph => !IsHelpSized(paragraph.StartTag))
                    .Select(paragraph => $"{Path.GetRelativePath(web, path)}:{paragraph.Line}: {paragraph.StartTag}"));
            }
        }

        paragraphs.Should().BeGreaterThanOrEqualTo(8, "guard: the scan finds the groups' help and status lines, or it proves nothing");
        string.Join(Environment.NewLine, offenders.Distinct()).Should().BeEmpty(
            "a field-level group's help and status lines are the size of a field's help (DESIGN.md § Form system, T188); " +
            "every one is listed");
    }

    [Theory]
    [InlineData("""<fieldset class="form-group"><legend>L</legend><p id="x-help" class="muted">Help</p></fieldset>""", false)]
    [InlineData("""<fieldset class="form-group"><legend>L</legend><p>Help</p></fieldset>""", false)]
    [InlineData("""<fieldset class="form-group"><legend>L</legend><p class="@(quiet ? "page-subtitle" : null)">Help</p></fieldset>""", false)]
    [InlineData("""<fieldset class="form-group"><legend>L</legend><p id="x-help" class="page-subtitle">Help</p></fieldset>""", true)]
    [InlineData("""<fieldset class="form-group"><legend>L</legend><div role="status"><p class="field-warning">Hint</p></div></fieldset>""", true)]
    [InlineData("""<fieldset class="form-group"><legend>L</legend><p id="x-msg" class="validation-message">Refused</p></fieldset>""", true)]
    // A <pre> or a <progress> is not a paragraph.
    [InlineData("""<fieldset class="form-group"><legend>L</legend><pre>x</pre><progress></progress></fieldset>""", true)]
    public void TheScan_ReadsAGroupsParagraphs_AsWritten(string razor, bool helpSized)
    {
        var fieldset = FieldsetsIn(razor).Should().ContainSingle().Subject;

        fieldset.Paragraphs.All(paragraph => IsHelpSized(paragraph.StartTag)).Should().Be(helpSized);
    }

    [Theory]
    // The shape T188 found: a group of checkboxes among the fields, with no form-group.
    [InlineData("""<fieldset class="full-width" aria-describedby="x-help"><legend>Tools</legend><div class="check-grid"></div></fieldset>""", false, false)]
    [InlineData("""<fieldset aria-describedby="@HelpId"><legend>Minimum by training year</legend><button class="btn">Add</button></fieldset>""", false, false)]
    [InlineData("""<fieldset class="form-group full-width" aria-describedby="x-help"><legend>Tools</legend></fieldset>""", false, true)]
    // ActivityForm's: form-group written out, with an expression beside it that may add full-width.
    [InlineData("""<fieldset class="form-group @(field.Type == FieldType.MultiChoice ? "full-width" : null)" aria-describedby="@helpId"><legend>L</legend></fieldset>""", false, true)]
    // A form-group only an expression can add is sometimes not one.
    [InlineData("""<fieldset class="@(multi ? "form-group" : null)"><legend>L</legend></fieldset>""", false, false)]
    [InlineData("""<fieldset class="form-group-like"><legend>L</legend></fieldset>""", false, false)]
    // A lambda's arrow is not the end of the start tag.
    [InlineData("""<fieldset @onfocusin="() => Seen()" class="form-group"><legend>L</legend></fieldset>""", false, true)]
    // Clusters: a fieldset holding a form grid, or FormFields, is not judged as a field.
    [InlineData("""<fieldset><legend>Edit PAED-001</legend><div class="form-grid form-grid--wide"><FormField Label="EPA" InputId="e"></FormField></div></fieldset>""", true, false)]
    [InlineData("""<fieldset class="detail-card detail-card--compact mb-3"><legend>Section</legend><div class="form-grid"></div></fieldset>""", true, false)]
    [InlineData("""<fieldset><legend>Defer</legend><FormField Label="Why" InputId="r"></FormField></fieldset>""", true, false)]
    public void TheScan_TellsAFieldFromACluster_AndReadsTheClassAsWritten(string razor, bool holdsFields, bool isFormGroup)
    {
        var fieldset = FieldsetsIn(razor).Should().ContainSingle().Subject;

        fieldset.HoldsFields.Should().Be(holdsFields);
        if (!holdsFields)
        {
            fieldset.IsFormGroup.Should().Be(isFormGroup);
        }
    }

    [Fact]
    public void TheScan_JudgesANestedGroup_OnItsOwn_AndSkipsCommentedMarkup()
    {
        // The review's evidence picker: a field whose options are grouped by EPA, each group a fieldset of its own.
        const string razor = """
            @* <fieldset><legend>Commented out</legend></fieldset> *@
            <fieldset class="form-group full-width">
              <legend>Evidence it rests on</legend>
              <fieldset><legend>PAED-001</legend><div class="check-grid"></div></fieldset>
              <fieldset class="form-group"><legend>Not about a single EPA</legend></fieldset>
            </fieldset>
            """;

        var fieldsets = FieldsetsIn(razor);

        fieldsets.Select(fieldset => (fieldset.Line, fieldset.HoldsFields, fieldset.IsFormGroup))
            .Should().Equal((2, false, true), (4, false, false), (5, false, true));
    }

    [Fact]
    public void AFormGroup_SpacesItsParts_WithItsOwnGap()
    {
        // A field-level fieldset holds what a FormField does not: its help as a paragraph, a status line and an action.
        // A paragraph's 1em margins made the group looser than the fields beside it, and a column stretches its items:
        // "Add year 3" ran the width of the curriculum item form. A group within a group (the evidence picker's lists by
        // EPA) has a legend the size of the field's, so it is set in by a rule down its left.
        var css = File.ReadAllText(WebFile("wwwroot", "app.css"));

        Declarations(css, ".form-group > p").Should().Contain("margin: 0");
        Declarations(css, ".form-group > .btn").Should().Contain("align-self: flex-start");
        Declarations(css, "fieldset.form-group > legend").Should().Contain(["font-size: inherit", "font-weight: inherit"]);
        Declarations(css, "fieldset.form-group fieldset.form-group").Should().Contain(
            ["border-left: 2px solid var(--border-color)", "padding-left: var(--space-md)"]);
        // A rendered legend straddles the top border, so the rule began halfway down it (T188 review). Floated, the
        // legend is a part of the group, and the rule runs its full height; the gap spaces it, so it takes no margin.
        Declarations(css, "fieldset.form-group fieldset.form-group > legend").Should().Contain(
            ["float: left", "margin: 0", "width: 100%"]);
        Declarations(css, ".stage-minima").Should().NotContain(declaration => declaration.StartsWith("margin", StringComparison.Ordinal),
            "the year list is in a form group, whose gap spaces it from the help and from Add year");
    }

    // ---- the scan ----

    /// <summary>A fieldset in a Razor file: its start tag, the line it starts on, what it is, and the paragraphs in it.</summary>
    private sealed record Fieldset(string StartTag, int Line, bool HoldsFields, bool IsFormGroup, IReadOnlyList<Paragraph> Paragraphs);

    /// <summary>A <c>&lt;p&gt;</c> inside a fieldset: its start tag and the line it starts on.</summary>
    private sealed record Paragraph(string StartTag, int Line);

    /// <summary>
    /// A paragraph whose class writes out the help's size (page-subtitle), or a warning's or a refusal's own look.
    /// </summary>
    private static bool IsHelpSized(string paragraphStartTag)
        => WrittenClasses(paragraphStartTag).Overlaps(["page-subtitle", "field-warning", "validation-message"]);

    private static List<Fieldset> FieldsetsIn(string razor)
    {
        // Comments go, keeping their line breaks so a line number still points at the file.
        var text = Comment().Replace(razor, match => new string('\n', match.Value.Count(character => character == '\n')));
        var found = new List<Fieldset>();

        foreach (Match open in FieldsetOpen().Matches(text))
        {
            var tagEnd = EndOfStartTag(text, open.Index);
            var startTag = text[open.Index..(tagEnd + 1)];
            var bodyEnd = EndOfElement(text, tagEnd + 1);
            var body = text[(tagEnd + 1)..bodyEnd];
            found.Add(new Fieldset(
                StartTag: Whitespace().Replace(startTag, " "),
                Line: LineOf(text, open.Index),
                HoldsFields: FormFieldOpen().IsMatch(body) || FormGridClass().IsMatch(body),
                IsFormGroup: WrittenClasses(startTag).Contains("form-group"),
                Paragraphs: ParagraphOpen().Matches(text[..bodyEnd], tagEnd + 1)
                    .Select(paragraph => new Paragraph(
                        Whitespace().Replace(text[paragraph.Index..(EndOfStartTag(text, paragraph.Index) + 1)], " "),
                        LineOf(text, paragraph.Index)))
                    .ToList()));
        }

        return found;
    }

    private static int LineOf(string text, int index) => text[..index].Count(character => character == '\n') + 1;

    /// <summary>
    /// The index of the <c>&gt;</c> that ends the start tag at <paramref name="start" />: not one inside a quoted value
    /// (a lambda's <c>=&gt;</c>) or an <c>@(…)</c>.
    /// </summary>
    private static int EndOfStartTag(string text, int start)
    {
        var depth = 0;
        var quoted = false;
        for (var index = start; index < text.Length; index++)
        {
            var character = text[index];
            if (character == '(')
            {
                depth++;
            }
            else if (character == ')')
            {
                depth--;
            }
            else if (character == '"' && depth == 0)
            {
                quoted = !quoted;
            }
            else if (character == '>' && depth == 0 && !quoted)
            {
                return index;
            }
        }

        throw new InvalidOperationException($"The start tag at {start} is never closed.");
    }

    /// <summary>Where the element whose content starts at <paramref name="contentStart" /> closes, nested fieldsets counted.</summary>
    private static int EndOfElement(string text, int contentStart)
    {
        var depth = 1;
        foreach (Match tag in FieldsetTag().Matches(text, contentStart))
        {
            depth += tag.Value.StartsWith("</", StringComparison.Ordinal) ? -1 : 1;
            if (depth == 0)
            {
                return tag.Index;
            }
        }

        throw new InvalidOperationException($"A <fieldset> at {contentStart} has no </fieldset>.");
    }

    /// <summary>The classes a start tag writes out: its class value without any <c>@(…)</c> or <c>@name</c> in it.</summary>
    private static IReadOnlySet<string> WrittenClasses(string startTag)
    {
        var match = ClassAttributeStart().Match(startTag);
        if (!match.Success)
        {
            return new HashSet<string>();
        }

        var value = new System.Text.StringBuilder();
        var depth = 0;
        for (var index = match.Index + match.Length; index < startTag.Length; index++)
        {
            var character = startTag[index];
            if (depth == 0 && character == '"')
            {
                break;
            }

            if (character == '@' && index + 1 < startTag.Length && startTag[index + 1] == '(')
            {
                depth++;
                index++;
                continue;
            }

            if (depth > 0)
            {
                depth += character switch { '(' => 1, ')' => -1, _ => 0 };
                continue;
            }

            value.Append(character);
        }

        return ImplicitExpression().Replace(value.ToString(), " ")
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet(StringComparer.Ordinal);
    }

    /// <summary>The declarations of the rule whose whole selector is <paramref name="selector" />, whitespace collapsed.</summary>
    private static IReadOnlyList<string> Declarations(string css, string selector)
    {
        var rules = Regex.Matches(CssComment().Replace(css, string.Empty), @"(?<selector>[^{}]+)\{(?<body>[^{}]*)\}")
            .Where(rule => Whitespace().Replace(rule.Groups["selector"].Value, " ").Trim() == selector)
            .ToList();
        rules.Should().ContainSingle($"app.css defines {selector} once");

        return rules[0].Groups["body"].Value
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(declaration => Whitespace().Replace(declaration, " "))
            .ToList();
    }

    private static string WebFile(params string[] parts)
        => Path.Combine([SolutionRoot(), "src", "Wombat.Web", .. parts]);

    private static string SolutionRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && directory.GetFiles("Wombat.sln").Length == 0)
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Could not find Wombat.sln.");
    }

    [GeneratedRegex(@"<fieldset\b")]
    private static partial Regex FieldsetOpen();

    /// <summary>A paragraph's start tag: <c>&lt;p&gt;</c> or <c>&lt;p class…</c>, not <c>&lt;pre</c> or <c>&lt;progress</c>.</summary>
    [GeneratedRegex(@"<p\b")]
    private static partial Regex ParagraphOpen();

    [GeneratedRegex(@"</?fieldset\b")]
    private static partial Regex FieldsetTag();

    [GeneratedRegex(@"<FormField\b")]
    private static partial Regex FormFieldOpen();

    /// <summary><c>form-grid</c> as a whole class, not <c>form-grid--wide</c> alone (which is only ever beside it).</summary>
    [GeneratedRegex(@"(?<![\w-])form-grid(?![\w-])")]
    private static partial Regex FormGridClass();

    [GeneratedRegex(@"(?<![\w-])class\s*=\s*""")]
    private static partial Regex ClassAttributeStart();

    /// <summary>An implicit Razor expression in a class value: <c>@name</c>, <c>@a.b</c>, <c>@Call(…)</c>'s name.</summary>
    [GeneratedRegex(@"@[\w.]+")]
    private static partial Regex ImplicitExpression();

    /// <summary>Razor and HTML comments, and a C# line comment standing on its own line.</summary>
    [GeneratedRegex(@"@\*.*?\*@|<!--.*?-->|^[ \t]*//[^\n]*", RegexOptions.Singleline | RegexOptions.Multiline)]
    private static partial Regex Comment();

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex CssComment();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
