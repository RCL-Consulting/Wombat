using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Dashboards.Trainee;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Dashboards;

public sealed class TraineeStageOverrideTests
{
    [Fact]
    public async Task CurriculumProgress_UsesStageOverride_WhenTraineeIsInYearThree()
    {
        await using var db = CreateDb();
        SeedWithStageOverrides(db, programmeStartDaysAgo: 365 * 2 + 30);

        var handler = new GetTraineeDashboardSummaryQueryHandler(db);
        var principal = CreatePrincipal("trainee-1", ["Trainee"]);

        var result = await handler.Handle(
            new GetTraineeDashboardSummaryQuery(principal), CancellationToken.None);

        var progress = result.CurriculumProgress.Should().ContainSingle().Subject;
        progress.TraineeStage.Should().Be(3);
        progress.EffectiveMinimumLevelOrder.Should().Be(4);  // year 3 override
        progress.MinimumLevelReachedCount.Should().Be(1);
    }

    [Fact]
    public async Task CurriculumProgress_FallsBackToFlatMinimum_WhenStageNotOverridden()
    {
        await using var db = CreateDb();
        SeedWithStageOverrides(db, programmeStartDaysAgo: 365 * 5);  // year 6 - not in overrides

        var handler = new GetTraineeDashboardSummaryQueryHandler(db);
        var principal = CreatePrincipal("trainee-1", ["Trainee"]);

        var result = await handler.Handle(
            new GetTraineeDashboardSummaryQuery(principal), CancellationToken.None);

        var progress = result.CurriculumProgress.Should().ContainSingle().Subject;
        progress.TraineeStage.Should().Be(6);
        progress.EffectiveMinimumLevelOrder.Should().Be(5);  // flat MinimumLevelOrder
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(364, 1)]
    [InlineData(365, 2)]
    [InlineData(730, 3)]
    public void ComputeTraineeStage_ReturnsExpectedYear(int daysSinceStart, int expectedStage)
    {
        var start = new DateOnly(2026, 1, 1);
        var today = start.AddDays(daysSinceStart);

        GetTraineeDashboardSummaryQueryHandler.ComputeTraineeStage(start, today).Should().Be(expectedStage);
    }

    [Fact]
    public void ComputeTraineeStage_ReturnsNull_WhenStartInFuture()
    {
        var start = new DateOnly(2026, 6, 1);
        var today = new DateOnly(2026, 1, 1);

        GetTraineeDashboardSummaryQueryHandler.ComputeTraineeStage(start, today).Should().BeNull();
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static void SeedWithStageOverrides(ApplicationDbContext db, int programmeStartDaysAgo)
    {
        db.Institutions.Add(new Institution { Id = 1, Name = "Test Hospital" });
        db.Specialities.Add(new Speciality { Id = 1, InstitutionId = 1, Name = "Spec" });
        db.SubSpecialities.Add(new SubSpeciality { Id = 1, SpecialityId = 1, Name = "SubSpec" });
        db.Epas.Add(new Epa { Id = 7, SubSpecialityId = 1, Code = "EPA-07", Title = "EPA 7", Category = EpaCategory.Core });
        db.Curricula.Add(new Curriculum
        {
            Id = 1,
            SubSpecialityId = 1,
            Name = "Programme",
            Version = "1.0",
            EffectiveFrom = new DateOnly(2020, 1, 1),
            IsActive = true
        });
        db.CurriculumItems.Add(new CurriculumItem
        {
            Id = 1,
            CurriculumId = 1,
            EpaId = 7,
            RequiredCount = 6,
            MinimumLevelOrder = 5,
            WindowMonths = 36,
            MinimumLevelByStageJson = "{\"1\":2,\"2\":3,\"3\":4}"
        });
        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1,
            UserId = "trainee-1",
            CurriculumId = 1,
            ProgrammeStartDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-programmeStartDaysAgo),
            ExpectedCompletionDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(365),
            IsActive = true
        });
        db.CurriculumItemProgresses.Add(new CurriculumItemProgress
        {
            Id = 1,
            CurriculumItemId = 1,
            TraineeUserId = "trainee-1",
            CountsSoFar = 2,
            MinimumLevelReachedCount = 1,
            LastUpdated = DateTime.UtcNow
        });
        db.SaveChanges();
    }

    private static ClaimsPrincipal CreatePrincipal(string userId, IReadOnlyCollection<string>? roles = null)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
        foreach (var role in roles ?? [])
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }
}
