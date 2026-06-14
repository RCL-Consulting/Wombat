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
using Wombat.Domain.Invitations;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Invitations;

/// <summary>
/// T056.d scope-guard tests for invitations.
/// </summary>
public sealed class InvitationScopeGuardTests
{
    private readonly IInvitationTokenService _tokenService = new InvitationTokenService();

    [Fact]
    public async Task IssueInvitation_InstitutionalAdmin_OtherInstitution_Rejected()
    {
        await using var db = CreateDb();
        db.Institutions.AddRange(
            new Institution { Id = 1, Name = "A", ShortCode = "A", IsActive = true, CreatedOn = DateTime.UtcNow },
            new Institution { Id = 2, Name = "B", ShortCode = "B", IsActive = true, CreatedOn = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var handler = new IssueInvitationCommandHandler(
            db,
            _tokenService,
            Mock.Of<IEmailSender>(),
            Options.Create(new WombatOptions { BaseUrl = "https://x" }));

        var command = new IssueInvitationCommand(
            "x@example.test",
            WombatRoles.Coordinator,
            InstitutionId: 2,
            CollegeId: null,
            SpecialityId: null,
            SubSpecialityId: null,
            IssuedByUserId: "u1",
            Principal: TestPrincipals.InstitutionalAdmin(institutionId: 1));

        await ((Func<Task>)(() => handler.Handle(command, CancellationToken.None)))
            .Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task RevokeInvitation_InstitutionalAdmin_OtherInstitution_Rejected()
    {
        await using var db = CreateDb();
        db.Institutions.Add(new Institution { Id = 2, Name = "B", ShortCode = "B", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.Set<Invitation>().Add(new Invitation
        {
            Id = 10,
            Email = "x@example.test",
            TokenHash = "hash",
            TargetRole = WombatRoles.Coordinator,
            InstitutionId = 2,
            IssuedByUserId = "u1",
            IssuedOn = DateTime.UtcNow,
            ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))
        });
        await db.SaveChangesAsync();

        var handler = new RevokeInvitationCommandHandler(db);
        var act = () => handler.Handle(
            new RevokeInvitationCommand(10, TestPrincipals.InstitutionalAdmin(institutionId: 1)),
            CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task ListActiveInvitations_InstitutionalAdmin_SeesOnlyOwn()
    {
        await using var db = CreateDb();
        db.Institutions.AddRange(
            new Institution { Id = 1, Name = "A", ShortCode = "A", IsActive = true, CreatedOn = DateTime.UtcNow },
            new Institution { Id = 2, Name = "B", ShortCode = "B", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.Set<Invitation>().AddRange(
            new Invitation { Id = 1, Email = "a@x", TokenHash = "h", TargetRole = WombatRoles.Coordinator, InstitutionId = 1, IssuedByUserId = "u", IssuedOn = DateTime.UtcNow, ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)) },
            new Invitation { Id = 2, Email = "b@x", TokenHash = "h", TargetRole = WombatRoles.Coordinator, InstitutionId = 2, IssuedByUserId = "u", IssuedOn = DateTime.UtcNow, ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)) });
        await db.SaveChangesAsync();

        var handler = new ListActiveInvitationsQueryHandler(db);
        var result = await handler.Handle(
            new ListActiveInvitationsQuery(TestPrincipals.InstitutionalAdmin(institutionId: 1)),
            CancellationToken.None);

        result.Should().ContainSingle().Which.InstitutionId.Should().Be(1);
    }

    private static ApplicationDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
