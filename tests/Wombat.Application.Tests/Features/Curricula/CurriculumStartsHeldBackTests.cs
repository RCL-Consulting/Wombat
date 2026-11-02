using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Curricula;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Curricula;

/// <summary>
/// T392 (flow 15, F5; T301's second half): a curriculum version made by the College's create or clone starts held back, so
/// no institution adopts a version still being written; its College opens it (<see cref="UpdateCurriculumCommand" />'s
/// IsActive, steps 6.13, 6.29 and 6.31). The seeders open theirs explicitly, so no seeded value changes.
/// </summary>
public sealed class CurriculumStartsHeldBackTests : IAsyncLifetime
{
    private ApplicationDbContext _db = null!;
    private int _collegeId;
    private int _subSpecialityId;

    public async Task InitializeAsync()
    {
        _db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        var college = new College { Name = "CPSA", ShortCode = "CPSA", IsActive = true, CreatedOn = DateTime.UtcNow };
        _db.Colleges.Add(college);
        await _db.SaveChangesAsync();
        var speciality = new Speciality { CollegeId = college.Id, Name = "Paediatrics", IsActive = true };
        _db.Specialities.Add(speciality);
        await _db.SaveChangesAsync();
        var subSpeciality = new SubSpeciality { SpecialityId = speciality.Id, Name = "Neonatology", IsActive = true };
        _db.SubSpecialities.Add(subSpeciality);
        await _db.SaveChangesAsync();

        _collegeId = college.Id;
        _subSpecialityId = subSpeciality.Id;
    }

    public Task DisposeAsync()
    {
        _db.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task A_CreatedVersion_StartsHeldBack()
    {
        var created = await new CreateCurriculumCommandHandler(_db).Handle(
            new CreateCurriculumCommand(_subSpecialityId, "Neonatology EPA Curriculum", "1.0", new DateOnly(2026, 10, 30), null, TestPrincipals.CollegeAdmin(_collegeId)),
            CancellationToken.None);

        created.IsActive.Should().BeFalse("a new version is closed to adoption until its College opens it (F5)");
        (await _db.Curricula.SingleAsync(entity => entity.Id == created.Id)).IsActive.Should().BeFalse();
    }

    [Fact]
    public void A_ClonedVersion_StartsHeldBack_AndTheVersionClonedFromKeepsItsFlag()
    {
        var open = new Curriculum { Id = 2, SubSpecialityId = _subSpecialityId, Name = "Paediatric EPA Curriculum", Version = "11.1", EffectiveFrom = new DateOnly(2026, 1, 1), IsActive = true };

        var clone = open.CloneAsNewVersion("11.2", new DateOnly(2026, 10, 30), null);

        clone.IsActive.Should().BeFalse("a clone is closed to adoption until its College opens it (F5)");
        open.IsActive.Should().BeTrue();
    }
}
