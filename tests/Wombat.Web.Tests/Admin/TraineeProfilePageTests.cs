using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Trainees;
using Wombat.Application.Features.Users;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Trainees;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T379 (flow 13; boards P01–P25): a trainee's profile is "‹name›'s programme". A running programme's End this programme
/// checks the last day when a trigger is pressed, before any dialog (Q21, S19); each dialog names the day, the address and
/// what the end would take back (M2, F8); the result is built from what the command took back (F2). An ended record shows
/// the outcome first and the account it belongs to now (F5). An unknown or out-of-scope id is "Profile unavailable" (S4).
/// T209, T234, T304 and T305's rules are kept.
/// </summary>
public sealed class TraineeProfilePageTests : TestContext
{
    private const int ProfileId = 2;

    /// <summary>
    /// 22:30 UTC on 25 October 2026, which is already 26 October in South Africa: a page that read the server's date would
    /// default the last day to the 25th.
    /// </summary>
    private static readonly DateTimeOffset Now = new(2026, 10, 25, 22, 30, 0, TimeSpan.Zero);

    private static readonly DateOnly Today = new(2026, 10, 26);

    private readonly TestAuthorizationContext _auth;
    private readonly FixedClock _clock = new(Now);

    public TraineeProfilePageTests()
    {
        _auth = this.AddTestAuthorization();
        _auth.SetAuthorized("mbatha@test");
        _auth.SetRoles(WombatRoles.InstitutionalAdmin);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "mbatha"));

        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<TimeProvider>(_clock);
    }

    // ── A running programme (P01) ──────────────────────────────────────────────

    [Fact]
    public void ARunningProgramme_IsTitledByIt_WithTheExportInTheHeader_AndNoBackToTrainees()
    {
        var cut = Render(new FakeSender());

        Text(cut.Find("h1")).Should().Be("Lerato Molefe's programme");
        Text(cut.Find(".page-subtitle")).Should().Be("Update curriculum and completion details for this trainee.");
        var export = cut.Find(".header-container .actions-cell a");
        export.GetAttribute("href").Should().Be("/portfolio/export/molefe");
        Text(export).Should().Be("Export Lerato Molefe's portfolio");
        export.ClassList.Should().Contain("btn-outline");
        cut.FindAll("a").Should().NotContain(link => Text(link) == "Back to trainees");
    }

    [Fact]
    public void TheExport_IsNotOffered_WhereTheCommandWouldRefuseIt()
    {
        var cut = Render(new FakeSender { MayExport = false });

        cut.FindAll(".header-container .actions-cell").Should().BeEmpty();
        cut.Markup.Should().NotContain("/portfolio/export/");
    }

    [Fact]
    public void TheSummary_IsStatusAndEmail_WithNoNameRow_AndActiveAPlainWord()
    {
        var cut = Render(new FakeSender());

        var summary = cut.Find("dl.detail-list[aria-label='Trainee summary']");
        summary.QuerySelectorAll("dt").Select(Text).Should().Equal("Status", "Email");
        Text(summary.QuerySelectorAll("dd")[0]).Should().Be("Active");
        summary.QuerySelectorAll("dd")[0].QuerySelector(".badge").Should().BeNull();
        summary.QuerySelectorAll("dd")[1].InnerHtml.Should().Be("molefe@<wbr>kgk.wombat.local");
        cut.FindAll("h2, h3").Select(Text).Should().NotContain("Trainee summary");
    }

    [Fact]
    public void EndThisProgramme_NamesTheLastDecision_DefaultsTheDayToTheSouthAfricanToday_AndDescribesEachTrigger()
    {
        var cut = Render(new FakeSender());

        var section = EndSection(cut);
        section.QuerySelector("h2")!.HasAttribute("tabindex").Should().BeFalse("N14");
        section.QuerySelectorAll("form").Should().BeEmpty("A2: the section itself, its triggers buttons");
        Text(section.QuerySelector("p")!).Should().Be(
            "The last committee decision: Graduate (programme complete), Pre-graduation review, ratified 2026-10-26.");

        var day = cut.Find("#programme-end-date");
        day.GetAttribute("value").Should().Be("2026-10-26");
        day.GetAttribute("aria-describedby").Should().Be("programme-end-date-help");
        Text(cut.Find("#programme-end-date-help")).Should().StartWith("The graduation day, or the last day of a registrar");

        var complete = cut.Find("#end-mark-complete");
        Text(complete).Should().Be("Mark complete…");
        complete.GetAttribute("aria-describedby").Should().Be("end-mc-note");
        Text(cut.Find("#end-mc-note")).Should().Be(
            "A graduation: archives the profile, removes the Trainee role and sends Lerato Molefe a graduation email.");
        var deactivate = cut.Find("#end-deactivate");
        Text(deactivate).Should().Be("Deactivate…");
        deactivate.GetAttribute("aria-describedby").Should().Be("end-dc-note");
        Text(cut.Find("#end-dc-note")).Should().Be("A withdrawal: archives the profile and removes the Trainee role. No email is sent.");
        foreach (var trigger in new[] { complete, deactivate })
        {
            trigger.ClassList.Should().Contain("btn-outline").And.NotContain("btn-danger");
            trigger.HasAttribute("disabled").Should().BeFalse();
        }
    }

    [Fact]
    public void WithNoRatifiedDecision_TheSectionSaysNothingAboutOne()
    {
        var cut = Render(new FakeSender { LastDecision = null });

        Text(EndSection(cut)).Should().NotContain("The last committee decision");
    }

    // ── The day checked on the press (Q21, S3, S19: P02, P22, P23) ─────────────

    [Theory]
    [InlineData("2027-01-14", "#end-mark-complete", "The last day cannot be after today (2026-10-26).")]
    [InlineData("2023-01-14", "#end-mark-complete", "The last day cannot be before the programme start date (2023-01-15).")]
    [InlineData("", "#end-deactivate", "Choose the last day.")]
    public void ADayThatCannotBeRecorded_IsRefusedUnderItsField_WithNoDialog(string typed, string trigger, string refusal)
    {
        var sender = new FakeSender();
        var cut = Render(sender);

        cut.Find("#programme-end-date").Change(typed);
        cut.Find(trigger).Click();

        var message = cut.Find("#programme-end-refusal");
        Text(message).Should().Be(refusal);
        message.GetAttribute("role").Should().Be("alert");
        message.ClassList.Should().Contain("validation-message");
        var day = cut.Find("#programme-end-date");
        day.GetAttribute("aria-invalid").Should().Be("true");
        day.GetAttribute("aria-describedby").Should().Be("programme-end-refusal programme-end-date-help", "the refusal first");
        (day.GetAttribute("value") ?? string.Empty).Should().Be(typed, "the typed day is kept");
        JSInterop.Invocations.Should().NotContain(call => call.Identifier == "wombatDialog.showModal");
        JSInterop.Invocations.Should().Contain(call => call.Identifier == "wombat.focusById"
            && Equals(call.Arguments[0], "programme-end-date"));
        sender.Sent.OfType<GetTakeBackForecastQuery>().Should().BeEmpty();
        sender.Commands.Should().BeEmpty();
    }

    [Fact]
    public void APageLeftOpenPastMidnight_JudgesTheDayByTheSouthAfricanTodayAtThePress()
    {
        // B2: opened on the 26th, pressed on the 27th: the 27th is no longer after today.
        var sender = new FakeSender();
        var cut = Render(sender);

        _clock.Now = Now.AddDays(1);
        cut.Find("#programme-end-date").Change("2026-10-27");
        cut.Find("#end-mark-complete").Click();

        cut.FindAll("#programme-end-refusal").Should().BeEmpty();
        cut.WaitForAssertion(() => sender.Sent.OfType<GetTakeBackForecastQuery>().Should().ContainSingle()
            .Which.LastDay.Should().Be(new DateOnly(2026, 10, 27)));
    }

    // ── The dialogs (P03, P09, P20) ────────────────────────────────────────────

    [Fact]
    public void MarkComplete_AsksForTheDayTyped_NamingTheAddress_AndWithNothingToTakeBack_NoCount()
    {
        var sender = new FakeSender();
        var cut = Render(sender);

        cut.Find("#end-mark-complete").Click();

        sender.Sent.OfType<GetTakeBackForecastQuery>().Should().ContainSingle()
            .Which.Should().Match<GetTakeBackForecastQuery>(query => query.ProfileId == ProfileId && query.LastDay == Today);
        JSInterop.Invocations.Should().Contain(call => call.Identifier == "wombatDialog.showModal");
        var dialog = Dialog(cut, "Mark Lerato Molefe's programme complete?");
        dialog.QuerySelectorAll(".dialog-body p, p").Select(Text).Should().ContainInOrder(
            "Record 2026-10-26 as Lerato Molefe's graduation day? The profile is archived, the Trainee role is removed and a graduation email is sent to molefe@kgk.wombat.local.",
            "Encounters observed after that day will count towards nothing in this programme, including any already counted. Lerato Molefe keeps the record, read-only, and can still export the portfolio.",
            "The day cannot be changed afterwards, and this cannot be undone.");
        dialog.InnerHtml.Should().Contain("molefe@<wbr>kgk.wombat.local");
        DialogButton(cut, "Mark Lerato Molefe's programme complete?", "Mark complete").ClassList.Should().Contain("btn-primary");
        sender.Commands.Should().BeEmpty();
    }

    [Fact]
    public void Deactivate_SaysTheOneActivityItWouldTakeBack_AndConfirmsWithTheDangerButton()
    {
        var sender = new FakeSender { Forecast = new TakeBackForecastDto(1, new DateOnly(2026, 10, 25)) };
        var cut = Render(sender);

        cut.Find("#programme-end-date").Change("2026-10-24");
        cut.Find("#end-deactivate").Click();

        var dialog = Dialog(cut, "Deactivate Lerato Molefe's profile?");
        dialog.QuerySelectorAll("p").Select(Text).Should().ContainInOrder(
            "Deactivate Lerato Molefe's profile, with 2026-10-24 as their last day in the programme? The Trainee role is removed; they keep their own record, read-only.",
            "Encounters observed after that day will count towards nothing in this programme, including any already counted: 1 activity, observed 2026-10-25, will stop counting.",
            "The day cannot be changed afterwards, and a deactivated profile cannot be made active again. This cannot be undone.");
        DialogButton(cut, "Deactivate Lerato Molefe's profile?", "Deactivate").ClassList.Should().Contain("btn-danger");
    }

    [Fact]
    public void MarkComplete_RecordedAfterTheFact_SaysHowManyWouldStopCounting()
    {
        var cut = Render(new FakeSender { Forecast = new TakeBackForecastDto(3, null) });

        cut.Find("#programme-end-date").Change("2026-09-30");
        cut.Find("#end-mark-complete").Click();

        Text(Dialog(cut, "Mark Lerato Molefe's programme complete?")).Should().Contain(
            "including any already counted: 3 activities, observed after 2026-09-30, will stop counting. Lerato Molefe keeps the record");
    }

    // ── The results (P05, P11, P21) ────────────────────────────────────────────

    [Fact]
    public void MarkComplete_RecordsTheDay_AndTheEndedRecordLeadsWithTheResult_AndTheOutcome()
    {
        var sender = new FakeSender();
        var cut = Render(sender);

        cut.Find("#end-mark-complete").Click();
        DialogButton(cut, "Mark Lerato Molefe's programme complete?", "Mark complete").Click();

        sender.Commands.Should().ContainSingle().Which.Should().BeOfType<CompleteTraineeProfileCommand>()
            .Which.CompletedOn.Should().Be(Today);
        cut.WaitForAssertion(() => Text(cut.Find("h1")).Should().Be("Lerato Molefe's programme"));
        Text(cut.Find(".page-subtitle")).Should().Be("The record of a programme that has ended.");

        var page = cut.Find(".user-page");
        page.FirstElementChild!.ClassList.Should().Contain("action-result", "the result comes first, at the record's top");
        var result = page.FirstElementChild.QuerySelector(".alert-success")!;
        Text(result).Should().Be("Trainee marked complete. The Trainee role has been removed and a graduation email sent.");
        Text(result.QuerySelector("strong")!).Should().Be("Trainee marked complete.");
        page.FirstElementChild.HasAttribute("autofocus").Should().BeTrue("the result takes the focus");

        var status = cut.Find("dl.detail-list[aria-label='Trainee summary'] dd");
        Text(status).Should().Be("Completed 2026-10-26");
        status.QuerySelector(".badge")!.ClassList.Should().Contain("badge-completed");
        cut.FindAll("#programme-end-date").Should().BeEmpty();
        cut.FindAll("form").Should().BeEmpty("T305: an ended record is read, not edited");
    }

    [Theory]
    [InlineData(0, "Trainee profile deactivated. The last day in the programme is recorded as 2026-10-24, and the Trainee role has been removed.")]
    [InlineData(1, "Trainee profile deactivated. The last day in the programme is recorded as 2026-10-24, and the Trainee role has been removed. 1 activity observed after that day no longer counts.")]
    [InlineData(2, "Trainee profile deactivated. The last day in the programme is recorded as 2026-10-24, and the Trainee role has been removed. 2 activities observed after that day no longer count.")]
    public void Deactivate_SaysWhatItTookBack_FromTheCommandsCount(int takenBack, string said)
    {
        var sender = new FakeSender { TakenBack = takenBack };
        var cut = Render(sender);

        cut.Find("#programme-end-date").Change("2026-10-24");
        cut.Find("#end-deactivate").Click();
        DialogButton(cut, "Deactivate Lerato Molefe's profile?", "Deactivate").Click();

        sender.Commands.Should().ContainSingle().Which.Should().BeOfType<DeactivateTraineeProfileCommand>()
            .Which.DeactivatedOn.Should().Be(new DateOnly(2026, 10, 24));
        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert-success")).Should().Be(said));
        var status = cut.Find("dl.detail-list[aria-label='Trainee summary'] dd");
        Text(status).Should().Be("Inactive Left the programme: 2026-10-24");
        status.QuerySelector(".badge")!.ClassList.Should().Contain("badge-withdrawn").And.NotContain("badge-draft");
    }

    [Fact]
    public void TheCommandsOwnRefusal_ShowsUnderTheField_InTheSamePlace()
    {
        // P07: the command keeps its check behind the press-time one.
        var sender = new FakeSender { Refusal = new InvalidOperationException("The last day cannot be after today (2026-10-26).") };
        var cut = Render(sender);

        cut.Find("#end-mark-complete").Click();
        DialogButton(cut, "Mark Lerato Molefe's programme complete?", "Mark complete").Click();

        cut.WaitForAssertion(() => Text(cut.Find("#programme-end-refusal")).Should().Be("The last day cannot be after today (2026-10-26)."));
        cut.Find("#programme-end-date").GetAttribute("aria-invalid").Should().Be("true");
        Text(cut.Find("h1")).Should().Be("Lerato Molefe's programme");
        cut.FindAll(".action-result").Should().BeEmpty();
    }

    [Fact]
    public void AFailedEnd_SaysSoInFixedWords_AtTheSectionsTop()
    {
        // P06, T329: never the exception's text.
        var sender = new FakeSender { Refusal = new TimeoutException("db timeout at 10.0.0.4") };
        var cut = Render(sender);

        cut.Find("#end-mark-complete").Click();
        DialogButton(cut, "Mark Lerato Molefe's programme complete?", "Mark complete").Click();

        cut.WaitForAssertion(() => Text(EndSection(cut).QuerySelector(".action-result .alert-danger")!).Should().Be(
            "The programme could not be ended just now. Nothing has changed. Try again, or come back in a few minutes."));
        Text(EndSection(cut).QuerySelector(".action-result strong")!).Should().Be("The programme could not be ended just now.");
        cut.Markup.Should().NotContain("db timeout");
        cut.Find("#programme-end-date").GetAttribute("value").Should().Be("2026-10-26", "nothing has changed; the day kept");
    }

    // ── The ended record's account (F5, S5: P05, P11, P24, P25) ─────────────────

    [Theory]
    [InlineData(true, "Lerato Molefe holds no role now, and can still sign in to read the record, export the portfolio and download the STAR certificates.")]
    [InlineData(false, "Lerato Molefe holds no role now, and can still sign in to read the record and export the portfolio.")]
    public void AnEndedRecord_SaysWhatTheAccountCanStillDo(bool anyStar, string sentence)
    {
        var cut = Render(new FakeSender { Ended = true, Account = new TraineeAccountDto([], IsLocked: false, AnyStarIssued: anyStar) });

        var account = Section(cut, "The account");
        account.QuerySelectorAll("p").Select(Text).Should().Equal(sentence, "Open Lerato Molefe's user page");
        account.QuerySelector(".account-open a")!.GetAttribute("href").Should().Be("/admin/users/molefe");
    }

    [Fact]
    public void AnEndedRecord_OfSomeoneWhoHoldsARoleNow_NamesIt()
    {
        var cut = Render(new FakeSender { Ended = true, Account = new TraineeAccountDto([WombatRoles.Assessor], false, true) });

        Text(Section(cut, "The account").QuerySelector("p")!).Should().Be(
            "Lerato Molefe now holds Assessor. They can still read the record and export the portfolio.");
    }

    [Fact]
    public void AnEndedRecord_WhoseAccountIsLocked_ShowsOnlyTheUserPagesLink()
    {
        var cut = Render(new FakeSender { Ended = true, Account = new TraineeAccountDto([], IsLocked: true, false) });

        Section(cut, "The account").QuerySelectorAll("p").Select(Text).Should().Equal("Open Lerato Molefe's user page");
    }

    [Fact]
    public void AnErasedPersonsRecord_IsReadUnderThePseudonym_WithNoAccountAndNoAddress()
    {
        var cut = Render(new FakeSender { Ended = true, Erased = true });

        Text(cut.Find("h1")).Should().Be("deleted_user_1a2b3c4d's programme");
        cut.FindAll("h2").Select(Text).Should().NotContain("The account");
        cut.Find("dl.detail-list[aria-label='Trainee summary']").QuerySelectorAll("dt").Select(Text).Should().Equal("Status");
        cut.FindAll(".header-container .actions-cell").Should().BeEmpty();
    }

    [Fact]
    public void AnEndedRecord_ShowsItsDetailsReadOnly_TheStoredValues()
    {
        var cut = Render(new FakeSender { Ended = true, Account = new TraineeAccountDto([], false, false) });

        var details = Section(cut, "Profile details");
        Text(details.QuerySelector("p")!).Should().Be("This programme has ended, so its record is archived and cannot be changed.");
        details.QuerySelectorAll("dl.detail-list dt").Select(Text).Should().Equal("Curriculum", "Programme start date", "Expected completion date");
        details.QuerySelectorAll("dl.detail-list dd").Select(Text).Should().Equal("Paediatric EPA Curriculum (11.1)", "2023-01-15", "2027-01-14");
        cut.FindAll("input, select, button[type=submit]").Should().BeEmpty();
    }

    // ── Unavailable, loading and the load error (P12–P15), and the Trainee note ──

    [Fact]
    public void AnUnknownOrOutOfScopeId_IsProfileUnavailable_WithAWayOn()
    {
        var cut = Render(new FakeSender { Missing = true });

        Text(cut.Find("h1")).Should().Be("Profile unavailable");
        Text(cut.Find(".detail-card--empty .state-panel-copy")).Should().Be("This trainee profile does not exist, or you cannot open it.");
        var way = cut.Find(".detail-card--empty a");
        Text(way).Should().Be("Go to Trainees");
        way.GetAttribute("href").Should().Be("/admin/trainees");
        cut.FindAll(".header-container .actions-cell").Should().BeEmpty();
    }

    [Fact]
    public void AFailedRead_SaysSoInFixedWords_WithTryAgain()
    {
        var cut = Render(new FakeSender { LoadFailure = new TimeoutException("connection refused at 10.0.0.4") });

        Text(cut.Find("h1")).Should().Be("Trainee profile");
        Text(cut.Find(".alert-danger .alert-row-text")).Should().Be(
            "Could not load this trainee profile. Nothing has changed. Try again, or come back in a few minutes.");
        cut.FindAll("button").Should().Contain(button => Text(button) == "Try again");
        cut.Markup.Should().NotContain("connection refused");
    }

    [Fact]
    public void WhileItLoads_TheHeadingIsTraineeProfile_AndItSaysSo()
    {
        var sender = new FakeSender { Hang = true };
        Services.AddSingleton<IScopedSender>(sender);
        Navigate();

        var cut = RenderComponent<TraineeProfileEdit>();

        Text(cut.Find("h1")).Should().Be("Trainee profile");
        Text(cut.Find(".loading-line")).Should().Be("Loading this trainee profile.");
        cut.FindAll(".header-container .actions-cell").Should().BeEmpty();
    }

    [Fact]
    public void ATraineeHoldingAdmin_IsShownTheNote_AndNothingIsRead()
    {
        _auth.SetRoles(WombatRoles.InstitutionalAdmin, WombatRoles.Trainee);
        var sender = new FakeSender();
        Services.AddSingleton<IScopedSender>(sender);
        Navigate();

        var cut = RenderComponent<TraineeProfileEdit>();

        cut.Find("#trainee-profile-trainee-note").Should().NotBeNull();
        Text(cut.Find("h1")).Should().Be("Trainee profile");
        sender.Sent.Should().BeEmpty();
    }

    // ── Kept: the old admission address (T373), the picker (T304), Save profile (T234) ──

    [Fact]
    public void TheOldAdmissionAddress_LandsOnAdmit()
    {
        var sender = new FakeSender();
        Services.AddSingleton<IScopedSender>(sender);
        var navigation = Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo(navigation.GetUriWithQueryParameter("userId", "pending-1"));

        RenderComponent<TraineeProfileEdit>();

        navigation.Uri.Should().EndWith("/admin/trainees/admit?userId=pending-1");
        sender.Sent.Should().BeEmpty();
    }

    [Fact]
    public void ThePicker_ListsWhatTheChoicesQueryAnswers_WithThePinnedVersionChosen()
    {
        var sender = new FakeSender();
        var cut = Render(sender);

        cut.FindAll("#curriculum-id option").Select(option => (option.GetAttribute("value"), Text(option)))
            .Should().Equal(("1", "Paediatric EPA Curriculum (11.1)"), ("2", "Paediatric EPA Curriculum (11.2)"));
        cut.Find("#curriculum-id").GetAttribute("value").Should().Be("1");
    }

    [Fact]
    public void ASave_ShowsItsResultInProfileDetails_ItsFirstSentenceBold()
    {
        var sender = new FakeSender { Recount = new TraineeCreditRecount(CurriculumMoved: true, CompletionsChecked: 1, CompletionsCounting: 1) };
        var cut = Render(sender);

        cut.Find("section[aria-labelledby='profile-details-heading'] form").Submit();

        cut.WaitForAssertion(() => sender.Saves.Should().Be(1));
        var result = Section(cut, "Profile details").QuerySelector(".action-result .alert-success")!;
        Text(result).Should().Be("Trainee profile saved. 1 completion was checked against 11.2, and 1 counts towards it.");
        Text(result.QuerySelector("strong")!).Should().Be("Trainee profile saved.");
    }

    [Fact]
    public void WhileASaveRuns_SaveProfileKeepsItsFocus_AndASecondPressSendsNothing()
    {
        // T234: never natively disabled, so the focus stays on it; aria-disabled, so a second Enter sends nothing.
        var sender = new FakeSender { HoldSave = true };
        var cut = Render(sender);

        cut.Find("section[aria-labelledby='profile-details-heading'] form").Submit();
        var save = cut.FindAll("button[type=submit]").Single();
        Text(save).Should().Be("Saving...");
        save.HasAttribute("disabled").Should().BeFalse();
        save.GetAttribute("aria-disabled").Should().Be("true");
        cut.Find("#end-mark-complete").GetAttribute("aria-disabled").Should().Be("true");

        cut.Find("section[aria-labelledby='profile-details-heading'] form").Submit();
        sender.Saves.Should().Be(1);
        sender.ReleaseSave();
        cut.WaitForAssertion(() => Text(Section(cut, "Profile details").QuerySelector(".action-result .alert-success")!)
            .Should().Be("Trainee profile saved."));
        sender.Saves.Should().Be(1);
    }

    [Fact]
    public void ARefusedSave_SaysTheHandlersWords_InProfileDetails()
    {
        var cut = Render(new FakeSender { SaveRefusal = new InvalidOperationException("The curriculum is not adopted.") });

        cut.Find("section[aria-labelledby='profile-details-heading'] form").Submit();

        cut.WaitForAssertion(() => Text(Section(cut, "Profile details").QuerySelector(".action-result .alert-danger")!)
            .Should().Be("The curriculum is not adopted."));
    }

    [Fact]
    public void AClearedStartDate_IsRefusedUnderItsField_Focused_WithNoSummary_AndNothingSent()
    {
        // P4: each refusal under its field, as the last day's and every flow 13 form's.
        var sender = new FakeSender();
        var cut = Render(sender);

        cut.Find("#programme-start-date").Change("");
        cut.Find("section[aria-labelledby='profile-details-heading'] form").Submit();

        Text(cut.Find("#programme-start-date-error")).Should().Be("Enter the programme start.");
        var start = cut.Find("#programme-start-date");
        start.GetAttribute("aria-invalid").Should().Be("true");
        start.GetAttribute("aria-required").Should().Be("true");
        start.GetAttribute("aria-describedby").Should().Be("programme-start-date-error");
        cut.Find("#curriculum-id").HasAttribute("aria-invalid").Should().BeFalse();
        cut.FindAll(".validation-summary-errors").Should().BeEmpty();
        JSInterop.Invocations.Should().Contain(call => call.Identifier == "wombat.focusById"
            && Equals(call.Arguments[0], "programme-start-date"));
        sender.Saves.Should().Be(0);

        cut.Find("#programme-start-date").Change("2023-01-15");
        cut.FindAll("#programme-start-date-error").Should().BeEmpty("a changed field clears its refusal");
    }

    [Fact]
    public void ProfileDetails_StacksItsActions_CancelFirstInTheSource()
    {
        // A1: the primary on top at 390 is .user-page .form-actions' column-reverse over a DOM of Cancel first.
        var cut = Render(new FakeSender());

        var actions = Section(cut, "Profile details").QuerySelector(".form-actions")!.Children.Select(Text).ToList();
        actions.Should().Equal("Cancel", "Save profile");
        Section(cut, "Profile details").Closest(".user-page").Should().NotBeNull();
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private void Navigate()
    {
        var navigation = Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo(navigation.GetUriWithQueryParameter("id", ProfileId));
    }

    private IRenderedComponent<TraineeProfileEdit> Render(FakeSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);
        Navigate();
        var cut = RenderComponent<TraineeProfileEdit>();
        cut.WaitForState(() => cut.FindAll(".user-page").Count > 0 && cut.FindAll(".loading-line").Count == 0);
        return cut;
    }

    private static IElement EndSection(IRenderedFragment cut) => Section(cut, "End this programme");

    private static IElement Section(IRenderedFragment cut, string heading)
        => cut.FindAll("section.user-section").Single(section => Text(section.QuerySelector("h2")!) == heading);

    private static IElement Dialog(IRenderedFragment cut, string title)
        => cut.FindAll("dialog").Single(dialog => Text(dialog.QuerySelector("h2")!) == title);

    private static IElement DialogButton(IRenderedFragment cut, string title, string label)
        => Dialog(cut, title).QuerySelectorAll("button").Single(button => Text(button) == label);

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private sealed class FakeSender : IScopedSender
    {
        public bool MayExport { get; init; } = true;
        public bool Ended { get; init; }
        public bool Erased { get; init; }
        public bool Missing { get; init; }
        public bool Hang { get; init; }
        public Exception? LoadFailure { get; init; }
        public TraineeAccountDto? Account { get; init; }

        public LastRatifiedDecisionDto? LastDecision { get; init; } =
            new("Graduate (programme complete)", "Pre-graduation review", new DateOnly(2026, 10, 26));

        public TakeBackForecastDto Forecast { get; init; } = new(0, null);
        public int TakenBack { get; init; }
        public Exception? Refusal { get; init; }
        public Exception? SaveRefusal { get; init; }
        public TraineeCreditRecount? Recount { get; init; }

        public List<object> Sent { get; } = [];
        public List<object> Commands { get; } = [];
        public int Saves { get; private set; }
        public bool HoldSave { get; init; }
        private readonly TaskCompletionSource _held = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public void ReleaseSave() => _held.SetResult();

        private async Task<TResponse> HeldSaveAsync<TResponse>()
        {
            await _held.Task;
            return (TResponse)(object)new UpdateTraineeProfileResult(Profile(), null);
        }

        private DateOnly? _completedOn;
        private DateOnly? _deactivatedOn;

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case UpdateTraineeProfileCommand:
                    Saves++;
                    if (HoldSave)
                    {
                        return HeldSaveAsync<TResponse>();
                    }

                    return SaveRefusal is not null
                        ? Task.FromException<TResponse>(SaveRefusal)
                        : Task.FromResult((TResponse)(object)new UpdateTraineeProfileResult(Recount is { CurriculumMoved: true } ? Profile() with { CurriculumId = 2, CurriculumVersion = "11.2" } : Profile(), Recount));
                case CompleteTraineeProfileCommand complete:
                    Commands.Add(request);
                    if (Refusal is not null)
                    {
                        return Task.FromException<TResponse>(Refusal);
                    }

                    _completedOn = complete.CompletedOn;
                    return Task.FromResult((TResponse)(object)new ProgrammeEndResult(TakenBack));
                case DeactivateTraineeProfileCommand deactivate:
                    Commands.Add(request);
                    if (Refusal is not null)
                    {
                        return Task.FromException<TResponse>(Refusal);
                    }

                    _deactivatedOn = deactivate.DeactivatedOn;
                    return Task.FromResult((TResponse)(object)new ProgrammeEndResult(TakenBack));
            }

            Sent.Add(request);
            if (Hang)
            {
                return new TaskCompletionSource<TResponse>().Task;
            }

            if (LoadFailure is not null && request is GetTraineeProfileByIdQuery)
            {
                return Task.FromException<TResponse>(LoadFailure);
            }

            object? response = request switch
            {
                GetTraineeProfileByIdQuery query when query.Id == ProfileId && !Missing => Profile(),
                GetTraineeProfileByIdQuery => null,
                GetTraineeCurriculumChoicesQuery => new List<CurriculumChoiceDto>
                {
                    new(1, "Paediatric EPA Curriculum", "11.1", 1, 1),
                    new(2, "Paediatric EPA Curriculum", "11.2", 1, 1)
                },
                GetTakeBackForecastQuery => Forecast,
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return Task.FromResult((TResponse)response!);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        private TraineeProfileDto Profile()
        {
            var completedOn = _completedOn ?? (Ended && _deactivatedOn is null ? new DateOnly(2026, 10, 26) : null);
            var deactivatedOn = _deactivatedOn;
            var active = completedOn is null && deactivatedOn is null;
            return new TraineeProfileDto(
                ProfileId,
                Erased ? "deleted_user_1a2b3c4d" : "molefe",
                Erased ? string.Empty : "molefe@kgk.wombat.local",
                Erased ? "deleted_user_1a2b3c4d" : "Lerato",
                Erased ? string.Empty : "Molefe",
                1, "Paediatric EPA Curriculum", "11.1", 1, "Paediatrics", 1, "Paediatrics",
                new DateOnly(2023, 1, 15), new DateOnly(2027, 1, 14),
                IsActive: active,
                CompletedOn: completedOn,
                DeactivatedOn: deactivatedOn)
            {
                MayExport = MayExport,
                IsErased = Erased,
                Ended = active ? null : new EndedProgrammeDto((completedOn ?? deactivatedOn)!.Value, completedOn is not null) { TrainingYearAtEnd = 4 },
                LastRatifiedDecision = active ? LastDecision : null,
                Account = active || Erased ? null : Account ?? new TraineeAccountDto([], false, true)
            };
        }
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = now;

        public override DateTimeOffset GetUtcNow() => Now;
    }
}
