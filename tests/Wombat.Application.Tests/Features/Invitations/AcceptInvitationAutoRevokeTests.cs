using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Audit;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Invitations;
using Wombat.Domain.Identity;
using Wombat.Domain.Invitations;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Invitations;

/// <summary>
/// T061: AcceptInvitation auto-revokes other Active invitations for the same email
/// once a user is provisioned. The provisioner refuses duplicate-email registration,
/// so stale invitations would otherwise linger forever in the Active list.
/// </summary>
public sealed class AcceptInvitationAutoRevokeTests
{
    private readonly InvitationTokenService _tokenService = new();

    [Fact]
    public async Task Handle_RevokesOtherActiveInvitationsForSameEmail()
    {
        await using var db = NewDb();
        var token = _tokenService.GenerateToken();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var primary = new Invitation
        {
            Email = "smit@kgk",
            TokenHash = _tokenService.HashToken(token),
            TargetRole = WombatRoles.Coordinator,
            InstitutionId = 1,
            IssuedByUserId = "admin",
            IssuedOn = DateTime.UtcNow.AddDays(-1),
            ExpiresOn = today.AddDays(14)
        };
        var secondaryActive = new Invitation
        {
            Email = "smit@kgk",
            TokenHash = "h-secondary",
            TargetRole = WombatRoles.Assessor,
            InstitutionId = 1,
            IssuedByUserId = "admin",
            IssuedOn = DateTime.UtcNow.AddHours(-2),
            ExpiresOn = today.AddDays(14)
        };
        var alreadyRevoked = new Invitation
        {
            Email = "smit@kgk",
            TokenHash = "h-revoked",
            TargetRole = WombatRoles.CommitteeMember,
            InstitutionId = 1,
            IssuedByUserId = "admin",
            IssuedOn = DateTime.UtcNow.AddDays(-3),
            ExpiresOn = today.AddDays(11),
            RevokedOn = DateTime.UtcNow.AddDays(-1)
        };
        var otherEmail = new Invitation
        {
            Email = "other@kgk",
            TokenHash = "h-other",
            TargetRole = WombatRoles.Assessor,
            InstitutionId = 1,
            IssuedByUserId = "admin",
            IssuedOn = DateTime.UtcNow,
            ExpiresOn = today.AddDays(14)
        };
        db.Set<Invitation>().AddRange(primary, secondaryActive, alreadyRevoked, otherEmail);
        await db.SaveChangesAsync();

        var handler = new AcceptInvitationCommandHandler(db, _tokenService, new StubProvisioner(), new NullAuditContext());
        var result = await handler.Handle(
            new AcceptInvitationCommand(token, "Pass!2026", "Sam", "Smit"),
            CancellationToken.None);

        result.UserId.Should().Be("provisioned-1");

        var primaryAfter = await db.Set<Invitation>().SingleAsync(invitation => invitation.Id == primary.Id);
        primaryAfter.UsedOn.Should().NotBeNull();
        primaryAfter.RevokedOn.Should().BeNull();

        var secondaryAfter = await db.Set<Invitation>().SingleAsync(invitation => invitation.Id == secondaryActive.Id);
        secondaryAfter.RevokedOn.Should().NotBeNull();
        secondaryAfter.UsedOn.Should().BeNull();

        var revokedAfter = await db.Set<Invitation>().SingleAsync(invitation => invitation.Id == alreadyRevoked.Id);
        revokedAfter.RevokedOn.Should().Be(alreadyRevoked.RevokedOn);

        var otherAfter = await db.Set<Invitation>().SingleAsync(invitation => invitation.Id == otherEmail.Id);
        otherAfter.RevokedOn.Should().BeNull();
    }

    private static ApplicationDbContext NewDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    /// <summary>
    /// The handler declares the invitation's institution to the audit pipeline (T101); nothing about
    /// the sweep depends on it, so this swallows the declaration. AcceptInvitationAuditScopeTests
    /// covers the stamping itself.
    /// </summary>
    private sealed class NullAuditContext : IAuditContextProvider
    {
        public string? UserId => null;
        public string? UserDisplay => null;
        public string? IpAddress => null;
        public string? UserAgent => null;
        public int? InstitutionId => null;
        public void DeclareInstitution(int institutionId) { }
        public void DeclareActor(string userId, string display) { }
    }

    private sealed class StubProvisioner : IInvitedUserProvisioner
    {
        public Task<InvitedAddressStatus> GetAddressStatusAsync(string email, CancellationToken cancellationToken = default)
            => Task.FromResult(InvitedAddressStatus.Available);

        public Task<ProvisionedInvitationUser> ProvisionAsync(
            string email,
            string password,
            string firstName,
            string lastName,
            string targetRole,
            int? institutionId,
            int? collegeId,
            int? specialityId,
            int? subSpecialityId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(new ProvisionedInvitationUser("provisioned-1", targetRole));
    }
}
