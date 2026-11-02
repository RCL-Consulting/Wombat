using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Adoptions;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
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
            new AdoptCurriculumCommand(10, 3000, null, TestPrincipals.InstitutionalAdmin(10)),
            CancellationToken.None);

        result.IsActive.Should().BeTrue();
        result.CurriculumId.Should().Be(3000);
        result.SubSpecialityId.Should().Be(1000);
        result.SupersededCurriculumId.Should().BeNull("a first adoption of the discipline supersedes nothing");
        result.SupersededRegistrarCount.Should().Be(0);

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

        await handler.Handle(new AdoptCurriculumCommand(10, 3000, null, TestPrincipals.InstitutionalAdmin(10)), CancellationToken.None);
        await handler.Handle(new AdoptCurriculumCommand(10, 3001, 3000, TestPrincipals.InstitutionalAdmin(10)), CancellationToken.None);

        var adoptions = await dbContext.Set<InstitutionCurriculumAdoption>().OrderBy(a => a.Id).ToListAsync();
        adoptions.Should().HaveCount(2);
        adoptions.Single(a => a.CurriculumId == 3000).IsActive.Should().BeFalse();
        adoptions.Single(a => a.CurriculumId == 3001).IsActive.Should().BeTrue();

        // Only one active adoption per (institution, discipline).
        adoptions.Count(a => a.IsActive && a.SubSpecialityId == 1000).Should().Be(1);
    }

    // T392 (Q6, S1): the result names what it superseded and how many registrars stay on it: the institution's ACTIVE
    // profiles pinned to it, not an ended one, nor another institution's.
    [Fact]
    public async Task Handle_Superseding_SaysWhatItSupersededAndHowManyActiveRegistrarsStayOnIt()
    {
        await using var dbContext = CreateDbContext();
        Seed(dbContext);
        AddCurriculumVersion(dbContext, id: 3001, version: "2027");
        var handler = new AdoptCurriculumCommandHandler(dbContext);
        await handler.Handle(new AdoptCurriculumCommand(10, 3000, null, TestPrincipals.InstitutionalAdmin(10)), CancellationToken.None);
        AddProfile(dbContext, 1, institutionId: 10, curriculumId: 3000, isActive: true);
        AddProfile(dbContext, 2, institutionId: 10, curriculumId: 3000, isActive: true);
        AddProfile(dbContext, 3, institutionId: 10, curriculumId: 3000, isActive: false);
        AddProfile(dbContext, 4, institutionId: 11, curriculumId: 3000, isActive: true);

        var result = await handler.Handle(new AdoptCurriculumCommand(10, 3001, 3000, TestPrincipals.InstitutionalAdmin(10)), CancellationToken.None);

        result.SupersededCurriculumId.Should().Be(3000);
        result.SupersededName.Should().Be("Cardiology Registrar");
        result.SupersededVersion.Should().Be("2026");
        result.SupersededRegistrarCount.Should().Be(2);
    }

    // T392 (Q7, Q21): going back to an older version is a re-adoption like any other.
    [Fact]
    public async Task Handle_GoingBackToAnOlderVersion_IsAccepted_AndSupersedesTheNewer()
    {
        await using var dbContext = CreateDbContext();
        Seed(dbContext);
        AddCurriculumVersion(dbContext, id: 3001, version: "2027");
        var handler = new AdoptCurriculumCommandHandler(dbContext);
        await handler.Handle(new AdoptCurriculumCommand(10, 3000, null, TestPrincipals.InstitutionalAdmin(10)), CancellationToken.None);
        await handler.Handle(new AdoptCurriculumCommand(10, 3001, 3000, TestPrincipals.InstitutionalAdmin(10)), CancellationToken.None);

        var result = await handler.Handle(new AdoptCurriculumCommand(10, 3000, 3001, TestPrincipals.InstitutionalAdmin(10)), CancellationToken.None);

        result.SupersededVersion.Should().Be("2027");
        var adoptions = await dbContext.Set<InstitutionCurriculumAdoption>().OrderBy(a => a.Id).ToListAsync();
        adoptions.Select(a => (a.CurriculumId, a.IsActive)).Should().Equal((3000, false), (3001, false), (3000, true));
    }

    [Fact]
    public async Task Handle_WhenSameVersionAlreadyAdopted_Throws()
    {
        await using var dbContext = CreateDbContext();
        Seed(dbContext);
        var handler = new AdoptCurriculumCommandHandler(dbContext);

        await handler.Handle(new AdoptCurriculumCommand(10, 3000, null, TestPrincipals.InstitutionalAdmin(10)), CancellationToken.None);

        var act = () => handler.Handle(new AdoptCurriculumCommand(10, 3000, 3000, TestPrincipals.InstitutionalAdmin(10)), CancellationToken.None);
        (await act.Should().ThrowAsync<AdoptionRefusedException>().WithMessage("*already adopted*"))
            .Which.Reason.Should().Be(AdoptionRefusal.AlreadyAdopted);
    }

    // C4 (T392 build review): the dialog named what the adoption supersedes; another adoption since makes it stale, and
    // nothing is adopted, nothing staged.
    [Theory]
    [InlineData(null)]   // The page read no adoption; another admin adopted 3000 since.
    [InlineData(3001)]   // The page named 3001; the active adoption is 3000.
    public async Task Handle_WhenTheActiveAdoptionIsNotTheOneThePageNamed_IsRefusedAsStale_WithNothingWritten(int? expected)
    {
        await using var dbContext = CreateDbContext();
        Seed(dbContext);
        AddCurriculumVersion(dbContext, id: 3001, version: "2027");
        AddCurriculumVersion(dbContext, id: 3002, version: "2028");
        var handler = new AdoptCurriculumCommandHandler(dbContext);
        await handler.Handle(new AdoptCurriculumCommand(10, 3000, null, TestPrincipals.InstitutionalAdmin(10)), CancellationToken.None);
        dbContext.ChangeTracker.Clear();

        var act = () => handler.Handle(new AdoptCurriculumCommand(10, 3002, expected, TestPrincipals.InstitutionalAdmin(10)), CancellationToken.None);

        var refused = await act.Should().ThrowAsync<AdoptionRefusedException>();
        refused.Which.Reason.Should().Be(AdoptionRefusal.Stale);
        refused.Which.Message.Should().Be("Another adoption was made since this page was read, so nothing was adopted.");
        dbContext.ChangeTracker.Entries().Should().OnlyContain(entry => entry.State == EntityState.Unchanged,
            "nothing is staged before the refusal (T201)");
        (await dbContext.Set<InstitutionCurriculumAdoption>().AsNoTracking().SingleAsync())
            .Should().Match<InstitutionCurriculumAdoption>(adoption => adoption.CurriculumId == 3000 && adoption.IsActive);
    }

    [Fact]
    public async Task Handle_WhenTheVersionDoesNotExist_IsRefusedAsNotFound()
    {
        await using var dbContext = CreateDbContext();
        Seed(dbContext);

        var act = () => new AdoptCurriculumCommandHandler(dbContext)
            .Handle(new AdoptCurriculumCommand(10, 9999, null, TestPrincipals.InstitutionalAdmin(10)), CancellationToken.None);

        (await act.Should().ThrowAsync<AdoptionRefusedException>()).Which.Reason.Should().Be(AdoptionRefusal.NotFound);
    }

    // T301 (T392, Q5): a version its College holds back is refused before anything is written, the institution's active
    // adoption of the discipline untouched. Before T392 only the picker left it out.
    [Fact]
    public async Task Handle_AHeldBackVersion_IsRefused_WithNothingWritten()
    {
        await using var dbContext = CreateDbContext();
        Seed(dbContext);
        AddCurriculumVersion(dbContext, id: 3001, version: "2027", isActive: false);
        var handler = new AdoptCurriculumCommandHandler(dbContext);
        await handler.Handle(new AdoptCurriculumCommand(10, 3000, null, TestPrincipals.InstitutionalAdmin(10)), CancellationToken.None);
        dbContext.ChangeTracker.Clear();

        var act = () => handler.Handle(new AdoptCurriculumCommand(10, 3001, 3000, TestPrincipals.InstitutionalAdmin(10)), CancellationToken.None);

        var refused = await act.Should().ThrowAsync<AdoptionRefusedException>();
        refused.Which.Reason.Should().Be(AdoptionRefusal.HeldBack);
        refused.Which.Message.Should().Be(
            "This curriculum version is not open for adoption. Its College has held it back, so nothing was adopted.");
        dbContext.ChangeTracker.Entries().Should().OnlyContain(entry => entry.State == EntityState.Unchanged,
            "nothing is staged before the refusal, so the audit pipeline commits nothing of it (T201)");
        (await dbContext.Set<InstitutionCurriculumAdoption>().AsNoTracking().SingleAsync())
            .Should().Match<InstitutionCurriculumAdoption>(adoption => adoption.CurriculumId == 3000 && adoption.IsActive);
    }

    // T325 (T392): 22:30 UTC is already the next day in South Africa, and the adoption is dated it.
    [Fact]
    public async Task Handle_DatesTheAdoptionByTheSouthAfricanDay()
    {
        await using var dbContext = CreateDbContext();
        Seed(dbContext);
        var lateEvening = new FixedClock(new DateTimeOffset(2026, 10, 29, 22, 30, 0, TimeSpan.Zero));

        var result = await new AdoptCurriculumCommandHandler(dbContext, lateEvening)
            .Handle(new AdoptCurriculumCommand(10, 3000, null, TestPrincipals.InstitutionalAdmin(10)), CancellationToken.None);

        result.AdoptedOn.Should().Be(new DateOnly(2026, 10, 30));
        (await dbContext.Set<InstitutionCurriculumAdoption>().SingleAsync()).AdoptedOn.Should().Be(new DateOnly(2026, 10, 30));
    }

    [Fact]
    public async Task Handle_WhenInstitutionalAdminOfAnotherInstitution_Throws()
    {
        await using var dbContext = CreateDbContext();
        Seed(dbContext);
        var handler = new AdoptCurriculumCommandHandler(dbContext);

        var act = () => handler.Handle(new AdoptCurriculumCommand(10, 3000, null, TestPrincipals.InstitutionalAdmin(99)), CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private static ApplicationDbContext CreateDbContext()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static void Seed(ApplicationDbContext dbContext)
    {
        dbContext.Institutions.Add(new Institution { Id = 10, Name = "Training Hospital", ShortCode = "TH" });
        dbContext.Institutions.Add(new Institution { Id = 11, Name = "Other Hospital", ShortCode = "OH" });
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

    private static void AddCurriculumVersion(ApplicationDbContext dbContext, int id, string version, bool isActive = true)
    {
        dbContext.Curricula.Add(new Curriculum
        {
            Id = id,
            SubSpecialityId = 1000,
            Name = "Cardiology Registrar",
            Version = version,
            EffectiveFrom = new DateOnly(2027, 1, 1),
            IsActive = isActive
        });
        dbContext.SaveChanges();
    }

    private static void AddProfile(ApplicationDbContext dbContext, int id, int institutionId, int curriculumId, bool isActive)
    {
        dbContext.TraineeProfiles.Add(new TraineeProfile
        {
            Id = id,
            UserId = $"trainee-{id}",
            InstitutionId = institutionId,
            CurriculumId = curriculumId,
            ProgrammeStartDate = new DateOnly(2026, 1, 15),
            ExpectedCompletionDate = new DateOnly(2030, 1, 15),
            IsActive = isActive
        });
        dbContext.SaveChanges();
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
