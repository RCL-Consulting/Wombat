using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Epas;
using Wombat.Application.Features.Institutions;
using Wombat.Application.Features.Institutions.Queries.GetInstitutionsList;
using Wombat.Application.Features.Institutions.Queries.GetSpecialitiesList;
using Wombat.Application.Features.Institutions.Queries.GetSubSpecialitiesList;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Epas;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T158, T196: what the EPA page says about deactivating, and that both ways of deactivating ask first.
/// </summary>
/// <remarks>
/// Since T158 deactivating an EPA takes its items off every progress page and pauses their credit, in every institution
/// that uses the curriculum. The Status field must say what that means, where a screen reader on the checkbox hears it,
/// and deactivating must go through a ConfirmDialog, as DESIGN.md asks of every destructive action. Until T196 only the
/// Deactivate button did: unticking Active and pressing Save deactivated without a word, and Deactivate was still offered
/// on an EPA that was already inactive.
/// </remarks>
public sealed class EpaEditDeactivationTests : TestContext
{
    private const int EpaId = 3;

    public EpaEditDeactivationTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("admin@test");
        auth.SetRoles(WombatRoles.Administrator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "admin-1"));

        JSInterop.SetupVoid("wombatDialog.showModal", _ => true).SetVoidResult();
        JSInterop.SetupVoid("wombatDialog.close", _ => true).SetVoidResult();
    }

    [Fact]
    public void TheStatusCheckbox_IsDescribedByWhatInactiveMeans()
    {
        var cut = RenderPage(new FakeSender());

        var describedBy = cut.Find("#epa-active").GetAttribute("aria-describedby");
        describedBy.Should().Be("epa-active-help");

        var help = Text(cut.Find($"#{describedBy}"));
        help.Should().Contain("cannot be chosen for a new activity and is not a target on any progress page or dashboard")
            .And.Contain("and so is the progress it earned while it was active")
            .And.Contain("counts towards nothing until the EPA is reactivated, and reactivating it credits those activities");
        help.Should().NotContain("rebuild", "since T196 reactivating credits the paused activities; no rebuild is needed");
        help.Should().NotContain("every progress page.",
            "the rating trajectory still shows evidence recorded against an inactive EPA; only its targets go");
    }

    [Fact]
    public void Deactivate_OpensTheDialog_AndSendsNothingUntilConfirmed()
    {
        var sender = new FakeSender();
        var cut = RenderPage(sender);

        var button = PageButton(cut, "Deactivate");
        button.ClassList.Should().NotContain("btn-danger", "the red button belongs only in the dialog's footer");

        button.Click();

        JSInterop.VerifyInvoke("wombatDialog.showModal");
        sender.Commands.Should().BeEmpty("opening the dialog must not deactivate the EPA");
        Text(cut.Find("dialog")).Should().Contain("stops being a target on every progress page and dashboard")
            .And.Contain("The progress it has earned is kept.")
            .And.Contain("counts towards nothing until it is reactivated");
    }

    [Fact]
    public void ConfirmingTheDialog_DeactivatesTheEpa_AndDeactivateIsNoLongerOffered()
    {
        var sender = new FakeSender();
        var cut = RenderPage(sender);

        PageButton(cut, "Deactivate").Click();
        ConfirmButton(cut).Click();

        sender.Commands.Should().ContainSingle().Which.Should().BeOfType<DeactivateEpaCommand>()
            .Which.Id.Should().Be(EpaId);
        Text(cut.Find(".alert.alert-success")).Should().Be("EPA deactivated.");
        PageButtons(cut, "Deactivate").Should().BeEmpty("the EPA is inactive now");
    }

    /// <summary>
    /// T196 review. The Deactivate button had the focus when the dialog opened, and a deactivation removes it, so closing
    /// the dialog dropped the focus to the page. The result takes it instead, once the dialog has closed.
    /// </summary>
    [Fact]
    public void ConfirmingDeactivate_MovesTheFocusToTheResult_OnceTheDialogHasClosed()
    {
        var cut = RenderPage(new FakeSender());

        PageButton(cut, "Deactivate").Click();
        ConfirmButton(cut).Click();

        cut.Find(".action-result").GetAttribute("tabindex").Should().Be("-1");
        cut.Find(".action-result .alert.alert-success").GetAttribute("role").Should().Be("status");

        // bUnit on .NET 10 leaves an element's blazor:elementReference empty in the markup, so the focused reference is
        // matched to the page's own result region, as on the MSF campaign list.
        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .Should().BeOfType<ElementReference>().Which.Id.Should().Be(cut.Instance.ResultRegion.Id));

        var calls = JSInterop.Invocations.Select(invocation => invocation.Identifier).ToList();
        calls.IndexOf("wombatDialog.close").Should().BeGreaterThan(-1)
            .And.BeLessThan(calls.IndexOf(FocusIdentifier), "while the modal is open nothing outside it can take the focus");
    }

    [Fact]
    public void ARefusedDeactivate_LeavesTheButton_AndTheFocusWithIt()
    {
        var cut = RenderPage(new FakeSender { Refusal = new UnauthorizedAccessException("You do not have permission to deactivate this EPA.") });

        PageButton(cut, "Deactivate").Click();
        ConfirmButton(cut).Click();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert.alert-danger"))
            .Should().Be("You do not have permission to deactivate this EPA."));
        PageButtons(cut, "Deactivate").Should().ContainSingle("the EPA is still active, and the dialog hands the focus back to it");
        JSInterop.Invocations.Should().NotContain(invocation => invocation.Identifier == FocusIdentifier);
    }

    [Fact]
    public void AnInactiveEpa_IsNotOfferedDeactivate()
    {
        var cut = RenderPage(new FakeSender(isActive: false));

        cut.Find("#epa-active").HasAttribute("checked").Should().BeFalse();
        PageButtons(cut, "Deactivate").Should().BeEmpty("an inactive EPA has nothing to deactivate");
        PageButtons(cut, "Save").Should().ContainSingle("it is reactivated by ticking Active and saving");
    }

    [Fact]
    public void UntickingActiveAndSaving_AsksFirst_AndSavesOnlyOnConfirm()
    {
        var sender = new FakeSender();
        var cut = RenderPage(sender);

        cut.Find("#epa-active").Change(false);
        cut.Find("form").Submit();

        JSInterop.VerifyInvoke("wombatDialog.showModal");
        sender.Commands.Should().BeEmpty("unticking Active and saving deactivates the EPA, so it asks first");

        ConfirmButton(cut).Click();

        sender.Commands.Should().ContainSingle().Which.Should().BeOfType<UpdateEpaCommand>()
            .Which.IsActive.Should().BeFalse("confirming saves the form as it stands, Active unticked");
        Text(cut.Find(".alert.alert-success")).Should().Be("EPA saved and deactivated.");
        PageButtons(cut, "Deactivate").Should().BeEmpty();
    }

    [Fact]
    public void UntickingActiveAndSaving_ThenCancelling_SavesNothing_AndSavingAgainAsksAgain()
    {
        var sender = new FakeSender();
        var cut = RenderPage(sender);

        cut.Find("#epa-active").Change(false);
        cut.Find("form").Submit();
        cut.FindAll("dialog button").Single(button => Text(button) == "Cancel").Click();

        sender.Commands.Should().BeEmpty();
        cut.FindAll(".alert.alert-success").Should().BeEmpty();

        cut.Find("form").Submit();

        JSInterop.VerifyInvoke("wombatDialog.showModal", calledTimes: 2);
        sender.Commands.Should().BeEmpty();
    }

    [Fact]
    public void SavingAnActiveEpaThatStaysActive_DoesNotAsk()
    {
        var sender = new FakeSender();
        var cut = RenderPage(sender);

        cut.Find("form").Submit();

        JSInterop.VerifyNotInvoke("wombatDialog.showModal");
        sender.Commands.Should().ContainSingle().Which.Should().BeOfType<UpdateEpaCommand>()
            .Which.IsActive.Should().BeTrue();
        Text(cut.Find(".alert.alert-success")).Should().Be("EPA saved.");
    }

    [Theory]
    [InlineData(0, "EPA reactivated. No activity was completed against it while it was inactive, so no progress changed.")]
    [InlineData(1, "EPA reactivated. 1 activity completed against it while it was inactive now counts towards progress.")]
    [InlineData(3, "EPA reactivated. 3 activities completed against it while it was inactive now count towards progress.")]
    public void TickingActiveOnAnInactiveEpa_ReactivatesWithoutAsking_AndSaysWhatItCredited(int credited, string expected)
    {
        var sender = new FakeSender(isActive: false, completionsCredited: credited);
        var cut = RenderPage(sender);

        cut.Find("#epa-active").Change(true);
        cut.Find("form").Submit();

        JSInterop.VerifyNotInvoke("wombatDialog.showModal");
        sender.Commands.Should().ContainSingle().Which.Should().BeOfType<UpdateEpaCommand>()
            .Which.IsActive.Should().BeTrue();
        Text(cut.Find(".alert.alert-success")).Should().Be(expected);
        PageButtons(cut, "Deactivate").Should().ContainSingle("the EPA is active again");
    }

    // ---- T234: a save keeps the keyboard focus on its result ----

    /// <summary>
    /// T234. Save was disabled while it ran, and a browser drops the focus of a button it disables, to the page: the G1
    /// browser check found <c>document.activeElement</c> on BODY after "EPA saved.". A save that is done now moves the
    /// focus to its result, as a deactivation does.
    /// </summary>
    [Fact]
    public void ASave_MovesTheFocusToItsResult()
    {
        var cut = RenderPage(new FakeSender());

        cut.Find("form").Submit();

        Text(cut.Find(".action-result .alert.alert-success")).Should().Be("EPA saved.");
        cut.WaitForAssertion(() => FocusedReference().Id.Should().Be(cut.Instance.ResultRegion.Id));
    }

    [Fact]
    public void AReactivatingSave_MovesTheFocusToWhatItCredited()
    {
        var cut = RenderPage(new FakeSender(isActive: false, completionsCredited: 2));

        cut.Find("#epa-active").Change(true);
        cut.Find("form").Submit();

        Text(cut.Find(".action-result .alert.alert-success")).Should().StartWith("EPA reactivated.");
        cut.WaitForAssertion(() => FocusedReference().Id.Should().Be(cut.Instance.ResultRegion.Id));
    }

    [Fact]
    public void ASaveThatDeactivates_MovesTheFocusToItsResult_OnceTheDialogHasClosed()
    {
        var cut = RenderPage(new FakeSender());

        cut.Find("#epa-active").Change(false);
        cut.Find("form").Submit();
        ConfirmButton(cut).Click();

        Text(cut.Find(".action-result .alert.alert-success")).Should().Be("EPA saved and deactivated.");
        cut.WaitForAssertion(() => FocusedReference().Id.Should().Be(cut.Instance.ResultRegion.Id));

        var calls = JSInterop.Invocations.Select(invocation => invocation.Identifier).ToList();
        calls.IndexOf("wombatDialog.close").Should().BeGreaterThan(-1)
            .And.BeLessThan(calls.IndexOf(FocusIdentifier), "while the modal is open nothing outside it can take the focus");
    }

    [Fact]
    public void WhileASaveRuns_SaveStaysEnabled_AndASecondPressSendsNothing()
    {
        var held = new TaskCompletionSource<UpdateEpaResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        var sender = new FakeSender { HeldUpdate = held };
        var cut = RenderPage(sender);

        cut.Find("form").Submit();

        var save = PageButton(cut, "Saving...");
        save.HasAttribute("disabled").Should().BeFalse(
            "it has the focus, and a browser drops the focus of a button it disables, to the page (T234)");
        save.GetAttribute("aria-disabled").Should().Be("true", "a second press does nothing, and a screen reader hears so (T234 review)");
        PageButton(cut, "Deactivate").HasAttribute("disabled").Should().BeTrue("nothing else may be sent while a save runs");

        cut.Find("form").Submit();
        sender.Commands.Should().ContainSingle("a second Enter while the save runs sends nothing");
        FocusCalls().Should().BeEmpty("nothing has answered yet");

        held.SetResult(new UpdateEpaResult(sender.Epa, 0));

        cut.WaitForAssertion(() => FocusedReference().Id.Should().Be(cut.Instance.ResultRegion.Id));
        PageButton(cut, "Save").HasAttribute("disabled").Should().BeFalse();
        PageButton(cut, "Save").HasAttribute("aria-disabled").Should().BeFalse();
        sender.Commands.Should().ContainSingle();
    }

    [Fact]
    public void ARefusedSave_LeavesTheFocusWhereItWas()
    {
        // The refusal is an alert, read at once, and Save is still there, enabled, for the correction.
        var cut = RenderPage(new FakeSender { UpdateRefusal = new InvalidOperationException("An EPA with this code already exists.") });

        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert.alert-danger")).Should().Be("An EPA with this code already exists."));
        cut.Find(".action-result .alert.alert-danger").GetAttribute("role").Should().Be("alert");
        FocusCalls().Should().BeEmpty();
        PageButton(cut, "Save").HasAttribute("disabled").Should().BeFalse();
    }

    private ElementReference FocusedReference()
        => JSInterop.VerifyFocusAsyncInvoke().Arguments[0].Should().BeOfType<ElementReference>().Subject;

    private IReadOnlyList<JSRuntimeInvocation> FocusCalls()
        => JSInterop.Invocations.Where(invocation => invocation.Identifier == FocusIdentifier).ToList();

    /// <summary>What <see cref="ElementReference" />.FocusAsync calls.</summary>
    private const string FocusIdentifier = "Blazor._internal.domWrapper.focus";

    private IRenderedComponent<EpaEdit> RenderPage(FakeSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<EpaEdit>(parameters => parameters.Add(page => page.Id, EpaId));
        cut.WaitForState(() => cut.FindAll("#epa-active").Count > 0);

        return cut;
    }

    private static IElement PageButton(IRenderedFragment cut, string label) => PageButtons(cut, label).Single();

    private static IReadOnlyList<IElement> PageButtons(IRenderedFragment cut, string label)
        => cut.FindAll("button").Where(button => button.Closest("dialog") is null && Text(button) == label).ToList();

    private static IElement ConfirmButton(IRenderedFragment cut)
        => cut.FindAll("dialog button").Single(button => Text(button) == "Deactivate");

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private sealed class FakeSender(bool isActive = true, int completionsCredited = 0) : IScopedSender
    {
        private readonly EpaDto _epa = new(
            EpaId, 7, "General Paediatrics", "CMSA", "PAED-003", "Resuscitating a child", null, null, EpaCategory.Core,
            isActive, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        /// <summary>The EPA as stored.</summary>
        public EpaDto Epa => _epa;

        /// <summary>Every command the page sent, in order. Queries are answered and not recorded.</summary>
        public List<object> Commands { get; } = [];

        /// <summary>When set, a deactivation is refused with it.</summary>
        public Exception? Refusal { get; init; }

        /// <summary>When set, a save is refused with it.</summary>
        public Exception? UpdateRefusal { get; init; }

        /// <summary>When set, a save answers only when the test completes it: the window a second press falls into.</summary>
        public TaskCompletionSource<UpdateEpaResult>? HeldUpdate { get; init; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is UpdateEpaCommand && (HeldUpdate is not null || UpdateRefusal is not null))
            {
                Commands.Add(request);
                return UpdateRefusal is not null
                    ? Task.FromException<TResponse>(UpdateRefusal)
                    : (Task<TResponse>)(object)HeldUpdate!.Task;
            }

            object response = request switch
            {
                GetInstitutionsListQuery => Array.Empty<InstitutionDto>(),
                GetSpecialitiesListQuery => Array.Empty<SpecialityDto>(),
                GetSubSpecialitiesListQuery => Array.Empty<SubSpecialityDto>(),
                GetEpaByIdQuery query when query.Id == EpaId => _epa,
                UpdateEpaCommand command => Record(command, new UpdateEpaResult(
                    _epa with { IsActive = command.IsActive },
                    !_epa.IsActive && command.IsActive ? completionsCredited : 0)),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return Task.FromResult((TResponse)response);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
        {
            if (Refusal is not null && request is DeactivateEpaCommand)
            {
                return Task.FromException(Refusal);
            }

            Commands.Add(request);
            return Task.CompletedTask;
        }

        private object Record(object command, object response)
        {
            Commands.Add(command);
            return response;
        }
    }
}
