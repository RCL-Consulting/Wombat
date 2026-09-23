using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Curricula;

/// <summary>
/// T122 — a curriculum item's tool list (Annexure A's fourth cell): which instruments may credit its EPA. Every
/// admin path that writes, reads or re-saves an item must carry the list canonically, refuse what the vocabulary
/// does not hold, and respect who owns the item.
/// </summary>
/// <remarks>
/// <para>
/// The failure this guards against is silent in the permissive direction. Null means "any instrument" (D21), so a
/// list that is dropped on an unchanged re-save, or an empty list stored as null by accident, does not fail: it
/// quietly lets a Mini-CEX credit PAED-005, which the College says only CBD, DOPS and MSF may. The opposite
/// failure is louder but just as wrong: an item listing a key nothing can match would refuse every recognised
/// instrument.
/// </para>
/// <para>
/// <c>PermittedToolsJson</c> is <c>jsonb</c>, so Postgres re-renders what is written and nothing may compare it as
/// a raw string, InMemory tests included. These tests read it back through
/// <see cref="CurriculumItem.ParsePermittedTools" />, or deserialise it structurally when the point is what order
/// the writer stored it in.
/// </para>
/// </remarks>
public sealed class CurriculumItemPermittedToolsTests
{
    private const int CollegeId = 1;
    private const int CurriculumId = 3000;
    private const int NationalItemId = 7000;
    private const int LocalItemId = 7001;
    private const int NationalEpaId = 5000;
    private const int LocalEpaId = 5001;
    private const int SpareEpaId = 5002;
    private const int InstitutionId = 40;

    private static readonly string[] Paed005Tools = ["cbd", "dops", "msf"];

    [Fact]
    public async Task AddCurriculumItem_StoresAnUnsortedDuplicatedMixedCaseListInCanonicalForm()
    {
        var databaseName = Guid.NewGuid().ToString();
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);

            var result = await new AddCurriculumItemCommandHandler(dbContext).Handle(
                AddCommand(SpareEpaId, ["MSF", " dops ", "cbd", "msf", "Cbd"], TestPrincipals.Administrator()),
                CancellationToken.None);

            result.Items.Single(item => item.EpaId == SpareEpaId).PermittedToolKeys.Should().Equal(Paed005Tools);
        }

        // A fresh context: the DTO is built from the tracked entity and would echo the value even if nothing was
        // stored.
        await using var readContext = CreateDbContext(databaseName);
        var stored = await readContext.Set<CurriculumItem>().SingleAsync(item => item.EpaId == SpareEpaId);
        CurriculumItem.ParsePermittedTools(stored.PermittedToolsJson).Should().Equal(Paed005Tools);
        StoredArray(stored.PermittedToolsJson).Should().Equal(
            Paed005Tools,
            "the writer stores the canonical form (normalised, distinct, sorted), not whatever order the form sent");
    }

    [Fact]
    public async Task UpdateCurriculumItem_StoresAnUnsortedDuplicatedMixedCaseListInCanonicalForm()
    {
        var databaseName = Guid.NewGuid().ToString();
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);

            await new UpdateCurriculumItemCommandHandler(dbContext).Handle(
                UpdateCommand(LocalItemId, LocalEpaId, ["Mini_Cex", "direct_observation", "MINI_CEX", " msf"], TestPrincipals.Administrator()),
                CancellationToken.None);
        }

        await using var readContext = CreateDbContext(databaseName);
        var stored = await readContext.Set<CurriculumItem>().SingleAsync(item => item.Id == LocalItemId);
        string[] expected = ["direct_observation", "mini_cex", "msf"];
        CurriculumItem.ParsePermittedTools(stored.PermittedToolsJson).Should().Equal(expected);
        StoredArray(stored.PermittedToolsJson).Should().Equal(expected);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AddCurriculumItem_WithAnEmptyOrNullList_StoresNull(bool sendEmptyList)
    {
        // An empty list would read as "no instrument may credit this" and make the EPA unfileable. It is stored as
        // null, the same "any instrument" an omitted list means.
        var databaseName = Guid.NewGuid().ToString();
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);

            await new AddCurriculumItemCommandHandler(dbContext).Handle(
                AddCommand(SpareEpaId, sendEmptyList ? [] : null, TestPrincipals.Administrator()),
                CancellationToken.None);
        }

        await using var readContext = CreateDbContext(databaseName);
        var stored = await readContext.Set<CurriculumItem>().SingleAsync(item => item.EpaId == SpareEpaId);
        stored.PermittedToolsJson.Should().BeNull();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdateCurriculumItem_WithAnEmptyOrNullList_WithdrawsTheRestrictionAsNull(bool sendEmptyList)
    {
        // The national item starts restricted to CBD, DOPS and MSF, so this is a real withdrawal, not a no-op.
        var databaseName = Guid.NewGuid().ToString();
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);

            await new UpdateCurriculumItemCommandHandler(dbContext).Handle(
                UpdateCommand(NationalItemId, NationalEpaId, sendEmptyList ? [] : null, TestPrincipals.Administrator()),
                CancellationToken.None);
        }

        await using var readContext = CreateDbContext(databaseName);
        var stored = await readContext.Set<CurriculumItem>().SingleAsync(item => item.Id == NationalItemId);
        stored.PermittedToolsJson.Should().BeNull("an empty list is never stored; null is the one form of 'unrestricted'");
    }

    [Fact]
    public async Task UpdateCurriculumItem_WithAnUnknownKey_IsRefusedNamingIt_BeforeAnythingIsWritten()
    {
        // The target change rides along so a refusal that fired after the assignments would leave the item
        // Modified, and the audit pipeline's catch would commit it under a failed command.
        var databaseName = Guid.NewGuid().ToString();
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);
        }

        await using (var dbContext = CreateDbContext(databaseName))
        {
            var act = () => new UpdateCurriculumItemCommandHandler(dbContext).Handle(
                UpdateCommand(NationalItemId, NationalEpaId, ["cbd", " OSCE "], TestPrincipals.Administrator(), requiredCount: 9),
                CancellationToken.None);

            (await act.Should().ThrowAsync<InvalidOperationException>())
                .WithMessage("*'osce'*")
                .Which.Message.Should().NotContain("'cbd'", "only the key the vocabulary lacks is named");

            var tracked = dbContext.ChangeTracker.Entries<CurriculumItem>().Single(entry => entry.Entity.Id == NationalItemId);
            tracked.State.Should().Be(EntityState.Unchanged, "guard: the handler loaded the item, so a mutation would show");
            tracked.Entity.RequiredCount.Should().Be(3);
            CurriculumItem.ParsePermittedTools(tracked.Entity.PermittedToolsJson).Should().Equal(Paed005Tools);

            await AssertNothingForTheAuditSaveToCommitAsync(dbContext);
        }

        await using var readContext = CreateDbContext(databaseName);
        var stored = await readContext.Set<CurriculumItem>().SingleAsync(item => item.Id == NationalItemId);
        stored.RequiredCount.Should().Be(3);
        CurriculumItem.ParsePermittedTools(stored.PermittedToolsJson).Should().Equal(Paed005Tools);
    }

    [Fact]
    public async Task AddCurriculumItem_WithUnknownKeys_IsRefusedNamingEach_WithNothingAdded()
    {
        var databaseName = Guid.NewGuid().ToString();
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);
        }

        await using (var dbContext = CreateDbContext(databaseName))
        {
            var act = () => new AddCurriculumItemCommandHandler(dbContext).Handle(
                AddCommand(SpareEpaId, ["viva", "msf", "OSCE"], TestPrincipals.Administrator()),
                CancellationToken.None);

            (await act.Should().ThrowAsync<InvalidOperationException>())
                .WithMessage("*'osce'*")
                .Which.Message.Should().Contain("'viva'", "every unknown key is named, not just the first");

            dbContext.ChangeTracker.Entries<CurriculumItem>()
                .Should().NotContain(entry => entry.State == EntityState.Added, "the item is added only after the list is accepted");
            await AssertNothingForTheAuditSaveToCommitAsync(dbContext);
        }

        await using var readContext = CreateDbContext(databaseName);
        (await readContext.Set<CurriculumItem>().AnyAsync(item => item.EpaId == SpareEpaId)).Should().BeFalse();
    }

    [Fact]
    public async Task UpdateCurriculumItem_ThatReSendsTheValuesItsDtoReturned_KeepsTheToolList()
    {
        // This is what CurriculumItemsEdit does: load the curriculum, copy the item's DTO into the form, save it
        // unchanged. If the DTO dropped the list, or swapped it with the stage-override JSON beside it, or the
        // command defaulted it, an administrator who only opened and saved PAED-005 would let every instrument
        // credit it, and nothing would say so.
        var databaseName = Guid.NewGuid().ToString();
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);
        }

        CurriculumItemDto loaded;
        await using (var dbContext = CreateDbContext(databaseName))
        {
            var curriculum = await new GetCurriculumByIdQueryHandler(dbContext).Handle(
                new GetCurriculumByIdQuery(CurriculumId, TestPrincipals.Administrator()),
                CancellationToken.None);
            loaded = curriculum!.Items.Single(item => item.Id == NationalItemId);
        }

        loaded.PermittedToolKeys.Should().Equal(Paed005Tools, "guard: the DTO must carry the list for the round trip to mean anything");

        await using (var dbContext = CreateDbContext(databaseName))
        {
            await new UpdateCurriculumItemCommandHandler(dbContext).Handle(
                new UpdateCurriculumItemCommand(
                    CurriculumId,
                    loaded.Id,
                    loaded.EpaId,
                    loaded.RequiredCount,
                    loaded.QuotaPeriod,
                    loaded.MinimumLevelOrder,
                    loaded.WindowMonths,
                    loaded.Weight,
                    loaded.MinimumLevelByStageJson,
                    loaded.PermittedToolKeys,
                    TestPrincipals.Administrator(),
                    loaded.ScaleId),
                CancellationToken.None);
        }

        await using var readContext = CreateDbContext(databaseName);
        var stored = await readContext.Set<CurriculumItem>().SingleAsync(item => item.Id == NationalItemId);
        CurriculumItem.ParsePermittedTools(stored.PermittedToolsJson).Should().Equal(Paed005Tools);
        CurriculumItem.ParseStageOverrides(stored.MinimumLevelByStageJson)
            .Should().BeEquivalentTo(new Dictionary<int, int> { [1] = 3, [4] = 5 }, "the adjacent string? column must not be swapped with the list");
    }

    [Fact]
    public async Task UpdateCurriculumItem_ThatReSendsAnUnrestrictedItemsDto_KeepsItUnrestricted()
    {
        // PermittedToolKeys is an empty list for an unrestricted item. Re-sending it must store null again, not "[]",
        // which would be a restriction that permits nothing.
        var databaseName = Guid.NewGuid().ToString();
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);
        }

        CurriculumItemDto loaded;
        await using (var dbContext = CreateDbContext(databaseName))
        {
            var curriculum = await new GetCurriculumByIdQueryHandler(dbContext).Handle(
                new GetCurriculumByIdQuery(CurriculumId, TestPrincipals.Administrator()),
                CancellationToken.None);
            loaded = curriculum!.Items.Single(item => item.Id == LocalItemId);
        }

        loaded.PermittedToolsJson.Should().BeNull("guard: the local item starts unrestricted");
        loaded.PermittedToolKeys.Should().BeEmpty();

        await using (var dbContext = CreateDbContext(databaseName))
        {
            await new UpdateCurriculumItemCommandHandler(dbContext).Handle(
                new UpdateCurriculumItemCommand(
                    CurriculumId,
                    loaded.Id,
                    loaded.EpaId,
                    loaded.RequiredCount,
                    loaded.QuotaPeriod,
                    loaded.MinimumLevelOrder,
                    loaded.WindowMonths,
                    loaded.Weight,
                    loaded.MinimumLevelByStageJson,
                    loaded.PermittedToolKeys,
                    TestPrincipals.Administrator(),
                    loaded.ScaleId),
                CancellationToken.None);
        }

        await using var readContext = CreateDbContext(databaseName);
        var stored = await readContext.Set<CurriculumItem>().SingleAsync(item => item.Id == LocalItemId);
        stored.PermittedToolsJson.Should().BeNull();
    }

    [Fact]
    public async Task GetCurriculaList_CarriesEachItemsToolList()
    {
        // One restricted item and one unrestricted, so a projection that hard-coded either state, or that passed the
        // stage-override JSON (the string? beside it) in its place, fails on one of them.
        await using var dbContext = CreateDbContext(Guid.NewGuid().ToString());
        await SeedAsync(dbContext);

        var curricula = await new GetCurriculaListQueryHandler(dbContext).Handle(
            new GetCurriculaListQuery(TestPrincipals.Administrator()),
            CancellationToken.None);

        AssertItemsCarryTheirToolLists(curricula.Single().Items);
    }

    [Fact]
    public async Task GetCurriculumById_CarriesEachItemsToolList()
    {
        await using var dbContext = CreateDbContext(Guid.NewGuid().ToString());
        await SeedAsync(dbContext);

        var curriculum = await new GetCurriculumByIdQueryHandler(dbContext).Handle(
            new GetCurriculumByIdQuery(CurriculumId, TestPrincipals.Administrator()),
            CancellationToken.None);

        curriculum.Should().NotBeNull();
        AssertItemsCarryTheirToolLists(curriculum!.Items);
    }

    [Fact]
    public async Task AnInstitutionalAdmin_CannotChangeANationalItemsToolList()
    {
        // The list is part of the College's national core. An institution may add local items to a curriculum it has
        // adopted, never loosen (or tighten) what the College says may credit a national EPA.
        var databaseName = Guid.NewGuid().ToString();
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);
        }

        await using (var dbContext = CreateDbContext(databaseName))
        {
            var act = () => new UpdateCurriculumItemCommandHandler(dbContext).Handle(
                UpdateCommand(NationalItemId, NationalEpaId, ["mini_cex"], TestPrincipals.InstitutionalAdmin(InstitutionId)),
                CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>();
            await AssertNothingForTheAuditSaveToCommitAsync(dbContext);
        }

        await using var readContext = CreateDbContext(databaseName);
        var stored = await readContext.Set<CurriculumItem>().SingleAsync(item => item.Id == NationalItemId);
        CurriculumItem.ParsePermittedTools(stored.PermittedToolsJson).Should().Equal(Paed005Tools);
    }

    [Fact]
    public async Task AnInstitutionalAdmin_CanSetAToolListOnTheirOwnLocalItem()
    {
        // The control for the refusal above: the same caller, their own item.
        var databaseName = Guid.NewGuid().ToString();
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);
        }

        await using (var dbContext = CreateDbContext(databaseName))
        {
            await new UpdateCurriculumItemCommandHandler(dbContext).Handle(
                UpdateCommand(LocalItemId, LocalEpaId, ["CBD", "direct_observation"], TestPrincipals.InstitutionalAdmin(InstitutionId)),
                CancellationToken.None);
        }

        await using var readContext = CreateDbContext(databaseName);
        var stored = await readContext.Set<CurriculumItem>().SingleAsync(item => item.Id == LocalItemId);
        CurriculumItem.ParsePermittedTools(stored.PermittedToolsJson).Should().Equal("cbd", "direct_observation");
        stored.OwningInstitutionId.Should().Be(InstitutionId, "guard: this is the institution's own item");
    }

    [Fact]
    public async Task AnInstitutionalAdmin_CanAddALocalItemWithAToolList()
    {
        var databaseName = Guid.NewGuid().ToString();
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);

            await new AddCurriculumItemCommandHandler(dbContext).Handle(
                AddCommand(SpareEpaId, ["msf", "cbd"], TestPrincipals.InstitutionalAdmin(InstitutionId)),
                CancellationToken.None);
        }

        await using var readContext = CreateDbContext(databaseName);
        var stored = await readContext.Set<CurriculumItem>().SingleAsync(item => item.EpaId == SpareEpaId);
        stored.OwningInstitutionId.Should().Be(InstitutionId);
        CurriculumItem.ParsePermittedTools(stored.PermittedToolsJson).Should().Equal("cbd", "msf");
    }

    private static void AssertItemsCarryTheirToolLists(IReadOnlyList<CurriculumItemDto> items)
    {
        items.Should().HaveCount(2);

        var restricted = items.Single(item => item.Id == NationalItemId);
        CurriculumItem.ParsePermittedTools(restricted.PermittedToolsJson).Should().Equal(Paed005Tools);
        restricted.PermittedToolKeys.Should().Equal(Paed005Tools);
        CurriculumItem.ParseStageOverrides(restricted.MinimumLevelByStageJson)
            .Should().BeEquivalentTo(new Dictionary<int, int> { [1] = 3, [4] = 5 });

        var unrestricted = items.Single(item => item.Id == LocalItemId);
        unrestricted.PermittedToolsJson.Should().BeNull();
        unrestricted.PermittedToolKeys.Should().BeEmpty();
    }

    /// <summary>
    /// The stored array element by element, WITHOUT canonicalising: what order and casing the writer stored. A
    /// structural parse, not a string comparison, so it holds after Postgres re-renders the jsonb.
    /// </summary>
    private static string[] StoredArray(string? json)
    {
        json.Should().NotBeNull();
        return JsonSerializer.Deserialize<string[]>(json!)!;
    }

    private static async Task AssertNothingForTheAuditSaveToCommitAsync(ApplicationDbContext dbContext)
    {
        dbContext.ChangeTracker.Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Should().BeEmpty("the audit pipeline's catch saves this context, so anything dirty here would be committed");

        (await dbContext.SaveChangesAsync()).Should().Be(0);
    }

    private static AddCurriculumItemCommand AddCommand(int epaId, IReadOnlyList<string>? permittedToolKeys, System.Security.Claims.ClaimsPrincipal principal)
        => new(CurriculumId, epaId, 3, QuotaPeriod.Semester, 4, 12, null, null, permittedToolKeys, principal);

    private static UpdateCurriculumItemCommand UpdateCommand(
        int itemId,
        int epaId,
        IReadOnlyList<string>? permittedToolKeys,
        System.Security.Claims.ClaimsPrincipal principal,
        int requiredCount = 3)
        => new(CurriculumId, itemId, epaId, requiredCount, QuotaPeriod.Semester, 4, 12, null, """{"1":3,"4":5}""", permittedToolKeys, principal);

    private static ApplicationDbContext CreateDbContext(string databaseName)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options);

    private static async Task SeedAsync(ApplicationDbContext dbContext)
    {
        var college = new College { Id = CollegeId, Name = "CPSA", ShortCode = "CPSA" };
        var speciality = new Speciality { Id = 1, Name = "Paediatrics", CollegeId = CollegeId, College = college };
        var subSpeciality = new SubSpeciality { Id = 1, Name = "Paediatrics", SpecialityId = 1, Speciality = speciality };

        dbContext.Colleges.Add(college);
        dbContext.Specialities.Add(speciality);
        dbContext.SubSpecialities.Add(subSpeciality);
        dbContext.Institutions.Add(new Institution { Id = InstitutionId, Name = "Institution A", ShortCode = "IA", IsActive = true, CreatedOn = DateTime.UtcNow });

        // The vocabulary rows PAED-005's list and the local item's lists draw on. Anything else is unknown.
        dbContext.WbaTools.AddRange(
            new WbaTool { Key = "cbd", Name = "CBD" },
            new WbaTool { Key = "direct_observation", Name = "Direct observation" },
            new WbaTool { Key = "dops", Name = "DOPS" },
            new WbaTool { Key = "mini_cex", Name = "Mini-CEX" },
            new WbaTool { Key = "msf", Name = "MSF" });

        dbContext.Epas.AddRange(
            new Epa { Id = NationalEpaId, Code = "PAED-005", Title = "Performing paediatric procedures", SubSpecialityId = 1 },
            new Epa { Id = LocalEpaId, Code = "LOC-001", Title = "An institution-local EPA", SubSpecialityId = 1, OwningInstitutionId = InstitutionId },
            new Epa { Id = SpareEpaId, Code = "PAED-009", Title = "An EPA not yet on the curriculum", SubSpecialityId = 1 });

        var curriculum = new Curriculum
        {
            Id = CurriculumId,
            SubSpecialityId = 1,
            SubSpeciality = subSpeciality,
            Name = "Paediatric EPA Curriculum",
            Version = "11.1",
            EffectiveFrom = new DateOnly(2026, 1, 1),
            IsActive = true
        };

        // The national core item carries Annexure A's PAED-005 list, stored canonically, beside a stage-override map
        // so a positional swap of the two string? columns would show.
        curriculum.Items.Add(new CurriculumItem
        {
            Id = NationalItemId,
            EpaId = NationalEpaId,
            RequiredCount = 3,
            QuotaPeriod = QuotaPeriod.Semester,
            MinimumLevelOrder = 4,
            WindowMonths = 12,
            MinimumLevelByStageJson = """{"1":3,"4":5}""",
            PermittedToolsJson = CurriculumItem.NormalizePermittedToolsJson(Paed005Tools)
        });

        // An institution-local addition, unrestricted.
        curriculum.Items.Add(new CurriculumItem
        {
            Id = LocalItemId,
            EpaId = LocalEpaId,
            OwningInstitutionId = InstitutionId,
            RequiredCount = 2,
            QuotaPeriod = QuotaPeriod.AcademicYear,
            MinimumLevelOrder = 4,
            WindowMonths = 12
        });

        dbContext.Curricula.Add(curriculum);
        await dbContext.SaveChangesAsync();
    }
}
