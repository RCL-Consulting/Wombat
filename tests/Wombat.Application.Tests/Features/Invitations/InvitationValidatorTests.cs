using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Invitations;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Invitations;

/// <summary>
/// T060: target-role scope rules in <see cref="InvitationRules.ValidateScope"/> are exercised
/// through <see cref="IssueInvitationCommandHandler"/>. Rules that are not satisfied
/// surface as an <see cref="InvalidOperationException"/> from the handler.
/// </summary>
public sealed class InvitationValidatorTests
{
    private readonly IInvitationTokenService _tokenService = new InvitationTokenService();

    [Fact]
    public async Task Coordinator_WithNoSpeciality_IsAccepted()
    {
        await using var db = CreateDbWithInstitution(institutionId: 1);
        var handler = CreateHandler(db);

        var command = new IssueInvitationCommand(
            Email: "coord@example.test",
            TargetRole: WombatRoles.Coordinator,
            InstitutionId: 1,
            SpecialityId: null,
            SubSpecialityId: null,
            IssuedByUserId: "admin-1",
            Principal: TestPrincipals.Administrator());

        var result = await handler.Handle(command, CancellationToken.None);

        result.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CommitteeMember_WithNoSpeciality_IsAccepted()
    {
        await using var db = CreateDbWithInstitution(institutionId: 1);
        var handler = CreateHandler(db);

        var command = new IssueInvitationCommand(
            Email: "external@example.test",
            TargetRole: WombatRoles.CommitteeMember,
            InstitutionId: 1,
            SpecialityId: null,
            SubSpecialityId: null,
            IssuedByUserId: "admin-1",
            Principal: TestPrincipals.Administrator());

        var result = await handler.Handle(command, CancellationToken.None);

        result.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SpecialityAdmin_WithNoSpeciality_IsRejectedWithSpecialityMessage()
    {
        await using var db = CreateDbWithInstitution(institutionId: 1);
        var handler = CreateHandler(db);

        var command = new IssueInvitationCommand(
            Email: "sa@example.test",
            TargetRole: WombatRoles.SpecialityAdmin,
            InstitutionId: 1,
            SpecialityId: null,
            SubSpecialityId: null,
            IssuedByUserId: "admin-1",
            Principal: TestPrincipals.Administrator());

        var act = () => handler.Handle(command, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.Which.Message.Should().Be("Speciality administrators must be scoped to a speciality.");
    }

    [Fact]
    public async Task Coordinator_WithSubSpeciality_IsStillRejected()
    {
        await using var db = CreateDbWithSpecialityAndSubSpeciality(institutionId: 1, specialityId: 2, subSpecialityId: 3);
        var handler = CreateHandler(db);

        var command = new IssueInvitationCommand(
            Email: "coord@example.test",
            TargetRole: WombatRoles.Coordinator,
            InstitutionId: 1,
            SpecialityId: 2,
            SubSpecialityId: 3,
            IssuedByUserId: "admin-1",
            Principal: TestPrincipals.Administrator());

        var act = () => handler.Handle(command, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.Which.Message.Should().Be("The selected role may not be scoped to a sub-speciality.");
    }

    [Fact]
    public async Task CommitteeMember_WithSubSpeciality_IsStillRejected()
    {
        await using var db = CreateDbWithSpecialityAndSubSpeciality(institutionId: 1, specialityId: 2, subSpecialityId: 3);
        var handler = CreateHandler(db);

        var command = new IssueInvitationCommand(
            Email: "ext@example.test",
            TargetRole: WombatRoles.CommitteeMember,
            InstitutionId: 1,
            SpecialityId: 2,
            SubSpecialityId: 3,
            IssuedByUserId: "admin-1",
            Principal: TestPrincipals.Administrator());

        var act = () => handler.Handle(command, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.Which.Message.Should().Be("The selected role may not be scoped to a sub-speciality.");
    }

    private IssueInvitationCommandHandler CreateHandler(ApplicationDbContext db) =>
        new(
            db,
            _tokenService,
            Mock.Of<IEmailSender>(),
            Options.Create(new WombatOptions { BaseUrl = "https://wombat.example.test" }));

    private static ApplicationDbContext CreateDbWithInstitution(int institutionId)
    {
        var db = NewInMemoryDb();
        db.Institutions.Add(new Institution
        {
            Id = institutionId,
            Name = $"Institution {institutionId}",
            ShortCode = $"I{institutionId}",
            IsActive = true,
            CreatedOn = DateTime.UtcNow
        });
        db.SaveChanges();
        return db;
    }

    private static ApplicationDbContext CreateDbWithSpecialityAndSubSpeciality(int institutionId, int specialityId, int subSpecialityId)
    {
        var db = CreateDbWithInstitution(institutionId);
        db.Specialities.Add(new Speciality
        {
            Id = specialityId,
            InstitutionId = institutionId,
            Name = $"Speciality {specialityId}",
            IsActive = true
        });
        db.SubSpecialities.Add(new SubSpeciality
        {
            Id = subSpecialityId,
            SpecialityId = specialityId,
            Name = $"SubSpeciality {subSpecialityId}",
            IsActive = true
        });
        db.SaveChanges();
        return db;
    }

    private static ApplicationDbContext NewInMemoryDb() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
