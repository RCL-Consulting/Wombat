using System.Text.Json.Nodes;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.Epas;
using Wombat.Web.Components.Shared.Activities;
using Wombat.Web.Tests.Accessibility;
using Wombat.Web.Tests.Design;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// T350, flow 04, lane D (R3-C-Activity's to-rate and rated states; R3-Spec § 3 and § 4; round 2, E2, E3, C5, C7, C9;
/// round 1, E3 and E4; note 15; nit A7): the rater's form. The rung picker on the rated level field only, when its ladder
/// loads; "What each rung means" as two renderings until a rung is chosen; the request fold's twins under
/// <see cref="ActivityForm.FoldFilledSections" />; a person read by name; and flow 03's readers unchanged.
/// </summary>
public sealed class RatedLevelPickerTests : TestContext
{
    private const string RatedKey = "overall_level";

    // The College's ladder as seeded, descriptors verbatim with their typographic apostrophes (C5).
    private static readonly IReadOnlyList<EntrustmentRung> SeededLadder = LoadSeededLadder();

    public RatedLevelPickerTests()
    {
        this.AddTestAuthorization().SetAuthorized("assessor@test");
        Services.AddSingleton<IActivityReferenceDataService>(new ReferenceData());
    }

    // ---- the picker: where it is drawn ----

    [Fact]
    public void ThePicker_IsOnTheRatedField_WhoseLadderLoaded_AndEveryOtherScaleKeepsTheSelect()
    {
        // E3: the demo Mini-CEX has six scale fields, and only the rated one's ladder is read.
        var cut = RenderSeed("mini_cex", data: "{}", writable: AllFieldsOf("mini_cex"), rungs: SeededLadder);
        var rated = SeedSchemas.Fields("mini_cex").Single(field => field.Key == RatedKeyOf("mini_cex"));

        cut.FindAll("fieldset.rung-picker").Should().ContainSingle();
        cut.FindAll("input[type=radio]").Should().OnlyContain(radio => radio.GetAttribute("name") == rated.Key);
        var otherScales = SeedSchemas.Fields("mini_cex")
            .Where(field => field.Type == Wombat.Domain.Activities.Schema.FieldType.Scale && field.Key != rated.Key)
            .ToList();
        otherScales.Should().NotBeEmpty("guard: the demo type carries other scale fields");
        foreach (var field in otherScales)
        {
            cut.Find($"#{field.Key}-in").LocalName.Should().Be("select", field.Key);
        }
    }

    [Fact]
    public void ALadderThatDidNotLoad_KeepsTheSelect()
    {
        // E3: the page passes no ladder when it cannot read one (ActivityView.RungsAsync); the builder's preview and Log
        // an activity pass none at all.
        var failed = RenderSeed("mini_cex_cpsa", data: "{}", writable: [RatedKey], rungs: null);
        var empty = RenderSeed("mini_cex_cpsa", data: "{}", writable: [RatedKey], rungs: []);

        foreach (var cut in new[] { failed, empty })
        {
            cut.FindAll("fieldset.rung-picker, input[type=radio]").Should().BeEmpty();
            cut.Find($"#{RatedKey}-in").LocalName.Should().Be("select");
        }
    }

    [Fact]
    public void AReaderWhoCannotWriteTheRatedField_ReadsTheRungRow_NotThePicker()
    {
        // The committee's reading and the registrar's own page: flow 03's read-only RungRow, as built.
        var cut = RenderSeed("mini_cex_cpsa", data: """{ "overall_level": "5" }""", writable: [], rungs: SeededLadder);

        cut.FindAll("fieldset.rung-picker, input").Should().BeEmpty();
        cut.Find($"#{RatedKey}-in ol.rung-row li.is-chosen").TextContent.Should().Be("4, chosen");
    }

    // ---- the picker: what it is ----

    [Fact]
    public void WithNoneChosen_SixRadios_NoneChecked_EachDescribedByItsDescriptorOutsideTheFolds()
    {
        var cut = RenderSeed("mini_cex_cpsa", data: "{}", writable: [RatedKey], rungs: SeededLadder);

        var picker = cut.Find("fieldset.rung-picker");
        picker.ClassList.Should().Contain("form-group");
        picker.QuerySelector("legend")!.TextContent.Should().Be("Supervision required for this encounter * required");
        picker.GetAttribute("aria-describedby").Should().Be($"{RatedKey}-help");
        cut.Find($"#{RatedKey}-help").TextContent.Should()
            .Be("The supervision this activity actually required on this occasion - not a judgement of the trainee's worth.");

        var radios = picker.QuerySelectorAll("ol.rung-row > li > label.rung.rung-choice > input[type=radio]").ToList();
        radios.Should().HaveCount(6);
        radios.Should().OnlyContain(radio => !radio.HasAttribute("checked"));
        radios.Select(radio => radio.GetAttribute("value")).Should().Equal("1", "2", "3", "4", "5", "6");
        radios.Select(radio => radio.Id).Should().Equal(
            $"{RatedKey}-in", $"{RatedKey}-in-2", $"{RatedKey}-in-3", $"{RatedKey}-in-4", $"{RatedKey}-in-5", $"{RatedKey}-in-6");
        radios.Select(radio => radio.ParentElement!.TextContent.Trim()).Should().Equal("1", "2", "3a", "3b", "4", "5");

        foreach (var (radio, rung) in radios.Zip(SeededLadder))
        {
            var descriptionId = radio.GetAttribute("aria-describedby");
            descriptionId.Should().Be($"rung-desc-{rung.Order}");
            var description = cut.Find($"#{descriptionId}");
            description.ClassList.Should().Contain("visually-hidden");
            description.TextContent.Should().Be(rung.Description);
            description.Closest("details").Should().BeNull("C7: read whichever fold is shut");
        }

        cut.FindAll(".rung-check, .rung-descriptor").Should().BeEmpty();
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void WithNoneChosen_WhatEachRungMeans_IsOpenWide_AndShutNarrow()
    {
        // Round 2, E2: two read-only renderings toggled by CSS at 641 px, no script; neither carries an id.
        var cut = RenderSeed("mini_cex_cpsa", data: "{}", writable: [RatedKey], rungs: SeededLadder);

        var legends = cut.FindAll("details.rung-legend").ToList();
        legends.Should().HaveCount(2);
        legends[0].ClassList.Should().Contain("only-wide");
        legends[0].HasAttribute("open").Should().BeTrue();
        legends[1].ClassList.Should().Contain("only-narrow");
        legends[1].HasAttribute("open").Should().BeFalse();

        foreach (var legend in legends)
        {
            legend.QuerySelector("summary")!.TextContent.Should().Contain("What each rung means").And.Contain("ShowHide");
            legend.QuerySelector("summary .section-fold-show .fold-show")!.TextContent.Should().Be("Show");
            legend.QuerySelector("summary .section-fold-show .fold-hide")!.TextContent.Should().Be("Hide");
            legend.QuerySelectorAll("[id]").Should().BeEmpty("the twin that is away names nothing");
            legend.QuerySelectorAll(".rung-legend-list > div").Select(row => (
                    row.QuerySelector("dt")!.TextContent,
                    row.QuerySelector("dd")!.TextContent))
                .Should().Equal(SeededLadder.Select(rung => (rung.Label, rung.Description!)));
        }
    }

    [Fact]
    public void WithOneChosen_ItIsCheckedAndFilled_ItsDescriptorUnderTheRow_AndOneShutLegend()
    {
        // The stored value is the rung's Order: 5 is the rung labelled "4".
        var cut = RenderSeed("mini_cex_cpsa", data: """{ "overall_level": "5" }""", writable: [RatedKey], rungs: SeededLadder);

        var radios = cut.FindAll("input[type=radio]").ToList();
        radios.Where(radio => radio.HasAttribute("checked")).Should().ContainSingle()
            .Which.Id.Should().Be($"{RatedKey}-in-5");
        var chosen = cut.FindAll("label.rung-choice.is-chosen").Should().ContainSingle().Subject;
        chosen.TextContent.Trim().Should().Be("4chosen");
        chosen.QuerySelector(".rung-check")!.GetAttribute("aria-hidden").Should().Be("true", "the browser says checked");

        cut.Find(".rung-descriptor").TextContent.Should()
            .Be("4Unsupervised practice. The trainee carries the responsibility for the activity.");
        cut.Find(".rung-descriptor").Closest("fieldset").Should().BeNull("the descriptor is under the row, not group help");

        var legend = cut.FindAll("details.rung-legend").Should().ContainSingle().Subject;
        legend.HasAttribute("open").Should().BeFalse();
        legend.ClassList.Should().NotContain("only-wide").And.NotContain("only-narrow");
    }

    [Fact]
    public void APress_IsOneRoundTrip_ThroughTheFormsOwnUpdate()
    {
        string? changed = null;
        var cut = RenderComponent<ActivityForm>(parameters => parameters
            .Add(form => form.SchemaJson, SeedSchemas.Schema("mini_cex_cpsa"))
            .Add(form => form.DataJson, "{}")
            .Add(form => form.EditableFieldKeys, new HashSet<string>([RatedKey]))
            .Add(form => form.Rungs, SeededLadder)
            .Add(form => form.DataJsonChanged, EventCallback.Factory.Create<string>(this, json => changed = json)));

        cut.Find($"#{RatedKey}-in-4").Change(true);

        JsonNode.Parse(changed!)![RatedKey]!.GetValue<string>().Should().Be("4");
        cut.Find($"#{RatedKey}-in-4").HasAttribute("checked").Should().BeTrue();
        cut.Find("label.rung-choice.is-chosen").TextContent.Trim().Should().Be("3bchosen");
        cut.Find(".rung-descriptor b").TextContent.Should().Be("3b");
    }

    [Fact]
    public void ARefusalThatNamedTheRatedField_LandsOnTheFirstRadio_AndMarksEachOne()
    {
        // Flow 03's refusal summary links #<key>-in; the first radio carries it.
        var cut = RenderComponent<ActivityForm>(parameters => parameters
            .Add(form => form.SchemaJson, SeedSchemas.Schema("mini_cex_cpsa"))
            .Add(form => form.DataJson, "{}")
            .Add(form => form.EditableFieldKeys, new HashSet<string>([RatedKey]))
            .Add(form => form.Rungs, SeededLadder)
            .Add(form => form.RefusedFieldKeys, [RatedKey])
            .Add(form => form.RefusalMessage, "Supervision required for this encounter: A value is required.")
            .Add(form => form.RefusalId, "refusal-summary"));

        cut.Find($"#{RatedKey}-in").GetAttribute("type").Should().Be("radio");
        cut.Find($"#{RatedKey}-msg").TextContent.Should().Be("A value is required.");
        var group = cut.Find("fieldset.rung-picker");
        group.GetAttribute("aria-describedby").Should().Be($"{RatedKey}-help {RatedKey}-msg refusal-summary");

        // The build review's A5: aria-invalid is on the group, where ARIA 1.2 permits it (a radiogroup), not on each radio;
        // and the summary link lands on a radio, which names the message itself, since a pair that reads no group
        // description on a member would otherwise say nothing of the refusal.
        group.GetAttribute("role").Should().Be("radiogroup");
        group.GetAttribute("aria-invalid").Should().Be("true");
        cut.FindAll("input[type=radio]").Should().OnlyContain(radio => !radio.HasAttribute("aria-invalid"));
        cut.FindAll("input[type=radio]").Should().OnlyContain(radio =>
            radio.GetAttribute("aria-describedby")!.Split(' ', StringSplitOptions.None).Contains($"{RatedKey}-msg"));
    }

    [Fact]
    public void Unrefused_TheGroupIsNotInvalid_AndNoRadioNamesAMessage()
    {
        var cut = RenderSeed("mini_cex_cpsa", data: "{}", writable: [RatedKey], rungs: SeededLadder);

        cut.Find("fieldset.rung-picker").HasAttribute("aria-invalid").Should().BeFalse();
        cut.FindAll("input[type=radio]").Should().OnlyContain(radio =>
            radio.GetAttribute("aria-describedby") == null || !radio.GetAttribute("aria-describedby")!.Contains("-msg"));
    }

    [Fact]
    public void ARungWithNoDescriptor_NamesNoDescription()
    {
        // Nit A7: no aria-describedby naming an empty or missing span.
        IReadOnlyList<EntrustmentRung> ladder = [new(1, "1", "Observe only."), new(2, "2", null), new(3, "3", "  ")];
        var cut = RenderSeed("mini_cex_cpsa", data: "{}", writable: [RatedKey], rungs: ladder);

        cut.FindAll("input[type=radio]").Select(radio => radio.GetAttribute("aria-describedby"))
            .Should().Equal("rung-desc-1", null, null);
        cut.FindAll(".rung-legend-list > div").Should().HaveCount(2, "one per rendering, each listing the one descriptor");
        cut.FindAll("[aria-describedby='']").Should().BeEmpty();
    }

    // ---- the request fold ----

    [Fact]
    public void UnderFoldFilledSections_EachFilledSectionIsDrawnTwice_TheFoldsIdsSuffixed()
    {
        const string data = """{ "epa_id": "11", "assessor_user_id": "assessor-1", "observed_on": "2026-09-30", "setting": "ward", "presenting_problem": "Fever", "complexity": "low" }""";
        var cut = RenderSeed("mini_cex_cpsa", data, writable: [RatedKey, "strengths", "improvements", "plan"],
            rungs: SeededLadder, fold: true,
            attributions: new Dictionary<string, string> { ["request"] = "Filled in by Anele Dlamini, 2026-09-30" });

        var card = cut.Find("section[aria-labelledby='sec-request']");
        card.ClassList.Should().Contain("only-wide");

        var fold = cut.Find("details.section-fold");
        fold.ClassList.Should().Contain(["detail-card", "form-section", "only-narrow"]);
        fold.HasAttribute("open").Should().BeFalse();
        fold.QuerySelector("summary .section-fold-head h2")!.Id.Should().Be("sec-request-narrow");
        fold.QuerySelector("summary .section-fold-head h2")!.TextContent.Should().Be("Request");
        fold.QuerySelector("summary .section-fold-show")!.TextContent.Should().Be("ShowHide");
        fold.QuerySelector("summary .section-fold-owner")!.TextContent.Should().Be("Filled in by Anele Dlamini, 2026-09-30");
        fold.QuerySelectorAll(".section-fold-body dl > div").Select(row => row.Id)
            .Should().Equal("epa_id-in-narrow", "assessor_user_id-in-narrow", "observed_on-in-narrow", "setting-in-narrow",
                "presenting_problem-in-narrow", "complexity-in-narrow");
        fold.QuerySelectorAll("[id]").Should().OnlyContain(element => element.Id!.EndsWith("-narrow"));

        // Only the filled section folds: the reader's own section is not drawn twice.
        cut.FindAll("details.section-fold").Should().ContainSingle();
        cut.FindAll("fieldset.rung-picker").Should().ContainSingle();
        IdReferences.Broken(cut).Should().BeEmpty();
        cut.FindAll("[id]").GroupBy(element => element.Id).Where(group => group.Count() > 1).Select(group => group.Key)
            .Should().BeEmpty("no id is drawn twice");
    }

    // The build review's A4 and G1: a refusal naming a field of a filled section. On a phone the card is not displayed, so
    // the fold opens itself, showing the refusal line, and wombat.focusById falls back to the fold's copy
    // (WombatJs_FocusById_FallsBackToTheNarrowCopy).
    [Fact]
    public void ARefusalNamingAFilledField_OpensItsFold()
    {
        const string data = """{ "epa_id": "11", "assessor_user_id": "assessor-1", "observed_on": "2026-09-30", "setting": "ward", "complexity": "low" }""";
        var cut = RenderComponent<ActivityForm>(parameters => parameters
            .Add(form => form.SchemaJson, SeedSchemas.Schema("mini_cex_cpsa"))
            .Add(form => form.DataJson, data)
            .Add(form => form.StoredDataJson, data)
            .Add(form => form.IsExistingActivity, true)
            .Add(form => form.EditableFieldKeys, new HashSet<string>([RatedKey, "strengths", "improvements", "plan"]))
            .Add(form => form.Rungs, SeededLadder)
            .Add(form => form.FoldFilledSections, true)
            .Add(form => form.RefusedFieldKeys, ["presenting_problem"])
            .Add(form => form.RefusalMessage, "Presenting problem: A value is required.")
            .Add(form => form.RefusalId, "refusal-summary"));

        var fold = cut.Find("details.section-fold");
        fold.HasAttribute("open").Should().BeTrue("the refused field's only displayed copy is in the fold");
        fold.QuerySelector("#presenting_problem-in-narrow .validation-message").Should().NotBeNull("the refusal line is in the fold");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void WombatJs_FocusById_FallsBackToTheNarrowCopy()
    {
        var script = File.ReadAllText(Stylesheet.WebFile("wwwroot", "wombat.js"));

        script.Should().Contain("getClientRects().length === 0", "a target that is not displayed (.only-wide on a phone)");
        script.Should().Contain("document.getElementById(id + \"-narrow\")", "falls back to the fold's copy");
        script.Should().Contain("closest(\"details\")", "whose fold is opened first");
    }

    [Fact]
    public void WithoutFoldFilledSections_NothingIsDrawnTwice()
    {
        // Every other reader keeps the order (R3): the registrar, the committee, Log an activity and the preview.
        const string data = """{ "observed_on": "2026-09-30", "presenting_problem": "Fever" }""";
        var cut = RenderSeed("mini_cex_cpsa", data, writable: [RatedKey], rungs: SeededLadder);

        cut.FindAll("details.section-fold, .only-wide.form-section, [id$='-narrow']").Should().BeEmpty();
    }

    // ---- people by name; flow 03's readers ----

    [Fact]
    public void APersonField_IsReadByName_NotTheDirectorysNameAndAddress()
    {
        // Note 15; round 1, E4: the Request's Assessor reads "Fatima Khumalo", not "Fatima Khumalo (fatima@kgk)".
        var cut = RenderSeed("mini_cex_cpsa", """{ "assessor_user_id": "assessor-1", "observed_on": "2026-09-30" }""",
            writable: [RatedKey], rungs: SeededLadder, fold: true);

        cut.Find("#assessor_user_id-in dd").TextContent.Trim().Should().Be("Fatima Khumalo");
        cut.Find("#assessor_user_id-in-narrow dd").TextContent.Trim().Should().Be("Fatima Khumalo");
        cut.Markup.Should().NotContain("fatima@kgk");
    }

    [Fact]
    public void TheDeclinedPagesLockedSections_KeepNotFilledIn_AndThePendingRungRow()
    {
        // C5: as built. The declined request's Entrustment and Feedback, closed.
        var cut = RenderComponent<ActivityForm>(parameters => parameters
            .Add(form => form.SchemaJson, SeedSchemas.Schema("mini_cex_cpsa"))
            .Add(form => form.DataJson, """{ "observed_on": "2026-09-30" }""")
            .Add(form => form.StoredDataJson, """{ "observed_on": "2026-09-30" }""")
            .Add(form => form.IsExistingActivity, true)
            .Add(form => form.EditableFieldKeys, new HashSet<string>())
            .Add(form => form.LockedSectionsClosed, true)
            .Add(form => form.LockedOwnerName, "Mohammed Patel")
            .Add(form => form.Rungs, SeededLadder));

        var locked = cut.FindAll(".form-section--locked").ToList();
        locked.Should().NotBeEmpty();
        locked.Select(section => section.QuerySelector(".form-section-empty")!.TextContent.Trim())
            .Should().OnlyContain(text => text == "Not filled in.");
        var pending = cut.Find(".form-section--locked ol.rung-row");
        pending.GetAttribute("aria-label").Should().Be("Supervision required for this encounter: 1, 2, 3a, 3b, 4, 5");
        pending.QuerySelectorAll("li.rung--pending").Should().HaveCount(6);
        cut.FindAll("input, fieldset.rung-picker").Should().BeEmpty();
    }

    [Fact]
    public void NoControl_SaysRequiredTwice_AndNothingNamesAnEmptyDescription()
    {
        // Nit A7: a control with aria-required has no visually hidden "required" in its label; a group, which cannot carry
        // aria-required, keeps it in its legend. No aria-describedby is ever empty.
        var cut = RenderSeed("mini_cex_cpsa", data: "{}", writable: AllFieldsOf("mini_cex_cpsa"), rungs: SeededLadder);

        foreach (var control in cut.FindAll("[aria-required='true']"))
        {
            var label = cut.Find($"label[for='{control.Id}']");
            label.QuerySelectorAll(".visually-hidden").Should().BeEmpty(control.Id);
        }

        cut.FindAll("fieldset legend .visually-hidden").Select(hidden => hidden.TextContent.Trim()).Should().OnlyContain(text => text == "required");
        cut.FindAll("[aria-describedby]").Should().OnlyContain(element => !string.IsNullOrWhiteSpace(element.GetAttribute("aria-describedby")));
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void TheBuildersPreview_KeepsTheSelect()
    {
        var preview = RenderComponent<ActivityForm>(parameters => parameters
            .Add(form => form.SchemaJson, SeedSchemas.Schema("mini_cex_cpsa"))
            .Add(form => form.SectionHeadingLevel, 4));

        preview.Find($"#{RatedKey}-in").LocalName.Should().Be("select");
        preview.FindAll("fieldset.rung-picker, details").Should().BeEmpty();
    }

    // ---- the stylesheet ----

    [Fact]
    public void ARungCell_Is3Point5Rem_AndEveryFoldsSummary44Px()
    {
        // Spec § 5 and C8: the cells at every width, the folds' summaries, a phone's 44 px as 2.75rem.
        var css = Stylesheet.AppCss();

        css.RulesFor(".rung-choice").Should().ContainSingle().Which.Value("min-height").Should().Be("3.5rem");
        css.RulesFor(".rung-legend > summary").Should().ContainSingle().Which.Value("min-height").Should().Be("2.75rem");
        css.RulesFor(".section-fold > summary").Should().ContainSingle().Which.Value("min-height").Should().Be("2.75rem");
        css.RulesFor(".rung-choice input").Should().ContainSingle().Which.Value("accent-color").Should().Be("var(--secondary-color)");
        css.RulesFor(".rung-choice.is-chosen input").Should().ContainSingle().Which.Value("accent-color").Should().Be("var(--on-fill)");
        css.RulesFor(".rung-picker").Should().ContainSingle().Which.Value("min-width").Should().Be("0",
            "a fieldset is as wide as its content by default, and the six cells must share the width at 390");
    }

    // ---- helpers ----

    private IRenderedComponent<ActivityForm> RenderSeed(
        string seed,
        string data,
        IReadOnlyCollection<string> writable,
        IReadOnlyList<EntrustmentRung>? rungs,
        bool fold = false,
        IReadOnlyDictionary<string, string>? attributions = null)
        => RenderComponent<ActivityForm>(parameters => parameters
            .Add(form => form.SchemaJson, SeedSchemas.Schema(seed))
            .Add(form => form.DataJson, data)
            .Add(form => form.StoredDataJson, data)
            .Add(form => form.IsExistingActivity, true)
            .Add(form => form.EditableFieldKeys, new HashSet<string>(writable, StringComparer.Ordinal))
            .Add(form => form.Rungs, rungs)
            .Add(form => form.FoldFilledSections, fold)
            .Add(form => form.SectionAttributions, attributions));

    private static IReadOnlyCollection<string> AllFieldsOf(string seed) => SeedSchemas.Fields(seed).Select(field => field.Key).ToList();

    private static string RatedKeyOf(string seed)
        => Wombat.Domain.Activities.Schema.FormSchemaParser.Parse(SeedSchemas.Schema(seed)).RatedLevelField!;

    private static IReadOnlyList<EntrustmentRung> LoadSeededLadder()
    {
        var path = Stylesheet.SolutionFile("src", "Wombat.Infrastructure", "Persistence", "Seeds", "paediatric-epa-v11.1.json");
        var levels = JsonNode.Parse(File.ReadAllText(path))!["scale"]!["levels"]!.AsArray();
        return levels.Select(level => new EntrustmentRung(
                level!["order"]!.GetValue<int>(),
                level["label"]!.GetValue<string>(),
                level["description"]!.GetValue<string>()))
            .ToList();
    }

    // The directory's labels, and a level list for every scale field, so a field that keeps the select renders one.
    private sealed class ReferenceData : StubActivityReferenceDataService
    {
        public override Task<ActivityCatalogueOption?> GetUserOptionAsync(string userId, CancellationToken cancellationToken = default)
            => Task.FromResult<ActivityCatalogueOption?>(userId == "assessor-1" ? new("assessor-1", "Fatima Khumalo (fatima@kgk)") : null);

        public override Task<IReadOnlyList<ActivityCatalogueOption>> GetNomineeOptionsAsync(
            NomineeOptionScope scope, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ActivityCatalogueOption>>([new("assessor-1", "Fatima Khumalo (fatima@kgk)")]);

        public override Task<IReadOnlyList<ActivityCatalogueOption>> GetEntrustmentScaleLevelOptionsAsync(
            string? scaleKey, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ActivityCatalogueOption>>(
                [new("1", "1"), new("2", "2"), new("3", "3a"), new("4", "3b"), new("5", "4"), new("6", "5")]);
    }
}
