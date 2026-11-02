using Wombat.Domain.Identity;
using Wombat.Application.Common.Security;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Epas;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Epas;

/// <summary>
/// T091 scope-guard tests: EPAs are a national catalogue owned by a College; a CollegeAdmin sees and edits
/// only EPAs scoped to their College. (Replaces the pre-T091 institution-scoped guards.)
/// </summary>
public sealed class EpaScopeGuardTests : IAsyncLifetime
{
    private ApplicationDbContext _db = null!;
    private int _collegeAId;
    private int _collegeBId;
    private int _collegeASubSpecialityId;
    private int _collegeBSubSpecialityId;
    private int _collegeAEpaId;
    private int _collegeBEpaId;
    private int _institutionAId;
    private int _localEpaId;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(options);

        var collegeA = new College { Name = "A", ShortCode = "A", IsActive = true, CreatedOn = DateTime.UtcNow };
        var collegeB = new College { Name = "B", ShortCode = "B", IsActive = true, CreatedOn = DateTime.UtcNow };
        _db.Colleges.AddRange(collegeA, collegeB);
        await _db.SaveChangesAsync();

        var specA = new Speciality { CollegeId = collegeA.Id, Name = "SpecA", IsActive = true };
        var specB = new Speciality { CollegeId = collegeB.Id, Name = "SpecB", IsActive = true };
        _db.Specialities.AddRange(specA, specB);
        await _db.SaveChangesAsync();

        var subA = new SubSpeciality { SpecialityId = specA.Id, Name = "SubA", IsActive = true };
        var subB = new SubSpeciality { SpecialityId = specB.Id, Name = "SubB", IsActive = true };
        _db.SubSpecialities.AddRange(subA, subB);
        await _db.SaveChangesAsync();

        var institutionA = new Institution { Name = "Inst A", ShortCode = "IA", IsActive = true, CreatedOn = DateTime.UtcNow };
        _db.Institutions.Add(institutionA);
        await _db.SaveChangesAsync();

        var epaA = new Epa { SubSpecialityId = subA.Id, Code = "EPA-A", Title = "EpaA", IsActive = true };
        var epaB = new Epa { SubSpecialityId = subB.Id, Code = "EPA-B", Title = "EpaB", IsActive = true };
        // Institution-local extra under College A's discipline, owned by institution A.
        var localEpa = new Epa { SubSpecialityId = subA.Id, OwningInstitutionId = institutionA.Id, Code = "LOC-1", Title = "Local", IsActive = true };
        _db.Epas.AddRange(epaA, epaB, localEpa);
        await _db.SaveChangesAsync();

        // Institution A has adopted College A's curriculum for SubA, so its InstitutionalAdmin sees SubA's
        // national EPAs (national-catalogue visibility is adoption-narrowed in T091 phase 4).
        var curriculumA = new Curriculum { SubSpecialityId = subA.Id, Name = "Curr A", Version = "2026", EffectiveFrom = new DateOnly(2026, 1, 1) };
        _db.Curricula.Add(curriculumA);
        await _db.SaveChangesAsync();
        _db.Set<InstitutionCurriculumAdoption>().Add(new InstitutionCurriculumAdoption
        {
            InstitutionId = institutionA.Id,
            CurriculumId = curriculumA.Id,
            SubSpecialityId = subA.Id,
            AdoptedOn = new DateOnly(2026, 1, 1),
            IsActive = true
        });
        await _db.SaveChangesAsync();

        _collegeAId = collegeA.Id;
        _collegeBId = collegeB.Id;
        _collegeASubSpecialityId = subA.Id;
        _collegeBSubSpecialityId = subB.Id;
        _collegeAEpaId = epaA.Id;
        _collegeBEpaId = epaB.Id;
        _institutionAId = institutionA.Id;
        _localEpaId = localEpa.Id;
    }

    public Task DisposeAsync()
    {
        _db.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ListEpas_CollegeAdmin_SeesOnlyOwnCollegeEpas()
    {
        var handler = new ListEpasForSubSpecialityQueryHandler(_db);
        var result = await handler.Handle(
            new ListEpasForSubSpecialityQuery(TestPrincipals.CollegeAdmin(_collegeAId)),
            CancellationToken.None);
        result.Should().ContainSingle().Which.Id.Should().Be(_collegeAEpaId);
    }

    [Fact]
    public async Task GetEpaById_CollegeAdmin_OtherCollege_ReturnsNull()
    {
        var handler = new GetEpaByIdQueryHandler(_db);
        var result = await handler.Handle(
            new GetEpaByIdQuery(_collegeBEpaId, TestPrincipals.CollegeAdmin(_collegeAId)),
            CancellationToken.None);
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateEpa_CollegeAdmin_RejectsOtherCollegeSubSpeciality()
    {
        var handler = new CreateEpaCommandHandler(_db);
        var act = () => handler.Handle(
            new CreateEpaCommand(_collegeBSubSpecialityId, "EPA-NEW", "title", null, null, EpaCategory.Core, TestPrincipals.CollegeAdmin(_collegeAId)),
            CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task UpdateEpa_CollegeAdmin_RejectsOtherCollegeEpa()
    {
        var handler = new UpdateEpaCommandHandler(_db, new CreditApplier(_db), new EpaCreditLock(_db), new TraineeCreditLock(_db));
        var act = () => handler.Handle(
            new UpdateEpaCommand(_collegeBEpaId, _collegeBSubSpecialityId, "EPA-B", "Renamed", null, null, EpaCategory.Core, true, TestPrincipals.CollegeAdmin(_collegeAId)),
            CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task DeactivateEpa_CollegeAdmin_RejectsOtherCollegeEpa()
    {
        var handler = new DeactivateEpaCommandHandler(_db, new EpaCreditLock(_db));
        var act = () => handler.Handle(
            new DeactivateEpaCommand(_collegeBEpaId, TestPrincipals.CollegeAdmin(_collegeAId)),
            CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    // --- T091 phase 3: institution-local extras ---

    [Fact]
    public async Task CreateEpa_InstitutionalAdmin_CreatesLocalExtra()
    {
        var handler = new CreateEpaCommandHandler(_db);
        var dto = await handler.Handle(
            new CreateEpaCommand(_collegeASubSpecialityId, "LOC-2", "Local 2", null, null, EpaCategory.Core, TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);
        var saved = await _db.Epas.FindAsync(dto.Id);
        saved!.OwningInstitutionId.Should().Be(_institutionAId);
    }

    [Fact]
    public async Task UpdateEpa_InstitutionalAdmin_RejectsNationalEpa()
    {
        var handler = new UpdateEpaCommandHandler(_db, new CreditApplier(_db), new EpaCreditLock(_db), new TraineeCreditLock(_db));
        var act = () => handler.Handle(
            new UpdateEpaCommand(_collegeAEpaId, _collegeASubSpecialityId, "EPA-A", "x", null, null, EpaCategory.Core, true, TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task DeactivateEpa_InstitutionalAdmin_CanDeactivateOwnLocalExtra()
    {
        var handler = new DeactivateEpaCommandHandler(_db, new EpaCreditLock(_db));
        await handler.Handle(
            new DeactivateEpaCommand(_localEpaId, TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);
        var saved = await _db.Epas.FindAsync(_localEpaId);
        saved!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeactivateEpa_InstitutionalAdmin_RejectsOtherInstitutionLocalExtra()
    {
        var handler = new DeactivateEpaCommandHandler(_db, new EpaCreditLock(_db));
        var act = () => handler.Handle(
            new DeactivateEpaCommand(_localEpaId, TestPrincipals.InstitutionalAdmin(_institutionAId + 999)),
            CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task ListEpas_InstitutionalAdmin_SeesNationalPlusOwnLocal()
    {
        var handler = new ListEpasForSubSpecialityQueryHandler(_db);
        var result = await handler.Handle(
            new ListEpasForSubSpecialityQuery(_collegeASubSpecialityId, TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);
        result.Select(e => e.Id).Should().Contain(new[] { _collegeAEpaId, _localEpaId });
    }

    // --- T392 (flow 15, lane C): the by-id read is the list's rule (Q2, Q19), and what each EPA says of its owner ---

    [Fact]
    public async Task GetEpaById_InstitutionalAdmin_NationalEpaOfADisciplineNotAdopted_ReturnsNull()
    {
        // The sweep (T291): Kgosi Kgari typing the Demo College's EPA-001 by id read it as a form until T392.
        var handler = new GetEpaByIdQueryHandler(_db);
        var result = await handler.Handle(
            new GetEpaByIdQuery(_collegeBEpaId, TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);
        result.Should().BeNull("the list does not show it, so the page is Unavailable");
    }

    [Fact]
    public async Task GetEpaById_InstitutionalAdmin_NationalEpaOfAnAdoptedDiscipline_IsReadOnly()
    {
        var handler = new GetEpaByIdQueryHandler(_db);
        var result = await handler.Handle(
            new GetEpaByIdQuery(_collegeAEpaId, TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.MayChange.Should().BeFalse("UpdateEpa admits only the College for a national EPA (S4)");
        result.OwningInstitutionId.Should().BeNull();
        result.OwningInstitutionName.Should().BeNull();
        result.DisciplineName.Should().Be("SpecA / SubA");
    }

    [Fact]
    public async Task GetEpaById_InstitutionalAdmin_OwnEpa_MayChange_AndIsMarkedAsHerInstitutions()
    {
        var handler = new GetEpaByIdQueryHandler(_db);
        var result = await handler.Handle(
            new GetEpaByIdQuery(_localEpaId, TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);

        result!.MayChange.Should().BeTrue();
        result.OwningInstitutionId.Should().Be(_institutionAId);
        result.OwningInstitutionName.Should().Be("Inst A", "the deactivate words name whose trainees it stops measuring (Q11)");
        result.NamesOwner.Should().BeFalse("she reads one institution's own EPAs: hers");
    }

    [Fact]
    public async Task GetEpaById_InstitutionalAdmin_AnotherInstitutionsOwnEpa_ReturnsNull()
    {
        var handler = new GetEpaByIdQueryHandler(_db);
        var result = await handler.Handle(
            new GetEpaByIdQuery(_localEpaId, TestPrincipals.InstitutionalAdmin(_institutionAId + 999)),
            CancellationToken.None);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetEpaById_CollegeAdmin_AnInstitutionsOwnEpa_ReturnsNull()
    {
        var handler = new GetEpaByIdQueryHandler(_db);
        var result = await handler.Handle(
            new GetEpaByIdQuery(_localEpaId, TestPrincipals.CollegeAdmin(_collegeAId)),
            CancellationToken.None);
        result.Should().BeNull("a College does not read an institution's own EPAs, on the list or by id");
    }

    [Fact]
    public async Task ListEpas_Administrator_MayChangeEveryEpa_AndNamesEachOwner()
    {
        var handler = new ListEpasForSubSpecialityQueryHandler(_db);
        var result = await handler.Handle(new ListEpasForSubSpecialityQuery(TestPrincipals.Administrator()), CancellationToken.None);

        result.Should().HaveCount(3).And.OnlyContain(epa => epa.MayChange);
        result.Single(epa => epa.Id == _localEpaId).Should().Match<EpaDto>(epa => epa.NamesOwner && epa.OwningInstitutionName == "Inst A");
    }

    [Fact]
    public async Task EpaList_InstitutionalAdmin_WithAnAdoption_MayCreate_AndReadsHerInstitutionsName()
    {
        var handler = new GetEpaListQueryHandler(_db);
        var result = await handler.Handle(new GetEpaListQuery(TestPrincipals.InstitutionalAdmin(_institutionAId)), CancellationToken.None);

        result.Reader.Should().Be(EpaListReader.Institution);
        result.MayCreate.Should().BeTrue();
        result.InstitutionName.Should().Be("Inst A");
        result.Epas.Select(epa => epa.Id).Should().BeEquivalentTo([_collegeAEpaId, _localEpaId]);
        result.Epas.Single(epa => epa.Id == _collegeAEpaId).MayChange.Should().BeFalse();
        result.Epas.Single(epa => epa.Id == _localEpaId).MayChange.Should().BeTrue();
    }

    [Fact]
    public async Task EpaList_InstitutionalAdmin_WithNoAdoption_MayNotCreate()
    {
        // Q12, T291's 1.19 widening: Create EPA only where the create can be completed.
        var handler = new GetEpaListQueryHandler(_db);
        var result = await handler.Handle(new GetEpaListQuery(TestPrincipals.InstitutionalAdmin(_institutionAId + 999)), CancellationToken.None);

        result.MayCreate.Should().BeFalse();
        result.Epas.Should().BeEmpty();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task EpaList_TheCollegeAndTheAdministrator_MayCreate(bool administrator)
    {
        var principal = administrator ? TestPrincipals.Administrator() : TestPrincipals.CollegeAdmin(_collegeAId);
        var result = await new GetEpaListQueryHandler(_db).Handle(new GetEpaListQuery(principal), CancellationToken.None);

        result.MayCreate.Should().BeTrue();
        result.Reader.Should().Be(administrator ? EpaListReader.Administrator : EpaListReader.College);
        result.InstitutionName.Should().BeNull();
    }

    [Fact]
    public async Task EpaPage_OutOfScope_IsUnavailable_AndReadsNothingElse()
    {
        var result = await new GetEpaPageQueryHandler(_db).Handle(
            new GetEpaPageQuery(_collegeBEpaId, TestPrincipals.InstitutionalAdmin(_institutionAId)), CancellationToken.None);

        result.Epa.Should().BeNull();
        result.Disciplines.Should().BeEmpty();
    }

    [Fact]
    public async Task EpaPage_Create_InstitutionalAdmin_OffersTheAdoptedDisciplines_AndCreatesHerOwn()
    {
        var result = await new GetEpaPageQueryHandler(_db).Handle(
            new GetEpaPageQuery(null, TestPrincipals.InstitutionalAdmin(_institutionAId)), CancellationToken.None);

        result.Disciplines.Should().ContainSingle().Which.Should().Be(new EpaDisciplineDto(_collegeASubSpecialityId, "SpecA / SubA"));
        result.CreatesOwn.Should().BeTrue();
        result.InstitutionName.Should().Be("Inst A");
        result.MayCreate.Should().BeTrue();
    }

    [Fact]
    public async Task EpaPage_AnOwnEpaLeftInADisciplineNoLongerAdopted_KeepsItsDisciplineOffered()
    {
        // F9: such an EPA keeps its Edit, so its form must still offer the discipline it is in.
        var local = await _db.Epas.FindAsync(_localEpaId);
        local!.SubSpecialityId = _collegeBSubSpecialityId;
        await _db.SaveChangesAsync();

        var result = await new GetEpaPageQueryHandler(_db).Handle(
            new GetEpaPageQuery(_localEpaId, TestPrincipals.InstitutionalAdmin(_institutionAId)), CancellationToken.None);

        result.Epa!.MayChange.Should().BeTrue();
        result.Disciplines.Select(discipline => discipline.SubSpecialityId)
            .Should().BeEquivalentTo([_collegeASubSpecialityId, _collegeBSubSpecialityId]);
    }

    // ---- C1 (T392 build review): each claim read only with its role ----

    /// <summary>A sign-in whose roles and claims are given apart, as a real one may carry them (T113: every user has an institution claim).</summary>
    private static ClaimsPrincipal Claims(string[] roles, int? institutionId, int? collegeId)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "c1-user"), new(ClaimTypes.Name, "c1-user") };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        if (institutionId is { } institution)
        {
            claims.Add(new Claim(WombatClaimTypes.InstitutionId, institution.ToString()));
        }

        if (collegeId is { } college)
        {
            claims.Add(new Claim(WombatClaimTypes.CollegeId, college.ToString()));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    [Fact]
    public async Task ACollegeAdmin_CarryingAnInstitutionClaim_ReadsNoneOfThatInstitutionsOwnEpas()
    {
        var principal = Claims([WombatRoles.CollegeAdmin], _institutionAId, _collegeAId);

        var byId = await new GetEpaByIdQueryHandler(_db).Handle(new GetEpaByIdQuery(_localEpaId, principal), CancellationToken.None);
        var list = await new GetEpaListQueryHandler(_db).Handle(new GetEpaListQuery(principal), CancellationToken.None);

        byId.Should().BeNull();
        list.Epas.Select(epa => epa.Id).Should().BeEquivalentTo([_collegeAEpaId]);
    }

    [Fact]
    public async Task AnAssessor_CarryingAnInstitutionClaim_ReadsNoEpa()
    {
        var principal = Claims([WombatRoles.Assessor], _institutionAId, collegeId: null);

        var national = await new GetEpaByIdQueryHandler(_db).Handle(new GetEpaByIdQuery(_collegeAEpaId, principal), CancellationToken.None);
        var own = await new GetEpaByIdQueryHandler(_db).Handle(new GetEpaByIdQuery(_localEpaId, principal), CancellationToken.None);
        var list = await new GetEpaListQueryHandler(_db).Handle(new GetEpaListQuery(principal), CancellationToken.None);

        national.Should().BeNull();
        own.Should().BeNull();
        list.Epas.Should().BeEmpty();
    }

    [Fact]
    public async Task AnInstitutionalAdmin_CarryingAStaleCollegeClaim_IsOfferedTheAdoptedDisciplinesOnly()
    {
        var principal = Claims([WombatRoles.InstitutionalAdmin], _institutionAId, _collegeBId);

        var page = await new GetEpaPageQueryHandler(_db).Handle(new GetEpaPageQuery(null, principal), CancellationToken.None);
        var list = await new GetEpaListQueryHandler(_db).Handle(new GetEpaListQuery(principal), CancellationToken.None);

        page.Disciplines.Select(discipline => discipline.SubSpecialityId).Should().BeEquivalentTo([_collegeASubSpecialityId]);
        list.Epas.Select(epa => epa.Id).Should().BeEquivalentTo([_collegeAEpaId, _localEpaId]);
    }
}
