using System.Text.RegularExpressions;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Tests.Accessibility;

/// <summary>
/// T193: a field's help text is linked to its input. <c>FormField</c> renders the help with an id
/// (<see cref="FieldHelp.Id" />), but the input is the caller's markup, which a component cannot change, so each caller
/// links it with <see cref="FieldHelp.DescribedBy" />. The rendered checks cover the activity form
/// (<c>ActivityFormHelpTextTests</c>), the curriculum item editor and the MSF page; the source checks here cover every
/// other caller, so the next FormField given help text cannot leave it unread.
/// </summary>
public sealed partial class FormFieldHelpTextLinkTests : TestContext
{
    // ---- FormField and FieldHelp ----

    [Fact]
    public void FormField_RendersItsHelpText_UnderTheIdItsInputNames()
    {
        var cut = RenderComponent<FormField>(parameters => parameters
            .Add(component => component.Label, "Period")
            .Add(component => component.InputId, "review-period")
            .Add(component => component.HelpText, "The semester the committee decides for.")
            .Add(component => component.ChildContent, (RenderFragment)(builder =>
            {
                builder.OpenElement(0, "select");
                builder.AddAttribute(1, "id", "review-period");
                builder.AddAttribute(2, "aria-describedby", FieldHelp.DescribedBy("review-period", "The semester the committee decides for."));
                builder.CloseElement();
            })));

        cut.Find("#review-period-help").TextContent.Should().Be("The semester the committee decides for.");
        cut.Find("#review-period").GetAttribute("aria-describedby").Should().Be("review-period-help");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void FormField_WithNoHelpText_RendersNoHelpElement()
    {
        var cut = RenderComponent<FormField>(parameters => parameters
            .Add(component => component.Label, "Chair")
            .Add(component => component.InputId, "panel-chair")
            .Add(component => component.ChildContent, (RenderFragment)(_ => { })));

        cut.FindAll("#panel-chair-help").Should().BeEmpty();
    }

    [Theory]
    [InlineData("Help.", new string?[] { }, "f-help")]
    [InlineData("Help.", new string?[] { "f-late-filing" }, "f-help f-late-filing")]
    [InlineData(null, new string?[] { "f-late-filing" }, "f-late-filing")]
    [InlineData("  ", new string?[] { null, "", "f-refused" }, "f-refused")]
    [InlineData("Help.", new string?[] { "f-help", "summary", "summary" }, "f-help summary")]
    [InlineData(null, new string?[] { null }, null)]
    [InlineData(null, new string?[] { }, null)]
    public void DescribedBy_NamesTheHelpFirst_ThenEachRegion_AndNothingThatIsNotThere(
        string? helpText, string?[] regionIds, string? expected)
    {
        FieldHelp.DescribedBy("f", helpText, regionIds).Should().Be(expected);
    }

    [Theory]
    [InlineData("review-period", "review-period-help")]
    [InlineData("observed_on", "observed_on-help")]
    [InlineData("patient age", "patient-age-help")]
    [InlineData("a\tb.c:d/é", "a-b-c-d---help")]
    public void TheHelpsId_KeepsOnlyWhatAnIdListCanCarry(string inputId, string expected)
    {
        // An activity form's input id is the builder's field key, which is free text. aria-describedby is a space-separated
        // list, so "patient age-help" would be two ids naming nothing.
        FieldHelp.Id(inputId).Should().Be(expected);
        FieldHelp.DescribedBy(inputId, "Help.").Should().Be(expected);
    }

    // ---- every caller ----

    [Fact]
    public void EveryFormFieldGivenHelpText_HasItsInputNameIt()
    {
        var fields = FormFieldsWithHelpText();

        fields.Should().HaveCountGreaterThanOrEqualTo(40, "guard: the scan finds the callers (43 in 13 files), or it proves nothing");
        fields.Select(field => field.File).Distinct().Should().HaveCountGreaterThanOrEqualTo(12);

        var unlinked = fields
            .Where(field => !InputNamesItsHelp(field.Markup, field.InputId))
            .Select(field => $"{field.File}: <FormField InputId=\"{field.InputId}\">")
            .ToList();

        unlinked.Should().BeEmpty(
            "an input in a FormField with help text must name it with aria-describedby=\"@FieldHelp.DescribedBy(...)\" " +
            "(or FieldHelp.Id when the help is constant), given its own InputId, or a screen reader never reads the help " +
            "with the field");
    }

    [Fact]
    public void NoFormField_WritesHelpIntoItsSlot_WhereNothingNamesIt()
    {
        // The help of 21 fields was written into the slot as a bare <small class="muted"> or <p class="form-hint">. The
        // scan above looks only at HelpText, so it never saw them, and no control named them. Help goes in HelpText; a
        // line that belongs in the slot (the Add form's "Suggested because …") has an id its control names.
        var fields = FormFields();

        fields.Should().HaveCountGreaterThanOrEqualTo(150, "guard: the scan finds every FormField (176), or it proves nothing");

        var unnamed = fields
            .SelectMany(field => UnnamedSlotText(field.Markup, field.InputId)
                .Select(tag => $"{field.File}: <FormField InputId=\"{field.InputId}\"> holds {tag}"))
            .ToList();

        unnamed.Should().BeEmpty(
            "help written into a FormField's slot is read by no screen reader: pass it as HelpText and link it with " +
            "aria-describedby=\"@FieldHelp.Id(...)\"");
    }

    [Fact]
    public void TheScan_FindsAnUnlinkedInput_AndPassesALinkedOne()
    {
        // The scan's own rule, shown failing, so a scan that matched nothing would not pass every page.
        const string unlinked = """
            <FormField Label="Chair" InputId="panel-chair" HelpText="@ChairHelpText">
              <InputSelect id="panel-chair" class="form-select" @bind-Value="_model.ChairUserId" />
            </FormField>
            """;
        const string linked = """
            <FormField Label="Chair" InputId="panel-chair" HelpText="@ChairHelpText">
              <InputSelect id="panel-chair" class="form-select" aria-describedby="@FieldHelp.DescribedBy("panel-chair", ChairHelpText)" />
            </FormField>
            """;
        const string linkedButNotItsHelp = """
            <FormField Label="Chair" InputId="panel-chair" HelpText="@ChairHelpText">
              <InputSelect id="panel-chair" class="form-select" aria-describedby="chair-refused" />
            </FormField>
            """;

        InputNamesItsHelp(unlinked, "panel-chair").Should().BeFalse();
        InputNamesItsHelp(linked, "panel-chair").Should().BeTrue();
        InputNamesItsHelp(linkedButNotItsHelp, "panel-chair").Should().BeFalse();
    }

    [Fact]
    public void TheScan_FindsALinkToAnotherFieldsHelp()
    {
        // A line copied from the field above names that field's help: it passes a check for "a FieldHelp call", and the
        // reader hears the wrong help.
        const string wrongField = """
            <FormField Label="Period" InputId="review-period" HelpText="@PeriodHelpText">
              <InputSelect id="review-period" class="form-select" aria-describedby="@FieldHelp.Id("review-trainee")" />
            </FormField>
            """;
        const string wrongFieldDescribed = """
            <FormField Label="Period" InputId="review-period" HelpText="@PeriodHelpText">
              <InputSelect id="review-period" class="form-select" aria-describedby="@FieldHelp.DescribedBy("review-trainee", PeriodHelpText)" />
            </FormField>
            """;
        const string rightField = """
            <FormField Label="Period" InputId="review-period" HelpText="@PeriodHelpText">
              <InputSelect id="review-period" class="form-select" aria-describedby="@FieldHelp.Id("review-period")" />
            </FormField>
            """;
        const string pagesOwnDescriber = """
            <FormField Label="@field.Label" InputId="@field.Key" HelpText="@field.HelpText">
              <input id="@field.Key" class="form-control" aria-describedby="@DescribedBy(field)" />
            </FormField>
            """;

        InputNamesItsHelp(wrongField, "review-period").Should().BeFalse();
        InputNamesItsHelp(wrongFieldDescribed, "review-period").Should().BeFalse();
        InputNamesItsHelp(rightField, "review-period").Should().BeTrue();
        InputNamesItsHelp(pagesOwnDescriber, "@field.Key").Should().BeTrue("a page's own describer is given no id to check");
    }

    [Fact]
    public void TheSlotScan_FindsBareHelp_AndPassesANamedLine()
    {
        const string bareSmall = """
            <FormField Label="Target" InputId="edit-count">
              <InputNumber id="edit-count" class="form-control" />
              <small class="muted">@TargetHelp</small>
            </FormField>
            """;
        const string bareHint = """
            <FormField Label="Scale" InputId="sub-speciality-scale">
              <InputSelect id="sub-speciality-scale" class="form-select" />
              <p class="form-hint">Committee STARs are limited to this scale's levels.</p>
            </FormField>
            """;
        const string idButUnnamed = """
            <FormField Label="Status" InputId="epa-active">
              <InputCheckbox id="epa-active" class="form-check-input" />
              <small id="epa-active-help" class="page-subtitle">An inactive EPA cannot be chosen.</small>
            </FormField>
            """;
        const string namedLine = """
            <FormField Label="Entrustment scale" InputId="curriculum-item-scale" HelpText="@ScaleHelp">
              <InputSelect id="curriculum-item-scale" class="form-select"
                           aria-describedby="@FieldHelp.DescribedBy("curriculum-item-scale", ScaleHelp, reason is null ? null : AddScaleReasonId)" />
              <small id="@AddScaleReasonId" class="muted">Suggested because @reason.</small>
            </FormField>
            """;

        UnnamedSlotText(bareSmall, "edit-count").Should().ContainSingle();
        UnnamedSlotText(bareHint, "sub-speciality-scale").Should().ContainSingle();
        UnnamedSlotText(idButUnnamed, "epa-active").Should().ContainSingle();
        UnnamedSlotText(namedLine, "curriculum-item-scale").Should().BeEmpty();
    }

    /// <summary>
    /// Whether every control in the field's markup carrying its <c>InputId</c> (the activity form renders one per field
    /// type) names its help: an <c>aria-describedby</c> in the same tag whose value goes through <see cref="FieldHelp" />
    /// or a page's own describer built on it. A <see cref="FieldHelp" /> call given a literal id must be given this
    /// field's own, because FormField renders the help under that id and no other.
    /// </summary>
    private static bool InputNamesItsHelp(string markup, string inputId)
    {
        var controls = ControlTag(inputId).Matches(markup);
        return controls.Count > 0 &&
               controls.All(control => DescribedByHelp().Match(control.Value) is { Success: true } link &&
                                       (!link.Groups["literal"].Success || link.Groups["literal"].Value == inputId));
    }

    /// <summary>
    /// The text written into a FormField's slot that nothing names: every <c>&lt;small&gt;</c>, and every element styled
    /// as help (<c>muted</c>, <c>form-hint</c>, <c>page-subtitle</c>), that has no id, or whose id (or the expression
    /// that makes it) the control's <c>aria-describedby</c> does not mention.
    /// </summary>
    private static IReadOnlyList<string> UnnamedSlotText(string markup, string inputId)
    {
        var control = ControlTag(inputId).Match(markup);
        var describedBy = control.Success ? DescribedByAttribute().Match(control.Value).Groups["value"].Value : string.Empty;

        return SlotText().Matches(markup)
            .Select(tag => tag.Value)
            .Where(tag => IdAttribute().Match(tag) is not { Success: true } id ||
                          string.IsNullOrEmpty(describedBy) ||
                          !describedBy.Contains(id.Groups["id"].Value.TrimStart('@').Trim('(', ')'), StringComparison.Ordinal))
            .ToList();
    }

    private static IReadOnlyList<(string File, string InputId, string Markup)> FormFieldsWithHelpText()
        // FormFields do not nest, and nothing inside one takes a HelpText of its own, so this is the FormField's.
        => FormFields().Where(field => field.Markup.Contains("HelpText=", StringComparison.Ordinal)).ToList();

    private static IReadOnlyList<(string File, string InputId, string Markup)> FormFields()
    {
        var web = Path.Combine(SolutionRoot(), "src", "Wombat.Web");
        var fields = new List<(string, string, string)>();

        foreach (var path in Directory.EnumerateFiles(web, "*.razor", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(path);
            foreach (Match open in OpeningFormField().Matches(text))
            {
                var close = text.IndexOf("</FormField>", open.Index, StringComparison.Ordinal);
                var markup = close < 0 ? text[open.Index..] : text[open.Index..close];
                var inputId = InputIdAttribute().Match(markup).Groups["id"].Value;
                fields.Add((Path.GetRelativePath(web, path), inputId, markup));
            }
        }

        return fields;
    }

    /// <summary>
    /// A tag carrying <c>id="{inputId}"</c>, from its <c>&lt;</c> to the next one: every attribute of the tag, whichever
    /// side of the id it is written on. The callers' attribute values hold <c>&gt;</c> (a lambda's arrow) but no
    /// <c>&lt;</c>; one that did would cut the tag short and fail the check, which is the safe way to be wrong.
    /// </summary>
    private static Regex ControlTag(string inputId)
        => new($"""<[A-Za-z]+\s[^<]*?(?<![\w-])id="{Regex.Escape(inputId)}"[^<]*""", RegexOptions.Singleline);

    /// <summary>
    /// A link through FieldHelp or through a page's own describer. <c>literal</c> is the id a call is given, when it is
    /// given one as a string literal; a page's own describer takes the field, not an id.
    /// </summary>
    [GeneratedRegex("""aria-describedby="@\(?[\w.]*(FieldHelp\.(Id|DescribedBy)|DescribedBy)\((?:"(?<literal>[^"]*)")?""")]
    private static partial Regex DescribedByHelp();

    /// <summary>
    /// The value of a control's <c>aria-describedby</c>, up to the end of the tag's line: a Razor expression carries
    /// quotes of its own, so the attribute's closing quote cannot be told from them by a pattern.
    /// </summary>
    [GeneratedRegex("""aria-describedby="(?<value>[^\r\n]*)""")]
    private static partial Regex DescribedByAttribute();

    /// <summary>The opening tag of a <c>small</c>, or of any element styled as help.</summary>
    [GeneratedRegex("""<(?:small\b[^>]*|[a-z]+\s[^>]*\bclass="[^"]*\b(?:muted|form-hint|page-subtitle)\b[^"]*"[^>]*)>""")]
    private static partial Regex SlotText();

    [GeneratedRegex("""\sid="(?<id>[^"]*)""")]
    private static partial Regex IdAttribute();

    [GeneratedRegex(@"<FormField\b")]
    private static partial Regex OpeningFormField();

    [GeneratedRegex("""InputId="(?<id>[^"]*)""")]
    private static partial Regex InputIdAttribute();

    private static string SolutionRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && directory.GetFiles("Wombat.sln").Length == 0)
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Could not find Wombat.sln.");
    }
}
