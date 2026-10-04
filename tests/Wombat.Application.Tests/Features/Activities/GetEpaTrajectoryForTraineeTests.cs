using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.Activities.Queries.GetEpaTrajectoryForTrainee;
using Wombat.Application.Features.Activities.Queries.ListActivitiesBySubject;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

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

    /// <summary>
    /// T161, D28: a rated type that declares no encounter-date field charts its observation on the day it was filed, and
    /// the point says so. Both undated seeds are unrated, so the type here is a test one; beside it, a type that
    /// declares the date charts on the date the form states. Each row is stamped as <c>ActivityService</c> stamps it.
    /// </summary>
    [Fact]
    public async Task AnObservationWithNoStatedEncounterDate_IsMarkedAsSittingOnItsFilingDay()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var undatedType = await SeedActivityTypeAsync(dbContext, "ward_review");
        var datedType = await SeedTypeAsync(dbContext, "mini_cex", DatedRatedSchemaJson);

        var filed = new DateTime(2026, 3, 20, 9, 0, 0, DateTimeKind.Utc);
        AddRatedActivity(dbContext, undatedType, "trainee-1", "assessor-a", 7, 3, filed);
        AddRatedActivity(dbContext, datedType, "trainee-1", "assessor-a", 7, 4, filed, observedOn: "2026-01-15");
        StampEncounterDatesAsTheServiceDoes(dbContext);
        await dbContext.SaveChangesAsync();

        var result = await new GetEpaTrajectoryForTraineeQueryHandler(dbContext).Handle(
            new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-1")), CancellationToken.None);

        var points = result.Should().ContainSingle().Which.Points;
        points.Select(point => (point.Rating, point.ObservedOn, point.ObservedOnDeclared)).Should().Equal(
            (4, new DateOnly(2026, 1, 15), true),
            (3, new DateOnly(2026, 3, 20), false));
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

    /// <summary>
    /// T255, D48. Each trajectory says whether its EPA is in force now, by the flag the activity's own picker labels by
    /// (<c>EpaOptionLabel</c>), so the heading can mark it "(no longer in use)". A deactivated EPA still charts: its
    /// ratings are evidence already recorded, and deactivating pauses credit, it erases nothing.
    /// </summary>
    [Fact]
    public async Task EachTrajectory_SaysWhetherItsEpaIsInForceNow_AndADeactivatedOneStillCharts()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        dbContext.Epas.Add(new Epa { Id = 3, SubSpecialityId = 1, Code = "EPA-03", Title = "Ward round", IsActive = true });
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");

        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, 3, new DateTime(2026, 2, 1, 9, 0, 0, DateTimeKind.Utc));
        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-b", 3, 4, new DateTime(2026, 2, 5, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        (await dbContext.Epas.SingleAsync(epa => epa.Id == 3))
            .Deactivate(new DateTime(2026, 3, 1, 8, 0, 0, DateTimeKind.Utc)).Should().BeTrue();
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        var result = await new GetEpaTrajectoryForTraineeQueryHandler(dbContext).Handle(
            new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-1")), CancellationToken.None);

        result.Select(dto => (dto.EpaCode, dto.EpaInForce, dto.Points.Count)).Should().Equal(
            ("EPA-03", false, 1),
            ("EPA-07", true, 1));
    }

    [Fact]
    public async Task IgnoresUnratedActivityTypes()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var reflectiveNote = await SeedUnratedActivityTypeAsync(dbContext, "reflective_note");

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
            // T137: stamped as ActivityService would, so the missing rating is not hidden behind a missing EPA.
            EpaId = 7,
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

    /// <summary>
    /// T144: a builder-made rated type under a key no family matches, whose author picked Mini-CEX as its
    /// instrument, charts as Direct observation. Before T144 its source read as its raw key.
    /// </summary>
    [Fact]
    public async Task MapsSourceByTheInstrumentTheTypeDeclares()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var builderMade = await SeedActivityTypeAsync(dbContext, "ward_round_review", wbaToolKey: "mini_cex");

        AddRatedActivity(dbContext, builderMade, "trainee-1", "assessor-a", 7, 3, new DateTime(2026, 2, 1, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var handler = new GetEpaTrajectoryForTraineeQueryHandler(dbContext);
        var result = await handler.Handle(new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-1")), CancellationToken.None);

        var trajectory = result.Should().ContainSingle().Subject;
        trajectory.Points.Should().ContainSingle()
            .Which.Source.Should().Be("Direct observation");
    }

    /// <summary>
    /// T135/T150 retired the literal <c>overall</c>/<c>overall_level</c> pair: the rating is the field the schema
    /// DECLARES. This replaces <c>ReadsOverallLevelFieldWhenOverallAbsent</c>, which pinned the literal fallback on a
    /// type that declared <c>overall</c> and stored <c>overall_level</c> — a row whose declared rating is empty.
    /// </summary>
    [Fact]
    public async Task DoesNotReadAnUndeclaredRatingKey()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var type = await SeedActivityTypeAsync(dbContext, "mini_cex_paed");

        AddActivity(dbContext, type, "completed", """{ "epa_id": 7, "assessor_user_id": "assessor-a", "overall_level": "4" }""");
        await dbContext.SaveChangesAsync();

        var result = await new GetEpaTrajectoryForTraineeQueryHandler(dbContext).Handle(
            new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-1")), CancellationToken.None);

        result.Should().BeEmpty("the type declares 'overall', which this row left empty");
    }

    /// <summary>
    /// A builder type whose EPA, assessor and rating all live under other keys, finishing in a state that is not
    /// called "completed". It charts by what it declares; a decoy <c>assessor_user_id</c> is not read.
    /// </summary>
    [Fact]
    public async Task ChartsARatedBuilderTypeByTheFieldsItDeclares()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var type = await SeedPublishedTypeAsync(dbContext, "ward_round_review", BuilderSchema, BuilderWorkflow, BuilderCredit);

        AddActivity(dbContext, type, "signed_off", """{ "target_epa": 7, "supervisor": "sup-a", "assessor_user_id": "decoy", "entrustment": 4 }""");
        await dbContext.SaveChangesAsync();

        var result = await new GetEpaTrajectoryForTraineeQueryHandler(dbContext).Handle(
            new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-1")), CancellationToken.None);

        var point = result.Should().ContainSingle().Which.Points.Should().ContainSingle().Subject;
        point.Rating.Should().Be(4);
        point.AssessorUserId.Should().Be("sup-a");
    }

    /// <summary>
    /// T106 item 9: only a terminal state of the pinned workflow is an observation (D44). A rating an assessor wrote
    /// and then declined, a draft the trainee filled in, and a request are not.
    /// </summary>
    [Theory]
    [InlineData("draft")]
    [InlineData("requested")]
    [InlineData("declined")]
    [InlineData("cancelled")]
    public async Task PlotsOnlyATerminalStateOfThePinnedWorkflow(string state)
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var miniCex = await SeedTypeFromSeedFolderAsync(dbContext, "mini_cex_cpsa");

        AddActivity(dbContext, miniCex, "completed", """{ "epa_id": 7, "assessor_user_id": "assessor-a", "overall_level": 3 }""");
        AddActivity(dbContext, miniCex, state, """{ "epa_id": 7, "assessor_user_id": "assessor-b", "overall_level": 5 }""");
        await dbContext.SaveChangesAsync();

        var result = await new GetEpaTrajectoryForTraineeQueryHandler(dbContext).Handle(
            new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-1")), CancellationToken.None);

        var point = result.Should().ContainSingle().Which.Points.Should().ContainSingle().Subject;
        point.AssessorUserId.Should().Be("assessor-a");
        point.Rating.Should().Be(3);
    }

    /// <summary>
    /// D36: an MSF names no observing assessor, so its level is not an assessor's rating and does not plot — now by
    /// declaration (its rated field is written by a role, and it has no nominee field) rather than by the accident of
    /// lacking a literal key.
    /// </summary>
    [Fact]
    public async Task AReleasedMsfRecordIsNotPlotted()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var msf = await SeedTypeFromSeedFolderAsync(dbContext, "msf_cpsa");

        AddActivity(dbContext, msf, "recorded", """{ "epa_id": 7, "campaign_id": 1, "observed_on": "2026-02-01", "respondent_count": 8, "overall_level": 4 }""");
        await dbContext.SaveChangesAsync();

        var result = await new GetEpaTrajectoryForTraineeQueryHandler(dbContext).Handle(
            new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-1")), CancellationToken.None);

        result.Should().BeEmpty();
    }

    /// <summary>
    /// The pinned version decides on the chart as it does in the committee report (T135 review). The type is now
    /// <c>mini_cex_cpsa</c> as shipped (v2). Its v1 made <c>declined</c> terminal and let an <c>observer</c> field write
    /// the rating, so a v1 declined row plots, attributed to its observer rather than to the v2 assessor key it also
    /// carries; a v2 declined row does not plot.
    /// </summary>
    [Fact]
    public async Task EachPointIsReadByItsPinnedVersion()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var schema = ReadSeedFile("mini_cex_cpsa", "schema.json");
        var workflow = ReadSeedFile("mini_cex_cpsa", "workflow.json");
        var credit = ReadSeedFile("mini_cex_cpsa", "credit.json");
        var type = await SeedPublishedTypeAsync(dbContext, "mini_cex_cpsa", VersionOneSchema, VersionOneWorkflow, credit);
        type.Version = 2;
        type.SchemaJson = schema;
        type.WorkflowJson = workflow;
        type.Versions.Add(new ActivityTypeVersion
        {
            Version = 2,
            SchemaJson = schema,
            WorkflowJson = workflow,
            CreditRulesJson = credit,
            PublishedByUserId = "seed-system",
            PublishedOn = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();

        AddActivity(dbContext, type, "declined", """{ "epa_id": 7, "observer": "obs-b", "assessor_user_id": "decoy-v1", "overall_level": 2 }""", schemaVersion: 1);
        AddActivity(dbContext, type, "completed", """{ "epa_id": 7, "observer": "obs-a", "assessor_user_id": "decoy-v1", "overall_level": 3 }""", schemaVersion: 1);
        AddActivity(dbContext, type, "declined", """{ "epa_id": 7, "assessor_user_id": "assessor-d", "overall_level": 5 }""", schemaVersion: 2);
        await dbContext.SaveChangesAsync();

        var result = await new GetEpaTrajectoryForTraineeQueryHandler(dbContext).Handle(
            new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-1")), CancellationToken.None);

        var points = result.Should().ContainSingle().Which.Points;
        points.Select(point => point.AssessorUserId).Should().Equal("obs-b", "obs-a");
        points.Select(point => point.Rating).Should().Equal(2, 3);
    }

    /// <summary>
    /// A point is drawn under the EPA stamped on its activity (T137), never under one read out of its data. In the
    /// product the two cannot disagree, since the stamp is re-read from the data on every write, so this is the test that
    /// shows which one the chart consults: two rows carry the same EPA in their data, one is stamped with another, and it
    /// is drawn there.
    /// </summary>
    [Fact]
    public async Task APointIsDrawnUnderTheEpaStampedOnItsActivity_NotOneReadFromItsData()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        dbContext.Epas.Add(new Epa { Id = 3, SubSpecialityId = 1, Code = "EPA-03", Title = "Ward round", IsActive = true });
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");

        AddActivity(dbContext, miniCex, "completed", """{ "epa_id": 7, "assessor_user_id": "assessor-a", "overall": 3 }""");
        AddActivity(dbContext, miniCex, "completed", """{ "epa_id": 7, "assessor_user_id": "assessor-b", "overall": 4 }""").EpaId = 3;
        await dbContext.SaveChangesAsync();

        var result = await new GetEpaTrajectoryForTraineeQueryHandler(dbContext).Handle(
            new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-1")), CancellationToken.None);

        result.Select(dto => dto.EpaCode).Should().Equal("EPA-03", "EPA-07");
        result.Select(dto => dto.Points.Single().AssessorUserId).Should().Equal("assessor-b", "assessor-a");
    }

    /// <summary>
    /// A rated type about no single EPA declares no <c>evidence_epa_field</c>, so its rows are stamped with none and
    /// have no trajectory to be drawn on, however plainly their data holds an EPA. Before the stamp, the chart fell back
    /// to the schema's <c>epa</c> field and drew this row.
    /// </summary>
    [Fact]
    public async Task ARowStampedWithNoEpaIsNotDrawn()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var type = await SeedTypeAsync(dbContext, "item_review", NoEvidenceEpaRatedSchemaJson);

        var row = AddActivity(dbContext, type, "completed", """{ "epa_id": 7, "assessor_user_id": "assessor-a", "overall": 3 }""");
        await dbContext.SaveChangesAsync();

        var result = await new GetEpaTrajectoryForTraineeQueryHandler(dbContext).Handle(
            new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-1")), CancellationToken.None);

        row.EpaId.Should().BeNull("the schema names no EPA field for the stamp to read");
        result.Should().BeEmpty();
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
    public async Task TwoPastProfiles_TheLadderIsThePreferredProfiles_NotTheLaterProgrammeStarts()
    {
        // T185. The ladder comes from the trainee's preferred profile, the highest id when none is active: the profile
        // credit lands on and the export reads. It took the latest programme start, with no final tie-break, so here it
        // read profile 5555's unpinned curriculum and labelled the rating "5" while credit went to profile 6000's.
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        await SeedCpsaCurriculumAsync(dbContext, "trainee-1", pin: false, profileIsActive: false);
        dbContext.Set<Curriculum>().Add(new Curriculum
        {
            Id = 56, SubSpecialityId = 1, Name = "Earlier programme",
            Version = "11.0", EffectiveFrom = new DateOnly(2020, 1, 1), IsActive = true
        });
        dbContext.Set<CurriculumItem>().Add(new CurriculumItem
        {
            Id = 556, CurriculumId = 56, EpaId = 7, RequiredCount = 6, MinimumLevelOrder = 6, WindowMonths = 12, ScaleId = 42
        });
        dbContext.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 6000, UserId = "trainee-1", CurriculumId = 56, InstitutionId = 2,
            ProgrammeStartDate = new DateOnly(2020, 1, 1), ExpectedCompletionDate = new DateOnly(2024, 1, 1),
            IsActive = false
        });
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");

        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, 5, new DateTime(2026, 3, 10, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var result = await new GetEpaTrajectoryForTraineeQueryHandler(dbContext).Handle(
            new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-1")), CancellationToken.None);

        var trajectory = result.Should().ContainSingle().Subject;
        trajectory.ScaleId.Should().Be(42, "profile 6000's curriculum pins the EPA to the CPSA ladder");
        trajectory.Points.Single().RatingLabel.Should().Be("4");
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

    // ---- T355: the EPA filter, the window, the stepped minimum, "against the minimum then", and the names (C10, E2) ----

    /// <summary>The window's first and last day: the 2026 academic year, as the EPA page asks for it.</summary>
    private static readonly DateOnly Year2026From = new(2026, 1, 1);

    private static readonly DateOnly Year2026To = new(2026, 12, 31);

    /// <summary>
    /// A start 3 × 365 days before 2026-01-14, so the training year changes on that day, as it does for the cast's
    /// Molefe and Dlamini (<see cref="TraineeProfile.StageOn" />, whole 365-day blocks): year 3 to 2026-01-13, year 4 from
    /// 2026-01-14.
    /// </summary>
    private static readonly DateOnly SteppedStart = new(2023, 1, 15);

    [Fact]
    public async Task TheEpaFilter_ReadsThatEpasRatingsOnly()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        dbContext.Epas.Add(new Epa { Id = 3, SubSpecialityId = 1, Code = "EPA-03", Title = "Ward round", IsActive = true });
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");

        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, 3, new DateTime(2026, 2, 1, 9, 0, 0, DateTimeKind.Utc));
        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-b", 3, 4, new DateTime(2026, 2, 5, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var result = await new GetEpaTrajectoryForTraineeQueryHandler(dbContext).Handle(
            new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-1"), EpaId: 3), CancellationToken.None);

        result.Select(dto => (dto.EpaCode, dto.Points.Count)).Should().Equal(("EPA-03", 1));
    }

    [Fact]
    public async Task TheWindowRead_IsReturned_WithTheExitLevel()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        await SeedSteppedCurriculumAsync(dbContext, SteppedStart);
        var cpsa = await SeedCpsaRatedTypeAsync(dbContext);

        AddRatedActivity(dbContext, cpsa, "trainee-1", "assessor-a", 7, 5, new DateTime(2025, 12, 31, 9, 0, 0, DateTimeKind.Utc));
        AddRatedActivity(dbContext, cpsa, "trainee-1", "assessor-a", 7, 6, new DateTime(2026, 9, 21, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var trajectory = (await ReadYear2026Async(dbContext)).Should().ContainSingle().Subject;

        trajectory.WindowFrom.Should().Be(Year2026From);
        trajectory.WindowTo.Should().Be(Year2026To);
        trajectory.Points.Select(point => point.ObservedOn).Should().Equal(new DateOnly(2026, 9, 21));
        trajectory.ExitLevelOrder.Should().Be(6);
        trajectory.ExitLevelLabel.Should().Be("5", "ordinal 6 is the College's rung 5, the item's flat minimum");
    }

    /// <summary>
    /// The chart's dashed edge steps where the minimum did: on 2026-01-14, the day <see cref="TraineeProfile.StageOn" />
    /// moves the trainee from training year 3 to 4 (R4).
    /// </summary>
    [Fact]
    public async Task TheMinimumStepsOnTheDayTheTrainingYearChanges()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        await SeedSteppedCurriculumAsync(dbContext, SteppedStart);
        var cpsa = await SeedCpsaRatedTypeAsync(dbContext);

        AddRatedActivity(dbContext, cpsa, "trainee-1", "assessor-a", 7, 6, new DateTime(2026, 9, 21, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var trajectory = (await ReadYear2026Async(dbContext)).Single();

        TraineeProfile.StageOn(SteppedStart, new DateOnly(2026, 1, 13)).Should().Be(3);
        TraineeProfile.StageOn(SteppedStart, new DateOnly(2026, 1, 14)).Should().Be(4);
        trajectory.MinimumSteps.Should().Equal(
            new TrajectoryMinimumStepDto(Year2026From, 3, 5, "4"),
            new TrajectoryMinimumStepDto(new DateOnly(2026, 1, 14), 4, 6, "5"));
    }

    /// <summary>
    /// E2: "against the minimum then" is computed live, at the training year of each encounter, and says what credit said
    /// of the same rating: each point is planned through <see cref="CreditApplier.PlanAsync" />, the credit path itself,
    /// and the plan's comparison for the item is the point's verdict (T355, build review R3): above, at and below.
    /// </summary>
    [Fact]
    public async Task AgainstTheMinimumThen_IsWhatCreditDecided_AtTheTrainingYearOfEachEncounter()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        await SeedSteppedCurriculumAsync(dbContext, SteppedStart);
        var cpsa = await SeedCpsaRatedTypeAsync(dbContext);

        AddRatedActivity(dbContext, cpsa, "trainee-1", "assessor-a", 7, 6, new DateTime(2026, 1, 10, 9, 0, 0, DateTimeKind.Utc)); // year 3: above 4
        AddRatedActivity(dbContext, cpsa, "trainee-1", "assessor-a", 7, 5, new DateTime(2026, 1, 12, 9, 0, 0, DateTimeKind.Utc)); // year 3: at 4
        AddRatedActivity(dbContext, cpsa, "trainee-1", "assessor-a", 7, 5, new DateTime(2026, 3, 10, 9, 0, 0, DateTimeKind.Utc)); // year 4: below 5
        await dbContext.SaveChangesAsync();

        var points = (await ReadYear2026Async(dbContext)).Single().Points;

        points.Select(point => (point.TrainingYear, point.MinimumLabel, point.AgainstMinimum)).Should().Equal(
            (3, "4", TrajectoryAgainstMinimum.AtOrAbove),
            (3, "4", TrajectoryAgainstMinimum.AtOrAbove),
            (4, "5", TrajectoryAgainstMinimum.Below));

        (await CreditVerdictsAsync(dbContext, cpsa, points)).Should().Equal(points.Select(point => point.AgainstMinimum));
    }

    /// <summary>
    /// R3: a rated type whose pinned credit rules credit the item with no minimum-level field counts the rating by volume
    /// alone, so a low rating is not "Below" on the chart: credit made no level judgement, and the plan says so.
    /// </summary>
    [Fact]
    public async Task ARatingWhoseCreditRulesNameNoMinimum_IsNotGated_AsCreditCountsIt()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        await SeedSteppedCurriculumAsync(dbContext, SteppedStart);
        var volumeOnly = await SeedActivityTypeAsync(dbContext, "mini_cex_volume");
        await SeedPinnedVersionAsync(dbContext, volumeOnly, ratedField: "overall", scaleKey: "42", creditRulesJson: VolumeOnlyCreditRules);

        AddRatedActivity(dbContext, volumeOnly, "trainee-1", "assessor-a", 7, 2, new DateTime(2026, 3, 10, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var point = (await ReadYear2026Async(dbContext)).Single().Points.Single();

        point.AgainstMinimum.Should().Be(TrajectoryAgainstMinimum.NotGated);
        point.MinimumLabel.Should().BeNull();
        (await CreditVerdictsAsync(dbContext, volumeOnly, [point])).Should().Equal(TrajectoryAgainstMinimum.NotGated);
    }

    /// <summary>
    /// R2: before the programme starts no training year's minimum applies, so the chart's minimum begins on the start day
    /// and a rating dated before it is judged by no minimum, with no minimum named.
    /// </summary>
    [Fact]
    public async Task BeforeTheProgrammeStarts_NoMinimumIsDrawn_AndARatingThenIsNotGated()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var start = new DateOnly(2026, 3, 1);
        await SeedSteppedCurriculumAsync(dbContext, start);
        var cpsa = await SeedCpsaRatedTypeAsync(dbContext);

        AddRatedActivity(dbContext, cpsa, "trainee-1", "assessor-a", 7, 2, new DateTime(2026, 2, 10, 9, 0, 0, DateTimeKind.Utc));
        AddRatedActivity(dbContext, cpsa, "trainee-1", "assessor-a", 7, 2, new DateTime(2026, 4, 10, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var trajectory = (await ReadYear2026Async(dbContext)).Single();

        trajectory.MinimumSteps.Should().Equal(new TrajectoryMinimumStepDto(start, 1, 3, "3a"));
        trajectory.Points.Select(point => (point.TrainingYear, point.MinimumLabel, point.AgainstMinimum)).Should().Equal(
            (null, null, TrajectoryAgainstMinimum.NotGated),
            (1, "3a", TrajectoryAgainstMinimum.Below));
    }

    /// <summary>
    /// What credit decided of each point's level: the activity planned through <see cref="CreditApplier.PlanAsync" />,
    /// against its pinned version, and the one planned credit's comparison read as the chart's verdict.
    /// </summary>
    private static async Task<IReadOnlyList<TrajectoryAgainstMinimum>> CreditVerdictsAsync(
        ApplicationDbContext dbContext, ActivityType type, IEnumerable<TrajectoryPointDto> points)
    {
        var pinned = await dbContext.Set<ActivityTypeVersion>()
            .SingleAsync(version => version.ActivityTypeId == type.Id && version.Version == type.Version);
        var asPinned = new ActivityType { Id = type.Id, SchemaJson = pinned.SchemaJson, CreditRulesJson = pinned.CreditRulesJson };
        var applier = new CreditApplier(dbContext);

        var verdicts = new List<TrajectoryAgainstMinimum>();
        foreach (var point in points)
        {
            var activity = await dbContext.Activities.SingleAsync(row => row.Id == point.ActivityId);
            var plan = await applier.PlanAsync(CreditSubject.Of(activity), asPinned, CancellationToken.None);
            var comparison = plan.Credits.Should().ContainSingle().Subject.Comparison;
            verdicts.Add(comparison.Basis switch
            {
                LevelComparisonBasis.NoGate => TrajectoryAgainstMinimum.NotGated,
                LevelComparisonBasis.ScaleMismatch => TrajectoryAgainstMinimum.NotComparable,
                _ => comparison.MinimumMet ? TrajectoryAgainstMinimum.AtOrAbove : TrajectoryAgainstMinimum.Below
            });
        }

        return verdicts;
    }

    /// <summary>
    /// C11's "[rating] on [scale]": a rating recorded on another ladder is not comparable (<see
    /// cref="LevelComparisonBasis.ScaleMismatch" />, which credit refuses), and carries its own rung and ladder's name.
    /// </summary>
    [Fact]
    public async Task ARatingOnAnotherLadder_IsNotComparable_AndNamesItsOwnRungAndLadder()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        await SeedSteppedCurriculumAsync(dbContext, SteppedStart);
        SeedOrScale(dbContext);
        var legacy = await SeedActivityTypeAsync(dbContext, "mini_cex_paed");
        await SeedPinnedVersionAsync(dbContext, legacy, ratedField: "overall", scaleKey: "O-R Scale", creditRulesJson: GatingCreditRules);

        AddRatedActivity(dbContext, legacy, "trainee-1", "assessor-a", 7, 4, new DateTime(2026, 8, 10, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var point = (await ReadYear2026Async(dbContext)).Single().Points.Single();

        point.OffLadder.Should().BeTrue();
        point.AgainstMinimum.Should().Be(TrajectoryAgainstMinimum.NotComparable);
        point.OtherScaleName.Should().Be("O-R Scale");
        point.OtherScaleRatingLabel.Should().Be("Independent", "ordinal 4 on the O-R Scale is its own rung, not CPSA's 3b");
        point.RatingLabel.Should().Be("3b", "the label on the axis's ladder is unchanged");
    }

    [Fact]
    public async Task WithNoCurriculumItemForTheEpa_NoMinimumApplies_AndNothingSteps()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");

        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, 3, new DateTime(2026, 3, 10, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var trajectory = (await ReadYear2026Async(dbContext)).Single();

        trajectory.Points.Single().AgainstMinimum.Should().Be(TrajectoryAgainstMinimum.NotGated);
        trajectory.Points.Single().MinimumLabel.Should().BeNull();
        trajectory.MinimumSteps.Should().BeEmpty();
        trajectory.ExitLevelLabel.Should().BeNull();
    }

    /// <summary>
    /// C10, E7: each point names its assessor, and its activity by the name My activities gives the row, read by the same
    /// code over the subject's whole list.
    /// </summary>
    [Fact]
    public async Task EachPointNamesItsAssessor_AndItsActivityAsMyActivitiesNamesTheRow()
    {
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");

        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, 3, new DateTime(2026, 3, 10, 9, 0, 0, DateTimeKind.Utc));
        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-b", 7, 4, new DateTime(2026, 3, 10, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();
        var users = new FakeUserDirectory(("assessor-a", "Thandi Zulu"), ("assessor-b", "David Naidoo"));

        var points = (await new GetEpaTrajectoryForTraineeQueryHandler(dbContext, users).Handle(
            new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-1")), CancellationToken.None)).Single().Points;
        var rows = (await new ListActivitiesBySubjectQueryHandler(dbContext, users).Handle(
            new ListActivitiesBySubjectQuery("trainee-1", Principal("trainee-1")), CancellationToken.None)).Items;

        points.Select(point => point.AssessorName).Should().Equal("Thandi Zulu", "David Naidoo");
        // The form declares no encounter-date field, so the name has no date segment (E9).
        points.Select(point => point.ActivityName).Should().OnlyContain(name => name.StartsWith("mini_cex · EPA-07"));
        points.Select(point => point.ActivityName).Should().Equal(
            points.Select(point => rows.Single(row => row.Id == point.ActivityId).DisplayName),
            "the chart's table names each activity as My activities names its row");
    }

    [Fact]
    public async Task WithNoUserStore_TheAssessorIsTheirId_AndNoActivityIsNamed()
    {
        // The portfolio export calls the handler directly for its counts and prints no name from it.
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");
        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, 3, new DateTime(2026, 3, 10, 9, 0, 0, DateTimeKind.Utc));
        await dbContext.SaveChangesAsync();

        var point = (await new GetEpaTrajectoryForTraineeQueryHandler(dbContext).Handle(
            new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-1")), CancellationToken.None)).Single().Points.Single();

        point.AssessorName.Should().Be("assessor-a");
        point.ActivityName.Should().BeEmpty();
    }

    [Fact]
    public async Task AnotherTraineesEpa_ReadsEmpty_ForACallerWhoDoesNotOverseeThem()
    {
        // T101: the EPA filter narrows what the caller may read; it never widens it.
        await using var dbContext = CreateDbContext();
        await SeedCoreAsync(dbContext);
        var miniCex = await SeedActivityTypeAsync(dbContext, "mini_cex");
        AddRatedActivity(dbContext, miniCex, "trainee-1", "assessor-a", 7, 3, new DateTime(2026, 2, 1, 9, 0, 0, DateTimeKind.Utc), specialityId: 4);
        await dbContext.SaveChangesAsync();

        var result = await new GetEpaTrajectoryForTraineeQueryHandler(dbContext, new FakeUserDirectory()).Handle(
            new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-2"), Year2026From, Year2026To, EpaId: 7),
            CancellationToken.None);

        result.Should().BeEmpty();
    }

    private const string SteppedMap = """{ "1": 3, "2": 4, "3": 5, "4": 6 }""";

    private static Task<IReadOnlyList<EpaTrajectoryDto>> ReadYear2026Async(ApplicationDbContext dbContext)
        => new GetEpaTrajectoryForTraineeQueryHandler(dbContext, new FakeUserDirectory()).Handle(
            new GetEpaTrajectoryForTraineeQuery("trainee-1", Principal("trainee-1"), Year2026From, Year2026To, EpaId: 7),
            CancellationToken.None);

    /// <summary>
    /// A rated type whose pinned version rates on the CPSA ladder (scale 42), the item's own, and credits the EPA's item
    /// gated on that rating, as the seeded CPSA tools do (<c>minimum_level_field</c>).
    /// </summary>
    private static async Task<ActivityType> SeedCpsaRatedTypeAsync(ApplicationDbContext dbContext)
    {
        var cpsa = await SeedActivityTypeAsync(dbContext, "mini_cex_cpsa");
        await SeedPinnedVersionAsync(dbContext, cpsa, ratedField: "overall", scaleKey: "42", creditRulesJson: GatingCreditRules);
        return cpsa;
    }

    /// <summary>Credits the item of the EPA the activity names, gated on the rating, as the seeded CPSA tools' rules do.</summary>
    private const string GatingCreditRules =
        """{ "counts_for": [ { "curriculum_item_match": { "epa_field": "epa_id" }, "amount": 1, "minimum_level_field": "overall" } ] }""";

    /// <summary>Credits the item of the EPA the activity names by volume alone: no minimum-level field (R3).</summary>
    private const string VolumeOnlyCreditRules =
        """{ "counts_for": [ { "curriculum_item_match": { "epa_field": "epa_id" }, "amount": 1 } ] }""";

    /// <summary>
    /// The CPSA ladder, an item for EPA 7 pinned to it with Annexure A's per-year map (year 3 at ordinal 5, rung "4"; year
    /// 4 at ordinal 6, rung "5"; exit "5"), and the trainee's profile from <paramref name="programmeStart" />.
    /// </summary>
    private static async Task SeedSteppedCurriculumAsync(ApplicationDbContext dbContext, DateOnly programmeStart)
    {
        dbContext.Set<EntrustmentScale>().Add(new EntrustmentScale
        {
            Id = 42, Name = "CPSA Paediatric Entrustment Scale v11.1", SeedKey = "cpsa:scale:v11.1"
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
            Id = 555, CurriculumId = 55, EpaId = 7, RequiredCount = 3, QuotaPeriod = QuotaPeriod.Semester,
            MinimumLevelOrder = 6, MinimumLevelByStageJson = SteppedMap, WindowMonths = 12, ScaleId = 42
        });
        dbContext.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 5555, UserId = "trainee-1", CurriculumId = 55, InstitutionId = 2,
            ProgrammeStartDate = programmeStart,
            ExpectedCompletionDate = programmeStart.AddYears(4),
            IsActive = true
        });
        await dbContext.SaveChangesAsync();
    }

    private static void SeedOrScale(ApplicationDbContext dbContext)
    {
        dbContext.Set<EntrustmentScale>().Add(new EntrustmentScale { Id = 43, Name = "O-R Scale", SeedKey = "demo:scale:o-r" });
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
        int version = 1,
        string creditRulesJson = "{}")
    {
        var pointer = ratedField is null
            ? string.Empty
            : "\"rated_level_field\": \"" + ratedField + "\",";

        // The EPA field and its pointer are in every version, as T137's migration left every stored version whose EPA
        // field was determined: the activity pinned here is stamped with its EPA from them.
        var schemaJson =
            "{ \"version\": 1, " + pointer + " \"evidence_epa_field\": \"epa_id\"," +
            "  \"sections\": [" +
            "  { \"key\": \"request\", \"title\": \"Request\", \"fields\": [" +
            "    { \"key\": \"epa_id\", \"type\": \"epa\", \"label\": \"EPA\" }," +
            "    { \"key\": \"assessor_user_id\", \"type\": \"user\", \"label\": \"Assessor\" } ] }," +
            "  { \"key\": \"assessment\", \"title\": \"Assessment\", \"editable_by\": \"field:assessor_user_id\", \"fields\": [" +
            "    { \"key\": \"overall\", \"type\": \"scale\", \"label\": \"Overall\"," +
            "      \"options\": [\"1\", \"2\", \"3\", \"4\", \"5\"], \"scale_key\": \"" + scaleKey + "\" }" +
            "  ] } ] }";

        dbContext.Set<ActivityTypeVersion>().Add(new ActivityTypeVersion
        {
            ActivityTypeId = activityType.Id,
            Version = version,
            SchemaJson = schemaJson,
            WorkflowJson = "{}",
            CreditRulesJson = creditRulesJson,
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
            Id = 42, Name = "CPSA Paediatric Entrustment Scale v11.1", SeedKey = "cpsa:scale:v11.1"
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

    /// <summary>
    /// A rated type, as the product produces one since T126: its schema declares which field holds
    /// the entrustment rating. Types with no schema at all used to be seeded here, which is a shape
    /// nothing can publish — and it is what let the trajectory gate stay on a hard-coded key list.
    /// </summary>
    private static Task<ActivityType> SeedActivityTypeAsync(
        ApplicationDbContext dbContext,
        string key,
        string? wbaToolKey = null)
        => SeedTypeAsync(dbContext, key, RatedSchemaJson, wbaToolKey);

    /// <summary>A type that asserts no entrustment level, and must not chart.</summary>
    private static Task<ActivityType> SeedUnratedActivityTypeAsync(ApplicationDbContext dbContext, string key)
        => SeedTypeAsync(dbContext, key, UnratedSchemaJson);

    private static async Task<ActivityType> SeedTypeAsync(
        ApplicationDbContext dbContext,
        string key,
        string schemaJson,
        string? wbaToolKey = null)
    {
        var activityType = new ActivityType
        {
            Key = key,
            Name = key,
            Version = 1,
            IsActive = true,
            OwnerUserId = "admin-1",
            CreatedOn = DateTime.UtcNow,
            SchemaJson = schemaJson,
            WbaToolKey = wbaToolKey
        };
        dbContext.ActivityTypes.Add(activityType);
        await dbContext.SaveChangesAsync();
        return activityType;
    }

    /// <summary>
    /// Declares its assessor as well as its rated field (T135, T150): the chart plots an assessor's rating, and the
    /// assessor is whoever the rated field's <c>editable_by</c> names. A type naming none is the MSF shape (D36). It also
    /// names the field its rows are filed against (<c>evidence_epa_field</c>, T137): the chart draws a point under the EPA
    /// stamped from that pointer, and a type with no pointer stamps none.
    /// </summary>
    private const string RatedSchemaJson = """
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
                { "key": "overall", "type": "scale", "label": "Overall", "options": ["1", "2"], "scale_key": "O-R Scale" }
              ]
            }
          ]
        }
        """;

    /// <summary><see cref="RatedSchemaJson" /> that also declares where the encounter date is written (T119).</summary>
    private const string DatedRatedSchemaJson = """
        {
          "version": 1,
          "observation_date_field": "observed_on",
          "rated_level_field": "overall",
          "evidence_epa_field": "epa_id",
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "epa_id", "type": "epa", "label": "EPA" },
                { "key": "observed_on", "type": "date", "label": "Encounter date" },
                { "key": "assessor_user_id", "type": "user", "label": "Assessor" }
              ]
            },
            {
              "key": "assessment",
              "title": "Assessment",
              "editable_by": "field:assessor_user_id",
              "fields": [
                { "key": "overall", "type": "scale", "label": "Overall", "options": ["1", "2"], "scale_key": "O-R Scale" }
              ]
            }
          ]
        }
        """;

    /// <summary><see cref="RatedSchemaJson" /> about no single EPA: the same <c>epa</c> field, and no pointer at it.</summary>
    private const string NoEvidenceEpaRatedSchemaJson = """
        {
          "version": 1,
          "rated_level_field": "overall",
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
                { "key": "overall", "type": "scale", "label": "Overall", "options": ["1", "2"], "scale_key": "O-R Scale" }
              ]
            }
          ]
        }
        """;

    private const string UnratedSchemaJson = """
        {
          "version": 1,
          "sections": [
            {
              "key": "reflection",
              "title": "Reflection",
              "fields": [
                { "key": "what_i_learned", "type": "longtext", "label": "What I learned" }
              ]
            }
          ]
        }
        """;

    private const string BuilderSchema = """
        {
          "version": 1,
          "rated_level_field": "entrustment",
          "evidence_epa_field": "target_epa",
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "target_epa", "type": "epa", "label": "EPA" },
                { "key": "supervisor", "type": "user", "label": "Supervisor" }
              ]
            },
            {
              "key": "judgement",
              "title": "Judgement",
              "editable_by": "field:supervisor",
              "fields": [
                { "key": "entrustment", "type": "scale", "label": "Entrustment", "scale_key": "CPSA Paediatric Entrustment Scale v11.1" }
              ]
            }
          ]
        }
        """;

    private const string BuilderWorkflow = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "signed_off", "label": "Signed off", "terminal": true }
          ],
          "transitions": [
            { "key": "sign_off", "from": "draft", "to": "signed_off", "actor": "field:supervisor", "validation": "all" }
          ]
        }
        """;

    private const string BuilderCredit = """
        {
          "counts_for": [
            { "curriculum_item_match": { "epa_field": "target_epa" }, "amount": 1, "minimum_level_field": "entrustment" }
          ]
        }
        """;

    private static Task<ActivityType> SeedTypeFromSeedFolderAsync(ApplicationDbContext dbContext, string key)
        => SeedPublishedTypeAsync(
            dbContext,
            key,
            ReadSeedFile(key, "schema.json"),
            ReadSeedFile(key, "workflow.json"),
            ReadSeedFile(key, "credit.json"));

    private static string ReadSeedFile(string key, string fileName)
        => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Activities", "Seeds", key, fileName));

    /// <summary>A type published at v1 with its version row, as the product pins an activity to it.</summary>
    private static async Task<ActivityType> SeedPublishedTypeAsync(
        ApplicationDbContext dbContext,
        string key,
        string schemaJson,
        string workflowJson,
        string creditRulesJson)
    {
        var activityType = new ActivityType
        {
            Key = key,
            Name = key,
            Version = 1,
            IsActive = true,
            OwnerUserId = "seed-system",
            CreatedOn = DateTime.UtcNow,
            SchemaJson = schemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = creditRulesJson
        };
        activityType.Versions.Add(new ActivityTypeVersion
        {
            Version = 1,
            SchemaJson = schemaJson,
            WorkflowJson = workflowJson,
            CreditRulesJson = creditRulesJson,
            PublishedByUserId = "seed-system",
            PublishedOn = DateTime.UtcNow
        });
        dbContext.ActivityTypes.Add(activityType);
        await dbContext.SaveChangesAsync();
        return activityType;
    }

    /// <summary><c>mini_cex_cpsa</c>'s v1 as it might have been: an <c>observer</c> writes the rating, and
    /// <c>declined</c> is terminal, as the generic seeds before <c>c33c14b</c> had it.</summary>
    private const string VersionOneSchema = """
        {
          "version": 1,
          "rated_level_field": "overall_level",
          "evidence_epa_field": "epa_id",
          "sections": [
            {
              "key": "request",
              "title": "Request",
              "fields": [
                { "key": "epa_id", "type": "epa", "label": "EPA", "required": true },
                { "key": "observer", "type": "user", "label": "Observer", "required": true },
                { "key": "assessor_user_id", "type": "user", "label": "Assessor" }
              ]
            },
            {
              "key": "assessment",
              "title": "Assessment",
              "editable_by": "field:observer",
              "fields": [
                { "key": "overall_level", "type": "scale", "label": "Overall", "required": true, "scale_key": "CPSA Paediatric Entrustment Scale v11.1" }
              ]
            }
          ]
        }
        """;

    private const string VersionOneWorkflow = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "requested", "label": "Requested", "editable_by": "field:observer" },
            { "key": "completed", "label": "Completed", "terminal": true },
            { "key": "declined", "label": "Declined", "terminal": true }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "requested", "actor": "subject|creator", "validation": "owned" },
            { "key": "complete", "from": "requested", "to": "completed", "actor": "field:observer", "validation": "all" },
            { "key": "decline", "from": "requested", "to": "declined", "actor": "field:observer", "validation": "draft" }
          ]
        }
        """;

    /// <summary>
    /// A row as <c>ActivityService</c> leaves it, including the EPA it stamps from the pinned version's pointer (T137),
    /// which is the only place the chart reads the EPA from. Returned so a test can set a stamp the data does not imply.
    /// </summary>
    private static Activity AddActivity(
        ApplicationDbContext dbContext,
        ActivityType activityType,
        string state,
        string dataJson,
        int? schemaVersion = null)
    {
        var on = new DateTime(2026, 2, 1, 9, 0, 0, DateTimeKind.Utc);
        var pinnedVersion = schemaVersion ?? activityType.Version;
        var activity = new Activity
        {
            ActivityTypeId = activityType.Id,
            SchemaVersion = pinnedVersion,
            SubjectUserId = "trainee-1",
            CreatedByUserId = "trainee-1",
            CurrentState = state,
            DataJson = dataJson,
            EpaId = EvidenceEpaStamp.For(dbContext, activityType.Id, pinnedVersion, dataJson),
            CreatedOn = on,
            ObservedOn = DateOnly.FromDateTime(on),
            UpdatedOn = on
        };
        dbContext.Activities.Add(activity);
        return activity;
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
        int? institutionId = null,
        string? observedOn = null)
    {
        var encounterDate = observedOn is null ? string.Empty : $", \"observed_on\": \"{observedOn}\"";
        var dataJson = $"{{\"epa_id\": {epaId}, \"assessor_user_id\": \"{assessor}\", \"overall\": \"{overall}\"{encounterDate}}}";
        dbContext.Activities.Add(new Activity
        {
            ActivityTypeId = activityType.Id,
            ActivityType = activityType,
            SchemaVersion = activityType.Version,
            SubjectUserId = subject,
            CreatedByUserId = assessor,
            CurrentState = "completed",
            DataJson = dataJson,
            // T137: likewise the EPA, from the pinned schema's pointer; the chart reads it from there alone.
            EpaId = EvidenceEpaStamp.For(dbContext, activityType.Id, activityType.Version, dataJson),
            CreatedOn = createdOn,
            // T119: production stamps this in ActivityService; a fixture that builds the
            // entity directly must set it, or it defaults to 0001-01-01.
            ObservedOn = DateOnly.FromDateTime(createdOn),
            UpdatedOn = createdOn,
            SpecialityId = specialityId,
            InstitutionId = institutionId
        });
    }

    /// <summary>
    /// Stamps each new row's encounter date and its source from its type's schema, with the resolver
    /// <c>ActivityService</c> uses on every write (T119). The other fixtures here set the date directly, which leaves the
    /// source at its default, Declared.
    /// </summary>
    private static void StampEncounterDatesAsTheServiceDoes(ApplicationDbContext dbContext)
    {
        foreach (var activity in dbContext.Activities.Local)
        {
            ObservationDateResolver.Stamp(activity, FormSchemaParser.Parse(activity.ActivityType.SchemaJson!), activity.DataJson);
        }
    }
}
