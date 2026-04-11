using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Trainees;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Trainees;

public sealed class AdmitTraineeCommandHandlerTests
{
    [Fact]
    public async Task Handle_CreatesProfileAndPromotesPendingTrainee()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new ApplicationDbContext(options);
        SeedCurriculum(dbContext);

        var userAdministrationService = new Mock<IUserAdministrationService>();
        userAdministrationService
            .Setup(service => service.GetByIdAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserIdentityDetails(
                "user-1",
                "trainee@example.test",
                "Pending",
                "Trainee",
                10,
                [100],
                [1000],
                [WombatRoles.PendingTrainee]));

        var handler = new AdmitTraineeCommandHandler(dbContext, userAdministrationService.Object);

        var result = await handler.Handle(
            new AdmitTraineeCommand(
                "user-1",
                3000,
                new DateOnly(2026, 1, 15),
                null),
            CancellationToken.None);

        result.UserId.Should().Be("user-1");
        result.CurriculumId.Should().Be(3000);
        result.ExpectedCompletionDate.Should().Be(new DateOnly(2028, 1, 15));

        var profile = await dbContext.TraineeProfiles.SingleAsync();
        profile.UserId.Should().Be("user-1");
        profile.CurriculumId.Should().Be(3000);
        profile.IsActive.Should().BeTrue();

        userAdministrationService.Verify(service => service.UpdateScopeAsync("user-1", 10, It.Is<IReadOnlyCollection<int>>(ids => ids.SequenceEqual(new[] { 100 })), It.Is<IReadOnlyCollection<int>>(ids => ids.SequenceEqual(new[] { 1000 })), It.IsAny<CancellationToken>()), Times.Once);
        userAdministrationService.Verify(service => service.PromotePendingTraineeAsync("user-1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotPendingTrainee_ThrowsCleanly()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new ApplicationDbContext(options);
        SeedCurriculum(dbContext);

        var userAdministrationService = new Mock<IUserAdministrationService>();
        userAdministrationService
            .Setup(service => service.GetByIdAsync("user-2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserIdentityDetails(
                "user-2",
                "trainee@example.test",
                "Existing",
                "Trainee",
                10,
                [100],
                [1000],
                [WombatRoles.Trainee]));

        var handler = new AdmitTraineeCommandHandler(dbContext, userAdministrationService.Object);

        var act = () => handler.Handle(
            new AdmitTraineeCommand(
                "user-2",
                3000,
                new DateOnly(2026, 1, 15),
                null),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Only users in the PendingTrainee role can be admitted.");

        dbContext.TraineeProfiles.Should().BeEmpty();
        userAdministrationService.Verify(service => service.PromotePendingTraineeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static void SeedCurriculum(ApplicationDbContext dbContext)
    {
        dbContext.Institutions.Add(new Institution
        {
            Id = 10,
            Name = "Training Hospital",
            ShortCode = "TH"
        });

        dbContext.Specialities.Add(new Speciality
        {
            Id = 100,
            InstitutionId = 10,
            Name = "Internal Medicine"
        });

        dbContext.SubSpecialities.Add(new SubSpeciality
        {
            Id = 1000,
            SpecialityId = 100,
            Name = "Cardiology"
        });

        dbContext.Curricula.Add(new Curriculum
        {
            Id = 3000,
            SubSpecialityId = 1000,
            Name = "Cardiology Registrar",
            Version = "2026",
            EffectiveFrom = new DateOnly(2026, 1, 1),
            Items =
            [
                new CurriculumItem
                {
                    Id = 4000,
                    WindowMonths = 24,
                    RequiredCount = 1,
                    MinimumLevelOrder = 3,
                    EpaId = 5000
                }
            ]
        });

        dbContext.SaveChanges();
    }
}
