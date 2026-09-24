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
