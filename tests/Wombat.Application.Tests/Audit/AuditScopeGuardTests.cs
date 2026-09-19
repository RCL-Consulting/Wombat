using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Audit.Queries.GetAuditEntryById;
using Wombat.Application.Features.Audit.Queries.ListAuditEntries;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Audit;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Audit;

/// <summary>
/// T056.e scope-guard tests: InstitutionalAdmin sees only audit entries stamped with their
/// institution.
///
/// T101 removed the "or global (no-institution) entries" allowance these tests used to assert.
/// A null InstitutionId means unknown scope, not global scope, and every command row was written
/// unstamped — so the allowance let an InstitutionalAdmin read other institutions' command
/// SummaryJson, raw activity form data included. Unknown scope is Administrator-only now; the
/// pipeline stamps the actor's institution so scoped admins still see their own institution's work.
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
    public async Task ListAuditEntries_InstitutionalAdmin_SeesOnlyOwnInstitution()
    {
        await using var db = CreateDb();
        var occ = DateTime.UtcNow.AddMinutes(-1);
        var entries = new[]
        {
            AuditEntry.Create(occ, AuditCategory.Command, "OwnCmd", true, institutionId: 1),
            AuditEntry.Create(occ, AuditCategory.Command, "OtherCmd", true, institutionId: 2),
            AuditEntry.Create(occ, AuditCategory.Authentication, "UnscopedEvent", true)
        };
        db.Set<AuditEntry>().AddRange(entries);
        await db.SaveChangesAsync();

        var handler = new ListAuditEntriesQueryHandler(db);
        var result = await handler.Handle(
            new ListAuditEntriesQuery(TestPrincipals.InstitutionalAdmin(institutionId: 1)),
            CancellationToken.None);

        result.Items.Select(i => i.Action).Should().BeEquivalentTo(new[] { "OwnCmd" });
    }

    [Fact]
    public async Task ListAuditEntries_Administrator_SeesUnscopedEntries()
    {
        await using var db = CreateDb();
        var occ = DateTime.UtcNow.AddMinutes(-1);
        db.Set<AuditEntry>().Add(
            AuditEntry.Create(occ, AuditCategory.Authentication, "UnscopedEvent", true));
        await db.SaveChangesAsync();

        var handler = new ListAuditEntriesQueryHandler(db);
        var result = await handler.Handle(
            new ListAuditEntriesQuery(TestPrincipals.Administrator()),
            CancellationToken.None);

        result.Items.Select(i => i.Action).Should().BeEquivalentTo(new[] { "UnscopedEvent" });
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

    /// <summary>
    /// The leak T101 closed: an unstamped row used to be readable by every InstitutionalAdmin, and
    /// AuditDetail renders its SummaryJson raw.
    /// </summary>
    [Fact]
    public async Task GetAuditEntryById_InstitutionalAdmin_UnscopedEntry_ReturnsNull()
    {
        await using var db = CreateDb();
        var entry = AuditEntry.Create(DateTime.UtcNow.AddMinutes(-1), AuditCategory.Authentication, "UnscopedEvent", true);
        db.Set<AuditEntry>().Add(entry);
        await db.SaveChangesAsync();

        var handler = new GetAuditEntryByIdQueryHandler(db);
        var result = await handler.Handle(
            new GetAuditEntryByIdQuery(entry.Id, TestPrincipals.InstitutionalAdmin(institutionId: 1)),
            CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAuditEntryById_Administrator_UnscopedEntry_Visible()
    {
        await using var db = CreateDb();
        var entry = AuditEntry.Create(DateTime.UtcNow.AddMinutes(-1), AuditCategory.Authentication, "UnscopedEvent", true);
        db.Set<AuditEntry>().Add(entry);
        await db.SaveChangesAsync();

        var handler = new GetAuditEntryByIdQueryHandler(db);
        var result = await handler.Handle(
            new GetAuditEntryByIdQuery(entry.Id, TestPrincipals.Administrator()),
            CancellationToken.None);

        result.Should().NotBeNull();
    }
}
