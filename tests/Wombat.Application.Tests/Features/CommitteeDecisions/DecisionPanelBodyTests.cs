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
/// Which College committee a panel sits as (T131 slice 3). Only an InstitutionalAdmin of the panel's institution or a
/// global Administrator says so; a SpecialityAdmin who manages a panel's members does not, because the tag takes that
/// committee's EPAs from every other panel at the institution. An institution has one institution-wide panel per body and
/// one per speciality, and institutions A and B each have their own.
/// </summary>
/// <remarks>
/// A refused command must leave the store as it was even after a save, because the audit pipeline saves the request's
/// DbContext from its catch. Every refusal below is followed by that save and a cleared change tracker, and the store is
/// read back through a second context.
/// </remarks>
public sealed class DecisionPanelBodyTests
{
    private const int InstitutionA = 1;
    private const int InstitutionB = 2;
    private const int Paediatrics = 1;
    private const int Surgery = 2;
    private const int PanelA = 10;
    private const int PaediatricPanelA = 11;
    private const int PanelB = 20;
    private const string Neonatal = "neonatal";

    private readonly string _databaseName = Guid.NewGuid().ToString();

    /// <summary>
    /// The committee members a panel at A may seat (T165, <c>PanelSeat</c>): every member list below names only them, as
    /// the chair plus one other, so each refusal is the one its test is about and each success is a panel T165 accepts.
    /// </summary>
    private static readonly FakeUserDirectory Seats =
        FakeUserDirectory.CommitteeMembersAt(InstitutionA, "chair", "member", "new-chair");

    private static IReadOnlyList<DecisionPanelMemberInput> ChairAndMember(string chair = "chair")
        => [new DecisionPanelMemberInput(chair, DecisionPanelMemberRole.Chair), new DecisionPanelMemberInput("member", DecisionPanelMemberRole.Member)];

    [Fact]
    public async Task AnInstitutionalAdmin_SetsTheBody_OnTheirInstitutionsPanel_AndClearsItAgain()
    {
        await using var db = await SeededDbAsync();
        var adminOfA = TestPrincipals.InstitutionalAdmin(InstitutionA);

        var set = await SetBodyAsync(db, PanelA, " Neonatal ", adminOfA);

        set.DecisionBodyKey.Should().Be(Neonatal);
        set.DecisionBodyName.Should().Be("Neonatal team Clinical Competency Committee");
        (await StoredBodiesAsync()).Should().Contain(PanelA, Neonatal);

        var cleared = await SetBodyAsync(db, PanelA, "", adminOfA);

        cleared.DecisionBodyKey.Should().BeNull();
        cleared.DecisionBodyName.Should().BeNull();
        (await StoredBodiesAsync())[PanelA].Should().BeNull();
    }

    [Theory]
    [InlineData(WombatRoles.SpecialityAdmin)]
    [InlineData(WombatRoles.SubSpecialityAdmin)]
    [InlineData(WombatRoles.Coordinator)]
    [InlineData(WombatRoles.CommitteeMember)]
    public async Task NoOneButAnInstitutionalAdminOrAdministrator_SetsABody_AndNothingIsWritten(string role)
    {
        // A SpecialityAdmin manages this Speciality-scoped panel's members (T182), but not which committee it sits as.
        await using var db = await SeededDbAsync();
        var caller = TestPrincipals.InRole(role, "caller", InstitutionA, specialityId: Paediatrics);

        var refusal = await RefusalAsync(() => SetBodyAsync(db, PaediatricPanelA, Neonatal, caller));

        refusal.Should().BeOfType<UnauthorizedAccessException>()
            .Which.Message.Should().Be("Only an institutional administrator can say which College committee a panel sits as.");
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await StoredBodiesAsync()).Values.Should().OnlyContain(key => key == null);
    }

    [Fact]
    public async Task ASpecialityAdmin_CannotCreateAPanelThatSitsAsABody_AndNothingIsWritten()
    {
        await using var db = await SeededDbAsync();
        var paediatricsAdmin = TestPrincipals.InRole(WombatRoles.SpecialityAdmin, "spec-admin", InstitutionA, Paediatrics);

        var refusal = await RefusalAsync(() => new CreateDecisionPanelCommandHandler(db, Seats).Handle(
            new CreateDecisionPanelCommand(
                "Hijacked neonatal CCC",
                DecisionPanelScope.Speciality,
                InstitutionId: null,
                SpecialityId: Paediatrics,
                ChairAndMember(),
                paediatricsAdmin,
                Neonatal),
            CancellationToken.None));

        refusal.Should().BeOfType<UnauthorizedAccessException>().Which.Message.Should().Contain("institutional administrator");
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await StoredBodiesAsync()).Keys.Should().BeEquivalentTo([PanelA, PaediatricPanelA, PanelB]);
    }

    [Fact]
    public async Task InstitutionBsAdmin_CannotSetABodyOnAsPanel_AndAnUnknownPanelIsRefusedTheSame_AndNothingIsWritten()
    {
        await using var db = await SeededDbAsync();
        var adminOfB = TestPrincipals.InstitutionalAdmin(InstitutionB);

        var outOfScope = await RefusalAsync(() => SetBodyAsync(db, PanelA, Neonatal, adminOfB));
        var unknown = await RefusalAsync(() => SetBodyAsync(db, 999, Neonatal, adminOfB));

        outOfScope.Should().BeOfType<UnauthorizedAccessException>()
            .Which.Message.Should().Be("You can only manage panels in your institution.");
        unknown.Should().BeOfType<UnauthorizedAccessException>().Which.Message.Should().Be(outOfScope.Message);

        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await StoredBodiesAsync()).Values.Should().OnlyContain(key => key == null);
    }

    [Fact]
    public async Task InstitutionsAAndB_EachOwnANeonatalPanel_ButNeitherHasTwoForTheSameScope()
    {
        await using var db = await SeededDbAsync();

        await SetBodyAsync(db, PanelA, Neonatal, TestPrincipals.InstitutionalAdmin(InstitutionA));
        await SetBodyAsync(db, PanelB, Neonatal, TestPrincipals.InstitutionalAdmin(InstitutionB));

        // A panel covering one speciality may sit as the body beside the institution-wide one: it comes first for that
        // speciality's trainees (DecisionRouting).
        await SetBodyAsync(db, PaediatricPanelA, Neonatal, TestPrincipals.InstitutionalAdmin(InstitutionA));

        var second = await RefusalAsync(() => new CreateDecisionPanelCommandHandler(db, Seats).Handle(
            new CreateDecisionPanelCommand(
                "Second neonatal CCC",
                DecisionPanelScope.Institution,
                InstitutionA,
                SpecialityId: null,
                ChairAndMember(),
                TestPrincipals.InstitutionalAdmin(InstitutionA),
                Neonatal),
            CancellationToken.None));

        second.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().StartWith("Panel 10 already sits as the Neonatal team Clinical Competency Committee for the whole institution.");
        await SaveAndClearAsAuditPipelineWouldAsync(db);

        var stored = await StoredBodiesAsync();
        stored.Should().HaveCount(3, "the second institution-wide neonatal panel was not written");
        stored.Values.Should().OnlyContain(key => key == Neonatal);
    }

    [Fact]
    public async Task AnUnknownBody_IsRefused_AndNothingIsWritten()
    {
        await using var db = await SeededDbAsync();

        var refusal = await RefusalAsync(() => SetBodyAsync(db, PanelA, "cardiac", TestPrincipals.InstitutionalAdmin(InstitutionA)));

        refusal.Should().BeOfType<InvalidOperationException>().Which.Message.Should().Be("Choose a College committee from the list.");
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await StoredBodiesAsync())[PanelA].Should().BeNull();
    }

    [Fact]
    public async Task AnAdministrator_SetsABodyAtAnyInstitution_AndIsToldWhenAPanelDoesNotExist()
    {
        await using var db = await SeededDbAsync();
        var administrator = TestPrincipals.Administrator();

        (await SetBodyAsync(db, PanelB, Neonatal, administrator)).DecisionBodyKey.Should().Be(Neonatal);

        var unknown = () => SetBodyAsync(db, 999, Neonatal, administrator);
        await unknown.Should().ThrowAsync<InvalidOperationException>().WithMessage("The decision panel could not be found.");
    }

    [Fact]
    public async Task AnInstitutionalAdmin_CreatesAPanelThatSitsAsABody_AndTheFormReadsItBack()
    {
        await using var db = await SeededDbAsync();
        var adminOfA = TestPrincipals.InstitutionalAdmin(InstitutionA);

        var created = await new CreateDecisionPanelCommandHandler(db, Seats).Handle(
            new CreateDecisionPanelCommand(
                "Neonatal CCC",
                DecisionPanelScope.Institution,
                InstitutionA,
                SpecialityId: null,
                ChairAndMember(),
                adminOfA,
                Neonatal),
            CancellationToken.None);
        db.ChangeTracker.Clear();

        created.DecisionBodyName.Should().Be("Neonatal team Clinical Competency Committee");
        var read = await new GetDecisionPanelByIdQueryHandler(db).Handle(
            new GetDecisionPanelByIdQuery(created.Id, adminOfA), CancellationToken.None);
        read!.DecisionBodyKey.Should().Be(Neonatal);
        read.DecisionBodyName.Should().Be("Neonatal team Clinical Competency Committee");

        var listed = await new ListDecisionPanelsQueryHandler(db, FakeUserDirectory.Empty).Handle(
            new ListDecisionPanelsQuery(adminOfA), CancellationToken.None);
        listed.Single(panel => panel.Id == created.Id).DecisionBodyName.Should().Be("Neonatal team Clinical Competency Committee");
        listed.Single(panel => panel.Id == PanelA).DecisionBodyName.Should().BeNull();
    }

    [Fact]
    public async Task UpdatingMembers_KeepsTheBody_AndAnUnknownPanelIsRefusedAsAnotherInstitutionsIs()
    {
        await using var db = await SeededDbAsync();
        await SetBodyAsync(db, PaediatricPanelA, Neonatal, TestPrincipals.InstitutionalAdmin(InstitutionA));
        var paediatricsAdmin = TestPrincipals.InRole(WombatRoles.SpecialityAdmin, "spec-admin", InstitutionA, Paediatrics);
        var handler = new UpdateDecisionPanelCommandHandler(db, Seats);

        var updated = await handler.Handle(
            new UpdateDecisionPanelCommand(
                PaediatricPanelA, ChairAndMember("new-chair"), paediatricsAdmin),
            CancellationToken.None);

        updated.DecisionBodyKey.Should().Be(Neonatal, "the panel's own speciality's administrator manages its members, and the body stays");

        var adminOfB = TestPrincipals.InstitutionalAdmin(InstitutionB);
        var outOfScope = await RefusalAsync(() => handler.Handle(
            new UpdateDecisionPanelCommand(PanelA, [new DecisionPanelMemberInput("x", DecisionPanelMemberRole.Chair)], adminOfB),
            CancellationToken.None));
        var unknown = await RefusalAsync(() => handler.Handle(
            new UpdateDecisionPanelCommand(999, [new DecisionPanelMemberInput("x", DecisionPanelMemberRole.Chair)], adminOfB),
            CancellationToken.None));

        unknown.Should().BeOfType<UnauthorizedAccessException>().Which.Message.Should().Be(outOfScope.Message);
    }

    [Fact]
    public async Task AnotherSpecialitysAdmin_CannotNameTheChairOfABodyPanel_OrReadItsMembers_AndNothingIsWritten()
    {
        // The review's hijack: the body tag is the institution's, but whoever names the chair of the panel carrying it
        // decides EPAs 4 and 5 for every paediatric trainee at A, ahead of every other panel. A Surgery SpecialityAdmin at
        // A used to pass the panel check, which never read the panel's speciality, and could name anyone chair,
        // themselves included.
        await using var db = await SeededDbAsync();
        await SetBodyAsync(db, PaediatricPanelA, Neonatal, TestPrincipals.InstitutionalAdmin(InstitutionA));
        var surgeryAdmin = TestPrincipals.InRole(WombatRoles.SpecialityAdmin, "surgery-admin", InstitutionA, Surgery);

        var refusal = await RefusalAsync(() => new UpdateDecisionPanelCommandHandler(db, Seats).Handle(
            new UpdateDecisionPanelCommand(
                PaediatricPanelA, [new DecisionPanelMemberInput("surgery-admin", DecisionPanelMemberRole.Chair)], surgeryAdmin),
            CancellationToken.None));

        refusal.Should().BeOfType<UnauthorizedAccessException>()
            .Which.Message.Should().Be("You can only manage panels in your institution.", "the one refusal, as for an unknown id");
        await SaveAndClearAsAuditPipelineWouldAsync(db);
        (await StoredMembersAsync(PaediatricPanelA)).Should().Equal($"chair-{PaediatricPanelA}");
        (await StoredBodiesAsync())[PaediatricPanelA].Should().Be(Neonatal);

        (await new GetDecisionPanelByIdQueryHandler(db).Handle(
                new GetDecisionPanelByIdQuery(PaediatricPanelA, surgeryAdmin), CancellationToken.None))
            .Should().BeNull("the form's read is the panels the caller may change");
    }

    // ─── Fixture ─────────────────────────────────────────────────────────────

    private static async Task<DecisionPanelDetailDto> SetBodyAsync(
        ApplicationDbContext db, int panelId, string? bodyKey, ClaimsPrincipal principal)
    {
        var result = await new SetDecisionPanelBodyCommandHandler(db).Handle(
            new SetDecisionPanelBodyCommand(panelId, bodyKey, principal), CancellationToken.None);
        db.ChangeTracker.Clear();
        return result;
    }

    private async Task<Dictionary<int, string?>> StoredBodiesAsync()
    {
        await using var read = CreateDb();
        return await read.DecisionPanels.ToDictionaryAsync(panel => panel.Id, panel => panel.DecisionBodyKey);
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

    private static async Task SaveAndClearAsAuditPipelineWouldAsync(ApplicationDbContext db)
    {
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }

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

        throw new InvalidOperationException("Expected a refusal.");
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
        db.DecisionBodies.Add(new DecisionBody { Key = Neonatal, Name = "Neonatal team Clinical Competency Committee" });
        db.DecisionPanels.AddRange(
            Panel(PanelA, InstitutionA, null),
            Panel(PaediatricPanelA, InstitutionA, Paediatrics),
            Panel(PanelB, InstitutionB, null));

        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return db;
    }

    private static DecisionPanel Panel(int id, int institutionId, int? specialityId)
        => new()
        {
            Id = id,
            Name = $"Panel {id}",
            Scope = specialityId is null ? DecisionPanelScope.Institution : DecisionPanelScope.Speciality,
            InstitutionId = institutionId,
            SpecialityId = specialityId,
            CreatedOn = DateTime.UtcNow,
            Members = [new DecisionPanelMember { UserId = $"chair-{id}", Role = DecisionPanelMemberRole.Chair }]
        };

    private ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options);
}
