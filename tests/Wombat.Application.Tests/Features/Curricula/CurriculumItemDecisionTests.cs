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
/// T131 slice 2: a curriculum item's entrustment-decision cells (Annexure B's cadence, the body that decides, and whether
/// it is decided as opportunity allows). Every admin path that writes, reads, re-saves or clones an item must carry all
/// three, and refuse what the vocabulary does not hold before it mutates anything.
/// </summary>
/// <remarks>
/// Two of the three fail silently when dropped. A cadence of <see cref="QuotaPeriod.AcademicYear" /> is the enum's zero
/// value, so a path that defaulted it would make every EPA due each year; a dropped body sends EPAs 4 and 5 to the general
/// panel. Nothing reads these fields for routing until slice 3, so only these tests would notice.
/// </remarks>
public sealed class CurriculumItemDecisionTests
{
    private const int CollegeId = 1;
    private const int CurriculumId = 3000;
    private const int NationalItemId = 7000;
    private const int LocalItemId = 7001;
    private const int NeonatalEpaId = 5000;
    private const int LocalEpaId = 5001;
    private const int SpareEpaId = 5002;
    private const int InstitutionId = 40;
    private const string Neonatal = "neonatal";
    private const string NeonatalName = "Neonatal team Clinical Competency Committee";

    // ---- Write, edit, re-save ----

    [Fact]
    public async Task AddCurriculumItem_StoresTheCadenceTheBodyAndTheFlag_WithTheBodyKeyNormalised()
    {
        var databaseName = Guid.NewGuid().ToString();
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);

            var result = await new AddCurriculumItemCommandHandler(dbContext).Handle(
                AddCommand(SpareEpaId, QuotaPeriod.Semester, " Neonatal ", false, TestPrincipals.Administrator()),
                CancellationToken.None);

            var dto = result.Items.Single(item => item.EpaId == SpareEpaId);
            dto.DecisionCadence.Should().Be(QuotaPeriod.Semester);
            dto.DecisionBodyKey.Should().Be(Neonatal);
            dto.DecisionBodyName.Should().Be(NeonatalName);
        }

        // A fresh context: the DTO is built from the tracked entity and would echo the value even if nothing was stored.
        var stored = await ReadItemAsync(databaseName, item => item.EpaId == SpareEpaId);
        stored.DecisionCadence.Should().Be(QuotaPeriod.Semester);
        stored.DecisionBodyKey.Should().Be(Neonatal);
        stored.DecisionIsOpportunistic.Should().BeFalse();
    }

    [Fact]
    public async Task AddCurriculumItem_WithNoCadence_StoresNull_NotTheZeroValue()
    {
        var databaseName = Guid.NewGuid().ToString();
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);

            await new AddCurriculumItemCommandHandler(dbContext).Handle(
                AddCommand(SpareEpaId, null, null, false, TestPrincipals.Administrator()),
                CancellationToken.None);
        }

        var stored = await ReadItemAsync(databaseName, item => item.EpaId == SpareEpaId);
        stored.DecisionCadence.Should().BeNull("no published cadence is not 'each academic year'");
        stored.DecisionBodyKey.Should().BeNull();
    }

    [Fact]
    public async Task UpdateCurriculumItem_ChangesEachCell()
    {
        // The national item starts decided each semester by the neonatal CCC, so every assertion is a real change.
        var databaseName = Guid.NewGuid().ToString();
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);

            var result = await new UpdateCurriculumItemCommandHandler(dbContext).Handle(
                UpdateCommand(NationalItemId, NeonatalEpaId, QuotaPeriod.AcademicYear, null, true, TestPrincipals.Administrator()),
                CancellationToken.None);

            var dto = result.Items.Single(item => item.Id == NationalItemId);
            dto.DecisionBodyKey.Should().BeNull();
            dto.DecisionBodyName.Should().BeNull("the name goes with the key");
        }

        var stored = await ReadItemAsync(databaseName, item => item.Id == NationalItemId);
        stored.DecisionCadence.Should().Be(QuotaPeriod.AcademicYear);
        stored.DecisionBodyKey.Should().BeNull("an edit that never assigned it would leave the neonatal CCC in place");
        stored.DecisionIsOpportunistic.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateCurriculumItem_ThatReSendsTheValuesItsDtoReturned_KeepsEveryCell()
    {
        // What CurriculumItemsEdit does: load, copy the DTO into the form, save unchanged. A DTO or command that dropped a
        // cell, or defaulted the cadence, would change an item nobody meant to change.
        var databaseName = Guid.NewGuid().ToString();
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);
        }

        IReadOnlyList<CurriculumItemDto> loaded;
        await using (var dbContext = CreateDbContext(databaseName))
        {
            loaded = (await new GetCurriculumByIdQueryHandler(dbContext).Handle(
                new GetCurriculumByIdQuery(CurriculumId, TestPrincipals.Administrator()), CancellationToken.None))!.Items;
        }

        foreach (var item in loaded)
        {
            await using var dbContext = CreateDbContext(databaseName);
            await new UpdateCurriculumItemCommandHandler(dbContext).Handle(
                new UpdateCurriculumItemCommand(
                    CurriculumId, item.Id, item.EpaId, item.RequiredCount, item.QuotaPeriod, item.MinimumLevelOrder,
                    item.WindowMonths, item.Weight, item.MinimumLevelByStageJson, item.PermittedToolKeys,
                    item.DecisionCadence, item.DecisionBodyKey, item.DecisionIsOpportunistic,
                    TestPrincipals.Administrator(), item.ScaleId),
                CancellationToken.None);
        }

        var national = await ReadItemAsync(databaseName, item => item.Id == NationalItemId);
        (national.DecisionCadence, national.DecisionBodyKey, national.DecisionIsOpportunistic)
            .Should().Be((QuotaPeriod.Semester, Neonatal, false));

        var local = await ReadItemAsync(databaseName, item => item.Id == LocalItemId);
        (local.DecisionCadence, local.DecisionBodyKey, local.DecisionIsOpportunistic)
            .Should().Be(((QuotaPeriod?)null, (string?)null, false), "a null cadence survives a re-save as null");
    }

    // ---- Refusals, before any mutation ----

    [Fact]
    public async Task UpdateCurriculumItem_RefusesABodyTheVocabularyDoesNotHold_AndWritesNothing()
    {
        var databaseName = Guid.NewGuid().ToString();
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);
        }

        await using (var dbContext = CreateDbContext(databaseName))
        {
            // The request also changes the target and the cadence, so a handler that assigned before checking would
            // leave something dirty for the audit pipeline's save to commit.
            var act = () => new UpdateCurriculumItemCommandHandler(dbContext).Handle(
                UpdateCommand(NationalItemId, NeonatalEpaId, QuotaPeriod.AcademicYear, "paediatric_icu", false, TestPrincipals.Administrator(), requiredCount: 9),
                CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("'paediatric_icu' is not a decision body*");
            await AssertNothingForTheAuditSaveToCommitAsync(dbContext);
        }

        var stored = await ReadItemAsync(databaseName, item => item.Id == NationalItemId);
        stored.RequiredCount.Should().Be(3);
        (stored.DecisionCadence, stored.DecisionBodyKey).Should().Be((QuotaPeriod.Semester, Neonatal));
    }

    [Fact]
    public async Task AddCurriculumItem_RefusesABodyTheVocabularyDoesNotHold_AndAddsNothing()
    {
        var databaseName = Guid.NewGuid().ToString();
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);
        }

        await using (var dbContext = CreateDbContext(databaseName))
        {
            var act = () => new AddCurriculumItemCommandHandler(dbContext).Handle(
                AddCommand(SpareEpaId, QuotaPeriod.Semester, "paediatric_icu", false, TestPrincipals.Administrator()),
                CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("'paediatric_icu' is not a decision body*");
            await AssertNothingForTheAuditSaveToCommitAsync(dbContext);
        }

        await using var readContext = CreateDbContext(databaseName);
        (await readContext.Set<CurriculumItem>().AnyAsync(item => item.EpaId == SpareEpaId)).Should().BeFalse();
    }

    /// <summary>
    /// National items stay read-only to institutions (T091), decision cells included: an institution's committee
    /// structure is its panels (slice 3), never a rewrite of which body the College says decides an EPA.
    /// </summary>
    [Fact]
    public async Task AnInstitutionalAdmin_CannotChangeANationalItemsDecision()
    {
        var databaseName = Guid.NewGuid().ToString();
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);
        }

        await using (var dbContext = CreateDbContext(databaseName))
        {
            var act = () => new UpdateCurriculumItemCommandHandler(dbContext).Handle(
                UpdateCommand(NationalItemId, NeonatalEpaId, QuotaPeriod.Semester, null, false, TestPrincipals.InstitutionalAdmin(InstitutionId)),
                CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>();
            await AssertNothingForTheAuditSaveToCommitAsync(dbContext);
        }

        (await ReadItemAsync(databaseName, item => item.Id == NationalItemId)).DecisionBodyKey.Should().Be(Neonatal);
    }

    /// <summary>
    /// The item editor loads the curriculum through this query. Since T211 an InstitutionalAdmin opens a curriculum their
    /// institution has adopted or keeps items of its own on, with every national item marked as one they may not change,
    /// so the page offers them Edit and Remove, and so a decision, only on their own (<see cref="CurriculumAdminScopeTests" />).
    /// This seed records no adoption: the institution with the local item opens the curriculum for it, and another
    /// institution, with neither, reads it as not found.
    /// </summary>
    [Fact]
    public async Task TheItemEditorsRead_OffersAnInstitutionalAdminOnlyTheirOwnItem_AndAnotherInstitutionNothing()
    {
        const int otherInstitutionId = InstitutionId + 1;
        await using var dbContext = CreateDbContext(Guid.NewGuid().ToString());
        await SeedAsync(dbContext);

        var own = await new GetCurriculumByIdQueryHandler(dbContext).Handle(
            new GetCurriculumByIdQuery(CurriculumId, TestPrincipals.InstitutionalAdmin(InstitutionId)), CancellationToken.None);

        own.Should().NotBeNull();
        own!.Items.ToDictionary(item => item.Id, item => item.CanEdit).Should().BeEquivalentTo(
            new Dictionary<int, bool> { [NationalItemId] = false, [LocalItemId] = true },
            "the national item's decision is the College's to set");

        var other = await new GetCurriculumByIdQueryHandler(dbContext).Handle(
            new GetCurriculumByIdQuery(CurriculumId, TestPrincipals.InstitutionalAdmin(otherInstitutionId)), CancellationToken.None);

        other.Should().BeNull();
    }

    [Fact]
    public async Task AnInstitutionalAdmin_CanSetADecisionOnTheirOwnLocalItem()
    {
        var databaseName = Guid.NewGuid().ToString();
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);

            await new UpdateCurriculumItemCommandHandler(dbContext).Handle(
                UpdateCommand(LocalItemId, LocalEpaId, QuotaPeriod.AcademicYear, null, true, TestPrincipals.InstitutionalAdmin(InstitutionId)),
                CancellationToken.None);
        }

        var stored = await ReadItemAsync(databaseName, item => item.Id == LocalItemId);
        (stored.DecisionCadence, stored.DecisionIsOpportunistic).Should().Be((QuotaPeriod.AcademicYear, true));
    }

    // ---- Validators ----

    [Fact]
    public void Validators_RefuseACadenceTheEnumDoesNotDeclare()
    {
        var add = AddCommand(SpareEpaId, (QuotaPeriod)99, null, false, TestPrincipals.Administrator());
        var update = UpdateCommand(NationalItemId, NeonatalEpaId, (QuotaPeriod)99, null, false, TestPrincipals.Administrator());

        new AddCurriculumItemCommandValidator().Validate(add).Errors
            .Should().ContainSingle().Which.PropertyName.Should().Be(nameof(AddCurriculumItemCommand.DecisionCadence));
        new UpdateCurriculumItemCommandValidator().Validate(update).Errors
            .Should().ContainSingle().Which.PropertyName.Should().Be(nameof(UpdateCurriculumItemCommand.DecisionCadence));
    }

    [Fact]
    public void Validators_RefuseAnOpportunisticDecisionWithNoCadence()
    {
        var add = AddCommand(SpareEpaId, null, null, true, TestPrincipals.Administrator());
        var update = UpdateCommand(NationalItemId, NeonatalEpaId, null, null, true, TestPrincipals.Administrator());

        new AddCurriculumItemCommandValidator().Validate(add).Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("An EPA can be decided as opportunity allows only if it has a decision cadence.");
        new UpdateCurriculumItemCommandValidator().Validate(update).Errors.Should().ContainSingle()
            .Which.PropertyName.Should().Be(nameof(UpdateCurriculumItemCommand.DecisionIsOpportunistic));
    }

    [Fact]
    public void Validators_RefuseABodyKeyLongerThanTheColumn()
    {
        var key = new string('k', DecisionBody.KeyMaxLength + 1);
        var update = UpdateCommand(NationalItemId, NeonatalEpaId, QuotaPeriod.Semester, key, false, TestPrincipals.Administrator());

        new UpdateCurriculumItemCommandValidator().Validate(update).Errors
            .Should().ContainSingle().Which.PropertyName.Should().Be(nameof(UpdateCurriculumItemCommand.DecisionBodyKey));
    }

    /// <summary>The control for the refusals above: each declared cadence, and none, with and without a body.</summary>
    [Theory]
    [InlineData(null, null, false)]
    [InlineData(QuotaPeriod.Semester, Neonatal, false)]
    [InlineData(QuotaPeriod.AcademicYear, null, true)]
    public void Validators_AcceptEveryDeclaredCombination(QuotaPeriod? cadence, string? body, bool opportunistic)
    {
        new AddCurriculumItemCommandValidator().Validate(AddCommand(SpareEpaId, cadence, body, opportunistic, TestPrincipals.Administrator()))
            .IsValid.Should().BeTrue();
        new UpdateCurriculumItemCommandValidator().Validate(UpdateCommand(NationalItemId, NeonatalEpaId, cadence, body, opportunistic, TestPrincipals.Administrator()))
            .IsValid.Should().BeTrue();
    }

    // ---- Reads and clone ----

    [Fact]
    public async Task BothCurriculumReads_CarryEachItemsDecision_WithTheBodysName()
    {
        await using var dbContext = CreateDbContext(Guid.NewGuid().ToString());
        await SeedAsync(dbContext);

        var list = await new GetCurriculaListQueryHandler(dbContext).Handle(
            new GetCurriculaListQuery(TestPrincipals.Administrator()), CancellationToken.None);
        var byId = await new GetCurriculumByIdQueryHandler(dbContext).Handle(
            new GetCurriculumByIdQuery(CurriculumId, TestPrincipals.Administrator()), CancellationToken.None);

        foreach (var items in new[] { list.Single().Items, byId!.Items })
        {
            var national = items.Single(item => item.Id == NationalItemId);
            (national.DecisionCadence, national.DecisionBodyKey, national.DecisionBodyName, national.DecisionIsOpportunistic)
                .Should().Be((QuotaPeriod.Semester, Neonatal, NeonatalName, false));

            var local = items.Single(item => item.Id == LocalItemId);
            local.DecisionCadence.Should().BeNull("the projection must not fill in the zero value");
            local.DecisionBodyName.Should().BeNull();
        }
    }

    [Fact]
    public async Task CloneCurriculumAsNewVersion_KeepsEachNationalItemsDecision()
    {
        var databaseName = Guid.NewGuid().ToString();
        int cloneId;
        await using (var dbContext = CreateDbContext(databaseName))
        {
            await SeedAsync(dbContext);

            var clone = await new CloneCurriculumAsNewVersionCommandHandler(dbContext).Handle(
                new CloneCurriculumAsNewVersionCommand(CurriculumId, "11.2", new DateOnly(2027, 1, 1), null, TestPrincipals.Administrator()),
                CancellationToken.None);
            cloneId = clone.Id;
        }

        var cloned = await ReadItemAsync(databaseName, item => item.CurriculumId == cloneId);
        (cloned.EpaId, cloned.DecisionCadence, cloned.DecisionBodyKey, cloned.DecisionIsOpportunistic)
            .Should().Be((NeonatalEpaId, QuotaPeriod.Semester, Neonatal, false));
    }

    // ---- The DTO each write returns ----
    //
    // The editor redraws the table from the DTO a command returns (CurriculumItemsEdit's RunAsync), so each write must
    // hand back the body's name, not only its key. Each handler runs on a context the seed did not use: on the seeding
    // context the tracked body fills the navigation by fix-up, which hides a load that forgot to include it.

    [Fact]
    public async Task AddCurriculumItem_OnAFreshContext_ReturnsEachBodysName()
    {
        var databaseName = await SeededDatabaseAsync();

        await using var dbContext = CreateDbContext(databaseName);
        var result = await new AddCurriculumItemCommandHandler(dbContext).Handle(
            AddCommand(SpareEpaId, QuotaPeriod.Semester, Neonatal, false, TestPrincipals.Administrator()),
            CancellationToken.None);

        result.Items.Single(item => item.EpaId == SpareEpaId).DecisionBodyName.Should().Be(NeonatalName);
        result.Items.Single(item => item.Id == NationalItemId).DecisionBodyName.Should().Be(NeonatalName);
    }

    [Fact]
    public async Task UpdateCurriculumItem_OnAFreshContext_ReturnsEachBodysName()
    {
        var databaseName = await SeededDatabaseAsync();

        await using var dbContext = CreateDbContext(databaseName);
        var result = await new UpdateCurriculumItemCommandHandler(dbContext).Handle(
            UpdateCommand(LocalItemId, LocalEpaId, QuotaPeriod.Semester, Neonatal, false, TestPrincipals.Administrator()),
            CancellationToken.None);

        result.Items.Single(item => item.Id == LocalItemId).DecisionBodyName.Should().Be(NeonatalName);
        result.Items.Single(item => item.Id == NationalItemId).DecisionBodyName.Should().Be(NeonatalName);
    }

    [Fact]
    public async Task RemoveCurriculumItem_OnAFreshContext_ReturnsTheRemainingBodysName()
    {
        var databaseName = await SeededDatabaseAsync();

        await using var dbContext = CreateDbContext(databaseName);
        var result = await new RemoveCurriculumItemCommandHandler(dbContext).Handle(
            new RemoveCurriculumItemCommand(CurriculumId, LocalItemId, TestPrincipals.Administrator()),
            CancellationToken.None);

        result.Items.Should().ContainSingle().Which.DecisionBodyName.Should().Be(NeonatalName);
    }

    [Fact]
    public async Task CloneCurriculumAsNewVersion_OnAFreshContext_ReturnsTheClonedBodysName()
    {
        var databaseName = await SeededDatabaseAsync();

        await using var dbContext = CreateDbContext(databaseName);
        var clone = await new CloneCurriculumAsNewVersionCommandHandler(dbContext).Handle(
            new CloneCurriculumAsNewVersionCommand(CurriculumId, "11.2", new DateOnly(2027, 1, 1), null, TestPrincipals.Administrator()),
            CancellationToken.None);

        clone.Items.Single(item => item.EpaId == NeonatalEpaId).DecisionBodyName.Should().Be(NeonatalName);
    }

    [Fact]
    public async Task GetDecisionBodies_ListsTheVocabularyByName()
    {
        await using var dbContext = CreateDbContext(Guid.NewGuid().ToString());
        await SeedAsync(dbContext);
        dbContext.DecisionBodies.Add(new DecisionBody { Key = "adolescent", Name = "Adolescent health committee" });
        await dbContext.SaveChangesAsync();

        var bodies = await new GetDecisionBodiesQueryHandler(dbContext).Handle(new GetDecisionBodiesQuery(), CancellationToken.None);

        bodies.Should().Equal(
            new DecisionBodyDto("adolescent", "Adolescent health committee"),
            new DecisionBodyDto(Neonatal, NeonatalName));
    }

    // ---- helpers ----

    private static AddCurriculumItemCommand AddCommand(
        int epaId, QuotaPeriod? cadence, string? bodyKey, bool opportunistic, System.Security.Claims.ClaimsPrincipal principal)
        => new(CurriculumId, epaId, 3, QuotaPeriod.Semester, 4, 12, null, null, null, cadence, bodyKey, opportunistic, principal);

    private static UpdateCurriculumItemCommand UpdateCommand(
        int itemId,
        int epaId,
        QuotaPeriod? cadence,
        string? bodyKey,
        bool opportunistic,
        System.Security.Claims.ClaimsPrincipal principal,
        int requiredCount = 3)
        => new(CurriculumId, itemId, epaId, requiredCount, QuotaPeriod.Semester, 4, 12, null, null, null, cadence, bodyKey, opportunistic, principal);

    /// <summary>
    /// The audit trap (the audit pipeline saves the request's context from its catch): a refused command must leave
    /// nothing dirty, and a save made now must write nothing.
    /// </summary>
    private static async Task AssertNothingForTheAuditSaveToCommitAsync(ApplicationDbContext dbContext)
    {
        dbContext.ChangeTracker.Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Should().BeEmpty("the audit pipeline's catch saves this context, so anything dirty here would be committed");

        (await dbContext.SaveChangesAsync()).Should().Be(0);
        dbContext.ChangeTracker.Clear();
    }

    /// <summary>A seeded database whose seeding context is already disposed, so nothing in it is tracked by the next.</summary>
    private static async Task<string> SeededDatabaseAsync()
    {
        var databaseName = Guid.NewGuid().ToString();
        await using var dbContext = CreateDbContext(databaseName);
        await SeedAsync(dbContext);
        return databaseName;
    }

    private static async Task<CurriculumItem> ReadItemAsync(string databaseName, System.Linq.Expressions.Expression<Func<CurriculumItem, bool>> predicate)
    {
        await using var readContext = CreateDbContext(databaseName);
        return await readContext.Set<CurriculumItem>().AsNoTracking().SingleAsync(predicate);
    }

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
        dbContext.DecisionBodies.Add(new DecisionBody { Key = Neonatal, Name = NeonatalName });

        dbContext.Epas.AddRange(
            new Epa { Id = NeonatalEpaId, Code = "PAED-004", Title = "Managing common neonatal conditions", SubSpecialityId = 1 },
            new Epa { Id = LocalEpaId, Code = "LOC-001", Title = "An institution-local EPA", SubSpecialityId = 1, OwningInstitutionId = InstitutionId },
            new Epa { Id = SpareEpaId, Code = "PAED-005", Title = "An EPA not yet on the curriculum", SubSpecialityId = 1 });

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

        // PAED-004 as Annexure B has it: decided each semester, by the neonatal CCC.
        curriculum.Items.Add(new CurriculumItem
        {
            Id = NationalItemId,
            EpaId = NeonatalEpaId,
            RequiredCount = 3,
            QuotaPeriod = QuotaPeriod.Semester,
            MinimumLevelOrder = 4,
            WindowMonths = 12,
            DecisionCadence = QuotaPeriod.Semester,
            DecisionBodyKey = Neonatal
        });

        // An institution-local addition with no published cadence.
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
