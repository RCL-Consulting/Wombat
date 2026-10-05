using FluentAssertions;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Programme;
using Wombat.Application.Features.Programme.Commands.SendActivityReminder;
using Wombat.Application.Features.Programme.Waiting;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Identity;
using static Wombat.Application.Tests.Features.Programme.ProgrammeWaitingFixture;

namespace Wombat.Application.Tests.Features.Programme;

/// <summary>
/// Waiting for assessors (T358, flow 06; Q3, C3, C7, C11; E3, E4; review 12): what waits for a named assessor in the
/// programme of the role read as, oldest first, on flow 04's clock, each row with whom it waits, its last reminder,
/// whether one went today, and whether its nominee can be reminded at all. One query for Home's card, the page and the
/// registrar page's section.
/// </summary>
public sealed class WaitingForAssessorsTests
{
    [Fact]
    public async Task TheRows_AreTheRequestsWaitingForANamedPerson_OldestFirst_EachNamedForItsAssessorAndRegistrar()
    {
        await using var db = Seeded();
        AddRequest(db, 1, Mahlangu, Zulu, Now.AddDays(-8));
        AddRequest(db, 2, DuPlessis, Khumalo, Now.AddHours(-2));
        AddRequest(db, 3, DuPlessis, Patel, Now.AddDays(-8).AddMinutes(-5));
        await db.SaveChangesAsync();

        var result = (await WaitingAsync(db, Coordinator(), WombatRoles.Coordinator))!;

        result.Items.Select(item => item.Id).Should().Equal([3, 1, 2], "the one that has waited longest leads");
        var first = result.Items[0];
        first.Holder!.Kind.Should().Be(ActivityHolderKind.Person);
        first.Holder.UserId.Should().Be(Patel);
        first.Holder.Name.Should().Be("Mohammed Patel");
        first.NomineeName.Should().Be("Mohammed Patel");
        first.SubjectName.Should().Be("Pieter du Plessis");
        first.CurrentStateLabel.Should().Be("Requested");
        first.DisplayName.Should().StartWith("Mini-CEX");
        result.Scope.Name.Should().Be("Kgosi Kgari Teaching Hospital");
        result.MayRemind.Should().BeTrue();
        (result.DueDays, result.NudgeDays).Should().Be((7, 5));
    }

    /// <summary>
    /// E3: a SpecialityAdmin's review of a teaching session and the Coordinator's MSF release await a reviewer too, but by
    /// role: the reader's own queue, never "waiting for an assessor". A request whose nominee field is empty names nobody.
    /// </summary>
    [Fact]
    public async Task ARoleHeldReview_AnMsfRelease_AndAnEmptyNomineeField_AreNeverListed()
    {
        await using var db = Seeded();
        AddRequest(db, 1, Mahlangu, Zulu, Now.AddDays(-1));
        AddRequest(db, 2, Mahlangu, null, Now.AddDays(-2));
        AddRequest(db, 3, Mahlangu, Zulu, Now.AddDays(-3), typeId: AssessorReads.TeachingTypeId, state: "submitted");
        AddRequest(db, 4, Mahlangu, Zulu, Now.AddDays(-4), typeId: AssessorReads.MsfTypeId, state: "draft");
        await db.SaveChangesAsync();

        foreach (var (principal, role) in new[] { (Coordinator(), WombatRoles.Coordinator), (SpecialityAdmin(), WombatRoles.SpecialityAdmin) })
        {
            (await WaitingAsync(db, principal, role))!.Items.Select(item => item.Id).Should().Equal([1], role);
        }
    }

    [Fact]
    public async Task FinishedWork_AndADraftOnlyItsAuthorMoves_AreNotWaiting()
    {
        await using var db = Seeded();
        AddRequest(db, 1, Mahlangu, Zulu, Now.AddDays(-1));
        AddRequest(db, 2, Mahlangu, Zulu, Now.AddDays(-2), state: "completed");
        AddRequest(db, 3, Mahlangu, Zulu, Now.AddDays(-3), state: "declined");
        AddRequest(db, 4, Mahlangu, Zulu, Now.AddDays(-4), typeId: AssessorReads.ReflectiveTypeId, state: "draft");
        await db.SaveChangesAsync();

        (await WaitingAsync(db, Coordinator(), WombatRoles.Coordinator))!.Items.Select(item => item.Id).Should().Equal(1);
    }

    /// <summary>E4: the scope of the role read as, never the union of the roles held.</summary>
    [Fact]
    public async Task ASpecialityAdminWhoAlsoSitsOnTheCommittee_ReadsTheSpecialityOnly()
    {
        await using var db = Seeded();
        AddRequest(db, 1, Mahlangu, Zulu, Now.AddDays(-1));
        AddRequest(db, 2, DuPlessis, Patel, Now.AddDays(-2), specialityId: Surgery, subSpecialityId: SurgerySub);
        await db.SaveChangesAsync();
        var both = TestPrincipals.InRoles(
            [WombatRoles.SpecialityAdmin, WombatRoles.CommitteeMember], Mokoena, Kgk, specialityId: Paediatrics);

        var asAdmin = (await WaitingAsync(db, both, WombatRoles.SpecialityAdmin))!;

        asAdmin.Items.Select(item => item.Id).Should().Equal([1], "Surgery's request is outside Paediatrics");
        asAdmin.Scope.Name.Should().Be("Paediatrics");
        (await WaitingAsync(db, Coordinator(), WombatRoles.Coordinator))!.Items.Select(item => item.Id)
            .Should().Equal([2, 1], "the Coordinator reads the whole hospital");
    }

    [Fact]
    public async Task TheSubSpecialityAdmin_ReadsTheSubSpeciality()
    {
        await using var db = Seeded();
        AddRequest(db, 1, Mahlangu, Zulu, Now.AddDays(-1));
        AddRequest(db, 2, DuPlessis, Patel, Now.AddDays(-2), specialityId: Surgery, subSpecialityId: SurgerySub);
        await db.SaveChangesAsync();

        var result = (await WaitingAsync(
            db, TestPrincipals.InRole(WombatRoles.SubSpecialityAdmin, Sithole, Kgk, subSpecialityId: PaediatricsSub),
            WombatRoles.SubSpecialityAdmin))!;

        result.Items.Select(item => item.Id).Should().Equal(1);
        result.Scope.Kind.Should().Be(ProgrammeScopeKind.SubSpeciality);
    }

    [Fact]
    public async Task TheReadersOwnRequest_AndAnotherInstitutionsRequest_AreNeverListed()
    {
        await using var db = Seeded();
        AddRequest(db, 1, Mahlangu, Zulu, Now.AddDays(-1));
        AddRequest(db, 2, Smit, Zulu, Now.AddDays(-2));
        AddRequest(db, 3, DuPlessis, Patel, Now.AddDays(-3), institutionId: OtherHospital);
        AddRequest(db, 4, DuPlessis, Patel, Now.AddDays(-4), institutionId: null);
        await db.SaveChangesAsync();

        (await WaitingAsync(db, Coordinator(), WombatRoles.Coordinator))!.Items.Select(item => item.Id)
            .Should().Equal([1], "the reader's own request, another hospital's and an unstamped one are not this programme's");
    }

    /// <summary>Flow 04's clock (E1): overdue at exactly 7 × 24 h since the last move, and whole days rounded down.</summary>
    [Fact]
    public async Task ARow_IsOverdueAtSevenTimesTwentyFourHours_AndItsWaitIsWholeDays()
    {
        await using var db = Seeded();
        AddRequest(db, 1, Mahlangu, Zulu, Now.AddDays(-7));
        AddRequest(db, 2, Mahlangu, Patel, Now.AddDays(-7).AddMinutes(1));
        AddRequest(db, 3, Mahlangu, Khumalo, Now.AddHours(-23));
        await db.SaveChangesAsync();

        var result = (await WaitingAsync(db, Coordinator(), WombatRoles.Coordinator))!;

        result.Items.Select(item => (item.Id, item.IsOverdue, item.WaitedDays)).Should().Equal((1, true, 7), (2, false, 6), (3, false, 0));
        (result.MatchCount, result.MatchOverdueCount, result.TotalCount, result.TotalOverdueCount).Should().Be((3, 1, 3, 1));
    }

    /// <summary>
    /// Review 12: the With filter's names are every waiting row's nominee once, by surname; the filters narrow the match
    /// and leave the totals, which count everything but the registrar's filter.
    /// </summary>
    [Fact]
    public async Task TheFilters_NarrowTheMatch_TheTotalsStand_AndTheWithNamesComeFromEveryRow()
    {
        await using var db = Seeded();
        AddRequest(db, 1, DuPlessis, Patel, Now.AddDays(-8));
        AddRequest(db, 2, Mahlangu, Zulu, Now.AddDays(-8).AddMinutes(5));
        AddRequest(db, 3, DuPlessis, Khumalo, Now.AddHours(-2));
        await db.SaveChangesAsync();

        var all = (await WaitingAsync(db, Coordinator(), WombatRoles.Coordinator))!;
        all.Nominees.Select(nominee => (nominee.UserId, nominee.Name)).Should().Equal(
            (Khumalo, "Fatima Khumalo"), (Patel, "Mohammed Patel"), (Zulu, "Thandi Zulu"));

        var withPatel = (await WaitingAsync(db, Coordinator(), WombatRoles.Coordinator, withUserId: Patel))!;
        withPatel.Items.Select(item => item.Id).Should().Equal(1);
        (withPatel.MatchCount, withPatel.MatchOverdueCount, withPatel.TotalCount, withPatel.TotalOverdueCount)
            .Should().Be((1, 1, 3, 2));
        withPatel.Nominees.Should().HaveCount(3, "the select keeps every name while one is chosen");
        withPatel.Filter.Should().Be(new Wombat.Application.Features.Programme.Waiting.WaitingForAssessorsFilter(WithUserId: Patel),
            "the page words its heading from what was asked");

        var overdueKhumalo = (await WaitingAsync(db, Coordinator(), WombatRoles.Coordinator, overdueOnly: true, withUserId: Khumalo))!;
        overdueKhumalo.Items.Should().BeEmpty();
        (overdueKhumalo.MatchCount, overdueKhumalo.TotalCount).Should().Be((0, 3), "no match, not empty: '0 of 3 waiting'");

        (await WaitingAsync(db, Coordinator(), WombatRoles.Coordinator, overdueOnly: true))!.Items.Select(item => item.Id)
            .Should().Equal([1, 2]);

        var mahlangus = (await WaitingAsync(db, Coordinator(), WombatRoles.Coordinator, subjectUserId: Mahlangu))!;
        mahlangus.Items.Select(item => item.Id).Should().Equal(2);
        mahlangus.TotalCount.Should().Be(1, "the registrar's filter is the section's whole list");
    }

    /// <summary>
    /// E3's w9: a nominee whose account is deactivated, who has no address, or whom no account answers is marked before
    /// anyone presses, in the order the command refuses; an opt-out of digest emails is no reason (E1).
    /// </summary>
    [Fact]
    public async Task ANomineeWhoCannotBeReminded_IsMarked_AndAnOptOutIsNoReason()
    {
        await using var db = Seeded();
        db.Users.Single(user => user.Id == Khumalo).LockoutEnd = UserDeactivation.IndefiniteLockoutEnd;
        db.Users.Single(user => user.Id == Zulu).Email = null;
        db.Users.Single(user => user.Id == Patel).OptOutOfDigestEmails = true;
        AddRequest(db, 1, Mahlangu, Khumalo, Now.AddDays(-4));
        AddRequest(db, 2, Mahlangu, Zulu, Now.AddDays(-3));
        AddRequest(db, 3, Mahlangu, "erased-assessor", Now.AddDays(-2));
        AddRequest(db, 4, Mahlangu, Patel, Now.AddDays(-1));
        await db.SaveChangesAsync();

        var result = (await WaitingAsync(db, Coordinator(), WombatRoles.Coordinator))!;

        result.Items.Select(item => (item.Id, item.CannotRemind)).Should().Equal(
            (1, ReminderOutcome.Deactivated), (2, ReminderOutcome.NoEmail), (3, ReminderOutcome.NoAccount), (4, (ReminderOutcome?)null));
    }

    /// <summary>
    /// Round 3's settled rule: a reminder today, on the South African calendar, blocks every staff member until tomorrow,
    /// and the last reminder's line stays the next day. 22:30 UTC on the 3rd is the 4th in South Africa.
    /// </summary>
    [Fact]
    public async Task EachRow_CarriesItsLastReminder_AndWhetherOneWentToday_OnTheSouthAfricanCalendar()
    {
        await using var db = Seeded();
        AddRequest(db, 1, Mahlangu, Zulu, Now.AddDays(-8));
        AddRequest(db, 2, DuPlessis, Patel, Now.AddDays(-8));
        AddRequest(db, 3, DuPlessis, Khumalo, Now.AddDays(-1));
        db.ActivityReminders.AddRange(
            ActivityReminder.Record(1, Mokoena, Zulu, new DateTime(2026, 10, 1, 9, 0, 0, DateTimeKind.Utc)),
            ActivityReminder.Record(1, Smit, Zulu, new DateTime(2026, 10, 3, 22, 30, 0, DateTimeKind.Utc)),
            ActivityReminder.Record(2, Smit, Patel, new DateTime(2026, 10, 3, 21, 30, 0, DateTimeKind.Utc)));
        await db.SaveChangesAsync();

        var rows = (await WaitingAsync(db, Coordinator(), WombatRoles.Coordinator))!.Items.ToDictionary(item => item.Id);

        rows[1].LastReminder.Should().Be(new Wombat.Application.Features.Programme.Waiting.ActivityReminderDto(
            new DateTime(2026, 10, 3, 22, 30, 0, DateTimeKind.Utc), new DateOnly(2026, 10, 4), "Pieter Smit"));
        rows[1].RemindedToday.Should().BeTrue("00:30 on the 4th in South Africa is today");
        rows[2].LastReminder!.SentOnDay.Should().Be(new DateOnly(2026, 10, 3));
        rows[2].RemindedToday.Should().BeFalse("23:30 on the 3rd in South Africa was yesterday");
        rows[3].LastReminder.Should().BeNull();
        rows[3].RemindedToday.Should().BeFalse();
    }

    [Fact]
    public async Task APage_IsTwentyRows_HeldToTheLastPage()
    {
        await using var db = Seeded();
        for (var id = 1; id <= 25; id++)
        {
            AddRequest(db, id, Mahlangu, Zulu, Now.AddHours(-id));
        }

        await db.SaveChangesAsync();

        var first = (await WaitingAsync(db, Coordinator(), WombatRoles.Coordinator))!;
        first.Items.Should().HaveCount(20);
        first.Items[0].Id.Should().Be(25);
        (first.Page, first.PageSize, first.MatchCount).Should().Be((1, 20, 25));

        var second = (await WaitingAsync(db, Coordinator(), WombatRoles.Coordinator, page: 2))!;
        second.Items.Select(item => item.Id).Should().Equal([5, 4, 3, 2, 1]);

        var beyond = (await WaitingAsync(db, Coordinator(), WombatRoles.Coordinator, page: 9))!;
        beyond.Page.Should().Be(2, "a page past the end reads as the last");
        beyond.Items.Should().HaveCount(5);

        var home = (await WaitingAsync(
            db, Coordinator(), WombatRoles.Coordinator,
            pageSize: Wombat.Application.Features.Programme.Waiting.ListWaitingForAssessorsQueryHandler.HomeRows))!;
        home.Items.Should().HaveCount(5);
        home.MatchCount.Should().Be(25);
    }

    /// <summary>
    /// D5: the page admits the three roles that may remind. The registrar page's section (a registrar named) admits a
    /// Committee member too, who reads it and may not remind.
    /// </summary>
    [Fact]
    public async Task ACommitteeMember_ReadsOnlyOneRegistrarsSection_AndMayNotRemind()
    {
        await using var db = Seeded();
        AddRequest(db, 1, Mahlangu, Zulu, Now.AddDays(-1));
        AddRequest(db, 2, DuPlessis, Patel, Now.AddDays(-2));
        await db.SaveChangesAsync();

        (await WaitingAsync(db, CommitteeMember(), WombatRoles.CommitteeMember)).Should().BeNull(
            "the page is not a Committee member's");
        var section = (await WaitingAsync(db, CommitteeMember(), WombatRoles.CommitteeMember, subjectUserId: Mahlangu))!;
        section.Items.Select(item => item.Id).Should().Equal(1);
        section.MayRemind.Should().BeFalse();
    }

    [Fact]
    public async Task ARoleNotHeld_OrNoneOfTheFour_ReadsAsNotFound()
    {
        await using var db = Seeded();
        AddRequest(db, 1, Mahlangu, Zulu, Now.AddDays(-1));
        await db.SaveChangesAsync();

        (await WaitingAsync(db, Coordinator(), WombatRoles.SpecialityAdmin)).Should().BeNull("not held");
        (await WaitingAsync(db, TestPrincipals.InRole(WombatRoles.Assessor, Zulu, Kgk), WombatRoles.Assessor)).Should().BeNull();
        (await WaitingAsync(db, TestPrincipals.InRoles([WombatRoles.Trainee, WombatRoles.Coordinator], "rep", Kgk), WombatRoles.Coordinator))
            .Should().BeNull("a registrar in an oversight seat reads no peer's record (T185)");
    }

    [Fact]
    public async Task NothingWaiting_IsAnEmptyAnswer_NotNull()
    {
        await using var db = Seeded();

        var result = (await WaitingAsync(db, Coordinator(), WombatRoles.Coordinator))!;

        result.Items.Should().BeEmpty();
        (result.TotalCount, result.Page, result.Nominees.Count).Should().Be((0, 1, 0));
    }

    // T358, build review R3: a page past 100 rows, or a page before the first, is refused at the pipeline, as Programme
    // trainees' is (ListProgrammeTraineesQueryValidator).
    [Theory]
    [InlineData(1, 20, true)]
    [InlineData(1, 1, true)]
    [InlineData(3, 100, true)]
    [InlineData(1, 0, false)]
    [InlineData(1, 101, false)]
    [InlineData(0, 20, false)]
    [InlineData(-1, 20, false)]
    public void APageIsOneOrLater_AndHoldsOneTo100Rows(int page, int pageSize, bool valid)
        => new ListWaitingForAssessorsQueryValidator()
            .Validate(new ListWaitingForAssessorsQuery(Coordinator(), WombatRoles.Coordinator, Page: page, PageSize: pageSize))
            .IsValid.Should().Be(valid);

    [Fact]
    public void AQueryWithNoPrincipal_IsRefused()
        => new ListWaitingForAssessorsQueryValidator()
            .Validate(new ListWaitingForAssessorsQuery(null!, WombatRoles.Coordinator)).IsValid.Should().BeFalse();
}
