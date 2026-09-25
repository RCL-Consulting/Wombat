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
using Wombat.Application.Features.Activities.Queries.ListActivityTypes;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Infrastructure.Activities;
using Wombat.Web.Components.Pages.Activities;
using Wombat.Web.Components.Shared.Activities;
using Wombat.Web.Services;
using Wombat.Web.Tests.Accessibility;
using Wombat.Web.Tests.Design;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// T263: a field the server refused is marked, not only named. The refusal carries the keys of the fields it names
/// (<see cref="ActivityFieldsRefusedException" />), and the page that shows it marks each of those controls as a predicted
/// refusal is marked (T192, T236): <c>.input-validation-error</c> and <c>aria-invalid="true"</c>, the control naming the
/// refusal's alert with <c>aria-describedby</c> after its help text.
/// </summary>
/// <remarks>
/// Before, the refusal named the field in words ("Presenting problem: A value is required.") and only the encounter date
/// was ever marked, and only when the page predicted its refusal. The three places a refused activity form is shown are
/// covered: <c>/activities/new</c>'s refused create, the notice of a submit refused straight after it, and a refused move
/// on the activity's own page, whether its form is writable or not.
/// </remarks>
public sealed class RefusedFieldMarkingTests : WombatTestContext
{
    private const string SchemaJson = """
        {
          "version": 1,
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "assessor_user_id", "type": "user", "label": "Assessor", "help_text": "Who will observe you." },
                { "key": "presenting_problem", "type": "text", "label": "Presenting problem", "required": true },
                { "key": "reflection", "type": "longtext", "label": "Reflection" }
              ]
            },
            {
              "key": "assessment",
              "title": "Entrustment",
              "editable_by": "field:assessor_user_id",
              "fields": [
                { "key": "overall_level", "type": "number", "label": "Supervision required for this encounter" }
              ]
            }
          ]
        }
        """;

    private const string WorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "requested", "label": "Requested", "editable_by": "field:assessor_user_id" },
            { "key": "completed", "label": "Completed", "terminal": true }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "requested", "actor": "subject|creator" },
            { "key": "complete", "from": "requested", "to": "completed", "actor": "field:assessor_user_id" }
          ]
        }
        """;

    private const string RequiredRefusal = "Presenting problem: A value is required.";

    private const string NomineeRefusal =
        "Assessor: Dr Gone cannot be named here. Only an active Assessor at the trainee's institution can be; choose someone else.";

    private static readonly string[] ControlIds = ["assessor_user_id", "presenting_problem", "reflection", "overall_level", "observed_on"];

    /// <summary>A second type on <c>/activities/new</c>'s list.</summary>
    private const int OtherTypeId = 3;

    private readonly RecordingReferenceDataService _referenceData = new();

    private readonly ActivityNotices _notices = new();

    public RefusedFieldMarkingTests()
    {
        Services.AddSingleton<IActivityReferenceDataService>(_referenceData);
        Services.AddSingleton<IWorkflowEvaluator, WorkflowEvaluator>();
        Services.AddSingleton<IFieldPermissionEvaluator, FieldPermissionEvaluator>();
        Services.AddSingleton(_notices);

        _referenceData.Nominees =
        [
            new ActivityCatalogueOption("assessor-1", "Dr One"),
            new ActivityCatalogueOption("assessor-gone", "Dr Gone")
        ];
    }

    // ---- /activities/new: a refused create ------------------------------------------------------------------------

    [Fact]
    public void NewActivity_ARefusedRequiredField_IsMarked_AndNamesTheRefusal()
    {
        SignIn("trainee-1");
        var sender = new NewSender { CreateFailure = Refused(RequiredRefusal, "presenting_problem") };
        var cut = SelectTheType(sender);

        ClickButton(cut, "Submit");
        cut.WaitForState(() => cut.FindAll($"#{NewActivity.RefusalAlertId}").Count == 1);

        cut.Find($"#{NewActivity.RefusalAlertId}").TextContent.Should().Contain(RequiredRefusal);
        ShouldBeMarked(cut.Find("#presenting_problem"), NewActivity.RefusalAlertId, "presenting_problem has no help text");
        OnlyTheseAreMarked(cut, "presenting_problem");
        IdReferences.Broken(cut).Should().BeEmpty("the refused field names an alert that is on the page");
    }

    [Fact]
    public void NewActivity_ARefusedNominee_IsMarked_ItsHelpReadFirst()
    {
        SignIn("trainee-1");
        var sender = new NewSender { CreateFailure = Refused(NomineeRefusal, "assessor_user_id") };
        var cut = SelectTheType(sender);

        cut.Find("#assessor_user_id").Change("assessor-gone");
        ClickButton(cut, "Save draft");
        cut.WaitForState(() => cut.FindAll($"#{NewActivity.RefusalAlertId}").Count == 1);

        var select = cut.Find("#assessor_user_id");
        ShouldBeMarked(select, NewActivity.RefusalAlertId);
        select.GetAttribute("aria-describedby").Should().Be(
            $"assessor_user_id-help {NewActivity.RefusalAlertId}", "the help text is read first, then the refusal (T193)");
        OnlyTheseAreMarked(cut, "assessor_user_id");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void NewActivity_ARefusalThatNamesNoField_MarksNoField()
    {
        // A refusal about no field (a type only the system files, say) is shown, and no control is blamed for it.
        SignIn("trainee-1");
        var sender = new NewSender { CreateFailure = new InvalidOperationException("The selected activity type has not been published yet.") };
        var cut = SelectTheType(sender);

        ClickButton(cut, "Submit");
        cut.WaitForState(() => cut.FindAll($"#{NewActivity.RefusalAlertId}").Count == 1);

        OnlyTheseAreMarked(cut);
        cut.FindAll("[aria-describedby]")
            .Where(element => element.GetAttribute("aria-describedby")!.Contains(NewActivity.RefusalAlertId, StringComparison.Ordinal))
            .Should().BeEmpty();
    }

    [Fact]
    public void NewActivity_TheNextRefusal_MarksOnlyTheFieldsItNames()
    {
        // The marks belong to the alert on show: the next action clears both, and its own refusal marks its own fields.
        SignIn("trainee-1");
        var sender = new NewSender { CreateFailure = Refused(RequiredRefusal, "presenting_problem") };
        var cut = SelectTheType(sender);

        ClickButton(cut, "Submit");
        cut.WaitForState(() => cut.Markup.Contains(RequiredRefusal, StringComparison.Ordinal));
        OnlyTheseAreMarked(cut, "presenting_problem");

        cut.Find("#presenting_problem").Input("Fever for three days");
        cut.Find("#assessor_user_id").Change("assessor-gone");
        sender.CreateFailure = Refused(NomineeRefusal, "assessor_user_id");
        ClickButton(cut, "Submit");
        cut.WaitForState(() => cut.Markup.Contains("cannot be named here", StringComparison.Ordinal));

        cut.Markup.Should().NotContain(RequiredRefusal);
        OnlyTheseAreMarked(cut, "assessor_user_id");
    }

    [Fact]
    public void NewActivity_SwitchingTheType_ClearsTheMarks_WithTheAlert()
    {
        // The refusal was about the type on show. Another type with a field of the same key must not inherit its mark,
        // nor point at the alert, which goes with the switch.
        SignIn("trainee-1");
        var sender = new NewSender { CreateFailure = Refused(RequiredRefusal, "presenting_problem") };
        var cut = SelectTheType(sender);

        ClickButton(cut, "Submit");
        cut.WaitForState(() => cut.FindAll($"#{NewActivity.RefusalAlertId}").Count == 1);
        OnlyTheseAreMarked(cut, "presenting_problem");

        cut.Find("#activity-type").Change(OtherTypeId.ToString(CultureInfo.InvariantCulture));
        cut.WaitForState(() => sender.EditorLoads.Contains(OtherTypeId));

        cut.FindAll($"#{NewActivity.RefusalAlertId}").Should().BeEmpty();
        cut.FindAll("#presenting_problem").Should().ContainSingle("guard: the other type has a field of the same key");
        OnlyTheseAreMarked(cut);
        IdReferences.Broken(cut).Should().BeEmpty("no field names the alert that went");
    }

    [Fact]
    public void NewActivity_ASubmitRefusedAfterTheCreate_TakesTheFieldsItNamesToTheDraft()
    {
        // The draft exists by then, so the refusal goes to its page as the notice (T127), and the fields go with it.
        SignIn("trainee-1");
        var sender = new NewSender { TransitionFailure = Refused(RequiredRefusal, "presenting_problem") };
        var cut = SelectTheType(sender);

        ClickButton(cut, "Submit");
        cut.WaitForAssertion(() => Services.GetRequiredService<FakeNavigationManager>().History.Should().ContainSingle());

        var notice = _notices.Take(7);
        notice.Should().NotBeNull();
        notice!.Message.Should().Contain(RequiredRefusal).And.Contain("Fix the fields below");
        notice.RefusedFieldKeys.Should().Equal("presenting_problem");
    }

    // ---- /activities/{id}: a refused move ---------------------------------------------------------------------------

    [Fact]
    public void ActivityView_ARefusedMove_MarksTheRequiredFieldItNames()
    {
        SignIn("trainee-1");
        var sender = new ViewSender(Detail()) { TransitionFailure = Refused(RequiredRefusal, "presenting_problem") };
        var cut = RenderActivityView(sender);

        ClickButton(cut, "Submit");
        cut.WaitForState(() => cut.FindAll($"#{ActivityView.ActionRefusalAlertId}").Count == 1);

        cut.Find($"#{ActivityView.ActionRefusalAlertId}").TextContent.Should().Contain(RequiredRefusal);
        ShouldBeMarked(cut.Find("#presenting_problem"), ActivityView.ActionRefusalAlertId, "presenting_problem has no help text");
        OnlyTheseAreMarked(cut, "presenting_problem");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void ActivityView_ARefusedMove_MarksTheNomineeItNames()
    {
        SignIn("trainee-1");
        var sender = new ViewSender(Detail()) { TransitionFailure = Refused(NomineeRefusal, "assessor_user_id") };
        var cut = RenderActivityView(sender);

        cut.Find("#assessor_user_id").Change("assessor-gone");
        ClickButton(cut, "Submit");
        cut.WaitForState(() => cut.FindAll($"#{ActivityView.ActionRefusalAlertId}").Count == 1);

        var select = cut.Find("#assessor_user_id");
        ShouldBeMarked(select, ActivityView.ActionRefusalAlertId);
        select.GetAttribute("aria-describedby").Should().Be($"assessor_user_id-help {ActivityView.ActionRefusalAlertId}");
        OnlyTheseAreMarked(cut, "assessor_user_id");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void ActivityView_AMoveThatThenSucceeds_ClearsTheMarks()
    {
        SignIn("trainee-1");
        var sender = new ViewSender(Detail()) { TransitionFailure = Refused(RequiredRefusal, "presenting_problem") };
        var cut = RenderActivityView(sender);

        ClickButton(cut, "Submit");
        cut.WaitForState(() => cut.Markup.Contains(RequiredRefusal, StringComparison.Ordinal));
        OnlyTheseAreMarked(cut, "presenting_problem");

        cut.Find("#presenting_problem").Input("Fever for three days");
        sender.TransitionFailure = null;
        ClickButton(cut, "Submit");
        cut.WaitForState(() => sender.Transitions.Count == 2);

        cut.FindAll($"#{ActivityView.ActionRefusalAlertId}").Should().BeEmpty();
        OnlyTheseAreMarked(cut);
    }

    [Fact]
    public void ActivityView_FollowingALinkToAnotherActivity_ClearsTheMarks_WithTheAlert()
    {
        // Blazor keeps the page when only the id changes (/activities/7 to /activities/8), so what it held for the move
        // refused on the first activity must go with that move's alert, not mark the same field on the second.
        SignIn("trainee-1");
        var sender = new ViewSender(Detail()) { TransitionFailure = Refused(RequiredRefusal, "presenting_problem") };
        var cut = RenderActivityView(sender);

        ClickButton(cut, "Submit");
        cut.WaitForState(() => cut.FindAll($"#{ActivityView.ActionRefusalAlertId}").Count == 1);
        OnlyTheseAreMarked(cut, "presenting_problem");

        cut.SetParametersAndRender(parameters => parameters.Add(page => page.ActivityId, 8));
        cut.WaitForState(() => sender.Loads.Contains(8) && cut.Markup.Contains("Activity details", StringComparison.Ordinal));

        cut.FindAll($"#{ActivityView.ActionRefusalAlertId}").Should().BeEmpty();
        cut.FindAll("#presenting_problem").Should().ContainSingle("guard: the second activity's form is shown");
        OnlyTheseAreMarked(cut);
        IdReferences.Broken(cut).Should().BeEmpty("no field names the alert that went");
    }

    [Fact]
    public void ActivityView_TheNoticeOfASubmitRefusedAfterTheCreate_MarksTheFieldsItNames_WhichPointAtIt()
    {
        // The notice arrives with the page, where an alert is not reliably announced, so the fields name it as well as
        // being marked (DESIGN.md § Alerts).
        SignIn("trainee-1");
        _notices.Post(
            7,
            "warning",
            $"Saved as a draft, but not submitted: {RequiredRefusal} Fix the fields below and submit again.",
            ["presenting_problem"]);

        var cut = RenderActivityView(new ViewSender(Detail()));

        cut.Find($"#{ActivityView.NoticeAlertId}").TextContent.Should().Contain(RequiredRefusal);
        ShouldBeMarked(cut.Find("#presenting_problem"), ActivityView.NoticeAlertId);
        OnlyTheseAreMarked(cut, "presenting_problem");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void ActivityView_TheNoticeOfAnyOtherOutcome_MarksNothing()
    {
        SignIn("trainee-1");
        _notices.Post(7, "success", "Draft saved. It has not been submitted.");

        var cut = RenderActivityView(new ViewSender(Detail()));

        cut.Find($"#{ActivityView.NoticeAlertId}").TextContent.Should().Contain("Draft saved");
        OnlyTheseAreMarked(cut);
    }

    [Fact]
    public void ActivityView_TheNextMove_ClearsTheNoticesMarks()
    {
        SignIn("trainee-1");
        _notices.Post(
            7, "warning", $"Saved as a draft, but not submitted: {RequiredRefusal}", ["presenting_problem"]);
        var sender = new ViewSender(Detail()) { TransitionFailure = Refused(NomineeRefusal, "assessor_user_id") };
        var cut = RenderActivityView(sender);
        OnlyTheseAreMarked(cut, "presenting_problem");

        ClickButton(cut, "Submit");
        cut.WaitForState(() => cut.FindAll($"#{ActivityView.ActionRefusalAlertId}").Count == 1);

        cut.FindAll($"#{ActivityView.NoticeAlertId}").Should().BeEmpty("the notice described the activity as it arrived");
        OnlyTheseAreMarked(cut, "assessor_user_id");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void ActivityView_OnAFormTheActorCannotWrite_ARefusedMoveStillMarksTheFieldThatStoppedIt()
    {
        // An actor with a move and no writable field sees the read-only form. A move refused for a field someone else owns
        // still shows which field it was.
        SignIn("trainee-1");
        var sender = new ViewSender(Detail(editable: [])) { TransitionFailure = Refused(RequiredRefusal, "presenting_problem") };
        var cut = RenderActivityView(sender);
        cut.Find("#presenting_problem").HasAttribute("disabled").Should().BeTrue("guard: the read-only form is shown");

        ClickButton(cut, "Submit");
        cut.WaitForState(() => cut.FindAll($"#{ActivityView.ActionRefusalAlertId}").Count == 1);

        ShouldBeMarked(cut.Find("#presenting_problem"), ActivityView.ActionRefusalAlertId);
        OnlyTheseAreMarked(cut, "presenting_problem");
    }

    // ---- the form: every control a refusal can name ---------------------------------------------------------------

    [Fact]
    public void ActivityForm_MarksEveryKindOfControlTheRefusalNames_AndNoOther()
    {
        // One control of each kind the form draws: an input, a textarea, a number, a date and a select. Only the fields
        // named are marked, and each names the refusal after its own help text.
        const string schemaJson = """
            {
              "version": 1,
              "sections": [ { "key": "s", "title": "S", "fields": [
                { "key": "title", "type": "text", "label": "Title", "help_text": "A short title." },
                { "key": "notes", "type": "longtext", "label": "Notes" },
                { "key": "count", "type": "number", "label": "Count" },
                { "key": "seen_on", "type": "date", "label": "Seen on" },
                { "key": "setting", "type": "choice", "label": "Setting", "options": ["ward", "clinic"] },
                { "key": "untouched", "type": "text", "label": "Untouched" }
              ] } ]
            }
            """;

        var cut = RenderForm(schemaJson, ["title", "notes", "count", "seen_on", "setting", "not_on_this_form"], "the-refusal");

        foreach (var id in new[] { "title", "notes", "count", "seen_on", "setting" })
        {
            ShouldBeMarked(cut.Find($"#{id}"), "the-refusal");
        }

        cut.Find("#title").GetAttribute("aria-describedby").Should().Be("title-help the-refusal");
        cut.Find("#untouched").HasAttribute("aria-invalid").Should().BeFalse();
        InvalidFieldStyleTests.ShowsInvalid(cut.Find("#untouched")).Should().BeFalse();
        cut.Find("#untouched").HasAttribute("aria-describedby").Should().BeFalse();
    }

    [Fact]
    public void ActivityForm_AMultiChoiceGroupTheRefusalNames_IsDescribedByIt_AndItsCheckboxesAreNotMarked()
    {
        // A native checkbox takes no border (DESIGN.md § Form system), so the group says it through its description.
        const string schemaJson = """
            {
              "version": 1,
              "sections": [ { "key": "s", "title": "S", "fields": [
                { "key": "settings", "type": "multichoice", "label": "Settings", "help_text": "Tick each.", "options": ["ward", "clinic"] }
              ] } ]
            }
            """;

        var cut = RenderForm(schemaJson, ["settings"], "the-refusal");

        cut.Find("fieldset fieldset").GetAttribute("aria-describedby").Should().Be("settings-help the-refusal");
        cut.FindAll("input[type=checkbox]").Should().HaveCount(2)
            .And.OnlyContain(checkbox => !checkbox.HasAttribute("aria-invalid"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ActivityForm_ARefusedEncounterDate_IsNeverAlsoCalledFileable(bool refused)
    {
        // T192 leaves the late-filing warning out when the page predicts the refusal. When it cannot (no programme start
        // was read, or the start changed after the page loaded), the server's refusal is the only word on the date, and
        // "It can still be filed" beside it would contradict it (DESIGN.md § Alerts). Unrefused, the same date warns.
        const string schemaJson = """
            {
              "version": 1,
              "observation_date_field": "observed_on",
              "sections": [ { "key": "s", "title": "S", "fields": [
                { "key": "observed_on", "type": "date", "label": "Date observed" }
              ] } ]
            }
            """;
        const string creditingRules = """
            { "counts_for": [ { "curriculum_item_match": { "epa_field": "epa_id" }, "amount": 1 } ] }
            """;
        var filedOn = new DateOnly(2026, 9, 24);
        var twentyDaysBack = filedOn.AddDays(-20).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        var cut = RenderForm(
            schemaJson,
            refused ? ["observed_on"] : [],
            "the-refusal",
            dataJson: $$"""{ "observed_on": "{{twentyDaysBack}}" }""",
            creditRulesJson: creditingRules,
            filedOn: filedOn);

        cut.FindAll("#observed_on-filing-notice").Should().ContainSingle("guard: the date carries its filing notice");
        cut.FindAll("#observed_on-filing-notice .validation-message").Should().BeEmpty("guard: the page predicts nothing");

        if (refused)
        {
            cut.FindAll(".field-warning").Should().BeEmpty("the server refused the date, so it cannot still be filed");
            ShouldBeMarked(cut.Find("#observed_on"), "the-refusal");
            cut.Find("#observed_on").GetAttribute("aria-describedby").Should().Be("observed_on-filing-notice the-refusal");
        }
        else
        {
            cut.Find("#observed_on-filing-notice .field-warning").TextContent.Should().Contain("20 days ago");
        }
    }

    [Fact]
    public void ActivityForm_WithNoRefusalId_MarksTheField_AndNamesNothingThatIsNotThere()
    {
        var cut = RenderForm(SchemaJson, ["presenting_problem"], refusalId: null);

        var input = cut.Find("#presenting_problem");
        input.GetAttribute("aria-invalid").Should().Be("true");
        input.HasAttribute("aria-describedby").Should().BeFalse();
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    // ---- helpers ----------------------------------------------------------------------------------------------------

    private static ActivityFieldsRefusedException Refused(string message, params string[] fieldKeys) => new(message, fieldKeys);

    /// <summary>
    /// Marked as T236's rule styles it and as a screen reader hears it: the hand-made class, <c>aria-invalid</c>, and the
    /// refusal's alert last among the ids that describe it.
    /// </summary>
    private static void ShouldBeMarked(IElement control, string refusalId, string? because = null)
    {
        control.ClassList.Should().Contain("input-validation-error", because ?? string.Empty);
        control.GetAttribute("aria-invalid").Should().Be("true");
        InvalidFieldStyleTests.ShowsInvalid(control).Should().BeTrue("app.css's invalid rule styles it (T236)");
        (control.GetAttribute("aria-describedby") ?? string.Empty)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Should().EndWith(refusalId, "the control names the refusal that marked it");
    }

    /// <summary>The form's controls that are marked invalid are exactly <paramref name="ids" />.</summary>
    private static void OnlyTheseAreMarked<TComponent>(IRenderedComponent<TComponent> cut, params string[] ids)
        where TComponent : IComponent
    {
        ControlIds
            .Where(id => cut.FindAll($"#{id}").Count == 1)
            .Where(id =>
            {
                var control = cut.Find($"#{id}");
                return control.GetAttribute("aria-invalid") == "true" || InvalidFieldStyleTests.ShowsInvalid(control);
            })
            .Should().BeEquivalentTo(ids);
    }

    private IRenderedComponent<ActivityForm> RenderForm(
        string schemaJson,
        IReadOnlyCollection<string> refusedFieldKeys,
        string? refusalId,
        string dataJson = "{}",
        string? creditRulesJson = null,
        DateOnly? filedOn = null)
    {
        SignIn("trainee-1");

        var cut = RenderComponent<ActivityForm>(parameters => parameters
            .Add(form => form.SchemaJson, schemaJson)
            .Add(form => form.DataJson, dataJson)
            .Add(form => form.CreditRulesJson, creditRulesJson)
            .Add(form => form.FiledOn, filedOn)
            .Add(form => form.RefusedFieldKeys, refusedFieldKeys)
            .Add(form => form.RefusalId, refusalId));
        cut.WaitForState(() => cut.FindAll(".form-container").Count > 0);

        return cut;
    }

    private void SignIn(string userId)
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized($"{userId}@test");
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, userId));
    }

    private IRenderedComponent<ActivityView> RenderActivityView(ViewSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<ActivityView>(parameters => parameters.Add(page => page.ActivityId, 7));
        cut.WaitForState(() => cut.Markup.Contains("Activity details", StringComparison.Ordinal));

        return cut;
    }

    private IRenderedComponent<NewActivity> SelectTheType(NewSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<NewActivity>();
        cut.WaitForState(() => cut.FindAll("#activity-type option").Count > 1);

        cut.Find("#activity-type").Change("2");
        cut.WaitForState(() => cut.FindAll("#assessor_user_id option").Count > 1);

        return cut;
    }

    private static void ClickButton<TComponent>(IRenderedComponent<TComponent> cut, string label)
        where TComponent : IComponent
        => cut.FindAll("button").First(button => button.TextContent.Trim() == label).Click();

    private static ActivityDetailDto Detail(IReadOnlyList<string>? editable = null)
    {
        var activity = new ActivityDto(
            7,
            2,
            "mini_cex_cpsa",
            "Mini-CEX (CPSA)",
            "mini_cex",
            1,
            SchemaJson,
            WorkflowJson,
            "[]",
            """{ "counts_for": [] }""",
            "trainee-1",
            7,
            "trainee-1",
            "draft",
            "Draft",
            """{"assessor_user_id":"assessor-1"}""",
            null,
            null,
            new DateOnly(2026, 9, 24),
            true,
            new DateTime(2026, 9, 24, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 9, 24, 8, 0, 0, DateTimeKind.Utc),
            []);

        return new ActivityDetailDto(
            activity,
            editable ?? ["assessor_user_id", "presenting_problem", "reflection"],
            [new ActivityActionDto("submit", false)]);
    }

    private sealed class ViewSender(ActivityDetailDto detail) : IScopedSender
    {
        public List<TransitionActivityCommand> Transitions { get; } = [];

        /// <summary>The ids the page loaded, in order.</summary>
        public List<int> Loads { get; } = [];

        public Exception? TransitionFailure { get; set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case GetActivityByIdQuery query:
                    Loads.Add(query.ActivityId);
                    var loaded = detail with { Activity = detail.Activity with { Id = query.ActivityId } };
                    return Task.FromResult((TResponse)(object)loaded);

                case TransitionActivityCommand command:
                    Transitions.Add(command);
                    if (TransitionFailure is not null)
                    {
                        throw TransitionFailure;
                    }

                    return Task.FromResult((TResponse)(object)detail.Activity);

                default:
                    throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class NewSender : IScopedSender
    {
        public Exception? CreateFailure { get; set; }

        /// <summary>The type ids whose editor the page loaded, in order.</summary>
        public List<int> EditorLoads { get; } = [];

        public Exception? TransitionFailure { get; init; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case ListActivityTypesQuery:
                    IReadOnlyList<ActivityTypeListItemDto> types =
                    [
                        new ActivityTypeListItemDto(2, "mini_cex_cpsa", "Mini-CEX (CPSA)", ActivityScope.Speciality, 3, 1, true),
                        new ActivityTypeListItemDto(OtherTypeId, "cbd_cpsa", "CBD (CPSA)", ActivityScope.Speciality, 3, 1, true)
                    ];
                    return Task.FromResult((TResponse)(object)types);

                case GetActivityTypeEditorQuery editorQuery:
                    // Both types draw the same form, so a field of the one has a field of the same key on the other.
                    var typeId = editorQuery.ActivityTypeId ?? 2;
                    EditorLoads.Add(typeId);
                    var editor = new ActivityTypeEditorDto(
                        typeId,
                        typeId == OtherTypeId ? "cbd_cpsa" : "mini_cex_cpsa",
                        typeId == OtherTypeId ? "CBD (CPSA)" : "Mini-CEX (CPSA)",
                        null,
                        ActivityScope.Speciality,
                        3,
                        true,
                        "mini_cex",
                        1,
                        false,
                        SchemaJson,
                        WorkflowJson,
                        "{}",
                        "[]",
                        SchemaJson,
                        WorkflowJson,
                        "{}",
                        "[]",
                        "admin-1",
                        null,
                        null,
                        []);
                    return Task.FromResult((TResponse)(object)editor);

                case CreateActivityCommand:
                    if (CreateFailure is not null)
                    {
                        throw CreateFailure;
                    }

                    return Task.FromResult((TResponse)(object)Detail().Activity);

                case GetActivityByIdQuery:
                    return Task.FromResult((TResponse)(object)Detail());

                case TransitionActivityCommand:
                    if (TransitionFailure is not null)
                    {
                        throw TransitionFailure;
                    }

                    return Task.FromResult((TResponse)(object)Detail().Activity);

                default:
                    throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
