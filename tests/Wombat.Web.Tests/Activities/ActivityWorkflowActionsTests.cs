using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Web.Components.Shared.Activities;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// T070 step 6: <see cref="ActivityWorkflowActions" /> renders the server's decision and nothing
/// more. This fixture deliberately registers neither <c>IWorkflowEvaluator</c> nor test
/// authorization — the renders below only succeed because the component no longer injects either,
/// which is what removed the client-side re-evaluation against a synthetic <c>ActivityType</c>
/// whose <c>Scope</c> defaulted to Global (so <c>scope:</c> rules were always false in the UI).
/// </summary>
public sealed class ActivityWorkflowActionsTests : TestContext
{
    [Fact]
    public void RendersOneButtonPerServerSuppliedAction_TitleCasingTheKey()
    {
        var cut = Render([new ActivityActionDto("complete", false), new ActivityActionDto("request_changes", true)]);

        cut.FindAll(".form-actions button")
            .Select(button => button.TextContent.Trim())
            .Should().Equal("Complete", "Request Changes");
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
        cut.FindAll("#transition-note").Should().BeEmpty();
    }

    [Fact]
    public void ActionRequiringANote_CollectsItFirst()
    {
        (string TransitionKey, string? Note)? captured = null;
        var cut = Render([new ActivityActionDto("decline", true)], request => captured = request);

        cut.Find("button").Click();
        captured.Should().BeNull("the note panel must open before the transition is sent");

        cut.Find("#transition-note").Change("Not observed directly.");
        cut.FindAll("button").Last().Click();

        captured.Should().Be(("decline", "Not observed directly."));
        cut.FindAll("#transition-note").Should().BeEmpty("the panel closes once applied");
    }

    [Fact]
    public void WhileBusy_EveryButtonThatSendsATransition_IsDisabled_UntilThePageIsDone()
    {
        // The page sets Busy while it carries out an action (T102): one action at a time.
        var cut = Render([new ActivityActionDto("complete", false), new ActivityActionDto("decline", true)]);
        cut.FindAll("button").Single(button => button.TextContent.Trim() == "Decline").Click();

        cut.SetParametersAndRender(parameters => parameters.Add(component => component.Busy, true));

        SendingButtons(cut).Select(button => button.TextContent.Trim()).Should().Equal("Complete", "Decline", "Apply");
        SendingButtons(cut).Should().OnlyContain(button => button.HasAttribute("disabled"));
        cut.FindAll("button").Single(button => button.TextContent.Trim() == "Cancel")
            .HasAttribute("disabled").Should().BeFalse("closing the note panel sends nothing");

        cut.SetParametersAndRender(parameters => parameters.Add(component => component.Busy, false));

        SendingButtons(cut).Should().HaveCount(3).And.OnlyContain(button => !button.HasAttribute("disabled"));
    }

    private const string StrandedReason =
        "Needs Overall level and Strengths, which you cannot fill in here. " +
        "This activity was filed on version 1 of the form; the current version is 2.";

    [Fact]
    public void AnUnavailableAction_IsADisabledButton_DescribedByItsReason_ShownAsText()
    {
        // T107: shown, not hidden, and the reason is on the page, not in a tooltip.
        var cut = Render([new ActivityActionDto("complete", false, StrandedReason), new ActivityActionDto("decline", true)]);

        var complete = Button(cut, "Complete");
        complete.HasAttribute("disabled").Should().BeTrue();

        var reasonId = complete.GetAttribute("aria-describedby");
        reasonId.Should().NotBeNullOrWhiteSpace();
        var reason = cut.Find($"#{reasonId}");
        reason.TextContent.Should().Contain(StrandedReason);
        reason.TextContent.Should().StartWith("Complete:", "the reason names the action it belongs to");
        // Visible text, not screen-reader-only copy: neither the reason nor anything around it is hidden.
        for (var element = reason; element is not null; element = element.ParentElement)
        {
            element.ClassList.Should().NotContain("visually-hidden", "the reason is shown to everyone, not only read out");
            element.HasAttribute("hidden").Should().BeFalse();
            element.GetAttribute("aria-hidden").Should().NotBe("true");
        }

        var decline = Button(cut, "Decline");
        decline.HasAttribute("disabled").Should().BeFalse("only the action that cannot be completed is disabled");
        decline.HasAttribute("aria-describedby").Should().BeFalse();
        cut.FindAll(".workflow-action-reasons li").Should().ContainSingle();
    }

    [Fact]
    public void ClickingAnUnavailableAction_DoesNothing_EvenWhenThePageIsNotBusy()
    {
        (string TransitionKey, string? Note)? captured = null;
        var cut = Render([new ActivityActionDto("complete", false, StrandedReason)], request => captured = request);

        var complete = Button(cut, "Complete");
        complete.HasAttribute("disabled").Should().BeTrue();

        // Not just disabled in the markup: the button carries no handler, so no click can reach the callback.
        var click = () => complete.Click();
        click.Should().Throw<MissingEventHandlerException>();

        captured.Should().BeNull();
        cut.FindAll("#transition-note").Should().BeEmpty();
    }

    [Fact]
    public void WithEveryActionAvailable_NoReasonListIsRendered()
    {
        var cut = Render([new ActivityActionDto("complete", false)]);

        cut.FindAll(".workflow-action-reasons").Should().BeEmpty();
        Button(cut, "Complete").HasAttribute("aria-describedby").Should().BeFalse();
    }

    [Fact]
    public void AReasonForAKeyWithASpace_IsStillOneId_ThatTheButtonResolves()
    {
        // A builder may give a transition any key. aria-describedby is a space-separated list of ids, so a raw
        // "sign off" would describe the button by two ids that do not exist.
        var cut = Render([new ActivityActionDto("sign off", false, "Needs Countersignature, which you cannot fill in here.")]);

        var reasonId = Button(cut, "Sign Off").GetAttribute("aria-describedby");

        reasonId.Should().MatchRegex("^[A-Za-z0-9_-]+$");
        cut.FindAll("li").Should().ContainSingle(item => item.Id == reasonId)
            .Which.TextContent.Should().Contain("Needs Countersignature");
    }

    [Fact]
    public void ANotePanelForAnActionNoLongerOffered_Closes_WhenTheActionsChange()
    {
        // Open decline's note panel, then the activity moves on (another action was taken) and decline is gone. The
        // panel's Apply would send a move the page can only refuse, so it must not stay open.
        (string TransitionKey, string? Note)? captured = null;
        var cut = Render(
            [new ActivityActionDto("accept", false), new ActivityActionDto("decline", true)],
            request => captured = request);
        Button(cut, "Decline").Click();
        cut.Find("#transition-note").Change("Not my patient.");

        cut.SetParametersAndRender(parameters => parameters.Add(
            component => component.Actions,
            (IReadOnlyList<ActivityActionDto>)[new ActivityActionDto("complete", false), new ActivityActionDto("decline", true, "Needs X.")]));

        cut.FindAll("#transition-note").Should().BeEmpty("decline is no longer available");
        cut.FindAll("button").Should().NotContain(button => button.TextContent.Trim() == "Apply");
        captured.Should().BeNull();
    }

    [Fact]
    public void ANotePanelForAnActionStillOffered_KeepsItsNote_WhenThePageReRenders()
    {
        // A refused move leaves the actions as they were: the actor's note must survive for the retry.
        var cut = Render([new ActivityActionDto("decline", true)]);
        Button(cut, "Decline").Click();
        cut.Find("#transition-note").Change("Not my patient.");

        cut.SetParametersAndRender(parameters => parameters.Add(
            component => component.Actions,
            (IReadOnlyList<ActivityActionDto>)[new ActivityActionDto("decline", true)]));

        cut.Find("#transition-note").GetAttribute("value").Should().Be("Not my patient.");
    }

    private static AngleSharp.Dom.IElement Button(IRenderedComponent<ActivityWorkflowActions> cut, string label)
        => cut.FindAll("button").Single(button => button.TextContent.Trim() == label);

    private static IReadOnlyList<AngleSharp.Dom.IElement> SendingButtons(IRenderedComponent<ActivityWorkflowActions> cut)
        => cut.FindAll("button").Where(button => button.TextContent.Trim() != "Cancel").ToList();

    private IRenderedComponent<ActivityWorkflowActions> Render(
        IReadOnlyList<ActivityActionDto> actions,
        Action<(string TransitionKey, string? Note)>? onRequested = null)
        => RenderComponent<ActivityWorkflowActions>(parameters => parameters
            .Add(component => component.Actions, actions)
            .Add(component => component.OnTransitionRequested, EventCallback.Factory.Create<(string TransitionKey, string? Note)>(
                this,
                request => onRequested?.Invoke(request))));
}
