using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// StatePanel's Try again (T339, flow 02, B12; T329): with OnRetry, a failed load offers to read again beside its words, as
/// Home's does; without it, the failure is its words alone, as before.
/// </summary>
public sealed class StatePanelRetryTests : TestContext
{
    private const string FocusIdentifier = "Blazor._internal.domWrapper.focus";

    public StatePanelRetryTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void WithoutOnRetry_AFailureIsItsWordsAlone()
    {
        var cut = RenderComponent<StatePanel>(parameters => parameters
            .Add(panel => panel.LoadError, "Could not load.")
            .Add(panel => panel.ChildContent, (RenderFragment)(_ => { })));

        cut.Find(".alert.alert-danger").TextContent.Trim().Should().Be("Could not load.");
        cut.FindAll("button").Should().BeEmpty();
    }

    [Fact]
    public void WithOnRetry_AFailureOffersTryAgain_WhichReadsAgain_AndAFailureAgainTakesTheFocus()
    {
        var reads = 0;
        var cut = RenderComponent<StatePanel>(parameters => parameters
            .Add(panel => panel.LoadError, "Could not load.")
            .Add(panel => panel.OnRetry, () => reads++)
            .Add(panel => panel.ChildContent, (RenderFragment)(_ => { })));

        var alert = cut.Find(".action-result .alert.alert-danger .alert-row");
        alert.QuerySelector(".alert-row-text")!.TextContent.Trim().Should().Be("Could not load.");
        var retry = alert.QuerySelector("button")!;
        retry.TextContent.Trim().Should().Be("Try again");
        retry.GetAttribute("type").Should().Be("button");

        retry.Click();

        reads.Should().Be(1);
        JSInterop.Invocations.Where(invocation => invocation.Identifier == FocusIdentifier)
            .Should().ContainSingle("the read failed again: the alert, drawn again, is read again")
            .Which.Arguments[0].Should().BeOfType<ElementReference>()
            .Which.Id.Should().Be(cut.FindComponent<ActionResult>().Instance.Element.Id, "the focus lands on the alert's region");
    }

    [Fact]
    public void ARetryThatLoads_HandsTheFocusToThePage()
    {
        var focused = 0;
        string? error = "Could not load.";
        var cut = RenderComponent<StatePanel>(parameters => parameters
            .Add(panel => panel.LoadError, error)
            .Add(panel => panel.FocusAfterRetry, () => { focused++; return ValueTask.CompletedTask; })
            .Add(panel => panel.ChildContent, (RenderFragment)(builder => builder.AddMarkupContent(0, "<p>Loaded</p>"))));
        cut.SetParametersAndRender(parameters => parameters.Add(panel => panel.OnRetry, () =>
        {
            error = null;
            cut.SetParametersAndRender(next => next.Add(panel => panel.LoadError, error));
        }));

        cut.Find("button").Click();

        cut.Markup.Should().Contain("Loaded");
        focused.Should().Be(1);
    }
}
