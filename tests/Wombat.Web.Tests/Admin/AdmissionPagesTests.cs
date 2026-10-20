using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Trainees;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Trainees;
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T373 (flow 11, lane A): Trainees (T01–T10) and Admit ‹name› (M01–M15), drawn from the cast on 2026-10-19: Lerato Molefe,
/// Anele Dlamini, Pieter du Plessis, Nomsa Mahlangu and Sipho Ndlovu, registered from Trainee invitations to Paediatrics /
/// Paediatrics at Kgosi Kgari Teaching Hospital.
/// </summary>
public sealed class AdmissionPagesTests : TestContext
{
    private static readonly DateOnly Today = new(2026, 10, 19);
    private const string FocusById = "wombat.focusById";

    private readonly TestAuthorizationContext _auth;

    public AdmissionPagesTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        _auth = this.AddTestAuthorization();
        SignedInAs(WombatRoles.InstitutionalAdmin);
    }

    // ── Trainees ───────────────────────────────────────────────────────────────

    [Fact]
    public void T01_FiveWaiting_EachWithItsScope_RegistrationDay_AndAdmit()
    {
        var cut = RenderTrainees(new Sender { Pending = FiveWaiting() });

        var waiting = Section(cut, "trainees-waiting-heading");
        Text(waiting.QuerySelector("h2")!).Should().Be("Waiting to be admitted 5 waiting");
        Text(waiting.QuerySelector("caption")!).Should().Be(AdmissionText.WaitingCaption);
        Headers(waiting).Should().Equal("Name", "Email", "Invited as", "Registered", "Actions");
        waiting.QuerySelector("table")!.ClassList.Should().Contain("clinic-table--stack-wide");

        var first = waiting.QuerySelector("tbody tr")!;
        Text(first.QuerySelector("th[scope=row]")!).Should().Be("Anele Dlamini");
        first.QuerySelector("td[data-label=Email]")!.InnerHtml.Should().Be("dlamini@<wbr>kgk.wombat.local");
        Text(first.QuerySelector("td[data-label='Invited as']")!).Should().Be("Paediatrics / Paediatrics");
        Text(first.QuerySelector("td[data-label=Registered]")!).Should().Be("2026-10-19");

        var admit = first.QuerySelector("a.btn")!;
        Text(admit).Should().Be("Admit…");
        admit.GetAttribute("aria-label").Should().Be("Admit Anele Dlamini");
        admit.GetAttribute("href").Should().Be("/admin/trainees/admit?userId=dlamini");
        admit.Closest("td")!.ClassList.Should().Contain("col-actions--single");

        var current = Section(cut, "trainees-current-heading");
        Text(current.QuerySelector("h2")!).Should().Be("Current trainees");
        Text(current.QuerySelector(".state-panel-title")!).Should().Be("No current trainees");
        Text(current.QuerySelector(".state-panel-copy")!).Should().Be("A registrar appears here once admitted.");
        cut.FindAll(".action-result").Should().BeEmpty("nothing was admitted");
    }

    [Fact]
    public void S5_ARegistrarInvitedToNone_ReadsADash()
    {
        var cut = RenderTrainees(new Sender { Pending = [Pending("sso", "Thabo", "Zwane") with { InvitedAs = null, RegisteredOn = null }] });

        var row = cut.Find("tbody tr");
        Text(row.QuerySelector("td[data-label='Invited as']")!).Should().Be("—");
        Text(row.QuerySelector("td[data-label=Registered]")!).Should().Be("—");
    }

    [Fact]
    public void T02_TheAdmissionsResult_IsReadAgain_AboveTheWaitingTable_AndFocused()
    {
        var sender = new Sender
        {
            Pending = FiveWaiting().Where(row => row.UserId != "molefe").ToArray(),
            Profiles = [Profile(7, "Lerato", "Molefe", new(2023, 1, 15), new(2027, 1, 14), 4)],
            Result = new AdmissionResultDto(7, "molefe", "Lerato Molefe", "Paediatric EPA Curriculum", "11.1", 4, new(2023, 1, 15), new(2027, 1, 14), false)
        };
        var cut = RenderTrainees(sender, "?admitted=7");

        var waiting = Section(cut, "trainees-waiting-heading");
        var result = waiting.QuerySelector(".action-result")!;
        Text(result).Should().Be("Lerato Molefe admitted to the Paediatric EPA Curriculum (11.1), training year 4, from 2023-01-15.");
        result.NextElementSibling!.ClassList.Should().Contain("table-container", "directly above the Waiting table");
        result.HasAttribute("autofocus").Should().BeTrue();
        Text(waiting.QuerySelector("h2")!).Should().Be("Waiting to be admitted 4 waiting");
        sender.Asked.OfType<GetAdmissionResultQuery>().Single().TraineeProfileId.Should().Be(7);

        var current = Section(cut, "trainees-current-heading");
        Text(current.QuerySelector("h2")!).Should().Be("Current trainees 1 current");
        Text(current.QuerySelector("caption")!).Should().Be(AdmissionText.CurrentCaption);
        Headers(current).Should().Equal("Name", "Curriculum and sub-speciality", "Programme start", "Expected completion", "Actions");
        var row = current.QuerySelector("tbody tr")!;
        row.QuerySelector(".person-cell")!.InnerHtml.Should().Be("Paediatric EPA Curriculum (11.1) <span>Paediatrics</span>");
        var start = row.QuerySelector("td.inv-when")!;
        Text(start).Should().Be("2023-01-15 Training year 4");
        Text(start.QuerySelector("span")!).Should().Be("Training year 4");
        row.QuerySelector("a.btn")!.GetAttribute("aria-label").Should().Be("Edit Lerato Molefe's profile");
    }

    /// <summary>
    /// T373 fix pass, P4: the router keeps the page when the address loses its ?admitted= (the menu's Trainees, the trail,
    /// Back), and the admission's result goes with it: the page reads the lists and the result again for the new address.
    /// </summary>
    [Fact]
    public void P4_TheResult_DoesNotOutliveItsAddress()
    {
        var sender = new Sender
        {
            Pending = FiveWaiting().Where(row => row.UserId != "molefe").ToArray(),
            Result = new AdmissionResultDto(7, "molefe", "Lerato Molefe", "Paediatric EPA Curriculum", "11.1", 4, new(2023, 1, 15), new(2027, 1, 14), false)
        };
        var cut = RenderTrainees(sender, "?admitted=7");
        cut.FindAll(".action-result").Should().NotBeEmpty("guard: the result is shown");

        NavigateTo("/admin/trainees");

        cut.WaitForAssertion(() => cut.FindAll(".action-result").Should().BeEmpty("the address no longer names an admission"));
        sender.Asked.OfType<ListPendingTraineesQuery>().Should().HaveCount(2, "the lists are read again for the new address");
    }

    /// <summary>T373 fix pass, P2: a Try again that loads hands the focus to the registrar's first heading, not the body.</summary>
    [Fact]
    public void P2_ATryAgainThatLoads_FocusesWhoIsBeingAdmitted()
    {
        var sender = new Sender { Fail = new InvalidOperationException("timeout") };
        var cut = RenderAdmit(sender);
        sender.Fail = null;

        cut.Find(".alert-danger button").Click();

        cut.WaitForAssertion(() => FocusedIds().Should().Contain("admit-who-heading"));
    }

    /// <summary>
    /// T373 fix pass, P3: Admit refused as not waiting (another administrator admitted the registrar from another tab)
    /// swaps the form for the not-found panel, and the focus goes to its heading, the page's h1, not to the body.
    /// </summary>
    [Fact]
    public void P3_ANotPendingRefusal_IsTheNotFoundPanel_WithTheFocusOnItsHeading()
    {
        var cut = RenderAdmit(new Sender { Refusal = new TraineeAdmissionRefusedException(TraineeAdmissionRefusal.NotPending) });

        cut.Find("form").Submit();

        cut.WaitForAssertion(() => cut.Find("h1").TextContent.Trim().Should().Be("Registrar not found"));
        cut.WaitForAssertion(() => JSInterop.Invocations.Should().Contain(call => call.Identifier == PageFocus.FocusHeadingIdentifier));
    }

    private IReadOnlyList<string> FocusedIds()
        => JSInterop.Invocations
            .Where(invocation => invocation.Identifier == PageFocus.FocusByIdIdentifier)
            .Select(invocation => invocation.Arguments[0]?.ToString() ?? string.Empty)
            .ToList();

    [Fact]
    public void S8_AnIdNotTheCallers_SaysNothing()
    {
        var cut = RenderTrainees(new Sender { Pending = FiveWaiting(), Result = null }, "?admitted=99");

        cut.FindAll(".action-result").Should().BeEmpty();
    }

    [Fact]
    public void T04_NobodyWaiting_AndTheDerivedCompletion()
    {
        var sender = new Sender
        {
            Profiles = [Profile(9, "Sipho", "Ndlovu", new(2026, 1, 15), new(2027, 1, 15), 1)],
            Result = new AdmissionResultDto(9, "ndlovu", "Sipho Ndlovu", "Paediatric EPA Curriculum", "11.1", 1, new(2026, 1, 15), new(2027, 1, 15), true)
        };
        var cut = RenderTrainees(sender, "?admitted=9");

        var waiting = Section(cut, "trainees-waiting-heading");
        Text(waiting.QuerySelector(".action-result")!).Should().Be(
            "Sipho Ndlovu admitted to the Paediatric EPA Curriculum (11.1), training year 1, from 2026-01-15. Expected completion " +
            "2027-01-15, derived from the curriculum window: change it under Edit.");
        // Only the first sentence is bold, as the board draws it (T373 step 8).
        Text(waiting.QuerySelector(".action-result strong")!).Should().Be(
            "Sipho Ndlovu admitted to the Paediatric EPA Curriculum (11.1), training year 1, from 2026-01-15.");
        Text(waiting.QuerySelector("h2")!).Should().Be("Waiting to be admitted");
        Text(waiting.QuerySelector(".state-panel-title")!).Should().Be("Nobody is waiting to be admitted.");
        Text(waiting.QuerySelector(".state-panel-copy")!).Should().Be("A registrar appears here after registering from a Trainee invitation.");
    }

    [Fact]
    public void T06_TheAdministrator_HasAnInstitutionColumn_AndTheInstitutionUnderEachCurrentName()
    {
        SignedInAs(WombatRoles.Administrator);
        var cut = RenderTrainees(new Sender
        {
            Pending = [Pending("ndlovu", "Sipho", "Ndlovu")],
            Profiles = [Profile(1, "Anele", "Dlamini", new(2024, 1, 15), new(2028, 1, 14), 3)]
        });

        var waiting = Section(cut, "trainees-waiting-heading");
        Text(waiting.QuerySelector("caption")!).Should().Be(AdmissionText.WaitingCaptionAdministrator);
        Headers(waiting).Should().Equal("Name", "Email", "Institution", "Invited as", "Registered", "Actions");
        Text(waiting.QuerySelector("td[data-label=Institution]")!).Should().Be("Kgosi Kgari Teaching Hospital");

        var current = Section(cut, "trainees-current-heading");
        Headers(current)[0].Should().Be("Name and institution");
        current.QuerySelector("th[scope=row] .person-cell")!.InnerHtml.Should().Be("Anele Dlamini <span>Kgosi Kgari Teaching Hospital</span>");
    }

    [Fact]
    public void T07_ATraineeHoldingAdmin_SeesTheNote_AndNothingIsRead()
    {
        SignedInAs(WombatRoles.InstitutionalAdmin, WombatRoles.Trainee);
        var sender = new Sender();
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<PendingTraineesList>();

        Text(cut.Find("#trainees-trainee-note")).Should().Be(TraineeAdministrationRules.TraineeAdministersNoTrainee);
        cut.FindAll("table, h2, .btn").Should().BeEmpty();
        sender.Asked.Should().BeEmpty();
    }

    [Fact]
    public void T08_Loading_SaysWhatLoads()
    {
        Services.AddSingleton<IScopedSender>(new Sender { Hang = true });

        var cut = RenderComponent<PendingTraineesList>();

        var line = cut.Find(".loading-line");
        Text(line).Should().Be("Loading the trainees.");
        line.GetAttribute("role").Should().Be("status");
        cut.FindAll("div.skeleton.skel-title").Should().ContainSingle();
        cut.Markup.Should().NotContain("style=");
    }

    [Fact]
    public void T09_AFailedRead_IsAFixedSentence_WithTryAgain_NeverTheExceptionsText()
    {
        Services.AddSingleton<IScopedSender>(new Sender { Fail = new InvalidOperationException("Npgsql: connection refused") });

        var cut = RenderComponent<PendingTraineesList>();

        cut.WaitForAssertion(() => Text(cut.Find(".alert-danger .alert-row-text")).Should().Be(AdmissionText.TraineesLoadFailed));
        cut.Find(".alert-danger button").TextContent.Should().Contain("Try again");
        cut.Markup.Should().NotContain("Npgsql");
    }

    [Fact]
    public void T10_CancelOnAdmit_BringsTheFocusBackToTheRegistrarsAdmit()
    {
        var cut = RenderTrainees(new Sender { Pending = FiveWaiting() }, "?back=molefe");

        cut.WaitForAssertion(() => JSInterop.Invocations.Should().Contain(call =>
            call.Identifier == FocusById && Equals(call.Arguments[0], "trainees-admit-3")));
        cut.Find("#trainees-admit-3").GetAttribute("aria-label").Should().Be("Admit Lerato Molefe");
    }

    // ── Admit ‹name› ───────────────────────────────────────────────────────────

    [Fact]
    public void M01_Opened_TheCurriculumReadOut_TodayAndTrainingYear1()
    {
        var sender = new Sender();
        var cut = RenderAdmit(sender);

        Text(cut.Find("h1")).Should().Be("Admit Lerato Molefe");
        Text(cut.Find(".page-subtitle")).Should().Be("Admit a registrar who is waiting into the institution's curriculum.");
        cut.Find(".inv-page").ClassList.Should().Contain("inv-page--form");

        var who = cut.Find("section[aria-labelledby=admit-who-heading]");
        Text(who.QuerySelector("h2")!).Should().Be("Who is being admitted");
        who.QuerySelectorAll("dt").Select(Text).Should().Equal("Email", "Institution", "Invited as", "Registered");
        who.QuerySelectorAll("dd").Select(Text).Should().Equal("molefe@kgk.wombat.local", "Kgosi Kgari Teaching Hospital", "Paediatrics / Paediatrics", "2026-10-19");
        who.QuerySelector("dd")!.InnerHtml.Should().Be("molefe@<wbr>kgk.wombat.local");

        var form = cut.Find("form");
        form.GetAttribute("aria-labelledby").Should().Be("admit-programme-heading");
        Text(form.QuerySelector("h2")!).Should().Be("Programme");
        Text(form.QuerySelector("p.field-help")!).Should().Be("* marks a field you must fill in.");
        Text(form.QuerySelector("dl.form-group dd.readonly-value")!).Should().Be("Paediatric EPA Curriculum (11.1)");
        Text(form.QuerySelector("dl.form-group dd.field-help")!).Should().Be(
            "Adopted at Kgosi Kgari Teaching Hospital for Paediatrics / Paediatrics, the invited sub-speciality.");
        cut.FindAll("select").Should().BeEmpty("F1: one curriculum, read out");

        var start = cut.Find("#admit-start");
        start.GetAttribute("value").Should().Be("2026-10-19");
        start.GetAttribute("aria-required").Should().Be("true");
        Text(start.Closest(".form-group")!.QuerySelector("label")!).Should().Be("Programme start *");

        var output = cut.Find("output#admit-training-year");
        output.GetAttribute("for").Should().Be("admit-start");
        Text(output).Should().Be("Training year 1");
        Text(output.NextElementSibling!).Should().Be(
            "Training years turn every 365 days from the programme start, counted to 2026-10-19. It follows the date when you change it.");

        var end = cut.Find("#admit-end");
        end.HasAttribute("aria-required").Should().BeFalse("Expected completion has no mark");
        Text(cut.Find("#admit-end-help")).Should().Be("Leave this empty to derive it from the curriculum window. Left empty, it will be 2027-10-19.");

        var check = cut.Find(".submit-check");
        Text(check.QuerySelector("strong")!).Should().Be("When you admit:");
        check.QuerySelectorAll("li").Select(Text).Should().Equal(
            "Lerato Molefe becomes a Trainee on the Paediatric EPA Curriculum (11.1), in training year 1.",
            "Any session Lerato Molefe has open ends once, and Lerato Molefe signs in again as a Trainee.",
            "An email tells Lerato Molefe so.");

        var actions = cut.Find(".form-actions");
        actions.Children.Select(Text).Should().Equal("Cancel", "Admit Lerato Molefe");
        actions.QuerySelector("a")!.GetAttribute("href").Should().Be("/admin/trainees?back=molefe");
        cut.Markup.Should().NotContain("style=").And.NotContain(" disabled");
    }

    [Fact]
    public void M02_AChangeOfStart_AsksTheDatesAgain_AndTheOutputFollows()
    {
        var sender = new Sender();
        var cut = RenderAdmit(sender);

        cut.Find("#admit-start").Change("2023-01-15");

        cut.WaitForAssertion(() => Text(cut.Find("output#admit-training-year")).Should().Be("Training year 4"));
        sender.Asked.OfType<GetAdmissionDatesQuery>().Last().ProgrammeStart.Should().Be(new DateOnly(2023, 1, 15));
        cut.Find(".submit-check li").TextContent.Should().Contain("in training year 4");
    }

    [Fact]
    public void M06_AFutureStart_IsNotStarted()
    {
        var cut = RenderAdmit(new Sender());

        cut.Find("#admit-start").Change("2027-01-15");

        cut.WaitForAssertion(() => Text(cut.Find("output#admit-training-year")).Should().Be("Not started: the programme starts on 2027-01-15"));
        cut.FindAll(".submit-check li").Select(Text).Should().Equal(
            "Lerato Molefe becomes a Trainee on the Paediatric EPA Curriculum (11.1), whose programme starts on 2027-01-15. Nothing can be filed before then.",
            "Any session Lerato Molefe has open ends once, and Lerato Molefe signs in again as a Trainee.",
            "An email tells Lerato Molefe the day the programme starts.");
    }

    [Fact]
    public void M07_AStartMoreThanFourYearsBack_IsWarnedAbout_NotRefused()
    {
        var cut = RenderAdmit(new Sender());

        cut.Find("#admit-start").Change("2022-01-15");

        cut.WaitForAssertion(() => Text(cut.Find("#admit-start-warning")).Should().Be("This start is more than four years ago. Check the date before you admit."));
        cut.Find("#admit-start-warning").ClassList.Should().Contain("field-warning");
        cut.Find("#admit-start").GetAttribute("aria-describedby").Should().Be("admit-start-warning");
        cut.Find("#admit-start-warning").HasAttribute("role").Should().BeFalse();
    }

    [Fact]
    public void M04_NothingAdoptedForTheInvitedSubSpeciality_IsAStandingWarning_AndNoForm()
    {
        var cut = RenderAdmit(new Sender { Candidate = Molefe() with { Curricula = [], Standing = AdmissionStanding.NothingAdoptedForInvitedScope } });

        var warning = cut.Find(".alert-warning");
        Text(warning).Should().Be("No curriculum is adopted at Kgosi Kgari Teaching Hospital for Paediatrics / Paediatrics, so Lerato Molefe cannot be admitted.");
        warning.HasAttribute("role").Should().BeFalse();
        cut.FindAll("form").Should().BeEmpty();
        Text(cut.Find(".inv-page a.btn")).Should().Be("Back to trainees");
    }

    [Fact]
    public void M04_NothingAdoptedAtAll_KeepsTheInstitutionWideSentence_WithItsLink()
    {
        var cut = RenderAdmit(new Sender { Candidate = Molefe() with { Curricula = [], Standing = AdmissionStanding.NothingAdopted } });

        Text(cut.Find(".alert-warning")).Should().Be(AdmissionText.NothingAdoptedAtAll);
        cut.Find(".alert-warning a").GetAttribute("href").Should().Be("/admin/adoptions");
    }

    [Fact]
    public void M04b_ARegistrarOfNoInstitution_ToTheAdministrator()
    {
        SignedInAs(WombatRoles.Administrator);
        var cut = RenderAdmit(new Sender { Candidate = Molefe() with { InstitutionName = null, Curricula = [], Standing = AdmissionStanding.NoInstitution } });

        Text(cut.Find(".alert-warning")).Should().Be("This registrar belongs to no institution, so no curriculum can be offered.");
        cut.FindAll("form").Should().BeEmpty();
    }

    [Fact]
    public void M08_NotWaiting_OutOfScope_OrUnknown_IsTheNotFoundPanel()
    {
        var cut = RenderAdmit(new Sender { Candidate = null });

        Text(cut.Find("h1")).Should().Be("Registrar not found");
        var panel = cut.Find(".system-panel");
        panel.QuerySelectorAll("p").Select(Text).Should().Equal("No registrar waiting for admission has this link.", "Those waiting are listed on Trainees.");
        Text(panel.QuerySelector("a.btn")!).Should().Be("Go to Trainees");
        panel.QuerySelector("a.btn")!.GetAttribute("href").Should().Be("/admin/trainees");
    }

    [Fact]
    public void M09_ASecondAdmitFromAStaleTab_IsRefusedInWords_AndTheResultTakesTheFocus()
    {
        var cut = RenderAdmit(new Sender { Refusal = new TraineeAdmissionRefusedException(TraineeAdmissionRefusal.AlreadyAdmitted) });

        cut.Find("form").Submit();

        var result = cut.Find("#admit-result");
        cut.WaitForAssertion(() => Text(result).Should().Be("Not admitted. Lerato Molefe already has an active trainee profile. Open Trainees"));
        Text(result.QuerySelector("strong")!).Should().Be("Not admitted.");
        result.QuerySelector("a")!.GetAttribute("href").Should().Be("/admin/trainees");
        result.QuerySelector(".alert-danger")!.GetAttribute("role").Should().Be("alert");
        JSInterop.Invocations.Should().Contain(call => call.Identifier == "Blazor._internal.domWrapper.focus");
    }

    [Fact]
    public void M13_ADeactivatedAccount_IsRefused_WithUsersAsItsLink()
    {
        var cut = RenderAdmit(new Sender { Refusal = new TraineeAdmissionRefusedException(TraineeAdmissionRefusal.Deactivated) });

        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Text(cut.Find("#admit-result")).Should().Be(
            "Not admitted. Lerato Molefe's account is deactivated. Reactivate it under Users first."));
        cut.Find("#admit-result a").GetAttribute("href").Should().Be("/admin/users/molefe");
    }

    [Fact]
    public void M14_InvitedToNone_ChoosesACurriculum_AndIsToldToBeforeAnythingIsSent()
    {
        var sender = new Sender
        {
            Candidate = Molefe() with { InvitedAs = null, HasInvitedScope = false, RegisteredOn = null }
        };
        var cut = RenderAdmit(sender);

        Text(cut.FindAll(".detail-list dd").ToList()[2]).Should().Be("—");
        var select = cut.Find("select#admit-curriculum");
        select.GetAttribute("aria-required").Should().Be("true");
        select.QuerySelectorAll("option").Select(Text).Should().Equal("Choose a curriculum", "Paediatric EPA Curriculum (11.1)");
        Text(cut.Find("#admit-curriculum-help")).Should().Be(
            "This registrar was invited to no sub-speciality, so choose from the curricula Kgosi Kgari Teaching Hospital has adopted. Admitting sets the registrar's scope from it.");

        cut.Find("form").Submit();

        Text(cut.Find("#admit-curriculum-error")).Should().Be("Choose a curriculum.");
        cut.Find("#admit-curriculum").GetAttribute("aria-invalid").Should().Be("true");
        cut.Find("#admit-curriculum").GetAttribute("aria-describedby").Should().Be("admit-curriculum-help admit-curriculum-error");
        JSInterop.Invocations.Should().Contain(call => call.Identifier == FocusById && Equals(call.Arguments[0], "admit-curriculum"));
        sender.Commands.Should().BeEmpty();
    }

    [Fact]
    public void M15_AnEmptyCompletionWhoseDerivedDateHasPassed_IsRefusedBesideTheField_InOneSentence()
    {
        var sender = new Sender
        {
            Refusal = new TraineeAdmissionRefusedException(TraineeAdmissionRefusal.CompletionPassed, new DateOnly(2024, 1, 15))
        };
        var cut = RenderAdmit(sender);
        cut.Find("#admit-start").Change("2023-01-15");

        // Before Admit, the hint is the sentence (check rule 7).
        cut.WaitForAssertion(() => Text(cut.Find("#admit-end-help")).Should().Be(
            "Left empty, it would be 2024-01-15, which has passed. Enter the expected completion."));

        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Text(cut.Find("#admit-end-error")).Should().Be(
            "Left empty, it would be 2024-01-15, which has passed. Enter the expected completion."));
        Text(cut.Find("#admit-end-help")).Should().Be("Leave this empty to derive it from the curriculum window.", "the sentence is shown once");
        var end = cut.Find("#admit-end");
        end.GetAttribute("aria-invalid").Should().Be("true");
        end.GetAttribute("aria-describedby").Should().Be("admit-end-help admit-end-error");
        JSInterop.Invocations.Should().Contain(call => call.Identifier == FocusById && Equals(call.Arguments[0], "admit-end"));
    }

    [Fact]
    public void Admitted_GoesToTrainees_WithTheProfilesIdAlone()
    {
        var sender = new Sender();
        var cut = RenderAdmit(sender);

        cut.Find("#admit-start").Change("2023-01-15");
        cut.Find("#admit-end").Change("2027-01-14");
        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Services.GetRequiredService<NavigationManager>().Uri.Should().EndWith("/admin/trainees?admitted=7"));
        var command = sender.Commands.OfType<AdmitTraineeCommand>().Single();
        command.Should().BeEquivalentTo(new
        {
            UserId = "molefe",
            CurriculumId = 3000,
            ProgrammeStartDate = new DateOnly(2023, 1, 15),
            ExpectedCompletionDate = (DateOnly?)new DateOnly(2027, 1, 14)
        });
    }

    [Fact]
    public void ARefusalTheAdmitPageReadsAsNotWaiting_TurnsToTheNotFoundPanel()
    {
        var cut = RenderAdmit(new Sender { Refusal = new TraineeAdmissionRefusedException(TraineeAdmissionRefusal.NotPending) });

        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Text(cut.Find("h1")).Should().Be("Registrar not found"));
    }

    [Fact]
    public void AnUnexpectedFault_IsAFixedSentence_NeverItsText()
    {
        var cut = RenderAdmit(new Sender { Refusal = new InvalidOperationException("Sequence contains no elements") });

        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Text(cut.Find("#admit-result")).Should().Be($"Not admitted. {AdmissionText.AdmitFailed}"));
        cut.Markup.Should().NotContain("Sequence contains");
    }

    [Fact]
    public void M10_Loading_SaysWhatLoads_AndNamesNobody()
    {
        Services.AddSingleton<IScopedSender>(new Sender { Hang = true });
        NavigateTo("/admin/trainees/admit?userId=molefe");

        var cut = RenderComponent<AdmitRegistrar>();

        Text(cut.Find("h1")).Should().Be("Admit a registrar", "no name before the read");
        Text(cut.Find(".loading-line")).Should().Be("Loading this registrar.");
        cut.Find(".loading-line").GetAttribute("role").Should().Be("status");
    }

    [Fact]
    public void M11_AFailedRead_IsAFixedSentence_WithTryAgain()
    {
        Services.AddSingleton<IScopedSender>(new Sender { Fail = new InvalidOperationException("Npgsql: connection refused") });
        NavigateTo("/admin/trainees/admit?userId=molefe");

        var cut = RenderComponent<AdmitRegistrar>();

        cut.WaitForAssertion(() => Text(cut.Find(".alert-danger .alert-row-text")).Should().Be(AdmissionText.AdmitLoadFailed));
        Text(cut.Find("h1")).Should().Be("Admit a registrar");
        cut.FindAll(".page-subtitle").Should().BeEmpty();
        cut.Markup.Should().NotContain("Npgsql");
    }

    [Fact]
    public void M12_ATraineeHoldingAdmin_SeesTheNote_UnderAdmitARegistrar_AndNothingIsRead()
    {
        SignedInAs(WombatRoles.InstitutionalAdmin, WombatRoles.Trainee);
        var sender = new Sender();
        Services.AddSingleton<IScopedSender>(sender);
        NavigateTo("/admin/trainees/admit?userId=molefe");

        var cut = RenderComponent<AdmitRegistrar>();

        Text(cut.Find("h1")).Should().Be("Admit a registrar");
        Text(cut.Find("#admit-trainee-note")).Should().Be(TraineeAdministrationRules.TraineeAdministersNoTrainee);
        cut.FindAll("form, .page-subtitle").Should().BeEmpty();
        sender.Asked.Should().BeEmpty();
    }

    // ── Fixtures ───────────────────────────────────────────────────────────────

    private void SignedInAs(params string[] roles)
    {
        _auth.SetAuthorized("mbatha@kgk.wombat.local");
        _auth.SetRoles(roles);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "mbatha"), new Claim("institution_id", "10"));
    }

    private IRenderedComponent<PendingTraineesList> RenderTrainees(Sender sender, string query = "")
    {
        Services.AddSingleton<IScopedSender>(sender);
        NavigateTo($"/admin/trainees{query}");
        var cut = RenderComponent<PendingTraineesList>();
        cut.WaitForState(() => cut.FindAll(".loading-line").Count == 0);
        return cut;
    }

    private IRenderedComponent<AdmitRegistrar> RenderAdmit(Sender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);
        NavigateTo("/admin/trainees/admit?userId=molefe");
        var cut = RenderComponent<AdmitRegistrar>();
        cut.WaitForState(() => cut.FindAll(".loading-line").Count == 0);
        return cut;
    }

    private void NavigateTo(string relative)
    {
        var navigation = Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo(navigation.ToAbsoluteUri(relative).ToString());
    }

    private static PendingTraineeDto Pending(string userId, string first, string last) => new(
        userId, $"{userId}@kgk.wombat.local", first, last, 10, [1], [1])
    {
        InstitutionName = "Kgosi Kgari Teaching Hospital",
        InvitedAs = "Paediatrics / Paediatrics",
        RegisteredOn = Today
    };

    private static PendingTraineeDto[] FiveWaiting() =>
    [
        Pending("dlamini", "Anele", "Dlamini"),
        Pending("duplessis", "Pieter", "du Plessis"),
        Pending("mahlangu", "Nomsa", "Mahlangu"),
        Pending("molefe", "Lerato", "Molefe"),
        Pending("ndlovu", "Sipho", "Ndlovu")
    ];

    private static TraineeProfileDto Profile(int id, string first, string last, DateOnly start, DateOnly end, int year) => new(
        id, last.ToLowerInvariant(), $"{last.ToLowerInvariant()}@kgk.wombat.local", first, last,
        3000, "Paediatric EPA Curriculum", "11.1", 1, "Paediatrics", 1, "Paediatrics", start, end, IsActive: true)
    {
        TrainingYear = year,
        InstitutionName = "Kgosi Kgari Teaching Hospital"
    };

    private static AdmissionCandidateDto Molefe() => new(
        "molefe", "molefe@kgk.wombat.local", "Lerato", "Molefe", "Kgosi Kgari Teaching Hospital", "Paediatrics / Paediatrics",
        Today, HasInvitedScope: true, [new CurriculumChoiceDto(3000, "Paediatric EPA Curriculum", "11.1", 1, 1)],
        AdmissionStanding.Ready, IsDeactivated: false, Today);

    private static IElement Section(IRenderedFragment cut, string headingId) => cut.Find($"section[aria-labelledby={headingId}]");

    private static List<string> Headers(IElement section) => section.QuerySelectorAll("thead th").Select(Text).ToList();

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private sealed class Sender : IScopedSender
    {
        public IReadOnlyList<PendingTraineeDto> Pending { get; init; } = [];
        public IReadOnlyList<TraineeProfileDto> Profiles { get; init; } = [];
        public AdmissionResultDto? Result { get; init; }
        public AdmissionCandidateDto? Candidate { get; init; } = Molefe();
        public Exception? Refusal { get; init; }
        public Exception? Fail { get; set; }
        public bool Hang { get; init; }
        public List<object> Asked { get; } = [];
        public List<object> Commands { get; } = [];

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is AdmitTraineeCommand admit)
            {
                Commands.Add(admit);
                return Refusal is not null
                    ? Task.FromException<TResponse>(Refusal)
                    : Task.FromResult((TResponse)(object)Profile(7, "Lerato", "Molefe", admit.ProgrammeStartDate, admit.ExpectedCompletionDate ?? Today, 4));
            }

            Asked.Add(request);
            if (Hang)
            {
                return new TaskCompletionSource<TResponse>().Task;
            }

            if (Fail is not null)
            {
                return Task.FromException<TResponse>(Fail);
            }

            object? response = request switch
            {
                ListPendingTraineesQuery => Pending,
                ListTraineesForSpecialityQuery => Profiles,
                GetAdmissionResultQuery => Result,
                GetAdmissionCandidateQuery => Candidate,
                GetAdmissionDatesQuery dates => Dates(dates.ProgrammeStart),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return Task.FromResult((TResponse)response!);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        // A 12-month window, counted to 2026-10-19, as GetAdmissionDatesQuery answers it.
        private static AdmissionDatesDto Dates(DateOnly start)
        {
            var derived = start.AddMonths(12);
            return new AdmissionDatesDto(Today, TraineeProfile.StageOn(start, Today), derived, derived < Today, start < Today.AddYears(-4));
        }
    }
}
