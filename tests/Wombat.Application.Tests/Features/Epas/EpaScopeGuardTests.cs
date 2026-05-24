using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Epas;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Epas;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Epas;

/// <summary>
/// T056.b scope-guard tests: InstitutionalAdmin sees and edits only EPAs scoped to their institution.
/// </summary>
public sealed class EpaScopeGuardTests : IAsyncLifetime
{
    private ApplicationDbContext _db = null!;
    private int _institutionAId;
    private int _institutionBId;
    private int _institutionASubSpecialityId;
    private int _institutionBSubSpecialityId;
    private int _institutionAEpaId;
    private int _institutionBEpaId;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ApplicationDbContext(options);

        var institutionA = new Institution { Name = "A", ShortCode = "A", IsActive = true, CreatedOn = DateTime.UtcNow };
        var institutionB = new Institution { Name = "B", ShortCode = "B", IsActive = true, CreatedOn = DateTime.UtcNow };
        _db.Institutions.AddRange(institutionA, institutionB);
        await _db.SaveChangesAsync();

        var specA = new Speciality { InstitutionId = institutionA.Id, Name = "SpecA", IsActive = true };
        var specB = new Speciality { InstitutionId = institutionB.Id, Name = "SpecB", IsActive = true };
        _db.Specialities.AddRange(specA, specB);
        await _db.SaveChangesAsync();

        var subA = new SubSpeciality { SpecialityId = specA.Id, Name = "SubA", IsActive = true };
        var subB = new SubSpeciality { SpecialityId = specB.Id, Name = "SubB", IsActive = true };
        _db.SubSpecialities.AddRange(subA, subB);
        await _db.SaveChangesAsync();

        var epaA = new Epa { SubSpecialityId = subA.Id, Code = "EPA-A", Title = "EpaA", IsActive = true };
        var epaB = new Epa { SubSpecialityId = subB.Id, Code = "EPA-B", Title = "EpaB", IsActive = true };
        _db.Epas.AddRange(epaA, epaB);
        await _db.SaveChangesAsync();

        _institutionAId = institutionA.Id;
        _institutionBId = institutionB.Id;
        _institutionASubSpecialityId = subA.Id;
        _institutionBSubSpecialityId = subB.Id;
        _institutionAEpaId = epaA.Id;
        _institutionBEpaId = epaB.Id;
    }

    public Task DisposeAsync()
    {
        _db.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ListEpas_InstitutionalAdmin_SeesOnlyOwnInstitutionEpas()
    {
        var handler = new ListEpasForSubSpecialityQueryHandler(_db);
        var result = await handler.Handle(
            new ListEpasForSubSpecialityQuery(TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);
        result.Should().ContainSingle().Which.Id.Should().Be(_institutionAEpaId);
    }

    [Fact]
    public async Task GetEpaById_InstitutionalAdmin_OtherInstitution_ReturnsNull()
    {
        var handler = new GetEpaByIdQueryHandler(_db);
        var result = await handler.Handle(
            new GetEpaByIdQuery(_institutionBEpaId, TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateEpa_InstitutionalAdmin_RejectsOtherInstitutionSubSpeciality()
    {
        var handler = new CreateEpaCommandHandler(_db);
        var act = () => handler.Handle(
            new CreateEpaCommand(_institutionBSubSpecialityId, "EPA-NEW", "title", null, null, EpaCategory.Core, TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task UpdateEpa_InstitutionalAdmin_RejectsOtherInstitutionEpa()
    {
        var handler = new UpdateEpaCommandHandler(_db);
        var act = () => handler.Handle(
            new UpdateEpaCommand(_institutionBEpaId, _institutionBSubSpecialityId, "EPA-B", "Renamed", null, null, EpaCategory.Core, true, TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task DeactivateEpa_InstitutionalAdmin_RejectsOtherInstitutionEpa()
    {
        var handler = new DeactivateEpaCommandHandler(_db);
        var act = () => handler.Handle(
            new DeactivateEpaCommand(_institutionBEpaId, TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
