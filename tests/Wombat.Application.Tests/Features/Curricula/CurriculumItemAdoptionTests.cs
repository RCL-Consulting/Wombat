using System.Security.Claims;
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
/// T223 — an institution keeps items of its own only on a curriculum it has adopted, now or before. The Add and Update
/// commands and the EPA picker refuse an institution's own item on a curriculum that institution never adopted; Remove
/// takes one off wherever it is; and such an item opens the curriculum to nobody at that institution.
/// </summary>
/// <remarks>
/// <para>
/// Before T223 no item command asked for an adoption. An InstitutionalAdmin could put an item of their institution's own on
/// any curriculum by calling the Add command directly, and since T211 opens a curriculum that holds the institution's own
/// items, then open that curriculum and read its national items.
/// </para>
/// <para>
/// The world: institution A adopts curriculum 3000. Institution B adopted it and moved to 3001, keeping an item of its own
/// on 3000 for the trainees it admitted there (T211's moved-off case). Institution C never adopted 3000, yet an item of its
/// own is on it, written below the handlers, as only a command without T223's check could have written it. Institution D
/// is C with an adoption elsewhere: it adopted 3001 and nothing else, and an item of its own is on 3000 too. Every working
/// institution has adopted something, so D is the case that shows the check asks about this curriculum, not any (T223
/// review: with C alone, a check that accepted an adoption of any curriculum passed every test).
/// </para>
/// <para>
/// Every refusal is checked for what it leaves behind as well as for the throw: the audit pipeline saves the request's
/// context from its catch, so a refusal that came after a mutation would commit it.
/// </para>
/// </remarks>
public sealed class CurriculumItemAdoptionTests
{
    private const int CollegeId = 1;
    private const int SubSpecialityId = 1;
    private const int InstitutionA = 40;
    private const int InstitutionB = 41;
    private const int InstitutionC = 42;
    private const int InstitutionD = 43;

    private const int CurriculumId = 3000;
    private const int NextVersionId = 3001;

    private const int NationalItemId = 7000;   // PAED-001
    private const int BItemId = 7002;          // LOC-B01, B's, from its earlier adoption
    private const int CItemId = 7003;          // LOC-C01, C's, on a curriculum C never adopted
    private const int DItemId = 7004;          // LOC-D01, D's, on 3000, though D adopted only 3001

    private const int Paed001 = 5000;
    private const int Paed009 = 5001;
    private const int LocA01 = 5010;
    private const int LocB01 = 5020;
    private const int LocB02 = 5021;
    private const int LocC01 = 5030;
    private const int LocC02 = 5031;
    private const int LocD01 = 5040;
    private const int LocD02 = 5041;

    private const string NotAdoptedByYours =
        "Your institution has not adopted this curriculum. An institution adds items of its own only to a curriculum it has adopted.";

    private const string NotAdoptedByTheItems =
        "This item's institution has not adopted this curriculum. An institution keeps items of its own only on a curriculum it has adopted.";

    // ---- Add ----

    [Theory]
    // Adopted nothing.
    [InlineData(InstitutionC, Paed009)]
    [InlineData(InstitutionC, LocC02)]
    // Adopted another version (3001), and not this one.
    [InlineData(InstitutionD, Paed009)]
    [InlineData(InstitutionD, LocD02)]
    public async Task AddCurriculumItem_ByAnInstitutionThatNeverAdoptedTheCurriculum_IsRefused_BeforeAnyWrite(int institutionId, int epaId)
    {
        var databaseName = Guid.NewGuid().ToString();
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);
        }

        await using (var dbContext = CreateDbContext(databaseName))
        {
            var act = () => new AddCurriculumItemCommandHandler(dbContext).Handle(
                AddCommand(CurriculumId, epaId, TestPrincipals.InstitutionalAdmin(institutionId)), CancellationToken.None);

            (await act.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message.Should().Be(NotAdoptedByYours);
            await AssertNothingForTheAuditSaveToCommitAsync(dbContext);
        }

        await using var readContext = CreateDbContext(databaseName);
        (await readContext.Set<CurriculumItem>().Where(item => item.CurriculumId == CurriculumId).Select(item => item.Id).ToListAsync())
            .Should().BeEquivalentTo([NationalItemId, BItemId, CItemId, DItemId]);
    }

    [Theory]
    // An active adoption.
    [InlineData(InstitutionA, CurriculumId, LocA01)]
    [InlineData(InstitutionA, CurriculumId, Paed009)]
    // An adoption since superseded (T211's moved-off case): B's trainees admitted to 3000 are still measured there.
    [InlineData(InstitutionB, CurriculumId, LocB02)]
    [InlineData(InstitutionB, CurriculumId, Paed009)]
    // D, on the version it did adopt: its refusal on 3000 is about 3000.
    [InlineData(InstitutionD, NextVersionId, LocD02)]
    [InlineData(InstitutionD, NextVersionId, Paed009)]
    public async Task AddCurriculumItem_ByAnInstitutionThatAdoptedTheCurriculum_ActiveOrSuperseded_IsStored(int institutionId, int curriculumId, int epaId)
    {
        var databaseName = Guid.NewGuid().ToString();
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);
            await new AddCurriculumItemCommandHandler(dbContext).Handle(
                AddCommand(curriculumId, epaId, TestPrincipals.InstitutionalAdmin(institutionId)), CancellationToken.None);
        }

        await using var readContext = CreateDbContext(databaseName);
        (await readContext.Set<CurriculumItem>().SingleAsync(item => item.CurriculumId == curriculumId && item.EpaId == epaId))
            .OwningInstitutionId.Should().Be(institutionId);
    }

    [Fact]
    public async Task AddCurriculumItem_ANationalItem_AsksForNoAdoption()
    {
        // The College adopts nothing: a national item belongs to every institution that adopts the curriculum.
        var databaseName = Guid.NewGuid().ToString();
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);
            await new AddCurriculumItemCommandHandler(dbContext).Handle(
                AddCommand(NextVersionId, Paed009, TestPrincipals.CollegeAdmin(CollegeId)), CancellationToken.None);
        }

        await using var readContext = CreateDbContext(databaseName);
        (await readContext.Set<CurriculumItem>().SingleAsync(item => item.CurriculumId == NextVersionId && item.EpaId == Paed009))
            .OwningInstitutionId.Should().BeNull();
    }

    // ---- Update ----

    [Theory]
    // C's own InstitutionalAdmin, moving the item and changing its target.
    [InlineData(InstitutionC, false, LocC02, NotAdoptedByYours)]
    // Keeping the EPA: the refusal is not about the EPA.
    [InlineData(InstitutionC, false, LocC01, NotAdoptedByYours)]
    // Judged by the item's owner, not the caller: an Administrator is refused as C would be.
    [InlineData(InstitutionC, true, LocC01, NotAdoptedByTheItems)]
    // D adopted 3001, not 3000, where this item is.
    [InlineData(InstitutionD, false, LocD02, NotAdoptedByYours)]
    [InlineData(InstitutionD, false, LocD01, NotAdoptedByYours)]
    [InlineData(InstitutionD, true, LocD01, NotAdoptedByTheItems)]
    public async Task UpdateCurriculumItem_OfAnItemWhoseInstitutionNeverAdoptedTheCurriculum_IsRefused_BeforeAnyWrite(
        int institutionId, bool asAdministrator, int epaId, string message)
    {
        var (itemId, storedEpaId) = institutionId == InstitutionC ? (CItemId, LocC01) : (DItemId, LocD01);
        var databaseName = Guid.NewGuid().ToString();
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);
        }

        var principal = asAdministrator ? TestPrincipals.Administrator() : TestPrincipals.InstitutionalAdmin(institutionId);
        await using (var dbContext = CreateDbContext(databaseName))
        {
            // The target change rides along, so a refusal that came after the assignments would show as a Modified item.
            var act = () => new UpdateCurriculumItemCommandHandler(dbContext).Handle(
                UpdateCommand(itemId, epaId, principal, requiredCount: 9), CancellationToken.None);

            (await act.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message.Should().Be(message);

            var tracked = dbContext.ChangeTracker.Entries<CurriculumItem>().Single(entry => entry.Entity.Id == itemId);
            tracked.State.Should().Be(EntityState.Unchanged, "guard: the handler loaded the item, so a mutation would show");
            await AssertNothingForTheAuditSaveToCommitAsync(dbContext);
        }

        await using var readContext = CreateDbContext(databaseName);
        var stored = await readContext.Set<CurriculumItem>().SingleAsync(item => item.Id == itemId);
        stored.EpaId.Should().Be(storedEpaId);
        stored.RequiredCount.Should().Be(3);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdateCurriculumItem_OnAVersionTheInstitutionHasMovedOff_IsStored(bool asAdministrator)
    {
        var databaseName = Guid.NewGuid().ToString();
        var principal = asAdministrator ? TestPrincipals.Administrator() : TestPrincipals.InstitutionalAdmin(InstitutionB);
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);
            await new UpdateCurriculumItemCommandHandler(dbContext).Handle(
                UpdateCommand(BItemId, LocB02, principal, requiredCount: 9), CancellationToken.None);
        }

        await using var readContext = CreateDbContext(databaseName);
        var stored = await readContext.Set<CurriculumItem>().SingleAsync(item => item.Id == BItemId);
        stored.EpaId.Should().Be(LocB02);
        stored.RequiredCount.Should().Be(9);
    }

    // ---- Remove ----

    [Theory]
    // Never adopted: Remove is how such an item comes off.
    [InlineData(InstitutionC, CItemId)]
    [InlineData(InstitutionD, DItemId)]
    // Moved off (T211).
    [InlineData(InstitutionB, BItemId)]
    public async Task RemoveCurriculumItem_TakesAnInstitutionsOwnItemOff_WhereverItIs(int institutionId, int itemId)
    {
        var databaseName = Guid.NewGuid().ToString();
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);
            await new RemoveCurriculumItemCommandHandler(dbContext).Handle(
                new RemoveCurriculumItemCommand(CurriculumId, itemId, TestPrincipals.InstitutionalAdmin(institutionId)), CancellationToken.None);
        }

        await using var readContext = CreateDbContext(databaseName);
        (await readContext.Set<CurriculumItem>().AnyAsync(item => item.Id == itemId)).Should().BeFalse();
    }

    // ---- The picker: the same rule ----

    [Theory]
    [InlineData(InstitutionC, null)]
    [InlineData(InstitutionC, CItemId)]
    [InlineData(InstitutionD, null)]
    [InlineData(InstitutionD, DItemId)]
    public async Task ThePicker_RefusesAnInstitutionsOwnItem_OnACurriculumItNeverAdopted(int institutionId, int? itemId)
    {
        await using var dbContext = CreateDbContext(Guid.NewGuid().ToString());
        await SeedAsync(dbContext);

        var act = () => new ListCurriculumItemEpaOptionsQueryHandler(dbContext).Handle(
            new ListCurriculumItemEpaOptionsQuery(CurriculumId, itemId, TestPrincipals.InstitutionalAdmin(institutionId)), CancellationToken.None);

        (await act.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message.Should().Be(NotAdoptedByYours);
    }

    [Theory]
    [InlineData(InstitutionA, CurriculumId, null, new[] { "LOC-A01", "PAED-009" })]
    [InlineData(InstitutionB, CurriculumId, null, new[] { "LOC-B02", "PAED-009" })]
    [InlineData(InstitutionB, CurriculumId, BItemId, new[] { "LOC-B01", "LOC-B02", "PAED-009" })]
    // D on the version it adopted. LOC-D01 is offered: D's item on 3000 holds it there, not here.
    [InlineData(InstitutionD, NextVersionId, null, new[] { "LOC-D01", "LOC-D02", "PAED-009" })]
    public async Task ThePicker_AnswersAnInstitutionThatAdoptedTheCurriculum_ActiveOrSuperseded(int institutionId, int curriculumId, int? itemId, string[] expected)
    {
        await using var dbContext = CreateDbContext(Guid.NewGuid().ToString());
        await SeedAsync(dbContext);

        var offered = await new ListCurriculumItemEpaOptionsQueryHandler(dbContext).Handle(
            new ListCurriculumItemEpaOptionsQuery(curriculumId, itemId, TestPrincipals.InstitutionalAdmin(institutionId)), CancellationToken.None);

        offered.Select(epa => epa.Code).Should().Equal(expected);
    }

    // ---- Who opens the curriculum ----

    [Theory]
    [InlineData(InstitutionA, true)]
    // Moved off, and keeps an item of its own there.
    [InlineData(InstitutionB, true)]
    // An item of its own there, but no adoption: T211's own-item arm opened it, and the Add picker then refused.
    [InlineData(InstitutionC, false)]
    // The same, with an adoption of 3001 only.
    [InlineData(InstitutionD, false)]
    public async Task ACurriculum_OpensToAnInstitution_OnlyWhereItAdoptedIt(int institutionId, bool opens)
    {
        await using var dbContext = CreateDbContext(Guid.NewGuid().ToString());
        await SeedAsync(dbContext);
        var principal = TestPrincipals.InstitutionalAdmin(institutionId);

        var read = await new GetCurriculumByIdQueryHandler(dbContext).Handle(
            new GetCurriculumByIdQuery(CurriculumId, principal), CancellationToken.None);
        var list = await new GetCurriculaListQueryHandler(dbContext).Handle(new GetCurriculaListQuery(principal), CancellationToken.None);

        (read is not null).Should().Be(opens);
        list.Any(curriculum => curriculum.Id == CurriculumId).Should().Be(opens, "the list and the read are one rule");
    }

    [Fact]
    public async Task AnInstitutionThatAdoptedAnotherVersion_OpensThatVersion_AndNotThisOne()
    {
        // Guard for D's refusals: its adoption of 3001 is a working one.
        await using var dbContext = CreateDbContext(Guid.NewGuid().ToString());
        await SeedAsync(dbContext);
        var principal = TestPrincipals.InstitutionalAdmin(InstitutionD);

        var list = await new GetCurriculaListQueryHandler(dbContext).Handle(new GetCurriculaListQuery(principal), CancellationToken.None);
        list.Select(curriculum => curriculum.Id).Should().Equal(NextVersionId);
        (await new GetCurriculumByIdQueryHandler(dbContext).Handle(new GetCurriculumByIdQuery(NextVersionId, principal), CancellationToken.None))
            .Should().NotBeNull();
    }

    // ---- helpers ----

    private static async Task AssertNothingForTheAuditSaveToCommitAsync(ApplicationDbContext dbContext)
    {
        dbContext.ChangeTracker.Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Should().BeEmpty("the audit pipeline's catch saves this context, so anything dirty here would be committed");

        (await dbContext.SaveChangesAsync()).Should().Be(0);
    }

    private static AddCurriculumItemCommand AddCommand(int curriculumId, int epaId, ClaimsPrincipal principal)
        => new(curriculumId, epaId, 3, QuotaPeriod.Semester, 4, 12, null, null, null, null, null, false, principal);

    private static UpdateCurriculumItemCommand UpdateCommand(int itemId, int epaId, ClaimsPrincipal principal, int requiredCount)
        => new(CurriculumId, itemId, epaId, requiredCount, QuotaPeriod.Semester, 4, 12, null, null, null, null, null, false, principal);

    private static ApplicationDbContext CreateDbContext(string databaseName)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options);

    private static async Task SeedAsync(ApplicationDbContext dbContext)
    {
        var college = new College { Id = CollegeId, Name = "CPSA", ShortCode = "CPSA" };
        var speciality = new Speciality { Id = 1, Name = "Paediatrics", CollegeId = CollegeId, College = college };
        var subSpeciality = new SubSpeciality { Id = SubSpecialityId, Name = "General Paediatrics", SpecialityId = 1, Speciality = speciality };

        dbContext.Colleges.Add(college);
        dbContext.Specialities.Add(speciality);
        dbContext.SubSpecialities.Add(subSpeciality);
        dbContext.Institutions.AddRange(
            new Institution { Id = InstitutionA, Name = "Institution A", ShortCode = "IA", IsActive = true, CreatedOn = DateTime.UtcNow },
            new Institution { Id = InstitutionB, Name = "Institution B", ShortCode = "IB", IsActive = true, CreatedOn = DateTime.UtcNow },
            new Institution { Id = InstitutionC, Name = "Institution C", ShortCode = "IC", IsActive = true, CreatedOn = DateTime.UtcNow },
            new Institution { Id = InstitutionD, Name = "Institution D", ShortCode = "ID", IsActive = true, CreatedOn = DateTime.UtcNow });

        dbContext.Epas.AddRange(
            new Epa { Id = Paed001, Code = "PAED-001", Title = "On the national core", SubSpecialityId = SubSpecialityId },
            new Epa { Id = Paed009, Code = "PAED-009", Title = "National, not yet on the curriculum", SubSpecialityId = SubSpecialityId },
            new Epa { Id = LocA01, Code = "LOC-A01", Title = "A's", SubSpecialityId = SubSpecialityId, OwningInstitutionId = InstitutionA },
            new Epa { Id = LocB01, Code = "LOC-B01", Title = "B's, on its item", SubSpecialityId = SubSpecialityId, OwningInstitutionId = InstitutionB },
            new Epa { Id = LocB02, Code = "LOC-B02", Title = "B's second", SubSpecialityId = SubSpecialityId, OwningInstitutionId = InstitutionB },
            new Epa { Id = LocC01, Code = "LOC-C01", Title = "C's, on its item", SubSpecialityId = SubSpecialityId, OwningInstitutionId = InstitutionC },
            new Epa { Id = LocC02, Code = "LOC-C02", Title = "C's second", SubSpecialityId = SubSpecialityId, OwningInstitutionId = InstitutionC },
            new Epa { Id = LocD01, Code = "LOC-D01", Title = "D's, on its item", SubSpecialityId = SubSpecialityId, OwningInstitutionId = InstitutionD },
            new Epa { Id = LocD02, Code = "LOC-D02", Title = "D's second", SubSpecialityId = SubSpecialityId, OwningInstitutionId = InstitutionD });

        var curriculum = Curriculum(CurriculumId, subSpeciality, "11.1");
        curriculum.Items.Add(Item(NationalItemId, Paed001, owner: null));
        curriculum.Items.Add(Item(BItemId, LocB01, InstitutionB));
        curriculum.Items.Add(Item(CItemId, LocC01, InstitutionC));
        curriculum.Items.Add(Item(DItemId, LocD01, InstitutionD));

        var nextVersion = Curriculum(NextVersionId, subSpeciality, "12.0");
        nextVersion.Items.Add(Item(7100, Paed001, owner: null));

        dbContext.Curricula.AddRange(curriculum, nextVersion);

        dbContext.InstitutionCurriculumAdoptions.AddRange(
            Adoption(1, InstitutionA, CurriculumId, isActive: true),
            Adoption(2, InstitutionB, CurriculumId, isActive: false),
            Adoption(3, InstitutionB, NextVersionId, isActive: true),
            // D adopted 3001 and nothing else; its item on 3000 has no adoption there.
            Adoption(4, InstitutionD, NextVersionId, isActive: true));

        await dbContext.SaveChangesAsync();
    }

    private static Curriculum Curriculum(int id, SubSpeciality subSpeciality, string version)
        => new()
        {
            Id = id,
            SubSpecialityId = subSpeciality.Id,
            SubSpeciality = subSpeciality,
            Name = "Paediatric EPA Curriculum",
            Version = version,
            EffectiveFrom = new DateOnly(2026, 1, 1),
            IsActive = true
        };

    private static CurriculumItem Item(int id, int epaId, int? owner)
        => new()
        {
            Id = id,
            EpaId = epaId,
            OwningInstitutionId = owner,
            RequiredCount = 3,
            QuotaPeriod = QuotaPeriod.Semester,
            MinimumLevelOrder = 4,
            WindowMonths = 12
        };

    private static InstitutionCurriculumAdoption Adoption(int id, int institutionId, int curriculumId, bool isActive)
        => new()
        {
            Id = id,
            InstitutionId = institutionId,
            CurriculumId = curriculumId,
            SubSpecialityId = SubSpecialityId,
            AdoptedOn = new DateOnly(2026, 1, 1),
            IsActive = isActive
        };
}
