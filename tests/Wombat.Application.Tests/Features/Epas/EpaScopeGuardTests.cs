using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Epas;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Epas;
using Wombat.Domain.Institutions;
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
        var handler = new UpdateEpaCommandHandler(_db);
        var act = () => handler.Handle(
            new UpdateEpaCommand(_collegeBEpaId, _collegeBSubSpecialityId, "EPA-B", "Renamed", null, null, EpaCategory.Core, true, TestPrincipals.CollegeAdmin(_collegeAId)),
            CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task DeactivateEpa_CollegeAdmin_RejectsOtherCollegeEpa()
    {
        var handler = new DeactivateEpaCommandHandler(_db);
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
        var handler = new UpdateEpaCommandHandler(_db);
        var act = () => handler.Handle(
            new UpdateEpaCommand(_collegeAEpaId, _collegeASubSpecialityId, "EPA-A", "x", null, null, EpaCategory.Core, true, TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task DeactivateEpa_InstitutionalAdmin_CanDeactivateOwnLocalExtra()
    {
        var handler = new DeactivateEpaCommandHandler(_db);
        await handler.Handle(
            new DeactivateEpaCommand(_localEpaId, TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);
        var saved = await _db.Epas.FindAsync(_localEpaId);
        saved!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeactivateEpa_InstitutionalAdmin_RejectsOtherInstitutionLocalExtra()
    {
        var handler = new DeactivateEpaCommandHandler(_db);
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
}
