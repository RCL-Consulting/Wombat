using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Audit;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Invitations;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Domain.Invitations;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Invitations;

/// <summary>
/// T373 (flow 11, lane G): what Register reads and what its submit does. The preview carries the scope's names and the
/// link's last day, so the card says "as Assessor in Paediatrics / Paediatrics at …" and "its link works until …"; the
/// accept judges the last day on the South African calendar (F7), sweeps the address's other invitations whatever their
/// spelling (S4), and provisions the account and marks the invitation used in one transaction (F10).
/// </summary>
public sealed class RegisterInvitationTests
{
    private readonly InvitationTokenService _tokenService = new();

    // ─── The preview ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ThePreview_NamesTheInstitutionSpecialityAndSubSpeciality_AndTheLinksLastDay()
    {
        await using var db = CreateDb();
        db.Set<Institution>().Add(new Institution { Id = 1, Name = "Kgosi Kgari Teaching Hospital", ShortCode = "KGK" });
        db.Set<Speciality>().Add(new Speciality { Id = 2, CollegeId = 9, Name = "Paediatrics" });
        db.Set<SubSpeciality>().Add(new SubSpeciality { Id = 3, SpecialityId = 2, Name = "Neonatology" });
        var token = await AddInvitationAsync(db, "patel@kgk.wombat.local", invitation =>
        {
            invitation.InstitutionId = 1;
            invitation.SpecialityId = 2;
            invitation.SubSpecialityId = 3;
        });

        var preview = await new GetInvitationPreviewQueryHandler(db, _tokenService, new Provisioner(db))
            .Handle(new GetInvitationPreviewQuery(token), CancellationToken.None);

        preview.Should().Be(new InvitationPreviewDto(
            "patel@kgk.wombat.local", WombatRoles.Assessor, "Kgosi Kgari Teaching Hospital", null, "Paediatrics", "Neonatology",
            new DateOnly(2026, 11, 2)));
    }

    [Fact]
    public async Task ThePreview_OfACollegeAdminsInvitation_NamesTheCollege_AndNoInstitution()
    {
        await using var db = CreateDb();
        db.Set<College>().Add(new College { Id = 4, Name = "College of Paediatricians of South Africa", ShortCode = "CPSA" });
        var token = await AddInvitationAsync(db, "kruger@cmsa.wombat.local", invitation =>
        {
            invitation.TargetRole = WombatRoles.CollegeAdmin;
            invitation.InstitutionId = null;
            invitation.CollegeId = 4;
        });

        var preview = await new GetInvitationPreviewQueryHandler(db, _tokenService, new Provisioner(db))
            .Handle(new GetInvitationPreviewQuery(token), CancellationToken.None);

        preview.InstitutionName.Should().BeNull();
        preview.CollegeName.Should().Be("College of Paediatricians of South Africa");
        preview.SpecialityName.Should().BeNull();
        preview.SubSpecialityName.Should().BeNull();
        InvitationScopeWords.RoleAndScope(preview.TargetRole, preview.InstitutionName, preview.CollegeName,
                preview.SpecialityName, preview.SubSpecialityName)
            .Should().Be("College admin at College of Paediatricians of South Africa", "Step 1.8's card");
    }

    /// <summary>G10: "its link worked until 2026-11-02": the expired refusal carries the day, and its message stays the same.</summary>
    [Fact]
    public async Task ThePreviewsExpiredRefusal_CarriesTheLinksLastDay()
    {
        await using var db = CreateDb();
        var token = await AddInvitationAsync(db, "smit@kgk.wombat.local");
        var clock = new FixedClock(new DateTimeOffset(2026, 11, 2, 22, 0, 0, TimeSpan.Zero));

        var act = () => new GetInvitationPreviewQueryHandler(db, _tokenService, new Provisioner(db), clock)
            .Handle(new GetInvitationPreviewQuery(token), CancellationToken.None);

        var refused = (await act.Should().ThrowAsync<InvitationRefusedException>()).Which;
        refused.Reason.Should().Be(InvitationRefusal.Expired);
        refused.LastDay.Should().Be(new DateOnly(2026, 11, 2));
        refused.Message.Should().Be("This invitation has expired.", "the audit row's words are unchanged");
    }

    // ─── The accept: the last day on the South African calendar (F7) ─────────

    /// <summary>
    /// 2026-11-02 22:00 UTC is 2026-11-03 00:00 SAST: a link whose last day was 2026-11-02 no longer registers anyone,
    /// though the UTC date is still its last day; a minute earlier it does, and its UsedOn is the clock's.
    /// </summary>
    [Theory]
    [InlineData(21, 59, false)]
    [InlineData(22, 0, true)]
    public async Task TheAccept_RefusesALinkAsExpired_OnceTheSouthAfricanDayIsPastItsLastDay(int hour, int minute, bool refused)
    {
        await using var db = CreateDb();
        var token = await AddInvitationAsync(db, "smit@kgk.wombat.local");
        var now = new DateTimeOffset(2026, 11, 2, hour, minute, 0, TimeSpan.Zero);
        var provisioner = new Provisioner(db);

        var act = () => Handler(db, provisioner, new FixedClock(now)).Handle(Accept(token), CancellationToken.None);

        if (refused)
        {
            (await act.Should().ThrowAsync<InvitationRefusedException>()).Which.Reason.Should().Be(InvitationRefusal.Expired);
            provisioner.Events.Should().BeEmpty("nothing is provisioned for a link that no longer works");
            (await db.Set<Invitation>().SingleAsync()).UsedOn.Should().BeNull();
        }
        else
        {
            await act.Should().NotThrowAsync();
            (await db.Set<Invitation>().SingleAsync()).UsedOn.Should().Be(now.UtcDateTime);
        }
    }

    // ─── The accept: the sweep, whatever the spelling (S4) ───────────────────

    [Fact]
    public async Task TheAccept_RevokesTheAddresssOtherActiveInvitations_InAnyCase_AndLeavesOtherAddressesAlone()
    {
        await using var db = CreateDb();
        var token = await AddInvitationAsync(db, "smit@kgk.wombat.local");
        var otherCase = await AddOtherAsync(db, "  Smit@KGK.wombat.local");
        var otherAddress = await AddOtherAsync(db, "smith@kgk.wombat.local");

        await Handler(db, new Provisioner(db), new FixedClock(new DateTimeOffset(2026, 10, 20, 8, 0, 0, TimeSpan.Zero)))
            .Handle(Accept(token), CancellationToken.None);

        (await db.Set<Invitation>().SingleAsync(entity => entity.Id == otherCase)).RevokedOn.Should().NotBeNull(
            "Smit@KGK… is the same address: until T373 it was swept only when spelt the same");
        (await db.Set<Invitation>().SingleAsync(entity => entity.Id == otherAddress)).RevokedOn.Should().BeNull();
    }

    // ─── The accept: one transaction (F10) ───────────────────────────────────

    [Fact]
    public async Task TheAccept_ProvisionsAndMarksTheInvitationUsed_InsideTheProvisionersOneTransaction()
    {
        await using var db = CreateDb();
        var token = await AddInvitationAsync(db, "smit@kgk.wombat.local");
        var provisioner = new Provisioner(db);

        await Handler(db, provisioner, new FixedClock(new DateTimeOffset(2026, 10, 20, 8, 0, 0, TimeSpan.Zero)))
            .Handle(Accept(token), CancellationToken.None);

        provisioner.Events.Should().Equal("begin", "provision", "end: used");
    }

    [Fact]
    public async Task AFaultAfterTheAccountIsProvisioned_LeavesTheTransaction_WithTheFault()
    {
        await using var db = CreateDb();
        var token = await AddInvitationAsync(db, "smit@kgk.wombat.local");
        var fault = new InvalidOperationException("the database went away");
        var provisioner = new Provisioner(db) { FaultAfterProvisioning = fault };

        var act = () => Handler(db, provisioner, new FixedClock(new DateTimeOffset(2026, 10, 20, 8, 0, 0, TimeSpan.Zero)))
            .Handle(Accept(token), CancellationToken.None);

        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Should().BeSameAs(fault);
        provisioner.Events.Should().Equal("begin", "provision", "faulted");
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private AcceptInvitationCommandHandler Handler(ApplicationDbContext db, Provisioner provisioner, TimeProvider clock)
        => new(db, _tokenService, provisioner, new NullAuditContext(), clock);

    private static AcceptInvitationCommand Accept(string token) => new(token, "Registered-Pa55word!", "Pieter", "Smit");

    private async Task<string> AddInvitationAsync(ApplicationDbContext db, string email, Action<Invitation>? configure = null)
    {
        var token = _tokenService.GenerateToken();
        var invitation = new Invitation
        {
            Email = email,
            TokenHash = _tokenService.HashToken(token),
            TargetRole = WombatRoles.Assessor,
            InstitutionId = 1,
            IssuedByUserId = "admin-1",
            IssuedOn = new DateTime(2026, 10, 19, 7, 22, 0, DateTimeKind.Utc),
            ExpiresOn = new DateOnly(2026, 11, 2)
        };
        configure?.Invoke(invitation);
        db.Set<Invitation>().Add(invitation);
        await db.SaveChangesAsync();
        return token;
    }

    private static async Task<int> AddOtherAsync(ApplicationDbContext db, string email)
    {
        var invitation = new Invitation
        {
            Email = email.Trim(),
            TokenHash = Guid.NewGuid().ToString("N"),
            TargetRole = WombatRoles.Coordinator,
            InstitutionId = 1,
            IssuedByUserId = "admin-1",
            IssuedOn = new DateTime(2026, 10, 19, 6, 0, 0, DateTimeKind.Utc),
            ExpiresOn = new DateOnly(2026, 11, 2)
        };
        db.Set<Invitation>().Add(invitation);
        await db.SaveChangesAsync();
        return invitation.Id;
    }

    private static ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    /// <summary>
    /// Records the transaction's shape: when it begins, the provisioning inside it, and at its end whether the accept's own
    /// save had marked the invitation used by then (it must have, inside the work).
    /// </summary>
    private sealed class Provisioner(ApplicationDbContext db) : IInvitedUserProvisioner
    {
        private bool _inTransaction;

        public List<string> Events { get; } = [];

        public Exception? FaultAfterProvisioning { get; init; }

        public Task<InvitedAddressStatus> GetAddressStatusAsync(string email, CancellationToken cancellationToken = default)
            => Task.FromResult(InvitedAddressStatus.Available);

        public Task<ProvisionedInvitationUser> ProvisionAsync(
            string email, string password, string firstName, string lastName, string targetRole, int? institutionId,
            int? collegeId, int? specialityId, int? subSpecialityId, CancellationToken cancellationToken = default)
        {
            Events.Add(_inTransaction ? "provision" : "provision outside the transaction");
            return FaultAfterProvisioning is { } fault
                ? Task.FromException<ProvisionedInvitationUser>(fault)
                : Task.FromResult(new ProvisionedInvitationUser("provisioned-1", targetRole));
        }

        public async Task<T> InOneTransactionAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken cancellationToken = default)
        {
            Events.Add("begin");
            _inTransaction = true;
            try
            {
                var result = await work(cancellationToken);
                var used = await db.Set<Invitation>().AsNoTracking().AnyAsync(entity => entity.UsedOn != null, cancellationToken);
                Events.Add(used ? "end: used" : "end: not used");
                return result;
            }
            catch
            {
                Events.Add("faulted");
                throw;
            }
            finally
            {
                _inTransaction = false;
            }
        }
    }

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
}
