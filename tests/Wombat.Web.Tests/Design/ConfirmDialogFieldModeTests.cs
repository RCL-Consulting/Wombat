using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// ConfirmDialog's action mode with a field (T366, flow 09, M5; build-contracts § 2): the register's Revoke puts the first
/// focus in its reason box (<see cref="ConfirmDialog.InitialFocusId" />), and a refusal the caller reports keeps the dialog
/// open with what was typed (<see cref="ConfirmDialog.OnConfirmClosesWhen" />). Both are additive: a dialog that sets
/// neither opens and closes as it always has.
/// </summary>
public sealed class ConfirmDialogFieldModeTests : TestContext
{
    public ConfirmDialogFieldModeTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public async Task WithNoInitialFocus_OpeningIsTheNativeDialogsAlone()
    {
        var cut = RenderComponent<ConfirmDialog>(parameters => parameters.Add(dialog => dialog.Title, "Withdraw?"));

        await cut.InvokeAsync(() => cut.Instance.ShowAsync().AsTask());

        Identifiers().Should().Equal("wombatDialog.showModal");
    }

    [Fact]
    public async Task WithAnInitialFocus_TheFieldTakesTheFocus_OnceTheDialogIsOpen()
    {
        var cut = RenderComponent<ConfirmDialog>(parameters => parameters
            .Add(dialog => dialog.Title, "Revoke Anele Dlamini's PAED-002 STAR?")
            .Add(dialog => dialog.BodyContent, (RenderFragment)(builder => builder.AddMarkupContent(0, "<textarea id=\"revoke-reason\"></textarea>")))
            .Add(dialog => dialog.InitialFocusId, "revoke-reason"));

        await cut.InvokeAsync(() => cut.Instance.ShowAsync().AsTask());

        Identifiers().Should().Equal("wombatDialog.showModal", PageFocus.FocusByIdIdentifier);
        JSInterop.Invocations.Last().Arguments.Should().Equal("revoke-reason");
    }

    [Fact]
    public async Task TheInitialFocus_IsAsked_OnlyOnceTheDialogHasRenderedAgain()
    {
        // T366, R5: the caller sets what its body draws and opens the dialog in one handler; the field may not be in the
        // DOM until the dialog renders, so the focus waits for that render.
        var cut = RenderComponent<ConfirmDialog>(parameters => parameters
            .Add(dialog => dialog.BodyContent, (RenderFragment)(builder => builder.AddMarkupContent(0, "<textarea id=\"revoke-reason\"></textarea>")))
            .Add(dialog => dialog.InitialFocusId, "revoke-reason"));
        var renders = cut.RenderCount;

        await cut.InvokeAsync(() => cut.Instance.ShowAsync().AsTask());

        cut.RenderCount.Should().BeGreaterThan(renders);
        Identifiers().Should().Equal("wombatDialog.showModal", PageFocus.FocusByIdIdentifier);
    }

    [Fact]
    public async Task OpeningRecordsWhoOpenedIt_ACallerGivenIdAsTheFallback()
    {
        // T366, D6: the script keeps document.activeElement, else this id (Safari and Firefox on macOS do not focus a
        // clicked button), and gives it the focus back on Cancel or Escape.
        var cut = RenderComponent<ConfirmDialog>(parameters => parameters.Add(dialog => dialog.ReturnFocusId, "lodge-appeal"));

        await cut.InvokeAsync(() => cut.Instance.ShowAsync().AsTask());

        JSInterop.Invocations.Single().Arguments.Last().Should().Be("lodge-appeal");
    }

    [Fact]
    public void Cancel_ClosesGivingTheFocusBack_AConfirmClosesLeavingItToTheCaller()
    {
        var cut = RenderComponent<ConfirmDialog>(parameters => parameters.Add(dialog => dialog.OnConfirm, () => { }));

        Cancel(cut).Click();
        JSInterop.Invocations.Last().Identifier.Should().Be("wombatDialog.close");
        JSInterop.Invocations.Last().Arguments.Last().Should().Be(true, "Cancel returns the focus to the opener (D6)");

        Confirm(cut).Click();
        JSInterop.Invocations.Last().Identifier.Should().Be("wombatDialog.close");
        JSInterop.Invocations.Last().Arguments.Last().Should().Be(false, "the caller puts the focus on its result");
    }

    [Fact]
    public void WithNoCloseTest_ConfirmRunsOnConfirm_AndCloses_AsItAlwaysHas()
    {
        var confirmed = 0;
        var cut = RenderComponent<ConfirmDialog>(parameters => parameters
            .Add(dialog => dialog.ConfirmLabel, "Withdraw")
            .Add(dialog => dialog.OnConfirm, () => confirmed++));

        Confirm(cut).Click();

        confirmed.Should().Be(1);
        Identifiers().Should().EndWith("wombatDialog.close");
    }

    [Fact]
    public void ARefusalTheCallerReports_KeepsTheDialogOpen_AndOnConfirmIsNotRun()
    {
        var confirmed = 0;
        var asked = 0;
        var cut = RenderComponent<ConfirmDialog>(parameters => parameters
            .Add(dialog => dialog.ConfirmLabel, "Revoke the STAR")
            .Add(dialog => dialog.OnConfirm, () => confirmed++)
            .Add(dialog => dialog.OnConfirmClosesWhen, () => { asked++; return Task.FromResult(false); }));

        Confirm(cut).Click();

        asked.Should().Be(1);
        confirmed.Should().Be(0, "the close test runs in OnConfirm's place");
        Identifiers().Should().NotContain("wombatDialog.close");
        Confirm(cut).HasAttribute("aria-disabled").Should().BeFalse("the refusal has answered: the button may be pressed again");
        Cancel(cut).HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void ASuccessTheCallerReports_ClosesTheDialog()
    {
        var cut = RenderComponent<ConfirmDialog>(parameters => parameters
            .Add(dialog => dialog.OnConfirmClosesWhen, () => Task.FromResult(true)));

        Confirm(cut).Click();

        Identifiers().Should().EndWith("wombatDialog.close");
    }

    [Fact]
    public void WhileTheCloseTestRuns_ConfirmIsAriaDisabled_CancelIsDisabled_AndASecondPressSendsNothing()
    {
        var answer = new TaskCompletionSource<bool>();
        var asked = 0;
        var cut = RenderComponent<ConfirmDialog>(parameters => parameters
            .Add(dialog => dialog.OnConfirmClosesWhen, () => { asked++; return answer.Task; }));

        Confirm(cut).Click();

        Confirm(cut).GetAttribute("aria-disabled").Should().Be("true");
        Confirm(cut).HasAttribute("disabled").Should().BeFalse("it keeps the focus (T234)");
        Cancel(cut).HasAttribute("disabled").Should().BeTrue("Cancel would race the action");
        Confirm(cut).Click();
        asked.Should().Be(1);

        cut.InvokeAsync(() => answer.SetResult(false));
        cut.WaitForAssertion(() => Confirm(cut).HasAttribute("aria-disabled").Should().BeFalse());
        Identifiers().Should().NotContain("wombatDialog.close");
    }

    private IEnumerable<string> Identifiers() => JSInterop.Invocations.Select(invocation => invocation.Identifier);

    private static AngleSharp.Dom.IElement Confirm(IRenderedComponent<ConfirmDialog> cut)
        => cut.FindAll(".form-actions button").Last();

    private static AngleSharp.Dom.IElement Cancel(IRenderedComponent<ConfirmDialog> cut)
        => cut.FindAll(".form-actions button").First();
}
