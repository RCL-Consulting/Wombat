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
/// T195 — a curriculum item names only an EPA its owner may use. A national item names a national EPA of the
/// curriculum's sub-speciality; an institution's local item names one of those or one of that institution's own local
/// EPAs of the sub-speciality. The Add and Update handlers refuse anything else before they mutate, and the picker query
/// offers exactly what they accept.
/// </summary>
/// <remarks>
/// <para>
/// A national item is part of the College's published curriculum and is shared by every institution that adopts it. Before
/// T195 nothing stopped one from naming institution A's local EPA, which no other institution's trainee could ever file
/// against, or an EPA of another sub-speciality, which the item's trainees are not trained in.
/// </para>
/// <para>
/// Every refusal is checked for what it leaves behind as well as for the throw: the audit pipeline saves the request's
/// context from its catch, so a refusal that came after a mutation would commit it.
/// </para>
/// </remarks>
public sealed class CurriculumItemEpaOwnershipTests
{
    private const int CollegeId = 1;
    private const int OtherCollegeId = 2;
    private const int HereSubSpecialityId = 1;
    private const int ElsewhereSubSpecialityId = 2;
    private const int InstitutionA = 40;
    private const int InstitutionB = 41;
    private const int CurriculumId = 3000;
    private const int NationalItemId = 7000;
    private const int LocalItemId = 7001;

    // On the curriculum already: the national item's EPA and institution A's local item's EPA.
    private const int CoreEpaId = 5000;          // PAED-001, national, here
    private const int LocalCoreEpaId = 5001;     // LOC-A00, A's, here

    // Candidates, one of each kind.
    private const int NationalHere = 5010;       // PAED-009, national, here
    private const int NationalElsewhere = 5011;  // NEO-001, national, another sub-speciality
    private const int LocalAHere = 5012;         // LOC-A01, A's, here
    private const int LocalAElsewhere = 5013;    // LOC-A02, A's, another sub-speciality
    private const int LocalBHere = 5014;         // LOC-B01, B's, here
    private const int Missing = 5999;            // no such EPA

    private const string Refusal = "This item cannot name the selected EPA.";

    private static readonly int[] EveryEpa =
        [CoreEpaId, LocalCoreEpaId, NationalHere, NationalElsewhere, LocalAHere, LocalAElsewhere, LocalBHere, Missing];

    public enum Caller
    {
        Administrator,
        CollegeAdmin,
        InstitutionalAdminA,
        InstitutionalAdminB,
        OtherCollegeAdmin
    }

    // ---- Add ----

    [Theory]
    // A national item (an Administrator or a CollegeAdmin adds one) names no local EPA and no EPA of another sub-speciality.
    [InlineData(Caller.Administrator, NationalElsewhere)]
    [InlineData(Caller.Administrator, LocalAHere)]
    [InlineData(Caller.Administrator, LocalBHere)]
    [InlineData(Caller.Administrator, LocalAElsewhere)]
    [InlineData(Caller.Administrator, Missing)]
    [InlineData(Caller.CollegeAdmin, NationalElsewhere)]
    [InlineData(Caller.CollegeAdmin, LocalAHere)]
    [InlineData(Caller.CollegeAdmin, LocalBHere)]
    // Institution A's local item names no other institution's local EPA and nothing of another sub-speciality.
    [InlineData(Caller.InstitutionalAdminA, LocalBHere)]
    [InlineData(Caller.InstitutionalAdminA, NationalElsewhere)]
    [InlineData(Caller.InstitutionalAdminA, LocalAElsewhere)]
    [InlineData(Caller.InstitutionalAdminA, Missing)]
    public async Task AddCurriculumItem_NamingAnEpaItsOwnerMayNotUse_IsRefused_WithNothingAdded(Caller caller, int epaId)
    {
        var databaseName = Guid.NewGuid().ToString();
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);
        }

        await using (var dbContext = CreateDbContext(databaseName))
        {
            var act = () => new AddCurriculumItemCommandHandler(dbContext).Handle(AddCommand(epaId, Principal(caller)), CancellationToken.None);

            var refusal = (await act.Should().ThrowAsync<InvalidOperationException>()).Which;
            refusal.Message.Should().StartWith(Refusal);
            AssertNamesNothingOf(refusal.Message, epaId);

            dbContext.ChangeTracker.Entries<CurriculumItem>()
                .Should().NotContain(entry => entry.State == EntityState.Added, "the item is added only after its EPA is accepted");
            await AssertNothingForTheAuditSaveToCommitAsync(dbContext);
        }

        await using var readContext = CreateDbContext(databaseName);
        (await readContext.Set<CurriculumItem>().Select(item => item.Id).ToListAsync())
            .Should().BeEquivalentTo([NationalItemId, LocalItemId]);
    }

    [Theory]
    [InlineData(Caller.Administrator, NationalHere, null)]
    [InlineData(Caller.CollegeAdmin, NationalHere, null)]
    [InlineData(Caller.InstitutionalAdminA, NationalHere, InstitutionA)]
    [InlineData(Caller.InstitutionalAdminA, LocalAHere, InstitutionA)]
    public async Task AddCurriculumItem_NamingAnEpaItsOwnerMayUse_IsStored(Caller caller, int epaId, int? expectedOwner)
    {
        var databaseName = Guid.NewGuid().ToString();
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);
            await new AddCurriculumItemCommandHandler(dbContext).Handle(AddCommand(epaId, Principal(caller)), CancellationToken.None);
        }

        await using var readContext = CreateDbContext(databaseName);
        var stored = await readContext.Set<CurriculumItem>().SingleAsync(item => item.EpaId == epaId);
        stored.OwningInstitutionId.Should().Be(expectedOwner, "guard: the item's owner is the one the rule was judged for");
    }

    // ---- Update ----

    [Theory]
    [InlineData(Caller.Administrator, NationalItemId, NationalElsewhere)]
    [InlineData(Caller.Administrator, NationalItemId, LocalAHere)]
    [InlineData(Caller.Administrator, NationalItemId, LocalBHere)]
    [InlineData(Caller.Administrator, NationalItemId, Missing)]
    [InlineData(Caller.CollegeAdmin, NationalItemId, LocalAHere)]
    [InlineData(Caller.CollegeAdmin, NationalItemId, NationalElsewhere)]
    // A's local item, edited by A's InstitutionalAdmin or by an Administrator acting for A.
    [InlineData(Caller.InstitutionalAdminA, LocalItemId, LocalBHere)]
    [InlineData(Caller.InstitutionalAdminA, LocalItemId, NationalElsewhere)]
    [InlineData(Caller.InstitutionalAdminA, LocalItemId, LocalAElsewhere)]
    [InlineData(Caller.InstitutionalAdminA, LocalItemId, Missing)]
    [InlineData(Caller.Administrator, LocalItemId, LocalBHere)]
    [InlineData(Caller.Administrator, LocalItemId, LocalAElsewhere)]
    public async Task UpdateCurriculumItem_NamingAnEpaItsOwnerMayNotUse_IsRefused_WithNothingChanged(Caller caller, int itemId, int epaId)
    {
        var databaseName = Guid.NewGuid().ToString();
        int storedEpaId;
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);
            storedEpaId = (await dbContext.Set<CurriculumItem>().SingleAsync(item => item.Id == itemId)).EpaId;
        }

        await using (var dbContext = CreateDbContext(databaseName))
        {
            // The target change rides along, so a refusal that came after the assignments would show as a Modified item.
            var act = () => new UpdateCurriculumItemCommandHandler(dbContext).Handle(
                UpdateCommand(itemId, epaId, Principal(caller), requiredCount: 9), CancellationToken.None);

            var refusal = (await act.Should().ThrowAsync<InvalidOperationException>()).Which;
            refusal.Message.Should().StartWith(Refusal);
            AssertNamesNothingOf(refusal.Message, epaId);

            var tracked = dbContext.ChangeTracker.Entries<CurriculumItem>().Single(entry => entry.Entity.Id == itemId);
            tracked.State.Should().Be(EntityState.Unchanged, "guard: the handler loaded the item, so a mutation would show");
            tracked.Entity.EpaId.Should().Be(storedEpaId);
            await AssertNothingForTheAuditSaveToCommitAsync(dbContext);
        }

        await using var readContext = CreateDbContext(databaseName);
        var stored = await readContext.Set<CurriculumItem>().SingleAsync(item => item.Id == itemId);
        stored.EpaId.Should().Be(storedEpaId);
        stored.RequiredCount.Should().Be(3);
    }

    [Theory]
    [InlineData(Caller.Administrator, NationalItemId, NationalHere)]
    [InlineData(Caller.CollegeAdmin, NationalItemId, NationalHere)]
    [InlineData(Caller.InstitutionalAdminA, LocalItemId, NationalHere)]
    [InlineData(Caller.InstitutionalAdminA, LocalItemId, LocalAHere)]
    // The item's owner decides whose local EPAs it may name, not the caller's: an Administrator holds no institution.
    [InlineData(Caller.Administrator, LocalItemId, LocalAHere)]
    public async Task UpdateCurriculumItem_NamingAnEpaItsOwnerMayUse_IsStored(Caller caller, int itemId, int epaId)
    {
        var databaseName = Guid.NewGuid().ToString();
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);
            await new UpdateCurriculumItemCommandHandler(dbContext).Handle(
                UpdateCommand(itemId, epaId, Principal(caller), requiredCount: 9), CancellationToken.None);
        }

        await using var readContext = CreateDbContext(databaseName);
        var stored = await readContext.Set<CurriculumItem>().SingleAsync(item => item.Id == itemId);
        stored.EpaId.Should().Be(epaId);
        stored.RequiredCount.Should().Be(9);
    }

    [Fact]
    public async Task UpdateCurriculumItem_ThatKeepsAnEpaTheItemMayNotName_IsRefused()
    {
        // Judged on every save, not only when the EPA changes: a national item that already names A's local EPA (written
        // below the handler, as nothing else could) is the defect, and re-saving it unchanged must not ratify it.
        var databaseName = Guid.NewGuid().ToString();
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);
            var item = await dbContext.Set<CurriculumItem>().SingleAsync(entity => entity.Id == NationalItemId);
            item.EpaId = LocalAHere;
            await dbContext.SaveChangesAsync();
        }

        await using (var dbContext = CreateDbContext(databaseName))
        {
            var act = () => new UpdateCurriculumItemCommandHandler(dbContext).Handle(
                UpdateCommand(NationalItemId, LocalAHere, TestPrincipals.Administrator(), requiredCount: 9), CancellationToken.None);

            (await act.Should().ThrowAsync<InvalidOperationException>()).WithMessage(Refusal + "*");
            await AssertNothingForTheAuditSaveToCommitAsync(dbContext);
        }

        await using var readContext = CreateDbContext(databaseName);
        (await readContext.Set<CurriculumItem>().SingleAsync(item => item.Id == NationalItemId)).RequiredCount.Should().Be(3);
    }

    [Fact]
    public async Task TheRefusal_StatesTheRuleForTheItemsOwner_WithTheSubSpecialityByName()
    {
        await using var dbContext = CreateDbContext(Guid.NewGuid().ToString());
        await SeedAsync(dbContext);

        var national = () => new AddCurriculumItemCommandHandler(dbContext).Handle(
            AddCommand(LocalAHere, TestPrincipals.Administrator()), CancellationToken.None);
        (await national.Should().ThrowAsync<InvalidOperationException>()).WithMessage(
            "This item cannot name the selected EPA. A national curriculum item names a national EPA of General Paediatrics.");

        var local = () => new AddCurriculumItemCommandHandler(dbContext).Handle(
            AddCommand(LocalBHere, TestPrincipals.InstitutionalAdmin(InstitutionA)), CancellationToken.None);
        (await local.Should().ThrowAsync<InvalidOperationException>()).WithMessage(
            "This item cannot name the selected EPA. An institution's own curriculum item names a national EPA of General Paediatrics, or one of that institution's local EPAs of General Paediatrics.");
    }

    // ---- The picker ----

    [Theory]
    [InlineData(Caller.Administrator, null, new[] { "PAED-001", "PAED-009" })]
    [InlineData(Caller.CollegeAdmin, null, new[] { "PAED-001", "PAED-009" })]
    [InlineData(Caller.InstitutionalAdminA, null, new[] { "LOC-A00", "LOC-A01", "PAED-001", "PAED-009" })]
    [InlineData(Caller.InstitutionalAdminB, null, new[] { "LOC-B01", "PAED-001", "PAED-009" })]
    [InlineData(Caller.Administrator, NationalItemId, new[] { "PAED-001", "PAED-009" })]
    [InlineData(Caller.CollegeAdmin, NationalItemId, new[] { "PAED-001", "PAED-009" })]
    [InlineData(Caller.Administrator, LocalItemId, new[] { "LOC-A00", "LOC-A01", "PAED-001", "PAED-009" })]
    [InlineData(Caller.InstitutionalAdminA, LocalItemId, new[] { "LOC-A00", "LOC-A01", "PAED-001", "PAED-009" })]
    public async Task ThePicker_OffersTheEpasTheItemsOwnerMayUse(Caller caller, int? itemId, string[] expectedCodes)
    {
        await using var dbContext = CreateDbContext(Guid.NewGuid().ToString());
        await SeedAsync(dbContext);

        var options = await new ListCurriculumItemEpaOptionsQueryHandler(dbContext).Handle(
            new ListCurriculumItemEpaOptionsQuery(CurriculumId, itemId, Principal(caller)), CancellationToken.None);

        options.Select(option => option.Code).Should().Equal(expectedCodes);
    }

    [Theory]
    [InlineData(Caller.Administrator, null)]
    [InlineData(Caller.CollegeAdmin, null)]
    [InlineData(Caller.InstitutionalAdminA, null)]
    [InlineData(Caller.InstitutionalAdminB, null)]
    [InlineData(Caller.Administrator, NationalItemId)]
    [InlineData(Caller.Administrator, LocalItemId)]
    [InlineData(Caller.InstitutionalAdminA, LocalItemId)]
    public async Task ThePicker_OffersAnEpa_ExactlyWhenTheCommandItFeedsAcceptsIt(Caller caller, int? itemId)
    {
        // The shared predicate, checked from both ends over every EPA there is: what is offered passes the handler's EPA
        // check, and what is not offered is refused by it. A refusal for another reason (the EPA is already on the
        // curriculum) is past the EPA check, so it counts as accepted here.
        IReadOnlySet<int> offered;
        await using (var dbContext = CreateDbContext(Guid.NewGuid().ToString()))
        {
            await SeedAsync(dbContext);
            offered = (await new ListCurriculumItemEpaOptionsQueryHandler(dbContext).Handle(
                    new ListCurriculumItemEpaOptionsQuery(CurriculumId, itemId, Principal(caller)), CancellationToken.None))
                .Select(option => option.Id)
                .ToHashSet();
        }

        offered.Should().NotBeEmpty("guard: an empty picker would agree with a handler that refused everything");

        foreach (var epaId in EveryEpa)
        {
            await using var dbContext = CreateDbContext(Guid.NewGuid().ToString());
            await SeedAsync(dbContext);

            var accepted = await PassesTheEpaCheckAsync(dbContext, caller, itemId, epaId);

            accepted.Should().Be(offered.Contains(epaId), $"EPA {epaId} is {(offered.Contains(epaId) ? "offered" : "not offered")}");
        }
    }

    [Theory]
    // A national item is the College's: no InstitutionalAdmin edits it, and a CollegeAdmin only their own College's.
    [InlineData(Caller.InstitutionalAdminA, NationalItemId)]
    [InlineData(Caller.OtherCollegeAdmin, NationalItemId)]
    [InlineData(Caller.OtherCollegeAdmin, null)]
    // A's local item is A's.
    [InlineData(Caller.InstitutionalAdminB, LocalItemId)]
    [InlineData(Caller.CollegeAdmin, LocalItemId)]
    // No such item. The coarse gate comes before the lookup, as in the Update handler, so a caller from another College
    // is refused alike for an id that exists and one that does not, and cannot tell them apart.
    [InlineData(Caller.OtherCollegeAdmin, 7999)]
    public async Task ThePicker_RefusesACallerTheCommandWouldRefuse(Caller caller, int? itemId)
    {
        await using var dbContext = CreateDbContext(Guid.NewGuid().ToString());
        await SeedAsync(dbContext);

        var act = () => new ListCurriculumItemEpaOptionsQueryHandler(dbContext).Handle(
            new ListCurriculumItemEpaOptionsQuery(CurriculumId, itemId, Principal(caller)), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Theory]
    [InlineData(CurriculumId + 1, NationalItemId, "The requested curriculum was not found.")]
    [InlineData(CurriculumId, 7999, "The requested curriculum item was not found.")]
    public async Task ThePicker_ForAnUnknownCurriculumOrItem_IsNotFound(int curriculumId, int itemId, string message)
    {
        await using var dbContext = CreateDbContext(Guid.NewGuid().ToString());
        await SeedAsync(dbContext);

        var act = () => new ListCurriculumItemEpaOptionsQueryHandler(dbContext).Handle(
            new ListCurriculumItemEpaOptionsQuery(curriculumId, itemId, TestPrincipals.Administrator()), CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>()).WithMessage(message);
    }

    [Fact]
    public async Task ThePicker_ForAnItemOfAnotherCurriculum_IsNotFound()
    {
        // The item is looked up within the curriculum asked about, as the Update handler finds it in curriculum.Items. An
        // item of another curriculum, here institution B's, must not decide whose local EPAs this curriculum's picker lists.
        const int otherCurriculumId = CurriculumId + 100;
        const int otherItemId = 7100;
        await using var dbContext = CreateDbContext(Guid.NewGuid().ToString());
        await SeedAsync(dbContext);
        var otherCurriculum = new Curriculum
        {
            Id = otherCurriculumId,
            SubSpecialityId = HereSubSpecialityId,
            Name = "Paediatric EPA Curriculum",
            Version = "12.0",
            EffectiveFrom = new DateOnly(2027, 1, 1),
            IsActive = true
        };
        otherCurriculum.Items.Add(new CurriculumItem
        {
            Id = otherItemId,
            EpaId = LocalBHere,
            OwningInstitutionId = InstitutionB,
            RequiredCount = 3,
            QuotaPeriod = QuotaPeriod.Semester,
            MinimumLevelOrder = 4,
            WindowMonths = 12
        });
        dbContext.Curricula.Add(otherCurriculum);
        await dbContext.SaveChangesAsync();

        var act = () => new ListCurriculumItemEpaOptionsQueryHandler(dbContext).Handle(
            new ListCurriculumItemEpaOptionsQuery(CurriculumId, otherItemId, TestPrincipals.Administrator()), CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>()).WithMessage("The requested curriculum item was not found.");
    }

    // ---- Remove: the same owner rule ----

    [Theory]
    // Remove shares CurriculumItemEpas.MayWrite with Add, Update and the picker. Its coarse gate lets every
    // InstitutionalAdmin through, so these are refused by the per-item check alone.
    [InlineData(Caller.InstitutionalAdminA, NationalItemId)]
    [InlineData(Caller.InstitutionalAdminB, LocalItemId)]
    [InlineData(Caller.CollegeAdmin, LocalItemId)]
    public async Task RemoveCurriculumItem_ByACallerWhoMayNotWriteTheItem_IsRefused_WithNothingRemoved(Caller caller, int itemId)
    {
        var databaseName = Guid.NewGuid().ToString();
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);
        }

        await using (var dbContext = CreateDbContext(databaseName))
        {
            var act = () => new RemoveCurriculumItemCommandHandler(dbContext).Handle(
                new RemoveCurriculumItemCommand(CurriculumId, itemId, Principal(caller)), CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>();
            await AssertNothingForTheAuditSaveToCommitAsync(dbContext);
        }

        await using var readContext = CreateDbContext(databaseName);
        (await readContext.Set<CurriculumItem>().Select(item => item.Id).ToListAsync())
            .Should().BeEquivalentTo([NationalItemId, LocalItemId]);
    }

    [Theory]
    [InlineData(Caller.CollegeAdmin, NationalItemId)]
    [InlineData(Caller.InstitutionalAdminA, LocalItemId)]
    [InlineData(Caller.Administrator, LocalItemId)]
    public async Task RemoveCurriculumItem_ByTheItemsOwner_RemovesIt(Caller caller, int itemId)
    {
        var databaseName = Guid.NewGuid().ToString();
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);
            await new RemoveCurriculumItemCommandHandler(dbContext).Handle(
                new RemoveCurriculumItemCommand(CurriculumId, itemId, Principal(caller)), CancellationToken.None);
        }

        await using var readContext = CreateDbContext(databaseName);
        (await readContext.Set<CurriculumItem>().AnyAsync(item => item.Id == itemId)).Should().BeFalse();
    }

    // ---- helpers ----

    private static async Task<bool> PassesTheEpaCheckAsync(ApplicationDbContext dbContext, Caller caller, int? itemId, int epaId)
    {
        try
        {
            if (itemId is int id)
            {
                await new UpdateCurriculumItemCommandHandler(dbContext).Handle(UpdateCommand(id, epaId, Principal(caller)), CancellationToken.None);
            }
            else
            {
                await new AddCurriculumItemCommandHandler(dbContext).Handle(AddCommand(epaId, Principal(caller)), CancellationToken.None);
            }

            return true;
        }
        catch (InvalidOperationException exception) when (!exception.Message.StartsWith(Refusal, StringComparison.Ordinal))
        {
            exception.Message.Should().Be("This curriculum already contains the selected EPA.",
                "guard: the only other refusal this seed can meet is the duplicate check, which comes after the EPA check");
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    // The refusal must not name the EPA it refused: a local EPA is its institution's, and the caller may be from elsewhere.
    private static void AssertNamesNothingOf(string message, int epaId)
    {
        var code = EpaCode(epaId);
        if (code is not null)
        {
            message.Should().NotContain(code);
        }

        message.Should().NotContain(epaId.ToString(System.Globalization.CultureInfo.InvariantCulture));
        message.Should().NotContain("Institution B").And.NotContain("Institution A");
    }

    private static string? EpaCode(int epaId) => epaId switch
    {
        CoreEpaId => "PAED-001",
        LocalCoreEpaId => "LOC-A00",
        NationalHere => "PAED-009",
        NationalElsewhere => "NEO-001",
        LocalAHere => "LOC-A01",
        LocalAElsewhere => "LOC-A02",
        LocalBHere => "LOC-B01",
        _ => null
    };

    private static ClaimsPrincipal Principal(Caller caller) => caller switch
    {
        Caller.Administrator => TestPrincipals.Administrator(),
        Caller.CollegeAdmin => TestPrincipals.CollegeAdmin(CollegeId),
        Caller.InstitutionalAdminA => TestPrincipals.InstitutionalAdmin(InstitutionA),
        Caller.InstitutionalAdminB => TestPrincipals.InstitutionalAdmin(InstitutionB),
        Caller.OtherCollegeAdmin => TestPrincipals.CollegeAdmin(OtherCollegeId),
        _ => throw new ArgumentOutOfRangeException(nameof(caller))
    };

    private static async Task AssertNothingForTheAuditSaveToCommitAsync(ApplicationDbContext dbContext)
    {
        dbContext.ChangeTracker.Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Should().BeEmpty("the audit pipeline's catch saves this context, so anything dirty here would be committed");

        (await dbContext.SaveChangesAsync()).Should().Be(0);
    }

    private static AddCurriculumItemCommand AddCommand(int epaId, ClaimsPrincipal principal)
        => new(CurriculumId, epaId, 3, QuotaPeriod.Semester, 4, 12, null, null, null, null, null, false, principal);

    private static UpdateCurriculumItemCommand UpdateCommand(int itemId, int epaId, ClaimsPrincipal principal, int requiredCount = 3)
        => new(CurriculumId, itemId, epaId, requiredCount, QuotaPeriod.Semester, 4, 12, null, null, null, null, null, false, principal);

    private static ApplicationDbContext CreateDbContext(string databaseName)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options);

    private static async Task SeedAsync(ApplicationDbContext dbContext)
    {
        var college = new College { Id = CollegeId, Name = "CPSA", ShortCode = "CPSA" };
        var otherCollege = new College { Id = OtherCollegeId, Name = "CoS", ShortCode = "COS" };
        var speciality = new Speciality { Id = 1, Name = "Paediatrics", CollegeId = CollegeId, College = college };
        var here = new SubSpeciality { Id = HereSubSpecialityId, Name = "General Paediatrics", SpecialityId = 1, Speciality = speciality };
        var elsewhere = new SubSpeciality { Id = ElsewhereSubSpecialityId, Name = "Neonatology", SpecialityId = 1, Speciality = speciality };

        dbContext.Colleges.AddRange(college, otherCollege);
        dbContext.Specialities.Add(speciality);
        dbContext.SubSpecialities.AddRange(here, elsewhere);
        dbContext.Institutions.AddRange(
            new Institution { Id = InstitutionA, Name = "Institution A", ShortCode = "IA", IsActive = true, CreatedOn = DateTime.UtcNow },
            new Institution { Id = InstitutionB, Name = "Institution B", ShortCode = "IB", IsActive = true, CreatedOn = DateTime.UtcNow });

        dbContext.Epas.AddRange(
            new Epa { Id = CoreEpaId, Code = "PAED-001", Title = "On the national core", SubSpecialityId = HereSubSpecialityId },
            new Epa { Id = LocalCoreEpaId, Code = "LOC-A00", Title = "On A's local item", SubSpecialityId = HereSubSpecialityId, OwningInstitutionId = InstitutionA },
            new Epa { Id = NationalHere, Code = "PAED-009", Title = "National, this sub-speciality", SubSpecialityId = HereSubSpecialityId },
            new Epa { Id = NationalElsewhere, Code = "NEO-001", Title = "National, another sub-speciality", SubSpecialityId = ElsewhereSubSpecialityId },
            new Epa { Id = LocalAHere, Code = "LOC-A01", Title = "A's, this sub-speciality", SubSpecialityId = HereSubSpecialityId, OwningInstitutionId = InstitutionA },
            new Epa { Id = LocalAElsewhere, Code = "LOC-A02", Title = "A's, another sub-speciality", SubSpecialityId = ElsewhereSubSpecialityId, OwningInstitutionId = InstitutionA },
            new Epa { Id = LocalBHere, Code = "LOC-B01", Title = "B's, this sub-speciality", SubSpecialityId = HereSubSpecialityId, OwningInstitutionId = InstitutionB });

        var curriculum = new Curriculum
        {
            Id = CurriculumId,
            SubSpecialityId = HereSubSpecialityId,
            SubSpeciality = here,
            Name = "Paediatric EPA Curriculum",
            Version = "11.1",
            EffectiveFrom = new DateOnly(2026, 1, 1),
            IsActive = true
        };

        curriculum.Items.Add(new CurriculumItem
        {
            Id = NationalItemId,
            EpaId = CoreEpaId,
            RequiredCount = 3,
            QuotaPeriod = QuotaPeriod.Semester,
            MinimumLevelOrder = 4,
            WindowMonths = 12
        });

        curriculum.Items.Add(new CurriculumItem
        {
            Id = LocalItemId,
            EpaId = LocalCoreEpaId,
            OwningInstitutionId = InstitutionA,
            RequiredCount = 3,
            QuotaPeriod = QuotaPeriod.Semester,
            MinimumLevelOrder = 4,
            WindowMonths = 12
        });

        dbContext.Curricula.Add(curriculum);
        await dbContext.SaveChangesAsync();
    }
}
