using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Curricula;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Curricula;

/// <summary>
/// T091 scope-guard tests: curricula are a national catalogue owned by a College; a CollegeAdmin sees and
/// edits only curricula scoped to their College. (Replaces the pre-T091 institution-scoped guards.)
/// </summary>
public sealed class CurriculumScopeGuardTests : IAsyncLifetime
{
    private ApplicationDbContext _db = null!;
    private int _collegeAId;
    private int _collegeBSubSpecialityId;
    private int _collegeACurriculumId;
    private int _collegeBCurriculumId;

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

        _collegeAId = collegeA.Id;
        _collegeBSubSpecialityId = subB.Id;
        _collegeACurriculumId = curriculumA.Id;
        _collegeBCurriculumId = curriculumB.Id;
    }

    public Task DisposeAsync()
    {
        _db.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetCurriculaList_CollegeAdmin_SeesOnlyOwn()
    {
        var handler = new GetCurriculaListQueryHandler(_db);
        var result = await handler.Handle(
            new GetCurriculaListQuery(TestPrincipals.CollegeAdmin(_collegeAId)),
            CancellationToken.None);
        result.Should().ContainSingle().Which.Id.Should().Be(_collegeACurriculumId);
    }

    [Fact]
    public async Task GetCurriculumById_CollegeAdmin_OtherCollege_ReturnsNull()
    {
        var handler = new GetCurriculumByIdQueryHandler(_db);
        var result = await handler.Handle(
            new GetCurriculumByIdQuery(_collegeBCurriculumId, TestPrincipals.CollegeAdmin(_collegeAId)),
            CancellationToken.None);
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateCurriculum_CollegeAdmin_RejectsOtherCollege()
    {
        var handler = new CreateCurriculumCommandHandler(_db);
        var act = () => handler.Handle(
            new CreateCurriculumCommand(_collegeBSubSpecialityId, "X", "1.0", new DateOnly(2026, 1, 1), null, TestPrincipals.CollegeAdmin(_collegeAId)),
            CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task UpdateCurriculum_CollegeAdmin_RejectsOtherCollege()
    {
        var handler = new UpdateCurriculumCommandHandler(_db);
        var act = () => handler.Handle(
            new UpdateCurriculumCommand(_collegeBCurriculumId, _collegeBSubSpecialityId, "X", "1.0", new DateOnly(2026, 1, 1), null, true, TestPrincipals.CollegeAdmin(_collegeAId)),
            CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task RemoveCurriculumItem_CollegeAdmin_RejectsOtherCollege()
    {
        var handler = new RemoveCurriculumItemCommandHandler(_db);
        var act = () => handler.Handle(
            new RemoveCurriculumItemCommand(_collegeBCurriculumId, 1, TestPrincipals.CollegeAdmin(_collegeAId)),
            CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
