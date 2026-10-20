using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Options;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Users;
using Wombat.Application.Features.Invitations;
using Wombat.Domain.Identity;
using Wombat.Domain.Invitations;
using Wombat.Web.Components.Pages.Admin.Invitations;
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T373 (flow 11, boards R3-L01–L17), over T283: the invitations list as the page. Each row's Delivery word with its icon
/// (Q15), a row not delivered first with its note row that its Resend names (L09, L12), the Issued date over its SAST time
/// (S3), "Last day" (F7), Revoke… through a dialog (T264, L06), what a revoke and a resend say (L07, L10), Expired, not used
/// (Q9, L14), loading, a failed read and the T290 note (L15–L17), and the link cleared at every action (M5).
/// </summary>
public sealed class InvitationsListDeliveryTests : TestContext
{
    // 2026-10-19 09:22 SAST.
    private static readonly DateTime Issued = new(2026, 10, 19, 7, 22, 0, DateTimeKind.Utc);
    private static readonly DateOnly LastDay = new(2026, 11, 2);

    private readonly TestAuthorizationContext _auth;

    public InvitationsListDeliveryTests()
    {
        _auth = this.AddTestAuthorization();
        SignIn(WombatRoles.InstitutionalAdmin, new Claim(WombatClaimTypes.InstitutionId, "1"));
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new WombatOptions { BaseUrl = "https://wombat.test" }));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    // ─── L01, L02: none ──────────────────────────────────────────────────────

    [Fact]
    public void WithNone_TheSectionSaysSo_AndTheHeaderOffersInviteAPerson()
    {
        var cut = Render(new FakeSender());

        cut.Find("h1").TextContent.Should().Be("Invitations");
        cut.Find(".page-subtitle").TextContent.Should().Be("Invitations not yet used, with the state of each one's email.");
        var invite = cut.Find(".header-container .actions-cell a");
        invite.TextContent.Trim().Should().Be("Invite a person");
        invite.GetAttribute("href").Should().Be(OnboardingLinks.InviteAPerson);
        cut.Find("h2#active-invitations-heading").TextContent.Trim().Should().Be("Active invitations");
        Text(cut.Find(".detail-card--empty")).Should().Be("No active invitations Issue an invitation to create the first onboarding link.");
        cut.FindAll("#expired-invitations-heading").Should().BeEmpty("Expired, not used is shown only with rows (Q9)");
    }

    // ─── L03, L04: the rows ──────────────────────────────────────────────────

    [Fact]
    public void ARow_IsTheAddress_TheRolesLabelOverItsScope_TheDateOverItsTime_AndItsLastDay()
    {
        var cut = Render(new FakeSender(
            Row(1, "sithole@kgk.wombat.local", WombatRoles.SubSpecialityAdmin, speciality: "Paediatrics", subSpeciality: "Paediatrics"),
            Row(2, "smit@kgk.wombat.local", WombatRoles.Coordinator)));

        cut.Find("table").ClassList.Should().Contain(["clinic-table--stack", "clinic-table--stack-wide"]);
        cut.FindAll("thead th").Select(Text).Should().Equal("Invitee", "Role and scope", "Issued", "Last day", "Delivery", "Actions");
        Text(cut.Find("h2#active-invitations-heading")).Should().Be("Active invitations 2 active");
        cut.Find("caption").TextContent.Should().Be(
            "At Kgosi Kgari Teaching Hospital. Not delivered first, then newest first. A used or revoked invitation leaves this " +
            "list; one that passes its last day unused moves to Expired, not used.");

        var header = cut.FindAll("tbody th[scope=row]").ToList()[0];
        header.InnerHtml.Should().Be("sithole@<wbr>kgk.wombat.local", "the address breaks only after its @ (M7)");
        header.ClassList.Should().NotContain("col-wrap", "check rule 1");
        var cells = cut.FindAll("tbody tr").ToList()[0].QuerySelectorAll("td").ToList();
        Text(cells[0]).Should().Be("Sub-speciality admin Paediatrics / Paediatrics");
        cells[1].ClassList.Should().Contain("inv-when");
        cells[1].InnerHtml.Should().Be("2026-10-19<span>09:22 SAST</span>", "the South African clock, never the server's (S3)");
        Text(cells[2]).Should().Be("2026-11-02");
        Text(cut.FindAll("tbody tr").ToList()[1].QuerySelectorAll("td").First()).Should().Be("Coordinator Any speciality", "Q18");
        cut.Markup.Should().NotContain(WombatRoles.SubSpecialityAdmin, "a role's label, never its key (Q13)");
    }

    [Fact]
    public void TheAdministratorsRows_SayThePlaceToo_AndTheCaptionSaysEveryInstitution()
    {
        SignIn(WombatRoles.Administrator);
        var cut = Render(new FakeSender(
            Row(1, "mbatha@kgk.wombat.local", WombatRoles.InstitutionalAdmin),
            Row(2, "kruger@cmsa.wombat.local", WombatRoles.CollegeAdmin, institution: null, college: "College of Paediatricians of South Africa")));

        cut.Find("caption").TextContent.Should().Be(
            "Every institution and College. Not delivered first, then newest first. A used or revoked invitation leaves this list.");
        cut.FindAll("tbody td[data-label='Role and scope']").Select(Text).Should().Equal(
            "Institutional admin Kgosi Kgari Teaching Hospital",
            "College admin College of Paediatricians of South Africa");
        cut.FindAll("tbody button").Select(button => button.GetAttribute("aria-label")).Should().Equal(
            "Revoke the Institutional admin invitation to mbatha@kgk.wombat.local",
            "Revoke the College admin invitation to kruger@cmsa.wombat.local");
    }

    [Fact]
    public void RowsAlike_AreNamedApartByTheirSouthAfricanTime()
    {
        var cut = Render(new FakeSender(
            Row(1, "molefe@kgk.wombat.local", WombatRoles.Trainee),
            Row(2, "molefe@kgk.wombat.local", WombatRoles.Trainee, issued: Issued.AddHours(1))));

        cut.FindAll("tbody button").Select(button => button.GetAttribute("aria-label")).Should().Equal(
            "Revoke the Trainee invitation to molefe@kgk.wombat.local, issued 2026-10-19 09:22 SAST",
            "Revoke the Trainee invitation to molefe@kgk.wombat.local, issued 2026-10-19 10:22 SAST");
    }

    // ─── Delivery (Q15, L09, L11, L12) ───────────────────────────────────────

    [Fact]
    public void EachRow_SaysWhatBecameOfItsEmail_AsAWordWithItsIcon()
    {
        var cut = Render(new FakeSender(
            Row(1, "sent@kgk.test", delivery: InvitationDelivery.Sent),
            Row(2, "sending@kgk.test", delivery: InvitationDelivery.BeingSent),
            Row(3, "dropped@kgk.test", delivery: InvitationDelivery.NotDelivered, failures: 1),
            Row(4, "check@kgk.test", delivery: InvitationDelivery.NotDelivered, failures: 2, checkAddress: true)));

        Delivery(cut, 1).Should().Be(("Sent", "delivery delivery--sent", "circle-check"));
        Delivery(cut, 2).Should().Be(("Being sent", "delivery delivery--sending", "clock"));
        Delivery(cut, 3).Should().Be(("Not delivered", "delivery delivery--warn", "circle-alert"));
        Delivery(cut, 4).Should().Be(("Check the address", "delivery delivery--warn", "circle-alert"));
        cut.FindAll(".badge").Should().BeEmpty("a state word, no badge (Q15)");
        cut.Find("thead th.col-wrap").TextContent.Should().Be("Delivery");
    }

    [Fact]
    public void ARowNotDelivered_HasANoteRowUnderIt_WhichItsResendNames_AndOffersResendBeforeRevoke()
    {
        var cut = Render(new FakeSender(
            Row(3, "duplessis@kgk.wombat.local", WombatRoles.Trainee, delivery: InvitationDelivery.NotDelivered, failures: 1),
            Row(4, "ndlvou@kgk.wombat.local", WombatRoles.Trainee, delivery: InvitationDelivery.NotDelivered, failures: 2, checkAddress: true),
            Row(5, "sent@kgk.wombat.local", WombatRoles.Trainee, delivery: InvitationDelivery.Sent)));

        var rows = cut.FindAll("tbody tr").ToList();
        rows.Should().HaveCount(5, "two rows with their notes, and one without");
        rows[0].ClassList.Should().Contain("has-note");
        var note = rows[1].QuerySelector("td")!;
        note.ClassList.Should().Contain(["history-note", "history-note--warning"]);
        note.GetAttribute("colspan").Should().Be("6");
        Text(note).Should().Be("Resend emails a new link in place of the current one, which then stops working.");
        Text(rows[3].QuerySelector("td")!).Should().Be(
            "Its email has failed 2 times, so the mail server may be refusing this address. Check the address; if it is " +
            "wrong, revoke this invitation and issue a new one to the right address. Resend emails a new link in place of " +
            "the current one, which then stops working.");

        var buttons = rows[0].QuerySelectorAll(".actions-cell button").ToList();
        buttons.Select(Text).Should().Equal("Resend", "Revoke…");
        buttons[0].GetAttribute("aria-label").Should().Be("Resend the Trainee invitation to duplessis@kgk.wombat.local");
        buttons[0].GetAttribute("aria-describedby").Should().Be("invitation-note-3");
        cut.Find("#invitation-note-3").Should().NotBeNull();
        buttons[0].ClassList.Should().Contain(["btn", "btn-sm", "btn-outline"]);
        buttons[1].ClassList.Should().Contain("btn-outline").And.NotContain("btn-danger", "the red one is the dialog's (T264)");
        rows[4].QuerySelectorAll("button").Select(Text).Should().Equal("Revoke…");
        rows[4].ClassList.Should().NotContain("has-note");
    }

    // ─── Resend (L10) ────────────────────────────────────────────────────────

    [Fact]
    public void AResend_ShowsTheNewLinkOnce_AndTheRowIsBeingSentAgain_WithNoResent()
    {
        var resentAt = Issued.AddMinutes(61);
        var sender = new FakeSender(Row(3, "duplessis@kgk.wombat.local", WombatRoles.Trainee, delivery: InvitationDelivery.NotDelivered, failures: 1))
        {
            AfterResend = [Row(3, "duplessis@kgk.wombat.local", WombatRoles.Trainee, delivery: InvitationDelivery.BeingSent, failures: 1, issued: resentAt)]
        };
        var cut = Render(sender);

        cut.FindAll("button").Single(button => Text(button) == "Resend").Click();

        cut.WaitForAssertion(() => cut.Find(".otl"));
        sender.Resent.Should().Equal(3);
        Text(cut.Find(".otl .alert-success")).Should().Be(
            "A new invitation link is being emailed to duplessis@kgk.wombat.local. The link it replaces no longer works. Copy " +
            "the new link below — it is shown only once.");
        cut.Find(".otl .alert-success strong").InnerHtml.Should().Be(
            "A new invitation link is being emailed to duplessis@<wbr>kgk.wombat.local.");
        cut.Find("#otl-link").GetAttribute("value").Should().Be(
            InvitationLinks.RegistrationUrl(new WombatOptions { BaseUrl = "https://wombat.test" }, "token-new"));
        cut.Find("#otl-link").HasAttribute("readonly").Should().BeTrue();
        Text(cut.Find(".otl-help")).Should().Be("It works until 2026-11-03. It is shown here until your next action, then never again.");
        cut.FindAll(".otl a").Should().BeEmpty("the list does not link to itself");
        cut.Markup.Should().NotContain("Resent", "F2");
        cut.Markup.Should().NotContain("style=", "S20");
        cut.Find("tbody td.inv-when").InnerHtml.Should().Be("2026-10-19<span>10:23 SAST</span>");
        cut.FindAll("button").Should().NotContain(button => Text(button) == "Resend");
    }

    [Fact]
    public void ARefusedResend_IsShownInItsOwnWords_AndTheListIsReadAgain()
    {
        var sender = new FakeSender(Row(3, "dropped@kgk.test", delivery: InvitationDelivery.NotDelivered, failures: 1))
        {
            Refusal = new InvitationIssueRefusedException(
                InvitationIssueRefusal.CannotResend, InvitationIssueField.None, ResendInvitationCommandHandler.InvitationChanged),
            AfterResend = [Row(3, "dropped@kgk.test", delivery: InvitationDelivery.Sent, failures: 1)]
        };
        var cut = Render(sender);

        cut.FindAll("button").Single(button => Text(button) == "Resend").Click();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert-danger")).Should().Be(ResendInvitationCommandHandler.InvitationChanged));
        cut.FindAll(".otl").Should().BeEmpty("no link was made");
        Delivery(cut, 3).Word.Should().Be("Sent");
        // The row no longer offers Resend: the refusal takes the focus.
        cut.WaitForAssertion(() => FocusCalls().Should().ContainSingle());
    }

    [Fact]
    public void AFaultInAResend_IsSaidInFixedWords_NeverItsText()
    {
        var sender = new FakeSender(Row(3, "dropped@kgk.test", delivery: InvitationDelivery.NotDelivered, failures: 1))
        {
            Refusal = new InvalidOperationException("Sequence contains no elements")
        };
        var cut = Render(sender);

        cut.FindAll("button").Single(button => Text(button) == "Resend").Click();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert-danger")).Should().Be(InvitationText.ActionFailed));
        cut.Markup.Should().NotContain("Sequence contains");
    }

    // ─── Revoke (L06, L07) ───────────────────────────────────────────────────

    [Fact]
    public void Revoke_OpensTheDialog_InTheBoardsWords()
    {
        var cut = Render(new FakeSender(Row(5, "ndlvou@kgk.wombat.local", WombatRoles.Trainee, speciality: "Paediatrics", subSpeciality: "Paediatrics")));

        cut.FindAll("tbody button").Single(button => Text(button) == "Revoke…").Click();

        var dialog = cut.Find("dialog");
        Text(dialog.QuerySelector("h2")!).Should().Be("Revoke this invitation?");
        dialog.QuerySelector(".dialog-body")!.QuerySelectorAll("p, li").Select(Text).Should().Equal(
            "Revoke the Trainee invitation to ndlvou@kgk.wombat.local (Paediatrics / Paediatrics)?",
            "The registration link already emailed to that address stops working at once: nobody can register from it.",
            "The invitation leaves this list. An address is never corrected in place: to invite the right one, issue a new invitation.",
            "Revoking cannot be undone.");
        var buttons = dialog.QuerySelectorAll(".form-actions button").ToList();
        buttons.Select(Text).Should().Equal("Keep the invitation", "Revoke invitation");
        buttons[1].ClassList.Should().Contain("btn-danger");
        JSInterop.Invocations.Should().Contain(invocation => invocation.Identifier == "wombatDialog.showModal");
    }

    [Fact]
    public void KeepTheInvitation_ClosesTheDialog_GivingTheFocusBack_AndRevokesNothing()
    {
        var sender = new FakeSender(Row(5, "ndlvou@kgk.wombat.local", WombatRoles.Trainee));
        var cut = Render(sender);

        cut.FindAll("tbody button").Single(button => Text(button) == "Revoke…").Click();
        cut.FindAll("dialog button").Single(button => Text(button) == "Keep the invitation").Click();

        sender.Revoked.Should().BeEmpty();
        JSInterop.Invocations.Should().Contain(invocation =>
            invocation.Identifier == "wombatDialog.close" && invocation.Arguments.Count == 2 && Equals(invocation.Arguments[1], true),
            "the dialog's script gives the focus back to Revoke… (T366, D6)");
    }

    [Fact]
    public void ARevoke_SaysTheLinkNoLongerWorks_AndOffersToInviteAnother_WithNoLink()
    {
        var revoked = Row(5, "ndlvou@kgk.wombat.local", WombatRoles.Trainee, speciality: "Paediatrics", subSpeciality: "Paediatrics");
        var sender = new FakeSender(revoked, Row(6, "molefe@kgk.wombat.local", WombatRoles.Trainee)) { AfterRevoke = [Row(6, "molefe@kgk.wombat.local", WombatRoles.Trainee)] };
        var cut = Render(sender);

        cut.FindAll("tbody button").ToList()[0].Click();
        cut.FindAll("dialog button").Single(button => Text(button) == "Revoke invitation").Click();

        cut.WaitForAssertion(() => cut.Find(".action-result .alert-success"));
        sender.Revoked.Should().Equal(5);
        var result = cut.Find(".action-result .alert-success");
        Text(result.QuerySelector(".alert-row-text")!).Should().Be(
            "Invitation revoked. The link sent to ndlvou@kgk.wombat.local no longer works: nobody can register from it.");
        var again = result.QuerySelector("a")!;
        Text(again).Should().Be("Invite another address as Trainee, Paediatrics / Paediatrics");
        again.GetAttribute("href").Should().Be(OnboardingLinks.InviteFrom(5));
        cut.FindAll(".otl").Should().BeEmpty("M5: no link on the page after a revoke");
        Text(cut.Find("h2#active-invitations-heading")).Should().Be("Active invitations 1 active");
        cut.WaitForAssertion(() => FocusCalls().Should().ContainSingle());
    }

    // ─── M5: the link cleared at every action ────────────────────────────────

    [Fact]
    public void TheResentLink_IsCleared_WhenRevokeIsOpened()
    {
        var cut = RenderWithAResentLink();

        cut.FindAll("tbody button").Single(button => Text(button) == "Revoke…").Click();

        cut.FindAll(".otl").Should().BeEmpty();
    }

    [Fact]
    public void TheResentLink_IsCleared_ByAnotherResend()
    {
        var sender = new FakeSender(
            Row(3, "a@kgk.test", delivery: InvitationDelivery.NotDelivered, failures: 1),
            Row(4, "b@kgk.test", delivery: InvitationDelivery.NotDelivered, failures: 1))
        {
            Refusal = null
        };
        var cut = Render(sender);
        cut.FindAll("button").First(button => Text(button) == "Resend").Click();
        cut.WaitForAssertion(() => cut.Find(".otl"));

        sender.Hold = new TaskCompletionSource();
        cut.FindAll("button").Last(button => Text(button) == "Resend").Click();

        cut.WaitForAssertion(() => cut.FindAll(".otl").Should().BeEmpty("cleared at the start of the next action, before it answers"));
        sender.Hold.SetResult();
    }

    [Fact]
    public void TheResentLink_IsCleared_ByANavigation()
    {
        var cut = RenderWithAResentLink();

        Services.GetRequiredService<NavigationManager>().NavigateTo(OnboardingLinks.InviteFrom(3));

        cut.WaitForAssertion(() => cut.FindAll(".otl").Should().BeEmpty());
    }

    [Theory]
    [InlineData(".header-container .actions-cell a")]
    [InlineData("#expired-invitations-heading ~ * a")]
    public void TheResentLink_IsCleared_ByInviteAgain_AndByInviteAPerson(string selector)
    {
        var cut = RenderWithAResentLink(expired: [Expired(9, "duplessis@kgk.wombat.local")]);

        cut.Find(selector).Click();

        cut.FindAll(".otl").Should().BeEmpty(selector);
    }

    // ─── Expired, not used (L14) ─────────────────────────────────────────────

    [Fact]
    public void ExpiredNotUsed_IsListedWithItsCount_Caption_AndInviteAgain()
    {
        var sender = new FakeSender { Expired = [Expired(9, "duplessis@kgk.wombat.local")] };
        var cut = Render(sender);

        Text(cut.Find("h2#expired-invitations-heading")).Should().Be("Expired, not used 1 expired");
        var table = cut.Find("#expired-invitations-heading").ParentElement!.QuerySelector("table")!;
        Text(table.QuerySelector("caption")!).Should().Be(
            "Past their last day unused, in the last 30 days on the South African calendar, newest first. Each leaves this " +
            "list 30 days after its last day, or once a new invitation goes to its address.");
        table.QuerySelectorAll("thead th").Select(Text).Should().Equal("Invitee", "Role and scope", "Issued", "Last day", "Actions");
        var again = table.QuerySelector("tbody a")!;
        Text(again).Should().Be("Invite again");
        // The visible label first in its name (WCAG 2.5.3, RowActionMarkupTests), where L14 wrote "Invite ‹address› again as …".
        again.GetAttribute("aria-label").Should().Be("Invite again duplessis@kgk.wombat.local as Trainee, Paediatrics / Paediatrics");
        again.GetAttribute("href").Should().Be(OnboardingLinks.InviteFrom(9));
        Text(cut.Find(".detail-card--empty")).Should().StartWith("No active invitations");
    }

    // ─── L15, L16, L17 ───────────────────────────────────────────────────────

    [Fact]
    public void WhileItLoads_ItSaysWhat_WithASkeletonOfClasses_AndOffersNoAction()
    {
        var sender = new FakeSender { Hold = new TaskCompletionSource() };
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<InvitationsList>();

        var line = cut.Find("p.loading-line");
        line.TextContent.Should().Be("Loading the invitations.");
        line.GetAttribute("role").Should().Be("status");
        cut.FindAll("div.skeleton").Should().HaveCount(4);
        cut.Markup.Should().NotContain("style=", "S20: no Skeleton's inline sizes");
        cut.FindAll(".header-container .actions-cell").Should().BeEmpty("L15: no action while loading");
        sender.Hold.SetResult();
    }

    /// <summary>T373 fix pass, P2: a Try again that loads hands the focus to Active invitations' heading, not the body.</summary>
    [Fact]
    public void ATryAgainThatLoads_FocusesActiveInvitations()
    {
        var sender = new FakeSender { ReadFailure = new InvalidOperationException("connection reset") };
        var cut = Render(sender);
        sender.ReadFailure = null;

        cut.Find(".action-result .alert-danger button").Click();

        cut.WaitForAssertion(() => JSInterop.Invocations
            .Where(invocation => invocation.Identifier == PageFocus.FocusByIdIdentifier)
            .Select(invocation => invocation.Arguments[0]?.ToString())
            .Should().Contain("active-invitations-heading"));
    }

    [Fact]
    public void AFailedRead_SaysSoInFixedWords_WithTryAgain_WhichTakesTheFocusWhenItFailsAgain()
    {
        var sender = new FakeSender { ReadFailure = new InvalidOperationException("connection reset") };
        var cut = Render(sender);

        Text(cut.Find(".action-result .alert-danger .alert-row-text")).Should().Be(
            "Could not load the invitations. Nothing has changed. Try again, or come back in a few minutes.");
        cut.Markup.Should().NotContain("connection reset");
        cut.FindAll(".header-container .actions-cell").Should().BeEmpty();

        cut.Find(".action-result .alert-danger button").Click();

        cut.WaitForAssertion(() => FocusCalls().Should().NotBeEmpty("M10: a failed retry's alert takes the focus"));
    }

    [Fact]
    public void AHolderOfTrainee_SeesTheHeaderAndTheNote_AndNothingIsRead()
    {
        _auth.SetRoles(WombatRoles.InstitutionalAdmin, WombatRoles.Trainee);
        var sender = new FakeSender(Row(1, "a@kgk.test"));
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<InvitationsList>();

        Text(cut.Find(".alert.alert-info")).Should().Be(UserAdministrationRules.TraineeAdministersNoUser);
        cut.FindAll(".alert[role]").Should().BeEmpty("P9: the note is an info alert with no role, on all four pages");
        cut.Find("h1").TextContent.Should().Be("Invitations");
        cut.FindAll("table, .header-container .actions-cell, .skeleton").Should().BeEmpty();
        sender.Reads.Should().Be(0, "M1: nothing is read before the note");
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private IRenderedComponent<InvitationsList> RenderWithAResentLink(IReadOnlyList<ExpiredInvitationDto>? expired = null)
    {
        var sender = new FakeSender(Row(3, "duplessis@kgk.wombat.local", delivery: InvitationDelivery.NotDelivered, failures: 1))
        {
            Expired = expired ?? []
        };
        var cut = Render(sender);
        cut.FindAll("button").Single(button => Text(button) == "Resend").Click();
        cut.WaitForAssertion(() => cut.Find(".otl"));
        return cut;
    }

    private IRenderedComponent<InvitationsList> Render(FakeSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<InvitationsList>();
        cut.WaitForState(() => cut.FindAll(".skeleton").Count == 0);
        return cut;
    }

    private void SignIn(string role, params Claim[] claims)
    {
        _auth.SetAuthorized("caller@test");
        _auth.SetRoles(role);
        _auth.SetClaims([new Claim(ClaimTypes.NameIdentifier, "admin-1"), .. claims]);
    }

    private IReadOnlyList<JSRuntimeInvocation> FocusCalls()
        => JSInterop.Invocations.Where(invocation => invocation.Identifier == "Blazor._internal.domWrapper.focus").ToList();

    private static (string Word, string Class, string Icon) Delivery(IRenderedFragment cut, int invitationId)
    {
        var cell = cut.Find($"#invitation-delivery-{invitationId}");
        var icon = cell.QuerySelector("use")!.GetAttribute("href")!;
        return (Text(cell), cell.GetAttribute("class")!, Regex.Match(icon, @"/icons/(.+)\.svg").Groups[1].Value);
    }

    private static ActiveInvitationDto Row(
        int id,
        string email,
        string role = WombatRoles.Coordinator,
        InvitationDelivery delivery = InvitationDelivery.BeingSent,
        int failures = 0,
        bool checkAddress = false,
        string? institution = "Kgosi Kgari Teaching Hospital",
        string? college = null,
        string? speciality = null,
        string? subSpeciality = null,
        DateTime? issued = null)
        => new(id, email, role, institution is null ? null : 1, institution, college is null ? null : 7, college,
            speciality is null ? null : 5, speciality, subSpeciality is null ? null : 6, subSpeciality,
            issued ?? Issued, LastDay, delivery, failures, checkAddress);

    private static ExpiredInvitationDto Expired(int id, string email)
        => new(id, email, WombatRoles.Trainee, 1, "Kgosi Kgari Teaching Hospital", null, null, 5, "Paediatrics", 6, "Paediatrics",
            Issued, LastDay);

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private sealed class FakeSender(params ActiveInvitationDto[] rows) : IScopedSender
    {
        private IReadOnlyList<ActiveInvitationDto> _rows = rows;

        public IReadOnlyList<ExpiredInvitationDto> Expired { get; init; } = [];
        public IReadOnlyList<ActiveInvitationDto>? AfterResend { get; init; }
        public IReadOnlyList<ActiveInvitationDto>? AfterRevoke { get; init; }
        public Exception? Refusal { get; set; }
        public Exception? ReadFailure { get; set; }

        /// <summary>When set, every request waits for it.</summary>
        public TaskCompletionSource? Hold { get; set; }

        public List<int> Resent { get; } = [];
        public List<int> Revoked { get; } = [];
        public int Reads { get; private set; }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (Hold is { } hold)
            {
                await hold.Task;
            }

            object? answer = request switch
            {
                ListActiveInvitationsQuery => Read(_rows),
                ListExpiredUnusedInvitationsQuery => Read(Expired),
                ResendInvitationCommand resend => Resend(resend),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return (TResponse)answer!;
        }

        public async Task Send(IRequest request, CancellationToken cancellationToken = default)
        {
            if (Hold is { } hold)
            {
                await hold.Task;
            }

            if (request is RevokeInvitationCommand revoke)
            {
                Revoked.Add(revoke.InvitationId);
                _rows = AfterRevoke ?? _rows.Where(row => row.Id != revoke.InvitationId).ToList();
                return;
            }

            throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
        }

        private object Read(object rowsRead)
        {
            Reads++;
            if (ReadFailure is not null)
            {
                throw ReadFailure;
            }

            return rowsRead;
        }

        private IssuedInvitationResult Resend(ResendInvitationCommand command)
        {
            Resent.Add(command.InvitationId);
            _rows = AfterResend ?? _rows;
            if (Refusal is { } refusal)
            {
                throw refusal;
            }

            return new IssuedInvitationResult(command.InvitationId, "token-new") { ExpiresOn = new DateOnly(2026, 11, 3) };
        }
    }
}
