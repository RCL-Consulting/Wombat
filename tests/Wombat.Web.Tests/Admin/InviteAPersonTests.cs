using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Options;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Colleges;
using Wombat.Application.Features.Institutions;
using Wombat.Application.Features.Institutions.Queries.GetInstitutionsList;
using Wombat.Application.Features.Institutions.Queries.GetSpecialitiesList;
using Wombat.Application.Features.Institutions.Queries.GetSubSpecialitiesList;
using Wombat.Application.Features.Invitations;
using Wombat.Application.Features.Users;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Invitations;
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T373 (flow 11, Q1, boards R3-N01–N26): Invite a person, the issue form on a page of its own. Only the chosen role's
/// fields, never a disabled one (S15); the required marks (M11); each refusal under its field, marked, described and focused
/// (Q3); the OneTimeLink and both copy answers (S10); the form again under "Invite the next person"; the link cleared at the
/// next action, refusal or navigation (M5); the two prefills (N19, N20); nothing adopted (N26); loading, a failed read and
/// the T290 note (N22–N24).
/// </summary>
public sealed class InviteAPersonTests : TestContext
{
    private static readonly DateTime Created = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly TestAuthorizationContext _auth;

    public InviteAPersonTests()
    {
        _auth = this.AddTestAuthorization();
        SignIn(WombatRoles.InstitutionalAdmin, new Claim(WombatClaimTypes.InstitutionId, "1"));
        Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(new WombatOptions { BaseUrl = "https://wombat.test" }));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    // ─── N01: the empty form ─────────────────────────────────────────────────

    [Fact]
    public void TheForm_OffersSevenRolesByLabel_NoneChosen_TheInstitutionReadOut_AndTheMarks()
    {
        var cut = Render(new FakeSender());

        cut.Find("h1").TextContent.Should().Be("Invite a person");
        cut.Find(".page-subtitle").TextContent.Should().Be("One person, one role. The registration link is shown here once, and emailed to them.");
        cut.Find(".inv-page").ClassList.Should().Contain("inv-page--form");
        cut.Find("form").HasAttribute("aria-label").Should().BeFalse("no form name repeating the h1");
        cut.Find("form").HasAttribute("aria-labelledby").Should().BeFalse();
        cut.FindAll("form h2").Should().BeEmpty();
        Text(cut.Find("form > p.field-help")).Should().Be("* marks a field you must fill in.");

        var email = cut.Find("#invitation-email");
        email.GetAttribute("type").Should().Be("email");
        email.GetAttribute("aria-required").Should().Be("true");
        cut.Find("label[for=invitation-email] .required-mark").GetAttribute("aria-hidden").Should().Be("true");

        var legend = cut.Find("fieldset#invitation-role legend");
        Text(legend).Should().Be("Role * required");
        legend.QuerySelector(".visually-hidden")!.TextContent.Should().Be(" required");
        cut.Find("fieldset#invitation-role").ClassList.Should().Contain(["form-group", "choice-row"]);

        var radios = cut.FindAll("input[type=radio]").ToList();
        radios.Should().HaveCount(7, "College admin is the Administrator's alone");
        radios.Should().OnlyContain(radio => !radio.HasAttribute("checked"));
        cut.FindAll(".review-type-choice label").Select(Text).Should().Equal(
            "Institutional admin", "Speciality admin", "Sub-speciality admin", "Coordinator", "Committee member", "Assessor", "Trainee");
        var trainee = cut.Find("#role-trainee");
        Text(cut.Find($"#{trainee.GetAttribute("aria-describedby")}")).Should().Be(
            "A registrar. Registers as Pending trainee, and waits to be admitted under Trainees.");
        cut.Find("label[for=role-trainee]").QuerySelector("p").Should().BeNull("named by the label alone (S17)");

        Text(cut.Find("dl.form-group dt")).Should().Be("Institution");
        cut.Find("dl.form-group dd").ClassList.Should().Contain("readonly-value");
        Text(cut.Find("dl.form-group dd")).Should().Be("Kgosi Kgari Teaching Hospital");

        var actions = cut.Find(".form-actions");
        Text(actions.QuerySelector("a")!).Should().Be("Cancel");
        actions.QuerySelector("a")!.GetAttribute("href").Should().Be(OnboardingLinks.Invitations);
        Text(actions.QuerySelector("button[type=submit]")!).Should().Be("Issue invitation");
        cut.FindAll("[disabled]").Should().BeEmpty("a field the role does not take is not drawn (S15)");
        cut.Markup.Should().NotContain("(optional)").And.NotContain("style=");
    }

    // ─── Each role's fields ──────────────────────────────────────────────────

    [Fact]
    public void TheAdministrator_IsOfferedCollegeAdmin_WhichTakesACollegeOnly()
    {
        SignIn(WombatRoles.Administrator);
        var cut = Render(new FakeSender());

        cut.FindAll("input[type=radio]").Should().HaveCount(8);
        cut.FindAll("#invitation-institution, dl.form-group").Should().BeEmpty("no role chosen, no institution to choose yet");

        cut.Find("#role-college-admin").Change(true);

        cut.Find("label[for=invitation-college]").TextContent.Should().Be("College *");
        cut.Find("#invitation-college").GetAttribute("aria-required").Should().Be("true");
        cut.FindAll("#invitation-college option").Select(Text).Should().Equal("Choose a College", "College of Paediatricians of South Africa");
        cut.FindAll("#invitation-institution, #invitation-speciality").Should().BeEmpty();
    }

    [Fact]
    public void TheAdministrator_ChoosesTheInstitution_ForAnInstitutionalAdmin_WithNoSpeciality()
    {
        SignIn(WombatRoles.Administrator);
        var cut = Render(new FakeSender());

        cut.Find("#role-institutional-admin").Change(true);

        cut.Find("label[for=invitation-institution]").TextContent.Should().Be("Institution *");
        cut.FindAll("#invitation-institution option").Select(Text).Should().Equal(
            "Choose an institution", "Demo Institution", "Kgosi Kgari Teaching Hospital");
        cut.FindAll("#invitation-speciality, #invitation-sub-speciality").Should().BeEmpty();
    }

    [Fact]
    public void ACoordinator_MayBeKeptToASpeciality_LeftAtAnySpeciality_WithNoStar()
    {
        var cut = Render(new FakeSender());

        cut.Find("#role-coordinator").Change(true);

        Text(cut.Find("label[for=invitation-speciality]")).Should().Be("Speciality");
        cut.Find("#invitation-speciality").HasAttribute("aria-required").Should().BeFalse();
        cut.FindAll("#invitation-speciality option").Select(Text).Should().Equal("Any speciality", "Paediatrics");
        cut.Find("#invitation-speciality").GetAttribute("aria-describedby").Should().Be("invitation-speciality-help");
        Text(cut.Find("#invitation-speciality-help")).Should().Be("Leave it at Any speciality for the whole institution.");
        cut.FindAll("#invitation-sub-speciality").Should().BeEmpty();
    }

    [Fact]
    public void AnAssessor_ChoosesTheSpeciality_ThenItsSubSpeciality_BothStarred_AndAChangeOfSpecialityClearsIt()
    {
        var cut = Render(new FakeSender());

        cut.Find("#role-assessor").Change(true);

        Text(cut.Find("label[for=invitation-speciality]")).Should().Be("Speciality *");
        Text(cut.Find("#invitation-speciality-help")).Should().Be("Then choose its sub-speciality.");
        cut.FindAll("#invitation-sub-speciality").Should().BeEmpty("N08a: only once a speciality is chosen");

        cut.Find("#invitation-speciality").Change("5");

        // R4 (R3-0-steps 2.4): the hint stays once a speciality is chosen, until its sub-speciality is.
        Text(cut.Find("#invitation-speciality-help")).Should().Be("Then choose its sub-speciality.");
        Text(cut.Find("label[for=invitation-sub-speciality]")).Should().Be("Sub-speciality *");
        cut.FindAll("#invitation-sub-speciality option").Select(Text).Should().Equal("Choose a sub-speciality", "Paediatrics");
        cut.Find("#invitation-sub-speciality").Change("6");

        cut.FindAll("#invitation-speciality-help").Should().BeEmpty("the sub-speciality is chosen");

        cut.Find("#invitation-speciality").Change("");

        cut.FindAll("#invitation-sub-speciality").Should().BeEmpty();
    }

    [Fact]
    public void TheIssue_SendsOnlyTheFieldsTheRoleTakes()
    {
        var sender = new FakeSender();
        var cut = Render(sender);
        cut.Find("#role-assessor").Change(true);
        cut.Find("#invitation-speciality").Change("5");
        cut.Find("#invitation-sub-speciality").Change("6");
        cut.Find("#role-coordinator").Change(true);
        cut.Find("#invitation-email").Change("smit@kgk.wombat.local");

        cut.Find("form").Submit();

        cut.WaitForAssertion(() => sender.Issued.Should().ContainSingle());
        var command = sender.Issued.Single();
        command.TargetRole.Should().Be(WombatRoles.Coordinator);
        command.InstitutionId.Should().Be(1);
        command.SpecialityId.Should().Be(5, "a Coordinator may be kept to a speciality");
        command.SubSpecialityId.Should().BeNull("a Coordinator takes no sub-speciality, so its field was not drawn");
        command.CollegeId.Should().BeNull();
    }

    // ─── N26: nothing adopted ────────────────────────────────────────────────

    [Fact]
    public void WithNothingAdopted_ARoleThatNeedsASpeciality_IsToldWhere_AndIssueStays()
    {
        var cut = Render(new FakeSender { Specialities = [] });

        cut.Find("#role-assessor").Change(true);

        var warning = cut.Find("p.field-warning");
        Text(warning).Should().Be(
            "Nothing is adopted at Kgosi Kgari Teaching Hospital yet, so roles that need a speciality cannot be invited. Adopt a " +
            "curriculum under Curriculum adoptions.");
        warning.QuerySelector("a")!.GetAttribute("href").Should().Be(OnboardingLinks.CurriculumAdoptions);
        cut.FindAll("#invitation-speciality").Should().BeEmpty();
        cut.Find("button[type=submit]").HasAttribute("disabled").Should().BeFalse();
    }

    // ─── The page's own refusals (N13, N14, N25) ─────────────────────────────

    [Fact]
    public void AnEmptyAddress_IsRefusedUnderEmail_InWombatsWords_AndEmailTakesTheFocus()
    {
        var sender = new FakeSender();
        var cut = Render(sender);
        cut.Find("#role-coordinator").Change(true);

        cut.Find("form").Submit();

        ShouldBeRefusedUnder(cut, "invitation-email", "Enter the person's email address.");
        cut.WaitForAssertion(() => Focused().Should().Contain("invitation-email"));
        sender.Issued.Should().BeEmpty();
    }

    [Fact]
    public void AMalformedAddress_IsRefusedUnderEmail()
    {
        var cut = Render(new FakeSender());
        cut.Find("#role-coordinator").Change(true);
        cut.Find("#invitation-email").Change("smit.kgk.wombat.local");

        cut.Find("form").Submit();

        ShouldBeRefusedUnder(cut, "invitation-email", "Enter a valid email address, for example name@example.org.");
    }

    [Fact]
    public void NoRole_IsRefusedUnderTheLegend_TheGroupMarked_AndTheFirstRadioFocused()
    {
        var cut = Render(new FakeSender());
        cut.Find("#invitation-email").Change("smit@kgk.wombat.local");

        cut.Find("form").Submit();

        var message = cut.Find("#invitation-role-msg");
        Text(message).Should().Be("Choose a role.");
        message.ClassList.Should().Contain("validation-message");
        message.PreviousElementSibling!.TagName.Should().Be("LEGEND");
        cut.Find("fieldset#invitation-role").GetAttribute("aria-describedby").Should().Be("invitation-role-msg");
        cut.Find(".choice-group").ClassList.Should().Contain("is-invalid");
        cut.WaitForAssertion(() => Focused().Should().Contain("role-institutional-admin"));
    }

    // ─── The command's refusals under their fields (N02, N10, N11, N15–N18) ──

    [Theory]
    [InlineData(WombatRoles.SpecialityAdmin, InvitationIssueField.Speciality, "invitation-speciality", "Speciality administrators must be scoped to a speciality.")]
    [InlineData(WombatRoles.SubSpecialityAdmin, InvitationIssueField.SubSpeciality, "invitation-sub-speciality", "The selected role requires speciality and sub-speciality scope.")]
    public void AScopeRefusal_SitsUnderItsField_FocusedWithNoLink(string role, InvitationIssueField field, string fieldId, string words)
    {
        var sender = new FakeSender { Refusal = new InvitationIssueRefusedException(InvitationIssueRefusal.Scope, field, words) };
        var cut = Render(sender);
        cut.Find("#invitation-email").Change("mokoena@kgk.wombat.local");
        cut.Find($"#{InvitationText.RoleInputId(role)}").Change(true);
        if (field == InvitationIssueField.SubSpeciality)
        {
            cut.Find("#invitation-speciality").Change("5");
        }

        cut.Find("form").Submit();

        ShouldBeRefusedUnder(cut, fieldId, words);
        cut.WaitForAssertion(() => Focused().Should().Contain(fieldId));
        cut.FindAll(".otl").Should().BeEmpty("M5");
        cut.FindAll(".validation-summary-errors").Should().BeEmpty("never a summary (Q3)");
    }

    [Fact]
    public void ACollegeRefusal_SitsUnderCollege()
    {
        SignIn(WombatRoles.Administrator);
        var sender = new FakeSender
        {
            Refusal = new InvitationIssueRefusedException(InvitationIssueRefusal.Scope, InvitationIssueField.College,
                "A college is required for a college administrator.")
        };
        var cut = Render(sender);
        cut.Find("#invitation-email").Change("kruger@cmsa.wombat.local");
        cut.Find("#role-college-admin").Change(true);

        cut.Find("form").Submit();

        ShouldBeRefusedUnder(cut, "invitation-college", "A college is required for a college administrator.");
    }

    [Fact]
    public void AnAccountInScope_IsNamedUnderEmail_WithOneLinkToIt()
    {
        var cut = RefusedOverTheAddress(new InvitationIssueRefusedException(
            InvitationIssueRefusal.AccountExists, InvitationIssueField.Email, InvitationIssueRefusedException.AddressInUse)
        {
            InCallerScope = true, HolderUserId = "smit-1", HolderName = "Pieter Smit"
        });

        var message = ShouldBeRefusedUnder(cut, "invitation-email",
            "smit@kgk.wombat.local already has a Wombat account (Pieter Smit), so it cannot be invited. To give Pieter Smit " +
            "another role, open Pieter Smit under Users.");
        message.QuerySelectorAll("a").Should().ContainSingle().Which.GetAttribute("href").Should().Be(OnboardingLinks.User("smit-1"));
        message.InnerHtml.Should().StartWith("smit@<wbr>kgk.wombat.local");
        cut.WaitForAssertion(() => Focused().Should().Contain("invitation-email"));
    }

    /// <summary>
    /// T373 fix pass, B6: an account in scope with no name (an erased or nameless one) is named by nothing, never "open  under
    /// Users": the sentence and its link speak of the account.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AnAccountInScope_WithNoName_IsTheAccount_NeverABlank(string name)
    {
        var cut = RefusedOverTheAddress(new InvitationIssueRefusedException(
            InvitationIssueRefusal.AccountExists, InvitationIssueField.Email, InvitationIssueRefusedException.AddressInUse)
        {
            InCallerScope = true, HolderUserId = "smit-1", HolderName = name
        });

        var message = ShouldBeRefusedUnder(cut, "invitation-email",
            "smit@kgk.wombat.local already has a Wombat account, so it cannot be invited. To give the account another role, " +
            "open it under Users.");
        message.QuerySelectorAll("a").Should().ContainSingle().Which.GetAttribute("href").Should().Be(OnboardingLinks.User("smit-1"));
    }

    [Fact]
    public void AnAccountElsewhere_IsRefusedNamingNobody()
    {
        var cut = RefusedOverTheAddress(new InvitationIssueRefusedException(
            InvitationIssueRefusal.AccountExists, InvitationIssueField.Email, InvitationIssueRefusedException.AddressInUse));

        var message = ShouldBeRefusedUnder(cut, "invitation-email",
            "An account with this address already exists. Ask a Wombat administrator to add the role.");
        message.QuerySelectorAll("a").Should().BeEmpty();
    }

    [Fact]
    public void AnActiveInvitationInScope_SaysWhenItWasIssued_AndUntilWhenItWorks()
    {
        var cut = RefusedOverTheAddress(new InvitationIssueRefusedException(
            InvitationIssueRefusal.ActiveInvitationExists, InvitationIssueField.Email, InvitationIssueRefusedException.AddressInUse)
        {
            InCallerScope = true,
            ActiveIssuedOn = new DateTime(2026, 10, 19, 7, 22, 0, DateTimeKind.Utc),
            ActiveExpiresOn = new DateOnly(2026, 11, 2)
        });

        var message = ShouldBeRefusedUnder(cut, "invitation-email",
            "smit@kgk.wombat.local already has an active invitation, issued 2026-10-19 09:22 SAST, which works until " +
            "2026-11-02. Resend it or revoke it under Invitations.");
        message.QuerySelector("a")!.GetAttribute("href").Should().Be(OnboardingLinks.Invitations);
    }

    [Theory]
    [InlineData(false, "This address already has an active invitation from another institution. Ask a Wombat administrator.")]
    [InlineData(true, "This address already has an active invitation from a College. Ask a Wombat administrator.")]
    public void AnActiveInvitationElsewhere_SaysOnlyThatItIsAnothers(bool college, string words)
    {
        var cut = RefusedOverTheAddress(new InvitationIssueRefusedException(
            InvitationIssueRefusal.ActiveInvitationExists, InvitationIssueField.Email, InvitationIssueRefusedException.AddressInUse)
        {
            HeldByCollege = college
        });

        ShouldBeRefusedUnder(cut, "invitation-email", words).QuerySelectorAll("a").Should().BeEmpty();
    }

    [Fact]
    public void AFaultInTheIssue_IsSaidInFixedWords_NeverItsText()
    {
        var cut = RefusedOverTheAddress(new InvalidOperationException("duplicate key value violates unique constraint"));

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert-danger")).Should().Be(InvitationText.IssueFailed));
        cut.Markup.Should().NotContain("duplicate key");
    }

    // ─── Issued: the OneTimeLink (N03, N06, N06b, N09, N21) ──────────────────

    [Fact]
    public void AnIssue_ShowsTheOneTimeLink_Focused_AndTheFormAgainUnderInviteTheNextPerson()
    {
        var cut = Issue(new FakeSender(), "smit@kgk.wombat.local");

        var result = cut.Find(".action-result .otl");
        Text(result.QuerySelector(".alert-success")!).Should().Be(
            "Invitation issued for smit@kgk.wombat.local. Its email is being sent. Copy the link below — it is shown only once.");
        result.QuerySelector(".alert-success strong")!.InnerHtml.Should().Be("Invitation issued for smit@<wbr>kgk.wombat.local.");
        var label = result.QuerySelector("label")!;
        label.GetAttribute("for").Should().Be("otl-link");
        label.InnerHtml.Should().Be("Registration link for smit@<wbr>kgk.wombat.local");
        var link = cut.Find("#otl-link");
        link.GetAttribute("value").Should().Be(InvitationLinks.RegistrationUrl(new WombatOptions { BaseUrl = "https://wombat.test" }, "token-issued"),
            "the real link, the one the mail carries (InvitationLinks)");
        link.HasAttribute("readonly").Should().BeTrue();
        link.GetAttribute("aria-describedby").Should().Be("otl-help otl-said");
        cut.Find("#otl-said").GetAttribute("aria-live").Should().Be("polite");
        Text(cut.Find("#otl-help")).Should().Be(
            "It works until 2026-11-02. It is shown here until your next action, then never again. If the email is not " +
            "delivered, share this link another way, or resend it from Invitations, which emails a new link in place of this one.");
        var open = result.QuerySelectorAll("a").Single();
        Text(open).Should().Be("Open Invitations");
        open.GetAttribute("href").Should().Be(OnboardingLinks.Invitations);

        Text(cut.Find("form h2")).Should().Be("Invite the next person");
        cut.Find("form").GetAttribute("aria-labelledby").Should().Be(cut.Find("form h2").Id);
        cut.Find("#invitation-email").GetAttribute("value").Should().BeNullOrEmpty();
        cut.FindAll("input[type=radio]").Should().OnlyContain(radio => !radio.HasAttribute("checked"));

        cut.WaitForAssertion(() => FocusCalls().Should().Contain(call =>
            call.Identifier == "Blazor._internal.domWrapper.focus" &&
            ((ElementReference)call.Arguments[0]!).Id == cut.FindComponent<ActionResult>().Instance.Element.Id));
    }

    [Fact]
    public void CopyLink_SaysLinkCopied_AndTheButtonCopied_WhenTheBrowserTookIt()
    {
        JSInterop.Setup<bool>("wombat.copyText",
            InvitationLinks.RegistrationUrl(new WombatOptions { BaseUrl = "https://wombat.test" }, "token-issued")).SetResult(true);
        var cut = Issue(new FakeSender(), "smit@kgk.wombat.local");

        cut.FindAll(".otl-row button").Single().Click();

        Text(cut.Find("#otl-said")).Should().Be("Link copied.");
        Text(cut.Find(".otl-row button")).Should().Be("Copied");
        cut.Find(".otl-row button").GetAttribute("aria-describedby").Should().Be("otl-said");
        cut.Find(".otl").Should().NotBeNull("a copy is not an action that clears the link");
    }

    [Fact]
    public void CopyLink_SaysToCopyItByHand_WhenTheBrowserRefused()
    {
        JSInterop.Setup<bool>("wombat.copyText", _ => true).SetResult(false);
        var cut = Issue(new FakeSender(), "smit@kgk.wombat.local");

        cut.Find(".otl-row button").Click();

        Text(cut.Find("#otl-said")).Should().Be("Could not copy. Select the link and copy it yourself.");
        Text(cut.Find(".otl-row button")).Should().Be("Copy link");
    }

    [Fact]
    public void ALongAddress_BreaksAfterItsAt_InTheResultAndTheLabel()
    {
        var address = new string('x', 47) + "@kgk.wombat.local";
        var cut = Issue(new FakeSender(), address);

        cut.Find(".otl label").InnerHtml.Should().Contain("@<wbr>kgk.wombat.local");
        cut.Find(".otl strong").InnerHtml.Should().Contain("@<wbr>kgk.wombat.local");
    }

    // ─── M5: the link cleared ────────────────────────────────────────────────

    [Fact]
    public void TheLink_IsCleared_AtTheStartOfTheNextIssue()
    {
        var sender = new FakeSender();
        var cut = Issue(sender, "smit@kgk.wombat.local");
        sender.Hold = new TaskCompletionSource<IssuedInvitationResult>();
        cut.Find("#role-coordinator").Change(true);
        cut.Find("#invitation-email").Change("botha@kgk.wombat.local");

        cut.Find("form").Submit();

        cut.WaitForAssertion(() => cut.FindAll(".otl").Should().BeEmpty("cleared before the issue answers"));
        sender.Hold.SetResult(new IssuedInvitationResult(10, "token-2") { ExpiresOn = new DateOnly(2026, 11, 2) });
        cut.WaitForAssertion(() => cut.Find("#otl-link").GetAttribute("value").Should().EndWith("token-2"));
    }

    [Fact]
    public void TheLink_IsCleared_ByTheNextIssuesRefusal_OrItsOwnChecks()
    {
        var sender = new FakeSender();
        var cut = Issue(sender, "smit@kgk.wombat.local");

        cut.Find("form").Submit();

        cut.FindAll(".otl").Should().BeEmpty();
        Text(cut.Find("#invitation-email-msg")).Should().Be("Enter the person's email address.");
    }

    [Fact]
    public void TheLink_IsCleared_ByANavigation()
    {
        var cut = Issue(new FakeSender(), "smit@kgk.wombat.local");

        Services.GetRequiredService<NavigationManager>().NavigateTo(OnboardingLinks.Invitations);

        cut.WaitForAssertion(() => cut.FindAll(".otl").Should().BeEmpty());
    }

    // ─── N19, N20: filled from an invitation ─────────────────────────────────

    [Fact]
    public void FromARevokedInvitation_TheRoleAndScopeAreFilled_TheAddressLeftEmptyAndFocused()
    {
        var sender = new FakeSender
        {
            Prefill = new InvitationPrefillDto(5, InvitationPrefillSource.Revoked, "ndlvou@kgk.wombat.local", WombatRoles.Trainee, 1, null, 5, 6,
                new DateOnly(2026, 11, 2))
        };
        var cut = Render(sender, from: 5);

        sender.PrefillAsked.Should().Equal(5);
        var notice = cut.Find("form .alert-info");
        notice.GetAttribute("role").Should().Be("status");
        Text(notice).Should().Be("Role and scope filled from the revoked invitation to ndlvou@kgk.wombat.local. Type the right address.");
        notice.InnerHtml.Should().Contain("ndlvou@<wbr>kgk.wombat.local");
        cut.Find("#role-trainee").HasAttribute("checked").Should().BeTrue();
        cut.Find("#invitation-speciality").GetAttribute("value").Should().Be("5");
        cut.Find("#invitation-sub-speciality").GetAttribute("value").Should().Be("6");
        cut.Find("#invitation-email").GetAttribute("value").Should().BeNullOrEmpty();
        cut.WaitForAssertion(() => Focused().Should().Contain("invitation-email"));
    }

    [Fact]
    public void FromAnExpiredInvitation_AllIsFilled_AndTheNoticeSaysItsLastDay()
    {
        var sender = new FakeSender
        {
            Prefill = new InvitationPrefillDto(9, InvitationPrefillSource.Expired, "duplessis@kgk.wombat.local", WombatRoles.Trainee, 1, null, 5, 6,
                new DateOnly(2026, 11, 2))
        };
        var cut = Render(sender, from: 9);

        Text(cut.Find("form .alert-info")).Should().Be(
            "Filled from the invitation to duplessis@kgk.wombat.local, whose last day was 2026-11-02. Check it, then issue.");
        cut.Find("#invitation-email").GetAttribute("value").Should().Be("duplessis@kgk.wombat.local");
    }

    [Fact]
    public void FromAnInvitationNotTheCallers_NothingIsFilled_AndNothingIsSaid()
    {
        var sender = new FakeSender { Prefill = null };
        var cut = Render(sender, from: 77);

        sender.PrefillAsked.Should().Equal(77);
        cut.FindAll("form .alert-info").Should().BeEmpty();
        cut.FindAll("input[type=radio]").Should().OnlyContain(radio => !radio.HasAttribute("checked"));
    }

    // ─── N22, N23, N24 ───────────────────────────────────────────────────────

    [Fact]
    public void WhileItLoads_ItSaysWhat()
    {
        var sender = new FakeSender { ReadHold = new TaskCompletionSource() };
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<InviteAPerson>();

        Text(cut.Find("p.loading-line[role=status]")).Should().Be("Loading what you can invite.");
        cut.FindAll("div.skeleton").Should().HaveCount(4);
        cut.FindAll("form").Should().BeEmpty();
        sender.ReadHold.SetResult();
    }

    [Fact]
    public void AFailedRead_SaysSoInFixedWords_WithTryAgain()
    {
        var cut = Render(new FakeSender { ReadFailure = new InvalidOperationException("timeout") });

        Text(cut.Find(".alert-danger .alert-row-text")).Should().Be(
            "Could not load what you can invite. Nothing has changed. Try again, or come back in a few minutes.");
        cut.Find(".alert-danger button").TextContent.Trim().Should().Be("Try again");
        cut.Markup.Should().NotContain("timeout");
    }

    /// <summary>T373 fix pass, P2: a Try again that loads hands the focus to the form's first field, not the body.</summary>
    [Fact]
    public void ATryAgainThatLoads_FocusesTheEmailField()
    {
        var sender = new FakeSender { ReadFailure = new InvalidOperationException("timeout") };
        var cut = Render(sender);
        sender.ReadFailure = null;

        cut.Find(".alert-danger button").Click();

        cut.WaitForAssertion(() => Focused().Should().Contain("invitation-email"));
    }

    [Fact]
    public void AHolderOfTrainee_SeesTheHeaderAndTheNote_BeforeAnythingIsRead()
    {
        _auth.SetRoles(WombatRoles.InstitutionalAdmin, WombatRoles.Trainee);
        var sender = new FakeSender();
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<InviteAPerson>();

        Text(cut.Find(".alert.alert-info")).Should().Be(UserAdministrationRules.TraineeAdministersNoUser);
        cut.FindAll(".alert[role]").Should().BeEmpty("P9: the note is an info alert with no role, on all four pages");
        cut.FindAll("form, .skeleton").Should().BeEmpty();
        sender.Reads.Should().Be(0, "M1");
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private IRenderedComponent<InviteAPerson> RefusedOverTheAddress(Exception refusal)
    {
        var cut = Render(new FakeSender { Refusal = refusal });
        cut.Find("#invitation-email").Change("smit@kgk.wombat.local");
        cut.Find("#role-coordinator").Change(true);
        cut.Find("form").Submit();
        return cut;
    }

    private IRenderedComponent<InviteAPerson> Issue(FakeSender sender, string email)
    {
        var cut = Render(sender);
        cut.Find("#invitation-email").Change(email);
        cut.Find("#role-coordinator").Change(true);
        cut.Find("form").Submit();
        cut.WaitForAssertion(() => cut.Find(".otl"));
        return cut;
    }

    private static IElement ShouldBeRefusedUnder(IRenderedFragment cut, string fieldId, string words)
    {
        IElement? message = null;
        cut.WaitForAssertion(() => message = cut.Find($"#{fieldId}-msg"));
        Text(message!).Should().Be(words);
        message!.ClassList.Should().Contain("validation-message");
        var field = cut.Find($"#{fieldId}");
        field.GetAttribute("aria-invalid").Should().Be("true");
        field.ClassList.Should().Contain("input-validation-error");
        field.GetAttribute("aria-describedby").Should().Contain($"{fieldId}-msg");
        message.ParentElement!.QuerySelector($"#{fieldId}").Should().NotBeNull("the message sits under its field");
        return message;
    }

    private IRenderedComponent<InviteAPerson> Render(FakeSender sender, int? from = null)
    {
        Services.AddSingleton<IScopedSender>(sender);
        if (from is int id)
        {
            var navigation = Services.GetRequiredService<NavigationManager>();
            navigation.NavigateTo(OnboardingLinks.InviteFrom(id));
        }

        var cut = RenderComponent<InviteAPerson>();
        cut.WaitForState(() => cut.FindAll(".skeleton").Count == 0);
        return cut;
    }

    private void SignIn(string role, params Claim[] claims)
    {
        _auth.SetAuthorized("caller@test");
        _auth.SetRoles(role);
        _auth.SetClaims([new Claim(ClaimTypes.NameIdentifier, "admin-1"), .. claims]);
    }

    private IReadOnlyList<JSRuntimeInvocation> FocusCalls() => JSInterop.Invocations.ToList();

    /// <summary>The ids the page asked wombat.js to focus.</summary>
    private IReadOnlyList<string> Focused()
        => JSInterop.Invocations
            .Where(invocation => invocation.Identifier == "wombat.focusById")
            .Select(invocation => invocation.Arguments[0]?.ToString() ?? string.Empty)
            .ToList();

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private sealed class FakeSender : IScopedSender
    {
        public IReadOnlyList<SpecialityDto> Specialities { get; init; } = [new SpecialityDto(5, 7, "Paediatrics", null, true)];
        public InvitationPrefillDto? Prefill { get; init; }
        public Exception? Refusal { get; init; }
        public Exception? ReadFailure { get; set; }
        public TaskCompletionSource<IssuedInvitationResult>? Hold { get; set; }
        public TaskCompletionSource? ReadHold { get; init; }

        public List<IssueInvitationCommand> Issued { get; } = [];
        public List<int> PrefillAsked { get; } = [];
        public int Reads { get; private set; }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is IssueInvitationCommand issue)
            {
                Issued.Add(issue);
                if (Refusal is not null)
                {
                    throw Refusal;
                }

                var result = Hold is { } hold
                    ? await hold.Task
                    : new IssuedInvitationResult(9, "token-issued") { ExpiresOn = new DateOnly(2026, 11, 2) };
                return (TResponse)(object)result;
            }

            if (ReadHold is { } readHold)
            {
                await readHold.Task;
            }

            Reads++;
            if (ReadFailure is not null)
            {
                throw ReadFailure;
            }

            object? answer = request switch
            {
                GetInstitutionsListQuery => new[]
                {
                    new InstitutionDto(2, "Demo Institution", "DEMO", null, true, Created),
                    new InstitutionDto(1, "Kgosi Kgari Teaching Hospital", "KGK", null, true, Created)
                },
                GetCollegesListQuery => new[] { new CollegeDto(7, "College of Paediatricians of South Africa", "CPSA", null, true, Created) },
                GetSpecialitiesListQuery => Specialities,
                GetSubSpecialitiesListQuery => new[] { new SubSpecialityDto(6, 5, "Paediatrics", null, true, null) },
                GetInvitationPrefillQuery prefill => Asked(prefill.InvitationId),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return (TResponse)answer!;
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();

        private InvitationPrefillDto? Asked(int id)
        {
            PrefillAsked.Add(id);
            return Prefill;
        }
    }
}
