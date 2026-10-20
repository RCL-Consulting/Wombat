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
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.Invitations;

/// <summary>
/// T290: someone who holds Trainee beside InstitutionalAdmin issues, lists, resends and revokes no invitation, as they
/// administer no user (T278); and a revoke or resend by id gives an InstitutionalAdmin one refusal whether the id names
/// nothing or another institution's invitation.
/// </summary>
public sealed class InvitationTraineeFirstTests
{
    private const string TraineeAdministersNoUser =
        "You hold the Trainee role, so you cannot view or change user accounts, including your own.";

    private const int InstitutionA = 1;
    private const int InstitutionB = 2;
    private const int InvitationAtA = 10;
    private const int InvitationAtB = 20;

    private readonly IInvitationTokenService _tokenService = new InvitationTokenService();

    [Fact]
    public async Task ATraineeWhoAdministersTheirInstitution_IsRefusedEveryInvitationRequest_AndNothingChanges()
    {
        await using var db = await SeededDbAsync();
        var caller = TestPrincipals.InRoles([WombatRoles.Trainee, WombatRoles.InstitutionalAdmin], "registrar-a", InstitutionA);

        var issue = () => IssueHandler(db).Handle(
            new IssueInvitationCommand("new@a.test", WombatRoles.Coordinator, InstitutionA, null, null, null, "registrar-a", caller),
            CancellationToken.None);
        var list = () => new ListActiveInvitationsQueryHandler(db).Handle(new ListActiveInvitationsQuery(caller), CancellationToken.None);
        var resend = () => ResendHandler(db).Handle(new ResendInvitationCommand(InvitationAtA, caller), CancellationToken.None);
        var revoke = () => new RevokeInvitationCommandHandler(db).Handle(new RevokeInvitationCommand(InvitationAtA, caller), CancellationToken.None);

        foreach (var act in new Func<Task>[] { issue, list, resend, revoke })
        {
            (await act.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message.Should().Be(TraineeAdministersNoUser);
        }

        db.ChangeTracker.Clear();
        (await db.Set<Invitation>().CountAsync()).Should().Be(2);
        (await db.Set<Invitation>().SingleAsync(invitation => invitation.Id == InvitationAtA)).RevokedOn.Should().BeNull();
    }

    [Theory]
    [InlineData(InvitationAtB)]
    [InlineData(999)]
    public async Task AnInstitutionalAdmin_IsGivenOneRefusal_WhetherTheIdNamesNothingOrAnotherInstitutionsInvitation(int invitationId)
    {
        await using var db = await SeededDbAsync();
        var caller = TestPrincipals.InstitutionalAdmin(InstitutionA);

        var revoke = () => new RevokeInvitationCommandHandler(db).Handle(new RevokeInvitationCommand(invitationId, caller), CancellationToken.None);
        var resend = () => ResendHandler(db).Handle(new ResendInvitationCommand(invitationId, caller), CancellationToken.None);

        (await revoke.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message
            .Should().Be("You do not have permission to revoke this invitation.");
        (await resend.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message
            .Should().Be("You do not have permission to resend this invitation.");
    }

    [Fact]
    public async Task AnAdministrator_IsToldThatAnIdNamesNoInvitation()
    {
        await using var db = await SeededDbAsync();
        var revoke = () => new RevokeInvitationCommandHandler(db).Handle(
            new RevokeInvitationCommand(999, TestPrincipals.Administrator()), CancellationToken.None);

        (await revoke.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be("The invitation was not found.");
    }

    private IssueInvitationCommandHandler IssueHandler(ApplicationDbContext db)
        => new(db, _tokenService, Mock.Of<IEmailSender>(), FakeUserDirectory.Empty, Options.Create(new WombatOptions { BaseUrl = "https://x" }));

    private ResendInvitationCommandHandler ResendHandler(ApplicationDbContext db)
        => new(db, _tokenService, Mock.Of<IEmailSender>(), Options.Create(new WombatOptions { BaseUrl = "https://x" }));

    private static async Task<ApplicationDbContext> SeededDbAsync()
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
        db.Institutions.AddRange(
            new Institution { Id = InstitutionA, Name = "A", ShortCode = "A", IsActive = true, CreatedOn = DateTime.UtcNow },
            new Institution { Id = InstitutionB, Name = "B", ShortCode = "B", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.Set<Invitation>().AddRange(Pending(InvitationAtA, InstitutionA), Pending(InvitationAtB, InstitutionB));
        await db.SaveChangesAsync();
        return db;
    }

    private static Invitation Pending(int id, int institutionId) => new()
    {
        Id = id,
        Email = $"invitee{id}@x.test",
        TokenHash = $"hash{id}",
        TargetRole = WombatRoles.Coordinator,
        InstitutionId = institutionId,
        IssuedByUserId = "u1",
        IssuedOn = DateTime.UtcNow,
        ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))
    };
}
