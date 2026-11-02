using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Commands.RebuildCurriculumProgress;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.CurriculumProgress;
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T392 (flow 15, boards R2-F01–F05, R3-F02): the rebuild page as built, in .user-page's column. A failure is said in fixed
/// words in the result's region, which takes the focus (Q16, S9); the exception is logged, never shown (T329, T272's rebuild
/// half); no Try again button, Rebuild progress stays the primary that asks first (T8). Until T392 the page printed
/// "The rebuild failed and nothing was changed: ‹the exception's text›" in an alert at its top, the focus left on the button.
/// </summary>
public sealed class CurriculumProgressRebuildPageTests : TestContext
{
    private const string FocusIdentifier = "Blazor._internal.domWrapper.focus";

    public CurriculumProgressRebuildPageTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("admin@test");
        auth.SetRoles(WombatRoles.Administrator);
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void ThePage_IsAsBuilt_InTheUserColumn_WithRebuildProgressAPrimaryThatAsks()
    {
        var cut = Render(new Sender(failure: null));

        cut.Find(".user-page.rebuild-page .form-container").Should().NotBeNull();
        var rebuild = Trigger(cut);
        rebuild.ClassList.Should().Contain("btn-primary");
        Text(rebuild).Should().Be("Rebuild progress");

        rebuild.Click();

        JSInterop.Invocations.Should().Contain(call => call.Identifier == "wombatDialog.showModal");
        var dialog = cut.Find("dialog");
        Text(dialog.QuerySelector("h2")!).Should().Be("Rebuild curriculum progress?");
        dialog.QuerySelectorAll(".form-actions button").Select(Text).Should().Equal("Cancel", "Rebuild progress");
    }

    [Fact]
    public void ADoneRebuild_SaysSo_ItsFirstSentenceBold_OverTheFiveFigures_AndTakesTheFocus()
    {
        var cut = Render(new Sender(failure: null));

        Rebuild(cut);

        cut.WaitForAssertion(() => cut.FindAll(".action-result .alert-success").Should().ContainSingle());
        Text(cut.Find(".action-result .alert-success strong")).Should().Be("Curriculum progress was rebuilt.");
        cut.FindAll(".action-result .details-list dt").Select(Text).Should().Equal(
            "Activities re-read", "Curriculum items credited", "Semester tallies written", "Stale tallies removed", "Completions re-stamped");
        FocusedTheRegion(cut);
    }

    [Fact]
    public void AFailedRebuild_IsSaidInFixedWords_InTheResultsRegion_Focused_WithNoExceptionText_AndNoTryAgain()
    {
        var cut = Render(new Sender(failure: new InvalidOperationException("Npgsql: deadlock detected on curriculum_progress")));

        Rebuild(cut);

        cut.WaitForAssertion(() => cut.FindAll(".action-result .alert-danger").Should().ContainSingle());
        var alert = cut.Find(".action-result .alert-danger");
        Text(alert).Should().Be("The rebuild failed, and nothing was changed. Try again, or come back in a few minutes.");
        Text(alert.QuerySelector("strong")!).Should().Be("The rebuild failed, and nothing was changed.");
        alert.GetAttribute("role").Should().Be("alert");
        cut.Markup.Should().NotContain("Npgsql").And.NotContain("deadlock");
        FocusedTheRegion(cut);

        // T8: no Try again button; Rebuild progress is still there, the primary, its action answered.
        cut.FindAll("button").Select(Text).Should().NotContain("Try again");
        var rebuild = Trigger(cut);
        Text(rebuild).Should().Be("Rebuild progress");
        rebuild.HasAttribute("aria-disabled").Should().BeFalse();
        cut.FindAll(".user-page > .alert, .header-container + .alert").Should().BeEmpty("the failure is no longer an alert at the top");
    }

    [Fact]
    public void ARebuildAfterAFailure_ClearsIt()
    {
        var sender = new Sender(failure: new InvalidOperationException("boom"));
        var cut = Render(sender);
        Rebuild(cut);
        cut.WaitForAssertion(() => cut.FindAll(".action-result .alert-danger").Should().ContainSingle());

        sender.Failure = null;
        Rebuild(cut);

        cut.WaitForAssertion(() => cut.FindAll(".action-result .alert-success").Should().ContainSingle());
        cut.FindAll(".action-result .alert-danger").Should().BeEmpty();
    }

    private IRenderedComponent<CurriculumProgressRebuild> Render(Sender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);
        return RenderComponent<CurriculumProgressRebuild>();
    }

    private static IElement Trigger(IRenderedFragment cut)
        => cut.FindAll(".form-container button").Single(button => button.Closest("dialog") is null);

    private static void Rebuild(IRenderedFragment cut)
    {
        Trigger(cut).Click();
        cut.FindAll("dialog button").Single(button => Text(button) == "Rebuild progress").Click();
    }

    private void FocusedTheRegion(IRenderedComponent<CurriculumProgressRebuild> cut)
    {
        var region = cut.FindComponent<ActionResult>();
        cut.WaitForAssertion(() => JSInterop.Invocations
            .Where(call => call.Identifier == FocusIdentifier)
            .Should().Contain(call => call.Arguments[0] is ElementReference
                && ((ElementReference)call.Arguments[0]!).Id == region.Instance.Element.Id));
    }

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private sealed class Sender(Exception? failure) : IScopedSender
    {
        public Exception? Failure { get; set; } = failure;

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => request switch
            {
                RebuildCurriculumProgressCommand when Failure is not null => Task.FromException<TResponse>(Failure),
                RebuildCurriculumProgressCommand => Task.FromResult((TResponse)(object)new RebuildCurriculumProgressResult(15, 14, 9, 0, 15)),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

        public Task Send(IRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
