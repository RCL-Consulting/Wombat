using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Audit.Queries.GetAuditEntryById;
using Wombat.Application.Features.Audit.Queries.ListAuditEntries;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Audit;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Audit;

/// <summary>
/// T056.e scope-guard tests: InstitutionalAdmin sees only audit entries scoped to their
/// institution or global (no-institution) entries.
/// </summary>
public sealed class AuditScopeGuardTests
{
    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task ListAuditEntries_InstitutionalAdmin_SeesOwnAndGlobal()
    {
        await using var db = CreateDb();
        var occ = DateTime.UtcNow.AddMinutes(-1);
        var entries = new[]
        {
            AuditEntry.Create(occ, AuditCategory.Command, "OwnCmd", true, institutionId: 1),
            AuditEntry.Create(occ, AuditCategory.Command, "OtherCmd", true, institutionId: 2),
            AuditEntry.Create(occ, AuditCategory.Authentication, "GlobalEvent", true)
        };
        db.Set<AuditEntry>().AddRange(entries);
        await db.SaveChangesAsync();

        var handler = new ListAuditEntriesQueryHandler(db);
        var result = await handler.Handle(
            new ListAuditEntriesQuery(TestPrincipals.InstitutionalAdmin(institutionId: 1)),
            CancellationToken.None);

        result.Items.Select(i => i.Action).Should().BeEquivalentTo(new[] { "OwnCmd", "GlobalEvent" });
    }

    [Fact]
    public async Task GetAuditEntryById_InstitutionalAdmin_OtherInstitution_ReturnsNull()
    {
        await using var db = CreateDb();
        var entry = AuditEntry.Create(DateTime.UtcNow.AddMinutes(-1), AuditCategory.Command, "OtherCmd", true, institutionId: 2);
        db.Set<AuditEntry>().Add(entry);
        await db.SaveChangesAsync();

        var handler = new GetAuditEntryByIdQueryHandler(db);
        var result = await handler.Handle(
            new GetAuditEntryByIdQuery(entry.Id, TestPrincipals.InstitutionalAdmin(institutionId: 1)),
            CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAuditEntryById_InstitutionalAdmin_GlobalEvent_Visible()
    {
        await using var db = CreateDb();
        var entry = AuditEntry.Create(DateTime.UtcNow.AddMinutes(-1), AuditCategory.Authentication, "GlobalEvent", true);
        db.Set<AuditEntry>().Add(entry);
        await db.SaveChangesAsync();

        var handler = new GetAuditEntryByIdQueryHandler(db);
        var result = await handler.Handle(
            new GetAuditEntryByIdQuery(entry.Id, TestPrincipals.InstitutionalAdmin(institutionId: 1)),
            CancellationToken.None);

        result.Should().NotBeNull();
    }
}
