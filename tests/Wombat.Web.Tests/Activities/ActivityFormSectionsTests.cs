using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;
using Wombat.Web.Components.Shared.Activities;
using Wombat.Web.Tests.Accessibility;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// T342, flow 03 (R3-C-Activity's form parts, R3-Spec § 2): what the activity form shows a reader of each section, and
/// the parameters the activity page passes it (the contract between the flow's lanes): the locked section's owner line,
/// open and closed; the read-only rung row with the chosen rung and its descriptor; a filled section's attribution; and
/// the programme hint worded for whoever reads it. With the pure helpers the form and Log an activity use.
/// </summary>
public sealed class ActivityFormSectionsTests : TestContext
{
    // The seeded CPSA shape: the assessor's sections are theirs by editable_by (G2 reads each section's owner from it), and
    // the rated field names the scale whose rungs the page reads (G5).
    private const string Schema = """
        {
          "version": 1,
          "observation_date_field": "observed_on",
          "rated_level_field": "entrustment_level",
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "assessor_user_id", "type": "user", "label": "Assessor" },
                { "key": "observed_on", "type": "date", "label": "Date observed" }
              ]
            },
            {
              "key": "entrustment",
              "title": "Entrustment",
              "editable_by": "field:assessor_user_id",
              "fields": [ { "key": "entrustment_level", "type": "scale", "label": "Entrustment level", "scale_key": "cpsa" } ]
            },
            {
              "key": "feedback",
              "title": "Feedback",
              "editable_by": "field:assessor_user_id",
              "fields": [ { "key": "feedback", "type": "longtext", "label": "Feedback" } ]
            }
          ]
        }
        """;

    private static readonly IReadOnlyList<EntrustmentRung> Ladder =
    [
        new(1, "1", "Not allowed to practise the EPA."),
        new(2, "2", "Allowed to practise under proactive, full supervision."),
        new(3, "3a", null),
        new(4, "3b", null),
        new(5, "4", "Allowed to practise unsupervised."),
        new(6, "5", "Allowed to supervise others.")
    ];

    public ActivityFormSectionsTests()
    {
        this.AddTestAuthorization().SetAuthorized("trainee@test");
        Services.AddSingleton<IActivityReferenceDataService, StubActivityReferenceDataService>();
    }

    [Fact]
    public void ALockedSection_NamesItsOwner_OrTheAssessorYetToBeNamed()
    {
        var named = Render(dataJson: """{ "observed_on": "2026-09-09" }""", writable: ["assessor_user_id", "observed_on"],
            ownerName: "Fatima Khumalo");
        var unnamed = Render(dataJson: "{}", writable: ["assessor_user_id", "observed_on"]);

        named.FindAll(".form-section--locked .form-section-owner").Select(owner => owner.TextContent.Trim())
            .Should().Equal("Fatima Khumalo fills this in", "Fatima Khumalo fills this in");
        unnamed.FindAll(".form-section--locked .form-section-owner").Select(owner => owner.TextContent.Trim())
            .Should().Equal("The assessor you name fills this in", "The assessor you name fills this in");
        named.FindAll(".form-section--locked .form-section-empty").Select(empty => empty.TextContent.Trim())
            .Should().Equal("Not filled in yet.", "Not filled in yet.");

        // The reader's own section says it is theirs, beside the locked ones.
        named.Find("section:not(.form-section--locked) .form-section-owner").TextContent.Trim().Should().Be("You fill this in");
        IdReferences.Broken(named).Should().BeEmpty();
    }

    [Fact]
    public void OnAClosedRecord_ALockedSectionSaysWhoWasToFillItIn()
    {
        var named = Render(dataJson: """{ "observed_on": "2026-09-09" }""", writable: [], ownerName: "Fatima Khumalo", closed: true);
        var unnamed = Render(dataJson: """{ "observed_on": "2026-09-09" }""", writable: [], closed: true);

        named.FindAll(".form-section--locked .form-section-owner").Select(owner => owner.TextContent.Trim())
            .Should().Equal("Fatima Khumalo was to fill this in.", "Fatima Khumalo was to fill this in.");
        unnamed.Find(".form-section--locked .form-section-owner").TextContent.Trim().Should().Be("The assessor was to fill this in.");
        named.Find(".form-section--locked .form-section-empty").TextContent.Trim().Should().Be("Not filled in.");
    }

    [Fact]
    public void AFilledScale_IsTheRungRow_TheChosenRungNamedInWords_ItsDescriptorUnderIt()
    {
        // The stored value is the rung's Order: 5 is the rung labelled "4" (B12).
        var cut = Render(
            dataJson: """{ "observed_on": "2026-09-09", "entrustment_level": "5", "feedback": "Good history." }""",
            writable: [],
            rungs: Ladder,
            attributions: new Dictionary<string, string> { ["entrustment"] = "Filled in by Fatima Khumalo, 2026-09-29" });

        cut.FindAll("select, input, textarea").Should().BeEmpty();
        var row = cut.Find("#entrustment_level-in ol.rung-row");
        row.GetAttribute("aria-label").Should().Be("Entrustment level");
        var rungs = row.QuerySelectorAll("li").ToList();
        rungs.Should().HaveCount(6).And.OnlyContain(rung => !rung.HasAttribute("aria-hidden"));
        rungs.Where(rung => rung.ClassList.Contains("is-chosen")).Should().ContainSingle()
            .Which.TextContent.Should().Be("4, chosen");
        cut.Find("#entrustment_level-in .rung-descriptor").TextContent.Should().Be("4Allowed to practise unsupervised.");

        // A filled section is a plain card, with who filled it in when the page says.
        cut.FindAll(".form-section--locked").Should().BeEmpty();
        cut.Find("section[aria-labelledby='sec-entrustment'] .form-section-owner").TextContent.Trim()
            .Should().Be("Filled in by Fatima Khumalo, 2026-09-29");
        cut.Find("#feedback-in dd").TextContent.Trim().Should().Be("Good history.");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void AnAssessorWhoMayWriteTheRatedScale_ChoosesOnTheRungRow()
    {
        // T350, flow 04 (round 2, E3): the writer of the rated level field, with its ladder loaded, chooses on radios in
        // the rung row; the first radio keeps the field's input id (RatedLevelPickerTests has the rest).
        var cut = Render(dataJson: """{ "observed_on": "2026-09-09" }""", writable: ["entrustment_level", "feedback"], rungs: Ladder);

        cut.Find("#entrustment_level-in").GetAttribute("type").Should().Be("radio");
        cut.FindAll("fieldset.rung-picker ol.rung-row input[type=radio]").Should().HaveCount(6);
    }

    [Theory]
    [InlineData(true, "This date is before your programme started (2026-01-15), and will not be accepted.")]
    [InlineData(false, "This date is before the trainee's programme started (2026-01-15), and will not be accepted.")]
    public void TheProgrammeHint_IsWordedForItsReader(bool readerIsSubject, string expected)
    {
        // C12: the registrar reads "your programme"; an assessor or the committee reading the same form, "the trainee's".
        var cut = RenderComponent<ActivityForm>(parameters => parameters
            .Add(form => form.SchemaJson, Schema)
            .Add(form => form.DataJson, """{ "observed_on": "2026-01-14" }""")
            .Add(form => form.CreditRulesJson, """{ "counts_for": [ { "curriculum_item_match": { "epa_field": "epa_id" }, "amount": 1 } ] }""")
            .Add(form => form.FiledOn, new DateOnly(2026, 9, 29))
            .Add(form => form.ProgrammeStartsOn, new DateOnly(2026, 1, 15))
            .Add(form => form.ReaderIsSubject, readerIsSubject));

        // A form that opens with a date predicts its refusal at once.
        cut.Find("#observed_on-filing-notice .validation-message").TextContent.Trim().Should().Be(expected);
        cut.Find("#observed_on-in").GetAttribute("aria-invalid").Should().Be("true");
    }

    [Fact]
    public void TheHandOffName_IsReported_FromThePickersOwnOptions_AndOnlyWhenItChanges()
    {
        Services.AddSingleton<IActivityReferenceDataService>(new NomineeData());
        var reported = new List<string?>();

        var cut = RenderComponent<ActivityForm>(parameters => parameters
            .Add(form => form.SchemaJson, Schema)
            .Add(form => form.DataJson, "{}")
            .Add(form => form.SubjectUserId, "trainee-1")
            .Add(form => form.HandOffFieldKey, "assessor_user_id")
            .Add(form => form.HandOffNameChanged, EventCallback.Factory.Create<string?>(this, name => reported.Add(name))));

        cut.Find("#assessor_user_id-in").Change("assessor-1");
        cut.Find("#observed_on-in").Change("2026-09-09");

        reported.Should().Equal(null, "Fatima Khumalo");
    }

    // ---- the fix pass (T342 step 6, the build review) ----

    [Fact]
    public void ASectionTheAuthorOwns_ReadsNotFilledIn_WithNoOwnerLine()
    {
        // G2, qi_project: a submitted project whose later PDSA cycles were left empty. They are the registrar's own
        // sections (no editable_by), so nobody else is named as the one to fill them in, and they are not "yet" to come.
        var cut = RenderSeed("qi_project", """{ "project_title": "Fewer missed doses", "pdsa_1_plan": "Audit the chart." }""");

        var locked = cut.FindAll(".form-section--locked").ToList();
        locked.Select(section => section.QuerySelector("h2")!.TextContent.Trim())
            .Should().Equal("PDSA cycle 2", "PDSA cycle 3", "Close out");
        locked.Should().OnlyContain(section => section.QuerySelector(".form-section-owner") == null);
        locked.Select(section => section.QuerySelector(".form-section-empty")!.TextContent.Trim())
            .Should().OnlyContain(text => text == "Not filled in.");
    }

    [Theory]
    [InlineData("msf_cpsa")]
    [InlineData("learner_feedback_cpsa")]
    public void ASectionARoleOwns_NamesTheRole_NotAnAssessor(string seed)
    {
        // G2: the review is role:Coordinator|role:Administrator's. Nobody is named, so the role is, by its label.
        var open = RenderSeed(seed, """{ "observed_on": "2026-09-09", "respondent_count": "8" }""");
        var closed = RenderSeed(seed, """{ "observed_on": "2026-09-09", "respondent_count": "8" }""", closed: true);

        open.Find(".form-section--locked .form-section-owner").TextContent.Trim().Should().Be("A coordinator fills this in");
        open.Find(".form-section--locked .form-section-empty").TextContent.Trim().Should().Be("Not filled in yet.");
        closed.Find(".form-section--locked .form-section-owner").TextContent.Trim().Should().Be("A coordinator was to fill this in.");
        open.Markup.Should().NotContain("assessor");
    }

    [Fact]
    public void AFieldOwner_WhoIsNotTheHandOff_IsNamedFromTheFieldsOwnValue()
    {
        // G2: the page's LockedOwnerName is for its hand-off field; a section another user field owns names that
        // field's person, or the field by its label.
        Services.AddSingleton<IActivityReferenceDataService>(new NomineeData());
        const string schema = """
            {
              "version": 1,
              "sections": [
                { "key": "request", "title": "Request", "fields": [
                  { "key": "assessor_user_id", "type": "user", "label": "Assessor" },
                  { "key": "countersigner_user_id", "type": "user", "label": "Countersigner" } ] },
                { "key": "countersign", "title": "Countersignature", "editable_by": "field:countersigner_user_id", "fields": [
                  { "key": "countersigned", "type": "text", "label": "Countersigned" } ] }
              ]
            }
            """;

        RenderComponent<ActivityForm>(parameters => parameters
                .Add(form => form.SchemaJson, schema)
                .Add(form => form.DataJson, """{ "countersigner_user_id": "assessor-1" }""")
                .Add(form => form.EditableFieldKeys, new HashSet<string>(["assessor_user_id", "countersigner_user_id"]))
                .Add(form => form.HandOffFieldKey, "assessor_user_id")
                .Add(form => form.LockedOwnerName, "David Naidoo"))
            .Find(".form-section--locked .form-section-owner").TextContent.Trim().Should().Be("Fatima Khumalo fills this in");

        RenderComponent<ActivityForm>(parameters => parameters
                .Add(form => form.SchemaJson, schema)
                .Add(form => form.DataJson, "{}")
                .Add(form => form.EditableFieldKeys, new HashSet<string>(["assessor_user_id", "countersigner_user_id"]))
                .Add(form => form.HandOffFieldKey, "assessor_user_id"))
            .Find(".form-section--locked .form-section-owner").TextContent.Trim().Should().Be("The countersigner you name fills this in");
    }

    [Fact]
    public void TheRungsThePageRead_AreOnlyForTheRatedFieldsScale()
    {
        // G5: the page reads the rated field's ladder alone. A scale field on another scale shows its own options.
        const string schema = """
            {
              "version": 1,
              "rated_level_field": "entrustment_level",
              "sections": [ { "key": "s", "title": "S", "fields": [
                { "key": "entrustment_level", "type": "scale", "label": "Entrustment level", "scale_key": "cpsa" },
                { "key": "history", "type": "scale", "label": "History taking", "scale_key": "o_score" } ] } ]
            }
            """;

        var cut = RenderComponent<ActivityForm>(parameters => parameters
            .Add(form => form.SchemaJson, schema)
            .Add(form => form.DataJson, """{ "entrustment_level": "5", "history": "5" }""")
            .Add(form => form.EditableFieldKeys, new HashSet<string>())
            .Add(form => form.Rungs, Ladder));

        cut.Find("#entrustment_level-in ol.rung-row").QuerySelectorAll("li").Should().HaveCount(6);
        cut.FindAll("#history-in ol.rung-row").Should().BeEmpty("the stub offers o_score no levels, and CPSA's are not its");
    }

    [Fact]
    public void EverySummaryLinkTarget_CanTakeTheFocus()
    {
        // A2: a refusal summary links each field's ActivityFieldIds.Input. The group, the read-out, a locked field among
        // writable ones and a locked section's refusal line are not controls, so each is focusable by script.
        const string schema = """
            {
              "version": 1,
              "sections": [
                { "key": "a", "title": "A", "fields": [
                  { "key": "settings", "type": "multichoice", "label": "Settings", "options": ["ward", "clinic"] },
                  { "key": "title", "type": "text", "label": "Title" } ] },
                { "key": "b", "title": "B", "fields": [ { "key": "summary", "type": "text", "label": "Summary" } ] },
                { "key": "c", "title": "C", "fields": [ { "key": "notes", "type": "text", "label": "Notes" } ] }
              ]
            }
            """;

        var cut = RenderComponent<ActivityForm>(parameters => parameters
            .Add(form => form.SchemaJson, schema)
            .Add(form => form.DataJson, """{ "title": "T", "summary": "S" }""")
            .Add(form => form.EditableFieldKeys, new HashSet<string>(["settings"]))
            .Add(form => form.RefusedFieldKeys, ["settings", "title", "summary", "notes"]));

        cut.Find("#settings-in").LocalName.Should().Be("fieldset");
        cut.Find("#title-in").LocalName.Should().Be("dl", "a locked field among writable ones");
        cut.Find("#summary-in").LocalName.Should().Be("div", "a filled section's read-out");
        cut.Find("#notes-in").LocalName.Should().Be("p", "a locked section's refusal line");
        foreach (var id in new[] { "settings-in", "title-in", "summary-in", "notes-in" })
        {
            cut.Find($"#{id}").GetAttribute("tabindex").Should().Be("-1", id);
        }
    }

    [Fact]
    public async Task AForgedEventAtALockedField_ChangesNothing()
    {
        // G9: a locked field renders no control, so nothing on the page can send its event. The guards in UpdateValue and
        // ToggleMultiChoice are called directly, as a tampered-with circuit would.
        string? changed = null;
        const string schema = """
            { "version": 1, "sections": [ { "key": "s", "title": "S", "fields": [
              { "key": "title", "type": "text", "label": "Title" },
              { "key": "settings", "type": "multichoice", "label": "Settings", "options": ["ward"] },
              { "key": "notes", "type": "text", "label": "Notes" } ] } ] }
            """;
        var cut = RenderComponent<ActivityForm>(parameters => parameters
            .Add(form => form.SchemaJson, schema)
            .Add(form => form.DataJson, """{ "title": "Kept" }""")
            .Add(form => form.EditableFieldKeys, new HashSet<string>(["notes"]))
            .Add(form => form.DataJsonChanged, EventCallback.Factory.Create<string>(this, json => changed = json)));

        await cut.InvokeAsync(() => cut.Instance.UpdateValue("title", "Forged"));
        await cut.InvokeAsync(() => cut.Instance.ToggleMultiChoice("settings", "ward", true));
        changed.Should().BeNull("neither guard lets a locked key through");

        await cut.InvokeAsync(() => cut.Instance.UpdateValue("notes", "Written"));
        changed.Should().Contain("\"notes\":\"Written\"").And.Contain("\"title\":\"Kept\"");
    }

    [Fact]
    public void TheBuildersPreview_PutsItsSectionHeadingsUnderItsOwn()
    {
        // G8: under the builder's "Live preview" h3, a section is an h4; the activity pages keep h2.
        var preview = RenderComponent<ActivityForm>(parameters => parameters
            .Add(form => form.SchemaJson, Schema)
            .Add(form => form.SectionHeadingLevel, 4));

        preview.FindAll("section.form-section h4").Select(heading => heading.TextContent.Trim())
            .Should().Equal("Request", "Entrustment", "Feedback");
        preview.FindAll("section.form-section h2").Should().BeEmpty();
    }

    // ---- the pure helpers ----

    [Fact]
    public void AFieldRefusal_IsCutOneFieldALine_ByTheFormsLabels()
    {
        var labels = new Dictionary<string, string>
        {
            ["observed_on"] = "Date observed",
            ["presenting_problem"] = "Presenting problem",
            ["epa_id"] = "EPA"
        };

        var lines = FieldRefusals.Split(
            "Date observed: The date cannot be after today (2026-09-29). Presenting problem: A value is required. EPA: not permitted for this tool.",
            ["observed_on", "presenting_problem", "epa_id"],
            key => labels.GetValueOrDefault(key));

        lines.Should().Equal(
            new RefusalLine("observed_on", "Date observed: The date cannot be after today (2026-09-29).", "The date cannot be after today (2026-09-29)."),
            new RefusalLine("presenting_problem", "Presenting problem: A value is required.", "A value is required."),
            new RefusalLine("epa_id", "EPA: not permitted for this tool.", "not permitted for this tool."));
    }

    [Fact]
    public void ARefusalAboutNoField_IsOneLine_AndAFieldItDoesNotName_IsItsLabelAlone()
    {
        FieldRefusals.Split("The type has not been published yet.", [], _ => null)
            .Should().Equal(new RefusalLine(null, "The type has not been published yet.", null));

        FieldRefusals.Split("EPA: not permitted, and 2 more.", ["epa_id", "other_epa"], key => key == "epa_id" ? "EPA" : "Other EPA")
            .Should().Equal(
                new RefusalLine("epa_id", "EPA: not permitted, and 2 more.", "not permitted, and 2 more."),
                new RefusalLine("other_epa", "Other EPA", null));
    }

    [Theory]
    [InlineData("assessor-1", "Fatima Khumalo")]
    [InlineData("assessor-2", "ops@kgk")]
    [InlineData("gone", null)]
    [InlineData("stale", null)]
    [InlineData("", null)]
    public void ANomineesName_IsReadFromTheOption_WithoutTheAddress(string value, string? expected)
    {
        ActivityCatalogueOption[] options =
        [
            new("assessor-1", "Fatima Khumalo (fatima@kgk)"),
            new("assessor-2", "ops@kgk"),
            new("gone", NomineeNames.NotAvailableLabel),
            new("stale", "Mohammed Patel (not on the current list)")
        ];

        NomineeNames.NameOf(value, options).Should().Be(expected);
    }

    [Fact]
    public void TheFilingWords_FollowTheMoveTheStateAndTheHandOff()
    {
        var workflow = WorkflowParser.Parse("""
            {
              "version": 1,
              "initial_state": "draft",
              "states": [
                { "key": "draft", "label": "Draft" },
                { "key": "awaiting_discussion", "label": "Awaiting discussion" },
                { "key": "discussed", "label": "Discussed", "terminal": true }
              ],
              "transitions": [
                { "key": "submit", "from": "draft", "to": "awaiting_discussion", "actor": "subject|creator" },
                { "key": "record_discussion", "from": "awaiting_discussion", "to": "discussed", "actor": "field:supervisor_user_id" }
              ]
            }
            """);
        var schema = FormSchemaParser.Parse("""
            { "version": 1, "sections": [ { "key": "s", "title": "Reflection", "fields": [
              { "key": "supervisor_user_id", "type": "user", "label": "Supervisor or mentor" } ] } ] }
            """);

        var words = FilingWords.For(workflow, schema, workflow.Transitions[0]);

        words.ButtonLabel(null).Should().Be("Submit");
        words.ButtonLabel("Sarah Botha").Should().Be("Submit to Sarah Botha");
        words.CheckLead.Should().Be("When you submit:");
        words.CheckText("Sarah Botha", creditsNothing: true, daysLate: null).Should().Be(
            "it goes to Sarah Botha's Activity inbox and stays Awaiting discussion until Sarah Botha acts on it.");
        words.CheckText(null, creditsNothing: true, daysLate: 20).Should().Be(
            "it goes to the Activity inbox of the supervisor or mentor you name, and stays Awaiting discussion until that " +
            "supervisor or mentor acts on it. Filed today, 20 days after the encounter: it will be recorded as late.");
        words.RunningLabel.Should().Be("Submitting…");
    }

    private IRenderedComponent<ActivityForm> Render(
        string dataJson,
        IReadOnlyCollection<string> writable,
        string? ownerName = null,
        bool closed = false,
        IReadOnlyList<EntrustmentRung>? rungs = null,
        IReadOnlyDictionary<string, string>? attributions = null)
        => RenderComponent<ActivityForm>(parameters => parameters
            .Add(form => form.SchemaJson, Schema)
            .Add(form => form.DataJson, dataJson)
            .Add(form => form.StoredDataJson, dataJson)
            .Add(form => form.IsExistingActivity, true)
            .Add(form => form.EditableFieldKeys, new HashSet<string>(writable, StringComparer.Ordinal))
            .Add(form => form.LockedOwnerName, ownerName)
            .Add(form => form.LockedSectionsClosed, closed)
            .Add(form => form.Rungs, rungs)
            .Add(form => form.SectionAttributions, attributions));

    private IRenderedComponent<ActivityForm> RenderSeed(string seed, string dataJson, bool closed = false)
        => RenderComponent<ActivityForm>(parameters => parameters
            .Add(form => form.SchemaJson, SeedSchemas.Schema(seed))
            .Add(form => form.DataJson, dataJson)
            .Add(form => form.StoredDataJson, dataJson)
            .Add(form => form.IsExistingActivity, true)
            .Add(form => form.EditableFieldKeys, new HashSet<string>())
            .Add(form => form.LockedSectionsClosed, closed));

    private sealed class NomineeData : StubActivityReferenceDataService
    {
        public override Task<IReadOnlyList<ActivityCatalogueOption>> GetNomineeOptionsAsync(
            NomineeOptionScope scope, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ActivityCatalogueOption>>(
                [new ActivityCatalogueOption("assessor-1", "Fatima Khumalo (fatima@kgk)")]);
    }
}
