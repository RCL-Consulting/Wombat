using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
/// T283: an account invitation's mail says which invitation and link it carries, the invitations list says what became
/// of it, and the resend refuses, before anything is written or sent, an invitation it may not or need not resend.
/// </summary>
/// <remarks>
/// The resend's store is one conditioned <c>UPDATE</c>, and the recorder's too, which the in-memory provider cannot run.
/// What they do, and how they meet the other writers of the row, is <c>AccountInvitationDeliveryPostgresTests</c>.
/// </remarks>
public sealed class InvitationDeliveryTests
{
    private static readonly DateTime Now = new(2029, 3, 7, 9, 30, 0, DateTimeKind.Utc);
    private static readonly DateTime LongAgo = Now - Invitation.DeliveryReportDeadline - TimeSpan.FromMinutes(1);

    private const int InstitutionId = 4;

    private readonly InvitationTokenService _tokens = new();

    // ─── The issue ───────────────────────────────────────────────────────────

    [Fact]
    public async Task AnIssuedInvitationsMail_CarriesTheInvitationAndItsStoredLink_AndItsLinkIsBeingSent()
    {
        await using var db = CreateDb();
        db.Institutions.Add(Hospital());
        await db.SaveChangesAsync();
        var sender = new CapturingSender();

        var issued = await new IssueInvitationCommandHandler(db, _tokens, sender, Options(), new FixedClock(Now)).Handle(
            new IssueInvitationCommand(
                "registrar@hospital.test", WombatRoles.InstitutionalAdmin, InstitutionId, null, null, null, "admin-1",
                TestPrincipals.Administrator()),
            CancellationToken.None);

        db.ChangeTracker.Clear();
        var stored = await db.Set<Invitation>().SingleAsync();
        var mail = sender.Sent.Should().ContainSingle().Which;

        Invitation.TryReadDeliveryKey(mail.DeliveryKey, out var invitationId, out var tokenHash).Should().BeTrue();
        invitationId.Should().Be(issued.InvitationId).And.Be(stored.Id);
        tokenHash.Should().Be(stored.TokenHash).And.Be(_tokens.HashToken(issued.Token));
        mail.Tags.Should().NotContain(tag => tag.Contains(stored.TokenHash, StringComparison.OrdinalIgnoreCase),
            "the key is never a tag: every log line about a mail prints its tags");

        stored.IssuedOn.Should().Be(Now);
        stored.ExpiresOn.Should().Be(new DateOnly(2029, 3, 21));
        (stored.SentOn, stored.DeliveryFailedOn, stored.DeliveryFailures).Should().Be(((DateTime?)null, (DateTime?)null, 0));
        stored.DeliveryAt(Now).Should().Be(InvitationDelivery.BeingSent);
    }

    // ─── The list ────────────────────────────────────────────────────────────

    [Fact]
    public async Task TheList_SaysWhatBecameOfEachMail_AndToCheckTheAddressFromTheSecondFailure()
    {
        await using var db = CreateDb();
        db.Institutions.Add(Hospital());
        db.Set<Invitation>().AddRange(
            NewInvitation(1, "sent@hospital.test", LongAgo, sentOn: LongAgo.AddSeconds(2)),
            NewInvitation(2, "sending@hospital.test", Now.AddMinutes(-5)),
            NewInvitation(3, "unheard@hospital.test", LongAgo),
            NewInvitation(4, "dropped-once@hospital.test", Now.AddMinutes(-2), failedOn: Now.AddMinutes(-1), failures: 1),
            NewInvitation(5, "dropped-twice@hospital.test", Now.AddMinutes(-2), failedOn: Now.AddMinutes(-1), failures: 2),
            // Resent after two failures, and delivered this time: nothing to check.
            NewInvitation(6, "delivered-at-last@hospital.test", Now.AddMinutes(-3), sentOn: Now.AddMinutes(-2), failures: 2));
        await db.SaveChangesAsync();

        var rows = await new ListActiveInvitationsQueryHandler(db, new FixedClock(Now))
            .Handle(new ListActiveInvitationsQuery(TestPrincipals.Administrator()), CancellationToken.None);

        rows.Select(row => (row.Email, row.Delivery, row.DeliveryFailures, row.SuggestCheckingAddress))
            .Should().BeEquivalentTo(new[]
            {
                ("sent@hospital.test", InvitationDelivery.Sent, 0, false),
                ("sending@hospital.test", InvitationDelivery.BeingSent, 0, false),
                ("unheard@hospital.test", InvitationDelivery.NotDelivered, 0, false),
                ("dropped-once@hospital.test", InvitationDelivery.NotDelivered, 1, false),
                ("dropped-twice@hospital.test", InvitationDelivery.NotDelivered, 2, true),
                ("delivered-at-last@hospital.test", InvitationDelivery.Sent, 2, false)
            });
    }

    // ─── The resend's refusals: nothing written, nothing sent ────────────────

    public static TheoryData<string, string> Refusals() => new()
    {
        { "used", ResendInvitationCommandHandler.AlreadyUsed },
        { "revoked", ResendInvitationCommandHandler.Revoked },
        { "expired", ResendInvitationCommandHandler.Expired },
        { "sent", ResendInvitationCommandHandler.NothingToResend },
        { "being sent", ResendInvitationCommandHandler.NothingToResend }
    };

    [Theory]
    [MemberData(nameof(Refusals))]
    public async Task AResendOfAnInvitationItNeedNotOrMayNotResend_IsRefused_BeforeAnythingIsWrittenOrSent(
        string state, string refusal)
    {
        await using var db = CreateDb();
        var invitation = NewInvitation(7, "registrar@hospital.test", LongAgo, failedOn: LongAgo.AddSeconds(5), failures: 1);
        switch (state)
        {
            case "used":
                invitation.UsedOn = Now.AddMinutes(-1);
                break;
            case "revoked":
                invitation.RevokedOn = Now.AddMinutes(-1);
                break;
            case "expired":
                invitation.ExpiresOn = DateOnly.FromDateTime(Now).AddDays(-1);
                break;
            case "sent":
                invitation.DeliveryFailedOn = null;
                invitation.SentOn = LongAgo.AddSeconds(5);
                break;
            case "being sent":
                invitation.DeliveryFailedOn = null;
                invitation.IssuedOn = Now.AddMinutes(-5);
                break;
        }

        db.Set<Invitation>().Add(invitation);
        await db.SaveChangesAsync();
        var before = Snapshot(invitation);
        db.ChangeTracker.Clear();
        var sender = new CapturingSender();

        var act = () => Resend(db, sender, 7, TestPrincipals.Administrator());

        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be(refusal);
        await AssertNothingChangedAsync(db, before);
        sender.Sent.Should().BeEmpty();
    }

    [Fact]
    public async Task AResendByAnInstitutionalAdminOfAnotherInstitution_IsRefused_BeforeAnythingIsWrittenOrSent()
    {
        await using var db = CreateDb();
        var invitation = NewInvitation(7, "registrar@hospital.test", LongAgo, failedOn: LongAgo.AddSeconds(5), failures: 1);
        db.Set<Invitation>().Add(invitation);
        await db.SaveChangesAsync();
        var before = Snapshot(invitation);
        db.ChangeTracker.Clear();
        var sender = new CapturingSender();

        var act = () => Resend(db, sender, 7, TestPrincipals.InstitutionalAdmin(institutionId: InstitutionId + 1));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        await AssertNothingChangedAsync(db, before);
        sender.Sent.Should().BeEmpty();
    }

    /// <summary>
    /// A resend hands its caller a new registration link, so it is held to the issue's rule: only an Administrator issues
    /// a CollegeAdmin invitation, so only an Administrator resends one. That College's own CollegeAdmin passes the
    /// revoke's <c>CanAccessCollege</c>, and until the T283 review passed the resend's too, and was handed a fresh
    /// CollegeAdmin link. That an Administrator may resend one is <c>AccountInvitationDeliveryPostgresTests</c>.
    /// </summary>
    [Theory]
    [InlineData("that College's CollegeAdmin")]
    [InlineData("an InstitutionalAdmin")]
    public async Task AResendOfACollegeAdministratorsInvitation_IsTheAdministratorsAlone(string caller)
    {
        await using var db = CreateDb();
        var invitation = NewInvitation(7, "college@hospital.test", LongAgo, failedOn: LongAgo.AddSeconds(5), failures: 1);
        invitation.InstitutionId = null;
        invitation.CollegeId = 3;
        invitation.TargetRole = WombatRoles.CollegeAdmin;
        db.Set<Invitation>().Add(invitation);
        await db.SaveChangesAsync();
        var before = Snapshot(invitation);
        db.ChangeTracker.Clear();
        var sender = new CapturingSender();
        var principal = caller == "an InstitutionalAdmin"
            ? TestPrincipals.InstitutionalAdmin(institutionId: InstitutionId)
            : TestPrincipals.CollegeAdmin(collegeId: 3);

        var act = () => Resend(db, sender, 7, principal);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        await AssertNothingChangedAsync(db, before);
        sender.Sent.Should().BeEmpty();
    }

    [Fact]
    public async Task AResendOfNoSuchInvitation_IsRefused()
    {
        await using var db = CreateDb();
        var sender = new CapturingSender();

        var act = () => Resend(db, sender, 99, TestPrincipals.Administrator());

        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be("The invitation was not found.");
        sender.Sent.Should().BeEmpty();
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private Task<IssuedInvitationResult> Resend(
        ApplicationDbContext db, IEmailSender sender, int invitationId, System.Security.Claims.ClaimsPrincipal principal)
        => new ResendInvitationCommandHandler(db, _tokens, sender, Options(), new FixedClock(Now))
            .Handle(new ResendInvitationCommand(invitationId, principal), CancellationToken.None);

    /// <summary>
    /// The audit trap: the audit pipeline saves the handler's context after a refusal, so a refused handler that had
    /// touched a tracked row would commit it. So the context is saved, its tracker cleared, and the row read again.
    /// </summary>
    private static async Task AssertNothingChangedAsync(ApplicationDbContext db, object before)
    {
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var after = await db.Set<Invitation>().SingleAsync();
        Snapshot(after).Should().Be(before);
    }

    private static object Snapshot(Invitation invitation) => (
        invitation.TokenHash,
        invitation.IssuedOn,
        invitation.ExpiresOn,
        invitation.UsedOn,
        invitation.RevokedOn,
        invitation.SentOn,
        invitation.DeliveryFailedOn,
        invitation.DeliveryFailures);

    private Invitation NewInvitation(
        int id, string email, DateTime issuedOn, DateTime? sentOn = null, DateTime? failedOn = null, int failures = 0)
        => new()
        {
            Id = id,
            Email = email,
            TokenHash = _tokens.HashToken($"token-{id}"),
            TargetRole = WombatRoles.Coordinator,
            InstitutionId = InstitutionId,
            IssuedByUserId = "admin-1",
            IssuedOn = issuedOn,
            ExpiresOn = Invitation.LinkExpiresOn(issuedOn),
            SentOn = sentOn,
            DeliveryFailedOn = failedOn,
            DeliveryFailures = failures
        };

    private static Institution Hospital() => new()
    {
        Id = InstitutionId, Name = "Groote Schuur Hospital", ShortCode = "GSH", IsActive = true, CreatedOn = Now
    };

    private static IOptions<WombatOptions> Options()
        => Microsoft.Extensions.Options.Options.Create(new WombatOptions { BaseUrl = "https://wombat.example.test" });

    private static ApplicationDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private sealed class CapturingSender : IEmailSender
    {
        public List<EmailMessage> Sent { get; } = [];

        public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            Sent.Add(message);
            return Task.CompletedTask;
        }
    }

    private sealed class FixedClock(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow);
    }
}
