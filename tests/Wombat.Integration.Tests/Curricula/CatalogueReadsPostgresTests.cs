using System.Globalization;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Wombat.Application.Common;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Epas;
using Wombat.Application.Features.Epas.Commands.CreateEntrustmentScale;
using Wombat.Application.Features.Epas.Commands.UpdateEntrustmentScale;
using Wombat.Application.Features.Institutions.Queries.GetCollegeTree;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;
using Wombat.Integration.Tests.TestSupport;

namespace Wombat.Integration.Tests.Curricula;

/// <summary>
/// T394 fix pass (code S-2): flow 16's new reads, run on PostgreSQL, since the in-memory provider is no evidence that a
/// query translates to SQL (<c>CurriculumAdminScopePostgresTests</c>). The College's tree (nested ordered projections and the
/// default ladder's name), the reach (<see cref="CatalogueReach" />: a composite-key group, an own EPA's owner-only count) and
/// "in use" (<c>EntrustmentScaleUses</c>: every one of the seven reads, through the scale's page and Q16's
/// refusal), each on the seeded CPSA catalogue; and a scale's name taken by a racing save (code N-7).
/// </summary>
public sealed class CatalogueReadsPostgresTests : IAsyncLifetime
{
    private const string CurriculumName = "Paediatric EPA Curriculum";
    private const string CpsaLadder = "CPSA Paediatric Entrustment Scale v11.1";

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task TheCollegesTree_IsReadInSql_WithEachSubSpecialitysDefaultLadderByName()
    {
        try
        {
            var schema = await SeededSchemaAsync();
            int collegeId;
            await using (var db = NewContext(schema))
            {
                collegeId = await db.Colleges.Where(college => college.ShortCode == "CPSA").Select(college => college.Id).SingleAsync();
            }

            await using (var db = NewContext(schema))
            {
                var tree = await new GetCollegeTreeQueryHandler(db).Handle(
                    new GetCollegeTreeQuery(collegeId, CollegeAdmin(collegeId)), CancellationToken.None);

                tree.Should().NotBeNull();
                tree!.ShortCode.Should().Be("CPSA");
                var paediatrics = tree.Specialities.Should().ContainSingle(speciality => speciality.Name == "Paediatrics").Subject;
                var sub = paediatrics.SubSpecialities.Should().ContainSingle(subSpeciality => subSpeciality.Name == "Paediatrics").Subject;
                sub.DefaultEntrustmentScaleId.Should().NotBeNull();
                sub.DefaultEntrustmentScaleName.Should().Be(CpsaLadder);
                tree.Specialities.Select(speciality => speciality.Name).Should().BeInAscendingOrder(StringComparer.Ordinal);

                (await new GetCollegeTreeQueryHandler(db).Handle(
                        new GetCollegeTreeQuery(collegeId, CollegeAdmin(collegeId + 1000)), CancellationToken.None))
                    .Should().BeNull("another College's tree is unavailable");
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task TheReach_IsReadInSql_AdoptersAndSupersedersCountedByActiveProfiles_AnOwnEpaByItsOwnerAlone()
    {
        try
        {
            var schema = await SeededSchemaAsync();
            var world = await SeedReachAsync(schema);

            await using var db = NewContext(schema);
            var reaches = await CatalogueReach.ForCurriculaAsync(db, [world.CurriculumId, world.UnadoptedId], CancellationToken.None);

            var reach = reaches[world.CurriculumId];
            reach.AdoptedBy.Should().Equal("T394 Adopter");
            reach.RegistrarCount.Should().Be(2, "two active profiles at the adopter; its graduate is not counted");
            reach.SupersededBy.Should().Equal("T394 Superseder");
            reach.SupersededRegistrarCount.Should().Be(1);
            reaches[world.UnadoptedId].Reaches.Should().BeFalse("no institution has adopted it");

            var national = await CatalogueReach.ForEpaAsync(db, world.Paed001, null, CancellationToken.None);
            national.CurriculumCount.Should().Be(1);
            national.Institutions.Should().Equal("T394 Adopter", "T394 Superseder");
            national.RegistrarCount.Should().Be(3);

            var own = await CatalogueReach.ForEpaAsync(db, world.OwnEpa, world.Adopter, CancellationToken.None);
            own.CurriculumCount.Should().Be(1);
            own.Institutions.Should().Equal("T394 Adopter");
            own.RegistrarCount.Should().Be(2, "an own item reaches its owner's registrars alone");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task InUse_IsReadInSql_ForAUsedAndAnUnusedScale_AndQ16RefusesAReorderOfTheUsedOne_ChangingNothing()
    {
        try
        {
            var schema = await SeededSchemaAsync();
            await SeedReachAsync(schema); // two ended programmes, so the ended records' read runs on real rows
            int cpsaId;
            EntrustmentScaleDto unused;
            await using (var db = NewContext(schema))
            {
                cpsaId = await db.EntrustmentScales.Where(scale => scale.Name == CpsaLadder).Select(scale => scale.Id).SingleAsync();
                unused = await new CreateEntrustmentScaleCommandHandler(db).Handle(
                    new CreateEntrustmentScaleCommand("T394 Unused Scale", null,
                        [new EntrustmentLevelInput(1, "Low", null), new EntrustmentLevelInput(2, "High", null)], Administrator()),
                    CancellationToken.None);
            }

            await using (var db = NewContext(schema))
            {
                var used = await new GetEntrustmentScaleByIdQueryHandler(db).Handle(new GetEntrustmentScaleByIdQuery(cpsaId), CancellationToken.None);
                used!.InUse.Should().BeTrue("the v11.1 items are pinned to the CPSA ladder");

                // Nothing uses it, so every one of the seven reads runs, down to the decisions' level navigation.
                var free = await new GetEntrustmentScaleByIdQueryHandler(db).Handle(new GetEntrustmentScaleByIdQuery(unused.Id), CancellationToken.None);
                free!.InUse.Should().BeFalse();
            }

            await using (var db = NewContext(schema))
            {
                var stored = await new GetEntrustmentScaleByIdQueryHandler(db).Handle(new GetEntrustmentScaleByIdQuery(cpsaId), CancellationToken.None);
                var swapped = stored!.Levels
                    .Select(level => new EntrustmentLevelUpdate(level.Id, level.Order switch { 1 => 2, 2 => 1, _ => level.Order }, level.Label, level.Description))
                    .ToList();

                var reorder = () => new UpdateEntrustmentScaleCommandHandler(db).Handle(
                    new UpdateEntrustmentScaleCommand(cpsaId, stored.Name, stored.Description, swapped, Administrator()), CancellationToken.None);

                (await reorder.Should().ThrowAsync<EntrustmentScaleUpdateRefusedException>()).Which.Reason
                    .Should().Be(EntrustmentScaleUpdateRefusal.InUse);
            }

            await using (var db = NewContext(schema))
            {
                (await db.Set<EntrustmentLevel>().Where(level => level.ScaleId == cpsaId).OrderBy(level => level.Order)
                        .Select(level => level.Label).Take(2).ToListAsync())
                    .Should().Equal("1", "2");
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task ASwapOfTwoRungs_OnAScaleNothingUses_IsSaved_OnPostgres()
    {
        // T397: a swap inside the unique (scale, order) index; EF Core alone finds no order for it and gives up.
        try
        {
            var schema = await SeededSchemaAsync();
            EntrustmentScaleDto created;
            await using (var db = NewContext(schema))
            {
                created = await new CreateEntrustmentScaleCommandHandler(db).Handle(
                    new CreateEntrustmentScaleCommand("T397 Swap Scale", null,
                        [new EntrustmentLevelInput(1, "Low", null), new EntrustmentLevelInput(2, "Mid", null), new EntrustmentLevelInput(3, "High", null)],
                        Administrator()),
                    CancellationToken.None);
            }

            await using (var db = NewContext(schema))
            {
                // Low and High change places (1 and 3, a cycle in the index), Mid stays, and a new rung 4 is added.
                var low = created.Levels.Single(level => level.Label == "Low");
                var mid = created.Levels.Single(level => level.Label == "Mid");
                var high = created.Levels.Single(level => level.Label == "High");
                var result = await new UpdateEntrustmentScaleCommandHandler(db).Handle(
                    new UpdateEntrustmentScaleCommand(created.Id, created.Name, null,
                        [new EntrustmentLevelUpdate(high.Id, 1, "High", null), new EntrustmentLevelUpdate(mid.Id, 2, "Mid", null),
                         new EntrustmentLevelUpdate(low.Id, 3, "Low", null), new EntrustmentLevelUpdate(null, 4, "Top", null)],
                        Administrator()),
                    CancellationToken.None);
                result.Scale.Levels.Select(level => level.Label).Should().Equal("High", "Mid", "Low", "Top");
            }

            await using (var check = NewContext(schema))
            {
                (await check.Set<EntrustmentLevel>().Where(level => level.ScaleId == created.Id).OrderBy(level => level.Order)
                        .Select(level => level.Order + ":" + level.Label).ToListAsync())
                    .Should().Equal("1:High", "2:Mid", "3:Low", "4:Top");
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task ASwapWhoseNewNameARacingSaveTakes_IsRefusedUnderName_AndUndoesTheParkedRungs()
    {
        // T397: the swap's first write parks the moved rungs; the second write is refused (code N-7), and the one
        // transaction takes the first back, so no rung is left at a negative order.
        try
        {
            var schema = await SeededSchemaAsync();
            const string Taken = "T397 Raced Name";
            EntrustmentScaleDto created;
            await using (var db = NewContext(schema))
            {
                created = await new CreateEntrustmentScaleCommandHandler(db).Handle(
                    new CreateEntrustmentScaleCommand("T397 Rename Scale", null,
                        [new EntrustmentLevelInput(1, "Low", null), new EntrustmentLevelInput(2, "High", null)], Administrator()),
                    CancellationToken.None);
            }

            await using (var db = NewContext(schema, beforeFirstSave: async () =>
            {
                // Another request, so outside this command's transaction.
                using var outside = new System.Transactions.TransactionScope(
                    System.Transactions.TransactionScopeOption.Suppress, System.Transactions.TransactionScopeAsyncFlowOption.Enabled);
                await using var other = NewContext(schema);
                await new CreateEntrustmentScaleCommandHandler(other).Handle(
                    new CreateEntrustmentScaleCommand(Taken, null,
                        [new EntrustmentLevelInput(1, "A", null), new EntrustmentLevelInput(2, "B", null)], Administrator()),
                    CancellationToken.None);
            }))
            {
                var low = created.Levels.Single(level => level.Label == "Low");
                var high = created.Levels.Single(level => level.Label == "High");
                var update = () => new UpdateEntrustmentScaleCommandHandler(db).Handle(
                    new UpdateEntrustmentScaleCommand(created.Id, Taken, null,
                        [new EntrustmentLevelUpdate(high.Id, 1, "High", null), new EntrustmentLevelUpdate(low.Id, 2, "Low", null)],
                        Administrator()),
                    CancellationToken.None);

                (await update.Should().ThrowAsync<FieldRefusalException>()).Which.Field
                    .Should().Be(nameof(UpdateEntrustmentScaleCommand.Name));
            }

            await using (var check = NewContext(schema))
            {
                (await check.Set<EntrustmentLevel>().Where(level => level.ScaleId == created.Id).OrderBy(level => level.Order)
                        .Select(level => level.Order + ":" + level.Label).ToListAsync())
                    .Should().Equal("1:Low", "2:High");
                (await check.EntrustmentScales.Where(scale => scale.Id == created.Id).Select(scale => scale.Name).SingleAsync())
                    .Should().Be("T397 Rename Scale");
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task AScalesNameTakenByARacingSave_IsRefusedUnderName_InTheCheckWords()
    {
        // Code N-7: the racing save takes the name after the command's check and before its save reaches the database.
        try
        {
            var schema = await SeededSchemaAsync();
            const string Name = "T394 Raced Scale";

            await using var db = NewContext(schema, beforeFirstSave: async () =>
            {
                await using var other = NewContext(schema);
                await new CreateEntrustmentScaleCommandHandler(other).Handle(
                    new CreateEntrustmentScaleCommand(Name, null,
                        [new EntrustmentLevelInput(1, "A", null), new EntrustmentLevelInput(2, "B", null)], Administrator()),
                    CancellationToken.None);
            });
            var create = () => new CreateEntrustmentScaleCommandHandler(db).Handle(
                new CreateEntrustmentScaleCommand(Name, null,
                    [new EntrustmentLevelInput(1, "Low", null), new EntrustmentLevelInput(2, "High", null)], Administrator()),
                CancellationToken.None);

            var refused = (await create.Should().ThrowAsync<FieldRefusalException>()).Which;
            refused.Field.Should().Be(nameof(CreateEntrustmentScaleCommand.Name));
            refused.Message.Should().Be(EntrustmentScaleBindings.NameTaken(Name));
            refused.InnerException.Should().BeAssignableTo<DbUpdateException>("the pipeline discards the refused save (T201)");

            await using var check = NewContext(schema);
            (await check.EntrustmentScales.CountAsync(scale => scale.Name == Name)).Should().Be(1);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    private sealed record ReachWorld(int CurriculumId, int UnadoptedId, int Adopter, int Paed001, int OwnEpa);

    /// <summary>
    /// On 11.1: an adopter with two active registrars and a graduate, and an own item of its own EPA; a superseder (its
    /// adoption inactive) with one registrar still on it; an institution that superseded it with none left, not named. And
    /// an unadopted version beside it.
    /// </summary>
    private async Task<ReachWorld> SeedReachAsync(string schema)
    {
        await using var db = NewContext(schema);
        var curriculum = await db.Curricula
            .Where(entity => entity.Name == CurriculumName && entity.Version == "11.1")
            .Select(entity => new { entity.Id, entity.SubSpecialityId })
            .SingleAsync();
        var paed001 = await db.Epas.Where(epa => epa.Code == "PAED-001" && epa.OwningInstitutionId == null).Select(epa => epa.Id).SingleAsync();

        var adopter = new Institution { Name = "T394 Adopter", ShortCode = "T394-A" };
        var superseder = new Institution { Name = "T394 Superseder", ShortCode = "T394-S" };
        var gone = new Institution { Name = "T394 Gone", ShortCode = "T394-G" };
        db.Institutions.AddRange(adopter, superseder, gone);
        var unadopted = new Curriculum
        {
            SubSpecialityId = curriculum.SubSpecialityId, Name = CurriculumName, Version = "T394", EffectiveFrom = new DateOnly(2027, 1, 1),
            IsActive = false
        };
        db.Curricula.Add(unadopted);
        await db.SaveChangesAsync();

        var ownEpa = new Epa
        {
            Code = "T394-001", Title = "The adopter's own EPA", SubSpecialityId = curriculum.SubSpecialityId, OwningInstitutionId = adopter.Id
        };
        db.Epas.Add(ownEpa);
        db.InstitutionCurriculumAdoptions.AddRange(
            Adoption(adopter.Id, curriculum.Id, curriculum.SubSpecialityId, active: true),
            Adoption(superseder.Id, curriculum.Id, curriculum.SubSpecialityId, active: false),
            Adoption(gone.Id, curriculum.Id, curriculum.SubSpecialityId, active: false));
        db.TraineeProfiles.AddRange(
            Profile("t394-a1", adopter.Id, curriculum.Id, active: true),
            Profile("t394-a2", adopter.Id, curriculum.Id, active: true),
            Profile("t394-a3", adopter.Id, curriculum.Id, active: false),
            Profile("t394-s1", superseder.Id, curriculum.Id, active: true),
            Profile("t394-g1", gone.Id, curriculum.Id, active: false));
        // Two ended programmes with the same (empty) list: "in use" reads each distinct list once, in SQL (code N-5).
        foreach (var ended in db.ChangeTracker.Entries<TraineeProfile>().Where(entry => !entry.Entity.IsActive))
        {
            ended.Property(profile => profile.EndedCurriculumJson).CurrentValue = "[]";
        }

        await db.SaveChangesAsync();

        db.CurriculumItems.Add(new CurriculumItem
        {
            CurriculumId = curriculum.Id, EpaId = ownEpa.Id, OwningInstitutionId = adopter.Id, RequiredCount = 1,
            QuotaPeriod = QuotaPeriod.AcademicYear, MinimumLevelOrder = 3, WindowMonths = 12
        });
        await db.SaveChangesAsync();

        return new ReachWorld(curriculum.Id, unadopted.Id, adopter.Id, paed001, ownEpa.Id);
    }

    private static InstitutionCurriculumAdoption Adoption(int institutionId, int curriculumId, int subSpecialityId, bool active)
        => new()
        {
            InstitutionId = institutionId, CurriculumId = curriculumId, SubSpecialityId = subSpecialityId,
            AdoptedOn = new DateOnly(2026, 1, 1), IsActive = active
        };

    private static TraineeProfile Profile(string userId, int institutionId, int curriculumId, bool active)
        => new()
        {
            UserId = userId, InstitutionId = institutionId, CurriculumId = curriculumId,
            ProgrammeStartDate = new DateOnly(2024, 1, 15), ExpectedCompletionDate = new DateOnly(2028, 1, 14), IsActive = active
        };

    private static ClaimsPrincipal Administrator()
        => Principal(new Claim(ClaimTypes.Role, WombatRoles.Administrator));

    private static ClaimsPrincipal CollegeAdmin(int collegeId)
        => Principal(
            new Claim(ClaimTypes.Role, WombatRoles.CollegeAdmin),
            new Claim(WombatClaimTypes.CollegeId, collegeId.ToString(CultureInfo.InvariantCulture)));

    private static ClaimsPrincipal Principal(params Claim[] claims)
        => new(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "t394-admin"), .. claims],
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

    private static ApplicationDbContext NewContext(string schema, Func<Task>? beforeFirstSave = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(TestDatabase.SchemaConnectionString(schema));
        if (beforeFirstSave is not null)
        {
            options.AddInterceptors(new BeforeFirstSave(beforeFirstSave));
        }

        return new ApplicationDbContext(options.Options);
    }

    /// <summary>Runs the racing write once, after the command's checks and just before its first save reaches the database.</summary>
    private sealed class BeforeFirstSave(Func<Task> action) : SaveChangesInterceptor
    {
        private Func<Task>? _action = action;

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (Interlocked.Exchange(ref _action, null) is { } run)
            {
                await run();
            }

            return result;
        }
    }
}
