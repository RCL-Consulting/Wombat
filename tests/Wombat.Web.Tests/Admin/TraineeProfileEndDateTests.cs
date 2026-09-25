using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Trainees;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Trainees;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T209: the trainee profile page records the last day in the programme for both ways out of it. Deactivating used to
/// record no day at all, so a withdrawn trainee's portfolio read every period after they left as one they fell short in.
/// D49 reads the day: the period it falls in holds no target unless it is in that period's last month.
/// </summary>
public sealed class TraineeProfileEndDateTests : TestContext
{
    private const int ProfileId = 7;

    /// <summary>What <see cref="ElementReference" />.FocusAsync calls.</summary>
    private const string FocusIdentifier = "Blazor._internal.domWrapper.focus";

    /// <summary>
    /// 22:30 UTC on 31 March 2027, which is already 1 April in South Africa. Not near the real date, so a page that
    /// read the system clock would fail.
    /// </summary>
    private static readonly DateTimeOffset Now = new(2027, 3, 31, 22, 30, 0, TimeSpan.Zero);

    public TraineeProfileEndDateTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("instadmin@test");
        auth.SetRoles(WombatRoles.InstitutionalAdmin);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "instadmin-1"));

        JSInterop.SetupVoid("wombatDialog.showModal", _ => true).SetVoidResult();
        JSInterop.SetupVoid("wombatDialog.close", _ => true).SetVoidResult();
        Services.AddSingleton<TimeProvider>(new FixedClock(Now));
    }

    [Fact]
    public void TheLastDayField_SaysWhatEachButtonRecords_AndBothButtonsNameIt()
    {
        var cut = RenderPage(new FakeSender());

        cut.Find("label[for='programme-end-date']").TextContent.Should().Contain("Last day in the programme");
        cut.Find("#programme-end-date").GetAttribute("aria-describedby").Should().Be("programme-end-date-help");
        Text(cut.Find("#programme-end-date-help")).Should()
            .Contain("Mark complete records it as the graduation day").And
            .Contain("Deactivate records it as the day the trainee left without completing").And
            .Contain("It cannot be after today, and it cannot be changed afterwards").And
            .Contain("the period it falls in holds no target unless it is in that period's last month").And
            .Contain("encounters observed after it count towards nothing in this programme, even those already counted",
                "recording the day takes back credit given for encounters after it (T281), and the day cannot be changed");

        PageButton(cut, "Deactivate").GetAttribute("aria-describedby").Should().Be("programme-end-date-help");
        PageButton(cut, "Mark complete").GetAttribute("aria-describedby").Should().Be("programme-end-date-help");
    }

    [Fact]
    public void TheLastDay_DefaultsToTodayOnTheSouthAfricanCalendar_NotTheServersDate()
    {
        // T209 review: the default was the server's local date. At 22:30 UTC it is already tomorrow in South Africa.
        var cut = RenderPage(new FakeSender());

        cut.Find("#programme-end-date").GetAttribute("value").Should().Be("2027-04-01");
    }

    [Fact]
    public void Deactivate_AsksFirst_NamingTheDay_AndSendsNothingUntilConfirmed()
    {
        // T209 review: DESIGN.md asks a destructive action to confirm first, and the red button to be only in the dialog.
        // Deactivating cannot be undone, and the day it records cannot be changed afterwards.
        var sender = new FakeSender();
        var cut = RenderPage(sender);

        cut.Find("#programme-end-date").Change("2026-05-31");
        var button = PageButton(cut, "Deactivate");
        button.ClassList.Should().Contain(["btn", "btn-outline"]).And.NotContain("btn-danger");
        button.Click();

        JSInterop.VerifyInvoke("wombatDialog.showModal");
        sender.Commands.Should().BeEmpty("opening the dialog must not deactivate the profile");
        Text(Dialog(cut, "Deactivate this trainee profile?")).Should()
            .Contain("Deactivate Lerato Molefe's profile, with 2026-05-31 as their last day in the programme?").And
            .Contain("Encounters observed after that day will count towards nothing in this programme, including any already counted.").And
            .Contain("The day cannot be changed afterwards, and a deactivated profile cannot be made active again.");
        DialogButton(cut, "Deactivate this trainee profile?", "Deactivate").ClassList.Should().Contain("btn-danger");

        DialogButton(cut, "Deactivate this trainee profile?", "Cancel").Click();
        sender.Commands.Should().BeEmpty();
    }

    [Fact]
    public void MarkComplete_AsksFirst_NamingTheDay()
    {
        var sender = new FakeSender();
        var cut = RenderPage(sender);

        cut.Find("#programme-end-date").Change("2026-09-20");
        PageButton(cut, "Mark complete").Click();

        sender.Commands.Should().BeEmpty();
        Text(Dialog(cut, "Mark this programme complete?")).Should()
            .Contain("Record 2026-09-20 as Lerato Molefe's graduation day?").And
            .Contain("the Trainee role is removed and a graduation email is sent").And
            .Contain("Encounters observed after that day will count towards nothing in this programme, including any already counted.").And
            .Contain("this cannot be undone");
    }

    [Fact]
    public void Deactivate_SendsTheDayTyped_AndShowsItAsTheLastDay()
    {
        var sender = new FakeSender();
        var cut = RenderPage(sender);

        cut.Find("#programme-end-date").Change("2026-05-31");
        PageButton(cut, "Deactivate").Click();
        DialogButton(cut, "Deactivate this trainee profile?", "Deactivate").Click();

        var command = sender.Commands.Should().ContainSingle().Which.Should().BeOfType<DeactivateTraineeProfileCommand>().Subject;
        command.Id.Should().Be(ProfileId);
        command.DeactivatedOn.Should().Be(new DateOnly(2026, 5, 31));
        Text(cut.Find(".alert.alert-success")).Should()
            .Be("Trainee profile deactivated. Their last day in the programme is recorded as 2026-05-31.");
        Text(cut.Find("aside.detail-card")).Should().Contain("Left the programme: 2026-05-31");
        cut.FindAll("#programme-end-date").Should().BeEmpty("an inactive profile has nothing left to end");

        // Both buttons are gone, so the result takes the focus, once the dialog has closed.
        ResultTookTheFocusAfterTheDialogClosed(cut);
    }

    [Fact]
    public void MarkComplete_SendsTheSameField()
    {
        var sender = new FakeSender();
        var cut = RenderPage(sender);

        cut.Find("#programme-end-date").Change("2026-09-20");
        PageButton(cut, "Mark complete").Click();
        DialogButton(cut, "Mark this programme complete?", "Mark complete").Click();

        sender.Commands.Should().ContainSingle().Which.Should().BeOfType<CompleteTraineeProfileCommand>()
            .Which.CompletedOn.Should().Be(new DateOnly(2026, 9, 20));
        ResultTookTheFocusAfterTheDialogClosed(cut);
    }

    [Fact]
    public void ARefusal_IsNamedByTheLastDayField_AndTakesTheFocus()
    {
        // T209 review: a refused day appeared only in the alert at the top, which the field did not name.
        var sender = new FakeSender
        {
            Refusal = new InvalidOperationException("The deactivation date cannot be after today (2026-09-25).")
        };
        var cut = RenderPage(sender);

        cut.Find("#programme-end-date").GetAttribute("aria-describedby").Should().Be("programme-end-date-help");
        cut.Find("#programme-end-date").Change("2026-09-26");
        PageButton(cut, "Deactivate").Click();
        DialogButton(cut, "Deactivate this trainee profile?", "Deactivate").Click();

        cut.WaitForAssertion(() => cut.Find("#programme-end-refusal").GetAttribute("role").Should().Be("alert"));
        Text(cut.Find("#programme-end-refusal")).Should().Be("The deactivation date cannot be after today (2026-09-25).");
        cut.Find("#programme-end-date").GetAttribute("aria-describedby")
            .Should().Be("programme-end-date-help programme-end-refusal");
        Text(cut.Find("aside.detail-card")).Should().Contain("Status: Active");
        ResultTookTheFocusAfterTheDialogClosed(cut);
    }

    [Fact]
    public void ASaveRefusal_IsNotNamedByTheLastDayField()
    {
        var sender = new FakeSender { Refusal = new InvalidOperationException("The selected curriculum could not be found.") };
        var cut = RenderPage(sender);

        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Text(cut.Find(".alert.alert-danger")).Should().Be("The selected curriculum could not be found."));
        cut.Find(".alert.alert-danger").HasAttribute("id").Should().BeFalse();
        cut.Find("#programme-end-date").GetAttribute("aria-describedby").Should().Be("programme-end-date-help");
    }

    [Fact]
    public void AnInactiveProfile_ShowsTheDayItsTraineeLeft()
    {
        var cut = RenderPage(new FakeSender(deactivatedOn: new DateOnly(2026, 8, 20)));

        Text(cut.Find("aside.detail-card")).Should().Contain("Status: Inactive").And.Contain("Left the programme: 2026-08-20");
        cut.FindAll("#programme-end-date").Should().BeEmpty();
    }

    // ---- T234: Save profile keeps the keyboard focus ----

    /// <summary>
    /// T234. Save profile was disabled while it ran, and a browser drops the focus of a button it disables, to the page.
    /// A save that is done moves the focus to its result, which the region reads out.
    /// </summary>
    [Fact]
    public void ASave_MovesTheFocusToItsResult()
    {
        var sender = new FakeSender();
        var cut = RenderPage(sender);

        PageButton(cut, "Save profile").Closest("form")!.Submit();

        sender.Updates.Should().Be(1);
        Text(cut.Find(".action-result .alert.alert-success")).Should().Be("Trainee profile saved.");
        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .Should().BeOfType<ElementReference>().Which.Id.Should().Be(cut.Instance.ResultRegion.Id));
    }

    [Fact]
    public void WhileASaveRuns_SaveProfileStaysEnabled_TheEndActionsAreDisabled_AndASecondPressSendsNothing()
    {
        var sender = new FakeSender { HoldUpdate = true };
        var cut = RenderPage(sender);

        PageButton(cut, "Save profile").Closest("form")!.Submit();

        PageButton(cut, "Saving...").HasAttribute("disabled").Should().BeFalse(
            "it has the focus, and a browser drops the focus of a button it disables, to the page (T234)");
        PageButton(cut, "Saving...").GetAttribute("aria-disabled").Should().Be("true", "a second press does nothing (T234 review)");
        PageButton(cut, "Deactivate").HasAttribute("disabled").Should().BeTrue();
        PageButton(cut, "Mark complete").HasAttribute("disabled").Should().BeTrue();

        PageButton(cut, "Saving...").Closest("form")!.Submit();
        sender.Updates.Should().Be(1, "a second Enter while the save runs sends nothing");

        sender.ReleaseUpdate();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert.alert-success")).Should().Be("Trainee profile saved."));
        sender.Updates.Should().Be(1);
    }

    [Fact]
    public void ARefusedSave_LeavesTheFocusWhereItWas()
    {
        // The refusal is an alert, read at once, and Save profile is still there, enabled.
        var cut = RenderPage(new FakeSender { Refusal = new InvalidOperationException("The curriculum is not adopted.") });

        PageButton(cut, "Save profile").Closest("form")!.Submit();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert.alert-danger")).Should().Be("The curriculum is not adopted."));
        JSInterop.Invocations.Should().NotContain(invocation => invocation.Identifier == FocusIdentifier);
        PageButton(cut, "Save profile").HasAttribute("disabled").Should().BeFalse();
    }

    private IRenderedComponent<TraineeProfileEdit> RenderPage(FakeSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);

        // The page reads ?id= through SupplyParameterFromQuery, which bUnit fills from the navigation manager's URI.
        var navigation = Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo(navigation.GetUriWithQueryParameter("id", ProfileId));

        var cut = RenderComponent<TraineeProfileEdit>();
        cut.WaitForState(() => cut.FindAll("aside.detail-card").Count > 0);

        return cut;
    }

    private static IElement PageButton(IRenderedFragment cut, string label)
        => cut.FindAll("form button").Single(button => Text(button) == label);

    private static IElement Dialog(IRenderedFragment cut, string title)
        => cut.FindAll("dialog").Single(dialog => Text(dialog.QuerySelector("h2")!) == title);

    private static IElement DialogButton(IRenderedFragment cut, string title, string label)
        => Dialog(cut, title).QuerySelectorAll("button").Single(button => Text(button) == label);

    private void ResultTookTheFocusAfterTheDialogClosed(IRenderedComponent<TraineeProfileEdit> cut)
    {
        // bUnit on .NET 10 leaves an element's blazor:elementReference empty in the markup, so the focused reference is
        // matched to the page's own result region, the one element that captures it.
        cut.Find(".action-result").GetAttribute("tabindex").Should().Be("-1");
        cut.WaitForAssertion(() => JSInterop.VerifyFocusAsyncInvoke().Arguments[0]
            .Should().BeOfType<ElementReference>().Which.Id.Should().Be(cut.Instance.ResultRegion.Id));

        // While the modal is open, nothing outside it can take the focus.
        var calls = JSInterop.Invocations.Select(invocation => invocation.Identifier).ToList();
        calls.IndexOf("wombatDialog.close").Should().BeGreaterThan(-1)
            .And.BeLessThan(calls.IndexOf(FocusIdentifier), "the dialog closes before the result takes the focus");
    }

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private sealed class FakeSender : IScopedSender
    {
        private readonly DateOnly? _deactivatedOn;

        public FakeSender(DateOnly? deactivatedOn = null)
        {
            _deactivatedOn = deactivatedOn;
        }

        private TaskCompletionSource? _heldUpdate;

        public List<IRequest> Commands { get; } = [];

        /// <summary>Thrown by every command, as a refusing handler would.</summary>
        public Exception? Refusal { get; init; }

        /// <summary>A save answers only once <see cref="ReleaseUpdate" /> is called: the window a second press falls into.</summary>
        public bool HoldUpdate { get; init; }

        /// <summary>How many saves the page sent.</summary>
        public int Updates { get; private set; }

        public void ReleaseUpdate() => (_heldUpdate ?? throw new InvalidOperationException("No save was sent.")).SetResult();

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is UpdateTraineeProfileCommand)
            {
                Updates++;
                if (Refusal is not null)
                {
                    return Task.FromException<TResponse>(Refusal);
                }

                return SaveAsync<TResponse>();
            }

            object response = request switch
            {
                GetCurriculaListQuery => new[]
                {
                    new CurriculumDto(
                        1, 1, 1, "Paediatrics", "General Paediatrics", "CMSA", "CPSA Paediatrics", "11.1",
                        new DateOnly(2025, 1, 1), null, IsActive: true, CanEditInPlace: false, [], SubSpecialityDefaultScaleId: null)
                },
                GetTraineeProfileByIdQuery query when query.Id == ProfileId => Profile(),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return Task.FromResult((TResponse)response);
        }

        private async Task<TResponse> SaveAsync<TResponse>()
        {
            if (HoldUpdate)
            {
                _heldUpdate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                await _heldUpdate.Task;
            }

            return (TResponse)(object)Profile();
        }

        private TraineeProfileDto Profile() => new(
            ProfileId, "trainee-1", "trainee@wombat.local", "Lerato", "Molefe",
            1, "CPSA Paediatrics", "11.1", 1, "Paediatrics", 1, "General Paediatrics",
            new DateOnly(2025, 1, 1), new DateOnly(2029, 1, 1),
            IsActive: _deactivatedOn is null,
            CompletedOn: null,
            DeactivatedOn: _deactivatedOn);

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
        {
            Commands.Add(request);
            return Refusal is null ? Task.CompletedTask : Task.FromException(Refusal);
        }
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
