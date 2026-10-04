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
/// T304: the trainee profile page's curriculum picker offers exactly what its commands accept. A profile's save keeps its
/// pinned version or moves into the institution's active adoption for the discipline; an admission is into an active
/// adoption alone. Before T304 both pickers listed every curriculum the caller could open, every one in the catalogue for
/// an Administrator, and the command refused most of them.
/// </summary>
public sealed class TraineeCurriculumChoicesTests
{
    private const int Kgk = 10;
    private const int Other = 11;
    private const int V111 = 3000;
    private const int V112 = 3001;
    private const int Neonatology = 3002;
    private const int OtherInstitutionsOnly = 3003;
    private const int NeverAdopted = 3004;

    [Fact]
    public async Task AProfilePinnedToASupersededVersion_IsOfferedItsPinnedVersion_AndTheActiveAdoptions()
    {
        await using var db = Seeded(profileOn: V111);

        var choices = await new GetTraineeCurriculumChoicesQueryHandler(db).Handle(
            new GetTraineeCurriculumChoicesQuery(1, TestPrincipals.InstitutionalAdmin(Kgk)), CancellationToken.None);

        choices.Select(choice => (choice.Id, choice.Version)).Should().Equal((V111, "11.1"), (V112, "11.2"));
    }

    [Fact]
    public async Task AProfileOnTheActiveAdoption_IsOfferedThatVersionAlone()
    {
        // 11.1 is the institution's superseded adoption: a move back into it is refused, so it is not offered.
        await using var db = Seeded(profileOn: V112);

        var choices = await new GetTraineeCurriculumChoicesQueryHandler(db).Handle(
            new GetTraineeCurriculumChoicesQuery(1, TestPrincipals.Administrator()), CancellationToken.None);

        choices.Select(choice => choice.Id).Should().Equal(V112);
    }

    [Fact]
    public async Task AnotherInstitutionsProfile_IsNotFound()
    {
        await using var db = Seeded(profileOn: V111);

        var act = () => new GetTraineeCurriculumChoicesQueryHandler(db).Handle(
            new GetTraineeCurriculumChoicesQuery(1, TestPrincipals.InstitutionalAdmin(Other)), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("The trainee profile could not be found.");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AnAdmission_IsOfferedTheInstitutionsActiveAdoptionsAlone_AnAdministratorIncluded(bool administrator)
    {
        // Not 11.1 (superseded), not a version only another institution adopted, not one nobody adopted.
        await using var db = Seeded(profileOn: V111);
        var principal = administrator ? TestPrincipals.Administrator() : TestPrincipals.InstitutionalAdmin(Kgk);

        var choices = await new GetAdmissionCurriculumChoicesQueryHandler(db, Users().Object).Handle(
            new GetAdmissionCurriculumChoicesQuery("pending-1", principal), CancellationToken.None);

        choices.Select(choice => choice.Id).Should().BeEquivalentTo([V112, Neonatology]);
    }

    [Fact]
    public async Task AnAdmission_OfAnotherInstitutionsRegistrar_IsRefused()
    {
        await using var db = Seeded(profileOn: V111);

        var act = () => new GetAdmissionCurriculumChoicesQueryHandler(db, Users().Object).Handle(
            new GetAdmissionCurriculumChoicesQuery("pending-1", TestPrincipals.InstitutionalAdmin(Other)), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("The pending trainee could not be found.");
    }

    private static Mock<IUserAdministrationService> Users()
    {
        var users = new Mock<IUserAdministrationService>();
        users.Setup(s => s.GetByIdAsync("pending-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserIdentityDetails("pending-1", "new@kgk", "Ayanda", "Zulu", Kgk, [], [], [WombatRoles.PendingTrainee]));
        return users;
    }

    private static ApplicationDbContext Seeded(int profileOn)
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        db.Set<Institution>().AddRange(
            new Institution { Id = Kgk, Name = "KGK", ShortCode = "KGK", IsActive = true, CreatedOn = DateTime.UtcNow },
            new Institution { Id = Other, Name = "Other", ShortCode = "OTH", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.Set<Speciality>().Add(new Speciality { Id = 1, CollegeId = Kgk, Name = "Paediatrics", IsActive = true });
        db.Set<SubSpeciality>().AddRange(
            new SubSpeciality { Id = 1, SpecialityId = 1, Name = "General Paediatrics", IsActive = true },
            new SubSpeciality { Id = 2, SpecialityId = 1, Name = "Neonatology", IsActive = true },
            new SubSpeciality { Id = 3, SpecialityId = 1, Name = "Cardiology", IsActive = true });
        db.Set<Curriculum>().AddRange(
            new Curriculum { Id = V111, SubSpecialityId = 1, Name = "CPSA Paediatrics", Version = "11.1" },
            new Curriculum { Id = V112, SubSpecialityId = 1, Name = "CPSA Paediatrics", Version = "11.2" },
            new Curriculum { Id = Neonatology, SubSpecialityId = 2, Name = "CPSA Neonatology", Version = "1.0" },
            new Curriculum { Id = OtherInstitutionsOnly, SubSpecialityId = 3, Name = "CPSA Cardiology", Version = "1.0" },
            new Curriculum { Id = NeverAdopted, SubSpecialityId = 1, Name = "CPSA Paediatrics", Version = "12.0" });
        db.Set<InstitutionCurriculumAdoption>().AddRange(
            new InstitutionCurriculumAdoption { Id = 1, InstitutionId = Kgk, CurriculumId = V111, SubSpecialityId = 1, AdoptedOn = new DateOnly(2025, 1, 1), IsActive = false },
            new InstitutionCurriculumAdoption { Id = 2, InstitutionId = Kgk, CurriculumId = V112, SubSpecialityId = 1, AdoptedOn = new DateOnly(2026, 9, 1), IsActive = true },
            new InstitutionCurriculumAdoption { Id = 3, InstitutionId = Kgk, CurriculumId = Neonatology, SubSpecialityId = 2, AdoptedOn = new DateOnly(2025, 1, 1), IsActive = true },
            new InstitutionCurriculumAdoption { Id = 4, InstitutionId = Other, CurriculumId = OtherInstitutionsOnly, SubSpecialityId = 3, AdoptedOn = new DateOnly(2025, 1, 1), IsActive = true });
        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1,
            UserId = "trainee-1",
            InstitutionId = Kgk,
            CurriculumId = profileOn,
            AdoptionId = profileOn == V111 ? 1 : 2,
            ProgrammeStartDate = new DateOnly(2025, 1, 1),
            ExpectedCompletionDate = new DateOnly(2028, 1, 14),
            IsActive = true
        });
        db.SaveChanges();
        return db;
    }
}
