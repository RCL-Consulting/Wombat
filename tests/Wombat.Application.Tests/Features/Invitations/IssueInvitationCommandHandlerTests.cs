using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Common.Security;
using Wombat.Domain.Identity;
using Wombat.Application.Features.Invitations;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Invitations;

public sealed class IssueInvitationCommandHandlerTests
{
    private readonly InvitationTokenService _tokenService = new();

    [Fact]
    public async Task Handle_SendsExactlyOneEmail_WithInvitationSubject()
    {
        await using var db = CreateDb();
        db.Institutions.Add(new Domain.Institutions.Institution { Id = 1, Name = "Test" });
        await db.SaveChangesAsync();

        var emailSender = new Mock<IEmailSender>();
        var options = Options.Create(new WombatOptions { BaseUrl = "https://wombat.example.test" });

        var handler = new IssueInvitationCommandHandler(db, _tokenService, emailSender.Object, options);

        var command = new IssueInvitationCommand(
            Email: "invitee@example.test",
            TargetRole: WombatRoles.InstitutionalAdmin,
            InstitutionId: 1,
            SpecialityId: null,
            SubSpecialityId: null,
            IssuedByUserId: "admin-1");

        await handler.Handle(command, CancellationToken.None);

        emailSender.Verify(
            s => s.SendAsync(
                It.Is<EmailMessage>(m =>
                    m.To == "invitee@example.test" &&
                    m.Subject.Contains("invitation", StringComparison.OrdinalIgnoreCase) &&
                    m.TextBody.Contains("https://wombat.example.test/account/register")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_EmailBodyContainsRoleAndExpiryDate()
    {
        await using var db = CreateDb();
        db.Institutions.Add(new Domain.Institutions.Institution { Id = 1, Name = "Test" });
        await db.SaveChangesAsync();

        EmailMessage? captured = null;
        var emailSender = new Mock<IEmailSender>();
        emailSender
            .Setup(s => s.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Callback<EmailMessage, CancellationToken>((msg, _) => captured = msg)
            .Returns(Task.CompletedTask);

        var options = Options.Create(new WombatOptions { BaseUrl = "https://wombat.example.test" });
        var handler = new IssueInvitationCommandHandler(db, _tokenService, emailSender.Object, options);

        await handler.Handle(
            new IssueInvitationCommand("t@e.test", WombatRoles.InstitutionalAdmin, 1, null, null, "admin-1"),
            CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.TextBody.Should().Contain(WombatRoles.InstitutionalAdmin);
        captured.TextBody.Should().MatchRegex(@"\d{4}-\d{2}-\d{2}"); // expiry date
        captured.Tags.Should().Contain("invitation");
    }

    private static ApplicationDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
