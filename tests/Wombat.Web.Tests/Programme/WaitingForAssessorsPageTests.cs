using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Programme;
using Wombat.Application.Features.Programme.Commands.SendActivityReminder;
using Wombat.Application.Features.Programme.Waiting;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Programme;
using Wombat.Web.Navigation;
using Wombat.Web.Services;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Programme;

/// <summary>
/// Waiting for assessors (T358, flow 06, lane D; R2-Waiting w1–w15; Q3, Q4, C3, C4, C11, E3, E4, E6): the requests in the
/// programme of the role read as whose next move names one assessor, oldest first. The filters are applied with Show and
/// carried in the address (DESIGN.md § List page; D3); the heading counts the answer in flow 04's words and takes the
/// focus after Show and a page turn; each row's Waiting cell ends in Send a reminder, whose answer is shown above the
/// table, taking the focus, once the list has been read again.
/// </summary>
/// <remarks>The cast at Step 3.30 (D = 2026-10-04): Pieter Smit, the Coordinator, reads three requests.</remarks>
public sealed class WaitingForAssessorsPageTests : WombatTestContext
{
    /// <summary>What <see cref="ElementReference" />.FocusAsync calls.</summary>
    private const string FocusIdentifier = "Blazor._internal.domWrapper.focus";

    private const string Route = "/programme/waiting";

    /// <summary>08:10 SAST on 26 September, eight days before D.</summary>
    private static readonly DateTime EightDaysAgo = new(2026, 9, 26, 6, 10, 0, DateTimeKind.Utc);

    /// <summary>08:02 SAST on D, 4 October.</summary>
    private static readonly DateTime ThisMorning = new(2026, 10, 4, 6, 2, 0, DateTimeKind.Utc);

    private static readonly ProgrammeScopeDto Institution =
        new(WombatRoles.Coordinator, ProgrammeScopeKind.Institution, "Kgosi Kgari Teaching Hospital", 1, [], []);

    private readonly TestAuthorizationContext _auth;

    public WaitingForAssessorsPageTests()
    {
        _auth = this.AddTestAuthorization();
        _auth.SetAuthorized("smit@kgk.test");
        _auth.SetRoles(WombatRoles.Coordinator);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "smit"));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    // ---- w1: typical, Step 3.30 ----

    [Fact]
    public void Typical_ListsTheThreeRequests_OldestFirst_WithWhomEachWaits_HowLong_AndSendAReminder()
    {
        var sender = new Sender(Rows(Three()));

        var cut = Render(sender);

        cut.Find("h1").TextContent.Should().Be("Waiting for assessors");
        TabTitle.Of(this, cut).Should().Be("Waiting for assessors · Wombat");
        cut.Find(".page-subtitle").TextContent.Trim().Should().Be(
            "Requests at Kgosi Kgari Teaching Hospital whose next move names an assessor, supervisor or reviewer, read as " +
            "Coordinator. Your own requests are not listed.");

        var heading = cut.Find("section.list-section > h2#waiting-h");
        heading.ClassList.Should().Contain("list-section-title");
        heading.GetAttribute("tabindex").Should().Be("-1");
        Text(heading).Should().Be("3 waiting, 2 overdue");
        cut.Find("section.list-section").GetAttribute("aria-labelledby").Should().Be("waiting-h");
        cut.Find(".needs-you-rule").TextContent.Should().Be(
            "Oldest first. Overdue once it has waited 7 days. Its assessor is emailed after 5. Waiting counts from the last " +
            "move: any save restarts it.");

        var table = cut.Find("table");
        table.ClassList.Should().Contain("clinic-table", "clinic-table--stack");
        table.GetAttribute("role").Should().Be("table");
        var caption = table.QuerySelector("caption")!;
        caption.ClassList.Should().Contain("visually-hidden");
        caption.TextContent.Should().Be("Requests waiting for a named assessor, oldest first");
        table.QuerySelectorAll("thead th").Select(Text).Should().Equal("Activity", "With", "State", "Waiting");

        var rows = table.QuerySelectorAll("tbody tr").ToList();
        rows.Should().HaveCount(3);
        var cells = rows[1].QuerySelectorAll("td").ToList();
        cells.Select(cell => cell.GetAttribute("data-label")).Should().Equal(null, "With", "State", "Waiting");
        var link = cells[0].QuerySelector("a.activity-link")!;
        link.GetAttribute("href").Should().Be("/activities/4");
        link.ClassList.Should().Contain("activity-block-link", "a 44px block below 641px (Step A.7.9)");
        link.TextContent.Should().Be("Mini-CEX (Paediatrics) · PAED-004 · 2026-10-01, from Nomsa Mahlangu");
        link.HasAttribute("aria-label").Should().BeFalse("no other link reads the same");
        Text(cells[1]).Should().Be("Thandi Zulu");
        cells[2].QuerySelectorAll(".needs-you-badges .badge").Select(Text).Should().Equal("Requested", "Overdue");
        cells[3].ClassList.Should().Contain("waited-cell");
        cells[3].QuerySelector("b")!.TextContent.Should().Be("8 days");
        cells[3].QuerySelector("span")!.TextContent.Should().Be("since 2026-09-26 08:10 SAST");
        var button = cells[3].QuerySelector("button.reminder-action")!;
        button.TextContent.Should().Be("Send a reminder");
        button.GetAttribute("aria-label").Should().Be(
            "Send Thandi Zulu a reminder about Mini-CEX (Paediatrics) · PAED-004 · 2026-10-01, from Nomsa Mahlangu");

        var newest = rows[2].QuerySelectorAll("td").ToList();
        newest[3].QuerySelector("b")!.TextContent.Should().Be("Less than a day");
        newest[2].QuerySelectorAll(".badge").Select(Text).Should().Equal("Requested");

        var query = sender.Queries.Should().ContainSingle().Subject;
        (query.ActingRole, query.OverdueOnly, query.WithUserId, query.SubjectUserId, query.Page, query.PageSize)
            .Should().Be((WombatRoles.Coordinator, false, (string?)null, (string?)null, 1, 20));
        cut.Find("nav.pager").GetAttribute("aria-label").Should().Be("Waiting, pages");
    }

    [Fact]
    public void TheFilterForm_IsAGetFormNamedForItsList_WaitingAndWith_ThenShow_AndNoClearFiltersWhenNoneIsSet()
    {
        var cut = Render(new Sender(Rows(Three())));

        var form = cut.Find("form.search-container");
        form.GetAttribute("aria-label").Should().Be("Filter Waiting for assessors");
        form.GetAttribute("method").Should().Be("get");
        cut.Find("label[for='f-show']").TextContent.Should().Be("Waiting");
        cut.Find("#f-show").GetAttribute("name").Should().Be("show");
        cut.FindAll("#f-show option").Select(Text).Should().Equal("All", "Overdue only");
        cut.Find("label[for='f-with']").TextContent.Should().Be("With");
        cut.Find("#f-with").GetAttribute("name").Should().Be("with");
        cut.FindAll("#f-with option").Select(Text).Should().Equal("Anyone", "Fatima Khumalo", "Mohammed Patel", "Thandi Zulu");
        var actions = cut.Find("div.search-field.filter-actions");
        actions.QuerySelectorAll("button, a").Select(Text).Should().Equal("Show");
        actions.QuerySelector("button")!.ClassList.Should().Contain("btn-primary");
        actions.QuerySelector("button")!.GetAttribute("type").Should().Be("submit");
    }

    // ---- nothing changes until Show; the address carries the filters ----

    [Fact]
    public void SettingTheFields_ChangesNothing_UntilShow_WhichPutsTheFiltersInTheAddress_AndTheAnswersHeadingTakesTheFocus()
    {
        var sender = new Sender(query => query.OverdueOnly ? Answer(NoMatch()) : Answer(Dto(Three())));
        var cut = Render(sender);
        var navigation = Services.GetRequiredService<NavigationManager>();

        cut.Find("#f-show").Change(ProgrammeLinksShowOverdue);
        cut.Find("#f-with").Change("khumalo");

        sender.Queries.Should().ContainSingle("no field acts on change: no live filtering");
        navigation.Uri.Should().EndWith(Route);
        Text(cut.Find("h2#waiting-h")).Should().Be("3 waiting, 2 overdue");

        cut.Find("form.search-container").Submit();

        navigation.Uri.Should().EndWith("/programme/waiting?show=overdue&with=khumalo");
        cut.WaitForAssertion(() => sender.Queries.Should().HaveCount(2));
        var asked = sender.Queries[^1];
        (asked.OverdueOnly, asked.WithUserId, asked.Page).Should().Be((true, "khumalo", 1));
        cut.WaitForAssertion(() => FocusCalls().Should().ContainSingle()
            .Which.Arguments[0].Should().BeOfType<ElementReference>()
            .Which.Id.Should().Be(cut.Instance.WaitingHeading.Id));
    }

    [Fact]
    public void ShowWithNothingChanged_ReadsTheListAgain_AndItsHeadingTakesTheFocus()
    {
        var sender = new Sender(Rows(Three()));
        var cut = Render(sender);

        cut.Find("form.search-container").Submit();

        cut.WaitForAssertion(() => sender.Queries.Should().HaveCount(2));
        Services.GetRequiredService<NavigationManager>().Uri.Should().EndWith(Route);
        cut.WaitForAssertion(() => FocusCalls().Should().ContainSingle()
            .Which.Arguments[0].Should().BeOfType<ElementReference>()
            .Which.Id.Should().Be(cut.Instance.WaitingHeading.Id));
    }

    // ---- w10: filtered by With, read from the address ----

    [Fact]
    public void FilteredByWith_ReadFromTheAddress_CountsTheAnswerWithItsName_AndOffersClearFilters()
    {
        var patel = Three()[0];
        var sender = new Sender(_ => Answer(Dto(
            [patel], scope: Paediatrics(WombatRoles.SpecialityAdmin), total: 3, totalOverdue: 2,
            nominees: Nominees(Three()), filter: new WaitingForAssessorsFilter(WithUserId: "patel"))));

        var cut = Render(sender, $"{Route}?with=patel", SpecialityAdmin());

        var query = sender.Queries.Should().ContainSingle().Subject;
        (query.ActingRole, query.OverdueOnly, query.WithUserId).Should().Be((WombatRoles.SpecialityAdmin, false, "patel"));
        Text(cut.Find("h2#waiting-h")).Should().Be("1 waiting, 1 overdue, with Mohammed Patel");
        cut.Find("caption").TextContent.Should().Be("Requests waiting for Mohammed Patel, oldest first");
        cut.Find("#f-with").GetAttribute("value").Should().Be("patel");
        cut.Find("#f-show").GetAttribute("value").Should().Be("all");
        var clear = cut.Find("div.filter-actions a");
        Text(clear).Should().Be("Clear filters");
        clear.ClassList.Should().Contain("btn-outline");
        clear.GetAttribute("href").Should().Be(Route);
    }

    // ---- w11: no match is not empty ----

    [Fact]
    public void NoMatch_SaysNoRequestMatches_WhatWasAsked_AndClearFilters_UnderTheZeroOfTotalHeading()
    {
        var sender = new Sender(_ => Answer(NoMatch()));

        var cut = Render(sender, $"{Route}?show=overdue&with=khumalo", SpecialityAdmin());

        Text(cut.Find("h2#waiting-h")).Should().Be("0 of 3 waiting");
        cut.Find("h2#waiting-h").GetAttribute("tabindex").Should().Be("-1");
        var card = cut.Find("section.list-section .detail-card.detail-card--empty");
        card.QuerySelector(".state-panel-title")!.TextContent.Should().Be("No request matches these filters.");
        card.QuerySelector(".state-panel-copy")!.TextContent.Should().Be("Overdue only, with Fatima Khumalo.");
        var clear = card.QuerySelector(".form-actions a")!;
        Text(clear).Should().Be("Clear filters");
        clear.GetAttribute("href").Should().Be(Route);
        cut.FindAll("table, .needs-you-rule").Should().BeEmpty();
        cut.FindAll("form.search-container").Should().ContainSingle("the filters stand, so they can be changed");
        cut.Find("#f-show").GetAttribute("value").Should().Be("overdue");
    }

    // ---- w12: empty ----

    [Fact]
    public void Empty_SaysNothingIsWaiting_AndDrawsNoFilterForm()
    {
        var cut = Render(new Sender(Always(Dto([]))));

        Text(cut.Find("section.list-section > h2#waiting-h")).Should().Be("Waiting");
        var card = cut.Find(".detail-card.detail-card--empty");
        card.QuerySelector(".state-panel-title")!.TextContent.Should().Be("Nothing is waiting");
        card.QuerySelector(".state-panel-copy")!.TextContent.Should().Be(
            "Nothing at Kgosi Kgari Teaching Hospital is waiting for an assessor, supervisor or reviewer.");
        cut.FindAll("form, table, .pager").Should().BeEmpty();
    }

    /// <summary>
    /// A caller the read has no scope for (no institution, or no claimed sub-speciality; the query's null): nothing to
    /// list, said in the card's words, never a refusal (T358, lane D).
    /// </summary>
    [Fact]
    public void NoScope_IsNothingWaiting_WithNoFormAndNoScopeNamed()
    {
        var cut = Render(new Sender(_ => Task.FromResult<WaitingForAssessorsDto?>(null)));

        cut.Find("h1").TextContent.Should().Be("Waiting for assessors");
        cut.FindAll(".page-subtitle").Should().BeEmpty();
        cut.Find(".state-panel-title").TextContent.Should().Be("Nothing is waiting");
        cut.Find(".state-panel-copy").TextContent.Should().Be("Nothing is waiting for an assessor.");
        cut.FindAll("form, table").Should().BeEmpty();
    }

    // ---- w13, w14: loading and the load error ----

    [Fact]
    public void Loading_KeepsTheFilters_DrawsSkeletonRowsUnderTheHeading_AndSaysSo()
    {
        var held = new TaskCompletionSource<WaitingForAssessorsDto?>();
        var cut = Render(new Sender(_ => held.Task));

        cut.Find("p.visually-hidden[role=status]").TextContent.Should().Be("Loading Waiting for assessors.");
        var section = cut.Find("section.list-section[aria-busy=true]");
        Text(section.QuerySelector("h2")!).Should().Be("Waiting");
        section.QuerySelector(".dashboard-card-skeleton[aria-hidden=true]")!.QuerySelectorAll(".skeleton").Should().HaveCount(3);
        cut.FindAll("form.search-container").Should().ContainSingle();
        cut.FindAll("table").Should().BeEmpty();

        held.SetResult(Dto(Three()));
        cut.WaitForAssertion(() => cut.FindAll("tbody tr").Should().HaveCount(3), AsyncWorkTimeout);
        cut.Find("p.visually-hidden[role=status]").TextContent.Should().BeEmpty("the status stays, empty once loaded");
    }

    [Fact]
    public void ALoadError_IsFixedWords_WithTryAgain_WhoseAnswersHeadingTakesTheFocus()
    {
        var fail = true;
        var sender = new Sender(_ => fail
            ? Task.FromException<WaitingForAssessorsDto?>(new InvalidOperationException("a secret connection string"))
            : Answer(Dto(Three())));
        var cut = Render(sender);

        var alert = cut.Find(".action-result .alert-danger");
        Text(alert.QuerySelector(".alert-row-text")!).Should().Be(
            "Could not load Waiting for assessors. Nothing has changed. Try again, or come back in a few minutes.");
        cut.Markup.Should().NotContain("secret");

        fail = false;
        alert.QuerySelector("button")!.Click();

        cut.WaitForAssertion(() => cut.FindAll("tbody tr").Should().HaveCount(3));
        cut.WaitForAssertion(() => FocusCalls().Should().ContainSingle()
            .Which.Arguments[0].Should().BeOfType<ElementReference>()
            .Which.Id.Should().Be(cut.Instance.WaitingHeading.Id));
    }

    // ---- w2: the dialog ----

    [Fact]
    public void SendAReminder_OpensTheDialog_NamingTheAssessorTheMailAndWhatItDoesNotDo_AndSendsNothing()
    {
        var sender = new Sender(Rows(Three()));
        var cut = Render(sender);

        MiniCexButton(cut).Click();

        var dialog = MiniCexRow(cut).QuerySelector("dialog")!;
        dialog.QuerySelector("h2")!.TextContent.Should().Be("Send Thandi Zulu a reminder?");
        dialog.QuerySelectorAll("button").Select(Text).Should().Equal("Don't send", "Send the reminder");
        sender.Commands.Should().BeEmpty();
    }

    // ---- w2b, w3: in flight, then the result above the table, taking the focus, the list read again ----

    [Fact]
    public void TheResult_IsShownAboveTheTable_AfterTheListIsReadAgain_TheRowsButtonGivingWayToTheRecord_AndTakesTheFocus()
    {
        var held = new TaskCompletionSource<SendActivityReminderResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        var reminded = false;
        var sender = new Sender(_ => Answer(Dto(reminded ? RemindedToday(Three()) : Three())), _ => held.Task);
        var cut = Render(sender);

        MiniCexButton(cut).Click();
        MiniCexConfirm(cut).Click();
        cut.WaitForAssertion(() => sender.Commands.Should().ContainSingle());
        MiniCexConfirm(cut).TextContent.Should().Be("Sending…");

        var command = sender.Commands.Single();
        (command.ActingRole, command.ActivityId, command.ExpectedState, command.ExpectedUpdatedOn)
            .Should().Be((WombatRoles.Coordinator, 4, "requested", EightDaysAgo));

        reminded = true;
        held.SetResult(Sent());

        cut.WaitForAssertion(() => cut.Find(".action-result .alert-success").TextContent.Should().Be(
            "Reminder sent to Thandi Zulu. It lists Mini-CEX (Paediatrics) · PAED-004 · 2026-10-01, from Nomsa Mahlangu, " +
            "waiting 8 days. The request is still Requested; its wait is unchanged."), AsyncWorkTimeout);
        cut.Find(".action-result .alert-success strong").TextContent.Should().Be("Reminder sent to Thandi Zulu.");
        cut.Find(".action-result .alert-success").GetAttribute("role").Should().Be("status");
        sender.Queries.Should().HaveCount(2, "the list is read again after a send");
        sender.Queries[^1].Page.Should().Be(1);

        var row = cut.FindAll("tbody tr").ToList()[1];
        row.QuerySelectorAll("button.reminder-action").Should().BeEmpty();
        row.QuerySelector(".waited-cell .progress-row-meta")!.TextContent.Should().Be("Reminded 2026-10-04 by Pieter Smit");

        cut.WaitForAssertion(() => FocusCalls().Should().ContainSingle()
            .Which.Arguments[0].Should().BeOfType<ElementReference>()
            .Which.Id.Should().Be(cut.Instance.ResultRegion.Id), AsyncWorkTimeout);
        var order = cut.FindAll(".action-result, section.list-section").Select(element => element.TagName).ToList();
        order.Should().Equal(new[] { "DIV", "SECTION" }, "the result is above the list");
    }

    // ---- w4, w4b: the same day, the next day ----

    [Fact]
    public void RemindedTodayBySomeoneElse_ShowsTheRecord_AndNoButton()
    {
        var cut = Render(new Sender(Always(Dto(RemindedToday(Three()), scope: Paediatrics(WombatRoles.SpecialityAdmin)))), acting: SpecialityAdmin());

        var cell = cut.FindAll("tbody tr").ToList()[1].QuerySelector(".waited-cell")!;
        cell.QuerySelectorAll("button").Should().BeEmpty();
        cell.QuerySelector(".progress-row-meta")!.TextContent.Should().Be("Reminded 2026-10-04 by Pieter Smit");
    }

    [Fact]
    public void TheNextDay_TheRecordStays_AndTheButtonIsBack()
    {
        var rows = RemindedToday(Three()).Select(row => row with { RemindedToday = false }).ToList();
        var cut = Render(new Sender(Always(Dto(rows))));

        var cell = cut.FindAll("tbody tr").ToList()[1].QuerySelector(".waited-cell")!;
        cell.QuerySelector(".progress-row-meta")!.TextContent.Should().Be("Reminded 2026-10-04 by Pieter Smit");
        cell.QuerySelectorAll("button.reminder-action").Should().ContainSingle();
    }

    // ---- w5–w7: refusals, the row unchanged ----

    public static TheoryData<ReminderOutcome, string> Refusals() => new()
    {
        { ReminderOutcome.Deactivated, "Not sent. Thandi Zulu's account is deactivated, so Wombat sends Thandi Zulu no email. The request still waits." },
        { ReminderOutcome.NoEmail, "Not sent. Thandi Zulu has no email address in Wombat. Ask your institutional admin to add one." },
        { ReminderOutcome.NoAccount, "Not sent. Wombat has no account for the person this request names; it was erased or deleted, so there is nobody to email. The request still waits." },
        { ReminderOutcome.NotFound, "Not sent. This request is no longer on your list." }
    };

    [Theory]
    [MemberData(nameof(Refusals))]
    public void ARefusal_IsAnAlertAboveTheTable_ThatTakesTheFocus_TheRowUnchanged(ReminderOutcome outcome, string words)
    {
        var sender = new Sender(Rows(Three()), _ => Task.FromResult(Refused(outcome)));
        var cut = Render(sender);

        MiniCexButton(cut).Click();
        MiniCexConfirm(cut).Click();

        cut.WaitForAssertion(() => cut.Find(".action-result .alert-danger").TextContent.Should().Be(words), AsyncWorkTimeout);
        cut.Find(".action-result .alert-danger").GetAttribute("role").Should().Be("alert");
        cut.Find(".action-result .alert-danger strong").TextContent.Should().Be("Not sent.");
        cut.FindAll("tbody tr").Should().HaveCount(3);
        MiniCexButton(cut).TextContent.Should().Be("Send a reminder", "the row is unchanged");
        cut.WaitForAssertion(() => FocusCalls().Should().ContainSingle()
            .Which.Arguments[0].Should().BeOfType<ElementReference>()
            .Which.Id.Should().Be(cut.Instance.ResultRegion.Id), AsyncWorkTimeout);
    }

    // ---- w8: moved meanwhile, the row gone, the heading recounted ----

    [Fact]
    public void MovedMeanwhile_ReadsTheListAgain_TheRowGone_AndTheHeadingRecounted()
    {
        var moved = false;
        var three = Three();
        var sender = new Sender(
            _ => Answer(Dto(moved ? [three[0], three[2]] : three)),
            _ =>
            {
                moved = true;
                return Task.FromResult(Refused(ReminderOutcome.MovedMeanwhile));
            });
        var cut = Render(sender);

        MiniCexButton(cut).Click();
        MiniCexConfirm(cut).Click();

        cut.WaitForAssertion(() => cut.Find(".action-result .alert-danger").TextContent.Should().Be(
            "Not sent. Mini-CEX (Paediatrics) · PAED-004 · 2026-10-01 moved at 2026-10-04 08:12 SAST: it is now Completed and " +
            "waits for nobody."), AsyncWorkTimeout);
        Text(cut.Find("h2#waiting-h")).Should().Be("2 waiting, 1 overdue");
        cut.FindAll("tbody tr").Should().HaveCount(2);
        cut.Markup.Should().NotContain("PAED-004 · 2026-10-01, from");
    }

    [Fact]
    public void ASendThatFails_IsFixedWordsAboveTheTable_NeverTheExceptionsText()
    {
        var sender = new Sender(Rows(Three()), _ => Task.FromException<SendActivityReminderResult>(new InvalidOperationException("a secret")));
        var cut = Render(sender);

        MiniCexButton(cut).Click();
        MiniCexConfirm(cut).Click();

        cut.WaitForAssertion(() => cut.Find(".action-result .alert-danger").TextContent.Should().Be(
            "Not sent. Something went wrong, and no reminder was sent. Try again, or come back in a few minutes."), AsyncWorkTimeout);
        cut.Markup.Should().NotContain("secret");
    }

    // ---- w9: heavy, the deactivated row and the pair that reads the same ----

    [Fact]
    public void Heavy_NamesTwoAlikeLinksApart_SaysWhyADeactivatedNomineeHasNoButton_AndPagesTwentyAtATime()
    {
        var rows = Heavy();
        var sender = new Sender(query => Answer(Dto(
            rows.Skip((query.Page - 1) * 20).Take(20).ToList(), match: rows.Count, matchOverdue: rows.Count(row => row.IsOverdue),
            page: query.Page, scope: Paediatrics(WombatRoles.SpecialityAdmin))));
        var cut = Render(sender, acting: SpecialityAdmin());

        Text(cut.Find("h2#waiting-h")).Should().Be("22 waiting, 21 overdue");
        var links = cut.FindAll("a.activity-link").ToList();
        links[1].GetAttribute("aria-label").Should().Be(
            "Mini-CEX (Paediatrics) · PAED-003 · 2026-09-24, from Nomsa Mahlangu (1 of 2)");
        links[2].GetAttribute("aria-label").Should().Be(
            "Mini-CEX (Paediatrics) · PAED-003 · 2026-09-24, from Nomsa Mahlangu (2 of 2)");
        cut.FindAll("button.reminder-action").Select(button => button.GetAttribute("aria-label")).Should()
            .Contain("Send Mohammed Patel a reminder about Mini-CEX (Paediatrics) · PAED-003 · 2026-09-24, from Nomsa Mahlangu (2 of 2)");

        var deactivated = cut.FindAll("tbody tr").ToList()[3].QuerySelector(".waited-cell")!;
        deactivated.QuerySelectorAll("button").Should().BeEmpty();
        deactivated.QuerySelector(".progress-row-meta")!.TextContent.Should().Be("No reminder: Fatima Khumalo's account is deactivated.");

        var pager = cut.Find("nav.pager");
        pager.GetAttribute("aria-label").Should().Be("Waiting, pages");
        cut.FindAll("tbody tr").Should().HaveCount(20);

        pager.QuerySelectorAll("button").Single(button => Text(button) == "Next").Click();

        cut.WaitForAssertion(() => cut.FindAll("tbody tr").Should().HaveCount(2));
        sender.Queries[^1].Page.Should().Be(2);
        cut.WaitForAssertion(() => FocusCalls().Should().ContainSingle()
            .Which.Arguments[0].Should().BeOfType<ElementReference>()
            .Which.Id.Should().Be(cut.Instance.WaitingHeading.Id));
    }

    // ---- w15: the Sub-speciality admin, read for the acting role ----

    [Fact]
    public void ASubSpecialityAdmin_ReadsAsTheActingRole_AndTheSubtitleSaysWhatWasRead()
    {
        var sender = new Sender(_ => Answer(Dto(Three(), scope: Paediatrics(WombatRoles.SubSpecialityAdmin))));
        var acting = ActingRoleResolver.Resolve(WombatRoles.SubSpecialityAdmin,
            [WombatRoles.SubSpecialityAdmin, WombatRoles.CommitteeMember, WombatRoles.Assessor]);

        var cut = Render(sender, acting: acting);

        sender.Queries.Single().ActingRole.Should().Be(WombatRoles.SubSpecialityAdmin);
        cut.Find(".page-subtitle").TextContent.Trim().Should().Be(
            "Requests in Paediatrics whose next move names an assessor, supervisor or reviewer, read as Sub-speciality admin. " +
            "Your own requests are not listed.");
    }

    /// <summary>D2: acting as Assessor, a Speciality admin who types the address reads it as Speciality admin.</summary>
    [Fact]
    public void ActingInARoleThePageDoesNotAdmit_ReadsAsTheFirstAdmittedRoleHeld()
    {
        var sender = new Sender(_ => Answer(Dto(Three(), scope: Paediatrics(WombatRoles.SpecialityAdmin))));

        Render(sender, acting: ActingRoleResolver.Resolve(WombatRoles.Assessor, [WombatRoles.Assessor, WombatRoles.SpecialityAdmin]));

        sender.Queries.Single().ActingRole.Should().Be(WombatRoles.SpecialityAdmin);
    }

    [Fact]
    public void ARoleThatMayNotRemind_IsOfferedNoButton()
    {
        var cut = Render(new Sender(Always(Dto(Three(), mayRemind: false))));

        cut.FindAll("tbody tr").Should().HaveCount(3);
        cut.FindAll("button.reminder-action, dialog").Should().BeEmpty();
    }

    /// <summary>Round 3 check 1: no third-person pronoun in any word the page shows.</summary>
    [Fact]
    public void NoWordOnThePage_IsAPronoun()
    {
        var cut = Render(new Sender(Rows(Three())));

        var words = string.Join(' ', cut.Nodes.Select(node => node.TextContent))
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .Select(word => word.Trim('.', ',', ':', ';', '"', '(', ')').ToLowerInvariant());
        words.Should().NotContain(["she", "her", "hers", "he", "him", "his"]);
    }

    // ---- helpers ----

    private const string ProgrammeLinksShowOverdue = "overdue";

    private IRenderedComponent<WaitingForAssessors> Render(Sender sender, string uri = Route, ActingRole? acting = null)
    {
        Services.AddSingleton<IScopedSender>(sender);
        Services.GetRequiredService<NavigationManager>().NavigateTo(uri);
        return RenderComponent<WaitingForAssessors>(parameters => parameters
            .AddCascadingValue(acting ?? ActingRoleResolver.Resolve(null, [WombatRoles.Coordinator])));
    }

    private static ActingRole SpecialityAdmin() => ActingRoleResolver.Resolve(null, [WombatRoles.SpecialityAdmin]);

    private static ProgrammeScopeDto Paediatrics(string role)
        => new(role, role == WombatRoles.SpecialityAdmin ? ProgrammeScopeKind.Speciality : ProgrammeScopeKind.SubSpeciality,
            "Paediatrics", 1, [3], [4]);

    private static IElement MiniCexRow(IRenderedFragment cut) => cut.FindAll("tbody tr").ToList()[1];

    private static IElement MiniCexButton(IRenderedFragment cut) => MiniCexRow(cut).QuerySelector("button.reminder-action")!;

    private static IElement MiniCexConfirm(IRenderedFragment cut) => MiniCexRow(cut).QuerySelector("dialog button.btn-primary")!;

    private IEnumerable<JSRuntimeInvocation> FocusCalls()
        => JSInterop.Invocations.Where(invocation => invocation.Identifier == FocusIdentifier);

    private static string Text(IElement element) => string.Join(' ', element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static Func<ListWaitingForAssessorsQuery, Task<WaitingForAssessorsDto?>> Rows(IReadOnlyList<ActivitySummaryDto> rows)
        => Always(Dto(rows));

    private static Func<ListWaitingForAssessorsQuery, Task<WaitingForAssessorsDto?>> Always(WaitingForAssessorsDto dto)
        => _ => Answer(dto);

    private static Task<WaitingForAssessorsDto?> Answer(WaitingForAssessorsDto dto) => Task.FromResult<WaitingForAssessorsDto?>(dto);

    private static WaitingForAssessorsDto Dto(
        IReadOnlyList<ActivitySummaryDto> rows,
        ProgrammeScopeDto? scope = null,
        int? match = null,
        int? matchOverdue = null,
        int? total = null,
        int? totalOverdue = null,
        IReadOnlyList<NomineeOptionDto>? nominees = null,
        WaitingForAssessorsFilter? filter = null,
        int page = 1,
        bool mayRemind = true)
    {
        return new WaitingForAssessorsDto(
            scope ?? Institution, rows, match ?? rows.Count, matchOverdue ?? rows.Count(row => row.IsOverdue), total ?? match ?? rows.Count,
            totalOverdue ?? matchOverdue ?? rows.Count(row => row.IsOverdue), nominees ?? Nominees(rows), 7, 5, page, 20, mayRemind)
        {
            Filter = filter ?? new WaitingForAssessorsFilter()
        };
    }

    /// <summary>w11: Overdue only, with Fatima Khumalo, whose Case-Based Discussion has waited less than a day.</summary>
    private static WaitingForAssessorsDto NoMatch()
        => Dto([], scope: Paediatrics(WombatRoles.SpecialityAdmin), match: 0, matchOverdue: 0, total: 3, totalOverdue: 2,
            nominees: Nominees(Three()), filter: new WaitingForAssessorsFilter(true, "khumalo"));

    private static IReadOnlyList<NomineeOptionDto> Nominees(IEnumerable<ActivitySummaryDto> rows)
        => rows.Select(row => new NomineeOptionDto(row.Holder!.UserId!, row.Holder.Name!))
            .DistinctBy(nominee => nominee.UserId)
            .OrderBy(nominee => nominee.Name.Split(' ')[^1], StringComparer.Ordinal)
            .ToList();

    /// <summary>Step 3.30's three, oldest first: du Plessis's portfolio review, Mahlangu's Mini-CEX, du Plessis's CBD.</summary>
    private static List<ActivitySummaryDto> Three() =>
    [
        Request(15, "Portfolio and Logbook Review (Paediatrics)", "Pieter du Plessis", "patel", "Mohammed Patel", "submitted",
            "Awaiting review", 8, EightDaysAgo.AddMinutes(-4), "PAED-015", new DateOnly(2026, 10, 3)),
        Request(4, "Mini-CEX (Paediatrics)", "Nomsa Mahlangu", "zulu", "Thandi Zulu", "requested", "Requested", 8, EightDaysAgo,
            "PAED-004", new DateOnly(2026, 10, 1)),
        Request(2, "Case-Based Discussion (Paediatrics)", "Pieter du Plessis", "khumalo", "Fatima Khumalo", "requested",
            "Requested", 0, ThisMorning, "PAED-002", new DateOnly(2026, 9, 29))
    ];

    private static List<ActivitySummaryDto> RemindedToday(List<ActivitySummaryDto> rows)
    {
        rows[1] = rows[1] with
        {
            LastReminder = new ActivityReminderDto(new DateTime(2026, 10, 4, 7, 0, 0, DateTimeKind.Utc), new DateOnly(2026, 10, 4), "Pieter Smit"),
            RemindedToday = true
        };
        return rows;
    }

    /// <summary>
    /// w9, invented: 22 waiting, one page of 20 and a second of 2. Two Mini-CEX from Nomsa Mahlangu on one EPA and date,
    /// Requested, saved in one minute (so only their number tells them apart), and a request whose nominee's account is
    /// deactivated.
    /// </summary>
    private static List<ActivitySummaryDto> Heavy()
    {
        var since = new DateTime(2026, 9, 25, 6, 10, 0, DateTimeKind.Utc);
        var rows = new List<ActivitySummaryDto>
        {
            Request(15, "Portfolio and Logbook Review (Paediatrics)", "Pieter du Plessis", "patel", "Mohammed Patel", "submitted",
                "Awaiting review", 9, since.AddDays(-1), "PAED-015", new DateOnly(2026, 10, 3)),
            Request(31, "Mini-CEX (Paediatrics)", "Nomsa Mahlangu", "zulu", "Thandi Zulu", "requested", "Requested", 9, since,
                "PAED-003", new DateOnly(2026, 9, 24)),
            Request(32, "Mini-CEX (Paediatrics)", "Nomsa Mahlangu", "patel", "Mohammed Patel", "requested", "Requested", 9,
                since.AddSeconds(20), "PAED-003", new DateOnly(2026, 9, 24)),
            Request(33, "DOPS (Paediatrics)", "Anele Dlamini", "khumalo", "Fatima Khumalo", "requested", "Requested", 9,
                since.AddHours(1), "PAED-006", new DateOnly(2026, 9, 23)) with { CannotRemind = ReminderOutcome.Deactivated }
        };
        for (var id = 40; rows.Count < 21; id++)
        {
            rows.Add(Request(id, "Case-Based Discussion (Paediatrics)", "Sipho Ndlovu", "zulu", "Thandi Zulu", "requested",
                "Requested", 8, since.AddDays(1).AddMinutes(id), "PAED-002", new DateOnly(2026, 9, 1).AddDays(id - 40)));
        }

        rows.Add(Request(2, "Case-Based Discussion (Paediatrics)", "Pieter du Plessis", "khumalo", "Fatima Khumalo",
            "requested", "Requested", 0, ThisMorning, "PAED-002", new DateOnly(2026, 9, 29)));
        return rows;
    }

    private static ActivitySummaryDto Request(
        int id, string type, string subject, string nomineeId, string nominee, string state, string stateLabel, int days,
        DateTime since, string epa, DateOnly observed)
        => ActivityRows.Waiting(id, type, subject, state, stateLabel, days, overdue: days >= 7, since, epa, observed) with
        {
            Holder = new ActivityHolderDto(ActivityHolderKind.Person, nomineeId, nominee, false, since),
            NomineeName = nominee
        };

    private static SendActivityReminderResult Sent()
        => new(ReminderOutcome.Sent, "Thandi Zulu", "Mini-CEX (Paediatrics) · PAED-004 · 2026-10-01", "Nomsa Mahlangu", 8,
            "Requested", null, new ActivityReminderDto(new DateTime(2026, 10, 4, 7, 0, 0, DateTimeKind.Utc), new DateOnly(2026, 10, 4), "Pieter Smit"));

    private static SendActivityReminderResult Refused(ReminderOutcome outcome)
        => outcome == ReminderOutcome.NotFound
            ? SendActivityReminderResult.NotFound
            : new(outcome, outcome == ReminderOutcome.MovedMeanwhile ? null : "Thandi Zulu",
                "Mini-CEX (Paediatrics) · PAED-004 · 2026-10-01", "Nomsa Mahlangu", 8,
                outcome == ReminderOutcome.MovedMeanwhile ? "Completed" : "Requested",
                outcome == ReminderOutcome.MovedMeanwhile ? new DateTime(2026, 10, 4, 6, 12, 0, DateTimeKind.Utc) : null, null);

    /// <summary>Answers the list's query and the reminder's command, keeping each it was sent.</summary>
    private sealed class Sender(
        Func<ListWaitingForAssessorsQuery, Task<WaitingForAssessorsDto?>> list,
        Func<SendActivityReminderCommand, Task<SendActivityReminderResult>>? remind = null) : IScopedSender
    {
        public List<ListWaitingForAssessorsQuery> Queries { get; } = [];

        public List<SendActivityReminderCommand> Commands { get; } = [];

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case ListWaitingForAssessorsQuery query:
                    Queries.Add(query);
                    return (TResponse)(object?)(await list(query))!;
                case SendActivityReminderCommand command:
                    Commands.Add(command);
                    return (TResponse)(object)await (remind ?? (_ => Task.FromResult(Sent())))(command);
                default:
                    throw new NotSupportedException(request.GetType().Name);
            }
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
