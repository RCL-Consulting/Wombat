using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Commands.CreateActivity;
using Wombat.Application.Features.Activities.Commands.TransitionActivity;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityById;
using Wombat.Application.Features.Activities.Queries.GetActivityTypeEditor;
using Wombat.Application.Features.Activities.Queries.ListActivityTypes;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Workflow;
using Wombat.Infrastructure.Activities;
using Wombat.Web.Components.Pages.Activities;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// T127, T143, T148: what <c>/activities/new</c> does once Save draft or Submit is pressed.
/// </summary>
/// <remarks>
/// <para>
/// Once the create has succeeded the page always leaves for the new activity, and a one-shot notice goes with it
/// (<see cref="ActivityNotices" />). The filled form goes too, so the same encounter cannot be filed twice (T143). A
/// refused submit is retried on the draft's own page, whose Submit is a patched transition, so no second create is
/// possible (T127). A refused create leaves nothing behind, so the page stays and says so.
/// </para>
/// <para>
/// Submit sends the first move the server offers the author on the new activity that leads on towards a terminal state,
/// never a withdrawal. A type born in <c>requested</c> has no such move left, so the create is the filing and nothing is
/// sent (T148). The fake answers the read-back as the server does, with the real <see cref="WorkflowEvaluator" />,
/// unless a test says what the server offers.
/// </para>
/// </remarks>
public sealed class NewActivitySubmitFlowTests : TestContext
{
    private const int CreatedId = 41;

    /// <summary>A submit's refusal as <c>ActivityService</c> words it: field key, the validator's sentence, "; " between.</summary>
    private const string Refusal = "epa_id: A value is required.; assessor_user_id: A value is required.";

    private const string SchemaJson = """
        {
          "version": 1,
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "epa_id", "type": "text", "label": "EPA" },
                { "key": "assessor_user_id", "type": "text", "label": "Assessor" }
              ]
            }
          ]
        }
        """;

    /// <summary><c>mini_cex_cpsa</c>'s shape: born a draft, and <c>submit</c> sends it on to the assessor.</summary>
    private const string DraftBornWorkflow = """
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

    /// <summary>
    /// The version an activity is pinned to when a new one was published after the page loaded the type: the submit is
    /// called <c>send</c> now, and the state it reaches has a new name.
    /// </summary>
    private const string RepublishedDraftBornWorkflow = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "requested", "label": "With the assessor", "editable_by": "field:assessor_user_id" },
            { "key": "completed", "label": "Completed", "terminal": true }
          ],
          "transitions": [
            { "key": "send", "from": "draft", "to": "requested", "actor": "subject|creator", "validation": "owned" },
            { "key": "complete", "from": "requested", "to": "completed", "actor": "field:assessor_user_id", "validation": "all" }
          ]
        }
        """;

    /// <summary>
    /// The generic <c>mini_cex</c> seed: born in <c>requested</c>. Its author may take only <c>cancel</c>, into a dead end.
    /// </summary>
    private const string RequestedBornWorkflow = """
        {
          "version": 1,
          "initial_state": "requested",
          "states": [
            { "key": "requested", "label": "Requested" },
            { "key": "accepted", "label": "Accepted", "editable_by": "field:assessor_user_id" },
            { "key": "completed", "label": "Completed", "terminal": true },
            { "key": "declined", "label": "Declined" },
            { "key": "cancelled", "label": "Cancelled" }
          ],
          "transitions": [
            { "key": "accept", "from": "requested", "to": "accepted", "actor": "field:assessor_user_id", "validation": "draft" },
            { "key": "decline", "from": "requested", "to": "declined", "actor": "field:assessor_user_id", "requires_note": true, "validation": "draft" },
            { "key": "cancel", "from": ["requested", "accepted"], "to": "cancelled", "actor": "subject|field:assessor_user_id", "validation": "draft" },
            { "key": "complete", "from": "accepted", "to": "completed", "actor": "field:assessor_user_id", "validation": "all" }
          ]
        }
        """;

    /// <summary><c>procedure_log</c>'s shape: born terminal, so the create is the whole record.</summary>
    private const string TerminalBornWorkflow = """
        {
          "version": 1,
          "initial_state": "logged",
          "states": [
            { "key": "logged", "label": "Logged", "terminal": true }
          ],
          "transitions": []
        }
        """;

    /// <summary><c>msf_cpsa</c>'s shape, with the author as the actor: one move, straight into a terminal state.</summary>
    private const string StraightToTerminalWorkflow = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "recorded", "label": "Recorded", "terminal": true }
          ],
          "transitions": [
            { "key": "record", "from": "draft", "to": "recorded", "actor": "subject|creator", "validation": "owned" }
          ]
        }
        """;

    /// <summary>
    /// A builder-made withdrawal declared before the submission: <c>cancelled</c> is not a dead end, because
    /// <c>reopen</c> leads back to the draft, from which credit is still reachable. It reaches credit only through the
    /// state it left, so it is a withdrawal all the same.
    /// </summary>
    private const string ReopenableWithdrawalFirstWorkflow = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "cancelled", "label": "Cancelled" },
            { "key": "requested", "label": "Requested", "editable_by": "field:assessor_user_id" },
            { "key": "completed", "label": "Completed", "terminal": true }
          ],
          "transitions": [
            { "key": "cancel", "from": "draft", "to": "cancelled", "actor": "subject|creator", "validation": "draft" },
            { "key": "reopen", "from": "cancelled", "to": "draft", "actor": "subject|creator", "validation": "draft" },
            { "key": "submit", "from": "draft", "to": "requested", "actor": "subject|creator", "validation": "owned" },
            { "key": "complete", "from": "requested", "to": "completed", "actor": "field:assessor_user_id", "validation": "all" }
          ]
        }
        """;

    /// <summary>
    /// A builder type whose submit is limited to the subject's own institution. The page cannot judge that rule: the
    /// institution is stamped by the create, so on the page it reads false. The server, holding the stamp, allows it.
    /// </summary>
    private const string InstitutionScopedSubmitWorkflow = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "requested", "label": "Requested", "editable_by": "field:assessor_user_id" },
            { "key": "completed", "label": "Completed", "terminal": true },
            { "key": "cancelled", "label": "Cancelled" }
          ],
          "transitions": [
            { "key": "cancel", "from": "draft", "to": "cancelled", "actor": "subject|creator", "validation": "draft" },
            { "key": "submit", "from": "draft", "to": "requested", "actor": "subject+scope:institution", "validation": "owned" },
            { "key": "complete", "from": "requested", "to": "completed", "actor": "field:assessor_user_id", "validation": "all" }
          ]
        }
        """;

    public NewActivitySubmitFlowTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("trainee@test");
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "trainee-1"));

        Services.AddSingleton<IActivityReferenceDataService, StubActivityReferenceDataService>();
        Services.AddSingleton<IWorkflowEvaluator, WorkflowEvaluator>();
        Services.AddSingleton<IFieldPermissionEvaluator, FieldPermissionEvaluator>();
        Services.AddScoped<ActivityNotices>();
    }

    // ---- Save draft ----

    [Fact]
    public void SaveDraft_CreatesOnce_AndLeavesForTheDraft_SayingItWasNotSubmitted()
    {
        var sender = new FlowSender(DraftBornWorkflow);
        var cut = SelectTheType(sender);

        Click(cut, "Save draft");

        sender.Creates.Should().ContainSingle();
        sender.Transitions.Should().BeEmpty("Save draft files the draft and moves nothing");
        LeftForTheActivity();
        TakeNotice().Should().Be(new ActivityNotice("success", "Draft saved. It has not been submitted."));
    }

    [Fact]
    public void SaveDraft_PressedAgainBeforeThePageIsReplaced_CreatesNothingMore()
    {
        // T127's "Save draft twice makes two drafts". The page has navigated after the first press, but until the router
        // swaps it out its buttons are still on screen, so a second press that beats the swap must do nothing.
        var sender = new FlowSender(DraftBornWorkflow);
        var cut = SelectTheType(sender);

        Click(cut, "Save draft");

        PressEverythingAgain(cut);
        sender.Creates.Should().ContainSingle();
        sender.Transitions.Should().BeEmpty();
        Navigation.History.Should().ContainSingle();
    }

    [Fact]
    public void OnATypeBornInRequested_OnlySubmitIsOffered()
    {
        // Born in `requested`, the create is the submission: whichever button filed it, the request would be in the
        // assessor's inbox at once. "Save draft" would say otherwise, so it is not offered.
        var cut = SelectTheType(new FlowSender(RequestedBornWorkflow));

        ActionButtons(cut).Select(button => button.TextContent.Trim()).Should().Equal("Submit");
    }

    [Fact]
    public void OnATypeBornTerminal_OnlySubmitIsOffered_AndItFilesTheRecord()
    {
        // procedure_log: the create is the finished record. There is no draft to come back to.
        var sender = new FlowSender(TerminalBornWorkflow);
        var cut = SelectTheType(sender);

        ActionButtons(cut).Select(button => button.TextContent.Trim()).Should().Equal("Submit");

        Click(cut, "Submit");

        sender.Transitions.Should().BeEmpty();
        LeftForTheActivity();
        TakeNotice().Should().Be(new ActivityNotice("success", "Filed. It is now Logged."));
    }

    // ---- Submit ----

    [Fact]
    public void Submit_CreatesThenSubmits_AndLeavesForTheActivity_NamingTheStateItReached()
    {
        var sender = new FlowSender(DraftBornWorkflow);
        var cut = SelectTheType(sender);

        Click(cut, "Submit");

        sender.Creates.Should().ContainSingle();
        var submitted = sender.Transitions.Should().ContainSingle().Subject;
        submitted.ActivityId.Should().Be(CreatedId);
        submitted.TransitionKey.Should().Be("submit");
        submitted.DataPatchJson.Should().BeNull("the create already stored the form, filtered to what the author owns");
        LeftForTheActivity();
        TakeNotice().Should().Be(new ActivityNotice("success", "Submitted. It is now Requested."));

        // T143: "Submit twice files one activity". A press that beats the router's swap does nothing.
        PressEverythingAgain(cut);
        sender.Creates.Should().ContainSingle();
        sender.Transitions.Should().ContainSingle();
        Navigation.History.Should().ContainSingle();
    }

    [Fact]
    public void Submit_RefusedAtTheTransition_LeavesForTheDraft_SayingItWasSavedButNotSubmitted()
    {
        // The draft exists by the time the submit is refused. Staying here would leave it orphaned, and a second press
        // would create another (T127). The draft's own page is where the fields are fixed and Submit is pressed again.
        var sender = new FlowSender(DraftBornWorkflow) { TransitionFailure = new InvalidOperationException(Refusal) };
        var cut = SelectTheType(sender);

        Click(cut, "Submit");

        sender.Creates.Should().ContainSingle();
        sender.Transitions.Should().ContainSingle().Which.TransitionKey.Should().Be("submit");
        LeftForTheActivity();

        var notice = TakeNotice();
        notice.Should().Be(new ActivityNotice(
            "warning",
            $"Saved as a draft, but not submitted: {Refusal} Fix the fields below and submit again."));
        notice!.Message.Should().NotContain("..", "the refusal already ends as a sentence");
        cut.FindAll(".alert-danger").Should().BeEmpty("the refusal is said on the draft's page, not on the one being left");

        // "Let them retry" here would be T127's duplicate: the page is leaving, and nothing on it creates again.
        PressEverythingAgain(cut);
        sender.Creates.Should().ContainSingle();
        sender.Transitions.Should().ContainSingle();
        Navigation.History.Should().ContainSingle();
    }

    [Fact]
    public void Submit_RefusedWithAMessageThatIsNotASentence_EndsItBeforeGoingOn()
    {
        var sender = new FlowSender(DraftBornWorkflow) { TransitionFailure = new InvalidOperationException("The EPA is closed") };
        var cut = SelectTheType(sender);

        Click(cut, "Submit");

        TakeNotice()!.Message.Should().Be(
            "Saved as a draft, but not submitted: The EPA is closed. Fix the fields below and submit again.");
    }

    [Fact]
    public void Submit_RefusedAtTheCreate_StaysOnThePage_SayingNothingWasSaved_AndPostsNothing()
    {
        // Nothing exists, so there is nowhere to go. The page keeps what was typed, says so and why, and lets the author
        // retry (T127: "the error message says whether anything was saved").
        var sender = new FlowSender(DraftBornWorkflow) { CreateFailure = new InvalidOperationException("EPA: not permitted for this tool.") };
        var cut = SelectTheType(sender);

        Click(cut, "Submit");

        cut.WaitForAssertion(() => cut.Find(".alert-danger").TextContent.Trim()
            .Should().Be("Nothing was saved. EPA: not permitted for this tool."));
        sender.Transitions.Should().BeEmpty("the create was refused, so there was nothing to submit");
        sender.Reads.Should().Be(0);
        Navigation.History.Should().BeEmpty("nothing was created, so the page stays");
        TakeNotice().Should().BeNull();
        ActionButtons(cut).Should().OnlyContain(button => !button.HasAttribute("disabled"), "the author may try again");
        cut.Find("#activity-type").HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void Submit_OnATypeBornInRequested_SendsNoTransition_AndSaysItWasFiled()
    {
        // T148. The create already landed it in `requested`, the assessor's inbox. The only move its author may take is
        // `cancel`, into a dead end; the page used to send it and withdraw the encounter the moment it was filed.
        var sender = new FlowSender(RequestedBornWorkflow);
        var cut = SelectTheType(sender);

        Click(cut, "Submit");

        sender.Creates.Should().ContainSingle();
        sender.Transitions.Should().BeEmpty("the create was the submission; the next move is the assessor's");
        LeftForTheActivity();
        TakeNotice().Should().Be(new ActivityNotice("success", "Filed. It is now Requested."));
    }

    [Fact]
    public void Submit_WhenTheAuthorsFirstMoveGoesStraightToATerminalState_StillSendsIt()
    {
        // A terminal state is where credit fires, so a move straight into one leads on by definition.
        var sender = new FlowSender(StraightToTerminalWorkflow);
        var cut = SelectTheType(sender);

        Click(cut, "Submit");

        sender.Transitions.Should().ContainSingle().Which.TransitionKey.Should().Be("record");
        LeftForTheActivity();
        TakeNotice().Should().Be(new ActivityNotice("success", "Submitted. It is now Recorded."));
    }

    [Fact]
    public void Submit_SkipsAWithdrawalDeclaredFirst_EvenOneThatCanBeReopened()
    {
        // The first move the author may take is `cancel`. Its target is no dead end, since `reopen` leads back to the
        // draft, but credit is reachable from it only through the draft it left. That is a withdrawal, as the T122 gate
        // counts one, so the page passes over it to `submit`.
        var sender = new FlowSender(ReopenableWithdrawalFirstWorkflow);
        var cut = SelectTheType(sender);

        Click(cut, "Submit");

        sender.Transitions.Should().ContainSingle().Which.TransitionKey.Should().Be("submit");
        TakeNotice().Should().Be(new ActivityNotice("success", "Submitted. It is now Requested."));
    }

    [Fact]
    public void Submit_SendsTheMoveTheServerOffers_WhereThePageCannotJudgeTheRule()
    {
        // `scope:institution` reads the institution the create stamps, which the page does not know. Before the create it
        // errs towards offering less (no Save draft); after it, the server says which moves are the author's.
        var sender = new FlowSender(InstitutionScopedSubmitWorkflow) { OfferedMoves = ["cancel", "submit"] };
        var cut = SelectTheType(sender);

        ActionButtons(cut).Select(button => button.TextContent.Trim()).Should().Equal("Submit");

        Click(cut, "Submit");

        sender.Transitions.Should().ContainSingle().Which.TransitionKey.Should().Be("submit");
        TakeNotice().Should().Be(new ActivityNotice("success", "Submitted. It is now Requested."));
    }

    [Fact]
    public void Submit_NeverSendsAMoveTheServerDoesNotOffer()
    {
        // The page would have judged `submit` the author's. The server, holding the activity as built, says it is not, so
        // nothing is sent and the notice states where the activity stands.
        var sender = new FlowSender(DraftBornWorkflow) { OfferedMoves = ["cancel"] };
        var cut = SelectTheType(sender);

        Click(cut, "Submit");

        sender.Transitions.Should().BeEmpty();
        TakeNotice().Should().Be(new ActivityNotice("success", "Filed. It is now Draft."));
    }

    [Fact]
    public void Submit_JudgesTheMoveAndItsLabel_AgainstTheVersionTheActivityIsPinnedTo()
    {
        // A version published after the page loaded the type: the activity is pinned to it, and the server judges the
        // move against it. The editor's copy would name a move that no longer exists.
        var sender = new FlowSender(RepublishedDraftBornWorkflow, editorWorkflowJson: DraftBornWorkflow);
        var cut = SelectTheType(sender);

        Click(cut, "Submit");

        sender.Transitions.Should().ContainSingle().Which.TransitionKey.Should().Be("send");
        TakeNotice().Should().Be(new ActivityNotice("success", "Submitted. It is now With the assessor."));
    }

    [Fact]
    public void Submit_WhenTheNextStepCannotBeWorkedOut_LeavesForTheActivity_ClaimingNothingMore()
    {
        // The create worked; reading it back did not. The activity exists, so the page still leaves for it, but the notice
        // says neither "submitted" nor "a draft": it cannot know which.
        var sender = new FlowSender(RequestedBornWorkflow) { ReadFailure = new InvalidOperationException("The database is unavailable") };
        var cut = SelectTheType(sender);

        Click(cut, "Submit");

        sender.Transitions.Should().BeEmpty();
        LeftForTheActivity();
        TakeNotice().Should().Be(new ActivityNotice(
            "warning",
            "Saved, but the next step could not be worked out: The database is unavailable. Continue from this page."));
    }

    [Fact]
    public async Task LeavingWhileTheCreateRuns_DoesNotDragTheAuthorBack()
    {
        // The author clicks elsewhere while a slow create runs. The page is torn down; when the create returns, the
        // activity exists and the submit they asked for still goes, but the page no longer takes them to it.
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var sender = new FlowSender(DraftBornWorkflow) { CreateGate = release.Task };
        var cut = SelectTheType(sender);

        var submitting = FindButton(cut, "Submit").ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());
        cut.WaitForAssertion(() => cut.Find("#activity-type").HasAttribute("disabled").Should().BeTrue(
            "the type the outcome is judged against cannot change while it runs"));

        DisposeComponents();
        release.SetResult();
        await submitting;

        sender.Creates.Should().ContainSingle();
        sender.Transitions.Should().ContainSingle("the author pressed Submit; leaving the page does not withdraw that");
        Navigation.History.Should().BeEmpty("the author's own click is not undone");
        TakeNotice().Should().BeNull("there is no page coming to show it");
    }

    // ---- helpers ----

    private FakeNavigationManager Navigation => Services.GetRequiredService<FakeNavigationManager>();

    private ActivityNotice? TakeNotice() => Services.GetRequiredService<ActivityNotices>().Take(CreatedId);

    private void LeftForTheActivity()
    {
        var navigation = Navigation.History.Should().ContainSingle().Subject;
        navigation.Uri.Should().Be($"/activities/{CreatedId}");
        navigation.Options.ForceLoad.Should().BeFalse(
            "a forced load would start a new circuit, and the notice lives in this one's scope");
        navigation.Options.ReplaceHistoryEntry.Should().BeTrue(
            "Back from the activity must not return to an empty form, where retyping the encounter would file it again");
    }

    /// <summary>
    /// Everything on the page that could act, pressed again after a create succeeded: the buttons and the type picker are
    /// disabled until the router replaces the page, and a press that beats the re-render is ignored.
    /// </summary>
    private static void PressEverythingAgain(IRenderedComponent<NewActivity> cut)
    {
        cut.WaitForAssertion(() => ActionButtons(cut).Should().OnlyContain(button => button.HasAttribute("disabled"),
            "the page is leaving for the activity, so it offers nothing more"));
        cut.Find("#activity-type").HasAttribute("disabled").Should().BeTrue();

        foreach (var label in ActionButtons(cut).Select(button => button.TextContent.Trim()).ToList())
        {
            Click(cut, label);
        }
    }

    private IRenderedComponent<NewActivity> SelectTheType(FlowSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<NewActivity>();
        cut.WaitForState(() => cut.FindAll("#activity-type option").Count > 1);

        cut.Find("#activity-type").Change("2");
        cut.WaitForState(() => cut.FindAll("#epa_id").Count == 1);

        return cut;
    }

    private static void Click(IRenderedComponent<NewActivity> cut, string label)
        => FindButton(cut, label).Click();

    private static IElement FindButton(IRenderedComponent<NewActivity> cut, string label)
        => ActionButtons(cut).First(button => button.TextContent.Trim() == label);

    private static IReadOnlyList<IElement> ActionButtons(IRenderedComponent<NewActivity> cut)
        => cut.FindAll("button")
            .Where(button => button.TextContent.Trim() is "Save draft" or "Submit")
            .ToList();

    /// <summary>
    /// Answers the page's queries with one type, and records the commands. A create lands in the initial state of the
    /// workflow the activity is pinned to; a transition lands in its declared target; the read-back offers the moves the
    /// real evaluator allows the author there, as the server would, unless <see cref="OfferedMoves" /> says otherwise.
    /// </summary>
    private sealed class FlowSender : IScopedSender
    {
        private readonly string _pinnedWorkflowJson;
        private readonly string _editorWorkflowJson;
        private readonly Workflow _pinned;
        private string _state;

        public FlowSender(string workflowJson, string? editorWorkflowJson = null)
        {
            _pinnedWorkflowJson = workflowJson;
            _editorWorkflowJson = editorWorkflowJson ?? workflowJson;
            _pinned = WorkflowParser.Parse(workflowJson);
            _state = _pinned.InitialState;
        }

        public List<CreateActivityCommand> Creates { get; } = [];

        public List<TransitionActivityCommand> Transitions { get; } = [];

        public int Reads { get; private set; }

        public Exception? CreateFailure { get; init; }

        public Exception? TransitionFailure { get; init; }

        public Exception? ReadFailure { get; init; }

        /// <summary>When set, the create answers only once this completes, so a test can act while it runs.</summary>
        public Task? CreateGate { get; init; }

        /// <summary>What the server offers the author on the created activity, where the test says so.</summary>
        public IReadOnlyList<string>? OfferedMoves { get; init; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case ListActivityTypesQuery:
                    IReadOnlyList<ActivityTypeListItemDto> types =
                        [new ActivityTypeListItemDto(2, "mini_cex", "Mini-CEX", ActivityScope.Speciality, 3, 1, true)];
                    return Task.FromResult((TResponse)(object)types);

                case GetActivityTypeEditorQuery:
                    var editor = new ActivityTypeEditorDto(
                        2,
                        "mini_cex",
                        "Mini-CEX",
                        null,
                        ActivityScope.Speciality,
                        3,
                        true,
                        "mini_cex",
                        1,
                        false,
                        SchemaJson,
                        _editorWorkflowJson,
                        "{}",
                        "[]",
                        SchemaJson,
                        _editorWorkflowJson,
                        "{}",
                        "[]",
                        "admin-1",
                        null,
                        null,
                        []);
                    return Task.FromResult((TResponse)(object)editor);

                case CreateActivityCommand create:
                    Creates.Add(create);
                    if (CreateFailure is not null)
                    {
                        throw CreateFailure;
                    }

                    return CreateGate is null
                        ? Task.FromResult((TResponse)(object)Activity(_state))
                        : (Task<TResponse>)(object)CreatedOnceReleasedAsync(CreateGate);

                case GetActivityByIdQuery read:
                    Reads++;
                    if (ReadFailure is not null)
                    {
                        throw ReadFailure;
                    }

                    ActivityDetailDto? detail = new(Activity(_state), [], Offered(read.Principal));
                    return Task.FromResult((TResponse)(object)detail!);

                case TransitionActivityCommand transition:
                    Transitions.Add(transition);
                    if (TransitionFailure is not null)
                    {
                        throw TransitionFailure;
                    }

                    _state = _pinned.Transitions.Single(candidate => candidate.Key == transition.TransitionKey).To;
                    return Task.FromResult((TResponse)(object)Activity(_state));

                default:
                    throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        private async Task<ActivityDto> CreatedOnceReleasedAsync(Task gate)
        {
            await gate;
            return Activity(_state);
        }

        private IReadOnlyList<ActivityActionDto> Offered(ClaimsPrincipal principal)
        {
            var created = new Activity
            {
                ActivityType = new ActivityType { Scope = ActivityScope.Speciality, ScopeId = 3 },
                CurrentState = _state,
                SubjectUserId = "trainee-1",
                CreatedByUserId = "trainee-1",
                DataJson = "{}",
                InstitutionId = 7
            };

            var evaluator = new WorkflowEvaluator();
            var keys = OfferedMoves ?? _pinned.Transitions
                .Where(transition => transition.From.Contains(_state, StringComparer.Ordinal))
                .Where(transition => evaluator.Evaluate(_pinned, created, transition.Key, principal).Allowed)
                .Select(transition => transition.Key)
                .ToList();

            return keys.Select(key => new ActivityActionDto(key, false)).ToList();
        }

        private ActivityDto Activity(string state)
            => new(
                CreatedId,
                2,
                "mini_cex",
                "Mini-CEX",
                "mini_cex",
                1,
                SchemaJson,
                _pinnedWorkflowJson,
                "[]",
                """{ "counts_for": [] }""",
                "trainee-1",
                7,
                "trainee-1",
                state,
                "{}",
                null,
                null,
                new DateTime(2026, 9, 24, 8, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 9, 24, 8, 0, 0, DateTimeKind.Utc),
                []);
    }
}
