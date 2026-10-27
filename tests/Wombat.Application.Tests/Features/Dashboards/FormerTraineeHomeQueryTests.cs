using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Dashboards.FormerTrainee;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Application.Features.Trainees;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.Dashboards;

/// <summary>
/// T379 (flow 13, S8 and S9; R3-H01, R3-H02, R3-H03): a former trainee's Home reads their own record, the preferred
/// profile's end and the record's STARs (F6), and answers nothing for a current Trainee, though the <c>trainee_record</c>
/// claim is issued to one too.
/// </summary>
public sealed class FormerTraineeHomeQueryTests
{
    private const string Molefe = "molefe";
    private const string DuPlessis = "du-plessis";
    private const string Registrar = "still-training";
    private const int Host = 1;
    private const int CurriculumId = 10;
    private const int Cpsa = 42;

    private static readonly DateOnly Start = new(2023, 1, 15);
    private static readonly DateOnly LastDay = new(2026, 9, 26);
    private static readonly DateTimeOffset Now = new(2026, 9, 26, 12, 0, 0, TimeSpan.Zero);

    /// <summary>A month after the end: what the STARs are read as of, which T312 holds to the programme's last day.</summary>
    private static readonly DateTimeOffset Later = new(2026, 10, 27, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ACompletedProgramme_ReadsItsDay_TheTrainingYearItEndedIn_AndItsStarsInForce()
    {
        await using var db = CreateDb();
        await SeedAsync(db);
        Star(db, Molefe, 1);
        Star(db, Molefe, 2);
        await db.SaveChangesAsync();
        await CompleteAsync(db, profileId: 1);

        var home = await HomeAsync(db, RecordHolder(Molefe));

        home.Should().Be(new FormerTraineeHomeDto(LastDay, Completed: true, TrainingYearAtEnd: 4, new RecordStarsDto(true, 2)));
    }

    [Fact]
    public async Task AWithdrawal_WithNoStarEverIssued_ReadsItsDay_AndNoStar()
    {
        // R3-H02: Pieter du Plessis's Home shows the STARs row as plain text.
        await using var db = CreateDb();
        await SeedAsync(db);
        await WithdrawAsync(db, profileId: 2);

        var home = await HomeAsync(db, RecordHolder(DuPlessis));

        home.Should().Be(new FormerTraineeHomeDto(LastDay, Completed: false, TrainingYearAtEnd: 2, new RecordStarsDto(false, 0)));
    }

    [Fact]
    public async Task AStarRevokedBeforeTheEnd_IsIssued_ButNoneIsInForce()
    {
        // F6: any ratified decision, in force or not, offers My authorisations; Home's PROPOSED sentence says none is in force.
        await using var db = CreateDb();
        await SeedAsync(db);
        var star = Star(db, Molefe, 1);
        await db.SaveChangesAsync();
        star.Revoke("Recorded in error", "chair-1", new DateTime(2026, 9, 20, 8, 0, 0, DateTimeKind.Utc));
        await db.SaveChangesAsync();
        await CompleteAsync(db, profileId: 1);

        var home = await HomeAsync(db, RecordHolder(Molefe));

        home!.Stars.Should().Be(new RecordStarsDto(AnyIssued: true, InForce: 0));
    }

    [Fact]
    public async Task ACurrentTrainee_HoldingTheRecordClaim_IsAnsweredNothing()
    {
        // S8: the claim is issued for a running profile too, so it says nothing about an end.
        await using var db = CreateDb();
        await SeedAsync(db);

        (await HomeAsync(db, RecordHolder(Registrar, WombatRoles.Trainee))).Should().BeNull();
    }

    [Fact]
    public async Task AFormerTraineeActingAsAssessor_IsStillAnswered()
    {
        // H03, Q25: Home shows them the Assessor's dashboard; the record is theirs whatever role they hold.
        await using var db = CreateDb();
        await SeedAsync(db);
        await CompleteAsync(db, profileId: 1);

        var home = await HomeAsync(db, RecordHolder(Molefe, WombatRoles.Assessor));

        home!.Completed.Should().BeTrue();
        home.EndedOn.Should().Be(LastDay);
    }

    [Fact]
    public async Task APersonWithNoProfile_IsAnsweredNothing()
    {
        await using var db = CreateDb();
        await SeedAsync(db);

        (await HomeAsync(db, RecordHolder("nobody"))).Should().BeNull();
    }

    [Fact]
    public async Task OnlyTheCallersOwnRecordIsRead()
    {
        // The user id is the principal's: du Plessis's own record, never Molefe's, though both ended.
        await using var db = CreateDb();
        await SeedAsync(db);
        Star(db, Molefe, 1);
        await db.SaveChangesAsync();
        await CompleteAsync(db, profileId: 1);
        await WithdrawAsync(db, profileId: 2);

        var home = await HomeAsync(db, RecordHolder(DuPlessis));

        home!.Completed.Should().BeFalse();
        home.Stars.AnyIssued.Should().BeFalse();
    }

    // ─── Fixtures ────────────────────────────────────────────────────────────

    private static Task<FormerTraineeHomeDto?> HomeAsync(ApplicationDbContext db, ClaimsPrincipal caller)
        => new GetFormerTraineeHomeQueryHandler(db, new FixedClock(Later))
            .Handle(new GetFormerTraineeHomeQuery(caller), CancellationToken.None);

    /// <summary>The person themselves, holding the record claim and whatever roles are given (none: a graduate).</summary>
    private static ClaimsPrincipal RecordHolder(string userId, params string[] roles)
    {
        var principal = TestPrincipals.InRoles(roles, userId, Host);
        ((ClaimsIdentity)principal.Identity!).AddClaim(new Claim(WombatClaimTypes.TraineeRecord, "true"));
        return principal;
    }

    private static async Task CompleteAsync(ApplicationDbContext db, int profileId)
    {
        await new CompleteTraineeProfileCommandHandler(
                db, new Mock<IUserAdministrationService>().Object, new Mock<IEmailSender>().Object, new CreditApplier(db),
                new TraineeCreditLock(db), new FixedClock(Now))
            .Handle(new CompleteTraineeProfileCommand(profileId, LastDay, TestPrincipals.Administrator()), CancellationToken.None);
        db.ChangeTracker.Clear();
    }

    private static async Task WithdrawAsync(ApplicationDbContext db, int profileId)
    {
        await new DeactivateTraineeProfileCommandHandler(
                db, new Mock<IUserAdministrationService>().Object, new CreditApplier(db), new TraineeCreditLock(db), new FixedClock(Now))
            .Handle(new DeactivateTraineeProfileCommand(profileId, LastDay, TestPrincipals.Administrator()), CancellationToken.None);
        db.ChangeTracker.Clear();
    }

    private static EntrustmentDecision Star(ApplicationDbContext db, string traineeUserId, int epaId)
    {
        var decision = EntrustmentDecision.Issue(
            traineeUserId, epaId, 4206, new DateOnly(2026, 6, 1), expiresOn: null,
            committeeReviewId: 30, "chair-1", "Consistent across the period.", StarEvidence.One());
        db.EntrustmentDecisions.Add(decision);
        return decision;
    }

    /// <summary>
    /// Two national EPAs on one curriculum; Lerato Molefe from 2023-01-15 (training year 4 on her last day), Pieter du
    /// Plessis from 2025-01-15 (training year 2), and a registrar still in the programme; a review of Molefe's programme.
    /// </summary>
    private static async Task SeedAsync(ApplicationDbContext db)
    {
        db.Institutions.Add(new Institution { Id = Host, Name = "Kgosi Kgari Teaching Hospital" });
        db.Colleges.Add(new College { Id = 1, Name = "CPSA", ShortCode = "CPSA" });
        db.Specialities.Add(new Speciality { Id = 1, CollegeId = 1, Name = "Paediatrics" });
        db.SubSpecialities.Add(new SubSpeciality { Id = 1, SpecialityId = 1, Name = "General Paediatrics" });
        db.Set<EntrustmentScale>().Add(new EntrustmentScale { Id = Cpsa, Name = "CPSA Paediatric Entrustment Scale v11.1" });
        string[] rungs = ["1", "2", "3a", "3b", "4", "5"];
        for (var order = 1; order <= rungs.Length; order++)
        {
            db.Set<EntrustmentLevel>().Add(new EntrustmentLevel { Id = 4200 + order, ScaleId = Cpsa, Order = order, Label = rungs[order - 1] });
        }

        db.Curricula.Add(new Curriculum
        {
            Id = CurriculumId, SubSpecialityId = 1, Name = "Paediatric EPA Curriculum", Version = "11.1",
            EffectiveFrom = new DateOnly(2023, 1, 1), IsActive = true
        });
        db.Epas.Add(new Epa { Id = 1, SubSpecialityId = 1, Code = "PAED-001", Title = "Clerk an acute admission" });
        db.Epas.Add(new Epa { Id = 2, SubSpecialityId = 1, Code = "PAED-002", Title = "Counsel a family" });
        foreach (var (id, epaId) in new[] { (101, 1), (102, 2) })
        {
            db.CurriculumItems.Add(new CurriculumItem
            {
                Id = id, CurriculumId = CurriculumId, EpaId = epaId, RequiredCount = 3, QuotaPeriod = QuotaPeriod.Semester,
                MinimumLevelOrder = 6, WindowMonths = 12, ScaleId = Cpsa
            });
        }

        foreach (var (id, userId, start) in new[] { (1, Molefe, Start), (2, DuPlessis, new DateOnly(2025, 1, 15)), (3, Registrar, Start) })
        {
            db.TraineeProfiles.Add(new TraineeProfile
            {
                Id = id, UserId = userId, InstitutionId = Host, CurriculumId = CurriculumId,
                ProgrammeStartDate = start, ExpectedCompletionDate = start.AddYears(4), IsActive = true
            });
        }

        db.CommitteeReviews.Add(new CommitteeReview
        {
            Id = 30, TraineeUserId = Molefe, PanelId = 1, AcademicYear = 2026, Semester = 1,
            ReviewPeriodFrom = new DateOnly(2026, 1, 1), ReviewPeriodTo = new DateOnly(2026, 6, 30), ScheduledOn = new DateOnly(2026, 6, 30)
        });

        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }

    private static ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
