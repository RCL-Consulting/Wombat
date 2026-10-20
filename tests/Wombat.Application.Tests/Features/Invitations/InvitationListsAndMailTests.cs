using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Email.Templates;
using Wombat.Application.Common.Interfaces;
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
/// T373 (flow 11): the invitations list's order (L04, L12), "Expired, not used" (Q9, L14), Invite a person's prefill (N19,
/// N20) and the invitation's mail (S12).
/// </summary>
public sealed class InvitationListsAndMailTests
{
    private const int Kgk = 1;
    private const int Sun = 2;

    // 2026-11-10 09:00 SAST.
    private static readonly DateTimeOffset Now = new(2026, 11, 10, 7, 0, 0, TimeSpan.Zero);

    private readonly string _databaseName = Guid.NewGuid().ToString();

    // ─── The active list's order ─────────────────────────────────────────────

    [Fact]
    public async Task TheRowsNotDelivered_ComeFirst_NewestFirstAmongThem_ThenTheRestNewestFirst()
    {
        await using var db = await SeededAsync();
        var lastDay = new DateOnly(2026, 11, 20);
        Seed(db, "sent-old@kgk.test", Kgk, Now.UtcDateTime.AddHours(-5), lastDay, sentOn: Now.UtcDateTime.AddHours(-5));
        Seed(db, "sending@kgk.test", Kgk, Now.UtcDateTime.AddMinutes(-10), lastDay);
        Seed(db, "dropped-old@kgk.test", Kgk, Now.UtcDateTime.AddHours(-6), lastDay, failedOn: Now.UtcDateTime.AddHours(-5));
        Seed(db, "sent-new@kgk.test", Kgk, Now.UtcDateTime.AddHours(-2), lastDay, sentOn: Now.UtcDateTime.AddHours(-2));
        Seed(db, "unheard@kgk.test", Kgk, Now.UtcDateTime.AddHours(-3), lastDay);

        var rows = await new ListActiveInvitationsQueryHandler(db, new FixedClock(Now))
            .Handle(new ListActiveInvitationsQuery(TestPrincipals.InstitutionalAdmin(Kgk)), CancellationToken.None);

        rows.Select(row => row.Email).Should().Equal(
            "unheard@kgk.test", "dropped-old@kgk.test", "sending@kgk.test", "sent-new@kgk.test", "sent-old@kgk.test");
    }

    // ─── Expired, not used (Q9) ──────────────────────────────────────────────

    [Fact]
    public async Task AnExpiredInvitation_IsListedFor30SouthAfricanDaysAfterItsLastDay_NewestFirst_InScope()
    {
        await using var db = await SeededAsync();
        Seed(db, "yesterday@kgk.test", Kgk, Now.UtcDateTime.AddDays(-15), new DateOnly(2026, 11, 9));
        Seed(db, "day-29@kgk.test", Kgk, Now.UtcDateTime.AddDays(-45), new DateOnly(2026, 10, 12));
        Seed(db, "day-30@kgk.test", Kgk, Now.UtcDateTime.AddDays(-46), new DateOnly(2026, 10, 11));
        Seed(db, "still-works@kgk.test", Kgk, Now.UtcDateTime.AddDays(-14), new DateOnly(2026, 11, 10));
        Seed(db, "elsewhere@sun.test", Sun, Now.UtcDateTime.AddDays(-15), new DateOnly(2026, 11, 9));
        var revoked = Seed(db, "revoked@kgk.test", Kgk, Now.UtcDateTime.AddDays(-15), new DateOnly(2026, 11, 9));
        revoked.RevokedOn = Now.UtcDateTime.AddDays(-14);
        var used = Seed(db, "used@kgk.test", Kgk, Now.UtcDateTime.AddDays(-15), new DateOnly(2026, 11, 9));
        used.UsedOn = Now.UtcDateTime.AddDays(-14);
        await db.SaveChangesAsync();

        var rows = await Expired(db, TestPrincipals.InstitutionalAdmin(Kgk));

        rows.Select(row => row.Email).Should().Equal("yesterday@kgk.test", "day-29@kgk.test");
        rows[0].InstitutionName.Should().Be("Kgosi Kgari Teaching Hospital");
        (await Expired(db, TestPrincipals.Administrator())).Select(row => row.Email)
            .Should().BeEquivalentTo("yesterday@kgk.test", "day-29@kgk.test", "elsewhere@sun.test");
    }

    [Fact]
    public async Task TheLastDay_IsJudgedOnTheSouthAfricanCalendar()
    {
        await using var db = await SeededAsync();
        Seed(db, "lastday@kgk.test", Kgk, Now.UtcDateTime.AddDays(-14), new DateOnly(2026, 11, 10));

        // 21:59 UTC on the last day is 23:59 SAST: still working. 22:00 UTC is the next South African day.
        (await Expired(db, TestPrincipals.InstitutionalAdmin(Kgk), new DateTimeOffset(2026, 11, 10, 21, 59, 0, TimeSpan.Zero)))
            .Should().BeEmpty();
        (await Expired(db, TestPrincipals.InstitutionalAdmin(Kgk), new DateTimeOffset(2026, 11, 10, 22, 0, 0, TimeSpan.Zero)))
            .Should().ContainSingle();
    }

    [Fact]
    public async Task AnExpiredInvitation_LeavesTheList_OnceANewInvitationGoesToItsAddress_InAnyCase_Anywhere()
    {
        await using var db = await SeededAsync();
        Seed(db, "duplessis@kgk.wombat.local", Kgk, Now.UtcDateTime.AddDays(-20), new DateOnly(2026, 11, 2));
        Seed(db, "kept@kgk.test", Kgk, Now.UtcDateTime.AddDays(-20), new DateOnly(2026, 11, 2));
        // A newer invitation at another institution, revoked since: it still went to the address.
        var newer = Seed(db, "DuPlessis@KGK.wombat.local", Sun, Now.UtcDateTime.AddDays(-1), new DateOnly(2026, 11, 23));
        newer.RevokedOn = Now.UtcDateTime;
        await db.SaveChangesAsync();

        (await Expired(db, TestPrincipals.InstitutionalAdmin(Kgk))).Select(row => row.Email).Should().Equal("kept@kgk.test");
    }

    /// <summary>
    /// T373 fix pass, B4: where the database's lower-casing and <see cref="Invitation.NormaliseAddress" /> disagree on an
    /// address, the row is still listed, and the read never throws (it took the whole invitations page with it). The
    /// in-memory provider lowers in the current culture, so Turkish's dotless ı stands in for Postgres's locale here.
    /// </summary>
    [Fact]
    public async Task AnAddressTheTwoLowerCasingsDisagreeAbout_IsStillListed_NeverAFault()
    {
        await using var db = await SeededAsync();
        Seed(db, "Ilker@kgk.test", Kgk, Now.UtcDateTime.AddDays(-20), new DateOnly(2026, 11, 2));

        var culture = System.Globalization.CultureInfo.CurrentCulture;
        System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("tr-TR");
        try
        {
            "I".ToLower().Should().NotBe("I".ToLowerInvariant(), "guard: the two lower-casings disagree");
            (await Expired(db, TestPrincipals.InstitutionalAdmin(Kgk))).Select(row => row.Email).Should().Equal("Ilker@kgk.test");
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = culture;
        }
    }

    [Fact]
    public async Task AnExpiredInvitation_LeavesTheList_OnceAnAccountHoldsItsAddress()
    {
        await using var db = await SeededAsync();
        Seed(db, "Duplessis@kgk.wombat.local", Kgk, Now.UtcDateTime.AddDays(-20), new DateOnly(2026, 11, 2));
        Seed(db, "kept@kgk.test", Kgk, Now.UtcDateTime.AddDays(-20), new DateOnly(2026, 11, 2));
        var users = new FakeUserDirectory().With(new UserIdentityDetails(
            "dp-1", "duplessis@kgk.wombat.local", "Thandi", "du Plessis", Kgk, [], [], [WombatRoles.PendingTrainee]));

        (await Expired(db, TestPrincipals.InstitutionalAdmin(Kgk), users: users)).Select(row => row.Email).Should().Equal("kept@kgk.test");
    }

    // ─── The prefill (N19, N20) ──────────────────────────────────────────────

    [Fact]
    public async Task ARevokedInvitation_FillsItsRoleAndScope_InScope()
    {
        await using var db = await SeededAsync();
        var revoked = Seed(db, "ndlvou@kgk.wombat.local", Kgk, Now.UtcDateTime.AddDays(-1), new DateOnly(2026, 11, 23));
        revoked.SpecialityId = 5;
        revoked.SubSpecialityId = 6;
        revoked.RevokedOn = Now.UtcDateTime;
        await db.SaveChangesAsync();

        var prefill = await Prefill(db, revoked.Id, TestPrincipals.InstitutionalAdmin(Kgk));

        prefill.Should().NotBeNull();
        prefill!.Source.Should().Be(InvitationPrefillSource.Revoked);
        prefill.TargetRole.Should().Be(WombatRoles.Trainee);
        prefill.InstitutionId.Should().Be(Kgk);
        prefill.SpecialityId.Should().Be(5);
        prefill.SubSpecialityId.Should().Be(6);
        prefill.Email.Should().Be("ndlvou@kgk.wombat.local");
    }

    [Fact]
    public async Task AnExpiredInvitation_FillsAll_AndOneThatStillWorksOrWasUsed_FillsNothing()
    {
        await using var db = await SeededAsync();
        var expired = Seed(db, "duplessis@kgk.wombat.local", Kgk, Now.UtcDateTime.AddDays(-20), new DateOnly(2026, 11, 2));
        var active = Seed(db, "molefe@kgk.wombat.local", Kgk, Now.UtcDateTime.AddDays(-1), new DateOnly(2026, 11, 23));
        var used = Seed(db, "dlamini@kgk.wombat.local", Kgk, Now.UtcDateTime.AddDays(-1), new DateOnly(2026, 11, 23));
        used.UsedOn = Now.UtcDateTime;
        await db.SaveChangesAsync();

        var prefill = await Prefill(db, expired.Id, TestPrincipals.InstitutionalAdmin(Kgk));
        prefill!.Source.Should().Be(InvitationPrefillSource.Expired);
        prefill.ExpiresOn.Should().Be(new DateOnly(2026, 11, 2));

        (await Prefill(db, active.Id, TestPrincipals.InstitutionalAdmin(Kgk))).Should().BeNull();
        (await Prefill(db, used.Id, TestPrincipals.InstitutionalAdmin(Kgk))).Should().BeNull();
    }

    [Fact]
    public async Task AnInvitationOutOfScope_OrUnknown_FillsNothing()
    {
        await using var db = await SeededAsync();
        var elsewhere = Seed(db, "duplessis@sun.wombat.local", Sun, Now.UtcDateTime.AddDays(-20), new DateOnly(2026, 11, 2));
        var college = Seed(db, "kruger@cmsa.wombat.local", null, Now.UtcDateTime.AddDays(-20), new DateOnly(2026, 11, 2));
        college.CollegeId = 7;
        college.TargetRole = WombatRoles.CollegeAdmin;
        await db.SaveChangesAsync();

        (await Prefill(db, elsewhere.Id, TestPrincipals.InstitutionalAdmin(Kgk))).Should().BeNull();
        (await Prefill(db, college.Id, TestPrincipals.InstitutionalAdmin(Kgk))).Should().BeNull();
        (await Prefill(db, 999, TestPrincipals.InstitutionalAdmin(Kgk))).Should().BeNull();
        (await Prefill(db, elsewhere.Id, TestPrincipals.Administrator())).Should().NotBeNull();
        (await Prefill(db, college.Id, TestPrincipals.Administrator())).Should().NotBeNull();
    }

    [Fact]
    public async Task AHolderOfTrainee_IsRefusedBeforeTheIdIsRead()
    {
        await using var db = await SeededAsync();
        var caller = TestPrincipals.InRoles([WombatRoles.InstitutionalAdmin, WombatRoles.Trainee], "both", Kgk);

        var act = () => Prefill(db, 999, caller);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    // ─── The mail (S12) ──────────────────────────────────────────────────────

    [Fact]
    public void TheMail_SaysTheRolesLabelAndScope_AndHowLongTheLinkWorks_AndNeverTheRolesKey()
    {
        var mail = InvitationEmail.Build(
            "mbatha@kgk.wombat.local",
            InvitationScopeWords.RoleAndScope(WombatRoles.InstitutionalAdmin, "Kgosi Kgari Teaching Hospital", null, null, null),
            "https://wombat.test/account/register/start?token=abc",
            new DateOnly(2026, 11, 2),
            Invitation.DeliveryKey(4, new string('b', 64)));

        mail.Subject.Should().Be("Your Wombat invitation");
        foreach (var body in new[] { mail.TextBody, mail.HtmlBody })
        {
            body.Should().Contain("You are invited to Wombat as Institutional admin at Kgosi Kgari Teaching Hospital.");
            body.Should().Contain("Register with this link; it works until 2026-11-02.");
            body.Should().Contain("https://wombat.test/account/register/start?token=abc");
            body.Should().NotContain(WombatRoles.InstitutionalAdmin).And.NotContain("expires");
        }

        mail.Tags.Should().Equal("invitation");
    }

    [Fact]
    public async Task AnIssuedMail_NamesTheScopeTheInvitationIsFor()
    {
        await using var db = await SeededAsync();
        db.Specialities.Add(new Speciality { Id = 5, CollegeId = 7, Name = "Paediatrics", IsActive = true });
        db.SubSpecialities.Add(new SubSpeciality { Id = 6, SpecialityId = 5, Name = "Paediatrics", IsActive = true });
        await db.SaveChangesAsync();
        var sender = new CapturingSender();

        await new IssueInvitationCommandHandler(
                db, new InvitationTokenService(), sender, FakeUserDirectory.Empty,
                Microsoft.Extensions.Options.Options.Create(new Wombat.Application.Common.Options.WombatOptions { BaseUrl = "https://wombat.test" }),
                new FixedClock(Now))
            .Handle(new IssueInvitationCommand(
                "molefe@kgk.wombat.local", WombatRoles.Trainee, Kgk, null, 5, 6, "admin-1", TestPrincipals.InstitutionalAdmin(Kgk)),
                CancellationToken.None);

        sender.Sent.Should().ContainSingle().Which.TextBody.Should().Contain(
            "You are invited to Wombat as Trainee in Paediatrics / Paediatrics at Kgosi Kgari Teaching Hospital.");
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private Task<IReadOnlyList<ExpiredInvitationDto>> Expired(
        ApplicationDbContext db,
        System.Security.Claims.ClaimsPrincipal caller,
        DateTimeOffset? now = null,
        IUserAdministrationService? users = null)
        => new ListExpiredUnusedInvitationsQueryHandler(db, users ?? FakeUserDirectory.Empty, new FixedClock(now ?? Now))
            .Handle(new ListExpiredUnusedInvitationsQuery(caller), CancellationToken.None);

    private Task<InvitationPrefillDto?> Prefill(ApplicationDbContext db, int id, System.Security.Claims.ClaimsPrincipal caller)
        => new GetInvitationPrefillQueryHandler(db, new FixedClock(Now))
            .Handle(new GetInvitationPrefillQuery(id, caller), CancellationToken.None);

    private static Invitation Seed(
        ApplicationDbContext db,
        string email,
        int? institutionId,
        DateTime issuedOn,
        DateOnly expiresOn,
        DateTime? sentOn = null,
        DateTime? failedOn = null)
    {
        var invitation = new Invitation
        {
            Email = email,
            TokenHash = new string('a', 64),
            TargetRole = WombatRoles.Trainee,
            InstitutionId = institutionId,
            IssuedByUserId = "someone",
            IssuedOn = issuedOn,
            ExpiresOn = expiresOn,
            SentOn = sentOn,
            DeliveryFailedOn = failedOn
        };
        db.Set<Invitation>().Add(invitation);
        db.SaveChanges();
        return invitation;
    }

    private async Task<ApplicationDbContext> SeededAsync()
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(_databaseName).Options);
        db.Institutions.AddRange(
            new Institution { Id = Kgk, Name = "Kgosi Kgari Teaching Hospital", ShortCode = "KGK", IsActive = true, CreatedOn = DateTime.UtcNow },
            new Institution { Id = Sun, Name = "Sun Hospital", ShortCode = "SUN", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.Colleges.Add(new College { Id = 7, Name = "College of Paediatricians of South Africa", ShortCode = "CPSA", IsActive = true, CreatedOn = DateTime.UtcNow });
        await db.SaveChangesAsync();
        return db;
    }

    private sealed class CapturingSender : Wombat.Application.Common.Interfaces.IEmailSender
    {
        public List<Wombat.Application.Common.Email.EmailMessage> Sent { get; } = [];

        public Task SendAsync(Wombat.Application.Common.Email.EmailMessage message, CancellationToken cancellationToken = default)
        {
            Sent.Add(message);
            return Task.CompletedTask;
        }
    }

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
