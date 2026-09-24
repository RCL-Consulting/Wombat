using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Domain.Audit;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Audit;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Tests.Audit;

/// <summary>
/// The writer saves through the request's own DbContext. Its two writes differ only in what else that save carries.
/// </summary>
/// <remarks>
/// EF InMemory cannot refuse a save, so this shows only what each write sends. The refusal itself, with the row that
/// survives it and the error that reaches the caller, is <c>AuditOnRefusedSavePostgresTests</c>. (T201)
/// </remarks>
public sealed class AuditWriterTests
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task WriteDiscardingPendingChanges_SavesTheEntryAlone()
    {
        await using (var db = CreateDb())
        {
            db.Institutions.Add(new Institution { Name = "Refused", ShortCode = "REF" });

            await new AuditWriter(db).WriteDiscardingPendingChangesAsync(Entry());

            db.ChangeTracker.Entries().Should().ContainSingle(
                "only the entry was attached after the discard, and the save left it tracked as Unchanged");
        }

        await using var read = CreateDb();
        (await read.AuditEntries.CountAsync()).Should().Be(1);
        (await read.Institutions.CountAsync()).Should().Be(0, "the handler's pending insert was discarded, not saved");
    }

    /// <summary>
    /// The ordinary write carries whatever the handler left pending. That is the known trap, kept for every failure
    /// that is not a refused save; this pins that the discard above is a separate write, not a change to this one.
    /// </summary>
    [Fact]
    public async Task Write_AlsoSavesWhatTheHandlerLeftPending()
    {
        await using (var db = CreateDb())
        {
            db.Institutions.Add(new Institution { Name = "Pending", ShortCode = "PEN" });

            await new AuditWriter(db).WriteAsync(Entry());
        }

        await using var read = CreateDb();
        (await read.AuditEntries.CountAsync()).Should().Be(1);
        (await read.Institutions.CountAsync()).Should().Be(1);
    }

    private static AuditEntry Entry()
        => AuditEntry.Create(
            occurredAt: DateTime.UtcNow,
            category: AuditCategory.Command,
            action: "CreateInstitutionCommand",
            success: false,
            errorMessage: "An institution with the same name or short code already exists.");

    private ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options);
}
