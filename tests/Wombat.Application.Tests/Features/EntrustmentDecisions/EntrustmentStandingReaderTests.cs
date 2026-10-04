using FluentAssertions;
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
/// The standing's shared reader (T355, note 3): <c>GetEntrustmentStandingForTraineeQuery</c>'s body after its scope check,
/// whose rules <c>GetEntrustmentStandingForTraineeTests</c> holds, and its summary mode for the Trainee's Home, which reads
/// no rating and is otherwise the same standing.
/// </summary>
public sealed class EntrustmentStandingReaderTests
{
    internal const string TraineeUserId = "trainee-1";
    private const int Cpsa = 42;

    /// <summary>424 days after a 1 January 2025 start: training year 2.</summary>
    internal static readonly DateOnly YearTwo = new(2026, 3, 1);

    [Fact]
    public async Task InSummaryMode_NoRatingIsRead_AndEverythingElseIsTheQuerysStanding()
    {
        await using var db = AssessorReads.CreateDb();
        await SeedRatedAsync(db);
        var self = TestPrincipals.Trainee(TraineeUserId, 1);

        var full = await new GetEntrustmentStandingForTraineeQueryHandler(db).Handle(
            new GetEntrustmentStandingForTraineeQuery(TraineeUserId, self, YearTwo), CancellationToken.None);
        var summary = await EntrustmentStandingReader.ReadAsync(
            db, self, TraineeUserId, YearTwo, withLatestRatings: false, CancellationToken.None);

        full!.Epas.Single().LatestRating.Should().NotBeNull("the query reads the latest rating");
        summary.Should().NotBeNull();
        summary!.Epas.Should().AllSatisfy(epa => epa.LatestRating.Should().BeNull());
        summary.Should().BeEquivalentTo(
            full, options => options.Excluding(standing => standing.Epas),
            "the summary is the same standing without the ratings");
        summary.Epas.Should().BeEquivalentTo(full.Epas, options => options.Excluding(epa => epa.LatestRating));
    }

    [Fact]
    public async Task WithLatestRatings_TheReaderIsTheQuery()
    {
        await using var db = AssessorReads.CreateDb();
        await SeedRatedAsync(db);
        var self = TestPrincipals.Trainee(TraineeUserId, 1);

        var viaQuery = await new GetEntrustmentStandingForTraineeQueryHandler(db).Handle(
            new GetEntrustmentStandingForTraineeQuery(TraineeUserId, self, YearTwo), CancellationToken.None);
        var viaReader = await EntrustmentStandingReader.ReadAsync(
            db, self, TraineeUserId, YearTwo, withLatestRatings: true, CancellationToken.None);

        viaReader.Should().BeEquivalentTo(viaQuery);
    }

    [Fact]
    public async Task ATraineeWithNoProfile_ReadsNull()
    {
        await using var db = AssessorReads.CreateDb();

        (await EntrustmentStandingReader.ReadAsync(
                db, TestPrincipals.Trainee("nobody", 1), "nobody", YearTwo, withLatestRatings: false, CancellationToken.None))
            .Should().BeNull();
    }

    /// <summary>
    /// One EPA on the CPSA ladder (exit rung 5, year 2's level 3b), a STAR at 3b, and one completed Mini-CEX rated 4 on it:
    /// a standing with a latest rating to read or leave out.
    /// </summary>
    internal static async Task SeedRatedAsync(ApplicationDbContext db)
    {
        db.Institutions.Add(new Institution { Id = 1, Name = "Host" });
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
            Id = 10, SubSpecialityId = 1, Name = "Paediatric EPA Curriculum", Version = "11.1",
            EffectiveFrom = new DateOnly(2025, 1, 1), IsActive = true
        });
        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = 101, CurriculumId = 10, EpaId = 1,
            Epa = new Epa { Id = 1, SubSpecialityId = 1, Code = "PAED-001", Title = "PAED-001 title", IsActive = true },
            RequiredCount = 3, QuotaPeriod = QuotaPeriod.Semester, MinimumLevelOrder = 6,
            MinimumLevelByStageJson = """{"1": 3, "2": 4, "3": 5, "4": 6}""", WindowMonths = 12, ScaleId = Cpsa
        });
        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1, UserId = TraineeUserId, InstitutionId = 1, CurriculumId = 10,
            ProgrammeStartDate = new DateOnly(2025, 1, 1), ExpectedCompletionDate = new DateOnly(2029, 1, 1), IsActive = true
        });
        db.EntrustmentDecisions.Add(EntrustmentDecision.Issue(
            TraineeUserId, 1, 4204, new DateOnly(2026, 1, 15), expiresOn: null,
            committeeReviewId: 30, "chair-1", "Consistent across the period.", StarEvidence.One()));
        await db.SaveChangesAsync();

        const string schemaJson = """
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
                    { "key": "overall", "type": "scale", "label": "Overall", "scale_key": "CPSA Paediatric Entrustment Scale v11.1" }
                  ]
                }
              ]
            }
            """;
        var type = new ActivityType
        {
            Key = "rated", Name = "Rated", Version = 1, IsActive = true, OwnerUserId = "seed-system",
            CreatedOn = DateTime.UtcNow, SchemaJson = schemaJson, WorkflowJson = "{}", CreditRulesJson = "{}"
        };
        type.Versions.Add(new ActivityTypeVersion
        {
            Version = 1, SchemaJson = schemaJson, WorkflowJson = "{}", CreditRulesJson = "{}",
            PublishedByUserId = "seed-system", PublishedOn = DateTime.UtcNow
        });
        db.ActivityTypes.Add(type);
        await db.SaveChangesAsync();

        const string dataJson = """{"epa_id": 1, "assessor_user_id": "assessor-a", "overall": "5"}""";
        var on = new DateTime(2026, 2, 20, 9, 0, 0, DateTimeKind.Utc);
        db.Activities.Add(new Activity
        {
            ActivityTypeId = type.Id, SchemaVersion = 1, SubjectUserId = TraineeUserId, CreatedByUserId = "assessor-a",
            CurrentState = "completed", DataJson = dataJson,
            EpaId = EvidenceEpaStamp.For(db, type.Id, 1, dataJson),
            CreatedOn = on, UpdatedOn = on, ObservedOn = new DateOnly(2026, 2, 20),
            ObservedOnSource = ObservationDateSource.Declared, InstitutionId = 1, SpecialityId = 1, SubSpecialityId = 1
        });
        await db.SaveChangesAsync();
    }
}
