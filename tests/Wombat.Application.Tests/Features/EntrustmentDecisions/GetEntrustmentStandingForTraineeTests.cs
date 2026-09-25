using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.EntrustmentDecisions;

/// <summary>
/// Where a trainee stands on each EPA against Annexure A's target for their training year and against the exit rule
/// (T166). The catalogue here is the CPSA v11.1 shape: a six-rung ladder (1, 2, 3a, 3b, 4, 5 at orders 1 to 6), each
/// item pinned to it, with the per-stage map the seeded catalogue carries for its exit level: {1: 3a, 2: 3b, 3: 4, 4: 5}
/// for the nine rung-5 EPAs (PAED-001) and {1: 2, 2: 3a, 3: 3b, 4: 4} for the six rung-4 EPAs (PAED-008).
/// </summary>
public sealed class GetEntrustmentStandingForTraineeTests
{
    private const string TraineeUserId = "trainee-1";
    private const int HostInstitution = 1;
    private const int OtherInstitution = 2;
    private const int CurriculumId = 10;
    private const int OtherCurriculumId = 11;
    private const int Cpsa = 42;
    private const int OrScale = 43;

    /// <summary>424 days after a 1 January 2025 start: training year 2.</summary>
    private static readonly DateOnly YearTwo = new(2026, 3, 1);

    private const string StageMap = """{"1": 3, "2": 4, "3": 5, "4": 6}""";

    private const string RungFourStageMap = """{"1": 2, "2": 3, "3": 4, "4": 5}""";

    // ─── The year target ─────────────────────────────────────────────────────

    [Theory]
    [InlineData("3a", EntrustmentStandingStatus.Below)]
    [InlineData("3b", EntrustmentStandingStatus.AtOrAbove)]
    [InlineData("4", EntrustmentStandingStatus.AtOrAbove)]
    [InlineData(null, EntrustmentStandingStatus.NoDecision)]
    public async Task AYearTwoTrainee_IsJudgedAgainstTheYearTwoTarget(string? decidedRung, EntrustmentStandingStatus expected)
    {
        await using var db = CreateDb();
        await SeedAsync(db, [Item(1, "PAED-001", exitOrder: 6)]);
        if (decidedRung is not null)
        {
            Decide(db, epaId: 1, CpsaLevel(decidedRung));
        }

        await db.SaveChangesAsync();

        var standing = await ReadAsync(db, Self());

        standing!.TargetYear.Should().Be(2);
        standing.ProgrammeNotStarted.Should().BeFalse();
        var epa = standing.Epas.Should().ContainSingle().Subject;
        epa.YearTargetLabel.Should().Be("3b", "Annexure A's year-2 level for PAED-001, as a rung on its own ladder");
        epa.ExitLevelLabel.Should().Be("5");
        epa.ScaleName.Should().Be("CPSA Paediatric Entrustment Scale v11.1");
        epa.YearStatus.Should().Be(expected);
        epa.Decision?.LevelLabel.Should().Be(decidedRung);
    }

    [Fact]
    public async Task ATraineeWhoseProgrammeHasNotStarted_IsShownTheYearOneTargets()
    {
        await using var db = CreateDb();
        await SeedAsync(db, [Item(1, "PAED-001", exitOrder: 6)], programmeStart: new DateOnly(2026, 6, 1));
        await db.SaveChangesAsync();

        var standing = await ReadAsync(db, Self());

        standing!.TargetYear.Should().Be(1);
        standing.ProgrammeNotStarted.Should().BeTrue();
        standing.Epas.Single().YearTargetLabel.Should().Be("3a");
    }

    [Fact]
    public async Task ATraineePastTheYearsTheMapNames_IsHeldToTheExitLevel_AndTheRowSaysSo()
    {
        await using var db = CreateDb();
        await SeedAsync(db, [Item(1, "PAED-001", exitOrder: 6), Item(8, "PAED-008", exitOrder: 5)]);
        Decide(db, epaId: 1, CpsaLevel("4"));
        await db.SaveChangesAsync();

        // 1,612 days after a 1 January 2025 start: training year 5, which Annexure A (y1 to y4) does not name.
        var standing = await ReadAsync(db, Self(), new DateOnly(2029, 6, 1));

        standing!.TargetYear.Should().Be(5);
        standing.Epas.Select(epa => (epa.EpaCode, epa.YearTargetLabel, epa.YearTargetIsExitLevel))
            .Should().Equal(("PAED-001", "5", true), ("PAED-008", "4", true));
        standing.YearTargetsFromExitLevel.Should().Be(2);
        standing.Epas[0].YearStatus.Should().Be(EntrustmentStandingStatus.Below, "the exit level stands in as the target");
    }

    [Fact]
    public async Task AnItemWithNoStageMap_IsHeldToItsExitLevel_AndSaysSo_WhileAMappedItemIsNot()
    {
        await using var db = CreateDb();
        await SeedAsync(
            db,
            [
                Item(1, "PAED-001", exitOrder: 6),
                Item(2, "LOCAL-002", exitOrder: 6, owningInstitutionId: HostInstitution, stageMap: "")
            ]);
        await db.SaveChangesAsync();

        var standing = await ReadAsync(db, Self());

        standing!.TargetYear.Should().Be(2);
        standing.Epas.Select(epa => (epa.EpaCode, epa.YearTargetLabel, epa.YearTargetIsExitLevel))
            .Should().Equal(("LOCAL-002", "5", true), ("PAED-001", "3b", false));
        standing.YearTargetsFromExitLevel.Should().Be(1);
    }

    [Fact]
    public async Task OnlyTheActiveDecisionCounts_AndTheLatestIssuedWinsATie()
    {
        await using var db = CreateDb();
        await SeedAsync(db, [Item(1, "PAED-001", exitOrder: 6)]);
        var revoked = Decide(db, epaId: 1, CpsaLevel("5"), issuedOn: new DateOnly(2026, 2, 1));
        Decide(db, epaId: 1, CpsaLevel("3a"), issuedOn: new DateOnly(2025, 6, 1));
        Decide(db, epaId: 1, CpsaLevel("3b"), issuedOn: new DateOnly(2025, 12, 1));
        await db.SaveChangesAsync();
        revoked.Revoke("Issued in error.", "admin-1", DateTime.UtcNow);
        await db.SaveChangesAsync();

        var epa = (await ReadAsync(db, Self()))!.Epas.Single();

        epa.Decision!.LevelLabel.Should().Be("3b", "the revoked 5 is not in force, and 3b was issued after 3a");
        epa.YearStatus.Should().Be(EntrustmentStandingStatus.AtOrAbove);
    }

    // ─── The exit rule ───────────────────────────────────────────────────────

    [Fact]
    public async Task ATraineeAtEveryExitLevel_MeetsTheExitRule()
    {
        await using var db = CreateDb();
        await SeedExitCatalogueAsync(db);
        Decide(db, epaId: 1, CpsaLevel("5"));
        Decide(db, epaId: 2, CpsaLevel("5"));
        Decide(db, epaId: 3, CpsaLevel("4"));
        await db.SaveChangesAsync();

        var exit = (await ReadAsync(db, Self()))!.Exit;

        exit.Met.Should().BeTrue();
        exit.EpaCount.Should().Be(3);
        exit.AtExitLevel.Should().Be(3);
        exit.NotYetEpaCodes.Should().BeEmpty();
        exit.Groups.Select(group => (group.LevelLabel, group.AtLevel, group.EpaCount))
            .Should().Equal(("5", 2, 2), ("4", 1, 1));
    }

    [Fact]
    public async Task OneEpaShortOfItsExitLevel_IsNamed()
    {
        await using var db = CreateDb();
        await SeedExitCatalogueAsync(db);
        Decide(db, epaId: 1, CpsaLevel("5"));
        Decide(db, epaId: 2, CpsaLevel("5"));
        Decide(db, epaId: 3, CpsaLevel("3b"));
        await db.SaveChangesAsync();

        var standing = await ReadAsync(db, Self());

        standing!.Exit.Met.Should().BeFalse();
        standing.Exit.AtExitLevel.Should().Be(2);
        standing.Exit.NotYetEpaCodes.Should().Equal("PAED-003");
        standing.Exit.Groups.Select(group => (group.LevelLabel, group.AtLevel, group.EpaCount))
            .Should().Equal(("5", 2, 2), ("4", 0, 1));
        standing.Epas.Single(epa => epa.EpaCode == "PAED-003").ExitStatus.Should().Be(EntrustmentStandingStatus.Below);
    }

    [Fact]
    public async Task AnEpaWithNoDecision_IsShortOfTheExitRule()
    {
        await using var db = CreateDb();
        await SeedExitCatalogueAsync(db);
        Decide(db, epaId: 1, CpsaLevel("5"));
        Decide(db, epaId: 3, CpsaLevel("4"));
        await db.SaveChangesAsync();

        var standing = await ReadAsync(db, Self());

        standing!.Exit.NotYetEpaCodes.Should().Equal("PAED-002");
        standing.WithoutDecision.Should().Be(1);
    }

    [Fact]
    public async Task AnInstitutionsOwnEpa_IsARowOfTheTable_ButNotCountedInTheCollegesExitRule()
    {
        await using var db = CreateDb();
        await SeedAsync(
            db,
            [
                Item(1, "PAED-001", exitOrder: 6),
                Item(3, "PAED-003", exitOrder: 5),
                Item(20, "LOCAL-020", exitOrder: 6, owningInstitutionId: HostInstitution)
            ]);
        Decide(db, epaId: 1, CpsaLevel("5"));
        Decide(db, epaId: 3, CpsaLevel("4"));
        await db.SaveChangesAsync();

        var standing = await ReadAsync(db, Self());

        var local = standing!.Epas.Single(epa => epa.EpaCode == "LOCAL-020");
        local.IsLocal.Should().BeTrue();
        local.ExitStatus.Should().Be(EntrustmentStandingStatus.NoDecision, "the row still reads against its own exit level");
        standing.Epas.Where(epa => epa.EpaCode != "LOCAL-020").Should().OnlyContain(epa => !epa.IsLocal);
        standing.LocalEpas.Should().Be(1);

        standing.Exit.EpaCount.Should().Be(2, "the rule is the College's: its EPAs only");
        standing.Exit.AtExitLevel.Should().Be(2);
        standing.Exit.Met.Should().BeTrue();
        standing.Exit.NotYetEpaCodes.Should().BeEmpty();
        standing.Exit.Groups.Select(group => (group.LevelLabel, group.AtLevel, group.EpaCount))
            .Should().Equal(("5", 1, 1), ("4", 1, 1));
    }

    // ─── Only the item's ladder ──────────────────────────────────────────────

    [Fact]
    public async Task ADecisionOnAnotherLadder_IsNotComparable_NeverCoerced()
    {
        await using var db = CreateDb();
        await SeedAsync(db, [Item(1, "PAED-001", exitOrder: 6)]);
        // Order 5 on the O-R Scale is "Supervises others": read as an ordinal on the CPSA ladder it would be rung 4,
        // above the year-2 target. It is neither above nor below; it is on another ladder.
        Decide(db, epaId: 1, levelId: 4305);
        await db.SaveChangesAsync();

        var standing = await ReadAsync(db, Self());

        var epa = standing!.Epas.Single();
        epa.YearStatus.Should().Be(EntrustmentStandingStatus.NotComparable);
        epa.ExitStatus.Should().Be(EntrustmentStandingStatus.NotComparable);
        epa.Decision!.LevelLabel.Should().Be("Supervises others");
        epa.Decision.OtherLadderName.Should().Be("O-R Scale");
        standing.Exit.AtExitLevel.Should().Be(0);
        standing.NotComparable.Should().Be(1);
    }

    [Fact]
    public async Task AnUnpinnedItem_IsNotComparable_AndPrintsItsBareOrdinals()
    {
        await using var db = CreateDb();
        await SeedAsync(db, [Item(1, "PAED-001", exitOrder: 6, scaleId: null)]);
        Decide(db, epaId: 1, CpsaLevel("5"));
        await db.SaveChangesAsync();

        var epa = (await ReadAsync(db, Self()))!.Epas.Single();

        epa.ScaleName.Should().BeNull();
        epa.YearTargetLabel.Should().Be("4", "an unpinned item's minima are ordinals on no known ladder");
        epa.YearStatus.Should().Be(EntrustmentStandingStatus.NotComparable);
        epa.ExitStatus.Should().Be(EntrustmentStandingStatus.NotComparable);
        epa.Decision!.OtherLadderName.Should().BeNull("an unpinned item names no ladder to differ from");
    }

    // ─── Which items ─────────────────────────────────────────────────────────

    [Fact]
    public async Task TheItemsAreTheTraineesCurriculumInForce_WithOnlyTheirOwnInstitutionsLocalItems()
    {
        await using var db = CreateDb();
        await SeedAsync(
            db,
            [
                Item(1, "PAED-001", exitOrder: 6),
                Item(2, "PAED-002", exitOrder: 6, owningInstitutionId: HostInstitution),
                Item(3, "PAED-003", exitOrder: 5, owningInstitutionId: OtherInstitution),
                Item(4, "PAED-004", exitOrder: 5)
            ]);
        (await db.Epas.SingleAsync(epa => epa.Id == 4)).Deactivate(DateTime.MinValue);
        await db.SaveChangesAsync();

        var standing = await ReadAsync(db, Self());

        standing!.Epas.Select(epa => epa.EpaCode).Should().Equal(
            ["PAED-001", "PAED-002"],
            "another institution's local item is a target this trainee could never meet, and a deactivated EPA is no target (T158)");
        standing.Exit.EpaCount.Should().Be(1, "PAED-002 is the host institution's own, outside the College's rule");
    }

    [Fact]
    public async Task AnotherCurriculumsItems_AreNotThisTrainees()
    {
        await using var db = CreateDb();
        await SeedAsync(
            db,
            [
                Item(1, "PAED-001", exitOrder: 6),
                Item(5, "PAED-005", exitOrder: 6, curriculumId: OtherCurriculumId)
            ]);
        await db.SaveChangesAsync();

        var standing = await ReadAsync(db, Self());

        standing!.Epas.Select(epa => epa.EpaCode).Should().Equal("PAED-001");
    }

    [Fact]
    public async Task LocalItems_AreTheTraineesInstitutions_NotTheCallers()
    {
        await using var db = CreateDb();
        await SeedAsync(
            db,
            [
                Item(1, "PAED-001", exitOrder: 6),
                Item(2, "HOST-002", exitOrder: 6, owningInstitutionId: HostInstitution),
                Item(3, "ELSE-003", exitOrder: 6, owningInstitutionId: OtherInstitution)
            ]);
        await db.SaveChangesAsync();

        // An Administrator whose own sign-in is at the other institution: the trainee's institution decides.
        var administrator = TestPrincipals.InRole(WombatRoles.Administrator, "admin-elsewhere", OtherInstitution);
        var standing = await ReadAsync(db, administrator);

        standing!.Epas.Select(epa => epa.EpaCode).Should().Equal("HOST-002", "PAED-001");
    }

    [Fact]
    public async Task ThePreferredProfile_DecidesTheCurriculumAndTheInstitution_NotAnOlderOne()
    {
        await using var db = CreateDb();
        await SeedAsync(
            db,
            [
                Item(1, "PAED-001", exitOrder: 6),
                Item(2, "HOST-002", exitOrder: 6, owningInstitutionId: HostInstitution),
                Item(5, "PAED-005", exitOrder: 6, curriculumId: OtherCurriculumId),
                Item(6, "ELSE-006", exitOrder: 6, owningInstitutionId: OtherInstitution, curriculumId: OtherCurriculumId),
                Item(7, "ELSE-007", exitOrder: 6, owningInstitutionId: OtherInstitution)
            ],
            profileId: 7);
        // An earlier programme on another curriculum at another institution, left before this one: a lower id.
        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 3, UserId = TraineeUserId, InstitutionId = OtherInstitution, CurriculumId = OtherCurriculumId,
            ProgrammeStartDate = new DateOnly(2021, 1, 1), ExpectedCompletionDate = new DateOnly(2025, 1, 1),
            IsActive = false
        });
        await db.SaveChangesAsync();

        var standing = await ReadAsync(db, Self());

        standing!.Epas.Select(epa => epa.EpaCode).Should().Equal("HOST-002", "PAED-001");
        standing.ProgrammeStartDate.Should().Be(new DateOnly(2025, 1, 1));
    }

    // ─── The latest rating ───────────────────────────────────────────────────

    [Fact]
    public async Task TheLatestRating_IsTheLatestEncounter_LabelledAndJudgedOnTheItemsLadder()
    {
        await using var db = CreateDb();
        await SeedAsync(db, [Item(1, "PAED-001", exitOrder: 6)]);
        var type = await SeedRatedTypeAsync(db, "CPSA Paediatric Entrustment Scale v11.1");
        var later = Rate(db, type, epaId: 1, rating: 4, observedOn: new DateOnly(2026, 2, 20));
        Rate(db, type, epaId: 1, rating: 3, observedOn: new DateOnly(2026, 1, 10));
        await db.SaveChangesAsync();

        var rating = (await ReadAsync(db, Self()))!.Epas.Single().LatestRating;

        rating.Should().NotBeNull();
        rating!.ActivityId.Should().Be(later.Id);
        rating.RatingLabel.Should().Be("3b");
        rating.ObservedOn.Should().Be(new DateOnly(2026, 2, 20));
        rating.OtherLadderName.Should().BeNull();
        rating.AgainstYearTarget.Should().Be(EntrustmentStandingStatus.AtOrAbove);
    }

    [Fact]
    public async Task ARatingOnAnotherLadder_IsLabelledOnItsOwn_AndNotCompared()
    {
        await using var db = CreateDb();
        await SeedAsync(db, [Item(1, "PAED-001", exitOrder: 6)]);
        var type = await SeedRatedTypeAsync(db, "O-R Scale");
        Rate(db, type, epaId: 1, rating: 4, observedOn: new DateOnly(2026, 2, 20));
        await db.SaveChangesAsync();

        var rating = (await ReadAsync(db, Self()))!.Epas.Single().LatestRating!;

        rating.RatingLabel.Should().Be("Independent");
        rating.OtherLadderName.Should().Be("O-R Scale");
        rating.AgainstYearTarget.Should().Be(EntrustmentStandingStatus.NotComparable);
    }

    [Fact]
    public async Task AnOverseer_SeesTheStanding_ButNotARatingStampedToAnotherInstitution()
    {
        await using var db = CreateDb();
        await SeedAsync(db, [Item(1, "PAED-001", exitOrder: 6)]);
        Decide(db, epaId: 1, CpsaLevel("3b"));
        var type = await SeedRatedTypeAsync(db, "CPSA Paediatric Entrustment Scale v11.1");
        // Recorded before the trainee transferred: stamped to the institution they trained at then (T101).
        Rate(db, type, epaId: 1, rating: 4, observedOn: new DateOnly(2026, 2, 20), institutionId: OtherInstitution);
        await db.SaveChangesAsync();

        var coordinator = await ReadAsync(db, TestPrincipals.Coordinator(HostInstitution));
        var trainee = await ReadAsync(db, Self());

        coordinator!.Epas.Single().Decision!.LevelLabel.Should().Be("3b");
        coordinator.Epas.Single().LatestRating.Should().BeNull("the rating is outside what the coordinator may read");
        trainee!.Epas.Single().LatestRating.Should().NotBeNull("the trainee reads their own evidence wherever it is stamped");
    }

    // ─── Who may ask ─────────────────────────────────────────────────────────

    [Fact]
    public async Task SomeoneWhoMayNotReadAboutTheTrainee_GetsNull()
    {
        await using var db = CreateDb();
        await SeedAsync(db, [Item(1, "PAED-001", exitOrder: 6)]);
        Decide(db, epaId: 1, CpsaLevel("3b"));
        await db.SaveChangesAsync();

        (await ReadAsync(db, TestPrincipals.Coordinator(OtherInstitution))).Should().BeNull();
        (await ReadAsync(db, TestPrincipals.Trainee("trainee-2", HostInstitution))).Should().BeNull();
    }

    [Fact]
    public async Task ATraineeWithNoProfile_GetsNull()
    {
        await using var db = CreateDb();
        await SeedAsync(db, [Item(1, "PAED-001", exitOrder: 6)]);
        await db.SaveChangesAsync();

        var standing = await new GetEntrustmentStandingForTraineeQueryHandler(db).Handle(
            new GetEntrustmentStandingForTraineeQuery("nobody", TestPrincipals.Administrator(), YearTwo),
            CancellationToken.None);

        standing.Should().BeNull();
    }

    // ─── Fixtures ────────────────────────────────────────────────────────────

    private static Task<EntrustmentStandingDto?> ReadAsync(
        ApplicationDbContext db,
        ClaimsPrincipal principal,
        DateOnly? asOf = null)
        => new GetEntrustmentStandingForTraineeQueryHandler(db).Handle(
            new GetEntrustmentStandingForTraineeQuery(TraineeUserId, principal, asOf ?? YearTwo),
            CancellationToken.None);

    private static ClaimsPrincipal Self() => TestPrincipals.Trainee(TraineeUserId, HostInstitution);

    private static int CpsaLevel(string rung) => 4200 + Array.IndexOf(CpsaRungs, rung) + 1;

    private static readonly string[] CpsaRungs = ["1", "2", "3a", "3b", "4", "5"];

    private static CurriculumItem Item(
        int epaId,
        string code,
        int exitOrder,
        int? scaleId = Cpsa,
        int? owningInstitutionId = null,
        int curriculumId = CurriculumId,
        string? stageMap = null)
        => new()
        {
            Id = 100 + epaId,
            CurriculumId = curriculumId,
            EpaId = epaId,
            Epa = new Epa { Id = epaId, SubSpecialityId = 1, Code = code, Title = $"{code} title", IsActive = true },
            RequiredCount = 3,
            QuotaPeriod = QuotaPeriod.Semester,
            MinimumLevelOrder = exitOrder,
            MinimumLevelByStageJson = stageMap ?? (exitOrder == 6 ? StageMap : RungFourStageMap),
            WindowMonths = 12,
            ScaleId = scaleId,
            OwningInstitutionId = owningInstitutionId
        };

    /// <summary>
    /// Three EPAs: two whose exit level is rung 5 and one whose exit level is rung 4, the two groups the College's
    /// "Level 5 in 9 EPAs and Level 4 in the remaining 6" divides the catalogue into.
    /// </summary>
    private static Task SeedExitCatalogueAsync(ApplicationDbContext db)
        => SeedAsync(
            db,
            [
                Item(1, "PAED-001", exitOrder: 6),
                Item(2, "PAED-002", exitOrder: 6),
                Item(3, "PAED-003", exitOrder: 5)
            ]);

    private static async Task SeedAsync(
        ApplicationDbContext db,
        CurriculumItem[] items,
        DateOnly? programmeStart = null,
        int profileId = 1)
    {
        db.Institutions.Add(new Institution { Id = HostInstitution, Name = "Host" });
        db.Institutions.Add(new Institution { Id = OtherInstitution, Name = "Elsewhere" });
        db.Specialities.Add(new Speciality { Id = 1, CollegeId = 1, Name = "Paediatrics" });
        db.SubSpecialities.Add(new SubSpeciality { Id = 1, SpecialityId = 1, Name = "General Paediatrics" });

        db.Set<EntrustmentScale>().Add(new EntrustmentScale { Id = Cpsa, Name = "CPSA Paediatric Entrustment Scale v11.1" });
        for (var order = 1; order <= CpsaRungs.Length; order++)
        {
            db.Set<EntrustmentLevel>().Add(new EntrustmentLevel
            {
                Id = 4200 + order, ScaleId = Cpsa, Order = order, Label = CpsaRungs[order - 1]
            });
        }

        db.Set<EntrustmentScale>().Add(new EntrustmentScale { Id = OrScale, Name = "O-R Scale" });
        string[] orRungs = ["Observe only", "Direct supervision", "Indirect supervision", "Independent", "Supervises others"];
        for (var order = 1; order <= orRungs.Length; order++)
        {
            db.Set<EntrustmentLevel>().Add(new EntrustmentLevel
            {
                Id = 4300 + order, ScaleId = OrScale, Order = order, Label = orRungs[order - 1]
            });
        }

        db.Curricula.Add(new Curriculum
        {
            Id = CurriculumId, SubSpecialityId = 1, Name = "Paediatric EPA Curriculum", Version = "11.1",
            EffectiveFrom = new DateOnly(2025, 1, 1), IsActive = true
        });
        db.Curricula.Add(new Curriculum
        {
            Id = OtherCurriculumId, SubSpecialityId = 1, Name = "Paediatric EPA Curriculum", Version = "10",
            EffectiveFrom = new DateOnly(2020, 1, 1), IsActive = false
        });
        db.CurriculumItems.AddRange(items);

        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = profileId, UserId = TraineeUserId, InstitutionId = HostInstitution, CurriculumId = CurriculumId,
            ProgrammeStartDate = programmeStart ?? new DateOnly(2025, 1, 1),
            ExpectedCompletionDate = new DateOnly(2029, 1, 1),
            IsActive = true
        });

        await db.SaveChangesAsync();
    }

    private static EntrustmentDecision Decide(
        ApplicationDbContext db,
        int epaId,
        int levelId,
        DateOnly? issuedOn = null)
    {
        var decision = EntrustmentDecision.Issue(
            TraineeUserId, epaId, levelId, issuedOn ?? new DateOnly(2026, 1, 15), expiresOn: null,
            committeeReviewId: 30, "chair-1", "Consistent across the period.", StarEvidence.One());
        db.EntrustmentDecisions.Add(decision);
        return decision;
    }

    /// <summary>A rated type whose rating an assessor named on the form writes, on the ladder named here.</summary>
    private static async Task<ActivityType> SeedRatedTypeAsync(ApplicationDbContext db, string scaleKey)
    {
        var schemaJson = """
            {
              "version": 1,
              "rated_level_field": "overall",
              "evidence_epa_field": "epa_id",
              "sections": [
                {
                  "key": "request",
                  "title": "Request",
                  "fields": [
                    { "key": "epa_id", "type": "epa", "label": "EPA" },
                    { "key": "assessor_user_id", "type": "user", "label": "Assessor" }
                  ]
                },
                {
                  "key": "assessment",
                  "title": "Assessment",
                  "editable_by": "field:assessor_user_id",
                  "fields": [
                    { "key": "overall", "type": "scale", "label": "Overall", "scale_key": "SCALE" }
                  ]
                }
              ]
            }
            """.Replace("SCALE", scaleKey, StringComparison.Ordinal);

        var type = new ActivityType
        {
            Key = "rated_" + scaleKey.Length,
            Name = "Rated",
            Version = 1,
            IsActive = true,
            OwnerUserId = "seed-system",
            CreatedOn = DateTime.UtcNow,
            SchemaJson = schemaJson,
            WorkflowJson = "{}",
            CreditRulesJson = "{}"
        };
        type.Versions.Add(new ActivityTypeVersion
        {
            Version = 1,
            SchemaJson = schemaJson,
            WorkflowJson = "{}",
            CreditRulesJson = "{}",
            PublishedByUserId = "seed-system",
            PublishedOn = DateTime.UtcNow
        });
        db.ActivityTypes.Add(type);
        await db.SaveChangesAsync();
        return type;
    }

    private static Activity Rate(
        ApplicationDbContext db,
        ActivityType type,
        int epaId,
        int rating,
        DateOnly observedOn,
        int institutionId = HostInstitution)
    {
        var dataJson = $$"""{"epa_id": {{epaId}}, "assessor_user_id": "assessor-a", "overall": "{{rating}}"}""";
        var on = observedOn.ToDateTime(new TimeOnly(9, 0), DateTimeKind.Utc);
        var activity = new Activity
        {
            ActivityTypeId = type.Id,
            SchemaVersion = type.Version,
            SubjectUserId = TraineeUserId,
            CreatedByUserId = "assessor-a",
            CurrentState = "completed",
            DataJson = dataJson,
            EpaId = EvidenceEpaStamp.For(db, type.Id, type.Version, dataJson),
            CreatedOn = on,
            UpdatedOn = on,
            ObservedOn = observedOn,
            ObservedOnSource = ObservationDateSource.Declared,
            InstitutionId = institutionId,
            SpecialityId = 1,
            SubSpecialityId = 1
        };
        db.Activities.Add(activity);
        return activity;
    }

    private static ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
