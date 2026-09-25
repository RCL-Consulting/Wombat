using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.CommitteeDecisions;

/// <summary>
/// Every decision panel runs at one institution, and who may create, change, open and list a panel reads it. (T182)
/// </summary>
/// <remarks>
/// <para>
/// A panel reviews only trainees at its own institution (<see cref="CommitteeTraineeScopeTests" />). Until T182 only an
/// InstitutionalAdmin's panels were stamped with one: a SpecialityAdmin's panel, and an Administrator's Speciality-scoped
/// panel, carried none, so it could review nobody. Worse, the administration checks were skipped for a panel without an
/// institution, so any panel administrator in the country could rewrite its members, and every role but an
/// InstitutionalAdmin listed every panel in the country.
/// </para>
/// <para>
/// Every refused command is followed by a save and a cleared change tracker, as the audit pipeline does from its catch,
/// and the store is read back through a second context.
/// </para>
/// </remarks>
public sealed class DecisionPanelScopeTests
{
    private const int InstitutionA = 1;
    private const int InstitutionB = 2;
    private const int Paediatrics = 4;
    private const int Surgery = 5;
    private const int GeneralPaediatrics = 11;
    private const int GeneralSurgery = 12;

    /// <summary>
    /// Everyone these tests seat, at either institution: an active committee member there (T165, PanelSeat). Who may sit
    /// is not what these tests are about.
    /// </summary>
    private static readonly FakeUserDirectory Seats = FakeUserDirectory
        .CommitteeMembersAt(InstitutionA, "chair", "new-chair")
        .WithCommitteeMembers(InstitutionB, "chair", "new-chair");

    private readonly string _databaseName = Guid.NewGuid().ToString();

    // ─── Create ──────────────────────────────────────────────────────────────

    public static TheoryData<string> PanelAdministratorsOfA => new()
    {
        "InstitutionalAdmin", "SpecialityAdmin", "SubSpecialityAdmin"
    };

    [Theory]
    [MemberData(nameof(PanelAdministratorsOfA))]
    public async Task ASpecialityPanel_CreatedByAnyoneButAnAdministrator_RunsAtTheirInstitution(string role)
    {
        // The form offers them no institution for a Speciality-scoped panel, so the request carries none.
        await using var db = await SeededDbAsync();

        var panel = await CreateAsync(db, PanelAdministrator(role, InstitutionA), DecisionPanelScope.Speciality, institutionId: null);

        panel.InstitutionId.Should().Be(InstitutionA);
        (await StoredInstitutionsAsync()).Should().Equal(InstitutionA);
    }

    [Fact]
    public async Task AnAdministrator_NamesThePanelsInstitution_ForEveryScope()
    {
        await using var db = await SeededDbAsync();
        var administrator = TestPrincipals.Administrator();

        var speciality = await CreateAsync(db, administrator, DecisionPanelScope.Speciality, InstitutionB);
        var institution = await CreateAsync(db, administrator, DecisionPanelScope.Institution, InstitutionA);

        speciality.InstitutionId.Should().Be(InstitutionB);
        institution.InstitutionId.Should().Be(InstitutionA);
    }

    [Theory]
    [InlineData(DecisionPanelScope.Speciality, null, "Choose the institution that runs this panel.")]
    [InlineData(DecisionPanelScope.Institution, null, "Choose the institution that runs this panel.")]
    [InlineData(DecisionPanelScope.Speciality, 999, "The institution could not be found.")]
    [InlineData(DecisionPanelScope.Institution, 999, "The institution could not be found.")]
    public async Task AnAdministrator_WithoutAnInstitutionThatExists_CreatesNothing(
        DecisionPanelScope scope, int? institutionId, string refusal)
    {
        // An Administrator belongs to no institution, so nothing can be pinned for them; the form asks them to choose.
        await using var db = await SeededDbAsync();

        var act = () => CreateAsync(db, TestPrincipals.Administrator(), scope, institutionId);

        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be(refusal);
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await StoredInstitutionsAsync()).Should().BeEmpty();
    }

    public static TheoryData<string, DecisionPanelScope, int?> RefusedCreates => new()
    {
        // Another institution, whatever the scope.
        { "InstitutionalAdmin", DecisionPanelScope.Institution, InstitutionB },
        { "InstitutionalAdmin", DecisionPanelScope.Speciality, InstitutionB },
        { "SpecialityAdmin", DecisionPanelScope.Speciality, InstitutionB },
        { "SubSpecialityAdmin", DecisionPanelScope.Speciality, InstitutionB },
        // The institution-wide panel is the InstitutionalAdmin's, even at the caller's own institution.
        { "SpecialityAdmin", DecisionPanelScope.Institution, InstitutionA },
        { "SubSpecialityAdmin", DecisionPanelScope.Institution, InstitutionA },
        // No institution claim and none named: there is nowhere to put the panel.
        { "SpecialityAdmin with no institution", DecisionPanelScope.Speciality, null },
        // Another speciality's panel at their own institution (T131 slice 3; the panel covers Paediatrics).
        { "SpecialityAdmin of Surgery", DecisionPanelScope.Speciality, InstitutionA },
        { "SubSpecialityAdmin of General Surgery", DecisionPanelScope.Speciality, InstitutionA },
        { "SpecialityAdmin of Surgery with a Paediatrics sub-speciality", DecisionPanelScope.Speciality, InstitutionA }
    };

    [Theory]
    [MemberData(nameof(RefusedCreates))]
    public async Task APanelOutsideTheCallersScope_IsRefused_AndNothingIsWritten(
        string role, DecisionPanelScope scope, int? institutionId)
    {
        await using var db = await SeededDbAsync();

        var act = () => CreateAsync(db, PanelAdministrator(role, InstitutionA), scope, institutionId);

        (await act.Should().ThrowAsync<UnauthorizedAccessException>())
            .Which.Message.Should().Be("You can only manage panels in your institution.");
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await StoredInstitutionsAsync()).Should().BeEmpty();
    }

    // ─── Update ──────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("SpecialityAdmin")]
    [InlineData("SubSpecialityAdmin")]
    [InlineData("InstitutionalAdmin")]
    public async Task ASpecialityPanelAtTheirInstitution_IsTheirsToChange(string role)
    {
        // The panel they created: stamped with their institution now, where before the stamp was missing and the
        // check skipped.
        await using var db = await SeededDbAsync();
        var panelId = await AddPanelAsync(db, DecisionPanelScope.Speciality, InstitutionA);

        await UpdateAsync(db, PanelAdministrator(role, InstitutionA), panelId, "new-chair");

        (await StoredMembersAsync(panelId)).Should().Equal("new-chair");
    }

    public static TheoryData<string, DecisionPanelScope, int> RefusedUpdates => new()
    {
        { "InstitutionalAdmin", DecisionPanelScope.Institution, InstitutionB },
        { "SpecialityAdmin", DecisionPanelScope.Speciality, InstitutionB },
        { "SubSpecialityAdmin", DecisionPanelScope.Speciality, InstitutionB },
        { "SpecialityAdmin", DecisionPanelScope.Institution, InstitutionA },
        { "SubSpecialityAdmin", DecisionPanelScope.Institution, InstitutionA },
        // The Paediatrics panel at their own institution, for another speciality's administrator (T131 slice 3). Its
        // chair decides for every paediatric trainee there, and for EPAs 4 and 5 ahead of every other panel once it
        // sits as the neonatal CCC.
        { "SpecialityAdmin of Surgery", DecisionPanelScope.Speciality, InstitutionA },
        { "SubSpecialityAdmin of General Surgery", DecisionPanelScope.Speciality, InstitutionA },
        { "SpecialityAdmin of Surgery with a Paediatrics sub-speciality", DecisionPanelScope.Speciality, InstitutionA }
    };

    [Theory]
    [MemberData(nameof(RefusedUpdates))]
    public async Task APanelOutsideTheCallersScope_KeepsItsMembers_EvenAfterTheRefusalIsSaved(
        string role, DecisionPanelScope scope, int institutionId)
    {
        // The handler clears the member list before it adds the new one. A check after that would have the audit
        // pipeline's save commit an emptied panel.
        await using var db = await SeededDbAsync();
        var panelId = await AddPanelAsync(db, scope, institutionId);

        var act = () => UpdateAsync(db, PanelAdministrator(role, InstitutionA), panelId, "hijacker");

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await StoredMembersAsync(panelId)).Should().Equal("chair");
    }

    // ─── Open (the panel form's read) ────────────────────────────────────────

    [Fact]
    public async Task ThePanelForm_OpensOnlyAPanelTheCallerMayChange_AndNothingElseIsConfirmedToExist()
    {
        await using var db = await SeededDbAsync();
        var specialityAtA = await AddPanelAsync(db, DecisionPanelScope.Speciality, InstitutionA);
        var specialityAtB = await AddPanelAsync(db, DecisionPanelScope.Speciality, InstitutionB);
        var institutionAtA = await AddPanelAsync(db, DecisionPanelScope.Institution, InstitutionA);
        var specialityAdmin = PanelAdministrator("SpecialityAdmin", InstitutionA);

        (await GetAsync(db, specialityAdmin, specialityAtA)).Should().NotBeNull();
        (await GetAsync(db, specialityAdmin, specialityAtB)).Should().BeNull("another institution's members are not theirs to read");
        (await GetAsync(db, specialityAdmin, institutionAtA)).Should().BeNull();
        (await GetAsync(db, TestPrincipals.InstitutionalAdmin(InstitutionA), institutionAtA)).Should().NotBeNull();
        (await GetAsync(db, TestPrincipals.Coordinator(InstitutionA), specialityAtA)).Should().BeNull();
        (await GetAsync(db, TestPrincipals.Administrator(), specialityAtB)).Should().NotBeNull();
    }

    [Theory]
    [InlineData("SpecialityAdmin", true)]
    [InlineData("SubSpecialityAdmin", true)]
    [InlineData("SpecialityAdmin of Surgery", false)]
    [InlineData("SubSpecialityAdmin of General Surgery", false)]
    [InlineData("SpecialityAdmin of Surgery with a Paediatrics sub-speciality", false)]
    public async Task ThePanelForm_OpensASpecialityPanel_OnlyForThatSpecialitysAdministrators(string role, bool opens)
    {
        // The form's read is the panels the caller may change (T131 slice 3): the Paediatrics panel at A is not a Surgery
        // administrator's to open, and its members are not theirs to read.
        await using var db = await SeededDbAsync();
        var paediatricsAtA = await AddPanelAsync(db, DecisionPanelScope.Speciality, InstitutionA);

        var read = await GetAsync(db, PanelAdministrator(role, InstitutionA), paediatricsAtA);

        (read is not null).Should().Be(opens);
    }

    // ─── List ────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(WombatRoles.Coordinator)]
    [InlineData(WombatRoles.SpecialityAdmin)]
    [InlineData(WombatRoles.SubSpecialityAdmin)]
    [InlineData(WombatRoles.CommitteeMember)]
    [InlineData(WombatRoles.InstitutionalAdmin)]
    public async Task ThePanelList_HoldsTheCallersInstitutionsPanels(string role)
    {
        // It feeds the scheduling page's panel picker: a panel elsewhere is one the scheduling handler refuses.
        await using var db = await SeededDbAsync();
        await AddPanelAsync(db, DecisionPanelScope.Institution, InstitutionA, name: "A institution");
        await AddPanelAsync(db, DecisionPanelScope.Speciality, InstitutionA, name: "A speciality");
        await AddPanelAsync(db, DecisionPanelScope.Institution, InstitutionB, name: "B institution");

        var listed = await ListAsync(db, TestPrincipals.InRole(role, "someone-at-a", InstitutionA));

        listed.Should().BeEquivalentTo("A institution", "A speciality");
    }

    [Fact]
    public async Task ThePanelList_AlsoHoldsAPanelTheCallerSitsOnElsewhere()
    {
        // An External member may come from another institution; the panel is still theirs to see.
        await using var db = await SeededDbAsync();
        await AddPanelAsync(db, DecisionPanelScope.Institution, InstitutionA, name: "A institution", external: "external-from-b");
        await AddPanelAsync(db, DecisionPanelScope.Institution, InstitutionB, name: "B institution");
        await AddPanelAsync(db, DecisionPanelScope.Speciality, InstitutionA, name: "A speciality");

        var listed = await ListAsync(db, TestPrincipals.InRole(WombatRoles.CommitteeMember, "external-from-b", InstitutionB));

        listed.Should().BeEquivalentTo("A institution", "B institution");
    }

    [Fact]
    public async Task ThePanelList_IsEveryPanelForAnAdministrator_AndNoneForSomeoneWithNoInstitutionOrSeat()
    {
        await using var db = await SeededDbAsync();
        await AddPanelAsync(db, DecisionPanelScope.Institution, InstitutionA, name: "A institution");
        await AddPanelAsync(db, DecisionPanelScope.Institution, InstitutionB, name: "B institution");

        (await ListAsync(db, TestPrincipals.Administrator())).Should().BeEquivalentTo("A institution", "B institution");
        (await ListAsync(db, TestPrincipals.InRole(WombatRoles.Coordinator, "nowhere", institutionId: null))).Should().BeEmpty();
    }

    // ─── The commands ────────────────────────────────────────────────────────

    private static async Task<DecisionPanelDetailDto> CreateAsync(
        ApplicationDbContext db, ClaimsPrincipal principal, DecisionPanelScope scope, int? institutionId)
        => await new CreateDecisionPanelCommandHandler(db, Seats).Handle(
            new CreateDecisionPanelCommand(
                "Annual review panel",
                scope,
                institutionId,
                scope == DecisionPanelScope.Speciality ? Paediatrics : null,
                [new DecisionPanelMemberInput("chair", DecisionPanelMemberRole.Chair)],
                principal),
            CancellationToken.None);

    private static async Task UpdateAsync(ApplicationDbContext db, ClaimsPrincipal principal, int panelId, string chair)
        => await new UpdateDecisionPanelCommandHandler(db, Seats).Handle(
            new UpdateDecisionPanelCommand(panelId, [new DecisionPanelMemberInput(chair, DecisionPanelMemberRole.Chair)], principal),
            CancellationToken.None);

    private static async Task<DecisionPanelDetailDto?> GetAsync(ApplicationDbContext db, ClaimsPrincipal principal, int panelId)
        => await new GetDecisionPanelByIdQueryHandler(db).Handle(
            new GetDecisionPanelByIdQuery(panelId, principal), CancellationToken.None);

    private static async Task<IReadOnlyList<string>> ListAsync(ApplicationDbContext db, ClaimsPrincipal principal)
        => (await new ListDecisionPanelsQueryHandler(db, FakeUserDirectory.Empty).Handle(new ListDecisionPanelsQuery(principal), CancellationToken.None))
            .Select(panel => panel.Name)
            .ToArray();

    private static ClaimsPrincipal PanelAdministrator(string role, int institutionId) => role switch
    {
        "InstitutionalAdmin" => TestPrincipals.InstitutionalAdmin(institutionId),
        "SpecialityAdmin" => TestPrincipals.InRole(WombatRoles.SpecialityAdmin, "spec-admin", institutionId, specialityId: Paediatrics),
        "SubSpecialityAdmin" => TestPrincipals.InRole(
            WombatRoles.SubSpecialityAdmin, "sub-admin", institutionId, subSpecialityId: GeneralPaediatrics),
        // T131 slice 3: a panel's speciality is read, so these administer only their own speciality's panels.
        "SpecialityAdmin of Surgery" => TestPrincipals.InRole(WombatRoles.SpecialityAdmin, "surgery-admin", institutionId, specialityId: Surgery),
        "SubSpecialityAdmin of General Surgery" => TestPrincipals.InRole(
            WombatRoles.SubSpecialityAdmin, "surgery-sub-admin", institutionId, subSpecialityId: GeneralSurgery),
        // The role and its own claim are asked together: a Paediatrics sub-speciality claim reaches a panel only through
        // the SubSpecialityAdmin role, never through a SpecialityAdmin role scoped to Surgery.
        "SpecialityAdmin of Surgery with a Paediatrics sub-speciality" => TestPrincipals.InRole(
            WombatRoles.SpecialityAdmin, "mixed-admin", institutionId, specialityId: Surgery, subSpecialityId: GeneralPaediatrics),
        "SpecialityAdmin with no institution" => TestPrincipals.InRole(
            WombatRoles.SpecialityAdmin, "spec-admin", institutionId: null, specialityId: Paediatrics),
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
    };

    // ─── The store ───────────────────────────────────────────────────────────

    private async Task<int> AddPanelAsync(
        ApplicationDbContext db,
        DecisionPanelScope scope,
        int institutionId,
        string? name = null,
        string? external = null)
    {
        var members = new List<DecisionPanelMember> { new() { UserId = "chair", Role = DecisionPanelMemberRole.Chair } };
        if (external is not null)
        {
            members.Add(new DecisionPanelMember { UserId = external, Role = DecisionPanelMemberRole.External });
        }

        var panel = new DecisionPanel
        {
            Name = name ?? $"{scope} at {institutionId}",
            Scope = scope,
            InstitutionId = institutionId,
            SpecialityId = scope == DecisionPanelScope.Speciality ? Paediatrics : null,
            CreatedOn = DateTime.UtcNow,
            Members = members
        };

        db.DecisionPanels.Add(panel);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return panel.Id;
    }

    private async Task<ApplicationDbContext> SeededDbAsync()
    {
        var db = CreateDb();
        db.Institutions.AddRange(
            new Institution { Id = InstitutionA, Name = "A", ShortCode = "A", IsActive = true, CreatedOn = DateTime.UtcNow },
            new Institution { Id = InstitutionB, Name = "B", ShortCode = "B", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.Specialities.AddRange(
            new Speciality { Id = Paediatrics, CollegeId = 1, Name = "Paediatrics", IsActive = true },
            new Speciality { Id = Surgery, CollegeId = 1, Name = "Surgery", IsActive = true });
        db.SubSpecialities.AddRange(
            new SubSpeciality { Id = GeneralPaediatrics, SpecialityId = Paediatrics, Name = "General Paediatrics", IsActive = true },
            new SubSpeciality { Id = GeneralSurgery, SpecialityId = Surgery, Name = "General Surgery", IsActive = true });
        // A and B both train Paediatrics, the speciality every speciality panel here covers: a speciality panel is created
        // only for a speciality its institution has adopted, an Administrator's at B included (T245 and its review,
        // PanelSpecialityAdoptionTests).
        db.Curricula.Add(new Curriculum { Id = 100, SubSpecialityId = GeneralPaediatrics, Name = "General Paediatrics", Version = "11.1" });
        AdoptionSeed.Adopt(db, 1, InstitutionA, 100, GeneralPaediatrics);
        AdoptionSeed.Adopt(db, 2, InstitutionB, 100, GeneralPaediatrics);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return db;
    }

    private static async Task SaveAndClearAsAuditPipelineWouldAsync(ApplicationDbContext db)
    {
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }

    private async Task<IReadOnlyList<int>> StoredInstitutionsAsync()
    {
        await using var read = CreateDb();
        return await read.DecisionPanels.OrderBy(panel => panel.Id).Select(panel => panel.InstitutionId).ToListAsync();
    }

    private async Task<IReadOnlyList<string>> StoredMembersAsync(int panelId)
    {
        await using var read = CreateDb();
        return await read.DecisionPanelMembers
            .Where(member => member.PanelId == panelId)
            .OrderBy(member => member.UserId)
            .Select(member => member.UserId)
            .ToListAsync();
    }

    private ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options);
}
