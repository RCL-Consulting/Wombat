using System.Security.Claims;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Audit;
using Wombat.Application.Features.Institutions;
using Wombat.Application.Features.Institutions.Commands.DeactivateInstitution;
using Wombat.Application.Features.Institutions.Commands.ReactivateInstitution;
using Wombat.Application.Features.Institutions.Commands.UpdateInstitution;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Audit;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Audit;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.Institutions;

/// <summary>
/// T302: whether an institution is active is the Administrator's alone, in the commands and not only on the page. An
/// inactive institution issues no invitations, so its state turns onboarding there on and off.
/// </summary>
/// <remarks>
/// <para>
/// T056 made Deactivate the Administrator's outright, but <c>UpdateInstitutionCommand</c> carried <c>IsActive</c> and
/// wrote it for any caller in scope. So Prof Mbatha, KGK's InstitutionalAdmin, was refused Deactivate and then unticked
/// Active and saved KGK inactive (the T295 replay, Step A.6.3); and she could as well have reactivated KGK after an
/// Administrator had deactivated it. Now the update carries no state, and each state change is one command with one rule:
/// <see cref="DeactivateInstitutionCommand" /> and <see cref="ReactivateInstitutionCommand" />, both the Administrator's.
/// </para>
/// <para>
/// Each command runs inside the real <see cref="AuditPipelineBehavior{TRequest,TResponse}" /> writing through the real
/// <see cref="AuditWriter" /> on the handler's own context, as a request does, because that is where the audit trap
/// springs: the failure row's save flushes whatever the handler left tracked. What it left is read back through a
/// second context.
/// </para>
/// </remarks>
public sealed class InstitutionActiveStateTests : IAsyncLifetime
{
    private const string KgkName = "Kgosi Kgari Teaching Hospital";
    private const string KgkShortCode = "KGK";
    private const string KgkEmail = "paeds-admin@kgk.wombat.local";

    private readonly string _databaseName = Guid.NewGuid().ToString();
    private int _kgkId;

    public async Task InitializeAsync()
    {
        await using var db = CreateDb();
        var kgk = new Institution
        {
            Name = KgkName,
            ShortCode = KgkShortCode,
            ContactEmail = KgkEmail,
            IsActive = true,
            CreatedOn = new DateTime(2026, 9, 26, 8, 0, 0, DateTimeKind.Utc)
        };
        db.Institutions.AddRange(
            new Institution { Name = "Demo Institution", ShortCode = "DEMO", IsActive = true, CreatedOn = DateTime.UtcNow },
            kgk);
        await db.SaveChangesAsync();
        _kgkId = kgk.Id;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // ─── The update carries no state ─────────────────────────────────────────

    [Fact]
    public void TheUpdate_CarriesNoActiveState_SoNoUpdateCanChangeIt()
    {
        // The bypass was a field on the update. With none, the only ways to change the state are the two commands below,
        // each with the Administrator's rule; a field put back would reopen the bypass for every caller in scope.
        typeof(UpdateInstitutionCommand).GetProperties().Select(property => property.Name)
            .Should().NotContain(nameof(Institution.IsActive));
        typeof(UpdateInstitutionCommand).GetConstructors()
            .SelectMany(constructor => constructor.GetParameters())
            .Select(parameter => parameter.ParameterType)
            .Should().NotContain(typeof(bool), "no switch on the update may stand for the state");
    }

    [Fact]
    public async Task AnInstitutionalAdmin_StillSavesHerOwnInstitutionsNameShortCodeAndContactEmail()
    {
        await using (var db = CreateDb())
        {
            var saved = await SendThroughTheAuditPipelineAsync(db, new UpdateInstitutionCommand(
                _kgkId, "  Kgosi Kgari Hospital  ", " KGKH ", " hod.paediatrics@kgk.wombat.local ", Mbatha()));

            saved.Name.Should().Be("Kgosi Kgari Hospital");
            saved.ShortCode.Should().Be("KGKH");
            saved.ContactEmail.Should().Be("hod.paediatrics@kgk.wombat.local");
            saved.IsActive.Should().BeTrue();
        }

        var stored = await StoredKgkAsync();
        stored.Name.Should().Be("Kgosi Kgari Hospital");
        stored.ShortCode.Should().Be("KGKH");
        stored.ContactEmail.Should().Be("hod.paediatrics@kgk.wombat.local");
        stored.IsActive.Should().BeTrue("her save changes what she may change, and nothing else");
    }

    [Fact]
    public async Task HerUpdateOfTheInstitutionAnAdministratorDeactivated_LeavesItInactive()
    {
        // The reverse of the replay's finding: before T302 the page sent the box's state with every save, so her save of
        // an inactive institution's record, box ticked, reactivated it.
        await DeactivateAsAdministratorAsync();

        await using (var db = CreateDb())
        {
            var saved = await SendThroughTheAuditPipelineAsync(
                db, new UpdateInstitutionCommand(_kgkId, KgkName, KgkShortCode, "hod.paediatrics@kgk.wombat.local", Mbatha()));

            saved.IsActive.Should().BeFalse();
        }

        var stored = await StoredKgkAsync();
        stored.IsActive.Should().BeFalse("only an Administrator may reactivate it");
        stored.ContactEmail.Should().Be("hod.paediatrics@kgk.wombat.local");
    }

    // ─── Deactivate ──────────────────────────────────────────────────────────

    [Fact]
    public async Task AnInstitutionalAdminWhoDeactivatesHerOwnInstitution_IsRefused_AndAfterTheAuditSave_ItIsUnchanged()
    {
        await using (var db = CreateDb())
        {
            var act = () => SendThroughTheAuditPipelineAsync(db, new DeactivateInstitutionCommand(_kgkId, Mbatha()));

            (await act.Should().ThrowAsync<UnauthorizedAccessException>())
                .WithMessage("Only global administrators may deactivate institutions.");

            // The failure row is written through the same context, and its save flushes whatever the handler left
            // tracked. Save once more, as a later command on the request would, before reading back.
            await db.SaveChangesAsync();
        }

        var stored = await StoredKgkAsync();
        stored.IsActive.Should().BeTrue();
        stored.Name.Should().Be(KgkName);
        stored.ShortCode.Should().Be(KgkShortCode);
        stored.ContactEmail.Should().Be(KgkEmail);

        (await AuditRowsAsync()).Should().ContainSingle()
            .Which.Should().Match<AuditEntry>(row => row.Action == nameof(DeactivateInstitutionCommand) && !row.Success);
    }

    // ─── Reactivate ──────────────────────────────────────────────────────────

    [Fact]
    public async Task AnInstitutionalAdmin_CannotReactivateHerInactiveInstitution()
    {
        await DeactivateAsAdministratorAsync();

        await using (var db = CreateDb())
        {
            var act = () => SendThroughTheAuditPipelineAsync(db, new ReactivateInstitutionCommand(_kgkId, Mbatha()));

            (await act.Should().ThrowAsync<UnauthorizedAccessException>())
                .WithMessage("Only global administrators may reactivate institutions.");

            await db.SaveChangesAsync();
        }

        var stored = await StoredKgkAsync();
        stored.IsActive.Should().BeFalse("reactivating restarts onboarding there, which is the Administrator's to decide");
        stored.Name.Should().Be(KgkName);
        stored.ShortCode.Should().Be(KgkShortCode);
        stored.ContactEmail.Should().Be(KgkEmail);
    }

    [Fact]
    public async Task AnAdministrator_DeactivatesAndReactivatesIt()
    {
        await DeactivateAsAdministratorAsync();
        (await StoredKgkAsync()).IsActive.Should().BeFalse();

        await using (var db = CreateDb())
        {
            await SendThroughTheAuditPipelineAsync(db, new ReactivateInstitutionCommand(_kgkId, TestPrincipals.Administrator()));
        }

        var stored = await StoredKgkAsync();
        stored.IsActive.Should().BeTrue();
        stored.Name.Should().Be(KgkName, "a state change changes nothing else");
        stored.ContactEmail.Should().Be(KgkEmail);

        (await AuditRowsAsync()).Select(row => (row.Action, row.Success)).Should().Equal(
            (nameof(DeactivateInstitutionCommand), true),
            (nameof(ReactivateInstitutionCommand), true));
    }

    [Fact]
    public async Task ReactivatingAnInstitutionThatDoesNotExist_IsRefused()
    {
        await using var db = CreateDb();

        var act = () => SendThroughTheAuditPipelineAsync(db, new ReactivateInstitutionCommand(9_999, TestPrincipals.Administrator()));

        (await act.Should().ThrowAsync<InvalidOperationException>()).WithMessage("Institution 9999 was not found.");
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    /// <summary>Prof Mbatha, KGK's InstitutionalAdmin.</summary>
    private ClaimsPrincipal Mbatha() => TestPrincipals.InstitutionalAdmin(_kgkId, "mbatha");

    private async Task DeactivateAsAdministratorAsync()
    {
        await using var db = CreateDb();
        await SendThroughTheAuditPipelineAsync(db, new DeactivateInstitutionCommand(_kgkId, TestPrincipals.Administrator()));
    }

    private async Task<Institution> StoredKgkAsync()
    {
        await using var db = CreateDb();
        return await db.Institutions.AsNoTracking().SingleAsync(institution => institution.Id == _kgkId);
    }

    private async Task<List<AuditEntry>> AuditRowsAsync()
    {
        await using var db = CreateDb();
        // By OccurredAt: the Id is a version-7 Guid, random within its millisecond (AuditEntry, T244).
        return await db.Set<AuditEntry>().AsNoTracking().OrderBy(row => row.OccurredAt).ToListAsync();
    }

    /// <summary>The audit behaviour outermost, then the handler: the app's order.</summary>
    private static Task<InstitutionDto> SendThroughTheAuditPipelineAsync(ApplicationDbContext db, UpdateInstitutionCommand command)
        => new AuditPipelineBehavior<UpdateInstitutionCommand, InstitutionDto>(new AuditWriter(db), new FixedAuditContext())
            .Handle(command, () => new UpdateInstitutionCommandHandler(db).Handle(command, CancellationToken.None), CancellationToken.None);

    private static Task SendThroughTheAuditPipelineAsync(ApplicationDbContext db, DeactivateInstitutionCommand command)
        => new AuditPipelineBehavior<DeactivateInstitutionCommand, Unit>(new AuditWriter(db), new FixedAuditContext())
            .Handle(
                command,
                async () =>
                {
                    await new DeactivateInstitutionCommandHandler(db).Handle(command, CancellationToken.None);
                    return Unit.Value;
                },
                CancellationToken.None);

    private static Task SendThroughTheAuditPipelineAsync(ApplicationDbContext db, ReactivateInstitutionCommand command)
        => new AuditPipelineBehavior<ReactivateInstitutionCommand, Unit>(new AuditWriter(db), new FixedAuditContext())
            .Handle(
                command,
                async () =>
                {
                    await new ReactivateInstitutionCommandHandler(db).Handle(command, CancellationToken.None);
                    return Unit.Value;
                },
                CancellationToken.None);

    private ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options);

    private sealed class FixedAuditContext : IAuditContextProvider
    {
        public string? UserId => "caller";
        public string? UserDisplay => "Caller";
        public string? IpAddress => "10.0.0.0/24";
        public string? UserAgent => "Test/1.0";
        public int? InstitutionId => null;
        public void DeclareInstitution(int institutionId) { }
        public void DeclareActor(string userId, string display) { }
    }
}
