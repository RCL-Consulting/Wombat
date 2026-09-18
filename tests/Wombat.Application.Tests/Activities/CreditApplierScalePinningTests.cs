using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Activities;

/// <summary>
/// T109 — a rating on one entrustment ladder must never be compared against a minimum on another.
/// </summary>
/// <remarks>
/// Half of these cases assert that nothing changed. That is the point: the refusal fires only when both
/// ladders are known and differ, so every combination that credits today — no schema, no scale field, an
/// unresolvable <c>scale_key</c>, a <c>minimum_level_fixed</c> literal, an unpinned curriculum item — has to
/// go on crediting exactly as it did.
/// </remarks>
public sealed class CreditApplierScalePinningTests
{
    private const int FiveRungScaleId = 900;
    private const int SixRungScaleId = 901;
    private const string FiveRungScaleName = "Paed General Entrustment Scale";
    private const string SixRungScaleName = "CPSA Paediatric Entrustment Scale v11.1";

    [Fact]
    public async Task WhenTheAssessmentAndTheCurriculumAreOnDifferentScales_CountsVolumeButRefusesTheMinimum()
    {
        // T109 exactly as reported. The trainee is pinned to a five-rung curriculum whose minimum of 4
        // means "Independent". They file a tool bound to the six-rung CPSA ladder, where order 4 is the
        // rung labelled "3b" — still needing supervision. 4 >= 4 is arithmetically true, clinically false.
        await using var dbContext = CreateDbContext();
        await SeedAsync(dbContext, pinCurriculumItemToScaleId: FiveRungScaleId);

        var result = await ApplyAsync(dbContext, achievedOrder: 4, schemaJson: SchemaBindingScoreTo(SixRungScaleName));

        result.ScaleMismatchCount.Should().Be(1);
        result.UnverifiedLevelCount.Should().Be(0);

        var progress = await dbContext.CurriculumItemProgresses.SingleAsync();
        progress.CountsSoFar.Should().Be(1, "the encounter happened and is evidence of volume");
        progress.MinimumLevelReachedCount.Should().Be(0, "the two ordinals are not comparable");
        progress.ScaleMismatchCount.Should().Be(1);
        progress.UnverifiedLevelCount.Should().Be(0);
        progress.MinimumLevelScaleId.Should().BeNull("no verified comparison has contributed to this tally");
    }

    [Fact]
    public async Task WhenTheAssessmentAndTheCurriculumAreOnTheSameScale_CreditsTheMinimumAndRecordsTheLadder()
    {
        await using var dbContext = CreateDbContext();
        await SeedAsync(dbContext, pinCurriculumItemToScaleId: SixRungScaleId);

        var result = await ApplyAsync(dbContext, achievedOrder: 4, schemaJson: SchemaBindingScoreTo(SixRungScaleName));

        result.ScaleMismatchCount.Should().Be(0);
        result.UnverifiedLevelCount.Should().Be(0);

        var progress = await dbContext.CurriculumItemProgresses.SingleAsync();
        progress.CountsSoFar.Should().Be(1);
        progress.MinimumLevelReachedCount.Should().Be(1);
        progress.ScaleMismatchCount.Should().Be(0);
        progress.MinimumLevelScaleId.Should().Be(SixRungScaleId, "the row records the ladder it was scored on");
    }

    [Fact]
    public async Task WhenOnTheSameScaleButBelowTheMinimum_StillCountsVolumeOnly()
    {
        await using var dbContext = CreateDbContext();
        await SeedAsync(dbContext, pinCurriculumItemToScaleId: SixRungScaleId);

        var result = await ApplyAsync(dbContext, achievedOrder: 2, schemaJson: SchemaBindingScoreTo(SixRungScaleName));

        result.ScaleMismatchCount.Should().Be(0, "a scale mismatch and an honest shortfall are different things");

        var progress = await dbContext.CurriculumItemProgresses.SingleAsync();
        progress.CountsSoFar.Should().Be(1);
        progress.MinimumLevelReachedCount.Should().Be(0);
        progress.ScaleMismatchCount.Should().Be(0);
    }

    [Fact]
    public async Task WhenTheScaleIsResolvedByNumericId_ItComparesTheSameWay()
    {
        // ActivityReferenceDataService resolves a scale_key by numeric id OR exact name; the engine has to
        // match that, or the picker and the credit rules would disagree about which ladder a field is on.
        await using var dbContext = CreateDbContext();
        await SeedAsync(dbContext, pinCurriculumItemToScaleId: FiveRungScaleId);

        var result = await ApplyAsync(
            dbContext,
            achievedOrder: 4,
            schemaJson: SchemaBindingScoreTo(SixRungScaleId.ToString(System.Globalization.CultureInfo.InvariantCulture)));

        result.ScaleMismatchCount.Should().Be(1);
        (await dbContext.CurriculumItemProgresses.SingleAsync()).MinimumLevelReachedCount.Should().Be(0);
    }

    [Fact]
    public async Task WhenTheCurriculumItemIsUnpinned_ComparesExactlyAsBeforeT109()
    {
        await using var dbContext = CreateDbContext();
        await SeedAsync(dbContext, pinCurriculumItemToScaleId: null);

        var result = await ApplyAsync(dbContext, achievedOrder: 4, schemaJson: SchemaBindingScoreTo(SixRungScaleName));

        result.ScaleMismatchCount.Should().Be(0);
        result.UnverifiedLevelCount.Should().Be(1, "the pass rests on ordinals nobody has verified");

        var progress = await dbContext.CurriculumItemProgresses.SingleAsync();
        progress.MinimumLevelReachedCount.Should().Be(1);
        progress.UnverifiedLevelCount.Should().Be(1);
        progress.MinimumLevelScaleId.Should().BeNull();
    }

    [Fact]
    public async Task WhenTheScaleKeyResolvesToNothing_ComparesExactlyAsBeforeT109()
    {
        // mini_cex, cbd, dops and acat declare scale_key "or_scale" while the seeded scale is named
        // "O-R Scale", so they resolve to nothing today. They must go on crediting precisely as they do.
        await using var dbContext = CreateDbContext();
        await SeedAsync(dbContext, pinCurriculumItemToScaleId: FiveRungScaleId);

        var result = await ApplyAsync(dbContext, achievedOrder: 4, schemaJson: SchemaBindingScoreTo("or_scale"));

        result.ScaleMismatchCount.Should().Be(0);
        result.UnverifiedLevelCount.Should().Be(1);
        (await dbContext.CurriculumItemProgresses.SingleAsync()).MinimumLevelReachedCount.Should().Be(1);
    }

    [Fact]
    public async Task WhenTheActivityTypeCarriesNoSchema_ComparesExactlyAsBeforeT109()
    {
        // The synthetic ActivityType shape both call sites passed before T109 carried CreditRulesJson alone.
        await using var dbContext = CreateDbContext();
        await SeedAsync(dbContext, pinCurriculumItemToScaleId: FiveRungScaleId);

        var result = await ApplyAsync(dbContext, achievedOrder: 4, schemaJson: null);

        result.ScaleMismatchCount.Should().Be(0);
        result.UnverifiedLevelCount.Should().Be(1);
        (await dbContext.CurriculumItemProgresses.SingleAsync()).MinimumLevelReachedCount.Should().Be(1);
    }

    [Fact]
    public async Task WhenTheSchemaIsUnparseable_ComparesExactlyAsBeforeT109()
    {
        await using var dbContext = CreateDbContext();
        await SeedAsync(dbContext, pinCurriculumItemToScaleId: FiveRungScaleId);

        var result = await ApplyAsync(dbContext, achievedOrder: 4, schemaJson: "{ not json at all");

        result.ScaleMismatchCount.Should().Be(0);
        result.UnverifiedLevelCount.Should().Be(1);
        (await dbContext.CurriculumItemProgresses.SingleAsync()).MinimumLevelReachedCount.Should().Be(1);
    }

    [Fact]
    public async Task WhenTheSchemaFieldDeclaresNoScale_ComparesExactlyAsBeforeT109()
    {
        await using var dbContext = CreateDbContext();
        await SeedAsync(dbContext, pinCurriculumItemToScaleId: FiveRungScaleId);

        var schemaWithoutScaleKey = """
            {
              "version": 1,
              "sections": [
                {
                  "key": "assessment",
                  "title": "Entrustment",
                  "fields": [
                    { "key": "score", "type": "rating", "label": "Level" }
                  ]
                }
              ]
            }
            """;

        var result = await ApplyAsync(dbContext, achievedOrder: 4, schemaJson: schemaWithoutScaleKey);

        result.ScaleMismatchCount.Should().Be(0);
        result.UnverifiedLevelCount.Should().Be(1);
        (await dbContext.CurriculumItemProgresses.SingleAsync()).MinimumLevelReachedCount.Should().Be(1);
    }

    [Fact]
    public async Task WhenTheDirectiveGatesOnAFixedLevel_ComparesExactlyAsBeforeT109()
    {
        // minimum_level_fixed is a literal in the credit rules: it names no schema field and therefore has
        // no ladder of its own, so it is unpinned by construction and must never be refused.
        await using var dbContext = CreateDbContext();
        await SeedAsync(dbContext, pinCurriculumItemToScaleId: FiveRungScaleId);

        var fixedLevelType = new ActivityType
        {
            CreditRulesJson = """
                {
                  "counts_for": [
                    {
                      "curriculum_item_match": { "epa_field": "epa_id" },
                      "amount": 1,
                      "minimum_level_fixed": "4"
                    }
                  ]
                }
                """,
            SchemaJson = SchemaBindingScoreTo(SixRungScaleName)
        };

        var applier = new CreditApplier(dbContext);
        var result = await applier.ApplyAsync(
            CreateCompletedActivity("""{ "epa_id": 5000, "score": 1 }"""), fixedLevelType, CancellationToken.None);
        await dbContext.SaveChangesAsync();

        result.ScaleMismatchCount.Should().Be(0);
        result.UnverifiedLevelCount.Should().Be(1);
        (await dbContext.CurriculumItemProgresses.SingleAsync()).MinimumLevelReachedCount.Should().Be(1);
    }

    [Fact]
    public async Task WhenTheDirectiveGatesOnNoLevelAtAll_NeitherCounterMoves()
    {
        // A reflective note or procedure log credits volume without any entrustment judgement. It carries
        // no ladder on either side of anything and must not be counted as unverified.
        await using var dbContext = CreateDbContext();
        await SeedAsync(dbContext, pinCurriculumItemToScaleId: FiveRungScaleId);

        var ungatedType = new ActivityType
        {
            CreditRulesJson = """
                {
                  "counts_for": [
                    {
                      "curriculum_item_match": { "epa_field": "epa_id" },
                      "amount": 1
                    }
                  ]
                }
                """,
            SchemaJson = SchemaBindingScoreTo(SixRungScaleName)
        };

        var applier = new CreditApplier(dbContext);
        var result = await applier.ApplyAsync(
            CreateCompletedActivity("""{ "epa_id": 5000 }"""), ungatedType, CancellationToken.None);
        await dbContext.SaveChangesAsync();

        result.ScaleMismatchCount.Should().Be(0);
        result.UnverifiedLevelCount.Should().Be(0);

        var progress = await dbContext.CurriculumItemProgresses.SingleAsync();
        progress.CountsSoFar.Should().Be(1);
        progress.MinimumLevelReachedCount.Should().Be(1);
        progress.UnverifiedLevelCount.Should().Be(0);
    }

    private static async Task<Application.Features.Activities.Services.CreditApplicationResult> ApplyAsync(
        ApplicationDbContext dbContext,
        int achievedOrder,
        string? schemaJson)
    {
        var applier = new CreditApplier(dbContext);
        var activity = CreateCompletedActivity($$"""{ "epa_id": 5000, "score": {{achievedOrder}} }""");

        var result = await applier.ApplyAsync(
            activity,
            new ActivityType
            {
                CreditRulesJson = """
                    {
                      "counts_for": [
                        {
                          "curriculum_item_match": { "epa_field": "epa_id" },
                          "amount": 1,
                          "minimum_level_field": "score"
                        }
                      ]
                    }
                    """,
                SchemaJson = schemaJson ?? string.Empty
            },
            CancellationToken.None);

        await dbContext.SaveChangesAsync();
        return result;
    }

    private static string SchemaBindingScoreTo(string scaleKey)
        => $$"""
            {
              "version": 1,
              "sections": [
                {
                  "key": "assessment",
                  "title": "Entrustment",
                  "fields": [
                    { "key": "score", "type": "scale", "label": "Level", "scale_key": "{{scaleKey}}" }
                  ]
                }
              ]
            }
            """;

    private static Activity CreateCompletedActivity(string dataJson)
        => new()
        {
            Id = 100,
            SubjectUserId = "trainee-1",
            CurrentState = "completed",
            DataJson = dataJson,
            CreatedOn = DateTime.UtcNow,
            Transitions =
            [
                new ActivityTransition { TransitionKey = "complete", OccurredOn = DateTime.UtcNow }
            ]
        };

    private static ApplicationDbContext CreateDbContext()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    /// <summary>
    /// A curriculum item requiring order 4 — "Independent" on the five-rung ladder — and a trainee pinned
    /// to it. Both ladders exist so that either can be the one the assessment is bound to.
    /// </summary>
    private static async Task SeedAsync(ApplicationDbContext dbContext, int? pinCurriculumItemToScaleId)
    {
        dbContext.EntrustmentScales.AddRange(
            new EntrustmentScale { Id = FiveRungScaleId, Name = FiveRungScaleName },
            new EntrustmentScale { Id = SixRungScaleId, Name = SixRungScaleName });

        dbContext.Epas.Add(new Epa { Id = 5000, Code = "EPA-1", Title = "Take a history" });

        dbContext.CurriculumItems.Add(new CurriculumItem
        {
            Id = 4000,
            CurriculumId = 3000,
            EpaId = 5000,
            RequiredCount = 3,
            MinimumLevelOrder = 4,
            WindowMonths = 12,
            ScaleId = pinCurriculumItemToScaleId
        });

        dbContext.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1,
            UserId = "trainee-1",
            InstitutionId = 10,
            CurriculumId = 3000,
            ProgrammeStartDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-1),
            ExpectedCompletionDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(2),
            IsActive = true
        });

        await dbContext.SaveChangesAsync();
    }
}
