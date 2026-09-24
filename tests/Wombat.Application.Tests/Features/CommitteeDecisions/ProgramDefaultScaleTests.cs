using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Application.Features.Institutions.Commands.UpdateSubSpeciality;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.CommitteeDecisions;

/// <summary>
/// T076 / F-4D-1 — a sub-speciality may declare a default entrustment scale. When set, committee
/// STARs for its trainees are constrained to that scale: the picker filters to it and the stage
/// handler rejects a level from any other scale. Unset falls back to offering every scale.
/// </summary>
/// <remarks>
/// Since T167 the programme scale is the fallback for a curriculum item that is not pinned to a scale of its own; the
/// fixture's item is unpinned, so these are the T076 rules as they still stand. A pinned item's ladder wins
/// (<c>StarCurriculumTests</c>). The picker is <see cref="ListStarEpaOptionsForReviewQuery" />, which replaced
/// <c>GetProgramScaleIdForReviewQuery</c>: its one programme scale is now each EPA option's fallback.
/// </remarks>
public sealed class ProgramDefaultScaleTests
{
    private const int ScaleA = 1;   // not the programme scale
    private const int ScaleB = 2;   // the programme scale
    private const int LevelInA = 101;
    private const int LevelInB = 201;
    private const int SubSpecialityId = 10;
    private const int CurriculumId = 20;
    private const int EpaId = 30;
    private const int PanelId = 40;
    private const string TraineeUserId = "trainee-x";

    [Fact]
    public async Task Picker_OffersTheProgrammeScale_WhenConfigured()
    {
        await using var db = SeededDb(defaultScaleId: ScaleB);
        var reviewId = await AddInProgressReviewAsync(db);

        var result = await new ListStarEpaOptionsForReviewQueryHandler(db)
            .Handle(new ListStarEpaOptionsForReviewQuery(reviewId, TestPrincipals.Administrator()), CancellationToken.None);

        result.Should().ContainSingle().Which.ScaleId.Should().Be(ScaleB);
    }

    [Fact]
    public async Task Picker_OffersEveryScale_WhenUnset()
    {
        await using var db = SeededDb(defaultScaleId: null);
        var reviewId = await AddInProgressReviewAsync(db);

        var result = await new ListStarEpaOptionsForReviewQueryHandler(db)
            .Handle(new ListStarEpaOptionsForReviewQuery(reviewId, TestPrincipals.Administrator()), CancellationToken.None);

        result.Should().ContainSingle().Which.ScaleId.Should().BeNull();
    }

    [Fact]
    public async Task Stage_RejectsLevelFromOtherScale_WhenProgrammeScaleSet()
    {
        await using var db = SeededDb(defaultScaleId: ScaleB);
        var reviewId = await AddInProgressReviewAsync(db);

        var act = () => StageAsync(db, reviewId, LevelInA);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*programme's entrustment scale*");
    }

    [Fact]
    public async Task Stage_AllowsMatchingScaleLevel_WhenProgrammeScaleSet()
    {
        await using var db = SeededDb(defaultScaleId: ScaleB);
        var reviewId = await AddInProgressReviewAsync(db);

        var result = await StageAsync(db, reviewId, LevelInB);

        result.AuthorisedLevelId.Should().Be(LevelInB);
    }

    [Fact]
    public async Task Stage_AllowsAnyScaleLevel_WhenProgrammeScaleUnset()
    {
        await using var db = SeededDb(defaultScaleId: null);
        var reviewId = await AddInProgressReviewAsync(db);

        var result = await StageAsync(db, reviewId, LevelInA);

        result.AuthorisedLevelId.Should().Be(LevelInA);
    }

    [Fact]
    public async Task UpdateSubSpeciality_PersistsDefaultScale()
    {
        await using var db = SeededDb(defaultScaleId: null);

        var result = await new UpdateSubSpecialityCommandHandler(db).Handle(
            new UpdateSubSpecialityCommand(SubSpecialityId, SpecialityId: 1, Name: "Paeds", Description: null,
                IsActive: true, DefaultEntrustmentScaleId: ScaleB, Principal: TestPrincipals.Administrator()),
            CancellationToken.None);

        result.DefaultEntrustmentScaleId.Should().Be(ScaleB);
        (await db.Set<SubSpeciality>().FindAsync(SubSpecialityId))!.DefaultEntrustmentScaleId.Should().Be(ScaleB);
    }

    [Fact]
    public async Task UpdateSubSpeciality_RejectsUnknownScale()
    {
        await using var db = SeededDb(defaultScaleId: null);

        var act = () => new UpdateSubSpecialityCommandHandler(db).Handle(
            new UpdateSubSpecialityCommand(SubSpecialityId, SpecialityId: 1, Name: "Paeds", Description: null,
                IsActive: true, DefaultEntrustmentScaleId: 999, Principal: TestPrincipals.Administrator()),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*999*");
    }

    private static async Task<PendingEntrustmentDecisionDto> StageAsync(ApplicationDbContext db, int reviewId, int levelId)
        => await new StagePendingEntrustmentDecisionCommandHandler(db).Handle(
            new StagePendingEntrustmentDecisionCommand(
                ReviewId: reviewId,
                PendingId: null,
                EpaId: EpaId,
                AuthorisedLevelId: levelId,
                IssuedOn: new DateOnly(2027, 1, 8),
                ExpiresOn: null,
                Rationale: "Target met.",
                EvidenceItemIds: await db.Set<CommitteeEvidence>().Where(line => line.ReviewId == reviewId).Select(line => line.Id).ToArrayAsync(),
                Principal: TestPrincipals.Administrator()),
            CancellationToken.None);

    private static async Task<int> AddInProgressReviewAsync(ApplicationDbContext db)
    {
        // The Administrator stages below, because a panel's institution does not bind one (T182) and this fixture's
        // trainee has no institution. Since T165 the chair's actions have no Administrator bypass (D46), so the
        // Administrator chairs the panel.
        var panel = new DecisionPanel
        {
            Id = PanelId,
            Name = "Panel",
            Scope = DecisionPanelScope.Institution,
            InstitutionId = 1,
            CreatedOn = DateTime.UtcNow,
            Members =
            [
                new DecisionPanelMember { UserId = "admin-user", Role = DecisionPanelMemberRole.Chair },
                new DecisionPanelMember { UserId = "member-1", Role = DecisionPanelMemberRole.Member }
            ]
        };
        db.Set<DecisionPanel>().Add(panel);

        var review = new CommitteeReview
        {
            AcademicYear = 2026,
            Semester = 2,
            TraineeUserId = TraineeUserId,
            PanelId = PanelId,
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 12, 31),
            ScheduledOn = new DateOnly(2027, 1, 8)
        };
        // One frozen line, which a staged decision names as the evidence it rests on (D38, T131).
        review.Start(
            [new CommitteeEvidence { SourceType = CommitteeEvidenceSourceType.Activity, ActivityId = 1, SourceLabel = "Mini-CEX #1" }],
            "actor",
            DateTime.UtcNow);
        db.Set<CommitteeReview>().Add(review);
        await db.SaveChangesAsync();
        return review.Id;
    }

    private static ApplicationDbContext SeededDb(int? defaultScaleId)
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        db.Set<EntrustmentScale>().AddRange(
            new EntrustmentScale { Id = ScaleA, Name = "Scale A" },
            new EntrustmentScale { Id = ScaleB, Name = "Scale B" });
        db.Set<EntrustmentLevel>().AddRange(
            new EntrustmentLevel { Id = LevelInA, ScaleId = ScaleA, Order = 4, Label = "Independent" },
            new EntrustmentLevel { Id = LevelInB, ScaleId = ScaleB, Order = 4, Label = "Unsupervised" });

        db.Set<Institution>().Add(new Institution { Id = 1, Name = "KGK", ShortCode = "KGK", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.Set<Speciality>().Add(new Speciality { Id = 1, CollegeId = 1, Name = "Paediatrics", IsActive = true });
        db.Set<SubSpeciality>().Add(new SubSpeciality { Id = SubSpecialityId, SpecialityId = 1, Name = "Paeds", IsActive = true, DefaultEntrustmentScaleId = defaultScaleId });
        db.Set<Curriculum>().Add(new Curriculum { Id = CurriculumId, SubSpecialityId = SubSpecialityId, Name = "Paed", Version = "2026.1" });
        // At the panel's institution: the review's panel decides the trainee's EPAs only if it covers them (T131 slice 4).
        db.Set<TraineeProfile>().Add(new TraineeProfile { UserId = TraineeUserId, InstitutionId = 1, CurriculumId = CurriculumId, ProgrammeStartDate = new DateOnly(2023, 1, 15), ExpectedCompletionDate = new DateOnly(2029, 12, 31) });
        db.Set<Epa>().Add(new Epa { Id = EpaId, SubSpecialityId = SubSpecialityId, Code = "PAED-001", Title = "Acute admission" });
        // Unpinned (T109), so the programme's scale is the ladder a STAR's level must be on (T167).
        db.Set<CurriculumItem>().Add(new CurriculumItem { Id = 50, CurriculumId = CurriculumId, EpaId = EpaId, RequiredCount = 1, MinimumLevelOrder = 3 });
        db.SaveChanges();
        return db;
    }
}
