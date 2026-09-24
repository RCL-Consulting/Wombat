using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.EntrustmentDecisions;

/// <summary>
/// The entrustment-decision admin list and revoke answer only for trainees the caller administers, and the certificate
/// only for trainees the caller may read about, by T113's ladder (<c>TraineeScopeResolver</c>); an Administrator sees all
/// of them, the subject always has their own certificate, and the chair who issued a decision keeps their claim on it.
/// (T183)
/// </summary>
/// <remarks>
/// <para>
/// Before T183 each checked the caller's role and nothing else, so an InstitutionalAdmin, SpecialityAdmin or
/// SubSpecialityAdmin at any hospital could list every trainee's decisions in the country, revoke any of them, and
/// download any certificate; a Coordinator anywhere could download any certificate too.
/// </para>
/// <para>
/// A refused revoke must leave the store exactly as it was even after a save, because the audit pipeline saves the
/// request's DbContext from its catch: a revocation staged before the check would be committed by the refusal itself.
/// Every refusal below is followed by that save, the change tracker is cleared, and the decision is read back through a
/// second context.
/// </para>
/// </remarks>
public sealed class EntrustmentDecisionAdminScopeTests
{
    private const int HostInstitution = 1;
    private const int OtherInstitution = 2;
    private const int Paediatrics = 1;
    private const int Surgery = 2;
    private const int GeneralPaediatrics = 11;
    private const int GeneralSurgery = 21;
    private const int PaediatricsCurriculum = 100;
    private const int SurgeryCurriculum = 200;

    /// <summary>The subject of the decision every matrix row is asked about: a paediatric trainee at the host.</summary>
    private const string TraineeUserId = "trainee-1";

    /// <summary>A surgical trainee at the host: the host's InstitutionalAdmin oversees them, its paediatric admins do not.</summary>
    private const string SurgicalTraineeUserId = "trainee-surgery";

    /// <summary>A paediatric trainee at the other institution.</summary>
    private const string ElsewhereTraineeUserId = "trainee-elsewhere";

    /// <summary>
    /// Two past profiles and no current one: the host first, then the other institution. The shared tie-break (the
    /// highest id) says the other institution.
    /// </summary>
    private const string MovedTraineeUserId = "trainee-moved";

    private readonly string _databaseName = Guid.NewGuid().ToString();

    // ─── The matrix: every caller against the subject's decision ────────────

    private static readonly (string Caller, bool Lists, bool Revokes, bool Downloads)[] Callers =
    [
        ("a global Administrator", true, true, true),
        ("an InstitutionalAdmin of the trainee's institution", true, true, true),
        ("a SpecialityAdmin of the trainee's speciality at their institution", true, true, true),
        ("a SubSpecialityAdmin of the trainee's sub-speciality at their institution", true, true, true),
        ("an InstitutionalAdmin of another institution", false, false, false),
        // Speciality ids are national: holding the right one at the wrong hospital is not oversight.
        ("a SpecialityAdmin of the trainee's speciality at another institution", false, false, false),
        ("a SpecialityAdmin of another speciality at the trainee's institution", false, false, false),
        ("a SubSpecialityAdmin of the trainee's sub-speciality at another institution", false, false, false),
        ("an InstitutionalAdmin with no institution claim", false, false, false),
        // Users hold several roles. A committee seat or a coordinator's desk at the trainee's institution oversees the
        // trainee, so it reads the certificate; it does not make an admin role for another speciality administer them.
        ("a SpecialityAdmin of another speciality at the trainee's institution who also sits on its committee", false, false, true),
        ("a SubSpecialityAdmin of another sub-speciality at the trainee's institution who also coordinates there", false, false, true)
    ];

    /// <summary>Callers who hold no admin role: the list refuses them outright, as the page does.</summary>
    private static readonly (string Caller, bool Revokes, bool Downloads)[] NonAdminCallers =
    [
        ("the trainee themselves", false, true),
        ("the chair who issued it, from another institution", true, true),
        // A panel member's claim is read from nowhere: the panel's members can be replaced after the fact, and the ones
        // who belong there are CommitteeMembers of the institution it reviews, who oversee its trainees anyway.
        ("a member of the issuing panel, from another institution", false, false),
        ("a Coordinator at the trainee's institution", false, true),
        ("a Coordinator at another institution", false, false),
        ("a classmate at the same institution", false, false),
        ("an Assessor at the trainee's institution", false, false)
    ];

    public static TheoryData<string, bool> ListMatrix()
        => ToTheoryData(Callers.Select(row => (row.Caller, row.Lists)));

    public static TheoryData<string, bool> RevokeMatrix()
        => ToTheoryData(Callers.Select(row => (row.Caller, row.Revokes))
            .Concat(NonAdminCallers.Select(row => (row.Caller, row.Revokes))));

    public static TheoryData<string, bool> CertificateMatrix()
        => ToTheoryData(Callers.Select(row => (row.Caller, row.Downloads))
            .Concat(NonAdminCallers.Select(row => (row.Caller, row.Downloads))));

    public static TheoryData<string> NonAdmins()
    {
        var data = new TheoryData<string>();
        foreach (var row in NonAdminCallers)
        {
            data.Add(row.Caller);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(ListMatrix))]
    public async Task TheList_ShowsTheSubjectsDecision_OnlyToAnAdministratorOrAnAdminWhoAdministersThem(string caller, bool lists)
    {
        await using var db = CreateDb();
        var seeded = await SeedAsync(db);

        var ids = await ListAsync(db, Principal(caller));

        if (lists)
        {
            ids.Should().Contain(seeded[TraineeUserId], caller);
        }
        else
        {
            ids.Should().NotContain(seeded[TraineeUserId], caller);
        }
    }

    [Theory]
    [MemberData(nameof(NonAdmins))]
    public async Task TheList_RefusesAnyoneWithoutAnAdminRole(string caller)
    {
        await using var db = CreateDb();
        await SeedAsync(db);

        var act = () => ListAsync(db, Principal(caller));

        await act.Should().ThrowAsync<UnauthorizedAccessException>(caller);
    }

    [Theory]
    [MemberData(nameof(RevokeMatrix))]
    public async Task Revoke_IsAllowedOnlyToAnAdministratorTheChairWhoIssuedItOrAnAdminWhoAdministersTheTrainee(string caller, bool revokes)
    {
        await using var db = CreateDb();
        var seeded = await SeedAsync(db);
        var principal = Principal(caller);

        var act = () => RevokeAsync(db, seeded[TraineeUserId], principal);

        if (revokes)
        {
            await act.Should().NotThrowAsync(caller);
            var after = await ReadBackAsync(seeded[TraineeUserId]);
            after.Status.Should().Be(EntrustmentDecisionStatus.Revoked);
            after.RevokedByUserId.Should().Be(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        }
        else
        {
            await act.Should().ThrowAsync<UnauthorizedAccessException>(caller);
            await AssertTheRefusalChangedNothingAsync(db, seeded[TraineeUserId]);
        }
    }

    [Theory]
    [MemberData(nameof(CertificateMatrix))]
    public async Task TheCertificate_IsHandedOnlyToTheSubjectAnAdministratorAnOverseerOrTheChairWhoIssuedIt(string caller, bool downloads)
    {
        await using var db = CreateDb();
        var seeded = await SeedAsync(db);
        var pdf = new FakePdfService();

        var result = await new DownloadEntrustmentCertificateCommandHandler(db, pdf).Handle(
            new DownloadEntrustmentCertificateCommand(seeded[TraineeUserId], Principal(caller)), CancellationToken.None);

        if (downloads)
        {
            result.Should().NotBeNull(caller);
            pdf.Calls.Should().Be(1);
        }
        else
        {
            result.Should().BeNull(caller);
            pdf.Calls.Should().Be(0, "nothing is rendered for a caller who may not have it");
        }
    }

    // ─── The list ────────────────────────────────────────────────────────────

    [Fact]
    public async Task TheList_IsConfinedToTheCallersInstitution_AndFollowsTheSharedTieBreak()
    {
        await using var db = CreateDb();
        var seeded = await SeedAsync(db);

        (await ListAsync(db, TestPrincipals.InstitutionalAdmin(HostInstitution)))
            .Should().BeEquivalentTo([seeded[TraineeUserId], seeded[SurgicalTraineeUserId]]);

        // The moved trainee's older profile is at the host; the preferred one, the highest id, is not.
        (await ListAsync(db, TestPrincipals.InstitutionalAdmin(OtherInstitution)))
            .Should().BeEquivalentTo([seeded[ElsewhereTraineeUserId], seeded[MovedTraineeUserId]]);

        (await ListAsync(db, TestPrincipals.Administrator()))
            .Should().BeEquivalentTo(seeded.Values);
    }

    [Fact]
    public async Task TheList_ForASpecialityAdmin_HoldsOnlyTheirSpecialitysTraineesAtTheirInstitution()
    {
        await using var db = CreateDb();
        var seeded = await SeedAsync(db);

        (await ListAsync(db, Principal("a SpecialityAdmin of the trainee's speciality at their institution")))
            .Should().BeEquivalentTo([seeded[TraineeUserId]]);

        (await ListAsync(db, TestPrincipals.InRole(WombatRoles.SubSpecialityAdmin, "sub-surgery", HostInstitution, subSpecialityId: GeneralSurgery)))
            .Should().BeEquivalentTo([seeded[SurgicalTraineeUserId]]);
    }

    [Fact]
    public async Task TheList_ForAnAdminWhoAlsoHoldsAnOversightRole_HoldsOnlyWhatTheirAdminRoleAdministers()
    {
        // Every row the list shows carries a Revoke button, so it holds what the caller may revoke: what their admin
        // role administers, not everything a second role lets them read.
        await using var db = CreateDb();
        var seeded = await SeedAsync(db);

        (await ListAsync(db, Principal("a SpecialityAdmin of another speciality at the trainee's institution who also sits on its committee")))
            .Should().BeEquivalentTo([seeded[SurgicalTraineeUserId]]);

        (await ListAsync(db, Principal("a SubSpecialityAdmin of another sub-speciality at the trainee's institution who also coordinates there")))
            .Should().BeEquivalentTo([seeded[SurgicalTraineeUserId]]);

        (await ListAsync(db, TestPrincipals.InRoles(
                [WombatRoles.SpecialityAdmin, WombatRoles.CommitteeMember], "spec-host-committee", HostInstitution, specialityId: Paediatrics)))
            .Should().BeEquivalentTo([seeded[TraineeUserId]]);
    }

    [Fact]
    public async Task TheList_FilteredToATraineeOutOfScope_IsEmpty_AsForATraineeWithNoDecisions()
    {
        await using var db = CreateDb();
        await SeedAsync(db);

        var filtered = await new ListEntrustmentDecisionsForAdminQueryHandler(db, FakeUserDirectory.Empty).Handle(
            new ListEntrustmentDecisionsForAdminQuery(ElsewhereTraineeUserId, null, TestPrincipals.InstitutionalAdmin(HostInstitution)),
            CancellationToken.None);

        filtered.Should().BeEmpty();
    }

    // ─── Revoke ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Revoke_OfADecisionIdThatNamesNothing_IsRefusedExactlyAsAnotherInstitutionsDecisionIs()
    {
        await using var db = CreateDb();
        var seeded = await SeedAsync(db);

        var outOfScope = await RefusalAsync(() => RevokeAsync(db, seeded[TraineeUserId], TestPrincipals.InstitutionalAdmin(OtherInstitution)));
        var missing = await RefusalAsync(() => RevokeAsync(db, 9999, TestPrincipals.InstitutionalAdmin(OtherInstitution)));
        var ownMissing = await RefusalAsync(() => RevokeAsync(db, 9999, TestPrincipals.InstitutionalAdmin(HostInstitution)));

        outOfScope.Should().BeOfType<UnauthorizedAccessException>();
        missing.Should().BeOfType<UnauthorizedAccessException>().Which.Message.Should().Be(outOfScope.Message);
        ownMissing.Should().BeOfType<UnauthorizedAccessException>().Which.Message.Should().Be(outOfScope.Message);
    }

    [Fact]
    public async Task Revoke_ByAnAdministrator_OfADecisionIdThatNamesNothing_SaysSoPlainly()
    {
        await using var db = CreateDb();
        await SeedAsync(db);

        var missing = await RefusalAsync(() => RevokeAsync(db, 9999, TestPrincipals.Administrator()));

        missing.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Be("The entrustment decision could not be found.");
    }

    [Fact]
    public async Task Revoke_FollowsTheSharedTieBreak_ForATraineeWithTwoPastProfiles()
    {
        await using var db = CreateDb();
        var seeded = await SeedAsync(db);

        var host = () => RevokeAsync(db, seeded[MovedTraineeUserId], TestPrincipals.InstitutionalAdmin(HostInstitution));
        await host.Should().ThrowAsync<UnauthorizedAccessException>();
        await AssertTheRefusalChangedNothingAsync(db, seeded[MovedTraineeUserId]);

        await using var fresh = CreateDb();
        await RevokeAsync(fresh, seeded[MovedTraineeUserId], TestPrincipals.InstitutionalAdmin(OtherInstitution));
        (await ReadBackAsync(seeded[MovedTraineeUserId])).Status.Should().Be(EntrustmentDecisionStatus.Revoked);
    }

    [Fact]
    public async Task Revoke_AndTheCertificate_FollowTheChairWhoIssuedTheDecision_NotWhoeverSitsOnThePanelNow()
    {
        // A panel's members can be replaced after the decision is issued. Whoever is made chair or member since has no
        // claim on it; the chair who issued it keeps theirs, on the panel or off it.
        await using var db = CreateDb();
        var seeded = await SeedAsync(db);

        await new UpdateDecisionPanelCommandHandler(db, FakeUserDirectory.CommitteeMembersAt(HostInstitution, "chair-new", "member-new")).Handle(
            new UpdateDecisionPanelCommand(
                20,
                [
                    new DecisionPanelMemberInput("chair-new", DecisionPanelMemberRole.Chair),
                    new DecisionPanelMemberInput("member-new", DecisionPanelMemberRole.Member)
                ],
                TestPrincipals.Administrator()),
            CancellationToken.None);
        db.ChangeTracker.Clear();

        var newChair = TestPrincipals.InRole(WombatRoles.CommitteeMember, "chair-new", OtherInstitution);
        var newMember = TestPrincipals.InRole(WombatRoles.CommitteeMember, "member-new", OtherInstitution);
        var issuer = TestPrincipals.InRole(WombatRoles.CommitteeMember, "chair-1", OtherInstitution);
        var pdf = new FakePdfService();
        var certificates = new DownloadEntrustmentCertificateCommandHandler(db, pdf);

        var byNewChair = () => RevokeAsync(db, seeded[TraineeUserId], newChair);
        await byNewChair.Should().ThrowAsync<UnauthorizedAccessException>();
        await AssertTheRefusalChangedNothingAsync(db, seeded[TraineeUserId]);

        (await certificates.Handle(new DownloadEntrustmentCertificateCommand(seeded[TraineeUserId], newChair), CancellationToken.None))
            .Should().BeNull("being made chair since is no claim on a decision issued before");
        (await certificates.Handle(new DownloadEntrustmentCertificateCommand(seeded[TraineeUserId], newMember), CancellationToken.None))
            .Should().BeNull("nor is being made a member");
        pdf.Calls.Should().Be(0);

        (await certificates.Handle(new DownloadEntrustmentCertificateCommand(seeded[TraineeUserId], issuer), CancellationToken.None))
            .Should().NotBeNull("the chair who issued it keeps their claim off the panel");

        await using var fresh = CreateDb();
        await RevokeAsync(fresh, seeded[TraineeUserId], issuer);
        (await ReadBackAsync(seeded[TraineeUserId])).RevokedByUserId.Should().Be("chair-1");
    }

    // ─── The certificate ─────────────────────────────────────────────────────

    [Fact]
    public async Task TheCertificate_ForADecisionIdThatNamesNothing_IsNull_AsForOneOutOfScope()
    {
        await using var db = CreateDb();
        await SeedAsync(db);
        var pdf = new FakePdfService();

        var result = await new DownloadEntrustmentCertificateCommandHandler(db, pdf).Handle(
            new DownloadEntrustmentCertificateCommand(9999, TestPrincipals.Administrator()), CancellationToken.None);

        result.Should().BeNull();
        pdf.Calls.Should().Be(0);
    }

    [Fact]
    public async Task TheSubject_DownloadsTheirOwnCertificate_EvenWithNoProfileToPlaceThem()
    {
        await using var db = CreateDb();
        var seeded = await SeedAsync(db);
        var unplaced = await AddDecisionAsync(db, "trainee-unplaced");
        var pdf = new FakePdfService();
        var handler = new DownloadEntrustmentCertificateCommandHandler(db, pdf);

        (await handler.Handle(
                new DownloadEntrustmentCertificateCommand(unplaced, TestPrincipals.Trainee("trainee-unplaced", HostInstitution)),
                CancellationToken.None))
            .Should().NotBeNull();

        (await handler.Handle(
                new DownloadEntrustmentCertificateCommand(unplaced, TestPrincipals.InstitutionalAdmin(HostInstitution)),
                CancellationToken.None))
            .Should().BeNull("a trainee with no profile has no organisational home, so no admin oversees them");

        (await handler.Handle(
                new DownloadEntrustmentCertificateCommand(seeded[TraineeUserId], TestPrincipals.Trainee("trainee-unplaced", HostInstitution)),
                CancellationToken.None))
            .Should().BeNull("their own arm reaches their own certificate, not a classmate's");
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static ClaimsPrincipal Principal(string caller) => caller switch
    {
        "a global Administrator" => TestPrincipals.Administrator(),
        "an InstitutionalAdmin of the trainee's institution" => TestPrincipals.InstitutionalAdmin(HostInstitution, "inst-admin-host"),
        "a SpecialityAdmin of the trainee's speciality at their institution" =>
            TestPrincipals.InRole(WombatRoles.SpecialityAdmin, "spec-host", HostInstitution, specialityId: Paediatrics),
        "a SubSpecialityAdmin of the trainee's sub-speciality at their institution" =>
            TestPrincipals.InRole(WombatRoles.SubSpecialityAdmin, "sub-host", HostInstitution, subSpecialityId: GeneralPaediatrics),
        "an InstitutionalAdmin of another institution" => TestPrincipals.InstitutionalAdmin(OtherInstitution, "inst-admin-else"),
        "a SpecialityAdmin of the trainee's speciality at another institution" =>
            TestPrincipals.InRole(WombatRoles.SpecialityAdmin, "spec-else", OtherInstitution, specialityId: Paediatrics),
        "a SpecialityAdmin of another speciality at the trainee's institution" =>
            TestPrincipals.InRole(WombatRoles.SpecialityAdmin, "spec-surgery", HostInstitution, specialityId: Surgery),
        "a SubSpecialityAdmin of the trainee's sub-speciality at another institution" =>
            TestPrincipals.InRole(WombatRoles.SubSpecialityAdmin, "sub-else", OtherInstitution, subSpecialityId: GeneralPaediatrics),
        "an InstitutionalAdmin with no institution claim" =>
            TestPrincipals.InRole(WombatRoles.InstitutionalAdmin, "inst-admin-nowhere", institutionId: null),
        "a SpecialityAdmin of another speciality at the trainee's institution who also sits on its committee" =>
            TestPrincipals.InRoles(
                [WombatRoles.SpecialityAdmin, WombatRoles.CommitteeMember], "spec-surgery-committee", HostInstitution, specialityId: Surgery),
        "a SubSpecialityAdmin of another sub-speciality at the trainee's institution who also coordinates there" =>
            TestPrincipals.InRoles(
                [WombatRoles.SubSpecialityAdmin, WombatRoles.Coordinator], "sub-surgery-coordinator", HostInstitution, subSpecialityId: GeneralSurgery),
        "the trainee themselves" => TestPrincipals.Trainee(TraineeUserId, HostInstitution),
        "the chair who issued it, from another institution" =>
            TestPrincipals.InRole(WombatRoles.CommitteeMember, "chair-1", OtherInstitution),
        "a member of the issuing panel, from another institution" =>
            TestPrincipals.InRole(WombatRoles.CommitteeMember, "member-1", OtherInstitution),
        "a Coordinator at the trainee's institution" => TestPrincipals.Coordinator(HostInstitution, "coordinator-host"),
        "a Coordinator at another institution" => TestPrincipals.Coordinator(OtherInstitution, "coordinator-else"),
        // Every user carries an institution claim; a matching one must not widen anything on its own.
        "a classmate at the same institution" => TestPrincipals.Trainee("trainee-2", HostInstitution),
        "an Assessor at the trainee's institution" => TestPrincipals.InRole(WombatRoles.Assessor, "assessor-1", HostInstitution),
        _ => throw new ArgumentOutOfRangeException(nameof(caller), caller, null)
    };

    private static TheoryData<string, bool> ToTheoryData(IEnumerable<(string Caller, bool Expected)> rows)
    {
        var data = new TheoryData<string, bool>();
        foreach (var (caller, expected) in rows)
        {
            data.Add(caller, expected);
        }

        return data;
    }

    private static async Task<int[]> ListAsync(ApplicationDbContext db, ClaimsPrincipal principal)
        => (await new ListEntrustmentDecisionsForAdminQueryHandler(db, FakeUserDirectory.Empty).Handle(
                new ListEntrustmentDecisionsForAdminQuery(null, null, principal), CancellationToken.None))
            .Select(decision => decision.Id)
            .ToArray();

    private static Task<EntrustmentDecisionDto> RevokeAsync(ApplicationDbContext db, int decisionId, ClaimsPrincipal principal)
        => new RevokeEntrustmentDecisionCommandHandler(db).Handle(
            new RevokeEntrustmentDecisionCommand(decisionId, "Concern raised at the site.", principal), CancellationToken.None);

    private static async Task<Exception> RefusalAsync(Func<Task> act)
    {
        var thrown = await act.Should().ThrowAsync<Exception>();
        return thrown.Which;
    }

    /// <summary>
    /// What the audit pipeline does after a handler throws: save the request's context. Then the tracker is cleared and
    /// the decision read back through a second context, so only what reached the store is seen.
    /// </summary>
    private async Task AssertTheRefusalChangedNothingAsync(ApplicationDbContext db, int decisionId)
    {
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var after = await ReadBackAsync(decisionId);
        after.Status.Should().Be(EntrustmentDecisionStatus.Active);
        after.RevokedOn.Should().BeNull();
        after.RevokedByUserId.Should().BeNull();
        after.RevocationReason.Should().BeNull();
    }

    private async Task<EntrustmentDecision> ReadBackAsync(int decisionId)
    {
        await using var fresh = CreateDb();
        return await fresh.EntrustmentDecisions.AsNoTracking().SingleAsync(decision => decision.Id == decisionId);
    }

    /// <summary>One active decision for each of the four trainees, keyed by trainee.</summary>
    private static async Task<Dictionary<string, int>> SeedAsync(ApplicationDbContext db)
    {
        db.Institutions.Add(new Institution { Id = HostInstitution, Name = "Host" });
        db.Institutions.Add(new Institution { Id = OtherInstitution, Name = "Elsewhere" });
        db.Specialities.Add(new Speciality { Id = Paediatrics, CollegeId = 1, Name = "Paediatrics" });
        db.Specialities.Add(new Speciality { Id = Surgery, CollegeId = 1, Name = "Surgery" });
        db.SubSpecialities.Add(new SubSpeciality { Id = GeneralPaediatrics, SpecialityId = Paediatrics, Name = "General Paediatrics" });
        db.SubSpecialities.Add(new SubSpeciality { Id = GeneralSurgery, SpecialityId = Surgery, Name = "General Surgery" });
        db.Curricula.Add(new Curriculum { Id = PaediatricsCurriculum, SubSpecialityId = GeneralPaediatrics, Name = "FCPaed", Version = "11.1" });
        db.Curricula.Add(new Curriculum { Id = SurgeryCurriculum, SubSpecialityId = GeneralSurgery, Name = "FCS", Version = "1" });
        db.Epas.Add(new Epa { Id = 1, SubSpecialityId = GeneralPaediatrics, Code = "PAED-001", Title = "Clerk an acute admission", IsActive = true });
        db.Set<EntrustmentScale>().Add(new EntrustmentScale { Id = 1, Name = "v11.1" });
        db.Set<EntrustmentLevel>().Add(new EntrustmentLevel { Id = 3, ScaleId = 1, Order = 3, Label = "3a" });

        AddProfile(db, id: 1, TraineeUserId, HostInstitution, PaediatricsCurriculum, isActive: true);
        AddProfile(db, id: 2, SurgicalTraineeUserId, HostInstitution, SurgeryCurriculum, isActive: true);
        AddProfile(db, id: 3, ElsewhereTraineeUserId, OtherInstitution, PaediatricsCurriculum, isActive: true);
        AddProfile(db, id: 4, MovedTraineeUserId, HostInstitution, PaediatricsCurriculum, isActive: false);
        AddProfile(db, id: 5, MovedTraineeUserId, OtherInstitution, PaediatricsCurriculum, isActive: false);

        db.DecisionPanels.Add(new DecisionPanel
        {
            Id = 20,
            Name = "Paediatrics CCC",
            Scope = DecisionPanelScope.Institution,
            InstitutionId = HostInstitution,
            CreatedOn = DateTime.UtcNow,
            Members =
            [
                new DecisionPanelMember { UserId = "chair-1", Role = DecisionPanelMemberRole.Chair },
                new DecisionPanelMember { UserId = "member-1", Role = DecisionPanelMemberRole.Member }
            ]
        });

        await db.SaveChangesAsync();

        var seeded = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var traineeUserId in new[] { TraineeUserId, SurgicalTraineeUserId, ElsewhereTraineeUserId, MovedTraineeUserId })
        {
            seeded[traineeUserId] = await AddDecisionAsync(db, traineeUserId);
        }

        return seeded;
    }

    /// <summary>A ratified review of this trainee by the host's panel, and the active decision it issued.</summary>
    private static async Task<int> AddDecisionAsync(ApplicationDbContext db, string traineeUserId)
    {
        var review = new CommitteeReview
        {
            PanelId = 20,
            TraineeUserId = traineeUserId,
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 6, 30),
            ScheduledOn = new DateOnly(2026, 7, 1)
        };
        db.CommitteeReviews.Add(review);

        // The state a decision is issued from. Set through the change tracker: the domain gets there only by conducting
        // the review, which is not what these tests are about.
        db.Entry(review).Property(entity => entity.State).CurrentValue = CommitteeReviewState.Ratified;
        await db.SaveChangesAsync();

        var decision = EntrustmentDecision.Issue(
            traineeUserId, epaId: 1, authorisedLevelId: 3, new DateOnly(2026, 7, 1), expiresOn: null,
            committeeReviewId: review.Id, "chair-1", "Consistent across the period.", StarEvidence.One());
        db.EntrustmentDecisions.Add(decision);
        await db.SaveChangesAsync();

        // A request reads through a context that has not seen the seeding.
        db.ChangeTracker.Clear();
        return decision.Id;
    }

    private static void AddProfile(ApplicationDbContext db, int id, string userId, int institutionId, int curriculumId, bool isActive)
        => db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = id,
            UserId = userId,
            InstitutionId = institutionId,
            CurriculumId = curriculumId,
            ProgrammeStartDate = new DateOnly(2024, 1, 1),
            ExpectedCompletionDate = new DateOnly(2028, 1, 1),
            IsActive = isActive
        });

    private ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options);

    private sealed class FakePdfService : IEntrustmentCertificatePdfService
    {
        public int Calls { get; private set; }

        public Task<EntrustmentCertificateResult> GenerateAsync(EntrustmentCertificateRequest request, CancellationToken cancellationToken)
        {
            Calls++;
            return Task.FromResult(new EntrustmentCertificateResult([0x25, 0x50, 0x44, 0x46], $"certificate-{request.DecisionId}.pdf", "fakehash"));
        }
    }
}
