using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Institutions.Commands.UpdateInstitution;
using Wombat.Application.Features.Institutions.Commands.UpdateSpeciality;
using Wombat.Application.Features.Institutions.Queries.GetInstitutionById;
using Wombat.Application.Features.Institutions.Queries.GetInstitutionsList;
using Wombat.Application.Features.Institutions.Queries.GetSpecialitiesForInstitution;
using Wombat.Application.Features.Institutions.Queries.GetSpecialitiesList;
using Wombat.Application.Features.Institutions.Queries.GetSubSpecialitiesList;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Institutions;

/// <summary>
/// Scope-guard tests: InstitutionalAdmin sees and edits only their own institution (T056); specialities and
/// sub-specialities are a national catalogue scoped to a College, so a CollegeAdmin sees only their own (T091).
/// </summary>
public sealed class InstitutionalAdminScopeTests : IAsyncLifetime
{
    private ApplicationDbContext _dbContext = null!;
    private int _institutionAId;
    private int _institutionBId;
    private int _collegeAId;
    private int _collegeBId;
    private int _collegeASpecialityId;
    private int _collegeBSpecialityId;
    private int _collegeASubSpecialityId;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);

        var institutionA = new Institution { Name = "Institution A", ShortCode = "INSTA", IsActive = true, CreatedOn = DateTime.UtcNow };
        var institutionB = new Institution { Name = "Institution B", ShortCode = "INSTB", IsActive = true, CreatedOn = DateTime.UtcNow };
        _dbContext.Institutions.AddRange(institutionA, institutionB);
        await _dbContext.SaveChangesAsync();

        var collegeA = new College { Name = "College A", ShortCode = "COLA", IsActive = true, CreatedOn = DateTime.UtcNow };
        var collegeB = new College { Name = "College B", ShortCode = "COLB", IsActive = true, CreatedOn = DateTime.UtcNow };
        _dbContext.Colleges.AddRange(collegeA, collegeB);
        await _dbContext.SaveChangesAsync();

        _institutionAId = institutionA.Id;
        _institutionBId = institutionB.Id;
        _collegeAId = collegeA.Id;
        _collegeBId = collegeB.Id;

        var specialityA = new Speciality { CollegeId = collegeA.Id, Name = "Speciality A", IsActive = true };
        var specialityB = new Speciality { CollegeId = collegeB.Id, Name = "Speciality B", IsActive = true };
        _dbContext.Set<Speciality>().AddRange(specialityA, specialityB);
        await _dbContext.SaveChangesAsync();

        _collegeASpecialityId = specialityA.Id;
        _collegeBSpecialityId = specialityB.Id;

        var subSpecialityA = new SubSpeciality { SpecialityId = specialityA.Id, Name = "Sub A", IsActive = true };
        _dbContext.Set<SubSpeciality>().Add(subSpecialityA);
        await _dbContext.SaveChangesAsync();

        _collegeASubSpecialityId = subSpecialityA.Id;

        // Institution A has adopted College A's discipline (Sub A). Institution B has adopted nothing.
        // This is what makes College A's national speciality/sub-speciality visible to Institution A's
        // InstitutionalAdmin (T092). CurriculumId is not navigated by the queries under test.
        _dbContext.Set<InstitutionCurriculumAdoption>().Add(new InstitutionCurriculumAdoption
        {
            InstitutionId = institutionA.Id,
            CurriculumId = 1,
            SubSpecialityId = subSpecialityA.Id,
            AdoptedOn = new DateOnly(2026, 1, 15),
            IsActive = true
        });
        await _dbContext.SaveChangesAsync();
    }

    public Task DisposeAsync()
    {
        _dbContext.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetInstitutionsList_Administrator_SeesAll()
    {
        var handler = new GetInstitutionsListQueryHandler(_dbContext);
        var result = await handler.Handle(
            new GetInstitutionsListQuery(TestPrincipals.Administrator()),
            CancellationToken.None);
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetInstitutionsList_InstitutionalAdmin_SeesOnlyOwn()
    {
        var handler = new GetInstitutionsListQueryHandler(_dbContext);
        var result = await handler.Handle(
            new GetInstitutionsListQuery(TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);
        result.Should().ContainSingle().Which.Id.Should().Be(_institutionAId);
    }

    [Fact]
    public async Task GetInstitutionById_InstitutionalAdmin_FromOtherInstitution_ReturnsNull()
    {
        var handler = new GetInstitutionByIdQueryHandler(_dbContext);
        var result = await handler.Handle(
            new GetInstitutionByIdQuery(_institutionBId, TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetSpecialitiesList_CollegeAdmin_SeesOnlyOwn()
    {
        var handler = new GetSpecialitiesListQueryHandler(_dbContext);
        var result = await handler.Handle(
            new GetSpecialitiesListQuery(TestPrincipals.CollegeAdmin(_collegeAId)),
            CancellationToken.None);
        result.Should().ContainSingle().Which.Id.Should().Be(_collegeASpecialityId);
    }

    [Fact]
    public async Task GetSubSpecialitiesList_CollegeAdmin_SeesOnlyOwn()
    {
        var handler = new GetSubSpecialitiesListQueryHandler(_dbContext);
        var result = await handler.Handle(
            new GetSubSpecialitiesListQuery(TestPrincipals.CollegeAdmin(_collegeAId)),
            CancellationToken.None);
        result.Should().ContainSingle().Which.Id.Should().Be(_collegeASubSpecialityId);
    }

    [Fact]
    public async Task GetSpecialitiesList_InstitutionalAdmin_SeesAdoptedDisciplineSpecialities()
    {
        var handler = new GetSpecialitiesListQueryHandler(_dbContext);
        var result = await handler.Handle(
            new GetSpecialitiesListQuery(TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);
        result.Should().ContainSingle().Which.Id.Should().Be(_collegeASpecialityId);
    }

    [Fact]
    public async Task GetSpecialitiesList_InstitutionalAdmin_NoAdoption_SeesEmpty()
    {
        var handler = new GetSpecialitiesListQueryHandler(_dbContext);
        var result = await handler.Handle(
            new GetSpecialitiesListQuery(TestPrincipals.InstitutionalAdmin(_institutionBId)),
            CancellationToken.None);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSubSpecialitiesList_InstitutionalAdmin_SeesAdoptedDisciplines()
    {
        var handler = new GetSubSpecialitiesListQueryHandler(_dbContext);
        var result = await handler.Handle(
            new GetSubSpecialitiesListQuery(TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);
        result.Should().ContainSingle().Which.Id.Should().Be(_collegeASubSpecialityId);
    }

    [Fact]
    public async Task GetSubSpecialitiesList_InstitutionalAdmin_NoAdoption_SeesEmpty()
    {
        var handler = new GetSubSpecialitiesListQueryHandler(_dbContext);
        var result = await handler.Handle(
            new GetSubSpecialitiesListQuery(TestPrincipals.InstitutionalAdmin(_institutionBId)),
            CancellationToken.None);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSpecialitiesForCollege_CollegeAdmin_RejectsOtherCollege()
    {
        var handler = new GetSpecialitiesForInstitutionQueryHandler(_dbContext);
        var result = await handler.Handle(
            new GetSpecialitiesForInstitutionQuery(_collegeBId, TestPrincipals.CollegeAdmin(_collegeAId)),
            CancellationToken.None);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateInstitution_InstitutionalAdmin_RejectsOtherInstitution()
    {
        var handler = new UpdateInstitutionCommandHandler(_dbContext);
        var act = () => handler.Handle(
            new UpdateInstitutionCommand(_institutionBId, "Renamed", "REN", null, true, TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task UpdateSpeciality_CollegeAdmin_RejectsOtherCollegesSpeciality()
    {
        var handler = new UpdateSpecialityCommandHandler(_dbContext);
        var act = () => handler.Handle(
            new UpdateSpecialityCommand(_collegeBSpecialityId, _collegeBId, "Renamed", null, true, TestPrincipals.CollegeAdmin(_collegeAId)),
            CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
