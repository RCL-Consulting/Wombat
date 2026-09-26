using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Commands.CreateActivity;
using Wombat.Application.Features.Activities.Commands.TransitionActivity;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityById;
using Wombat.Application.Features.Activities.Queries.GetActivityTypeEditor;
using Wombat.Application.Features.Activities.Queries.ListActivityTypes;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Activities;
using Wombat.Web.Components.Pages.Activities;
using Wombat.Web.Services;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// T102: the two pages that host a nominee picker hand <see cref="Wombat.Web.Components.Shared.Activities.ActivityForm" />
/// the scope the write path will judge against, and remount it after a refusal so its list is fresh.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ActivityView" /> renders an activity that exists: its user fields offer the people of the activity's
/// STAMPED institution, read off the detail DTO the page was already authorised to load. <see cref="NewActivity" />
/// renders one that does not exist yet: it passes no stamp, and the service resolves the institution the create will
/// stamp. Mixing the two up offers people the gate then refuses.
/// </para>
/// <para>
/// After a refused action the form is remounted (a generation <c>@key</c>) so it asks again: the person the server just
/// refused may have fallen off the list since the page loaded, and the fresh list says so. The working copy survives.
/// </para>
/// </remarks>
public sealed class NomineePickerPageTests : WombatTestContext
{
    private const string SchemaJson = """
        {
          "version": 1,
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "epa_id", "type": "text", "label": "EPA" },
                { "key": "assessor_user_id", "type": "user", "label": "Assessor" }
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

    private const string Refusal =
        "Assessor: Dr Gone cannot be named here. Assessor must name an active Assessor at the trainee's institution; choose someone else.";

    private readonly RecordingReferenceDataService _recorder = new();

    public NomineePickerPageTests()
    {
        Services.AddSingleton<IActivityReferenceDataService>(_recorder);
        Services.AddSingleton<IWorkflowEvaluator, WorkflowEvaluator>();
        Services.AddSingleton<IFieldPermissionEvaluator, FieldPermissionEvaluator>();
        Services.AddScoped<ActivityNotices>();

        _recorder.Nominees =
        [
            new ActivityCatalogueOption("assessor-1", "Dr One"),
            new ActivityCatalogueOption("assessor-gone", "Dr Gone")
        ];
    }

    // ---- ActivityView ----

    [Fact]
    public void ActivityView_PassesTheActivitysSubjectAndStampedInstitution_AsAnExistingActivity()
    {
        // The viewer here is a coordinator filing for the trainee, so the subject can only have come off the activity.
        SignIn("coordinator-2");

        RenderActivityView(new ViewSender(Detail(institutionId: 7)));

        var scope = _recorder.NomineeScopes.Should().ContainSingle().Subject;
        scope.SubjectUserId.Should().Be("trainee-1", "the subject is the one person a nominee field may never name");
        scope.ForExistingActivity.Should().BeTrue();
        scope.ActivityInstitutionId.Should().Be(7, "an existing activity's nominee is judged against its stamp");
        scope.RequiredRoles.Should().Equal([WombatRoles.Assessor]);
        scope.StoredValue.Should().Be("assessor-1");
    }

    [Fact]
    public void ActivityView_WithNoStamp_StillSaysExisting_SoTheServiceListsNobody()
    {
        // A null stamp is not "resolve it from the subject": the gate accepts nobody on such an activity, so the
        // picker must offer nobody too.
        SignIn("trainee-1");

        RenderActivityView(new ViewSender(Detail(institutionId: null)));

        var scope = _recorder.NomineeScopes.Should().ContainSingle().Subject;
        scope.ForExistingActivity.Should().BeTrue();
        scope.ActivityInstitutionId.Should().BeNull();
    }

    [Fact]
    public void ActivityView_ReadOnly_AsksOnlyForTheNamedPersonsLabel()
    {
        // The read-only branch (ActivityDetail): no writable field, so the viewer is shown who was named and is not
        // sent the institution's list of assessors.
        SignIn("someone-else");
        _recorder.Labels["assessor-1"] = new ActivityCatalogueOption("assessor-1", "Dr One");

        var cut = RenderActivityView(new ViewSender(Detail(institutionId: 7, editable: [], actions: [])));

        _recorder.NomineeScopes.Should().BeEmpty();
        _recorder.UserLabelRequests.Should().Equal(["assessor-1"]);
        cut.FindAll("#assessor_user_id option").Single(option => option.HasAttribute("selected"))
            .TextContent.Trim().Should().Be("Dr One");
    }

    [Fact]
    public void ActivityView_ARefusedAction_RemountsTheForm_WhichAsksForAFreshList()
    {
        SignIn("trainee-1");
        var sender = new ViewSender(Detail(institutionId: 7)) { TransitionFailure = new InvalidOperationException(Refusal) };
        var cut = RenderActivityView(sender);

        _recorder.NomineeScopes.Should().HaveCount(1);

        cut.Find("#assessor_user_id").Change("assessor-gone");
        TheRefusedPersonFallsOffTheList();
        ClickButton(cut, "Submit");
        cut.WaitForState(() => cut.Markup.Contains("cannot be named here"));

        sender.Transitions.Should().ContainSingle("the refusal came from the server, on the move the trainee asked for");
        _recorder.NomineeScopes.Should().HaveCount(2, "the remounted form asks the directory again");

        var fresh = _recorder.NomineeScopes[1];
        fresh.ForExistingActivity.Should().BeTrue();
        fresh.ActivityInstitutionId.Should().Be(7);
        fresh.StoredValue.Should().Be("assessor-1",
            "only the value the activity stores is passed on to be labelled; the hand-picked working value is never looked " +
            "up, because it may be anyone's id (the leak the T102 review found)");

        ShowsTheRefusedChoiceAsNotAvailable(cut);
        _recorder.UserLabelRequests.Should().BeEmpty("the refused id is labelled on the page, never looked up");
    }

    [Fact]
    public void ActivityView_ADraftWhoseStoredNomineeIsNoLongerListed_ShowsThemAsNotAvailable()
    {
        // Where a Submit refused at the transition on /activities/new lands (T127): the draft stores the nominee the
        // server just refused, and the directory no longer lists them. The select must still show who the next move
        // would send, so the trainee sees that they have to choose someone else. The stored value is passed to be
        // labelled by the directory, which is allowed; it is never looked up by id on the page.
        SignIn("trainee-1");
        TheRefusedPersonFallsOffTheList();

        var cut = RenderActivityView(new ViewSender(Detail(institutionId: 7, dataJson: """{"assessor_user_id":"assessor-gone"}""")));

        _recorder.NomineeScopes.Should().ContainSingle().Which.StoredValue.Should().Be("assessor-gone");
        ShowsTheRefusedChoiceAsNotAvailable(cut);
        _recorder.UserLabelRequests.Should().BeEmpty();
    }

    [Fact]
    public void ActivityView_ARefusedAction_KeepsWhatWasTyped()
    {
        // The remount is a new component instance. It must be handed the working copy, not the stored data, or a
        // refusal would silently undo every edit on the page.
        SignIn("trainee-1");
        var sender = new ViewSender(Detail(institutionId: 7, editable: ["epa_id", "assessor_user_id"]))
        {
            TransitionFailure = new InvalidOperationException(Refusal)
        };
        var cut = RenderActivityView(sender);

        cut.Find("#epa_id").Input("12");
        cut.Find("#assessor_user_id").Change("assessor-gone");
        TheRefusedPersonFallsOffTheList();
        ClickButton(cut, "Submit");
        cut.WaitForState(() => cut.Markup.Contains("cannot be named here"));

        cut.Find("#epa_id").GetAttribute("value").Should().Be("12");
        ShowsTheRefusedChoiceAsNotAvailable(cut);
        _recorder.UserLabelRequests.Should().BeEmpty();
    }

    [Fact]
    public void ActivityView_AfterARefusal_ChoosingSelect_ClearsTheRefusedNominee()
    {
        // The refused person is shown as selected, so "Select…" is a different option and choosing it is a real change:
        // it takes the id out of the working copy, and the next move no longer sends it.
        SignIn("trainee-1");
        var sender = new ViewSender(Detail(institutionId: 7)) { TransitionFailure = new InvalidOperationException(Refusal) };
        var cut = RenderActivityView(sender);

        cut.Find("#assessor_user_id").Change("assessor-gone");
        TheRefusedPersonFallsOffTheList();
        ClickButton(cut, "Submit");
        cut.WaitForState(() => cut.Markup.Contains("cannot be named here"));
        sender.Transitions[0].DataPatchJson.Should().Be("""{"assessor_user_id":"assessor-gone"}""");

        cut.Find("#assessor_user_id").Change("");
        sender.TransitionFailure = null;
        ClickButton(cut, "Submit");
        cut.WaitForState(() => sender.Transitions.Count == 2);

        sender.Transitions[1].DataPatchJson.Should().Be("""{"assessor_user_id":null}""",
            "the key is gone from the working copy, which the patch sends as a cleared field, not as the refused id");
        _recorder.UserLabelRequests.Should().BeEmpty();
    }

    // ---- NewActivity ----

    [Fact]
    public void NewActivity_AsksForTheCreatePagesScope_NoStampAndTheCurrentUserAsSubject()
    {
        // The creator IS the subject on this page, and nothing is stamped yet: the service resolves the institution
        // the create will stamp, by the same helper ActivityService stamps with.
        SignIn("trainee-1");

        SelectTheType(new NewSender());

        var scope = _recorder.NomineeScopes.Should().ContainSingle().Subject;
        scope.ForExistingActivity.Should().BeFalse();
        scope.ActivityInstitutionId.Should().BeNull();
        scope.SubjectUserId.Should().Be("trainee-1");
        scope.RequiredRoles.Should().Equal([WombatRoles.Assessor]);
        scope.StoredValue.Should().BeNull();
    }

    [Fact]
    public void NewActivity_ARefusedSave_RemountsTheForm_WhichAsksForAFreshList_AndKeepsTheChoice()
    {
        SignIn("trainee-1");
        var sender = new NewSender { CreateFailure = new InvalidOperationException(Refusal) };
        var cut = SelectTheType(sender);

        cut.Find("#assessor_user_id").Change("assessor-gone");
        TheRefusedPersonFallsOffTheList();
        ClickButton(cut, "Save draft");
        cut.WaitForState(() => cut.Markup.Contains("cannot be named here"));

        sender.Creates.Should().ContainSingle()
            .Which.InitialDataJson.Should().Contain("assessor-gone");
        _recorder.NomineeScopes.Should().HaveCount(2, "the remounted form asks the directory again");
        _recorder.NomineeScopes[1].StoredValue.Should().BeNull("nothing is stored on the create page, so nothing is looked up");
        _recorder.NomineeScopes[1].ForExistingActivity.Should().BeFalse("it is still the create page");

        ShowsTheRefusedChoiceAsNotAvailable(cut);
        _recorder.UserLabelRequests.Should().BeEmpty("the refused id is labelled on the page, never looked up");
    }

    [Fact]
    public void NewActivity_AfterARefusal_ChoosingSelect_ClearsTheRefusedNominee()
    {
        SignIn("trainee-1");
        var sender = new NewSender { CreateFailure = new InvalidOperationException(Refusal) };
        var cut = SelectTheType(sender);

        cut.Find("#assessor_user_id").Change("assessor-gone");
        TheRefusedPersonFallsOffTheList();
        ClickButton(cut, "Save draft");
        cut.WaitForState(() => cut.Markup.Contains("cannot be named here"));

        cut.Find("#assessor_user_id").Change("");
        sender.CreateFailure = null;
        ClickButton(cut, "Save draft");
        cut.WaitForState(() => sender.Creates.Count == 2);

        sender.Creates[1].InitialDataJson.Should().Be("{}", "choosing Select… took the refused id out of the working copy");
        _recorder.UserLabelRequests.Should().BeEmpty();
    }

    [Fact]
    public void NewActivity_ASubmitRefusedAtCreate_RemountsTheFormToo()
    {
        SignIn("trainee-1");
        var sender = new NewSender { CreateFailure = new InvalidOperationException(Refusal) };
        var cut = SelectTheType(sender);

        cut.Find("#assessor_user_id").Change("assessor-gone");
        TheRefusedPersonFallsOffTheList();
        ClickButton(cut, "Submit");
        cut.WaitForState(() => cut.Markup.Contains("cannot be named here"));

        sender.Transitions.Should().BeEmpty("the create was refused, so there was nothing to submit");
        _recorder.NomineeScopes.Should().HaveCount(2);
        ShowsTheRefusedChoiceAsNotAvailable(cut);
        _recorder.UserLabelRequests.Should().BeEmpty();
    }

    [Fact]
    public void NewActivity_ASubmitRefusedAtTheTransition_TakesTheRefusalToTheDraft()
    {
        // Submit on this page is a create followed by the first transition, and either may be the one that refuses
        // (the transition is the author's hand-on, where an unchanged nominee is judged again). Until T127 a refusal at
        // the transition remounted this form, as a refused create does. But the draft exists by then, and a second
        // press here would create another, so the page leaves for the draft and the refusal goes with it. The draft's
        // page loads its own form, which asks for the list afresh and shows the stored, refused nominee as not
        // available (ActivityView_ADraftWhoseStoredNomineeIsNoLongerListed_ShowsThemAsNotAvailable).
        SignIn("trainee-1");
        var sender = new NewSender { TransitionFailure = new InvalidOperationException(Refusal) };
        var cut = SelectTheType(sender);

        cut.Find("#assessor_user_id").Change("assessor-gone");
        TheRefusedPersonFallsOffTheList();
        ClickButton(cut, "Submit");
        cut.WaitForAssertion(() => Services.GetRequiredService<FakeNavigationManager>().History.Should().ContainSingle()
            .Which.Uri.Should().Be("/activities/7"));

        sender.Creates.Should().ContainSingle();
        sender.Transitions.Should().ContainSingle().Which.TransitionKey.Should().Be("submit");
        var notice = Services.GetRequiredService<ActivityNotices>().Take(7);
        notice.Should().NotBeNull();
        notice!.Kind.Should().Be("warning");
        notice.Message.Should().Contain("Saved as a draft").And.Contain(Refusal);
        _recorder.UserLabelRequests.Should().BeEmpty();
    }

    // ---- one action at a time ----

    [Fact]
    public async Task NewActivity_WhileASaveRuns_TheOtherButtonIsDisabled_AndAnotherClickSendsNothing()
    {
        // A double-click used to file two drafts; since T102 two refusals at once would also each remount the form and
        // race their option loads. The button pressed is not disabled (T234): it has the focus, and a browser drops the
        // focus of a button it disables, to the page, where a refused create, which keeps the page, would leave it.
        SignIn("trainee-1");
        var pending = new TaskCompletionSource<ActivityDto>(TaskCreationOptions.RunContinuationsAsynchronously);
        var sender = new NewSender { PendingCreate = pending.Task };
        var cut = SelectTheType(sender);

        var saving = FindButton(cut, "Save draft").ClickAsync(new MouseEventArgs());
        cut.WaitForAssertion(() => FindButton(cut, "Submit").HasAttribute("disabled").Should().BeTrue());
        FindButton(cut, "Save draft").HasAttribute("disabled").Should().BeFalse("it has the focus (T234)");
        FindButton(cut, "Save draft").GetAttribute("aria-disabled").Should().Be("true", "a second press does nothing (T234 review)");
        FindButton(cut, "Submit").HasAttribute("aria-disabled").Should().BeFalse("it is disabled outright");

        // Dispatched at both buttons, as a second click would be, on Submit one that beat the re-render. Not awaited
        // until the first has finished: an unguarded handler would wait on the same pending create.
        var again = FindButton(cut, "Save draft").ClickAsync(new MouseEventArgs());
        var submitToo = FindButton(cut, "Submit").ClickAsync(new MouseEventArgs());

        pending.SetResult(Detail(institutionId: 7).Activity);
        await Task.WhenAll(saving, again, submitToo);

        sender.Creates.Should().ContainSingle("the page carries out one action at a time");
        sender.Transitions.Should().BeEmpty();

        // Since T127 a successful create leaves for the activity, and the page stays busy until the router replaces it:
        // a press in between would create again. Submit stays disabled, and Save draft, which has the focus, sends
        // nothing.
        Services.GetRequiredService<FakeNavigationManager>().History.Should().ContainSingle()
            .Which.Uri.Should().Be("/activities/7");
        FindButton(cut, "Submit").HasAttribute("disabled").Should().BeTrue();
        await FindButton(cut, "Save draft").ClickAsync(new MouseEventArgs());
        sender.Creates.Should().ContainSingle();
    }

    [Fact]
    public async Task ActivityView_WhileATransitionRuns_TheActionsAreDisabled_AndAnotherClickSendsNothing()
    {
        SignIn("trainee-1");
        var pending = new TaskCompletionSource<ActivityDto>(TaskCreationOptions.RunContinuationsAsynchronously);
        var sender = new ViewSender(Detail(institutionId: 7)) { PendingTransition = pending.Task };
        var cut = RenderActivityView(sender);

        var submitting = FindButton(cut, "Submit").ClickAsync(new MouseEventArgs());
        cut.WaitForAssertion(() => FindButton(cut, "Submit").HasAttribute("disabled").Should().BeTrue());

        var again = FindButton(cut, "Submit").ClickAsync(new MouseEventArgs());

        pending.SetResult(Detail(institutionId: 7).Activity);
        await Task.WhenAll(submitting, again);

        sender.Transitions.Should().ContainSingle("the page carries out one action at a time");
        cut.WaitForAssertion(() => FindButton(cut, "Submit").HasAttribute("disabled").Should().BeFalse(), AsyncWorkTimeout);
    }

    // ---- helpers ----

    /// <summary>
    /// What the real directory answers once the server has refused <c>assessor-gone</c>: they are no longer on the list.
    /// Set before the refused action, so the remounted form's fresh load is the first to see it.
    /// </summary>
    private void TheRefusedPersonFallsOffTheList()
        => _recorder.Nominees = [new ActivityCatalogueOption("assessor-1", "Dr One")];

    /// <summary>
    /// The working copy still names the refused person, and the next action would send them, so the select shows them,
    /// labelled on the page rather than looked up.
    /// </summary>
    private static void ShowsTheRefusedChoiceAsNotAvailable<TComponent>(IRenderedComponent<TComponent> cut)
        where TComponent : IComponent
    {
        var selected = cut.FindAll("#assessor_user_id option").Single(option => option.HasAttribute("selected"));
        selected.GetAttribute("value").Should().Be("assessor-gone");
        selected.TextContent.Trim().Should().Be("Not available (choose someone else)");
    }

    private static IElement FindButton<TComponent>(IRenderedComponent<TComponent> cut, string label)
        where TComponent : IComponent
        => cut.FindAll("button").First(button => button.TextContent.Trim() == label);

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
        cut.WaitForState(() => cut.Markup.Contains("Activity details"));

        return cut;
    }

    private IRenderedComponent<NewActivity> SelectTheType(NewSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<NewActivity>();
        cut.WaitForState(() => cut.FindAll("#activity-type option").Count > 1);

        cut.Find("#activity-type").Change("2");
        cut.WaitForState(() => cut.FindAll("#assessor_user_id").Count == 1);

        return cut;
    }

    private static void ClickButton(IRenderedComponent<ActivityView> cut, string label)
        => cut.FindAll("button").First(button => button.TextContent.Trim() == label).Click();

    private static void ClickButton(IRenderedComponent<NewActivity> cut, string label)
        => cut.FindAll("button").First(button => button.TextContent.Trim() == label).Click();

    private static ActivityDetailDto Detail(
        int? institutionId,
        IReadOnlyList<string>? editable = null,
        IReadOnlyList<ActivityActionDto>? actions = null,
        string dataJson = """{"assessor_user_id":"assessor-1"}""")
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
            institutionId,
            "trainee-1",
            "draft",
            "Draft",
            dataJson,
            null,
            null,
            new DateOnly(2026, 9, 24),
            true,
            new DateTime(2026, 9, 24, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 9, 24, 8, 0, 0, DateTimeKind.Utc),
            []);

        return new ActivityDetailDto(
            activity,
            editable ?? ["assessor_user_id"],
            actions ?? [new ActivityActionDto("submit", false)]);
    }

    private sealed class ViewSender : IScopedSender
    {
        private readonly ActivityDetailDto _detail;

        public ViewSender(ActivityDetailDto detail)
        {
            _detail = detail;
        }

        public List<TransitionActivityCommand> Transitions { get; } = [];

        public Exception? TransitionFailure { get; set; }

        /// <summary>When set, every transition answers with this task, so a test can hold the page mid-action.</summary>
        public Task<ActivityDto>? PendingTransition { get; init; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case GetActivityByIdQuery:
                    return Task.FromResult((TResponse)(object)_detail);

                case TransitionActivityCommand command:
                    Transitions.Add(command);
                    if (TransitionFailure is not null)
                    {
                        throw TransitionFailure;
                    }

                    if (PendingTransition is not null)
                    {
                        return (Task<TResponse>)(object)PendingTransition;
                    }

                    return Task.FromResult((TResponse)(object)_detail.Activity);

                default:
                    throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class NewSender : IScopedSender
    {
        public List<CreateActivityCommand> Creates { get; } = [];

        public List<TransitionActivityCommand> Transitions { get; } = [];

        public Exception? CreateFailure { get; set; }

        public Exception? TransitionFailure { get; init; }

        /// <summary>When set, every create answers with this task, so a test can hold the page mid-action.</summary>
        public Task<ActivityDto>? PendingCreate { get; init; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case ListActivityTypesQuery:
                    IReadOnlyList<ActivityTypeListItemDto> types =
                        [new ActivityTypeListItemDto(2, "mini_cex_cpsa", "Mini-CEX (CPSA)", ActivityScope.Speciality, 3, 1, true)];
                    return Task.FromResult((TResponse)(object)types);

                case GetActivityTypeEditorQuery:
                    var editor = new ActivityTypeEditorDto(
                        2,
                        "mini_cex_cpsa",
                        "Mini-CEX (CPSA)",
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
                        [],
                        false,
                        [],
                        null);
                    return Task.FromResult((TResponse)(object)editor);

                case CreateActivityCommand create:
                    Creates.Add(create);
                    if (CreateFailure is not null)
                    {
                        throw CreateFailure;
                    }

                    if (PendingCreate is not null)
                    {
                        return (Task<TResponse>)(object)PendingCreate;
                    }

                    return Task.FromResult((TResponse)(object)Detail(institutionId: 7).Activity);

                case GetActivityByIdQuery:
                    // The read-back after a create: the author's moves on the new draft, as the server offers them.
                    return Task.FromResult((TResponse)(object)Detail(institutionId: 7));

                case TransitionActivityCommand transition:
                    Transitions.Add(transition);
                    if (TransitionFailure is not null)
                    {
                        throw TransitionFailure;
                    }

                    return Task.FromResult((TResponse)(object)Detail(institutionId: 7).Activity);

                default:
                    throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
