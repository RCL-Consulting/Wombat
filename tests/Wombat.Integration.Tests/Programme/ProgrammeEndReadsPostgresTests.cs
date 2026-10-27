using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Trainees;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Integration.Tests.TestSupport;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.Programme;

/// <summary>
/// T379 (flow 13, lane P) on a real PostgreSQL server: the reads the unit suites run on EF InMemory must translate here.
/// Completed and closed is paged in SQL with the latest end first (the end day a COALESCE of the two end columns); the
/// profile's read checks the export's gate and the last ratified decision; the end's forecast reads the credited dates.
/// </summary>
/// <remarks>A migrated schema of its own (<c>it_&lt;guid&gt;</c>), seeded as startup seeds it; dropped on dispose.</remarks>
public sealed class ProgrammeEndReadsPostgresTests : IAsyncLifetime
{
    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task TheClosedList_TheProfile_AndTheForecast_TranslateOnPostgres()
    {
        var schema = await SeededSchemaAsync();

        int institutionId, completedId, withdrawnId;
        await using (var db = NewContext(schema))
        {
            institutionId = await db.Institutions.Where(entity => entity.ShortCode == "DEMO").Select(entity => entity.Id).SingleAsync();
            var curriculumId = await db.Curricula.OrderBy(entity => entity.Id).Select(entity => entity.Id).FirstAsync();

            var completed = Profile("t379-molefe", institutionId, curriculumId);
            var withdrawn = Profile("t379-duplessis", institutionId, curriculumId);
            var running = Profile("t379-nkosi", institutionId, curriculumId);
            db.TraineeProfiles.AddRange(completed, withdrawn, running);
            completed.Complete(new DateOnly(2026, 10, 26), today: new DateOnly(2026, 10, 27));
            withdrawn.Deactivate(new DateOnly(2026, 10, 24), today: new DateOnly(2026, 10, 27));
            await db.SaveChangesAsync();
            (completedId, withdrawnId) = (completed.Id, withdrawn.Id);
        }

        var users = new FakeUserDirectory()
            .With(new UserIdentityDetails("t379-molefe", "molefe@test.local", "Lerato", "Molefe", institutionId, [], [], []))
            .With(new UserIdentityDetails("t379-duplessis", "duplessis@test.local", "Pieter", "du Plessis", institutionId, [], [], []))
            .With(new UserIdentityDetails("t379-nkosi", "nkosi@test.local", "Thandi", "Nkosi", institutionId, [], [], [WombatRoles.Trainee]));
        var admin = TestPrincipalsFor(institutionId);

        await using (var db = NewContext(schema))
        {
            var page = await new ListEndedProfilesQueryHandler(db, users).Handle(
                new ListEndedProfilesQuery(1, 20, admin), CancellationToken.None);

            page.Rows.Where(row => row.UserId.StartsWith("t379-", StringComparison.Ordinal))
                .Select(row => (row.ProfileId, row.Name, row.Ended!.Completed))
                .Should().Equal((completedId, "Lerato Molefe", true), (withdrawnId, "Pieter du Plessis", false));

            var profile = await new GetTraineeProfileByIdQueryHandler(db, users).Handle(
                new GetTraineeProfileByIdQuery(withdrawnId, admin), CancellationToken.None);
            profile!.Ended!.EndedOn.Should().Be(new DateOnly(2026, 10, 24));
            profile.Account!.AnyStarIssued.Should().BeFalse();

            var forecast = await new GetTakeBackForecastQueryHandler(db).Handle(
                new GetTakeBackForecastQuery(withdrawnId, new DateOnly(2026, 10, 24), admin), CancellationToken.None);
            forecast.Should().Be(new TakeBackForecastDto(0, null));
        }
    }

    private static System.Security.Claims.ClaimsPrincipal TestPrincipalsFor(int institutionId)
        => new(new System.Security.Claims.ClaimsIdentity(
        [
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "t379-admin"),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, WombatRoles.InstitutionalAdmin),
            new System.Security.Claims.Claim(Wombat.Application.Common.Security.WombatClaimTypes.InstitutionId, institutionId.ToString(System.Globalization.CultureInfo.InvariantCulture))
        ], "test"));

    private static TraineeProfile Profile(string userId, int institutionId, int curriculumId) => new()
    {
        UserId = userId,
        InstitutionId = institutionId,
        CurriculumId = curriculumId,
        ProgrammeStartDate = new DateOnly(2023, 1, 15),
        ExpectedCompletionDate = new DateOnly(2027, 1, 14),
        IsActive = true
    };

    private async Task<string> SeededSchemaAsync()
    {
        var schema = await _schemas.CreateAsync();

        await using (var db = NewContext(schema))
        {
            await db.Database.MigrateAsync();
        }

        await using (var db = NewContext(schema))
        {
            await new DataSeeder(db).SeedAsync();
        }

        return schema;
    }

    private static ApplicationDbContext NewContext(string schema)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(TestDatabase.SchemaConnectionString(schema))
            .Options);
}
