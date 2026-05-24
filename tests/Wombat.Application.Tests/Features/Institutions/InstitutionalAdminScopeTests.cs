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
/// T056 scope-guard tests: InstitutionalAdmin sees and edits only their own institution.
/// </summary>
public sealed class InstitutionalAdminScopeTests : IAsyncLifetime
{
    private ApplicationDbContext _dbContext = null!;
    private int _institutionAId;
    private int _institutionBId;
    private int _institutionASpecialityId;
    private int _institutionBSpecialityId;
    private int _institutionASubSpecialityId;

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

        _institutionAId = institutionA.Id;
        _institutionBId = institutionB.Id;

        var specialityA = new Speciality { InstitutionId = institutionA.Id, Name = "Speciality A", IsActive = true };
        var specialityB = new Speciality { InstitutionId = institutionB.Id, Name = "Speciality B", IsActive = true };
        _dbContext.Set<Speciality>().AddRange(specialityA, specialityB);
        await _dbContext.SaveChangesAsync();

        _institutionASpecialityId = specialityA.Id;
        _institutionBSpecialityId = specialityB.Id;

        var subSpecialityA = new SubSpeciality { SpecialityId = specialityA.Id, Name = "Sub A", IsActive = true };
        _dbContext.Set<SubSpeciality>().Add(subSpecialityA);
        await _dbContext.SaveChangesAsync();

        _institutionASubSpecialityId = subSpecialityA.Id;
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
    public async Task GetSpecialitiesList_InstitutionalAdmin_SeesOnlyOwn()
    {
        var handler = new GetSpecialitiesListQueryHandler(_dbContext);
        var result = await handler.Handle(
            new GetSpecialitiesListQuery(TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);
        result.Should().ContainSingle().Which.Id.Should().Be(_institutionASpecialityId);
    }

    [Fact]
    public async Task GetSubSpecialitiesList_InstitutionalAdmin_SeesOnlyOwn()
    {
        var handler = new GetSubSpecialitiesListQueryHandler(_dbContext);
        var result = await handler.Handle(
            new GetSubSpecialitiesListQuery(TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);
        result.Should().ContainSingle().Which.Id.Should().Be(_institutionASubSpecialityId);
    }

    [Fact]
    public async Task GetSpecialitiesForInstitution_InstitutionalAdmin_RejectsOtherInstitution()
    {
        var handler = new GetSpecialitiesForInstitutionQueryHandler(_dbContext);
        var result = await handler.Handle(
            new GetSpecialitiesForInstitutionQuery(_institutionBId, TestPrincipals.InstitutionalAdmin(_institutionAId)),
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
    public async Task UpdateSpeciality_InstitutionalAdmin_RejectsOtherInstitutionsSpeciality()
    {
        var handler = new UpdateSpecialityCommandHandler(_dbContext);
        var act = () => handler.Handle(
            new UpdateSpecialityCommand(_institutionBSpecialityId, _institutionBId, "Renamed", null, true, TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
