using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Activities.Queries.GetEpaTrajectoryForTrainee;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Activities;

public sealed class GetEpaTrajectoryForTraineeTests
{
    [Fact]
    public async Task ReturnsPointsOrderedByDate()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");

        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, 3, new DateTime(2026, 2, 10, 9, 0, 0, DateTimeKind.Utc));
        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-b", 7, 4, new DateTime(2026, 1, 5, 9, 0, 0, DateTimeKind.Utc));
        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, 5, new DateTime(2026, 3, 1, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetEpaTrajectoryForTraineeQueryHandler(dbContext);
        var result = await handler.Handle(new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-1")), CancellationToken.None);

        var trajectory = result.Should().ContainSingle().Subject;
        trajectory.EpaId.Should().Be(7);
        trajectory.Points.Select(p => p.Rating).Should().Equal(4, 3, 5);
        trajectory.Points.Select(p => p.ObservedOn).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GroupsByEpaAndSortsByEpaCode()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        dbContext.Epas.Add(new Epa { Id = 3, SubSpecialityId = 1, Code = "EPA-03", Title = "Ward round", IsActive = true });
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");

        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, 3, new DateTime(2026, 2, 1, 9, 0, 0, DateTimeKind.Utc));
        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-b", 3, 4, new DateTime(2026, 2, 5, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetEpaTrajectoryForTraineeQueryHandler(dbContext);
        var result = await handler.Handle(new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-1")), CancellationToken.None);

        result.Select(dto => dto.EpaCode).Should().Equal("EPA-03", "EPA-07");
    }

    [Fact]
    public async Task IgnoresUnratedActivityTypes()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var reflectiveNote = await SeedActivityTypeAsync(dbContext, "reflective_note");

        AddRatedActivity(dbContext, reflectiveNote, "trainee-1", "assessor-a", 7, 3, new DateTime(2026, 2, 10, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetEpaTrajectoryForTraineeQueryHandler(dbContext);
        var result = await handler.Handle(new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-1")), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task IgnoresActivitiesWithoutOverallRating()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");

        // No overall field -> skipped
        dbContext.Activities.Add(new Activity
        {
            ActivityTypeId = miniCex.Id,
            ActivityType = miniCex,
            SchemaVersion = miniCex.Version,
            SubjectUserId = "trainee-1",
            CreatedByUserId = "assessor-a",
            CurrentState = "requested",
            DataJson = "{\"epa_id\": 7, \"assessor_user_id\": \"assessor-a\"}",
            CreatedOn = new DateTime(2026, 2, 1, 9, 0, 0, DateTimeKind.Utc),
            // T119: production stamps this in ActivityService; a fixture that builds the
            // entity directly must set it, or it defaults to 0001-01-01.
            ObservedOn = DateOnly.FromDateTime(new DateTime(2026, 2, 1, 9, 0, 0, DateTimeKind.Utc)),
            UpdatedOn = new DateTime(2026, 2, 1, 9, 0, 0, DateTimeKind.Utc)
        });
        await dbContext.SaveChangesAsync();

        var handler = new GetEpaTrajectoryForTraineeQueryHandler(dbContext);
        var result = await handler.Handle(new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-1")), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AppliesDateRangeFilter()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");

        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, 2, new DateTime(2025, 12, 1, 9, 0, 0, DateTimeKind.Utc));
        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-b", 7, 3, new DateTime(2026, 2, 5, 9, 0, 0, DateTimeKind.Utc));
        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-c", 7, 4, new DateTime(2026, 5, 5, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetEpaTrajectoryForTraineeQueryHandler(dbContext);
        var result = await handler.Handle(
            new GetEpaTrajectoryForTraineeQuery(
                "trainee-1",
                Principal("trainee-1"),
                From: new DateOnly(2026, 1, 1),
                To: new DateOnly(2026, 3, 31)),
            CancellationToken.None);

        var trajectory = result.Should().ContainSingle().Subject;
        trajectory.Points.Should().ContainSingle();
        trajectory.Points[0].Rating.Should().Be(3);
    }

    [Fact]
    public async Task ScopesByTrainee()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");

        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, 3, new DateTime(2026, 2, 1, 9, 0, 0, DateTimeKind.Utc));
        AddRatedActivity(dbContext, miniCex, "trainee-2", "assessor-a", 7, 5, new DateTime(2026, 2, 5, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetEpaTrajectoryForTraineeQueryHandler(dbContext);
        var result = await handler.Handle(new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-1")), CancellationToken.None);

        var trajectory = result.Should().ContainSingle().Subject;
        trajectory.Points.Should().ContainSingle();
        trajectory.Points[0].Rating.Should().Be(3);
    }

    [Fact]
    public async Task MapsSourceByActivityKey()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var dops = await SeedActivityTypeAsync(dbContext, "dops");
        var cbd = await SeedActivityTypeAsync(dbContext, "cbd");

        AddRatedActivity(dbContext, dops, "trainee-1", "assessor-a", 7, 3, new DateTime(2026, 2, 1, 9, 0, 0, DateTimeKind.Utc));
        AddRatedActivity(dbContext, cbd, "trainee-1", "assessor-b", 7, 4, new DateTime(2026, 2, 10, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetEpaTrajectoryForTraineeQueryHandler(dbContext);
        var result = await handler.Handle(new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-1")), CancellationToken.None);

        var trajectory = result.Should().ContainSingle().Subject;
        trajectory.Points.Select(p => p.Source).Should().Equal("Direct observation", "Conversation");
    }

    [Fact]
    public async Task MapsSchemaDrivenKeyVariant()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        // Schema-driven types built via the visual builder carry institution-specific
        // key suffixes (e.g. "mini_cex_paed"); they must still map to the assessment family.
        var miniCexPaed = await SeedActivityTypeAsync(dbContext, "mini_cex_paed");

        AddRatedActivity(dbContext, miniCexPaed, "trainee-1", "assessor-a", 7, 3, new DateTime(2026, 2, 1, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetEpaTrajectoryForTraineeQueryHandler(dbContext);
        var result = await handler.Handle(new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-1")), CancellationToken.None);

        var trajectory = result.Should().ContainSingle().Subject;
        trajectory.Points.Should().ContainSingle();
        trajectory.Points[0].Source.Should().Be("Direct observation");
    }

    [Fact]
    public async Task ReadsOverallLevelFieldWhenOverallAbsent()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var miniCexPaed = await SeedActivityTypeAsync(dbContext, "mini_cex_paed");

        // Visual-builder schema stores the overall rating as "overall_level", not "overall".
        dbContext.Activities.Add(new Activity
        {
            ActivityTypeId = miniCexPaed.Id,
            ActivityType = miniCexPaed,
            SchemaVersion = miniCexPaed.Version,
            SubjectUserId = "trainee-1",
            CreatedByUserId = "assessor-a",
            CurrentState = "completed",
            DataJson = "{\"epa_id\": \"7\", \"assessor_user_id\": \"assessor-a\", \"overall_level\": \"4\"}",
            CreatedOn = new DateTime(2026, 2, 1, 9, 0, 0, DateTimeKind.Utc),
            // T119: production stamps this in ActivityService; a fixture that builds the
            // entity directly must set it, or it defaults to 0001-01-01.
            ObservedOn = DateOnly.FromDateTime(new DateTime(2026, 2, 1, 9, 0, 0, DateTimeKind.Utc)),
            UpdatedOn = new DateTime(2026, 2, 1, 9, 0, 0, DateTimeKind.Utc)
        });
        await dbContext.SaveChangesAsync();

        var handler = new GetEpaTrajectoryForTraineeQueryHandler(dbContext);
        var result = await handler.Handle(new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-1")), CancellationToken.None);

        var trajectory = result.Should().ContainSingle().Subject;
        trajectory.Points.Should().ContainSingle();
        trajectory.Points[0].Rating.Should().Be(4);
    }

    [Fact]
    public async Task IncludesRatingsAboveFive()
    {
        // Regression (T098): the rating guard was hard-coded to 1..5, so an observation recorded
        // on a scale with more rungs was dropped silently — no error, no log, no gap in the chart.
        // The paediatric v11.1 ladder has six rungs (1, 2, 3a, 3b, 4, 5), so rung 6 must survive.
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");

        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, 6, new DateTime(2026, 2, 10, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetEpaTrajectoryForTraineeQueryHandler(dbContext);
        var result = await handler.Handle(new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-1")), CancellationToken.None);

        var trajectory = result.Should().ContainSingle().Subject;
        trajectory.Points.Should().ContainSingle();
        trajectory.Points[0].Rating.Should().Be(6);
    }

    [Fact]
    public async Task IgnoresImplausibleRatings()
    {
        // The 1..5 clamp was removed, but a value that cannot be a rung at all (a mis-mapped
        // percentage, a year) must still be rejected rather than drawn.
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");

        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, 2026, new DateTime(2026, 2, 10, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetEpaTrajectoryForTraineeQueryHandler(dbContext);
        var result = await handler.Handle(new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-1")), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task MapsVersionElevenAssessmentFamilies()
    {
        // T098: v11.1 names CCA, RCA, chart-stimulated recall and case note review as WBA tools.
        // They must chart as assessment evidence once those activity types exist.
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var cca = await SeedActivityTypeAsync(dbContext, "cca");
        var rca = await SeedActivityTypeAsync(dbContext, "rca");
        var csr = await SeedActivityTypeAsync(dbContext, "chart_stimulated_recall");

        AddRatedActivity(dbContext, cca, "trainee-1", "assessor-a", 7, 3, new DateTime(2026, 2, 1, 9, 0, 0, DateTimeKind.Utc));
        AddRatedActivity(dbContext, rca, "trainee-1", "assessor-b", 7, 4, new DateTime(2026, 2, 5, 9, 0, 0, DateTimeKind.Utc));
        AddRatedActivity(dbContext, csr, "trainee-1", "assessor-c", 7, 5, new DateTime(2026, 2, 9, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetEpaTrajectoryForTraineeQueryHandler(dbContext);
        var result = await handler.Handle(new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-1")), CancellationToken.None);

        var trajectory = result.Should().ContainSingle().Subject;
        trajectory.Points.Select(p => p.Source).Should().Equal("Case analysis", "Case analysis", "Conversation");
    }

    [Fact]
    public async Task WithholdsTrajectoryFromACallerWhoDoesNotOverseeTheTrainee()
    {
        // The trainee id is caller-supplied, so this is the door T101 closed: a signed-in user who
        // is neither the subject nor an overseer used to get ratings, assessor ids and activity ids
        // for anyone they cared to name. Empty, not an error — the chart has nothing to draw.
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");

        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, 3, new DateTime(2026, 2, 1, 9, 0, 0, DateTimeKind.Utc), specialityId: 4);
        await dbContext.SaveChangesAsync();

        var handler = new GetEpaTrajectoryForTraineeQueryHandler(dbContext);
        var result = await handler.Handle(
            new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("stranger-1")), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ShowsTrajectoryToTheSpecialityAdminTheActivityIsStampedTo()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");

        // Stamped to speciality 4 AND institution 9. T101 conjoins them, because a Speciality is
        // College-owned and its id is therefore national.
        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, 3, new DateTime(2026, 2, 1, 9, 0, 0, DateTimeKind.Utc), specialityId: 4, institutionId: 9);
        await dbContext.SaveChangesAsync();

        var handler = new GetEpaTrajectoryForTraineeQueryHandler(dbContext);
        var result = await handler.Handle(
            new GetEpaTrajectoryForTraineeQuery(
                "trainee-1",
                Principal("speciality-admin-1", role: WombatRoles.SpecialityAdmin, specialityId: 4, institutionId: 9)),
            CancellationToken.None);

        result.Should().ContainSingle();

        var elsewhere = await handler.Handle(
            new GetEpaTrajectoryForTraineeQuery(
                "trainee-1",
                Principal("speciality-admin-2", role: WombatRoles.SpecialityAdmin, specialityId: 4, institutionId: 10)),
            CancellationToken.None);

        elsewhere.Should().BeEmpty("the speciality id is national; the institution must match too");
    }

    private static ClaimsPrincipal Principal(string userId, string? role = null, int? specialityId = null, int? institutionId = null)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
        if (role is not null)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        if (specialityId.HasValue)
        {
            claims.Add(new Claim(WombatClaimTypes.SpecialityId, specialityId.Value.ToString()));
        }

        if (institutionId.HasValue)
        {
            claims.Add(new Claim(WombatClaimTypes.InstitutionId, institutionId.Value.ToString()));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }


    // ---- T123 defect 1: the ladder is resolved server-side from the trainee's pinned curriculum item ----

    [Fact]
    public async Task PinnedCurriculumItem_CarriesTheLadderAndLabelsEveryRating()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        await SeedCpsaCurriculumAsync(dbContext, "trainee-1", pin: true);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");

        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, 3, new DateTime(2026, 3, 10, 9, 0, 0, DateTimeKind.Utc));
        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, 5, new DateTime(2026, 4, 10, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetEpaTrajectoryForTraineeQueryHandler(dbContext);
        var result = await handler.Handle(
            new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-1")), CancellationToken.None);

        var trajectory = result.Should().ContainSingle().Subject;
        trajectory.ScaleId.Should().Be(42);
        trajectory.ScaleName.Should().Be("CPSA Paediatric Entrustment Scale v11.1");
        trajectory.Rungs.Select(rung => rung.Label).Should().Equal("1", "2", "3a", "3b", "4", "5");

        // The whole point: ordinal 5 is the College's rung "4".
        trajectory.Points.Select(point => point.Rating).Should().Equal(3, 5);
        trajectory.Points.Select(point => point.RatingLabel).Should().Equal("3a", "4");
    }

    [Fact]
    public async Task UnpinnedCurriculumItem_CarriesNoLadderAndFallsBackToTheOrdinal()
    {
        // Curriculum 2 is 0/15 pinned on dev. Unpinned is a permanent state, not a migration artefact,
        // and every legacy trajectory must render exactly as it did before T123.
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        await SeedCpsaCurriculumAsync(dbContext, "trainee-1", pin: false);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");

        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, 3, new DateTime(2026, 3, 10, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetEpaTrajectoryForTraineeQueryHandler(dbContext);
        var result = await handler.Handle(
            new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-1")), CancellationToken.None);

        var trajectory = result.Should().ContainSingle().Subject;
        trajectory.ScaleId.Should().BeNull();
        trajectory.ScaleName.Should().BeNull();
        trajectory.Rungs.Should().BeEmpty();
        trajectory.Points.Single().RatingLabel.Should().Be("3");
    }

    [Fact]
    public async Task NoTraineeProfile_CarriesNoLadder()
    {
        // A PendingTrainee has no profile. Nothing here may throw or hide the observations.
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");

        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, 3, new DateTime(2026, 3, 10, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetEpaTrajectoryForTraineeQueryHandler(dbContext);
        var result = await handler.Handle(
            new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-1")), CancellationToken.None);

        var trajectory = result.Should().ContainSingle().Subject;
        trajectory.Rungs.Should().BeEmpty();
        trajectory.Points.Should().ContainSingle();
    }

    [Fact]
    public async Task ARatingThatIsNotARungOnThePinnedLadder_KeepsTheOrdinalAsItsLabel()
    {
        // How a rating recorded on another scale reaches the chart. It is not dropped; the component
        // draws it hollow and leaves it out of the line.
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        await SeedCpsaCurriculumAsync(dbContext, "trainee-1", pin: true);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");

        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, 9, new DateTime(2026, 3, 10, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetEpaTrajectoryForTraineeQueryHandler(dbContext);
        var result = await handler.Handle(
            new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-1")), CancellationToken.None);

        result.Single().Points.Single().RatingLabel.Should().Be("9");
    }

    [Fact]
    public async Task AGraduatedTraineesProfileStillResolvesTheLadder()
    {
        // The profile rule is active-first then latest ProgrammeStartDate, NOT filtered on IsActive --
        // the same rule the credit engine's picker uses, so the two cannot disagree about which row is
        // in force. A graduated registrar still has a trajectory worth reading.
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        await SeedCpsaCurriculumAsync(dbContext, "trainee-1", pin: true, profileIsActive: false);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");

        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, 5, new DateTime(2026, 3, 10, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetEpaTrajectoryForTraineeQueryHandler(dbContext);
        var result = await handler.Handle(
            new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-1")), CancellationToken.None);

        result.Single().Points.Single().RatingLabel.Should().Be("4");
    }

    [Fact]
    public async Task AnotherInstitutionsLocalCurriculumItemIsNotThisTraineesLadder()
    {
        // Same predicate as the picker and as CreditApplier. Without it, a trainee at institution 2
        // would read their chart against institution 99's ladder -- and their own correctly-rated
        // observations would be marked "not a rung on this scale" against a scale never theirs.
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        await SeedCpsaCurriculumAsync(dbContext, "trainee-1", pin: true, owningInstitutionId: 99);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");

        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, 3, new DateTime(2026, 3, 10, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetEpaTrajectoryForTraineeQueryHandler(dbContext);
        var result = await handler.Handle(
            new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-1")), CancellationToken.None);

        var trajectory = result.Should().ContainSingle().Subject;
        trajectory.ScaleId.Should().BeNull();
        trajectory.Rungs.Should().BeEmpty();
        trajectory.Points.Single().RatingLabel.Should().Be("3");
    }

    // ---- T126: which ladder was this rating actually recorded against? ---------------------------

    /// <summary>
    /// The case T126 exists for, and the one the ordinal test cannot see. A five-rung "4"
    /// ("Independent" on the O-R Scale) plotted against the six-rung CPSA axis is inside 1-6, so it
    /// draws as rung "3b" and looks entirely normal. Only the activity's own declared ladder
    /// distinguishes them.
    /// </summary>
    [Fact]
    public async Task ARatingFromAnotherLadderIsMarkedOffLadderEvenWhenItsOrdinalIsValidHere()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        await SeedCpsaCurriculumAsync(dbContext, "trainee-1", pin: true);
        SeedOrScale(dbContext);
        var legacy = await SeedActivityTypeAsync(dbContext, "mini_cex_paed");
        await SeedPinnedVersionAsync(dbContext, legacy, ratedField: "overall", scaleKey: "O-R Scale");

        AddRatedActivity(dbContext, legacy, "trainee-1", "assessor-a", 7, 4, new DateTime(2026, 3, 10, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetEpaTrajectoryForTraineeQueryHandler(dbContext);
        var result = await handler.Handle(
            new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-1")), CancellationToken.None);

        var point = result.Single().Points.Single();
        point.Rating.Should().Be(4, "the ordinal is a perfectly valid rung on the CPSA ladder -- that is the trap");
        point.RatingLabel.Should().Be("3b", "it still renders against the axis; the chart draws it hollow rather than hiding it");
        point.OffLadder.Should().BeTrue("it was rated on the O-R Scale, not the CPSA ladder this axis is drawn from");
    }

    [Fact]
    public async Task ARatingFromTheAxisLadderIsNotMarkedOffLadder()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        await SeedCpsaCurriculumAsync(dbContext, "trainee-1", pin: true);
        SeedOrScale(dbContext);
        var cpsa = await SeedActivityTypeAsync(dbContext, "mini_cex_cpsa");
        await SeedPinnedVersionAsync(dbContext, cpsa, ratedField: "overall", scaleKey: "42");

        AddRatedActivity(dbContext, cpsa, "trainee-1", "assessor-a", 7, 4, new DateTime(2026, 3, 10, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetEpaTrajectoryForTraineeQueryHandler(dbContext);
        var result = await handler.Handle(
            new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-1")), CancellationToken.None);

        result.Single().Points.Single().OffLadder.Should().BeFalse();
    }

    /// <summary>
    /// False means "no disagreement established", never "on the ladder". A type published before T126
    /// declares no rated field, and four generic seeds still declare the unresolvable or_scale (T110).
    /// Marking those would assert a conflict nothing has shown.
    /// </summary>
    [Theory]
    [InlineData(null, "O-R Scale")]
    [InlineData("overall", "or_scale")]
    public async Task AnUnknownLadderIsLeftAloneRatherThanMarked(string? ratedField, string scaleKey)
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        await SeedCpsaCurriculumAsync(dbContext, "trainee-1", pin: true);
        SeedOrScale(dbContext);
        var legacy = await SeedActivityTypeAsync(dbContext, "mini_cex_paed");
        await SeedPinnedVersionAsync(dbContext, legacy, ratedField, scaleKey);

        AddRatedActivity(dbContext, legacy, "trainee-1", "assessor-a", 7, 4, new DateTime(2026, 3, 10, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetEpaTrajectoryForTraineeQueryHandler(dbContext);
        var result = await handler.Handle(
            new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-1")), CancellationToken.None);

        result.Single().Points.Single().OffLadder.Should().BeFalse();
    }

    /// <summary>
    /// The version an activity is PINNED to decides its ladder, not the newest one. Republishing a type
    /// onto a different scale must not retroactively re-interpret ratings already filed against it.
    /// </summary>
    [Fact]
    public async Task TheLadderComesFromThePinnedVersionNotTheNewestOne()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        await SeedCpsaCurriculumAsync(dbContext, "trainee-1", pin: true);
        SeedOrScale(dbContext);
        var type = await SeedActivityTypeAsync(dbContext, "mini_cex_paed");

        // v1 rated on the O-R Scale; the activity below is pinned to it.
        await SeedPinnedVersionAsync(dbContext, type, ratedField: "overall", scaleKey: "O-R Scale", version: 1);
        // v2 moved to the CPSA ladder. It must not change what v1's ratings meant.
        await SeedPinnedVersionAsync(dbContext, type, ratedField: "overall", scaleKey: "42", version: 2);

        AddRatedActivity(dbContext, type, "trainee-1", "assessor-a", 7, 4, new DateTime(2026, 3, 10, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var result = await new GetEpaTrajectoryForTraineeQueryHandler(dbContext).Handle(
            new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-1")), CancellationToken.None);

        result.Single().Points.Single().OffLadder.Should().BeTrue(
            "the activity is pinned to v1, which rated on the O-R Scale");
    }

    private static void SeedOrScale(ApplicationDbContext dbContext)
    {
        dbContext.Set<EntrustmentScale>().Add(new EntrustmentScale { Id = 43, Name = "O-R Scale" });
        var labels = new[] { "Observe only", "Direct supervision", "Indirect supervision", "Independent", "Supervises others" };
        for (var order = 1; order <= labels.Length; order++)
        {
            dbContext.Set<EntrustmentLevel>().Add(new EntrustmentLevel
            {
                Id = 4300 + order, ScaleId = 43, Order = order, Label = labels[order - 1]
            });
        }
    }

    private static async Task SeedPinnedVersionAsync(
        ApplicationDbContext dbContext,
        ActivityType activityType,
        string? ratedField,
        string scaleKey,
        int version = 1)
    {
        var pointer = ratedField is null
            ? string.Empty
            : "\"rated_level_field\": \"" + ratedField + "\",";

        var schemaJson =
            "{ \"version\": 1, " + pointer +
            "  \"sections\": [ { \"key\": \"assessment\", \"title\": \"Assessment\", \"fields\": [" +
            "    { \"key\": \"overall\", \"type\": \"scale\", \"label\": \"Overall\"," +
            "      \"options\": [\"1\", \"2\", \"3\", \"4\", \"5\"], \"scale_key\": \"" + scaleKey + "\" }" +
            "  ] } ] }";

        dbContext.Set<ActivityTypeVersion>().Add(new ActivityTypeVersion
        {
            ActivityTypeId = activityType.Id,
            Version = version,
            SchemaJson = schemaJson,
            WorkflowJson = "{}",
            CreditRulesJson = "{}",
            PublishedByUserId = "seed-system",
            PublishedOn = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedCpsaCurriculumAsync(
        ApplicationDbContext dbContext,
        string traineeUserId,
        bool pin,
        bool profileIsActive = true,
        int? owningInstitutionId = null)
    {
        dbContext.Set<EntrustmentScale>().Add(new EntrustmentScale
        {
            Id = 42, Name = "CPSA Paediatric Entrustment Scale v11.1"
        });
        var labels = new[] { "1", "2", "3a", "3b", "4", "5" };
        for (var order = 1; order <= labels.Length; order++)
        {
            dbContext.Set<EntrustmentLevel>().Add(new EntrustmentLevel
            {
                Id = 4200 + order, ScaleId = 42, Order = order, Label = labels[order - 1]
            });
        }

        dbContext.Set<Curriculum>().Add(new Curriculum
        {
            Id = 55, SubSpecialityId = 1, Name = "Paediatric EPA Curriculum",
            Version = "11.1", EffectiveFrom = new DateOnly(2026, 1, 1), IsActive = true
        });
        dbContext.Set<CurriculumItem>().Add(new CurriculumItem
        {
            Id = 555, CurriculumId = 55, EpaId = 7, RequiredCount = 6,
            MinimumLevelOrder = 6, WindowMonths = 12, ScaleId = pin ? 42 : null,
            OwningInstitutionId = owningInstitutionId
        });
        dbContext.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 5555, UserId = traineeUserId, CurriculumId = 55, InstitutionId = 2,
            ProgrammeStartDate = new DateOnly(2026, 1, 1),
            ExpectedCompletionDate = new DateOnly(2029, 12, 31),
            IsActive = profileIsActive
        });
        await dbContext.SaveChangesAsync();
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static async Task SeedCoreAsync(ApplicationDbContext dbContext)
    {
        dbContext.Epas.Add(new Epa { Id = 7, SubSpecialityId = 1, Code = "EPA-07", Title = "Emergency triage", IsActive = true });
        await dbContext.SaveChangesAsync();
    }

    private static async Task<ActivityType> SeedActivityTypeAsync(ApplicationDbContext dbContext, string key)
    {
        var activityType = new ActivityType
        {
            Key = key,
            Name = key,
            Version = 1,
            IsActive = true,
            OwnerUserId = "admin-1",
            CreatedOn = DateTime.UtcNow
        };
        dbContext.ActivityTypes.Add(activityType);
        await dbContext.SaveChangesAsync();
        return activityType;
    }

    private static void AddRatedActivity(
        ApplicationDbContext dbContext,
        ActivityType activityType,
        string subject,
        string assessor,
        int epaId,
        int overall,
        DateTime createdOn,
        int? specialityId = null,
        int? institutionId = null)
    {
        var dataJson = $"{{\"epa_id\": {epaId}, \"assessor_user_id\": \"{assessor}\", \"overall\": \"{overall}\"}}";
        dbContext.Activities.Add(new Activity
        {
            ActivityTypeId = activityType.Id,
            ActivityType = activityType,
            SchemaVersion = activityType.Version,
            SubjectUserId = subject,
            CreatedByUserId = assessor,
            CurrentState = "completed",
            DataJson = dataJson,
            CreatedOn = createdOn,
            // T119: production stamps this in ActivityService; a fixture that builds the
            // entity directly must set it, or it defaults to 0001-01-01.
            ObservedOn = DateOnly.FromDateTime(createdOn),
            UpdatedOn = createdOn,
            SpecialityId = specialityId,
            InstitutionId = institutionId
        });
    }
}
