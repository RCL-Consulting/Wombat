using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Dashboards.CommitteeMember;
using Wombat.Application.Features.Dashboards.Coordinator;
using Wombat.Application.Features.Dashboards.SpecialityAdmin;
using Wombat.Application.Features.Dashboards.SubSpecialityAdmin;
using Wombat.Application.Features.Programme.Trainees;
using Wombat.Application.Tests.Features.Programme;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Identity;
using Wombat.Domain.Invitations;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Scheduling;
using Wombat.Tests.Shared;
using Cast = Wombat.Application.Tests.Features.Programme.ProgrammeCast;
using Waiting = Wombat.Application.Tests.Features.Programme.ProgrammeWaitingFixture;

namespace Wombat.Application.Tests.Features.Dashboards;

/// <summary>
/// The four oversight Homes (T358, flow 06, lane B; R2-Home; Q1, Q3, Q5, E3, E4, E5): each card is the first five rows of
/// the list it previews, read by that list's own reader, as the Home's own role. The cast is Step 3.52's
/// (<see cref="ProgrammeCast" />) for the registrars and Step 3.30's requests (<see cref="ProgrammeWaitingFixture" />)
/// for what waits.
/// </summary>
public sealed class OversightHomesQueryTests
{
    // ---- Registrars and Targets by EPA: Programme trainees' first five (Q1, Q5) ----

    [Fact]
    public async Task TheCommitteeMembersHome_IsProgrammeTraineesFirstFive_AndTheSameRegistrarsTargetsByEpa()
    {
        var (db, users) = Cast.Build();
        await using var _ = db;

        var home = await CommitteeHome(db, users, Cast.CommitteeMember());
        var page = await Roster(db, users, Cast.CommitteeMember(), WombatRoles.CommitteeMember);

        // The institution's six current registrars (the surgical one too: a Committee member reads the institution), the
        // other hospital's never; five named, in the page's order.
        home.Registrars.Total.Should().Be(6);
        home.Registrars.Rows.Should().HaveCount(ListProgrammeTraineesQueryHandler.HomeRows);
        home.Registrars.Rows.Select(row => row.TraineeUserId)
            .Should().Equal(page.Read.Rows.Take(5).Select(row => row.TraineeUserId), "the card and its list cannot disagree");
        home.Registrars.Beyond.Should().Be(1, "\"1 more in Programme trainees.\"");
        home.Registrars.Rows.Select(row => row.TraineeUserId).Should().NotContain(Cast.Elsewhere);
        home.Coverage.Should().BeEquivalentTo(page.Read.Coverage, options => options.ComparingRecordsByMembers().WithStrictOrdering());
        home.Coverage.CurrentSemesterName.Should().Be("Semester 2, 2026");
    }

    [Fact]
    public async Task TheExternalMember_WhoHoldsNoSubSpeciality_ReadsTheInstitutionsRegistrars()
    {
        // T290, Step 2.37: a Committee member reads the institution, whatever sub-speciality claims the member holds.
        var (db, users) = Cast.Build();
        await using var _ = db;

        var home = await CommitteeHome(db, users, TestPrincipals.InRole(WombatRoles.CommitteeMember, "van-rensburg", Cast.Kgk));

        home.Registrars.Total.Should().Be(6);
    }

    [Fact]
    public async Task ARegistrarOnTheCommittee_ReadsNoPeer_AndTheEmptyCardsStillNameTheSemester()
    {
        // T185: someone who holds Trainee is a trainee first, and names no peer beside the targets they have met.
        var (db, users) = Cast.Build();
        await using var _ = db;
        var registrar = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, Cast.Mahlangu),
                new Claim(ClaimTypes.Role, WombatRoles.Trainee),
                new Claim(ClaimTypes.Role, WombatRoles.CommitteeMember),
                new Claim(WombatClaimTypes.InstitutionId, Cast.Kgk.ToString(System.Globalization.CultureInfo.InvariantCulture))
            ],
            "test"));

        var home = await CommitteeHome(db, users, registrar);

        home.Registrars.Rows.Should().BeEmpty();
        home.Registrars.Total.Should().Be(0);
        home.Coverage.Epas.Should().BeEmpty();
        home.Coverage.CurrentSemesterName.Should().Be("Semester 2, 2026");
    }

    [Fact]
    public async Task ACallerWhoDoesNotHoldTheRole_ReadsNothing_NeverTheUnionOfTheRolesHeld()
    {
        // E4: each Home reads as its own role. An Administrator who does not sit on the committee has no Committee member's
        // Home to read, and until T358 the card read every institution for one.
        var (db, users) = Cast.Build();
        await using var _ = db;

        var home = await CommitteeHome(db, users, TestPrincipals.InRole(WombatRoles.Administrator, "admin", Cast.Kgk));

        home.Registrars.Total.Should().Be(0);
    }

    [Fact]
    public async Task TheSpecialityAdminsHome_ReadsTheSpecialitysRegistrars_AsSpecialityAdmin()
    {
        // E4: a Speciality admin who also sits on the committee reads the speciality on this Home, not the institution.
        var (db, users) = Cast.Build();
        await using var _ = db;
        var both = TestPrincipals.InRole(WombatRoles.SpecialityAdmin, "mokoena", Cast.Kgk, specialityId: Cast.PaediatricsSpeciality);
        ((ClaimsIdentity)both.Identity!).AddClaim(new Claim(ClaimTypes.Role, WombatRoles.CommitteeMember));

        var speciality = await SpecialityHome(db, users, both);
        var committee = await CommitteeHome(db, users, both);

        speciality.Registrars.Total.Should().Be(5, "the surgical registrar is in another speciality");
        speciality.Registrars.Rows.Select(row => row.TraineeUserId)
            .Should().Equal(Cast.DuPlessis, Cast.Mahlangu, Cast.Ndlovu, Cast.Dlamini, Cast.Molefe);
        speciality.Registrars.Beyond.Should().Be(0);
        committee.Registrars.Total.Should().Be(6, "the same person's Committee member's Home reads the institution");
        speciality.Waiting!
            .MatchCount.Should().Be(0);
    }

    [Fact]
    public async Task TheSubSpecialityAdminsHome_ReadsTheSubSpecialitysRegistrars()
    {
        var (db, users) = Cast.Build();
        await using var _ = db;

        var home = await SubSpecialityHome(db, users, Cast.SubSpecialityAdmin());

        home.Registrars.Total.Should().Be(5);
        home.Coverage.Epas.Select(epa => epa.EpaCode).Should().NotContain("SURG-001");
    }

    // ---- Waiting for assessors: the page's first five (Q3, E3) ----

    [Fact]
    public async Task TheAdminsWaitingCard_IsThePagesFirstFive_WithItsCounts()
    {
        await using var db = Waiting.Seeded();
        for (var i = 1; i <= 7; i++)
        {
            Waiting.AddRequest(db, i, Waiting.Mahlangu, Waiting.Zulu, Waiting.Now.AddDays(-i));
        }

        // E3: a request whose field names nobody is never listed.
        Waiting.AddRequest(db, 8, Waiting.Mahlangu, null, Waiting.Now.AddDays(-20));
        await db.SaveChangesAsync();

        var home = await SpecialityHome(db, Waiting.Directory(), Waiting.SpecialityAdmin());
        var page = await Waiting.WaitingAsync(db, Waiting.SpecialityAdmin(), WombatRoles.SpecialityAdmin);

        var card = home.Waiting!;
        card.Items.Select(item => item.Id).Should().Equal([7, 6, 5, 4, 3], "oldest first, five");
        card.Items.Select(item => item.Id).Should().Equal(page!.Items.Take(5).Select(item => item.Id));
        (card.MatchCount, card.MatchOverdueCount).Should().Be((7, 1), "\"7 waiting, 1 overdue\"; only the 7-day-old is overdue");
        card.Items.Should().OnlyContain(item => item.Holder!.Name == "Thandi Zulu", "each row says whom it waits with (E6)");
        (card.DueDays, card.NudgeDays).Should().Be((7, 5));
    }

    [Fact]
    public async Task TheCoordinatorsWaitingCard_IsThePagesFirstFive_InTheInstitutionsScope()
    {
        await using var db = Waiting.Seeded();
        Waiting.AddRequest(db, 1, Waiting.DuPlessis, Waiting.Patel, Waiting.Now.AddDays(-8).AddMinutes(-5));
        Waiting.AddRequest(db, 2, Waiting.Mahlangu, Waiting.Zulu, Waiting.Now.AddDays(-8));
        Waiting.AddRequest(db, 3, Waiting.DuPlessis, Waiting.Khumalo, Waiting.Now.AddHours(-2));
        Waiting.AddRequest(db, 4, Waiting.Mahlangu, Waiting.Zulu, Waiting.Now.AddDays(-9), institutionId: Waiting.OtherHospital);
        await db.SaveChangesAsync();

        var home = await CoordinatorHome(db, Waiting.Directory(), Waiting.Coordinator());

        // Step 3.30: "3 waiting, 2 overdue", oldest first; another hospital's never.
        home.Waiting!.Items.Select(item => item.Id).Should().Equal([1, 2, 3]);
        (home.Waiting.MatchCount, home.Waiting.MatchOverdueCount).Should().Be((3, 2));
    }

    [Fact]
    public async Task ACoordinatorWithNoInstitution_ReadsNoCard()
    {
        await using var db = Waiting.Seeded();
        Waiting.AddRequest(db, 1, Waiting.Mahlangu, Waiting.Zulu, Waiting.Now.AddDays(-8));
        await db.SaveChangesAsync();
        var nowhere = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "nowhere"), new Claim(ClaimTypes.Role, WombatRoles.Coordinator)], "test"));

        var home = await CoordinatorHome(db, Waiting.Directory(), nowhere);

        home.Waiting.Should().BeNull();
        home.NothingFiled.Total.Should().Be(0);
    }

    // ---- Nothing filed in 30 days: Programme trainees' filter (E5) ----

    [Fact]
    public async Task TheCoordinatorsNothingFiled_IsProgrammeTraineesFilteredNothingFiled()
    {
        // E5: a registrar admitted at least 30 days ago with nothing filed in the last 30 is listed; the cast was admitted
        // at their programme start, so all six at the institution have filed nothing and are listed, five named.
        var (db, users) = Cast.Build();
        await using var _ = db;
        Cast.AddRegistrar(db, users, 120, "new-registrar", "Lindiwe", "New", trainingYear: 1, admittedOn: Cast.D.AddDays(-10));
        await db.SaveChangesAsync();

        var home = await CoordinatorHome(db, users, Cast.Coordinator());
        var page = await Roster(db, users, Cast.Coordinator(), WombatRoles.Coordinator, nothingFiled: true);

        home.NothingFiled.Rows.Select(row => row.TraineeUserId)
            .Should().Equal(page.Read.Rows.Take(5).Select(row => row.TraineeUserId));
        home.NothingFiled.Total.Should().Be(page.MatchCount);
        home.NothingFiled.Rows.Select(row => row.TraineeUserId).Should().NotContain("new-registrar",
            "admitted ten days ago: the 30 days start at admission (E5, Step 2.32)");
        home.NothingFiled.Rows.Should().OnlyContain(row => row.NothingFiled);
    }

    // ---- Invitations nearing expiry ----

    [Fact]
    public async Task AnInvitation_ExpiringWithinThreeSouthAfricanDays_IsListedWithItsRolesLabel()
    {
        await using var db = Waiting.Seeded();
        AddInvitation(db, 1, "expiring@kgk.wombat.local", WombatRoles.Trainee, Waiting.Kgk, expiresOn: new DateOnly(2026, 10, 7));
        AddInvitation(db, 2, "member@kgk.wombat.local", WombatRoles.CommitteeMember, Waiting.Kgk, expiresOn: new DateOnly(2026, 10, 4));
        AddInvitation(db, 3, "later@kgk.wombat.local", WombatRoles.Trainee, Waiting.Kgk, expiresOn: new DateOnly(2026, 10, 8));
        AddInvitation(db, 4, "gone@kgk.wombat.local", WombatRoles.Trainee, Waiting.Kgk, expiresOn: new DateOnly(2026, 10, 3));
        AddInvitation(db, 5, "elsewhere@oth.wombat.local", WombatRoles.Trainee, Waiting.OtherHospital, expiresOn: new DateOnly(2026, 10, 5));
        await db.SaveChangesAsync();

        var home = await CoordinatorHome(db, Waiting.Directory(), Waiting.Coordinator());

        home.ExpiringInvitations.Select(invitation => (invitation.Email, invitation.TargetRoleLabel, invitation.ExpiresOn))
            .Should().Equal(
                ("member@kgk.wombat.local", "Committee member", new DateOnly(2026, 10, 4)),
                ("expiring@kgk.wombat.local", "Trainee", new DateOnly(2026, 10, 7)));
    }

    [Fact]
    public async Task TheExpiryWindow_IsTheSouthAfricanDay_NotTheUtcOne()
    {
        // T325: at 23:30 UTC on 2026-10-03 it is already 2026-10-04 in South Africa, so an invitation whose last day was
        // 2026-10-03 has expired, and one expiring on 2026-10-07 is within three days.
        await using var db = Waiting.Seeded();
        AddInvitation(db, 1, "yesterday@kgk.wombat.local", WombatRoles.Trainee, Waiting.Kgk, expiresOn: new DateOnly(2026, 10, 3));
        AddInvitation(db, 2, "expiring@kgk.wombat.local", WombatRoles.Trainee, Waiting.Kgk, expiresOn: new DateOnly(2026, 10, 7));
        await db.SaveChangesAsync();

        var home = await CoordinatorHome(db, Waiting.Directory(), Waiting.Coordinator(), new DateTime(2026, 10, 3, 23, 30, 0, DateTimeKind.Utc));

        home.ExpiringInvitations.Select(invitation => invitation.Email).Should().Equal("expiring@kgk.wombat.local");
    }

    // ---- the handlers ----

    private static Task<CommitteeMemberDashboardSummaryDto> CommitteeHome(
        ApplicationDbContext db, IUserAdministrationService users, ClaimsPrincipal principal)
        => new GetCommitteeMemberDashboardSummaryQueryHandler(db, users, Clock())
            .Handle(new GetCommitteeMemberDashboardSummaryQuery(principal), CancellationToken.None);

    private static Task<SpecialityAdminDashboardSummaryDto> SpecialityHome(
        ApplicationDbContext db, IUserAdministrationService users, ClaimsPrincipal principal)
        => new GetSpecialityAdminDashboardSummaryQueryHandler(
                db, users, new ReminderRecipients(db), Options.Create(new DashboardThresholds()), Clock())
            .Handle(new GetSpecialityAdminDashboardSummaryQuery(principal), CancellationToken.None);

    private static Task<SubSpecialityAdminDashboardSummaryDto> SubSpecialityHome(
        ApplicationDbContext db, IUserAdministrationService users, ClaimsPrincipal principal)
        => new GetSubSpecialityAdminDashboardSummaryQueryHandler(
                db, users, new ReminderRecipients(db), Options.Create(new DashboardThresholds()), Clock())
            .Handle(new GetSubSpecialityAdminDashboardSummaryQuery(principal), CancellationToken.None);

    private static Task<CoordinatorDashboardSummaryDto> CoordinatorHome(
        ApplicationDbContext db, IUserAdministrationService users, ClaimsPrincipal principal, DateTime? now = null)
        => new GetCoordinatorDashboardSummaryQueryHandler(
                db, users, new ReminderRecipients(db), Options.Create(new DashboardThresholds()), Clock(now))
            .Handle(new GetCoordinatorDashboardSummaryQuery(principal), CancellationToken.None);

    private static async Task<ProgrammeTraineesDto> Roster(
        ApplicationDbContext db, IUserAdministrationService users, ClaimsPrincipal principal, string role, bool nothingFiled = false)
        => (await new ListProgrammeTraineesQueryHandler(db, users, Clock())
            .Handle(new ListProgrammeTraineesQuery(principal, role, NothingFiled: nothingFiled), CancellationToken.None))!;

    /// <summary>10:00 on D in South Africa, Step 3.52's and 3.30's moment.</summary>
    private static TimeProvider Clock(DateTime? now = null) => new AssessorReads.FixedClock(now ?? Cast.DAt10);

    private static void AddInvitation(
        ApplicationDbContext db, int id, string email, string role, int institutionId, DateOnly expiresOn)
        => db.Invitations.Add(new Invitation
        {
            Id = id, Email = email, TokenHash = $"hash-{id}", TargetRole = role, InstitutionId = institutionId,
            IssuedByUserId = "admin-1", IssuedOn = Cast.DAt10.AddDays(-7), ExpiresOn = expiresOn
        });
}
