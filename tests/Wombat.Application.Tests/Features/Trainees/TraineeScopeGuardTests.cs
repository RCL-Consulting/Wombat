using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Trainees;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Trainees;

/// <summary>
/// T056.d scope-guard tests: InstitutionalAdmin sees only trainees in their institution.
/// </summary>
public sealed class TraineeScopeGuardTests : IAsyncLifetime
{
    private ApplicationDbContext _db = null!;
    private Mock<IUserAdministrationService> _users = null!;
    private int _institutionAId;
    private int _institutionBProfileId;

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

        var specA = new Speciality { CollegeId = institutionA.Id, Name = "SpecA", IsActive = true };
        var specB = new Speciality { CollegeId = institutionB.Id, Name = "SpecB", IsActive = true };
        _db.Specialities.AddRange(specA, specB);
        await _db.SaveChangesAsync();

        var subA = new SubSpeciality { SpecialityId = specA.Id, Name = "SubA", IsActive = true };
        var subB = new SubSpeciality { SpecialityId = specB.Id, Name = "SubB", IsActive = true };
        _db.SubSpecialities.AddRange(subA, subB);
        await _db.SaveChangesAsync();

        var curriculumB = new Curriculum
        {
            SubSpecialityId = subB.Id,
            Name = "Curriculum B",
            Version = "1.0",
            EffectiveFrom = new DateOnly(2026, 1, 1),
            IsActive = true
        };
        _db.Curricula.Add(curriculumB);
        await _db.SaveChangesAsync();

        var profileB = new TraineeProfile
        {
            UserId = "user-b",
            InstitutionId = institutionB.Id,
            CurriculumId = curriculumB.Id,
            ProgrammeStartDate = new DateOnly(2026, 1, 15),
            ExpectedCompletionDate = new DateOnly(2028, 1, 15),
            IsActive = true
        };
        _db.TraineeProfiles.Add(profileB);
        await _db.SaveChangesAsync();

        _institutionAId = institutionA.Id;
        _institutionBProfileId = profileB.Id;

        _users = new Mock<IUserAdministrationService>();
        _users.Setup(s => s.GetByIdAsync("user-b", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserIdentityDetails("user-b", "b@x", "B", "User", institutionB.Id, Array.Empty<int>(), Array.Empty<int>(), new[] { WombatRoles.Trainee }));
    }

    public Task DisposeAsync()
    {
        _db.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetTraineeProfileById_InstitutionalAdmin_OtherInstitution_Throws()
    {
        var handler = new GetTraineeProfileByIdQueryHandler(_db, _users.Object);
        var act = () => handler.Handle(
            new GetTraineeProfileByIdQuery(_institutionBProfileId, TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task UpdateTraineeProfile_InstitutionalAdmin_RejectsOtherInstitution()
    {
        var handler = new UpdateTraineeProfileCommandHandler(_db, _users.Object);
        var act = () => handler.Handle(
            new UpdateTraineeProfileCommand(_institutionBProfileId, 1, new DateOnly(2026, 2, 1), null, TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task DeactivateTraineeProfile_InstitutionalAdmin_RejectsOtherInstitution()
    {
        var handler = new DeactivateTraineeProfileCommandHandler(_db);
        var act = () => handler.Handle(
            new DeactivateTraineeProfileCommand(_institutionBProfileId, TestPrincipals.InstitutionalAdmin(_institutionAId)),
            CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
