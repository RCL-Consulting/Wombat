using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Invitations;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Identity;
using Wombat.Domain.Invitations;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Invitations;

/// <summary>
/// T373 (flow 11, build-contracts § 0): what the three lanes share about an invitation. The scope in the page's words
/// (Register's card, the mail, the lists), an address compared as one spelling (S4), and a link's last day on the South
/// African calendar wherever a link is used or listed (F7).
/// </summary>
public sealed class InvitationSharedWordsTests
{
    private readonly InvitationTokenService _tokenService = new();

    // ─── The scope, in the page's words ──────────────────────────────────────

    [Theory]
    [InlineData("Paediatrics", "Paediatrics", "Paediatrics / Paediatrics")]
    [InlineData("Paediatrics", null, "Paediatrics")]
    [InlineData(null, null, null)]
    [InlineData("  ", null, null)]
    public void TheDisciplines_AreTheSpecialityOverItsSubSpeciality(string? speciality, string? subSpeciality, string? expected)
    {
        InvitationScopeWords.Disciplines(speciality, subSpeciality).Should().Be(expected);
    }

    /// <summary>
    /// Steps 2.8 and 1.8 (R3-0-steps): "in ‹speciality›[ / ‹sub-speciality›]" only when the invitation has one, then "at
    /// ‹institution›", or "at ‹College›" for a College admin; the role by its label, never its key (Q13, S12).
    /// </summary>
    [Theory]
    [InlineData(WombatRoles.Coordinator, "Kgosi Kgari Teaching Hospital", null, null, null,
        "Coordinator at Kgosi Kgari Teaching Hospital")]
    [InlineData(WombatRoles.InstitutionalAdmin, "Kgosi Kgari Teaching Hospital", null, null, null,
        "Institutional admin at Kgosi Kgari Teaching Hospital")]
    [InlineData(WombatRoles.CollegeAdmin, null, "College of Paediatricians of South Africa", null, null,
        "College admin at College of Paediatricians of South Africa")]
    [InlineData(WombatRoles.Assessor, "Kgosi Kgari Teaching Hospital", null, "Paediatrics", "Paediatrics",
        "Assessor in Paediatrics / Paediatrics at Kgosi Kgari Teaching Hospital")]
    [InlineData(WombatRoles.CommitteeMember, "Kgosi Kgari Teaching Hospital", null, "Paediatrics", null,
        "Committee member in Paediatrics at Kgosi Kgari Teaching Hospital")]
    [InlineData(WombatRoles.SubSpecialityAdmin, null, null, "Paediatrics", "Paediatrics",
        "Sub-speciality admin in Paediatrics / Paediatrics")]
    public void TheRoleAndScope_ReadAsTheRegisterCardAndTheMailSayThem(
        string role, string? institution, string? college, string? speciality, string? subSpeciality, string expected)
    {
        InvitationScopeWords.RoleAndScope(role, institution, college, speciality, subSpeciality).Should().Be(expected);
    }

    // ─── One address, one spelling (S4) ──────────────────────────────────────

    [Fact]
    public async Task AnInvitationsAddress_IsFoundWhateverItsCase_AndTheStoredSpellingIsKept()
    {
        await using var db = CreateDb();
        db.Set<Invitation>().AddRange(
            NewInvitation("Molefe@KGK.wombat.local", DateTime.UtcNow, new DateOnly(2026, 11, 2)),
            NewInvitation("dlamini@kgk.wombat.local", DateTime.UtcNow, new DateOnly(2026, 11, 2)));
        await db.SaveChangesAsync();

        var found = await db.Set<Invitation>().ToAddress("  molefe@kgk.WOMBAT.local ").ToListAsync();

        found.Should().ContainSingle().Which.Email.Should().Be("Molefe@KGK.wombat.local");
    }

    // ─── A link's last day, on the South African calendar (F7) ───────────────

    /// <summary>
    /// 2026-11-02 22:00 UTC is 2026-11-03 00:00 SAST: the link whose last day was 2026-11-02 has stopped working, though the
    /// UTC date is still its last day. Until T373 the preview and the accept read it as usable until 02:00 SAST.
    /// </summary>
    [Theory]
    [InlineData(21, 59, false)]
    [InlineData(22, 0, true)]
    public async Task ThePreview_RefusesALinkAsExpired_OnceTheSouthAfricanDayIsPastItsLastDay(int hour, int minute, bool refused)
    {
        await using var db = CreateDb();
        var token = _tokenService.GenerateToken();
        var invitation = NewInvitation("invitee@example.test", new DateTime(2026, 10, 19, 7, 22, 0, DateTimeKind.Utc), new DateOnly(2026, 11, 2));
        invitation.TokenHash = _tokenService.HashToken(token);
        db.Set<Invitation>().Add(invitation);
        await db.SaveChangesAsync();

        var clock = new FixedClock(new DateTimeOffset(2026, 11, 2, hour, minute, 0, TimeSpan.Zero));
        var act = () => new GetInvitationPreviewQueryHandler(db, _tokenService, new AvailableAddresses(), clock)
            .Handle(new GetInvitationPreviewQuery(token), CancellationToken.None);

        if (refused)
        {
            (await act.Should().ThrowAsync<InvitationRefusedException>()).Which.Reason.Should().Be(InvitationRefusal.Expired);
        }
        else
        {
            await act.Should().NotThrowAsync();
        }
    }

    [Theory]
    [InlineData(21, 59, 1)]
    [InlineData(22, 0, 0)]
    public async Task TheActiveList_KeepsALinkThroughItsLastSouthAfricanDay(int hour, int minute, int listed)
    {
        await using var db = CreateDb();
        db.Set<Invitation>().Add(NewInvitation("invitee@example.test", new DateTime(2026, 10, 19, 7, 22, 0, DateTimeKind.Utc), new DateOnly(2026, 11, 2)));
        await db.SaveChangesAsync();

        var clock = new FixedClock(new DateTimeOffset(2026, 11, 2, hour, minute, 0, TimeSpan.Zero));
        var rows = await new ListActiveInvitationsQueryHandler(db, clock)
            .Handle(new ListActiveInvitationsQuery(TestPrincipals.Administrator()), CancellationToken.None);

        rows.Should().HaveCount(listed);
    }

    private static Invitation NewInvitation(string email, DateTime issuedOn, DateOnly expiresOn) => new()
    {
        Email = email,
        TokenHash = new string('a', 64),
        TargetRole = WombatRoles.Assessor,
        InstitutionId = 1,
        SpecialityId = 2,
        SubSpecialityId = 3,
        IssuedByUserId = "admin-1",
        IssuedOn = issuedOn,
        ExpiresOn = expiresOn
    };

    private static ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class AvailableAddresses : IInvitedUserProvisioner
    {
        public Task<InvitedAddressStatus> GetAddressStatusAsync(string email, CancellationToken cancellationToken = default)
            => Task.FromResult(InvitedAddressStatus.Available);

        public Task<ProvisionedInvitationUser> ProvisionAsync(
            string email, string password, string firstName, string lastName, string targetRole, int? institutionId,
            int? collegeId, int? specialityId, int? subSpecialityId, CancellationToken cancellationToken = default)
            => throw new NotSupportedException("A preview creates no account.");

        public Task<T> InOneTransactionAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken cancellationToken = default)
            => work(cancellationToken);
    }
}
