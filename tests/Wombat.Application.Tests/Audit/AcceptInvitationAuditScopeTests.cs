using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Audit;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Invitations;
using Wombat.Domain.Identity;
using Wombat.Domain.Invitations;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Audit;

/// <summary>
/// T101: AcceptInvitationCommand runs from an AllowAnonymous endpoint that signs the new user in only
/// after the command returns, so the audit pipeline's principal carries no institution — and when the
/// browser still holds another user's cookie it carries the wrong one (the dev database has eleven such
/// rows, each naming the previously registered user). Since T101 made an unstamped row Administrator-only,
/// that meant every registration completion vanished from the inviting admin's /admin/audit.
///
/// The handler therefore declares the scope itself, from the invitation the token matched.
/// </summary>
public sealed class AcceptInvitationAuditScopeTests
{
    private readonly InvitationTokenService _tokenService = new();

    [Fact]
    public async Task Handle_DeclaresInvitationsInstitution_NotTheStaleCookies()
    {
        await using var db = NewDb();
        var token = _tokenService.GenerateToken();
        db.Set<Invitation>().Add(NewInvitation(token, institutionId: 3));
        await db.SaveChangesAsync();

        // 9 stands in for the previously registered user's cookie, still in the browser.
        var auditContext = new DeclarableAuditContext(principalInstitutionId: 9);
        var handler = new AcceptInvitationCommandHandler(db, _tokenService, new StubProvisioner(), auditContext);

        await handler.Handle(
            new AcceptInvitationCommand(token, "Pass!2026", "Sam", "Smit"),
            CancellationToken.None);

        auditContext.InstitutionId.Should().Be(3);
    }

    /// <summary>
    /// A stale invitation being retried is exactly what the issuing admin needs to see, so the scope is
    /// declared from the matched invitation before the validity checks reject it — the pipeline reads it
    /// when it writes the failure row.
    /// </summary>
    [Fact]
    public async Task Handle_RevokedToken_StillDeclaresIssuingInstitution()
    {
        await using var db = NewDb();
        var token = _tokenService.GenerateToken();
        var invitation = NewInvitation(token, institutionId: 3);
        invitation.RevokedOn = DateTime.UtcNow.AddHours(-1);
        db.Set<Invitation>().Add(invitation);
        await db.SaveChangesAsync();

        var auditContext = new DeclarableAuditContext(principalInstitutionId: null);
        var handler = new AcceptInvitationCommandHandler(db, _tokenService, new StubProvisioner(), auditContext);

        var act = async () => await handler.Handle(
            new AcceptInvitationCommand(token, "Pass!2026", "Sam", "Smit"),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        auditContext.InstitutionId.Should().Be(3);
    }

    /// <summary>
    /// A token matching nothing has no institution to attribute it to, and guessing one would put a
    /// stranger's registration attempt in an unrelated admin's log. Unknown scope is Administrator-only.
    /// </summary>
    [Fact]
    public async Task Handle_UnknownToken_DeclaresNothing()
    {
        await using var db = NewDb();
        db.Set<Invitation>().Add(NewInvitation(_tokenService.GenerateToken(), institutionId: 3));
        await db.SaveChangesAsync();

        var auditContext = new DeclarableAuditContext(principalInstitutionId: null);
        var handler = new AcceptInvitationCommandHandler(db, _tokenService, new StubProvisioner(), auditContext);

        var act = async () => await handler.Handle(
            new AcceptInvitationCommand("not-a-real-token", "Pass!2026", "Sam", "Smit"),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        auditContext.InstitutionId.Should().BeNull();
    }

    private Invitation NewInvitation(string token, int institutionId) => new()
    {
        Email = "smit@kgk",
        TokenHash = _tokenService.HashToken(token),
        TargetRole = WombatRoles.Coordinator,
        InstitutionId = institutionId,
        IssuedByUserId = "admin",
        IssuedOn = DateTime.UtcNow.AddDays(-1),
        ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(14)
    };

    private static ApplicationDbContext NewDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private sealed class DeclarableAuditContext(int? principalInstitutionId) : IAuditContextProvider
    {
        private int? _declared;

        public string? UserId => null;
        public string? UserDisplay => null;
        public string? IpAddress => null;
        public string? UserAgent => null;
        public int? InstitutionId => _declared ?? principalInstitutionId;
        public void DeclareInstitution(int institutionId) => _declared = institutionId;
    }

    private sealed class StubProvisioner : IInvitedUserProvisioner
    {
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
