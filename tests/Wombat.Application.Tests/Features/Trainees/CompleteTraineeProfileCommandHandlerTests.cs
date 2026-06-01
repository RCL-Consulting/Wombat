using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Trainees;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Trainees;

/// <summary>
/// T080 / F-5-4 — marking a trainee complete records the completion date, deactivates the profile,
/// removes the Trainee role (no Alumnus role exists) and emails the graduate; scope-guarded.
/// </summary>
public sealed class CompleteTraineeProfileCommandHandlerTests
{
    private const int InstitutionA = 1;
    private const int InstitutionB = 2;

    [Fact]
    public async Task Handle_RecordsCompletion_RemovesRole_AndEmails()
    {
        await using var db = SeededDb();
        var users = new Mock<IUserAdministrationService>();
        users.Setup(s => s.GetByIdAsync("trainee-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserIdentityDetails("trainee-1", "molefe@kgk", "Lerato", "Molefe", InstitutionA, [], [], [WombatRoles.Trainee]));
        var email = new Mock<IEmailSender>();

        var handler = new CompleteTraineeProfileCommandHandler(db, users.Object, email.Object);

        await handler.Handle(
            new CompleteTraineeProfileCommand(1, new DateOnly(2029, 12, 15), TestPrincipals.Administrator()),
            CancellationToken.None);

        var profile = await db.Set<TraineeProfile>().SingleAsync(p => p.Id == 1);
        profile.CompletedOn.Should().Be(new DateOnly(2029, 12, 15));
        profile.IsActive.Should().BeFalse();
        users.Verify(s => s.RemoveRoleAsync("trainee-1", WombatRoles.Trainee, It.IsAny<CancellationToken>()), Times.Once);
        email.Verify(s => s.SendAsync(It.Is<EmailMessage>(m => m.To == "molefe@kgk" && m.Tags.Contains("graduation")), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RejectsOutOfInstitution_AndDoesNotMutate()
    {
        await using var db = SeededDb();
        var users = new Mock<IUserAdministrationService>();
        var email = new Mock<IEmailSender>();
        var handler = new CompleteTraineeProfileCommandHandler(db, users.Object, email.Object);

        var act = () => handler.Handle(
            new CompleteTraineeProfileCommand(1, new DateOnly(2029, 12, 15), TestPrincipals.InstitutionalAdmin(InstitutionB)),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        (await db.Set<TraineeProfile>().SingleAsync(p => p.Id == 1)).IsActive.Should().BeTrue();
        users.Verify(s => s.RemoveRoleAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        email.Verify(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static ApplicationDbContext SeededDb()
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        db.Set<Institution>().Add(new Institution { Id = InstitutionA, Name = "KGK", ShortCode = "KGK", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.Set<Speciality>().Add(new Speciality { Id = 1, InstitutionId = InstitutionA, Name = "Paediatrics", IsActive = true });
        db.Set<SubSpeciality>().Add(new SubSpeciality { Id = 1, SpecialityId = 1, Name = "General Paediatrics", IsActive = true });
        db.Set<Curriculum>().Add(new Curriculum { Id = 1, SubSpecialityId = 1, Name = "FCPaed(SA) Part 1", Version = "2026.1" });
        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1,
            UserId = "trainee-1",
            CurriculumId = 1,
            ProgrammeStartDate = new DateOnly(2023, 1, 15),
            ExpectedCompletionDate = new DateOnly(2029, 12, 31),
            IsActive = true
        });
        db.SaveChanges();
        return db;
    }
}
