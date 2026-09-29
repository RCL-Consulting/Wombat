using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Web.Components.Shared;
using Wombat.Web.Components.Shared.Activities;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// T070 step 6: <see cref="ActivityWorkflowActions" /> renders the server's decision and nothing more. This fixture
/// registers neither <c>IWorkflowEvaluator</c> nor test authorization: the renders below succeed only because the
/// component injects neither.
/// </summary>
/// <remarks>
/// T342 (flow 03; R3-C-Activity, R3-Spec § 2): the bar is the move that leads on, filled, first; Save draft and Discard
/// changes; the other moves outlined; Cancel last and quiet. A move that needs a note opens the note panel (A6). A greyed
/// move is aria-disabled with its reason, never natively disabled (A10). The running move keeps the focus (C10).
/// </remarks>
public sealed class ActivityWorkflowActionsTests : TestContext
{
    public ActivityWorkflowActionsTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void RendersOneButtonPerServerSuppliedAction_TheMoveThatLeadsOnFirstAndFilled()
    {
        var cut = Render([new ActivityActionDto("complete", false), new ActivityActionDto("request_changes", true)]);

        var buttons = cut.FindAll(".form-actions--moves button").ToList();
        buttons.Select(button => button.TextContent.Trim()).Should().Equal("Complete", "Request Changes");
        buttons[0].ClassList.Should().Contain("btn-primary");
        buttons[1].ClassList.Should().Contain("btn-outline", "a move that needs a note is a toggle, outlined");
    }

    [Fact]
    public void AMoveThatHandsItToANamedPerson_SaysToWhom()
    {
        // E4, C3: "Submit to Fatima Khumalo" only when the move hands it to the person a filled field names.
        var cut = Render([
            new ActivityActionDto("submit", false) { HandOffFieldKey = "assessor_user_id", HandsToName = "Fatima Khumalo" },
            new ActivityActionDto("log", false) { HandsToName = "Nobody" }]);

        Labels(cut).Should().Equal("Submit to Fatima Khumalo", "Log");
    }

    [Fact]
    public void RendersNothing_WhenTheActorMayDoNothing()
    {
        Render([]).Markup.Trim().Should().BeEmpty();
    }

    [Fact]
    public void ActionWithoutANote_RaisesTheCallbackImmediately()
    {
        (string TransitionKey, string? Note)? captured = null;
        var cut = Render([new ActivityActionDto("complete", false)], request => captured = request);

        cut.Find("button").Click();

        captured.Should().Be(("complete", (string?)null));
        cut.FindAll("#note-in").Should().BeEmpty();
    }

    [Fact]
    public void ActionRequiringANote_IsAToggle_ThatOpensTheNotePanel_LabelledForItsReader()
    {
        (string TransitionKey, string? Note)? captured = null;
        var cut = Render([new ActivityActionDto("complete", false), new ActivityActionDto("decline", true)], request => captured = request);

        var decline = Button(cut, "Decline");
        decline.GetAttribute("aria-expanded").Should().Be("false");
        decline.GetAttribute("aria-controls").Should().Be(ActivityWorkflowActions.NotePanelId);

        decline.Click();
        captured.Should().BeNull("the note panel must open before the move is sent");

        Button(cut, "Decline").GetAttribute("aria-expanded").Should().Be("true");
        cut.Find($"#{ActivityWorkflowActions.NotePanelId}").Should().NotBeNull();
        cut.Find("label[for='note-in']").TextContent.Should().Contain("Reason for Sipho Ndlovu");
        cut.Find("#note-panel-title").TextContent.Should().Be("Decline this request");

        cut.Find("#note-in").Input("Not observed directly.");
        Button(cut, "Decline with this note").Click();

        captured.Should().Be(("decline", "Not observed directly."));
        cut.FindAll("#note-in").Should().ContainSingle("the panel stays open until the move has answered");
    }

    [Fact]
    public void KeepTheRequest_ClosesThePanel_SendingNothing()
    {
        (string TransitionKey, string? Note)? captured = null;
        var cut = Render([new ActivityActionDto("decline", true)], request => captured = request);

        Button(cut, "Decline").Click();
        Button(cut, "Keep the request").Click();

        cut.FindAll("#note-in").Should().BeEmpty();
        Button(cut, "Decline").GetAttribute("aria-expanded").Should().Be("false");
        captured.Should().BeNull();
    }

    [Fact]
    public void ARefusedNote_KeepsThePanelOpen_WithTheNoteAsTyped_AndSummarisesTheRefusal()
    {
        var cut = Render([new ActivityActionDto("decline", true) { ResultSentence = "Declined.", TargetStateLabel = "Declined", TargetIsFinal = true }]);
        Button(cut, "Decline").Click();
        cut.Find("#note-in").Input("  ");

        cut.SetParametersAndRender(parameters => parameters.Add(component => component.NoteRefusal, "Decline requires a note."));

        var summary = cut.Find("#note-summary");
        summary.TextContent.Should().Contain("Not declined.");
        summary.QuerySelector("a")!.GetAttribute("href").Should().Be("#note-in");
        summary.QuerySelector("a")!.TextContent.Should().Be("Reason for Sipho Ndlovu: Decline requires a note.");
        cut.Find("#note-msg").TextContent.Should().Be("Decline requires a note.");
        var note = cut.Find("#note-in");
        note.GetAttribute("aria-invalid").Should().Be("true");
        note.GetAttribute("aria-describedby").Should().Be("note-help note-msg note-summary");
        note.GetAttribute("value").Should().Be("  ");
    }

    [Fact]
    public void WhileAMoveRuns_ItsButtonKeepsTheFocus_SaysSo_AndTheOthersAreDisabled()
    {
        var cut = Render([new ActivityActionDto("submit", false), new ActivityActionDto("decline", true)], canSave: true);

        cut.SetParametersAndRender(parameters => parameters.Add(component => component.Running, "submit"));

        var running = Button(cut, "Submitting…");
        running.HasAttribute("disabled").Should().BeFalse("a browser drops the focus of a disabled button (T234)");
        running.GetAttribute("aria-disabled").Should().Be("true");
        Button(cut, "Save draft").HasAttribute("disabled").Should().BeTrue();
        Button(cut, "Decline").HasAttribute("disabled").Should().BeTrue();
        cut.Find("[role='status']").TextContent.Should().Be("Submitting.");

        cut.SetParametersAndRender(parameters => parameters.Add(component => component.Running, (string?)null));

        cut.FindAll("button").Should().OnlyContain(button => !button.HasAttribute("disabled"));
        Button(cut, "Submit").HasAttribute("aria-disabled").Should().BeFalse();
    }

    [Fact]
    public void RunningLabels_AreTheMoveInItsIngForm()
    {
        ActivityWorkflowActions.RunningLabel("Submit").Should().Be("Submitting…");
        ActivityWorkflowActions.RunningLabel("Complete").Should().Be("Completing…");
        ActivityWorkflowActions.RunningLabel("Log").Should().Be("Logging…");
        ActivityWorkflowActions.RunningLabel("Record Discussion").Should().Be("Working…");
    }

    private const string StrandedReason =
        "Needs Overall level and Strengths, which you cannot fill in here. " +
        "This activity was filed on version 1 of the form; the current version is 2.";

    [Fact]
    public void AnUnavailableAction_IsAriaDisabled_NotDisabled_DescribedByItsReason_ShownAsText()
    {
        // T107, A10: shown, not hidden; reachable, so its reason is read; the reason is on the page, not in a tooltip.
        var cut = Render([new ActivityActionDto("complete", false, StrandedReason), new ActivityActionDto("decline", true)]);

        var complete = Button(cut, "Complete");
        complete.HasAttribute("disabled").Should().BeFalse();
        complete.GetAttribute("aria-disabled").Should().Be("true");
        complete.ClassList.Should().Contain("is-unavailable");

        var reasonId = complete.GetAttribute("aria-describedby");
        reasonId.Should().NotBeNullOrWhiteSpace();
        var reason = cut.Find($"#{reasonId}");
        reason.TextContent.Should().Contain(StrandedReason);
        reason.TextContent.Should().StartWith("Complete:", "the reason names the action it belongs to");
        reason.ParentElement!.ClassList.Should().Contain("move-reasons");
        for (var element = reason; element is not null; element = element.ParentElement)
        {
            element.ClassList.Should().NotContain("visually-hidden", "the reason is shown to everyone, not only read out");
            element.HasAttribute("hidden").Should().BeFalse();
            element.GetAttribute("aria-hidden").Should().NotBe("true");
        }

        var decline = Button(cut, "Decline");
        decline.HasAttribute("aria-disabled").Should().BeFalse("only the action that cannot be completed is greyed");
        decline.HasAttribute("aria-describedby").Should().BeFalse();
        cut.FindAll(".move-reasons li").Should().ContainSingle();
    }

    [Fact]
    public void PressingAnUnavailableAction_SendsNothing()
    {
        (string TransitionKey, string? Note)? captured = null;
        var cut = Render([new ActivityActionDto("complete", false, StrandedReason)], request => captured = request);

        Button(cut, "Complete").Click();

        captured.Should().BeNull("the handler returns at once (A10)");
        cut.FindAll("#note-in").Should().BeEmpty();
    }

    [Fact]
    public void WithEveryActionAvailable_NoReasonListIsRendered()
    {
        var cut = Render([new ActivityActionDto("complete", false)]);

        cut.FindAll(".move-reasons").Should().BeEmpty();
        Button(cut, "Complete").HasAttribute("aria-describedby").Should().BeFalse();
    }

    [Fact]
    public void AReasonForAKeyWithASpace_IsStillOneId_ThatTheButtonResolves()
    {
        var cut = Render([new ActivityActionDto("sign off", false, "Needs Countersignature, which you cannot fill in here.")]);

        var reasonId = Button(cut, "Sign Off").GetAttribute("aria-describedby");

        reasonId.Should().MatchRegex("^[A-Za-z0-9_-]+$");
        cut.FindAll("li").Should().ContainSingle(item => item.Id == reasonId)
            .Which.TextContent.Should().Contain("Needs Countersignature");
    }

    [Fact]
    public void ANotePanelForAnActionNoLongerOffered_Closes_WhenTheActionsChange()
    {
        (string TransitionKey, string? Note)? captured = null;
        var cut = Render(
            [new ActivityActionDto("accept", false), new ActivityActionDto("decline", true)],
            request => captured = request);
        Button(cut, "Decline").Click();
        cut.Find("#note-in").Input("Not my patient.");

        cut.SetParametersAndRender(parameters => parameters.Add(
            component => component.Actions,
            (IReadOnlyList<ActivityActionDto>)[new ActivityActionDto("complete", false), new ActivityActionDto("decline", true, "Needs X.")]));

        cut.FindAll("#note-in").Should().BeEmpty("decline is no longer available");
        captured.Should().BeNull();
    }

    [Fact]
    public void ANotePanelForAnActionStillOffered_KeepsItsNote_WhenThePageReRenders()
    {
        // A refused move leaves the actions as they were: the actor's note must survive for the retry.
        var cut = Render([new ActivityActionDto("decline", true)]);
        Button(cut, "Decline").Click();
        cut.Find("#note-in").Input("Not my patient.");

        cut.SetParametersAndRender(parameters => parameters.Add(
            component => component.Actions,
            (IReadOnlyList<ActivityActionDto>)[new ActivityActionDto("decline", true)]));

        cut.Find("#note-in").GetAttribute("value").Should().Be("Not my patient.");
    }

    [Fact]
    public void OnADraft_TheBarIsSubmitSaveDraftDiscard_ThenCancelLastAndQuiet()
    {
        ActivityActionDto? cancelled = null;
        var cancel = new ActivityActionDto("cancel", false);
        var cut = RenderComponent<ActivityWorkflowActions>(parameters => parameters
            .Add(component => component.Actions, (IReadOnlyList<ActivityActionDto>)[new ActivityActionDto("submit", false), cancel])
            .Add(component => component.CanSaveDraft, true)
            .Add(component => component.CancelAction, cancel)
            .Add(component => component.CancelLabel, "Cancel this draft…")
            .Add(component => component.OnCancelRequested, EventCallback.Factory.Create<ActivityActionDto>(this, action => cancelled = action)));

        Labels(cut).Should().Equal("Submit", "Save draft", "Discard changes", "Cancel this draft…");
        var quiet = Button(cut, "Cancel this draft…");
        quiet.ClassList.Should().Contain("btn-quiet").And.Contain("btn-quiet--danger");

        quiet.Click();
        cancelled.Should().BeSameAs(cancel, "the page asks first, in its ConfirmDialog");
    }

    [Fact]
    public void DiscardChanges_WithNothingToDiscard_IsAriaDisabled_AndDoesNothing()
    {
        var discarded = 0;
        var cut = RenderComponent<ActivityWorkflowActions>(parameters => parameters
            .Add(component => component.Actions, (IReadOnlyList<ActivityActionDto>)[new ActivityActionDto("submit", false)])
            .Add(component => component.CanSaveDraft, true)
            .Add(component => component.OnDiscard, EventCallback.Factory.Create(this, () => discarded++)));

        var discard = cut.Find("#discard-changes");
        discard.HasAttribute("disabled").Should().BeFalse("A10: reachable, not natively disabled");
        discard.GetAttribute("aria-disabled").Should().Be("true");
        discard.Click();
        discarded.Should().Be(0);

        cut.SetParametersAndRender(parameters => parameters.Add(component => component.HasPendingChanges, true));
        cut.Find("#discard-changes").HasAttribute("aria-disabled").Should().BeFalse();
        cut.Find("#discard-changes").Click();
        discarded.Should().Be(1);
    }

    private static IReadOnlyList<string> Labels(IRenderedComponent<ActivityWorkflowActions> cut)
        => cut.FindAll(".activity-moves > .form-actions--moves > button").Select(button => button.TextContent.Trim()).ToList();

    // ---- the fix pass (T342 step 6, the build review) ----

    /// <summary>What <see cref="ElementReference" />.FocusAsync calls.</summary>
    private const string FocusIdentifier = "Blazor._internal.domWrapper.focus";

    [Fact]
    public void OpeningTheNotePanel_FocusesTheNote_AndKeepTheRequest_HandsTheFocusBackToTheToggle()
    {
        // A9 (A6, C10): the focus follows the panel in and back out.
        var cut = Render([new ActivityActionDto("decline", true)]);

        Button(cut, "Decline").Click();
        cut.WaitForAssertion(() => LastFocused().Should().Be(cut.Instance.NoteInput.Id));

        Button(cut, "Keep the request").Click();
        cut.WaitForAssertion(() => LastFocused().Should().Be(cut.Instance.ToggleOf("decline").Id));
    }

    [Fact]
    public void ARefusalOfTheNote_FocusesTheNotesSummary_WhoseLinkFocusesTheNoteInPlace()
    {
        // A9, and A1 in the panel: the summary takes the focus; its link moves it to the note by script, never by the
        // fragment, which <base href="/"> would make a navigation to Home.
        var cut = Render([new ActivityActionDto("decline", true) { ResultSentence = "Declined.", TargetStateLabel = "Declined", TargetIsFinal = true }]);
        Button(cut, "Decline").Click();

        cut.SetParametersAndRender(parameters => parameters.Add(component => component.NoteRefusal, "Decline requires a note."));
        cut.WaitForAssertion(() => LastFocused().Should().Be(cut.FindComponent<RefusalSummary>().Instance.Element.Id));

        cut.Find("#note-summary a").Click();
        JSInterop.Invocations.Where(call => call.Identifier == PageFocus.FocusByIdIdentifier)
            .Should().ContainSingle().Which.Arguments.Should().Equal(ActivityWorkflowActions.NoteInputId);
    }

    [Fact]
    public void TheRunningMovesStatusRegion_IsThereBeforeItHasWords()
    {
        // A6: a live region added with its words in it is often not read, so it is drawn empty and filled.
        var cut = Render([new ActivityActionDto("submit", false)]);

        cut.Find("[role='status']").TextContent.Should().BeEmpty();
        cut.SetParametersAndRender(parameters => parameters.Add(component => component.Running, "submit"));
        cut.Find("[role='status']").TextContent.Should().Be("Submitting.");
    }

    private string? LastFocused()
        => JSInterop.Invocations.Where(call => call.Identifier == FocusIdentifier).Select(call => call.Arguments[0]).LastOrDefault()
            is ElementReference reference
            ? reference.Id
            : null;

    private static AngleSharp.Dom.IElement Button(IRenderedComponent<ActivityWorkflowActions> cut, string label)
        => cut.FindAll("button").Single(button => button.TextContent.Trim() == label);

    private IRenderedComponent<ActivityWorkflowActions> Render(
        IReadOnlyList<ActivityActionDto> actions,
        Action<(string TransitionKey, string? Note)>? onRequested = null,
        bool canSave = false)
        => RenderComponent<ActivityWorkflowActions>(parameters => parameters
            .Add(component => component.Actions, actions)
            .Add(component => component.CanSaveDraft, canSave)
            .Add(component => component.SubjectName, "Sipho Ndlovu")
            .Add(component => component.OnTransitionRequested, EventCallback.Factory.Create<(string TransitionKey, string? Note)>(
                this,
                request => onRequested?.Invoke(request))));
}
