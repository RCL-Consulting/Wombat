using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Adoptions;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Curricula;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Adoptions;

public sealed class AdoptCurriculumTests
{
    [Fact]
    public async Task Handle_CreatesActiveAdoption()
    {
        await using var dbContext = CreateDbContext();
        Seed(dbContext);

        var handler = new AdoptCurriculumCommandHandler(dbContext);

        var result = await handler.Handle(
            new AdoptCurriculumCommand(10, 3000, TestPrincipals.InstitutionalAdmin(10)),
            CancellationToken.None);

        result.IsActive.Should().BeTrue();
        result.CurriculumId.Should().Be(3000);
        result.SubSpecialityId.Should().Be(1000);

        var adoption = await dbContext.Set<InstitutionCurriculumAdoption>().SingleAsync();
        adoption.InstitutionId.Should().Be(10);
        adoption.CurriculumId.Should().Be(3000);
        adoption.SubSpecialityId.Should().Be(1000);
        adoption.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ReAdoptingNewerVersion_SupersedesPreviousAdoption()
    {
        await using var dbContext = CreateDbContext();
        Seed(dbContext);
        AddCurriculumVersion(dbContext, id: 3001, version: "2027");
        var handler = new AdoptCurriculumCommandHandler(dbContext);

        await handler.Handle(new AdoptCurriculumCommand(10, 3000, TestPrincipals.InstitutionalAdmin(10)), CancellationToken.None);
        await handler.Handle(new AdoptCurriculumCommand(10, 3001, TestPrincipals.InstitutionalAdmin(10)), CancellationToken.None);

        var adoptions = await dbContext.Set<InstitutionCurriculumAdoption>().OrderBy(a => a.Id).ToListAsync();
        adoptions.Should().HaveCount(2);
        adoptions.Single(a => a.CurriculumId == 3000).IsActive.Should().BeFalse();
        adoptions.Single(a => a.CurriculumId == 3001).IsActive.Should().BeTrue();

        // Only one active adoption per (institution, discipline).
        adoptions.Count(a => a.IsActive && a.SubSpecialityId == 1000).Should().Be(1);
    }

    [Fact]
    public async Task Handle_WhenSameVersionAlreadyAdopted_Throws()
    {
        await using var dbContext = CreateDbContext();
        Seed(dbContext);
        var handler = new AdoptCurriculumCommandHandler(dbContext);

        await handler.Handle(new AdoptCurriculumCommand(10, 3000, TestPrincipals.InstitutionalAdmin(10)), CancellationToken.None);

        var act = () => handler.Handle(new AdoptCurriculumCommand(10, 3000, TestPrincipals.InstitutionalAdmin(10)), CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already adopted*");
    }

    [Fact]
    public async Task Handle_WhenInstitutionalAdminOfAnotherInstitution_Throws()
    {
        await using var dbContext = CreateDbContext();
        Seed(dbContext);
        var handler = new AdoptCurriculumCommandHandler(dbContext);

        var act = () => handler.Handle(new AdoptCurriculumCommand(10, 3000, TestPrincipals.InstitutionalAdmin(99)), CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task ListAdoptions_ForInstitutionalAdmin_ReturnsOwnInstitutionOnly()
    {
        await using var dbContext = CreateDbContext();
        Seed(dbContext);
        var handler = new AdoptCurriculumCommandHandler(dbContext);
        await handler.Handle(new AdoptCurriculumCommand(10, 3000, TestPrincipals.InstitutionalAdmin(10)), CancellationToken.None);

        var list = await new ListAdoptionsQueryHandler(dbContext)
            .Handle(new ListAdoptionsQuery(TestPrincipals.InstitutionalAdmin(10)), CancellationToken.None);

        list.Should().ContainSingle();
        list[0].CurriculumName.Should().Be("Cardiology Registrar");
        list[0].CollegeName.Should().Be("College of Physicians");

        var otherInstitution = await new ListAdoptionsQueryHandler(dbContext)
            .Handle(new ListAdoptionsQuery(TestPrincipals.InstitutionalAdmin(99)), CancellationToken.None);
        otherInstitution.Should().BeEmpty();
    }

    private static ApplicationDbContext CreateDbContext()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static void Seed(ApplicationDbContext dbContext)
    {
        dbContext.Institutions.Add(new Institution { Id = 10, Name = "Training Hospital", ShortCode = "TH" });
        dbContext.Colleges.Add(new College { Id = 5, Name = "College of Physicians", ShortCode = "COP" });
        dbContext.Specialities.Add(new Speciality { Id = 100, CollegeId = 5, Name = "Internal Medicine" });
        dbContext.SubSpecialities.Add(new SubSpeciality { Id = 1000, SpecialityId = 100, Name = "Cardiology" });
        dbContext.Curricula.Add(new Curriculum
        {
            Id = 3000,
            SubSpecialityId = 1000,
            Name = "Cardiology Registrar",
            Version = "2026",
            EffectiveFrom = new DateOnly(2026, 1, 1)
        });
        dbContext.SaveChanges();
    }

    private static void AddCurriculumVersion(ApplicationDbContext dbContext, int id, string version)
    {
        dbContext.Curricula.Add(new Curriculum
        {
            Id = id,
            SubSpecialityId = 1000,
            Name = "Cardiology Registrar",
            Version = version,
            EffectiveFrom = new DateOnly(2027, 1, 1)
        });
        dbContext.SaveChanges();
    }
}
