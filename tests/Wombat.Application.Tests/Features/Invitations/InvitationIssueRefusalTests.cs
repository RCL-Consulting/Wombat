using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wombat.Application.Audit;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Invitations;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Domain.Invitations;
using Wombat.Infrastructure.Audit;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.Invitations;

/// <summary>
/// T373 (flow 11): what an issue writes and refuses. The role the issue was authorised as (F3); an address an account holds
/// (Q12, T286's issue half) or an invitation that still works names (Q10), refused whatever its case (S4) and typed, the
/// holder named only in the caller's scope, the audit row naming nobody (M3); each scope refusal under its field (Q3).
/// </summary>
public sealed class InvitationIssueRefusalTests
{
    private const int Kgk = 1;
    private const int Sun = 2;
    private const int College = 7;
    private static readonly DateTimeOffset Now = new(2026, 10, 19, 7, 19, 0, TimeSpan.Zero);

    private readonly InvitationTokenService _tokens = new();
    private readonly string _databaseName = Guid.NewGuid().ToString();

    // ─── F3: the role the issue was authorised as ─────────────────────────────

    [Fact]
    public async Task AnAdministratorsIssue_IsRecordedAsTheAdministrators()
    {
        await using var db = await SeededAsync();

        await Handler(db).Handle(Coordinator("smit@kgk.wombat.local", Kgk, TestPrincipals.Administrator()), CancellationToken.None);

        (await db.Set<Invitation>().SingleAsync()).IssuedAsRole.Should().Be(WombatRoles.Administrator);
    }

    [Fact]
    public async Task AnInstitutionalAdminsIssue_IsRecordedAsTheInstitutionalAdmins()
    {
        await using var db = await SeededAsync();

        await Handler(db).Handle(Coordinator("smit@kgk.wombat.local", Kgk, TestPrincipals.InstitutionalAdmin(Kgk)), CancellationToken.None);

        (await db.Set<Invitation>().SingleAsync()).IssuedAsRole.Should().Be(WombatRoles.InstitutionalAdmin);
    }

    [Fact]
    public async Task TheAddress_IsStoredTrimmed_AsTyped()
    {
        await using var db = await SeededAsync();

        var issued = await Handler(db).Handle(
            Coordinator("  Smit@KGK.wombat.local ", Kgk, TestPrincipals.InstitutionalAdmin(Kgk)), CancellationToken.None);

        (await db.Set<Invitation>().SingleAsync()).Email.Should().Be("Smit@KGK.wombat.local");
        issued.ExpiresOn.Should().Be(new DateOnly(2026, 11, 2), "the issue's last day, for the OneTimeLink's words");
    }

    // ─── Q12: an account holds the address ────────────────────────────────────

    [Fact]
    public async Task AnAccountAtTheCallersInstitution_IsRefused_NamingItsHolder()
    {
        await using var db = await SeededAsync();
        var users = new FakeUserDirectory().With(Account("smit-1", "smit@kgk.wombat.local", "Pieter", "Smit", Kgk));

        var refusal = await RefusalOf(Handler(db, users), Coordinator("smit@kgk.wombat.local", Kgk, TestPrincipals.InstitutionalAdmin(Kgk)));

        refusal.Reason.Should().Be(InvitationIssueRefusal.AccountExists);
        refusal.Field.Should().Be(InvitationIssueField.Email);
        refusal.InCallerScope.Should().BeTrue();
        refusal.HolderUserId.Should().Be("smit-1");
        refusal.HolderName.Should().Be("Pieter Smit");
        refusal.Message.Should().Be(InvitationIssueRefusedException.AddressInUse, "the message names nobody (M3)");
        (await db.Set<Invitation>().AnyAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task AnAccountElsewhere_IsRefused_NamingNobody()
    {
        await using var db = await SeededAsync();
        var users = new FakeUserDirectory().With(Account("kruger-1", "kruger@cmsa.wombat.local", "Anna", "Kruger", institutionId: null));

        var refusal = await RefusalOf(Handler(db, users), Coordinator("kruger@cmsa.wombat.local", Kgk, TestPrincipals.InstitutionalAdmin(Kgk)));

        refusal.Reason.Should().Be(InvitationIssueRefusal.AccountExists);
        refusal.InCallerScope.Should().BeFalse();
        refusal.HolderUserId.Should().BeNull();
        refusal.HolderName.Should().BeNull();
    }

    [Fact]
    public async Task ForTheAdministrator_EveryAccount_IsInScope()
    {
        await using var db = await SeededAsync();
        var users = new FakeUserDirectory().With(Account("smit-1", "smit@kgk.wombat.local", "Pieter", "Smit", Sun));

        var refusal = await RefusalOf(Handler(db, users), Coordinator("smit@kgk.wombat.local", Kgk, TestPrincipals.Administrator()));

        refusal.InCallerScope.Should().BeTrue();
        refusal.HolderName.Should().Be("Pieter Smit");
    }

    [Fact]
    public async Task AnAccount_IsFoundWhateverTheAddressesCase()
    {
        await using var db = await SeededAsync();
        var users = new FakeUserDirectory().With(Account("smit-1", "smit@kgk.wombat.local", "Pieter", "Smit", Kgk));

        var refusal = await RefusalOf(Handler(db, users), Coordinator(" SMIT@kgk.Wombat.local", Kgk, TestPrincipals.InstitutionalAdmin(Kgk)));

        refusal.Reason.Should().Be(InvitationIssueRefusal.AccountExists);
    }

    // ─── Q10: an invitation that still works names the address ───────────────

    [Fact]
    public async Task AnActiveInvitationAtTheCallersInstitution_IsRefused_WithItsIssueAndLastDay()
    {
        await using var db = await SeededAsync();
        var issuedOn = new DateTime(2026, 10, 19, 7, 22, 0, DateTimeKind.Utc);
        Seed(db, "molefe@kgk.wombat.local", Kgk, issuedOn: issuedOn, expiresOn: new DateOnly(2026, 11, 2));

        var refusal = await RefusalOf(Handler(db), Coordinator("molefe@kgk.wombat.local", Kgk, TestPrincipals.InstitutionalAdmin(Kgk)));

        refusal.Reason.Should().Be(InvitationIssueRefusal.ActiveInvitationExists);
        refusal.Field.Should().Be(InvitationIssueField.Email);
        refusal.InCallerScope.Should().BeTrue();
        refusal.ActiveIssuedOn.Should().Be(issuedOn);
        refusal.ActiveExpiresOn.Should().Be(new DateOnly(2026, 11, 2));
        refusal.Message.Should().Be(InvitationIssueRefusedException.AddressInUse);
    }

    [Fact]
    public async Task AnActiveInvitationAtAnotherInstitution_IsRefused_WithNoDate()
    {
        await using var db = await SeededAsync();
        Seed(db, "molefe@kgk.wombat.local", Sun);

        var refusal = await RefusalOf(Handler(db), Coordinator("molefe@kgk.wombat.local", Kgk, TestPrincipals.InstitutionalAdmin(Kgk)));

        refusal.Reason.Should().Be(InvitationIssueRefusal.ActiveInvitationExists);
        refusal.InCallerScope.Should().BeFalse();
        refusal.ActiveIssuedOn.Should().BeNull();
        refusal.ActiveExpiresOn.Should().BeNull();
        refusal.HeldByCollege.Should().BeFalse();
    }

    [Fact]
    public async Task ACollegesActiveInvitation_IsRefused_SayingOnlyThatItIsACollegesForAnInstitutionalAdmin()
    {
        await using var db = await SeededAsync();
        Seed(db, "kruger@cmsa.wombat.local", institutionId: null, collegeId: College, role: WombatRoles.CollegeAdmin);

        var refusal = await RefusalOf(Handler(db), Coordinator("kruger@cmsa.wombat.local", Kgk, TestPrincipals.InstitutionalAdmin(Kgk)));

        refusal.InCallerScope.Should().BeFalse();
        refusal.HeldByCollege.Should().BeTrue();
        refusal.ActiveIssuedOn.Should().BeNull();
    }

    [Fact]
    public async Task AnActiveInvitationInAnotherCase_IsRefused()
    {
        await using var db = await SeededAsync();
        Seed(db, "Molefe@KGK.wombat.local", Kgk);

        var refusal = await RefusalOf(Handler(db), Coordinator("molefe@kgk.wombat.local ", Kgk, TestPrincipals.InstitutionalAdmin(Kgk)));

        refusal.Reason.Should().Be(InvitationIssueRefusal.ActiveInvitationExists);
    }

    [Theory]
    [InlineData("used")]
    [InlineData("revoked")]
    [InlineData("expired")]
    public async Task AnInvitationThatNoLongerWorks_DoesNotStandInTheWay(string state)
    {
        await using var db = await SeededAsync();
        var invitation = Seed(db, "molefe@kgk.wombat.local", Kgk);
        switch (state)
        {
            case "used": invitation.UsedOn = Now.UtcDateTime.AddDays(-1); break;
            case "revoked": invitation.RevokedOn = Now.UtcDateTime.AddDays(-1); break;
            default: invitation.ExpiresOn = new DateOnly(2026, 10, 18); break;
        }

        await db.SaveChangesAsync();

        await Handler(db).Handle(Coordinator("molefe@kgk.wombat.local", Kgk, TestPrincipals.InstitutionalAdmin(Kgk)), CancellationToken.None);

        (await db.Set<Invitation>().CountAsync()).Should().Be(2);
    }

    /// <summary>
    /// M3: the audit pipeline writes a refused command's message into the caller's row. An Institutional admin of one
    /// institution refused over an address another institution's account and invitation hold finds nothing of either there.
    /// </summary>
    [Fact]
    public async Task ARefusalOverAnotherInstitutionsAddress_LeavesNothingOfItInTheCallersAuditRow()
    {
        await using (var seed = await SeededAsync())
        {
            Seed(seed, "duplessis@sun.wombat.local", Sun);
        }

        var users = new FakeUserDirectory().With(Account("smit-9", "smit@sun.wombat.local", "Jan", "Smit", Sun));

        foreach (var address in new[] { "smit@sun.wombat.local", "duplessis@sun.wombat.local" })
        {
            await using var db = CreateDb();
            var command = Coordinator(address, Kgk, TestPrincipals.InstitutionalAdmin(Kgk));
            var handler = Handler(db, users);
            var act = () => new AuditPipelineBehavior<IssueInvitationCommand, IssuedInvitationResult>(new AuditWriter(db), new FixedAuditContext())
                .Handle(command, () => handler.Handle(command, CancellationToken.None), CancellationToken.None);
            await act.Should().ThrowAsync<InvitationIssueRefusedException>();
        }

        await using var read = CreateDb();
        var rows = await read.AuditEntries.AsNoTracking().ToListAsync();
        rows.Should().HaveCount(2).And.OnlyContain(row => !row.Success);
        foreach (var row in rows)
        {
            row.ErrorMessage.Should().Be(InvitationIssueRefusedException.AddressInUse);
            var stored = row.SummaryJson + row.ErrorMessage;
            stored.Should().NotContain("Jan").And.NotContain("Smit ").And.NotContain("smit-9")
                .And.NotContain("Sun Hospital").And.NotContain("2026-11-02");
        }
    }

    // ─── Q3: each scope refusal under its field ──────────────────────────────

    [Theory]
    [InlineData(WombatRoles.CollegeAdmin, null, null, null, null, InvitationIssueField.College, "A college is required for a college administrator.")]
    [InlineData(WombatRoles.SpecialityAdmin, Kgk, null, null, null, InvitationIssueField.Speciality, "Speciality administrators must be scoped to a speciality.")]
    [InlineData(WombatRoles.Assessor, Kgk, null, null, null, InvitationIssueField.Speciality, "The selected role requires speciality and sub-speciality scope.")]
    [InlineData(WombatRoles.SubSpecialityAdmin, Kgk, null, 5, null, InvitationIssueField.SubSpeciality, "The selected role requires speciality and sub-speciality scope.")]
    [InlineData(WombatRoles.Coordinator, null, null, null, null, InvitationIssueField.Institution, "An institution is required.")]
    public async Task AScopeRefusal_CarriesItsFieldAndItsWords(
        string role, int? institutionId, int? collegeId, int? specialityId, int? subSpecialityId,
        InvitationIssueField field, string words)
    {
        await using var db = await SeededAsync();

        var refusal = await RefusalOf(Handler(db), new IssueInvitationCommand(
            "someone@kgk.wombat.local", role, institutionId, collegeId, specialityId, subSpecialityId, "admin-1",
            TestPrincipals.Administrator()));

        refusal.Reason.Should().Be(InvitationIssueRefusal.Scope);
        refusal.Field.Should().Be(field);
        refusal.Message.Should().Be(words);
    }

    [Fact]
    public void TheIssuesValidator_SaysWhatToDo_InWombatsWords()
    {
        var validator = new IssueInvitationCommandValidator();

        validator.Validate(Coordinator("", Kgk, TestPrincipals.Administrator())).Errors
            .Select(error => error.ErrorMessage).Should().Equal(IssueInvitationCommandValidator.EmailMissing);
        validator.Validate(Coordinator("not-an-address", Kgk, TestPrincipals.Administrator())).Errors
            .Select(error => error.ErrorMessage).Should().Equal(IssueInvitationCommandValidator.EmailMalformed);
        validator.Validate(new IssueInvitationCommand("a@b.test", "", Kgk, null, null, null, "admin-1", TestPrincipals.Administrator()))
            .Errors.Select(error => error.ErrorMessage).Should().Equal(IssueInvitationCommandValidator.RoleMissing);
        IssueInvitationCommandValidator.EmailMissing.Should().Be("Enter the person's email address.");
        IssueInvitationCommandValidator.EmailMalformed.Should().Be("Enter a valid email address, for example name@example.org.");
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static IssueInvitationCommand Coordinator(string email, int institutionId, System.Security.Claims.ClaimsPrincipal caller)
        => new(email, WombatRoles.Coordinator, institutionId, null, null, null, "admin-1", caller);

    private static UserIdentityDetails Account(string id, string email, string first, string last, int? institutionId)
        => new(id, email, first, last, institutionId, [], [], [WombatRoles.Coordinator]);

    private IssueInvitationCommandHandler Handler(ApplicationDbContext db, IUserAdministrationService? users = null)
        => new(db, _tokens, new NullSender(), users ?? FakeUserDirectory.Empty,
            Options.Create(new WombatOptions { BaseUrl = "https://wombat.test" }), new FixedClock(Now));

    private static async Task<InvitationIssueRefusedException> RefusalOf(IssueInvitationCommandHandler handler, IssueInvitationCommand command)
    {
        var act = () => handler.Handle(command, CancellationToken.None);
        return (await act.Should().ThrowAsync<InvitationIssueRefusedException>()).Which;
    }

    private static Invitation Seed(
        ApplicationDbContext db,
        string email,
        int? institutionId,
        int? collegeId = null,
        string role = WombatRoles.Trainee,
        DateTime? issuedOn = null,
        DateOnly? expiresOn = null)
    {
        var invitation = new Invitation
        {
            Email = email,
            TokenHash = new string('a', 64),
            TargetRole = role,
            InstitutionId = institutionId,
            CollegeId = collegeId,
            IssuedByUserId = "someone",
            IssuedOn = issuedOn ?? Now.UtcDateTime.AddHours(-1),
            ExpiresOn = expiresOn ?? new DateOnly(2026, 11, 2)
        };
        db.Set<Invitation>().Add(invitation);
        db.SaveChanges();
        return invitation;
    }

    private async Task<ApplicationDbContext> SeededAsync()
    {
        var db = CreateDb();
        db.Institutions.AddRange(
            new Institution { Id = Kgk, Name = "Kgosi Kgari Teaching Hospital", ShortCode = "KGK", IsActive = true, CreatedOn = DateTime.UtcNow },
            new Institution { Id = Sun, Name = "Sun Hospital", ShortCode = "SUN", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.Colleges.Add(new College { Id = College, Name = "College of Paediatricians of South Africa", ShortCode = "CPSA", IsActive = true, CreatedOn = DateTime.UtcNow });
        await db.SaveChangesAsync();
        return db;
    }

    private ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(_databaseName).Options);

    private sealed class NullSender : IEmailSender
    {
        public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class FixedAuditContext : IAuditContextProvider
    {
        public string? UserId => "inst-admin-user";
        public string? UserDisplay => "Nolwazi Mbatha";
        public string? IpAddress => "10.0.0.0/24";
        public string? UserAgent => "Test/1.0";
        public int? InstitutionId => Kgk;
        public void DeclareInstitution(int institutionId) { }
        public void DeclareActor(string userId, string display) { }
    }
}
