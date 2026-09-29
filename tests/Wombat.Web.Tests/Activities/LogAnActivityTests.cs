using System.Globalization;
using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Commands.CreateActivity;
using Wombat.Application.Features.Activities.Commands.TransitionActivity;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityById;
using Wombat.Application.Features.Activities.Queries.GetActivityTypeEditor;
using Wombat.Application.Features.Activities.Queries.GetFileAgainSource;
using Wombat.Application.Features.Activities.Queries.GetProgrammeStartForTrainee;
using Wombat.Application.Features.Activities.Queries.ListActivityTypes;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Infrastructure.Activities;
using Wombat.Web.Components.Pages.Activities;
using Wombat.Web.Components.Shared;
using Wombat.Web.Components.Shared.Activities;
using Wombat.Web.Services;
using Wombat.Web.Tests.Accessibility;
using Wombat.Web.Tests.Design;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// T342, flow 03 (R3-C-Log, R3-Spec): Log an activity as the redesign draws it. The instrument picker (Q1), a type's form
/// with its locked sections (C14) and its button and check line (E4, C3, C11), the refusal summary (C8), the predicted
/// refusal of an encounter date before the programme (C9, C12), File it again (E5), and the load failure (T329).
/// </summary>
public sealed class LogAnActivityTests : TestContext
{
    private const string TraineeId = "trainee-1";

    /// <summary>The Mini-CEX's shape, in the College's seed order (E6), with a scale in the assessor's section.</summary>
    private const string MiniCexSchema = """
        {
          "version": 1,
          "observation_date_field": "observed_on",
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "epa_id", "type": "epa", "label": "EPA", "required": true },
                { "key": "assessor_user_id", "type": "user", "label": "Assessor", "required": true, "help_text": "Your institution's active assessors." },
                { "key": "observed_on", "type": "date", "label": "Date observed", "required": true },
                { "key": "presenting_problem", "type": "text", "label": "Presenting problem", "required": true }
              ]
            },
            {
              "key": "entrustment",
              "title": "Entrustment",
              "editable_by": "field:assessor_user_id",
              "fields": [
                { "key": "entrustment_level", "type": "scale", "label": "Entrustment level", "scale_key": "cpsa" }
              ]
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

    private const string MiniCexWorkflow = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "requested", "label": "Requested", "editable_by": "field:assessor_user_id" },
            { "key": "completed", "label": "Completed", "terminal": true },
            { "key": "declined", "label": "Declined" },
            { "key": "cancelled", "label": "Cancelled" }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "requested", "actor": "subject|creator", "validation": "owned" },
            { "key": "complete", "from": "requested", "to": "completed", "actor": "field:assessor_user_id", "validation": "all" },
            { "key": "decline", "from": "requested", "to": "declined", "actor": "field:assessor_user_id", "requires_note": true, "validation": "draft" },
            { "key": "cancel", "from": ["draft", "requested"], "to": "cancelled", "actor": "subject|creator", "validation": "draft" }
          ]
        }
        """;

    /// <summary>The teaching log: only its author fills it in, and its Supervising consultant receives nothing.</summary>
    private const string LogSchema = """
        {
          "version": 1,
          "sections": [
            {
              "key": "session",
              "title": "Teaching session",
              "fields": [
                { "key": "topic", "type": "text", "label": "Topic", "required": true },
                { "key": "supervisor", "type": "user", "label": "Supervising consultant (optional)" }
              ]
            }
          ]
        }
        """;

    private const string LogWorkflow = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "logged", "label": "Logged", "terminal": true }
          ],
          "transitions": [
            { "key": "log", "from": "draft", "to": "logged", "actor": "subject|creator", "validation": "owned" }
          ]
        }
        """;

    private const string CreditingRules = """
        { "counts_for": [ { "curriculum_item_match": { "epa_field": "epa_id" }, "amount": 1 } ] }
        """;

    private const string NoCredit = """{ "counts_for": [] }""";

    private readonly PageSender _sender = new();
    private readonly LabelledReferenceData _referenceData = new();

    public LogAnActivityTests()
    {
        // A type chosen on the same page moves the focus to its h1 (T342, A4): the one script call these tests allow.
        JSInterop.SetupVoid(PageFocus.FocusHeadingIdentifier);
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("trainee@test");
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, TraineeId));

        Services.AddSingleton<IActivityReferenceDataService>(_referenceData);
        Services.AddSingleton<IWorkflowEvaluator, WorkflowEvaluator>();
        Services.AddSingleton<IFieldPermissionEvaluator, FieldPermissionEvaluator>();
        Services.AddSingleton<IScopedSender>(_sender);
        Services.AddScoped<ActivityNotices>();
    }

    // ---- the instrument picker (Q1) ----------------------------------------------------------------------------------

    [Fact]
    public void WithNoType_ThePageIsThePicker_ItsGroupsInOrder_EachTypeALinkToItsForm()
    {
        _sender.Types =
        [
            Item(1, "mini_cex_cpsa", "Mini-CEX (Paediatrics)", ActivityTypeShape.Rated),
            Item(2, "cbd_cpsa", "Case-Based Discussion (Paediatrics)", ActivityTypeShape.Rated),
            Item(3, "reflective_exercise_cpsa", "Reflective Exercise (Paediatrics)", ActivityTypeShape.DiscussedOrReviewed, creditsNothing: true),
            Item(4, "teaching_log", "KGK Teaching Session Log", ActivityTypeShape.LoggedByYou, creditsNothing: true)
        ];

        var cut = RenderPicker();

        cut.Find(".header-container .page-subtitle").TextContent.Trim()
            .Should().Be("Choose what you are filing. Each opens its own form.");

        var groups = cut.FindAll("section.instrument-group").ToList();
        groups.Select(group => group.QuerySelector("h2")!.TextContent.Trim())
            .Should().Equal("Rated by an assessor", "Discussed or reviewed, not rated", "Logged by you");
        groups.Select(group => group.QuerySelector(".instrument-group-head .text-muted")!.TextContent.Trim())
            .Should().Equal("2 types", "1 type", "1 type");
        groups[2].QuerySelector("p")!.TextContent.Trim()
            .Should().Be("Only you fill it in. It is logged at once, and credits nothing.");
        groups.Should().OnlyContain(group => group.GetAttribute("aria-labelledby") == group.QuerySelector("h2")!.Id);

        // Each group's types by name, each a link to its own form.
        var rated = groups[0].QuerySelectorAll("a.instrument-link").ToList();
        rated.Select(link => link.TextContent.Trim()).Should().Equal("Case-Based Discussion (Paediatrics)", "Mini-CEX (Paediatrics)");
        rated.Select(link => link.GetAttribute("href"))
            .Should().Equal("/activities/new?type=cbd_cpsa", "/activities/new?type=mini_cex_cpsa");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void AnEmptyGroup_IsNotDrawn_AndALoggedGroupThatCanCredit_DoesNotSayItCreditsNothing()
    {
        _sender.Types =
        [
            Item(1, "mini_cex_cpsa", "Mini-CEX", ActivityTypeShape.Rated),
            Item(5, "procedure_log", "Procedure log", ActivityTypeShape.LoggedByYou, creditsNothing: false)
        ];

        var cut = RenderPicker();

        cut.FindAll("section.instrument-group h2").Select(heading => heading.TextContent.Trim())
            .Should().Equal("Rated by an assessor", "Logged by you");
        cut.FindAll("section.instrument-group").ToList()[1].QuerySelector("p")!.TextContent.Trim()
            .Should().Be("Only you fill it in. It is logged at once.");
    }

    // ---- loading and a failed load (T329) ----------------------------------------------------------------------------

    [Fact]
    public void AFailedLoad_SaysSoInThePagesWords_NeverTheExceptions_AndTryAgainReadsAgain()
    {
        _sender.Types = [Item(1, "mini_cex_cpsa", "Mini-CEX", ActivityTypeShape.Rated)];
        _sender.ListFailure = new InvalidOperationException("Npgsql: connection refused at 10.0.0.3");
        NewActivityPage.NavigateTo(this, typeKey: null);

        var cut = RenderComponent<NewActivity>();

        cut.WaitForState(() => cut.FindAll(".alert-danger").Count == 1);
        cut.Find("h1").TextContent.Trim().Should().Be("Log an activity", "the header is drawn whatever the read did");
        var alert = cut.Find(".alert-danger");
        alert.TextContent.Should().Contain("Could not load what you can file.")
            .And.Contain("Nothing has changed. Try again, or come back in a few minutes.")
            .And.NotContain("Npgsql");

        _sender.ListFailure = null;
        cut.Find(".alert-danger button").Click();

        cut.WaitForState(() => cut.FindAll(".instrument-link").Count == 1);
        cut.FindAll(".alert-danger").Should().BeEmpty();
    }

    // ---- a type's form ------------------------------------------------------------------------------------------------

    [Fact]
    public void TheForm_IsHeadedByItsType_KeepsTheSeedsOrder_AndMarksWhatIsRequired()
    {
        var cut = OpenMiniCex();

        cut.Find(".header-container .page-subtitle").TextContent.Should().Contain("Mini-CEX (Paediatrics)");
        cut.Find(".header-container .page-subtitle a").GetAttribute("href").Should().Be("/activities/new");

        // Sections in the seed's order, each a card named by its heading (E6).
        cut.FindAll(".activity-form > section h2").Select(heading => heading.TextContent.Trim())
            .Should().Equal("Request", "Entrustment", "Feedback");

        // The explanation comes before the first mark it explains (A15).
        var form = cut.Find(".activity-form");
        form.FirstElementChild!.TextContent.Trim().Should().Be("* marks a field you must fill in.");

        // A required field's mark is seen, and said on the control.
        var date = cut.Find("#observed_on-in");
        date.GetAttribute("aria-required").Should().Be("true");
        cut.Find("label[for=observed_on-in] .required-mark").GetAttribute("aria-hidden").Should().Be("true");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void TheAssessorsSections_AreLocked_SayingWhoFillsThemIn_WithTheLadderPending()
    {
        var cut = OpenMiniCex();

        var locked = cut.FindAll("section.form-section--locked").ToList();
        locked.Select(section => section.QuerySelector("h2")!.TextContent.Trim()).Should().Equal("Entrustment", "Feedback");
        locked.Should().OnlyContain(section => section.QuerySelectorAll("input, select, textarea").Length == 0);
        locked[0].QuerySelector(".form-section-lockhead .form-section-owner")!.TextContent.Trim()
            .Should().Be("The assessor you name fills this in");

        // The ladder the assessor rates on, pending: read once from the list's name, its empty rungs hidden (A12).
        var ladder = locked[0].QuerySelector("ol.rung-row")!;
        ladder.GetAttribute("aria-label").Should().Be("Entrustment level: 1, 2, 3a, 3b, 4, 5");
        ladder.QuerySelectorAll("li").Should().HaveCount(6)
            .And.OnlyContain(rung => rung.GetAttribute("aria-hidden") == "true" && rung.ClassList.Contains("rung--pending"));

        // Naming the assessor names the section's owner, the button and the check line (C3, C11).
        cut.Find("#assessor_user_id-in").Change("assessor-1");

        locked = cut.FindAll("section.form-section--locked").ToList();
        locked[0].QuerySelector(".form-section-owner")!.TextContent.Trim().Should().Be("Fatima Khumalo fills this in");
        NewActivityPage.Primary(cut).TextContent.Trim().Should().Be("Submit to Fatima Khumalo");
        cut.Find(".submit-check").TextContent.Trim().Should().Be(
            "When you submit: it goes to Fatima Khumalo's Activity inbox and stays Requested until Fatima Khumalo acts on it.");

        // The chosen person is printed in full under the select, for the eye (T323).
        cut.Find("#assessor_user_id-in ~ .field-readback").TextContent.Trim().Should().Be("Fatima Khumalo (fatima@kgk)");
    }

    [Fact]
    public void BeforeAnAssessorIsNamed_TheButtonIsTheMovesOwnLabel_AndTheCheckLineSaysWhoWillHaveIt()
    {
        var cut = OpenMiniCex();

        NewActivityPage.MoveButtons(cut).Select(button => button.TextContent.Trim()).Should().Equal("Submit", "Save draft");
        NewActivityPage.Primary(cut).ClassList.Should().Contain("btn-primary");
        cut.Find(".submit-check").TextContent.Trim().Should().Be(
            "When you submit: it goes to the Activity inbox of the assessor you name, and stays Requested until that assessor acts on it.");
        cut.Find(".submit-check").HasAttribute("role").Should().BeFalse("the check line is read in place, not announced");
    }

    [Fact]
    public void ALateEncounter_IsWarnedOfOnceEntered_AndTheCheckLineSaysItWillBeRecordedAsLate()
    {
        var cut = OpenMiniCex();
        var twentyDaysBack = FilingLateness.Today().AddDays(-20);

        cut.Find("#observed_on-filing-notice").TextContent.Trim().Should().BeEmpty("nothing is entered yet");
        cut.Find("#observed_on-in").Change(Iso(twentyDaysBack));

        cut.Find("#observed_on-filing-notice .field-warning").TextContent.Should().Contain("20 days ago");
        cut.Find(".submit-check").TextContent.Should().EndWith(
            "Filed today, 20 days after the encounter: it will be recorded as late.");
    }

    [Fact]
    public void ADateBeforeTheProgramme_IsAPredictedRefusal_WordedForTheAuthor()
    {
        // C9: the field's own validation message and the invalid marks, not a warning; C12: "your programme" to its
        // registrar. No summary until a submit refuses it.
        var start = FilingLateness.Today().AddDays(-100);
        _sender.ProgrammeStart = start;
        var cut = OpenMiniCex();

        cut.Find("#observed_on-in").Change(Iso(start.AddDays(-1)));

        var date = cut.Find("#observed_on-in");
        date.GetAttribute("aria-invalid").Should().Be("true");
        InvalidFieldStyleTests.ShowsInvalid(date).Should().BeTrue();
        cut.Find("#observed_on-filing-notice .validation-message").TextContent.Trim().Should().Be(
            $"This date is before your programme started ({Iso(start)}), and will not be accepted.");
        cut.FindAll("#observed_on-filing-notice .field-warning").Should().BeEmpty();
        cut.FindAll("#refusal-summary").Should().BeEmpty();
        cut.Find(".submit-check").TextContent.Should().NotContain("recorded as late", "a refused date is not late");
    }

    [Fact]
    public void TheTeachingLog_IsLogged_ItsButtonStaysLog_WhenItsConsultantIsNamed()
    {
        _sender.Types = [Item(9, "teaching_log", "KGK Teaching Session Log", ActivityTypeShape.LoggedByYou, creditsNothing: true)];
        _sender.Editors[9] = Editor(9, "teaching_log", "KGK Teaching Session Log", LogSchema, LogWorkflow, NoCredit);
        var cut = NewActivityPage.Open(this, "teaching_log", "#topic-in");

        cut.Find("#supervisor-in").Change("assessor-1");

        NewActivityPage.MoveButtons(cut).Select(button => button.TextContent.Trim()).Should().Equal("Log", "Save draft");
        cut.Find(".submit-check").TextContent.Trim().Should().Be(
            "When you log it: it is Logged at once, and credits nothing. Nobody else acts on it.");
        cut.FindAll(".form-section--locked").Should().BeEmpty();
        cut.FindAll(".form-section-owner").Should().BeEmpty("the whole form is the author's");
    }

    // ---- outcomes -----------------------------------------------------------------------------------------------------

    [Fact]
    public void Submitted_TheNoticeSaysWhereItIs_AndWhoseInboxItIsIn()
    {
        var cut = OpenMiniCex();
        cut.Find("#assessor_user_id-in").Change("assessor-1");

        NewActivityPage.Primary(cut).Click();

        cut.WaitForAssertion(() => Services.GetRequiredService<FakeNavigationManager>().Uri.Should().EndWith("/activities/41"));
        Services.GetRequiredService<ActivityNotices>().Take(41).Should().Be(new ActivityNotice(
            "success", "Submitted. It is now Requested. It is in Fatima Khumalo's Activity inbox."));
    }

    [Fact]
    public void ARefusedCreate_IsSummarised_EachLineLinkingToItsInput_AndTheSummaryTakesTheFocus()
    {
        _sender.CreateFailure = new ActivityFieldsRefusedException(
            "Date observed: The date cannot be after today (2026-09-29). Presenting problem: A value is required.",
            ["observed_on", "presenting_problem"]);
        var cut = OpenMiniCex();

        NewActivityPage.Primary(cut).Click();
        cut.WaitForState(() => cut.FindAll("#refusal-summary").Count == 1);

        var summary = cut.Find("#refusal-summary");
        summary.GetAttribute("role").Should().Be("alert");
        summary.GetAttribute("tabindex").Should().Be("-1");
        summary.QuerySelector("strong")!.TextContent.Trim().Should().Be("Nothing was saved.");
        summary.TextContent.Should().Contain("Everything you typed is kept below.");
        summary.QuerySelectorAll("li a").Select(link => (link.GetAttribute("href"), link.TextContent.Trim())).Should().Equal(
            ("#observed_on-in", "Date observed: The date cannot be after today (2026-09-29)."),
            ("#presenting_problem-in", "Presenting problem: A value is required."));

        // Each field its own part, under it, named before the summary.
        cut.Find("#observed_on-msg").TextContent.Trim().Should().Be("The date cannot be after today (2026-09-29).");
        cut.Find("#presenting_problem-in").GetAttribute("aria-describedby").Should().Be("presenting_problem-msg refusal-summary");

        var focus = JSInterop.Invocations.Where(invocation => invocation.Identifier == "Blazor._internal.domWrapper.focus").ToList();
        focus.Should().NotBeEmpty("the summary takes the focus once it is drawn");
        focus[^1].Arguments[0].Should().BeOfType<ElementReference>()
            .Which.Id.Should().Be(cut.FindComponent<RefusalSummary>().Instance.Element.Id);
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    // ---- File it again (E5) -------------------------------------------------------------------------------------------

    [Fact]
    public void FileItAgain_CopiesTheDeclinedRequest_SaysSo_AndWarnsOfLatenessAtOnce()
    {
        var twentyDaysBack = FilingLateness.Today().AddDays(-20);
        _sender.FileAgain = new FileAgainSourceDto(
            17, 1, "mini_cex_cpsa",
            $$"""{ "observed_on": "{{Iso(twentyDaysBack)}}", "presenting_problem": "Wheeze" }""",
            ["observed_on", "presenting_problem"], false, "Fatima Khumalo", DateTime.UtcNow);
        NewActivityPage.NavigateTo(this, typeKey: null, from: 17);

        var cut = RenderComponent<NewActivity>();
        cut.WaitForState(() => cut.FindAll("#observed_on-in").Count == 1);

        var notice = cut.Find(".form-column > .alert-info");
        notice.HasAttribute("role").Should().BeFalse("a standing notice has no role (A14)");
        notice.TextContent.Trim().Should().Be(
            "Copied from your request to Fatima Khumalo, which was declined. The EPA, date and request are as you filed " +
            "them. Name someone else, then submit. Nothing is saved until you do.");

        cut.Find("#observed_on-in").GetAttribute("value").Should().Be(Iso(twentyDaysBack));
        cut.Find("#presenting_problem-in").GetAttribute("value").Should().Be("Wheeze");
        cut.FindAll("#assessor_user_id-in option[selected]").Should().BeEmpty("the assessor is left for her to name");

        // A form that opens with a date shows its warning at once (Spec § 1).
        cut.Find("#observed_on-filing-notice .field-warning").TextContent.Should().Contain("20 days ago");
        _sender.FileAgainAskedFor.Should().Equal(17);
    }

    [Fact]
    public void FileItAgain_WithTheEpaDropped_SaysWhatWasNotCopied()
    {
        _sender.FileAgain = new FileAgainSourceDto(
            17, 1, "mini_cex_cpsa", """{ "presenting_problem": "Wheeze" }""", ["presenting_problem"], true, "Fatima Khumalo", DateTime.UtcNow);
        NewActivityPage.NavigateTo(this, typeKey: null, from: 17);

        var cut = RenderComponent<NewActivity>();
        cut.WaitForState(() => cut.FindAll("#presenting_problem-in").Count == 1);

        var notice = cut.Find(".form-column > .alert-info").TextContent;
        notice.Should().Contain("The date and request are as you filed them.").And.NotContain("The EPA, date");
        notice.Should().Contain("The EPA it was filed on cannot be chosen now, so choose one again.");
    }

    [Fact]
    public void FileItAgain_IsIgnored_WhenTheQueryDoesNotAnswer()
    {
        // Not the caller's request, or not declined: the query answers null, and the address is read as though it had no
        // ?from (E5). With ?type too, that type's empty form; with neither, the picker.
        _sender.FileAgain = null;
        NewActivityPage.NavigateTo(this, typeKey: null, from: 17);

        var picker = RenderComponent<NewActivity>();
        picker.WaitForState(() => picker.FindAll(".instrument-link").Count > 0);
        picker.FindAll(".alert-info").Should().BeEmpty();

        var navigation = Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo("/activities/new?type=mini_cex_cpsa&from=17");
        picker.WaitForState(() => picker.FindAll("#observed_on-in").Count == 1);
        picker.FindAll(".alert-info").Should().BeEmpty();
        picker.Find("#presenting_problem-in").GetAttribute("value").Should().BeNullOrEmpty();
    }

    // ---- the fix pass (T342 step 6, the build review) ------------------------------------------------------------------

    [Fact]
    public void ChoosingAType_AndChoosingAnother_FocusTheHeading_ButArrivingDoesNot()
    {
        // A4: the same page, so FocusOnNavigate does not move the focus, and the link pressed is gone. Arriving is
        // FocusOnNavigate's to focus.
        _sender.Types = [Item(1, "mini_cex_cpsa", "Mini-CEX (Paediatrics)", ActivityTypeShape.Rated)];
        var cut = RenderPicker();
        HeadingFocusCalls().Should().Be(0, "arriving is FocusOnNavigate's");

        var navigation = Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo("/activities/new?type=mini_cex_cpsa");
        cut.WaitForState(() => cut.FindAll("#observed_on-in").Count == 1);
        cut.WaitForAssertion(() => HeadingFocusCalls().Should().Be(1));

        navigation.NavigateTo("/activities/new");
        cut.WaitForState(() => cut.FindAll(".instrument-link").Count == 1);
        cut.WaitForAssertion(() => HeadingFocusCalls().Should().Be(2));
    }

    [Fact]
    public void NothingToFile_IsAnEmptyState_NotABlankPage()
    {
        // G7.
        _sender.Types = [];
        NewActivityPage.NavigateTo(this, typeKey: null);

        var cut = RenderComponent<NewActivity>();
        cut.WaitForState(() => cut.FindAll(".detail-card--empty").Count == 1);

        cut.Find(".detail-card--empty .state-panel-title").TextContent.Should().Be("Nothing can be filed yet.");
        cut.Find(".detail-card--empty .state-panel-copy").TextContent.Should().Be(InstrumentPicker.EmptyBody);
        cut.FindAll(".instrument-group").Should().BeEmpty();
    }

    private int HeadingFocusCalls() => JSInterop.Invocations.Count(call => call.Identifier == PageFocus.FocusHeadingIdentifier);

    // ---- helpers ------------------------------------------------------------------------------------------------------

    private IRenderedComponent<NewActivity> RenderPicker()
    {
        NewActivityPage.NavigateTo(this, typeKey: null);
        var cut = RenderComponent<NewActivity>();
        cut.WaitForState(() => cut.FindAll(".instrument-link").Count > 0);
        return cut;
    }

    private IRenderedComponent<NewActivity> OpenMiniCex()
        => NewActivityPage.Open(this, "mini_cex_cpsa", "#assessor_user_id-in option[value='assessor-1']");

    private static string Iso(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static ActivityTypeListItemDto Item(int id, string key, string name, ActivityTypeShape shape, bool creditsNothing = false)
        => new(id, key, name, ActivityScope.Speciality, 3, 1, true, shape, creditsNothing);

    private static ActivityTypeEditorDto Editor(int id, string key, string name, string schema, string workflow, string credit)
        => new(id, key, name, null, ActivityScope.Speciality, 3, true, "mini_cex", 1, false,
            schema, workflow, credit, "[]", schema, workflow, credit, "[]", "admin-1", null, null, [], false, [], null);

    private sealed class LabelledReferenceData : StubActivityReferenceDataService
    {
        public override Task<IReadOnlyList<ActivityCatalogueOption>> GetNomineeOptionsAsync(
            NomineeOptionScope scope, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ActivityCatalogueOption>>(
            [
                new ActivityCatalogueOption("assessor-1", "Fatima Khumalo (fatima@kgk)"),
                new ActivityCatalogueOption("assessor-2", "David Naidoo (david@kgk)")
            ]);

        public override Task<IReadOnlyList<ActivityCatalogueOption>> GetEntrustmentScaleLevelOptionsAsync(
            string? scaleKey, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<ActivityCatalogueOption>>(
            [
                new ActivityCatalogueOption("1", "1"), new ActivityCatalogueOption("2", "2"),
                new ActivityCatalogueOption("3", "3a"), new ActivityCatalogueOption("4", "3b"),
                new ActivityCatalogueOption("5", "4"), new ActivityCatalogueOption("6", "5")
            ]);
    }

    /// <summary>Answers the page's queries with the Mini-CEX unless a test says otherwise, and records what it was asked.</summary>
    private sealed class PageSender : IScopedSender
    {
        public PageSender()
            => Editors[1] = Editor(1, "mini_cex_cpsa", "Mini-CEX (Paediatrics)", MiniCexSchema, MiniCexWorkflow, CreditingRules);

        public IReadOnlyList<ActivityTypeListItemDto> Types { get; set; } =
            [Item(1, "mini_cex_cpsa", "Mini-CEX (Paediatrics)", ActivityTypeShape.Rated)];

        public Dictionary<int, ActivityTypeEditorDto> Editors { get; } = [];

        public Exception? ListFailure { get; set; }

        public Exception? CreateFailure { get; set; }

        public DateOnly? ProgrammeStart { get; set; }

        public FileAgainSourceDto? FileAgain { get; set; }

        public List<int> FileAgainAskedFor { get; } = [];

        private string _state = "draft";

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case ListActivityTypesQuery:
                    if (ListFailure is not null)
                    {
                        throw ListFailure;
                    }

                    return Task.FromResult((TResponse)(object)Types);

                case GetActivityTypeEditorQuery query:
                    return Task.FromResult((TResponse)(object)Editors[query.ActivityTypeId!.Value]);

                case GetProgrammeStartForTraineeQuery:
                    return Task.FromResult((TResponse)(object)ProgrammeStart!);

                case GetFileAgainSourceQuery query:
                    FileAgainAskedFor.Add(query.SourceActivityId);
                    return Task.FromResult((TResponse)(object)FileAgain!);

                case CreateActivityCommand:
                    if (CreateFailure is not null)
                    {
                        throw CreateFailure;
                    }

                    return Task.FromResult((TResponse)(object)Activity());

                case GetActivityByIdQuery:
                    ActivityDetailDto detail = new(Activity(), [], [new ActivityActionDto("submit", false), new ActivityActionDto("cancel", false)]);
                    return Task.FromResult((TResponse)(object)detail);

                case TransitionActivityCommand:
                    _state = "requested";
                    return Task.FromResult((TResponse)(object)Activity());

                default:
                    throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        private ActivityDto Activity()
            => new(
                41, 1, "mini_cex_cpsa", "Mini-CEX (Paediatrics)", "mini_cex", 1,
                MiniCexSchema, MiniCexWorkflow, "[]", CreditingRules,
                TraineeId, 7, TraineeId,
                _state, _state == "draft" ? "Draft" : "Requested",
                """{ "assessor_user_id": "assessor-1" }""",
                null, null, new DateOnly(2026, 9, 24), true,
                new DateTime(2026, 9, 24, 8, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 9, 24, 8, 0, 0, DateTimeKind.Utc),
                []);
    }
}
