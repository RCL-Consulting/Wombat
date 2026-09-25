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
/// Someone who holds Trainee administers no decision panel, whatever role they hold beside it, the Administrator's
/// included: they create none, change none, open none to change it, are offered nothing by the panel form or the panel
/// list, and say which College committee no panel sits as. (T256)
/// </summary>
/// <remarks>
/// <para>
/// Until T256 panel administration never asked the trainee rung (<c>TraineeScopeResolver.ActsAsTrainee</c>, T185): a
/// registrar who also held InstitutionalAdmin, SpecialityAdmin or SubSpecialityAdmin could name the chair and the external
/// members of the panel that decides their own EPAs. Each role pair runs against the Paediatrics panel at their own
/// institution, which each role alone reaches: the control shows the same caller without Trainee is admitted, so every
/// refusal below is the rung's.
/// </para>
/// <para>
/// Every refused command is followed by the save the audit pipeline makes from its catch and a cleared change tracker,
/// and the store is read back through a second context: a check that ran after a mutation would have the refusal commit
/// it.
/// </para>
/// </remarks>
public sealed class PanelAdministrationTraineeFirstTests
{
    private const int InstitutionA = 1;
    private const int Paediatrics = 1;
    private const int GeneralPaediatrics = 11;
    private const int PanelId = 10;
    private const int UnknownPanelId = 999;
    private const string Registrar = "registrar-a";

    /// <summary>The refusal, and the panel pages' note, for someone who holds Trainee beside a role that manages panels.</summary>
    private const string TraineeManagesNoPanel =
        "You hold the Trainee role, so you cannot create or change a decision panel, including one that reviews you.";

    /// <summary>The refusal for someone who holds no role that manages panels.</summary>
    private const string MayNotManagePanels = "You are not allowed to manage committee panels.";

    private readonly string _databaseName = Guid.NewGuid().ToString();

    private static readonly FakeUserDirectory Committee =
        FakeUserDirectory.CommitteeMembersAt(InstitutionA, "chair-a", "member-a", "external-a");

    /// <summary>Every role that manages panels, which the Trainee role is held beside.</summary>
    public static TheoryData<string> PanelAdministrationRoles => new()
    {
        WombatRoles.Administrator,
        WombatRoles.InstitutionalAdmin,
        WombatRoles.SpecialityAdmin,
        WombatRoles.SubSpecialityAdmin
    };

    // ─── Creating and changing a panel ──────────────────────────────────────

    [Theory]
    [MemberData(nameof(PanelAdministrationRoles))]
    public async Task ATraineeWhoAlsoManagesPanels_IsRefusedCreatingOne_BeforeAnythingIsRead_AndNothingIsWritten(string role)
    {
        await using var db = await SeededDbAsync();
        var before = await SnapshotAsync();

        var create = () => new CreateDecisionPanelCommandHandler(db, Committee).Handle(
            CreateCommand(Caller(role, holdsTrainee: true)), CancellationToken.None);

        (await create.Should().ThrowAsync<UnauthorizedAccessException>(role))
            .Which.Message.Should().Be(TraineeManagesNoPanel);
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await SnapshotAsync()).Should().Equal(before, role);
    }

    [Theory]
    [MemberData(nameof(PanelAdministrationRoles))]
    public async Task ATraineeWhoAlsoManagesPanels_IsRefusedChangingOne_WithTheSameSentenceForAnUnknownId_AndNothingIsWritten(
        string role)
    {
        await using var db = await SeededDbAsync();
        var before = await SnapshotAsync();
        var caller = Caller(role, holdsTrainee: true);

        var own = await RefusalAsync(() => UpdateAsync(db, PanelId, caller));
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        var unknown = await RefusalAsync(() => UpdateAsync(db, UnknownPanelId, caller));
        await SaveAndClearAsAuditPipelineWouldAsync(db);

        own.Should().BeOfType<UnauthorizedAccessException>(role)
            .Which.Message.Should().Be(TraineeManagesNoPanel);
        unknown.Should().BeOfType<UnauthorizedAccessException>(role)
            .Which.Message.Should().Be(own.Message, "the refusal is given before the panel is looked up");
        (await SnapshotAsync()).Should().Equal(before, role);
    }

    [Theory]
    [MemberData(nameof(PanelAdministrationRoles))]
    public async Task TheSameCallerWithoutTrainee_CreatesAndChangesThePanel(string role)
    {
        await using var db = await SeededDbAsync();
        var caller = Caller(role, holdsTrainee: false);

        await new CreateDecisionPanelCommandHandler(db, Committee).Handle(CreateCommand(caller), CancellationToken.None);
        db.ChangeTracker.Clear();
        await UpdateAsync(db, PanelId, caller);

        await using var read = CreateDb();
        (await read.DecisionPanels.CountAsync()).Should().Be(2, role);
        (await read.Set<DecisionPanelMember>().Where(member => member.PanelId == PanelId).Select(member => member.UserId).ToListAsync())
            .Should().BeEquivalentTo(["chair-a", "external-a"], role);
    }

    // ─── Opening a panel, and the form's offer ──────────────────────────────

    [Theory]
    [MemberData(nameof(PanelAdministrationRoles))]
    public async Task ATraineeWhoAlsoManagesPanels_OpensNoPanel_AndTheFormOffersNothing_SayingWhy(string role)
    {
        await using var db = await SeededDbAsync();
        var caller = Caller(role, holdsTrainee: true);

        (await new GetDecisionPanelByIdQueryHandler(db).Handle(new GetDecisionPanelByIdQuery(PanelId, caller), CancellationToken.None))
            .Should().BeNull(role);

        var options = await new GetDecisionPanelFormOptionsQueryHandler(db).Handle(
            new GetDecisionPanelFormOptionsQuery(caller), CancellationToken.None);
        options.MayCreateAny.Should().BeFalse(role);
        options.MayCreateInstitutionWide.Should().BeFalse(role);
        options.TraineeNote.Should().Be(TraineeManagesNoPanel, role);

        (await new ListPanelMemberCandidatesQueryHandler(Committee).Handle(
                new ListPanelMemberCandidatesQuery(caller, InstitutionA), CancellationToken.None))
            .Should().BeEmpty($"{role}: picker = gate, and the save refuses whoever is named");

        (await new ListDecisionPanelsQueryHandler(db, Committee).Handle(new ListDecisionPanelsQuery(caller), CancellationToken.None))
            .Should().ContainSingle(role).Which.CallerMayManage.Should().BeFalse($"{role}: the list offers no Edit");
    }

    [Theory]
    [MemberData(nameof(PanelAdministrationRoles))]
    public async Task TheSameCallerWithoutTrainee_OpensThePanel_AndIsOfferedIt(string role)
    {
        await using var db = await SeededDbAsync();
        var caller = Caller(role, holdsTrainee: false);

        (await new GetDecisionPanelByIdQueryHandler(db).Handle(new GetDecisionPanelByIdQuery(PanelId, caller), CancellationToken.None))
            .Should().NotBeNull(role);

        var options = await new GetDecisionPanelFormOptionsQueryHandler(db).Handle(
            new GetDecisionPanelFormOptionsQuery(caller), CancellationToken.None);
        options.MayCreateAny.Should().BeTrue(role);
        options.TraineeNote.Should().BeNull(role);

        (await new ListPanelMemberCandidatesQueryHandler(Committee).Handle(
                new ListPanelMemberCandidatesQuery(caller, InstitutionA), CancellationToken.None))
            .Select(candidate => candidate.UserId).Should().BeEquivalentTo(["chair-a", "member-a", "external-a"], role);

        (await new ListDecisionPanelsQueryHandler(db, Committee).Handle(new ListDecisionPanelsQuery(caller), CancellationToken.None))
            .Should().ContainSingle(role).Which.CallerMayManage.Should().BeTrue(role);
    }

    // ─── The College committee a panel sits as ──────────────────────────────

    public static TheoryData<string> DecisionBodyRoles => new()
    {
        WombatRoles.Administrator,
        WombatRoles.InstitutionalAdmin
    };

    [Theory]
    [MemberData(nameof(DecisionBodyRoles))]
    public async Task ATraineeWhoCouldOtherwiseSayWhichCommitteeAPanelSitsAs_IsRefused_AndNothingIsWritten(string role)
    {
        // The tag chooses which panel decides EPAs 4 and 5 for every trainee it covers, the caller among them.
        await using var db = await SeededDbAsync();
        var before = await SnapshotAsync();

        var tag = () => new SetDecisionPanelBodyCommandHandler(db).Handle(
            new SetDecisionPanelBodyCommand(PanelId, "neonatal", Caller(role, holdsTrainee: true)), CancellationToken.None);

        (await tag.Should().ThrowAsync<UnauthorizedAccessException>(role))
            .Which.Message.Should().Be(TraineeManagesNoPanel);
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await SnapshotAsync()).Should().Equal(before, role);

        // The control: the same caller without Trainee tags the panel.
        await new SetDecisionPanelBodyCommandHandler(db).Handle(
            new SetDecisionPanelBodyCommand(PanelId, "neonatal", Caller(role, holdsTrainee: false)), CancellationToken.None);
        await using var read = CreateDb();
        (await read.DecisionPanels.SingleAsync(panel => panel.Id == PanelId)).DecisionBodyKey.Should().Be("neonatal", role);
    }

    // ─── Who is told it is the Trainee role ─────────────────────────────────

    [Fact]
    public async Task ATraineeWhoManagesNoPanel_IsToldTheyMayNotManagePanels_NotThatTheTraineeRoleStandsInTheWay()
    {
        // As scheduling tells them (T216): the Trainee sentence would say that dropping the role lets them manage panels,
        // which it does not.
        await using var db = await SeededDbAsync();
        var trainee = TestPrincipals.InRoles([WombatRoles.Trainee, WombatRoles.CommitteeMember], Registrar, InstitutionA);

        var create = () => new CreateDecisionPanelCommandHandler(db, Committee).Handle(CreateCommand(trainee), CancellationToken.None);

        (await create.Should().ThrowAsync<UnauthorizedAccessException>())
            .Which.Message.Should().Be(MayNotManagePanels);
        (await new GetDecisionPanelFormOptionsQueryHandler(db).Handle(
                new GetDecisionPanelFormOptionsQuery(trainee), CancellationToken.None))
            .TraineeNote.Should().BeNull();
    }

    // ─── Fixture ─────────────────────────────────────────────────────────────

    /// <summary>
    /// The registrar at A, holding <paramref name="role" /> with the scope claims it carries, and Trainee beside it when
    /// <paramref name="holdsTrainee" />. A SpecialityAdmin administers Paediatrics; a SubSpecialityAdmin General
    /// Paediatrics, whose speciality is Paediatrics: so each reaches the Paediatrics panel without Trainee.
    /// </summary>
    private static ClaimsPrincipal Caller(string role, bool holdsTrainee)
    {
        string[] roles = holdsTrainee ? [WombatRoles.Trainee, role] : [role];
        var institutionId = role == WombatRoles.Administrator ? (int?)null : InstitutionA;
        return TestPrincipals.InRoles(
            roles,
            Registrar,
            institutionId,
            specialityId: role == WombatRoles.SpecialityAdmin ? Paediatrics : null,
            subSpecialityId: role == WombatRoles.SubSpecialityAdmin ? GeneralPaediatrics : null);
    }

    private static CreateDecisionPanelCommand CreateCommand(ClaimsPrincipal caller)
        => new(
            "Second Paediatrics CCC",
            DecisionPanelScope.Speciality,
            InstitutionA,
            Paediatrics,
            [
                new DecisionPanelMemberInput("chair-a", DecisionPanelMemberRole.Chair),
                new DecisionPanelMemberInput("member-a", DecisionPanelMemberRole.Member)
            ],
            caller);

    /// <summary>The panel's chair and its external member, the member taken off: a change every role alone may make.</summary>
    private static Task<DecisionPanelDetailDto> UpdateAsync(ApplicationDbContext db, int panelId, ClaimsPrincipal caller)
        => new UpdateDecisionPanelCommandHandler(db, Committee).Handle(
            new UpdateDecisionPanelCommand(
                panelId,
                [
                    new DecisionPanelMemberInput("chair-a", DecisionPanelMemberRole.Chair),
                    new DecisionPanelMemberInput("external-a", DecisionPanelMemberRole.External)
                ],
                caller),
            CancellationToken.None);

    private static async Task<Exception> RefusalAsync(Func<Task> act)
    {
        try
        {
            await act();
        }
        catch (Exception exception)
        {
            return exception;
        }

        throw new InvalidOperationException("The request was expected to be refused.");
    }

    private async Task<ApplicationDbContext> SeededDbAsync()
    {
        var db = CreateDb();

        db.Institutions.Add(new Institution { Id = InstitutionA, Name = "A", ShortCode = "A", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.Colleges.Add(new College { Id = 1, Name = "CMSA", ShortCode = "CMSA", IsActive = true });
        db.Specialities.Add(new Speciality { Id = Paediatrics, CollegeId = 1, Name = "Paediatrics", IsActive = true });
        db.SubSpecialities.Add(new SubSpeciality
        {
            Id = GeneralPaediatrics, SpecialityId = Paediatrics, Name = "General Paediatrics", IsActive = true
        });
        db.Curricula.Add(new Curriculum { Id = 100, SubSpecialityId = GeneralPaediatrics, Name = "General Paediatrics", Version = "11.1" });
        // A trains it: a speciality panel is created only for a speciality its institution has adopted (T245).
        AdoptionSeed.Adopt(db, 1, InstitutionA, 100, GeneralPaediatrics);
        db.Set<DecisionBody>().Add(new DecisionBody { Key = "neonatal", Name = "Neonatal team Clinical Competency Committee" });
        db.DecisionPanels.Add(new DecisionPanel
        {
            Id = PanelId,
            Name = "Paediatrics CCC",
            Scope = DecisionPanelScope.Speciality,
            InstitutionId = InstitutionA,
            SpecialityId = Paediatrics,
            CreatedOn = DateTime.UtcNow,
            Members =
            [
                new DecisionPanelMember { UserId = "chair-a", Role = DecisionPanelMemberRole.Chair },
                new DecisionPanelMember { UserId = "member-a", Role = DecisionPanelMemberRole.Member }
            ]
        });

        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return db;
    }

    private ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options);

    private static async Task SaveAndClearAsAuditPipelineWouldAsync(ApplicationDbContext db)
    {
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }

    /// <summary>Every panel as stored, its body and its members: what a refused panel command must leave as it was.</summary>
    private async Task<IReadOnlyList<string>> SnapshotAsync()
    {
        await using var read = CreateDb();
        var rows = new List<string>();
        rows.AddRange(await read.DecisionPanels.OrderBy(panel => panel.Id)
            .Select(panel => $"panel {panel.Id}:{panel.Name}:{panel.Scope}:{panel.InstitutionId}:{panel.SpecialityId}:{panel.DecisionBodyKey}")
            .ToListAsync());
        rows.AddRange(await read.Set<DecisionPanelMember>().OrderBy(member => member.PanelId).ThenBy(member => member.UserId)
            .Select(member => $"member {member.PanelId}:{member.UserId}:{member.Role}")
            .ToListAsync());
        return rows;
    }
}
