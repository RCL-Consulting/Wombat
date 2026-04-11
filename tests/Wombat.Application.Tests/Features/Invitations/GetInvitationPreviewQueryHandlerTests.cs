using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Invitations;
using Wombat.Domain.Invitations;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Invitations;

public sealed class GetInvitationPreviewQueryHandlerTests
{
    private readonly InvitationTokenService _tokenService = new();

    [Fact]
    public async Task Handle_RejectsExpiredInvitation()
    {
        var (dbContext, token) = await CreateDbContextWithInvitationAsync(
            invitation => invitation.ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)));

        var handler = new GetInvitationPreviewQueryHandler(dbContext, _tokenService);

        var act = () => handler.Handle(new GetInvitationPreviewQuery(token), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*expired*");
    }

    [Fact]
    public async Task Handle_RejectsRevokedInvitation()
    {
        var (dbContext, token) = await CreateDbContextWithInvitationAsync(
            invitation => invitation.RevokedOn = DateTime.UtcNow);

        var handler = new GetInvitationPreviewQueryHandler(dbContext, _tokenService);

        var act = () => handler.Handle(new GetInvitationPreviewQuery(token), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*revoked*");
    }

    [Fact]
    public async Task Handle_RejectsUsedInvitation()
    {
        var (dbContext, token) = await CreateDbContextWithInvitationAsync(
            invitation => invitation.UsedOn = DateTime.UtcNow);

        var handler = new GetInvitationPreviewQueryHandler(dbContext, _tokenService);

        var act = () => handler.Handle(new GetInvitationPreviewQuery(token), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*used*");
    }

    private async Task<(ApplicationDbContext DbContext, string Token)> CreateDbContextWithInvitationAsync(Action<Invitation>? configure = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dbContext = new ApplicationDbContext(options);
        var token = _tokenService.GenerateToken();
        var invitation = new Invitation
        {
            Email = "invitee@example.test",
            TokenHash = _tokenService.HashToken(token),
            TargetRole = "Assessor",
            InstitutionId = 1,
            SpecialityId = 2,
            SubSpecialityId = 3,
            IssuedByUserId = "admin-1",
            IssuedOn = DateTime.UtcNow.AddDays(-1),
            ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14))
        };

        configure?.Invoke(invitation);

        dbContext.Invitations.Add(invitation);
        await dbContext.SaveChangesAsync();
        return (dbContext, token);
    }
}
