using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Email.Templates;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.Trainees;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.Trainees;

/// <summary>
/// T379 (flow 13; S4, S5, S11, F5, F7, Q12, Q13): what the trainee profile and Trainees' Completed and closed read. The
/// profile is null for an unknown id and another institution's alike; a running programme carries its last ratified
/// decision; an ended one how it ended and the account it belongs to now. The closed list is paged in SQL, the latest end
/// first, in scope, with the institution for the Administrator and an erased person under the pseudonym.
/// </summary>
public sealed class TraineeProfileEndReadTests
{
    private const int KgkId = 1;
    private const int OtherId = 2;
    private static readonly DateOnly Start = new(2023, 1, 15);
    private static readonly DateTimeOffset Now = new(2026, 10, 27, 8, 0, 0, TimeSpan.Zero);

    // ── The profile's read ─────────────────────────────────────────────────────

    [Fact]
    public async Task AnUnknownId_AndAnotherInstitutionsProfile_ReadAlikeAsNothing()
    {
        await using var db = SeededDb();
        var handler = Handler(db, Users());

        (await handler.Handle(new GetTraineeProfileByIdQuery(999, TestPrincipals.InstitutionalAdmin(KgkId)), CancellationToken.None))
            .Should().BeNull();
        (await handler.Handle(new GetTraineeProfileByIdQuery(3, TestPrincipals.InstitutionalAdmin(KgkId)), CancellationToken.None))
            .Should().BeNull("profile 3 is at another institution: never confirmed");
    }

    [Fact]
    public async Task ARunningProgramme_CarriesItsLastRatifiedDecision_AndNoEnd()
    {
        await using var db = SeededDb();
        RatifiedReview(db, 1, "molefe", CommitteeReviewType.AnnualProgression, CommitteeDecisionCategory.SatisfactoryProgress, new DateTime(2025, 10, 20, 9, 0, 0, DateTimeKind.Utc));
        RatifiedReview(db, 2, "molefe", CommitteeReviewType.PreGraduation, CommitteeDecisionCategory.Graduate, new DateTime(2026, 10, 25, 22, 30, 0, DateTimeKind.Utc));
        await db.SaveChangesAsync();

        var profile = await Handler(db, Users()).Handle(new GetTraineeProfileByIdQuery(1, TestPrincipals.InstitutionalAdmin(KgkId)), CancellationToken.None);

        profile!.LastRatifiedDecision.Should().Be(
            new LastRatifiedDecisionDto("Graduate (programme complete)", "Pre-graduation review", new DateOnly(2026, 10, 26)),
            "the latest by when it was ratified, its day on the South African calendar");
        profile.Ended.Should().BeNull();
        profile.Account.Should().BeNull();
        profile.FullName.Should().Be("Lerato Molefe");
        profile.MayExport.Should().BeTrue();
    }

    [Fact]
    public async Task AnEntrustmentOnlyReview_IsNamedByItsType_AndAnUnratifiedOneNotAtAll()
    {
        await using var db = SeededDb();
        RatifiedReview(db, 1, "molefe", CommitteeReviewType.EntrustmentOnly, null, new DateTime(2026, 9, 1, 9, 0, 0, DateTimeKind.Utc));
        db.Set<CommitteeReview>().Add(Review(2, "molefe", CommitteeReviewType.PreGraduation));
        await db.SaveChangesAsync();

        (await LastRatifiedDecision.ReadAsync(db, "molefe", CancellationToken.None))
            .Should().Be(new LastRatifiedDecisionDto(null, "Entrustment-only review", new DateOnly(2026, 9, 1)));
        (await LastRatifiedDecision.ReadAsync(db, "duplessis", CancellationToken.None)).Should().BeNull();
    }

    [Fact]
    public async Task AClosedFormativeReview_IsNotTheLastDecision_ThoughItsCloseStampedRatifiedOn()
    {
        await using var db = SeededDb();
        RatifiedReview(db, 1, "molefe", CommitteeReviewType.PreGraduation, CommitteeDecisionCategory.Graduate, new DateTime(2026, 6, 1, 9, 0, 0, DateTimeKind.Utc));
        var formative = Review(2, "molefe", CommitteeReviewType.AnnualProgression);
        formative.IsFormative = true;
        db.Set<CommitteeReview>().Add(formative);
        db.Entry(formative).Property(entity => entity.RatifiedOn).CurrentValue = new DateTime(2026, 9, 15, 9, 0, 0, DateTimeKind.Utc);
        await db.SaveChangesAsync();

        (await LastRatifiedDecision.ReadAsync(db, "molefe", CancellationToken.None))
            .Should().Be(new LastRatifiedDecisionDto("Graduate (programme complete)", "Pre-graduation review", new DateOnly(2026, 6, 1)));
    }

    [Fact]
    public async Task ACompletedProgramme_ReadsItsEnd_AndAnAccountWithNoRole_WithItsStars()
    {
        await using var db = SeededDb();
        End(db, 1, completed: true, new DateOnly(2026, 10, 26));
        db.Set<EntrustmentDecision>().Add(EntrustmentDecision.Issue(
            "molefe", 7, authorisedLevelId: 3, issuedOn: new DateOnly(2026, 10, 26), expiresOn: null, committeeReviewId: 30,
            chairUserId: "chair-1", rationale: "Ratified at the pre-graduation review.", evidenceLinks: StarEvidence.One()));
        await db.SaveChangesAsync();

        var profile = await Handler(db, Users(molefeRoles: [])).Handle(
            new GetTraineeProfileByIdQuery(1, TestPrincipals.InstitutionalAdmin(KgkId)), CancellationToken.None);

        profile!.Ended!.EndedOn.Should().Be(new DateOnly(2026, 10, 26));
        profile.Ended.Completed.Should().BeTrue();
        profile.Ended.TrainingYearAtEnd.Should().Be(4);
        profile.Account.Should().BeEquivalentTo(new TraineeAccountDto([], IsLocked: false, AnyStarIssued: true));
        profile.LastRatifiedDecision.Should().BeNull("an ended record names no decision");
    }

    [Fact]
    public async Task AWithdrawnProgramme_WithNoStar_AndItsPersonNowAnAssessor()
    {
        await using var db = SeededDb();
        End(db, 2, completed: false, new DateOnly(2026, 10, 24));
        await db.SaveChangesAsync();

        var profile = await Handler(db, Users(duPlessisRoles: [WombatRoles.Assessor])).Handle(
            new GetTraineeProfileByIdQuery(2, TestPrincipals.InstitutionalAdmin(KgkId)), CancellationToken.None);

        profile!.Ended!.Completed.Should().BeFalse();
        profile.Ended.EndedOn.Should().Be(new DateOnly(2026, 10, 24));
        profile.Account!.Roles.Should().Equal(WombatRoles.Assessor);
        profile.Account.AnyStarIssued.Should().BeFalse("the clause is read only for an account with no role");
    }

    [Fact]
    public async Task ALockedAccount_IsSaidToBeLocked()
    {
        await using var db = SeededDb();
        End(db, 2, completed: false, new DateOnly(2026, 10, 24));
        await db.SaveChangesAsync();

        var profile = await Handler(db, Users(duPlessisLocked: true)).Handle(
            new GetTraineeProfileByIdQuery(2, TestPrincipals.InstitutionalAdmin(KgkId)), CancellationToken.None);

        profile!.Account!.IsLocked.Should().BeTrue();
    }

    [Fact]
    public async Task AnErasedPersonsProfile_IsReadUnderThePseudonym_WithNoAccount()
    {
        await using var db = SeededDb();
        var profile = await db.Set<TraineeProfile>().SingleAsync(entity => entity.Id == 2);
        profile.Erase("deleted_user_1a2b3c4d", new DateOnly(2026, 10, 27));
        await db.SaveChangesAsync();

        var read = await Handler(db, Users()).Handle(
            new GetTraineeProfileByIdQuery(2, TestPrincipals.InstitutionalAdmin(KgkId)), CancellationToken.None);

        read!.IsErased.Should().BeTrue();
        read.FullName.Should().Be("deleted_user_1a2b3c4d");
        read.Email.Should().BeEmpty();
        read.Account.Should().BeNull();
        read.Ended.Should().Be(new Wombat.Application.Features.Users.EndedProgrammeDto(new DateOnly(2026, 10, 27), false) { TrainingYearAtEnd = 2 });
    }

    // ── Completed and closed ──────────────────────────────────────────────────

    [Fact]
    public async Task TheClosedList_IsTheLatestEndFirst_InScope_AndLeavesRunningProgrammesOut()
    {
        await using var db = SeededDb();
        End(db, 1, completed: true, new DateOnly(2026, 10, 26));
        End(db, 2, completed: false, new DateOnly(2026, 10, 24));
        End(db, 3, completed: true, new DateOnly(2026, 10, 27));
        await db.SaveChangesAsync();

        var page = await ListHandler(db).Handle(new ListEndedProfilesQuery(1, 20, TestPrincipals.InstitutionalAdmin(KgkId)), CancellationToken.None);

        page.TotalCount.Should().Be(2, "profile 3 is another institution's, profile 4 still runs");
        page.Rows.Select(row => (row.ProfileId, row.Name, row.Ended!.Completed)).Should().Equal(
            (1, "Lerato Molefe", true), (2, "Pieter du Plessis", false));
        page.Rows[0].Ended!.TrainingYearAtEnd.Should().Be(4);
        page.Rows[0].Email.Should().Be("molefe@kgk.wombat.local");
        page.Rows.Should().OnlyContain(row => row.InstitutionName == null, "only the Administrator's list names the institution");
    }

    [Fact]
    public async Task TheAdministratorsClosedList_NamesEachInstitution_AndAnErasedPersonByThePseudonym()
    {
        await using var db = SeededDb();
        End(db, 1, completed: true, new DateOnly(2026, 10, 26));
        (await db.Set<TraineeProfile>().SingleAsync(entity => entity.Id == 3)).Erase("deleted_user_1a2b3c4d", new DateOnly(2026, 10, 27));
        await db.SaveChangesAsync();

        var page = await ListHandler(db).Handle(new ListEndedProfilesQuery(1, 20, TestPrincipals.Administrator()), CancellationToken.None);

        page.Rows.Select(row => (row.Name, row.InstitutionName, row.IsErased, row.Ended!.Completed)).Should().Equal(
            ("deleted_user_1a2b3c4d", "Other Hospital", true, false),
            ("Lerato Molefe", "Kgosi Kgari Teaching Hospital", false, true));
    }

    [Fact]
    public async Task TheClosedList_IsPagedAtTwenty_AndAPagePastTheEndReadsTheLast()
    {
        await using var db = SeededDb();
        for (var index = 0; index < 25; index++)
        {
            var ended = Profile(100 + index, $"user-{index}", KgkId, Start);
            var day = new DateOnly(2026, 1, 1).AddDays(index);
            ended.Deactivate(day, today: day);
            db.Set<TraineeProfile>().Add(ended);
        }

        await db.SaveChangesAsync();
        var handler = ListHandler(db);

        var first = await handler.Handle(new ListEndedProfilesQuery(1, 20, TestPrincipals.InstitutionalAdmin(KgkId)), CancellationToken.None);
        var last = await handler.Handle(new ListEndedProfilesQuery(9, 20, TestPrincipals.InstitutionalAdmin(KgkId)), CancellationToken.None);
        var odd = await handler.Handle(new ListEndedProfilesQuery(1, 7, TestPrincipals.InstitutionalAdmin(KgkId)), CancellationToken.None);

        first.TotalCount.Should().Be(25);
        first.Rows.Should().HaveCount(20);
        first.Rows[0].ProfileId.Should().Be(124, "the latest end first");
        last.Page.Should().Be(2);
        last.Rows.Should().HaveCount(5);
        odd.PageSize.Should().Be(20, "a size the pager does not offer is the default");
    }

    [Fact]
    public async Task ATraineeHoldingAdmin_IsRefusedBeforeAnythingIsRead()
    {
        await using var db = SeededDb();

        var act = () => ListHandler(db).Handle(
            new ListEndedProfilesQuery(1, 20, TestPrincipals.InRoles([WombatRoles.InstitutionalAdmin, WombatRoles.Trainee], "admin-1", KgkId)),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    // ── The graduation email (Q12) ─────────────────────────────────────────────

    [Fact]
    public void TheGraduationEmail_NoLongerClaimsARatification_AndLinksMyProgress()
    {
        var message = GraduationEmail.Build(
            "molefe@kgk.wombat.local", "Lerato Molefe", "Paediatric EPA Curriculum", new DateOnly(2026, 10, 26),
            "https://wombat.example/portfolio/progress");

        foreach (var body in new[] { message.HtmlBody, message.TextBody })
        {
            body.Should().NotContain("ratified").And.NotContain("final entrustment decisions");
            body.Should().Contain("Your record of the programme stays in Wombat, read-only. Sign in to read it on My progress, " +
                                  "to export your portfolio, and to download the certificate of any STAR issued to you.");
            body.Should().Contain("https://wombat.example/portfolio/progress");
        }

        message.HtmlBody.Should().Contain("<a class=\"btn\" href=\"https://wombat.example/portfolio/progress\">Open My progress</a>");
        message.TextBody.Should().Contain("Open My progress: https://wombat.example/portfolio/progress");
        message.Tags.Should().Contain("graduation");
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static GetTraineeProfileByIdQueryHandler Handler(ApplicationDbContext db, IUserAdministrationService users)
        => new(db, users, new FixedClock(Now));

    private static ListEndedProfilesQueryHandler ListHandler(ApplicationDbContext db) => new(db, Users());

    private static FakeUserDirectory Users(
        IReadOnlyCollection<string>? molefeRoles = null,
        IReadOnlyCollection<string>? duPlessisRoles = null,
        bool duPlessisLocked = false)
        => new FakeUserDirectory()
            .With(new UserIdentityDetails("molefe", "molefe@kgk.wombat.local", "Lerato", "Molefe", KgkId, [], [], molefeRoles ?? [WombatRoles.Trainee]))
            .With(new UserIdentityDetails("duplessis", "duplessis@kgk.wombat.local", "Pieter", "du Plessis", KgkId, [], [],
                duPlessisRoles ?? [], IsLockedOut: duPlessisLocked, IsDeactivated: duPlessisLocked))
            .With(new UserIdentityDetails("other", "other@other.local", "Olwethu", "Other", OtherId, [], [], [WombatRoles.Trainee]))
            .With(new UserIdentityDetails("nkosi", "nkosi@kgk.wombat.local", "Thandi", "Nkosi", KgkId, [], [], [WombatRoles.Trainee]));

    private static void End(ApplicationDbContext db, int profileId, bool completed, DateOnly day)
    {
        var profile = db.Set<TraineeProfile>().Local.Single(entity => entity.Id == profileId);
        if (completed)
        {
            profile.Complete(day, today: day);
        }
        else
        {
            profile.Deactivate(day, today: day);
        }
    }

    private static CommitteeReview Review(int id, string traineeUserId, CommitteeReviewType type) => new()
    {
        Id = id,
        TraineeUserId = traineeUserId,
        PanelId = 20,
        AcademicYear = 2026,
        Semester = 2,
        ReviewPeriodFrom = new DateOnly(2026, 1, 1),
        ReviewPeriodTo = new DateOnly(2026, 12, 31),
        ScheduledOn = new DateOnly(2026, 10, 13),
        ReviewType = type
    };

    private static void RatifiedReview(
        ApplicationDbContext db, int id, string traineeUserId, CommitteeReviewType type, CommitteeDecisionCategory? category, DateTime ratifiedOn)
    {
        var review = Review(id, traineeUserId, type);
        review.Decisions.Add(CommitteeDecision.Create(category, "Ratified.", null, "chair-1", ratifiedOn.AddHours(-1), []));
        db.Set<CommitteeReview>().Add(review);
        db.Entry(review).Property(entity => entity.RatifiedOn).CurrentValue = ratifiedOn;
    }

    private static ApplicationDbContext SeededDb()
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        db.Set<Institution>().AddRange(
            new Institution { Id = KgkId, Name = "Kgosi Kgari Teaching Hospital", ShortCode = "KGK", IsActive = true, CreatedOn = DateTime.UtcNow },
            new Institution { Id = OtherId, Name = "Other Hospital", ShortCode = "OTH", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.Set<Speciality>().Add(new Speciality { Id = 1, CollegeId = KgkId, Name = "Paediatrics", IsActive = true });
        db.Set<SubSpeciality>().Add(new SubSpeciality { Id = 1, SpecialityId = 1, Name = "Paediatrics", IsActive = true });
        db.Set<Curriculum>().Add(new Curriculum { Id = 1, SubSpecialityId = 1, Name = "Paediatric EPA Curriculum", Version = "11.1" });
        db.Set<TraineeProfile>().AddRange(
            Profile(1, "molefe", KgkId, Start),
            Profile(2, "duplessis", KgkId, new DateOnly(2025, 1, 15)),
            Profile(3, "other", OtherId, Start),
            Profile(4, "nkosi", KgkId, Start));
        db.SaveChanges();
        return db;
    }

    private static TraineeProfile Profile(int id, string userId, int institutionId, DateOnly start) => new()
    {
        Id = id,
        UserId = userId,
        InstitutionId = institutionId,
        CurriculumId = 1,
        ProgrammeStartDate = start,
        ExpectedCompletionDate = start.AddYears(4),
        IsActive = true
    };

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
