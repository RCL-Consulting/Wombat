using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Curricula;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Curricula;

/// <summary>
/// T056.b scope-guard tests: InstitutionalAdmin sees and edits only curricula scoped to their institution.
/// </summary>
public sealed class CurriculumScopeGuardTests : IAsyncLifetime
{
    private ApplicationDbContext _db = null!;
    private int _institutionAId;
    private int _institutionBSubSpecialityId;
    private int _institutionACurriculumId;
    private int _institutionBCurriculumId;

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

        var curriculumA = new Curriculum
        {
            SubSpecialityId = subA.Id,
            Name = "Curriculum A",
            Version = "1.0",
            EffectiveFrom = new DateOnly(2026, 1, 1),
            IsActive = true
        };
        var curriculumB = new Curriculum
        {
            SubSpecialityId = subB.Id,
            Name = "Curriculum B",
            Version = "1.0",
            EffectiveFrom = new DateOnly(2026, 1, 1),
            IsActive = true
        };
        _db.Curricula.AddRange(curriculumA, curriculumB);
        await _db.SaveChangesAsync();

        _institutionAId = institutionA.Id;
        _institutionBSubSpecialityId = subB.Id;
        _institutionACurriculumId = curriculumA.Id;
        _institutionBCurriculumId = curriculumB.Id;
    }

    public Task DisposeAsync()
    {
        _db.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetCurriculaList_InstitutionalAdmin_SeesOnlyOwn()
    {
        var handler = new GetCurriculaListQueryHandler(_db);
        var result = await handler.Handle(
            new GetCurriculaListQuery(TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);
        result.Should().ContainSingle().Which.Id.Should().Be(_institutionACurriculumId);
    }

    [Fact]
    public async Task GetCurriculumById_InstitutionalAdmin_OtherInstitution_ReturnsNull()
    {
        var handler = new GetCurriculumByIdQueryHandler(_db);
        var result = await handler.Handle(
            new GetCurriculumByIdQuery(_institutionBCurriculumId, TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateCurriculum_InstitutionalAdmin_RejectsOtherInstitution()
    {
        var handler = new CreateCurriculumCommandHandler(_db);
        var act = () => handler.Handle(
            new CreateCurriculumCommand(_institutionBSubSpecialityId, "X", "1.0", new DateOnly(2026, 1, 1), null, TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task UpdateCurriculum_InstitutionalAdmin_RejectsOtherInstitution()
    {
        var handler = new UpdateCurriculumCommandHandler(_db);
        var act = () => handler.Handle(
            new UpdateCurriculumCommand(_institutionBCurriculumId, _institutionBSubSpecialityId, "X", "1.0", new DateOnly(2026, 1, 1), null, true, TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task RemoveCurriculumItem_InstitutionalAdmin_RejectsOtherInstitution()
    {
        var handler = new RemoveCurriculumItemCommandHandler(_db);
        var act = () => handler.Handle(
            new RemoveCurriculumItemCommand(_institutionBCurriculumId, 1, TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
