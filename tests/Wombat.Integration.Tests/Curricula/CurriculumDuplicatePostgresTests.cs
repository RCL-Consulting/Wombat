using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common;
using Wombat.Application.Features.Curricula;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Integration.Tests.TestSupport;

namespace Wombat.Integration.Tests.Curricula;

/// <summary>
/// T394 (flow 16, T272; R3-D03, D09b): the curriculum's create, update and clone call a save a duplicate only when the unique
/// index (sub-speciality, name, version) refused it, and say so under Version (the clone's New version), in the built words.
/// Only PostgreSQL enforces the index, so these run here, on the seeded CPSA catalogue (Paediatric EPA Curriculum 11.1).
/// </summary>
public sealed class CurriculumDuplicatePostgresTests : IAsyncLifetime
{
    private const string Name = "Paediatric EPA Curriculum";
    private const string Duplicate = "A curriculum with the same name and version already exists for this sub-speciality.";

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task ACloneCreateOrUpdate_IntoATakenVersion_IsRefusedUnderVersion_AndNothingIsWritten()
    {
        try
        {
            var schema = await SeededSchemaAsync();
            int sourceId;
            int subSpecialityId;
            int count;
            await using (var db = NewContext(schema))
            {
                var source = await db.Set<Curriculum>().SingleAsync(entity => entity.Name == Name && entity.Version == "11.1");
                sourceId = source.Id;
                subSpecialityId = source.SubSpecialityId;
                count = await db.Set<Curriculum>().CountAsync();
            }

            await using (var db = NewContext(schema))
            {
                var clone = () => new CloneCurriculumAsNewVersionCommandHandler(db).Handle(
                    new CloneCurriculumAsNewVersionCommand(sourceId, "11.1", new DateOnly(2026, 11, 2), null, Administrator()),
                    CancellationToken.None);
                var refused = (await clone.Should().ThrowAsync<FieldRefusalException>()).Which;
                refused.Field.Should().Be(nameof(CloneCurriculumAsNewVersionCommand.Version));
                refused.Message.Should().Be(Duplicate);
            }

            await using (var db = NewContext(schema))
            {
                var create = () => new CreateCurriculumCommandHandler(db).Handle(
                    new CreateCurriculumCommand(subSpecialityId, Name, "11.1", new DateOnly(2026, 11, 2), null, Administrator()),
                    CancellationToken.None);
                var refused = (await create.Should().ThrowAsync<FieldRefusalException>()).Which;
                refused.Field.Should().Be(nameof(CreateCurriculumCommand.Version));
                refused.Message.Should().Be(Duplicate);
            }

            int secondId;
            await using (var db = NewContext(schema))
            {
                secondId = (await new CreateCurriculumCommandHandler(db).Handle(
                    new CreateCurriculumCommand(subSpecialityId, Name, "11.9", new DateOnly(2026, 11, 2), null, Administrator()),
                    CancellationToken.None)).Id;
            }

            await using (var db = NewContext(schema))
            {
                var update = () => new UpdateCurriculumCommandHandler(db).Handle(
                    new UpdateCurriculumCommand(secondId, subSpecialityId, Name, "11.1", new DateOnly(2026, 11, 2), null, false, Administrator()),
                    CancellationToken.None);
                var refused = (await update.Should().ThrowAsync<FieldRefusalException>()).Which;
                refused.Field.Should().Be(nameof(UpdateCurriculumCommand.Version));
                refused.Message.Should().Be(Duplicate);
            }

            await using (var db = NewContext(schema))
            {
                (await db.Set<Curriculum>().CountAsync()).Should().Be(count + 1, "only the 11.9 create went through");
                (await db.Set<Curriculum>().SingleAsync(entity => entity.Id == secondId)).Version.Should().Be("11.9");
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    private static ClaimsPrincipal Administrator()
        => new(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "t394-admin"), new Claim(ClaimTypes.Role, WombatRoles.Administrator)],
            "IntegrationTest",
            ClaimTypes.Name,
            ClaimTypes.Role));

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

        await using (var db = NewContext(schema))
        {
            await new PaediatricCatalogueSeeder(db).SeedAsync();
        }

        return schema;
    }

    private static ApplicationDbContext NewContext(string schema)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(TestDatabase.SchemaConnectionString(schema)).Options);
}
