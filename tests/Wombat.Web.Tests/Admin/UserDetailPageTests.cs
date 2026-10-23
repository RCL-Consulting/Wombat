using Bunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Accounts;
using Wombat.Application.Features.Institutions.Queries.GetAdoptedDisciplinesForInstitution;
using Wombat.Application.Features.Users;
using Wombat.Application.Features.Users.Commands.AddRoleToUser;
using Wombat.Application.Features.Users.Commands.RemoveRoleFromUser;
using Wombat.Application.Features.Users.Commands.RemoveUserInstitutionalSignIn;
using Wombat.Application.Features.Users.Queries.GetRequestsWaitingForUser;
using Wombat.Application.Features.Users.Queries.GetUserById;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Users;
using Wombat.Web.Components.Shared;
using Wombat.Web.Tests.Accessibility;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// The user page of sections (T376, flow 12; boards P01–P36): the who line, Roles and scope with Add a role…'s note panel
/// (F11, M8, M9, M10, S13, F1, F2), the Remove dialogs per role (S3, F6), Sign-in and access's parts (F3, F7, Q5, Q17), the
/// no-role states (S2), the training section's mount, the not-found, loading and failed states, and the way back (S6).
/// </summary>
public sealed class UserDetailPageTests : UserDetailPageTestBase
{
    private const string SubtitleAtKgk =
        "Roles, sign-in and lockout for one person at Kgosi Kgari Teaching Hospital, with assessor training for an Assessor.";

    private const string TraineeNotOffered =
        "Trainee is not offered: a registrar becomes a trainee only when admitted, from Trainees with 'Admit…'.";

    // ─── P02: the who line and the roles ───────────────────────────────────

    [Fact]
    public void P02_TheWhoLine_TheRoleRows_AndEachTriggerNamedForItsRow()
    {
        var cut = Open(Zulu());

        cut.Find("h1").TextContent.Trim().Should().Be("Thandi Zulu");
        Text(cut.Find(".page-subtitle")).Should().Be(SubtitleAtKgk);
        var who = cut.Find("dl.detail-list[aria-label='Account']");
        who.QuerySelectorAll("dt").Select(Text).Should().Equal("Email", "Institution", "Status", "Scope");
        who.QuerySelectorAll("dd").Select(Text).Should().Equal("zulu@kgk.wombat.local", KgkName, "Active", "Paediatrics");
        who.QuerySelector("dd")!.InnerHtml.Should().Contain("zulu@<wbr>kgk.wombat.local", "flow 11's M7: a break after the @");

        cut.FindAll(".role-row .role-row-label").Select(Text).Should().Equal("Committee member");
        var remove = Named(cut, "Remove Committee member from Thandi Zulu");
        Text(remove).Should().Be("Remove…");
        remove.ClassList.Should().Contain("btn-outline").And.NotContain("btn-danger", "S22: the filled button is the dialog's");
        remove.GetAttribute("aria-haspopup").Should().Be("dialog");

        var lockOut = Named(cut, "Lock out Thandi Zulu");
        Text(lockOut).Should().Be("Lock out…");
        lockOut.ClassList.Should().Contain("btn-outline");
        cut.FindAll("button[disabled]").Where(button => button.Closest("dialog") is null)
            .Should().BeEmpty("an unavailable action is aria-disabled, never disabled");
        Text(Section(cut, "Roles and scope").QuerySelector("p.field-help")!).Should().Be(TraineeNotOffered);
        cut.FindAll("h2").Select(Text).Should().Contain(["Roles and scope", "Sign-in and access"]);
        cut.FindAll(".section-part h3").Select(Text).Should().Equal("Password", "Institutional sign-in", "Lockout");
        cut.Markup.Should().NotContain("Pending invitations", "Q9");
        cut.Markup.Should().NotContain("Back to users", "P23: the header's way back is the trail");
    }

    [Fact]
    public void P16_ALockedAccount_ReadsLockedOut_ByTheAdministrativeLockAlone()
    {
        var cut = Open(Zulu() with { IsLockedOut = true });

        var status = cut.FindAll("dl.detail-list dd").ElementAt(2);
        status.QuerySelector(".badge.badge-declined.status-locked").Should().NotBeNull();
        Text(status).Should().Be("Locked out");
    }

    [Fact]
    public void ThePersonsRoles_ReadAsLabels_InOneOrder()
    {
        var cut = Open(Zulu(WombatRoles.Assessor, WombatRoles.CommitteeMember, WombatRoles.Coordinator, WombatRoles.SpecialityAdmin));

        cut.FindAll(".role-row-label").Select(Text).Should().Equal("Speciality admin", "Coordinator", "Committee member", "Assessor");
    }

    // ─── P01: her own account ───────────────────────────────────────────────

    [Fact]
    public void P01_HerOwnAccount_OffersNoChange_ItsSubtitleSaysSo_AndItsSignInIsMyAccounts()
    {
        var own = new UserDetailDto("mbatha", "mbatha@kgk.wombat.local", "Nolwazi", "Mbatha", Kgk, KgkName, [], [],
            [WombatRoles.InstitutionalAdmin], false) { Scope = KgkName };

        var cut = Open(own);

        Text(cut.Find(".page-subtitle")).Should().Be("Your own account: your roles and how you sign in.");
        cut.Find("#user-own-account-note").GetAttribute("role").Should().BeNull("S23: a standing notice is no live region");
        cut.FindAll(".role-row .btn").Should().BeEmpty();
        cut.FindAll("#add-role-toggle").Should().BeEmpty();
        cut.FindAll(".section-part h3").Select(Text).Should().Equal("Institutional sign-in");
        Text(Part(cut, "Institutional sign-in").QuerySelector("p")!)
            .Should().Be("No institutional sign-in is linked. Nolwazi Mbatha signs in with an email and a password.");
    }

    // ─── P03–P05: Add a role… ───────────────────────────────────────────────

    [Fact]
    public void P03_AddARole_OpensTheNotePanel_WiredByAriaControlsOnlyWhileItExists_TheFirstRadioFocused()
    {
        Sender.Adopted = KgkAdopted;
        var cut = Open(Zulu());

        var toggle = cut.Find("#add-role-toggle");
        toggle.GetAttribute("aria-expanded").Should().Be("false");
        toggle.HasAttribute("aria-controls").Should().BeFalse("M10: aria-controls only while the panel exists");

        toggle.Click();

        toggle = cut.Find("#add-role-toggle");
        toggle.GetAttribute("aria-expanded").Should().Be("true");
        toggle.GetAttribute("aria-controls").Should().Be("add-role-panel");
        var panel = cut.Find("#add-role-panel");
        panel.ClassList.Should().Contain(["detail-card", "note-panel", "note-panel--form"]);
        panel.HasAttribute("role").Should().BeFalse("the panel is not a group; its fieldset is");
        var fieldset = panel.QuerySelector("fieldset.choice-group.choice-help-stack")!;
        Text(fieldset.QuerySelector("legend")!).Should().Be("Role *");
        fieldset.QuerySelectorAll(".form-check label").Select(Text).Should().Equal(
            "Institutional admin", "Speciality admin", "Sub-speciality admin", "Coordinator", "Assessor");

        // M9: each help a paragraph after the label, outside it, named by the input.
        foreach (var check in fieldset.QuerySelectorAll(".form-check"))
        {
            var input = check.QuerySelector("input[type=radio]")!;
            var help = check.QuerySelector("p.choice-help")!;
            help.Closest("label").Should().BeNull();
            input.GetAttribute("aria-describedby").Should().Be(help.Id);
        }

        fieldset.QuerySelectorAll("p.choice-help").Select(Text).Should().Contain(
            "Works across the institution, kept to Thandi Zulu's scope: Paediatrics.",
            "the nit: a scoped person's Coordinator says the scope they keep to");
        Text(panel.QuerySelector("p.field-help")!).Should().Be(TraineeNotOffered);
        var actions = panel.QuerySelectorAll(".form-actions button").Select(Text).ToList();
        actions.Should().Equal("Cancel", "Add role");
        panel.QuerySelector(".form-actions .btn-quiet").Should().NotBeNull("F11: Cancel is quiet");
        cut.WaitForAssertion(() => FocusedIds().Should().Contain("add-role-institutionaladmin"));
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void F11_Cancel_ClosesThePanel_AndGivesTheFocusBackToItsTrigger()
    {
        var cut = Open(Zulu());
        cut.Find("#add-role-toggle").Click();

        Button(cut, "Cancel").Click();

        cut.FindAll("#add-role-panel").Should().BeEmpty();
        cut.Find("#add-role-toggle").HasAttribute("aria-controls").Should().BeFalse();
        cut.WaitForAssertion(() => FocusedIds().Should().EndWith("add-role-toggle"));
        Sender.Commands.Should().BeEmpty();
    }

    [Fact]
    public void P04_Assessor_ReadsOutTheHeldSpeciality_AndTheOnlyAdoptedSubSpeciality()
    {
        Sender.Adopted = KgkAdopted;
        var cut = Open(Zulu());
        cut.Find("#add-role-toggle").Click();

        cut.Find("#add-role-assessor").Change(WombatRoles.Assessor);

        var readOuts = cut.FindAll("#add-role-panel dl.form-group");
        readOuts.Should().HaveCount(2);
        readOuts.Select(readOut => Text(readOut.QuerySelector("dt")!)).Should().Equal("Speciality", "Sub-speciality");
        readOuts.Select(readOut => Text(readOut.QuerySelector("dd.readonly-value")!)).Should().Equal("Paediatrics", "Paediatrics");
        readOuts.Select(readOut => Text(readOut.QuerySelector("dd.field-help")!)).Should().Equal(
            "Already held by Thandi Zulu.", "The only sub-speciality Kgosi Kgari Teaching Hospital has adopted.");
        cut.FindAll("#add-role-panel select").Should().BeEmpty("S13: one option is a read-out, not a select");
        cut.FindAll("#add-role-panel .alert-info").Should().BeEmpty("no narrowing: she holds a speciality");
    }

    [Fact]
    public void P05_TheRoleAdded_TheResultInTheSection_Focused_ThePanelGone_AndTheTrainingShown()
    {
        Sender.Adopted = KgkAdopted;
        Sender.After = _ => ZuluAssessor();
        var cut = Open(Zulu());
        cut.Find("#add-role-toggle").Click();
        cut.Find("#add-role-assessor").Change(WombatRoles.Assessor);

        Button(cut, "Add role").Click();

        var sent = Sender.Commands.OfType<AddRoleToUserCommand>().Single();
        sent.Role.Should().Be(WombatRoles.Assessor);
        sent.SpecialityId.Should().BeNull("she holds one");
        sent.SubSpecialityId.Should().Be(20, "the only one adopted is posted");

        var result = Section(cut, "Roles and scope").QuerySelector(".action-result")!;
        result.GetAttribute("tabindex").Should().Be("-1");
        var alert = result.QuerySelector(".alert-success")!;
        alert.GetAttribute("role").Should().Be("status");
        Text(alert).Should().Be(
            "Assessor added to Thandi Zulu. Thandi Zulu's scope is now Paediatrics / Paediatrics. Any open session ends within a minute.");
        Text(alert.QuerySelector("strong")!).Should().Be("Assessor added to Thandi Zulu.", "only the first sentence is bold");
        cut.WaitForAssertion(() => ElementFocusCalls().Should().Be(1));
        cut.FindAll("#add-role-panel").Should().BeEmpty();
        cut.Find("#add-role-toggle").GetAttribute("aria-expanded").Should().Be("false");
        cut.FindComponent<AssessorTrainingSection>().Instance.HoldsAssessor.Should().BeTrue("Q18: the section shows with the role");
        Text(cut.FindAll("dl.detail-list dd").ElementAt(3)).Should().Be("Paediatrics / Paediatrics");
    }

    [Fact]
    public void ChoosingNoRole_IsRefusedOnTheFieldset_WithTheFocusOnTheFirstRadio_AndNothingSent()
    {
        var cut = Open(Zulu());
        cut.Find("#add-role-toggle").Click();

        Button(cut, "Add role").Click();

        cut.Find("fieldset.choice-group").ClassList.Should().Contain("is-invalid");
        Text(cut.Find("#add-role-error")).Should().Be("Choose a role.");
        cut.Find("#add-role-coordinator").GetAttribute("aria-invalid").Should().Be("true");
        cut.Find("#add-role-coordinator").GetAttribute("aria-describedby").Should().Contain("add-role-error");
        cut.FindAll(".validation-summary-errors").Should().BeEmpty("S22: no RefusalSummary for one field");
        Sender.Commands.Should().BeEmpty();
    }

    [Fact]
    public void P08_TwoAdoptedSpecialities_ASpecialityAdminWithNoneChosen_IsRefusedUnderSpeciality_Focused()
    {
        Sender.Adopted = new AdoptedDisciplinesDto(
            [new AdoptedSpecialityDto(10, "Paediatrics"), new AdoptedSpecialityDto(11, "Surgery")], []);
        var cut = Open(Smit());
        cut.Find("#add-role-toggle").Click();
        cut.Find("#add-role-specialityadmin").Change(WombatRoles.SpecialityAdmin);

        Text(cut.Find("label[for='add-role-speciality']")).Should().Be("Speciality *");
        cut.FindAll("#add-role-speciality option").Select(Text).Should().Equal("Choose a speciality", "Paediatrics", "Surgery");
        Button(cut, "Add role").Click();

        var select = cut.Find("#add-role-speciality");
        select.GetAttribute("aria-invalid").Should().Be("true");
        select.GetAttribute("aria-describedby").Should().Be("add-role-speciality-error");
        Text(cut.Find("#add-role-speciality-error")).Should().Be("Speciality administrators must be scoped to a speciality.");
        cut.WaitForAssertion(() => FocusedIds().Should().EndWith("add-role-speciality"));
        Sender.Commands.Should().BeEmpty();
    }

    [Fact]
    public void P29_P30_F2_TheNarrowing_IsSaidBeforeTheSave_AndInTheResult()
    {
        Sender.Adopted = new AdoptedDisciplinesDto([new AdoptedSpecialityDto(10, "Paediatrics")], []);
        Sender.After = _ => Smit() with
        {
            Roles = [WombatRoles.Coordinator, WombatRoles.SpecialityAdmin], SpecialityIds = [10], Scope = "Paediatrics",
            SpecialityNames = ["Paediatrics"]
        };
        var cut = Open(Smit());
        cut.Find("#add-role-toggle").Click();
        cut.Find("#add-role-specialityadmin").Change(WombatRoles.SpecialityAdmin);

        var readOut = cut.Find("#add-role-panel dl.form-group");
        Text(readOut.QuerySelector("dd.readonly-value")!).Should().Be("Paediatrics");
        Text(readOut.QuerySelector("dd.field-help")!).Should().Be("The only speciality Kgosi Kgari Teaching Hospital has adopted.");
        var narrowing = cut.Find("#add-role-panel .alert-info");
        Text(narrowing).Should().Be("Pieter Smit's scope becomes Paediatrics, for every role they hold.");
        narrowing.HasAttribute("role").Should().BeFalse();

        Button(cut, "Add role").Click();

        Sender.Commands.OfType<AddRoleToUserCommand>().Single().SpecialityId.Should().Be(10);
        Text(cut.Find(".action-result .alert-success")).Should().Be(
            "Speciality admin added to Pieter Smit. Pieter Smit's scope becomes Paediatrics, for every role they hold. Any open session ends within a minute.");
    }

    [Fact]
    public void P28_F1_ARoleWithNothingToAdd_KeepsTheScope_AndSaysItStays()
    {
        Sender.After = _ => ZuluAssessor() with { Roles = [WombatRoles.Assessor, WombatRoles.CommitteeMember] };
        var cut = Open(ZuluAssessor() with { Roles = [WombatRoles.Assessor] });
        cut.Find("#add-role-toggle").Click();
        cut.Find("#add-role-committeemember").Change(WombatRoles.CommitteeMember);

        cut.FindAll("#add-role-panel .form-grid").Should().BeEmpty("F1: the scope held is enough");
        Button(cut, "Add role").Click();

        var sent = Sender.Commands.OfType<AddRoleToUserCommand>().Single();
        (sent.SpecialityId, sent.SubSpecialityId).Should().Be((null, null));
        Text(cut.Find(".action-result .alert-success")).Should().Be(
            "Committee member added to Thandi Zulu. Thandi Zulu's scope stays Paediatrics / Paediatrics. Any open session ends within a minute.");
    }

    [Fact]
    public void ACommandsTypedRefusal_GoesBesideItsField()
    {
        Sender.Adopted = new AdoptedDisciplinesDto(
            [new AdoptedSpecialityDto(10, "Paediatrics"), new AdoptedSpecialityDto(11, "Surgery")], []);
        Sender.Refusal = new RoleAddRefusedException(RoleAddRefusal.NotAdopted, RoleAddField.Speciality, "Choose a speciality this person's institution has adopted.");
        var cut = Open(Smit());
        cut.Find("#add-role-toggle").Click();
        cut.Find("#add-role-specialityadmin").Change(WombatRoles.SpecialityAdmin);
        cut.Find("#add-role-speciality").Change("11");

        Button(cut, "Add role").Click();

        Text(cut.Find("#add-role-speciality-error")).Should().Be("Choose a speciality this person's institution has adopted.");
        cut.FindAll("#add-role-panel").Should().ContainSingle("the panel stays, with what was chosen");
    }

    [Fact]
    public void APersonWithNoInstitution_IsOfferedNoRole_AndTheReasonIsSaid()
    {
        SignIn(WombatRoles.Administrator);
        var college = new UserDetailDto("college", "college@cmsa.local", "College", "Admin", null, null, [], [],
            [WombatRoles.CollegeAdmin], false) { Scope = "College of Paediatricians", CollegeId = 1, CollegeName = "College of Paediatricians" };

        var cut = Open(college);

        cut.FindAll("#add-role-toggle").Should().BeEmpty();
        Text(Section(cut, "Roles and scope").QuerySelector("p.field-help")!).Should().Be(
            "College Admin has no institution, so no role can be added here: every role offered here works at an institution.");
        Text(cut.FindAll("dl.detail-list dd").ElementAt(1)).Should().Be("Global (no institution)");
        Text(cut.Find(".page-subtitle")).Should().Be("Roles, sign-in and lockout for one person, with assessor training for an Assessor.");
    }

    // ─── P06, P07, P36: Remove… ─────────────────────────────────────────────

    [Fact]
    public void P06_RemoveAssessor_ReadsTheWaitingRequestsOnOpen_AndSaysWhatStays()
    {
        Sender.Waiting = new WaitingRequestsDto(1, new DateTimeOffset(2026, 10, 12, 10, 35, 0, TimeSpan.Zero),
            [new WaitingRow("Pieter du Plessis", "Portfolio and Logbook Review (Paediatrics)", "PAED-015",
                new DateTimeOffset(2026, 10, 12, 10, 35, 0, TimeSpan.Zero), true)]);
        var cut = Open(ZuluAssessor());
        Sender.Received.OfType<GetRequestsWaitingForUserQuery>().Should().BeEmpty("F6: never read on a page view");

        Named(cut, "Remove Assessor from Thandi Zulu").Click();

        Sender.Received.OfType<GetRequestsWaitingForUserQuery>().Should().ContainSingle();
        var dialog = Dialog(cut, "Remove Assessor from Thandi Zulu?");
        dialog.QuerySelectorAll(".dialog-body > p").Select(Text).Should().Equal(
            "Thandi Zulu can no longer be named as an activity's assessor, and leaves the Assessors list. Any open session ends within a minute.",
            "Any assessor training recorded for Thandi Zulu is kept, and shows again if Assessor is given back.",
            "Thandi Zulu's scope stays Paediatrics / Paediatrics, for Committee member, the role still held.");
        var waiting = dialog.QuerySelector(".alert.alert-warning")!;
        waiting.HasAttribute("role").Should().BeFalse("S23");
        Text(waiting.QuerySelector("p")!).Should().Be(
            "1 request is waiting for Thandi Zulu, since 2026-10-12 12:35 SAST. It stays with them; it does not move to anyone else.");
        waiting.QuerySelectorAll("li").Select(Text).Should().Equal(
            "Pieter du Plessis · Portfolio and Logbook Review (Paediatrics) · PAED-015 · waiting since 2026-10-12 12:35 SAST · overdue");
        dialog.QuerySelectorAll(".form-actions button").Select(Text).Should().Equal("Keep the role", "Remove Assessor");
        dialog.QuerySelector(".form-actions .btn-danger")!.TextContent.Trim().Should().Be("Remove Assessor");
    }

    [Fact]
    public void P07_TheRoleRemoved_TheResultFocused_AndTheTrainingHidden()
    {
        Sender.After = _ => ZuluAssessor() with { Roles = [WombatRoles.CommitteeMember] };
        var cut = Open(ZuluAssessor());
        Named(cut, "Remove Assessor from Thandi Zulu").Click();

        DialogButton(cut, "Remove Assessor").Click();

        Sender.Commands.OfType<RemoveRoleFromUserCommand>().Single().Role.Should().Be(WombatRoles.Assessor);
        var alert = Section(cut, "Roles and scope").QuerySelector(".action-result .alert-success")!;
        Text(alert).Should().Be("Assessor removed from Thandi Zulu. Any open session ends within a minute.");
        cut.WaitForAssertion(() => ElementFocusCalls().Should().Be(1));
        cut.FindComponent<AssessorTrainingSection>().Instance.HoldsAssessor.Should().BeFalse("Q18: hidden, never deleted");
    }

    [Theory]
    [InlineData(WombatRoles.CommitteeMember, "David Naidoo leaves every committee panel and can no longer sit reviews. If David Naidoo chairs a review, its ratification waits until the panel has a chair again. Any open session ends within a minute.")]
    [InlineData(WombatRoles.SpecialityAdmin, "David Naidoo no longer runs the Paediatrics committee's work: panels, reviews and decisions due. Any open session ends within a minute.")]
    [InlineData(WombatRoles.SubSpecialityAdmin, "David Naidoo no longer runs the Paediatrics / Paediatrics committee's work. Any open session ends within a minute.")]
    [InlineData(WombatRoles.Coordinator, "David Naidoo can no longer run MSF campaigns or chase requests waiting for assessors; a campaign already under way can still be finished. Any open session ends within a minute.")]
    [InlineData(WombatRoles.InstitutionalAdmin, "David Naidoo can no longer invite, admit or manage people, or adopt curricula, at Kgosi Kgari Teaching Hospital. Any open session ends within a minute.")]
    public void P36_EachRolesDialog_SaysWhatItsRemovalDoes_AndTheLastRoleSaysWhatIsLeft(string role, string effect)
    {
        var naidoo = new UserDetailDto("naidoo", "naidoo@kgk.wombat.local", "David", "Naidoo", Kgk, KgkName, [10], [20], [role], false)
        {
            Scope = "Paediatrics / Paediatrics", SpecialityNames = ["Paediatrics"], SubSpecialityNames = ["Paediatrics"]
        };
        var cut = Open(naidoo);

        Named(cut, $"Remove {WombatRoleLabels.For(role)} from David Naidoo").Click();

        var dialog = Dialog(cut, $"Remove {WombatRoleLabels.For(role)} from David Naidoo?");
        dialog.QuerySelectorAll(".dialog-body > p").Select(Text).Should().Equal(
            effect, "David Naidoo will hold no role. They can sign in to My account and My data rights.");
        Sender.Received.OfType<GetRequestsWaitingForUserQuery>().Should().BeEmpty("only Assessor's removal asks what waits");
    }

    // ─── S2: no role ────────────────────────────────────────────────────────

    [Theory]
    [InlineData(false, "Pieter du Plessis holds no role: they left the programme on 2026-10-18.", "They can still sign in to read their training record (My progress) and their data rights.")]
    [InlineData(true, "Pieter du Plessis holds no role: they completed the programme on 2026-10-18.", "They can still sign in to read their training record (My progress) and their data rights.")]
    public void P09_P12_ANoRolePerson_WithAnEndedProgramme_SaysHowAndWhen(bool completed, string lead, string rest)
    {
        var cut = Open(DuPlessis() with { EndedProgramme = new EndedProgrammeDto(new DateOnly(2026, 10, 18), completed) });

        var block = cut.Find(".no-role p");
        Text(block.QuerySelector("strong")!).Should().Be(lead);
        Text(block).Should().Be($"{lead} {rest}");
        cut.FindAll(".role-rows").Should().BeEmpty();
        Part(cut, "Lockout").QuerySelectorAll("#lockout-effects li").Select(Text)
            .Should().Equal(["They are sent no activity reminders or weekly digests."], "Q17: one line for a person with no role");
    }

    [Fact]
    public void P35_ANoRolePerson_WithNoProfile_CanSignInToMyAccount()
    {
        var cut = Open(DuPlessis());

        Text(cut.Find(".no-role p")).Should().Be("Pieter du Plessis holds no role. They can sign in to My account and My data rights.");
    }

    [Fact]
    public void P13_P18_TheSystemManagedRoles_SayWhyThereIsNoRemove()
    {
        var cut = Open(Zulu(WombatRoles.PendingTrainee));

        var row = cut.Find(".role-row");
        Text(row.QuerySelector(".role-row-label")!).Should().Be("Pending trainee");
        Text(row.QuerySelector(".role-row-note")!).Should().Be("System-managed: waiting to be admitted. Open Trainees");
        row.QuerySelector(".role-row-note a")!.GetAttribute("href").Should().Be(OnboardingLinks.Trainees);
        row.QuerySelector("button").Should().BeNull();
    }

    // ─── Sign-in and access ─────────────────────────────────────────────────

    [Fact]
    public void P20_P21_P33_AnotherPersonsSignIn_IsListed_RemovedThroughItsDialog_AndTheResultFocused()
    {
        var zulu = ZuluAssessor() with { InstitutionalSignIns = [new InstitutionalSignIn("kgk-sso", "KGK SSO")] };
        Sender.After = _ => ZuluAssessor();
        var cut = Open(zulu);

        var part = Part(cut, "Institutional sign-in");
        Text(part.QuerySelector(".signin-method strong")!).Should().Be("KGK SSO");
        var remove = Named(cut, "Remove the KGK SSO sign-in from Thandi Zulu");
        Text(remove).Should().Be("Remove…");
        remove.Click();

        var dialog = Dialog(cut, "Remove the KGK SSO sign-in from Thandi Zulu?");
        Text(dialog.QuerySelector("p")!).Should().Be("Thandi Zulu can no longer sign in through KGK SSO. Signing in with the email and a password still works.");
        dialog.QuerySelectorAll(".form-actions button").Select(Text).Should().Equal("Keep the sign-in", "Remove sign-in");
        DialogButton(cut, "Remove sign-in").Click();

        Sender.Commands.OfType<RemoveUserInstitutionalSignInCommand>().Single().Provider.Should().Be("kgk-sso");
        part = Part(cut, "Institutional sign-in");
        Text(part.QuerySelector(".action-result .alert-success")!).Should().Be(
            "The KGK SSO sign-in was removed from Thandi Zulu. Thandi Zulu signs in with an email and a password.");
        Text(part.QuerySelector(".action-result + p")!).Should().Be("No institutional sign-in is linked. Thandi Zulu signs in with an email and a password.");
        cut.WaitForAssertion(() => ElementFocusCalls().Should().Be(1));
    }

    [Fact]
    public void P4_WithTwoSignInsAndNoPassword_TheDialogSaysTheOtherSignInStillWorks()
    {
        // A password is not promised to an account that has none (T376 build review, P4): the dialog's words follow the
        // account, as the result's do.
        var zulu = ZuluAssessor() with
        {
            HasLocalPassword = false,
            InstitutionalSignIns = [new InstitutionalSignIn("kgk-sso", "KGK SSO"), new InstitutionalSignIn("kgk-entra", "KGK Entra")]
        };
        var cut = Open(zulu);

        Named(cut, "Remove the KGK SSO sign-in from Thandi Zulu").Click();

        var dialog = Dialog(cut, "Remove the KGK SSO sign-in from Thandi Zulu?");
        Text(dialog.QuerySelector("p")!).Should().Be(
            "Thandi Zulu can no longer sign in through KGK SSO. Thandi Zulu still signs in through their other institutional sign-in.");
    }

    [Fact]
    public void ARemovalTheDirectoryRefuses_IsWordedByItsReason()
    {
        Sender.Refusal = new InstitutionalSignInRemovalRefusedException(InstitutionalSignInRemoval.LastWayIn);
        var cut = Open(ZuluAssessor() with { InstitutionalSignIns = [new InstitutionalSignIn("kgk-sso", "KGK SSO")] });
        Named(cut, "Remove the KGK SSO sign-in from Thandi Zulu").Click();

        DialogButton(cut, "Remove sign-in").Click();

        var alert = Part(cut, "Institutional sign-in").QuerySelector(".action-result .alert-danger")!;
        Text(alert).Should().Be("The institutional sign-in is the account's only way in.");
        alert.GetAttribute("role").Should().Be("alert");
    }

    [Fact]
    public void P22_AnSsoOnlyAccount_HasNoPasswordField_AndItsLastWayInIsUnavailable_InTheBuiltWords()
    {
        var sso = ZuluAssessor() with
        {
            Roles = [WombatRoles.Assessor],
            AllowsLocalPassword = false,
            HasLocalPassword = false,
            InstitutionalSignIns = [new InstitutionalSignIn("kgk-sso", "KGK SSO")]
        };
        var cut = Open(sso);

        var password = Part(cut, "Password");
        password.QuerySelector("input").Should().BeNull("F3: no field");
        password.QuerySelector("button").Should().BeNull();
        Text(password.QuerySelector("p")!).Should().Be("Thandi Zulu signs in only through KGK SSO. A password cannot be set for them.");

        var remove = Named(cut, "Remove the KGK SSO sign-in from Thandi Zulu");
        remove.GetAttribute("aria-disabled").Should().Be("true");
        remove.HasAttribute("disabled").Should().BeFalse();
        remove.ClassList.Should().Contain("is-unavailable");
        Text(cut.Find($"#{remove.GetAttribute("aria-describedby")}")).Should().Be("The institutional sign-in is the account's only way in.");
        Sender.Commands.Should().BeEmpty();

        cut.FindComponent<AssessorTrainingSection>().Instance.HoldsAssessor.Should().BeTrue("check Left 4: P22's Assessor has the section");
    }

    // ─── The states ─────────────────────────────────────────────────────────

    [Fact]
    public void P23_AnIdOutOfScopeOrUnknown_IsUserUnavailable_WithAWayToUsers()
    {
        var cut = Open(null, userId: "someone-elsewhere");

        cut.Find("h1").TextContent.Trim().Should().Be("User");
        Text(cut.Find(".state-panel-title")).Should().Be("User unavailable");
        Text(cut.Find(".state-panel-copy")).Should().Be("The user could not be found or is outside your scope.");
        var link = cut.FindAll("a.btn").Single(anchor => Text(anchor) == "Go to Users");
        link.GetAttribute("href").Should().Be(PeopleLinks.Users);
        cut.FindAll(".page-subtitle").Should().BeEmpty();
    }

    [Fact]
    public void P25_AFailedRead_IsSaidInFixedWords_WithTryAgain_AndTheFailureLogged()
    {
        Sender.ReadFault = new TimeoutException("the connection's internals");
        var cut = Open(Zulu());

        cut.Find("h1").TextContent.Trim().Should().Be("User");
        var alert = cut.Find(".alert-danger");
        Text(alert).Should().Contain("Could not load this person. Nothing has changed. Try again, or come back in a few minutes.");
        cut.Markup.Should().NotContain("the connection's internals", "T329: never an exception's text");
        Text(cut.Find(".page-subtitle")).Should().Be("Roles, sign-in and lockout for one person, with assessor training for an Assessor.");

        Sender.ReadFault = null;
        cut.FindAll("button").Single(button => Text(button) == "Try again").Click();
        cut.WaitForAssertion(() => cut.Find("h1").TextContent.Trim().Should().Be("Thandi Zulu"));
    }

    [Fact]
    public void P26_ATraineeHoldingAdmin_GetsTheNote_NoSubtitle_AndNothingIsRead()
    {
        SignIn(WombatRoles.InstitutionalAdmin, WombatRoles.Trainee);

        var cut = Open(Zulu());

        Text(cut.Find("#user-trainee-note")).Should().Be(UserAdministrationRules.TraineeAdministersNoUser);
        cut.Find("#user-trainee-note").HasAttribute("role").Should().BeFalse();
        cut.FindAll(".page-subtitle").Should().BeEmpty();
        Sender.Received.Should().BeEmpty();
        cut.FindComponents<AssessorTrainingSection>().Should().BeEmpty("no section before a read or for the note");
    }

    // ─── S6, S10: the way back and the training section ─────────────────────

    [Fact]
    public void S6_TheUsersCrumb_ReturnsToTheFilteredUsersPage_ItWasOpenedFrom()
    {
        var cut = OpenWithTrail(Zulu(), "?back=%2Fadmin%2Fusers%3Frole%3DAssessor%26page%3D2");

        cut.Find("nav[aria-label='Breadcrumb'] a[href^='/admin/users']").GetAttribute("href")
            .Should().Be("/admin/users?role=Assessor&page=2");
    }

    [Fact]
    public void S6_ACraftedBack_IsRefused_ForPlainUsers()
    {
        var cut = OpenWithTrail(Zulu(), "?back=https%3A%2F%2Fevil.example%2Fadmin%2Fusers");

        cut.Find("nav[aria-label='Breadcrumb'] a[href^='/admin/users']").GetAttribute("href").Should().Be("/admin/users");
    }

    [Theory]
    [InlineData("?section=training", true)]
    [InlineData("?section=Training", false)]
    [InlineData("?section=lockout", false)]
    [InlineData("", false)]
    public void TheTrainingSection_IsMountedBetweenRolesAndSignIn_OpenOnlyForSectionTraining(string query, bool open)
    {
        var cut = Open(ZuluAssessor(), query);

        var section = cut.FindComponent<AssessorTrainingSection>().Instance;
        section.Open.Should().Be(open);
        section.UserId.Should().Be("zulu");
        section.PersonName.Should().Be("Thandi Zulu");
        section.HoldsAssessor.Should().BeTrue();
    }

    // ─── Fixture ────────────────────────────────────────────────────────────

    private static UserDetailDto Smit() => new(
        "smit", "smit@kgk.wombat.local", "Pieter", "Smit", Kgk, KgkName, [], [], [WombatRoles.Coordinator], false)
    {
        Scope = PersonScopeWords.AnySpeciality
    };

    private static UserDetailDto DuPlessis() => new(
        "duplessis", "duplessis@kgk.wombat.local", "Pieter", "du Plessis", Kgk, KgkName, [10], [20], [], false)
    {
        Scope = "Paediatrics / Paediatrics", SpecialityNames = ["Paediatrics"], SubSpecialityNames = ["Paediatrics"]
    };
}
