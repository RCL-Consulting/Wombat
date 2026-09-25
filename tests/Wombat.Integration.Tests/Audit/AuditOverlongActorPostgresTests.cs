using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Audit;
using Wombat.Application.Features.Institutions;
using Wombat.Application.Features.Institutions.Commands.CreateInstitution;
using Wombat.Domain.Audit;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Audit;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Integration.Tests.Audit;

/// <summary>
/// T208 on a real PostgreSQL server: a request whose User-Agent header, or whose actor's display name, is longer than
/// its audit column still leaves its audit row, truncated, and the caller sees the command's own outcome.
/// </summary>
/// <remarks>
/// <para>
/// Until T208 <see cref="AuditEntry.Create" /> passed both through whole, and <see cref="HttpAuditContextProvider" /> hands
/// it the raw header. The row's save was refused (<c>varchar(500)</c>, <c>varchar(200)</c>), so a command that had
/// committed answered with EF's error and left no row, and a command that failed answered with EF's error in place of
/// its own. Only a real server refuses a value longer than its column: EF InMemory stores anything.
/// </para>
/// <para>
/// Each command runs as a request would: the real <see cref="HttpAuditContextProvider" /> reading the header and the
/// signed-in user from an HTTP context, inside the real <see cref="AuditPipelineBehavior{TRequest,TResponse}" />, writing
/// through the real <see cref="AuditWriter" /> on the handler's own context. Isolated as
/// <c>AuditOnRefusedSavePostgresTests</c> is: a migrated schema of its own, dropped in a finally and again on dispose.
/// </para>
/// </remarks>
public sealed class AuditOverlongActorPostgresTests : IAsyncLifetime
{
    private const string ActorId = "admin-t208";

    /// <summary>Four times the column's width.</summary>
    private static readonly string LongUserAgent = string.Concat(Enumerable.Repeat("Mozilla/5.0 (T208) ", 106))[..2000];

    /// <summary>
    /// Over the column's width, with an emoji straddling the cut: 198 letters, then a surrogate pair at the 199th and
    /// 200th characters. A cut at 199 would keep the pair's first half alone.
    /// </summary>
    private static readonly string LongDisplay = new string('d', 198) + "\U0001F600" + new string('e', 100);

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task ACommandThatSucceeds_WithA2000CharacterUserAgent_LeavesItsRow_Truncated()
    {
        LongUserAgent.Should().HaveLength(2000);

        try
        {
            var schema = await MigratedSchemaAsync();
            var suffix = Guid.NewGuid().ToString("N")[..8];

            var created = await CreateInstitutionAsync(schema, $"T208 {suffix}", $"T208-{suffix}", Administrator());

            created.ShortCode.Should().Be($"T208-{suffix}");

            await using var read = NewContext(schema);
            (await read.Institutions.CountAsync(institution => institution.ShortCode == $"T208-{suffix}")).Should().Be(1);

            var row = (await read.AuditEntries
                    .AsNoTracking()
                    .Where(entry => entry.Action == nameof(CreateInstitutionCommand))
                    .ToListAsync())
                .Should().ContainSingle("the command committed, so its row must be there too").Subject;

            row.Success.Should().BeTrue();
            row.ActorUserId.Should().Be(ActorId);
            ShouldBeTruncated(row);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// A handler that refuses before it mutates anything: the failure row goes through the ordinary write, and the
    /// caller must see the handler's refusal, not the error of a row the database would not take.
    /// </summary>
    [Fact]
    public async Task ACommandThatFails_WithA2000CharacterUserAgent_LeavesItsRow_Truncated_AndSurfacesItsOwnError()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            var suffix = Guid.NewGuid().ToString("N")[..8];

            var create = () => CreateInstitutionAsync(schema, $"T208 {suffix}", $"T208-{suffix}", Trainee());

            (await create.Should().ThrowAsync<UnauthorizedAccessException>())
                .Which.Message.Should().Be("Only global administrators may create institutions.");

            await using var read = NewContext(schema);
            var row = (await read.AuditEntries
                    .AsNoTracking()
                    .Where(entry => entry.Action == nameof(CreateInstitutionCommand))
                    .ToListAsync())
                .Should().ContainSingle("the refused command leaves its row").Subject;

            row.Success.Should().BeFalse();
            row.ErrorMessage.Should().Be("Only global administrators may create institutions.");
            ShouldBeTruncated(row);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    private static void ShouldBeTruncated(AuditEntry row)
    {
        row.ActorUserAgent.Should().HaveLength(AuditEntry.MaxActorUserAgentLength);
        row.ActorUserAgent.Should().Be(LongUserAgent[..(AuditEntry.MaxActorUserAgentLength - 1)] + "…");

        row.ActorDisplay.Should().Be(
            new string('d', 198) + "…",
            "the cut backs off the emoji whole rather than store half of it");
    }

    // ─── The command, as a request runs it ───────────────────────────────────

    /// <summary>
    /// Signed in as the actor, with the long header and name, whichever principal the command itself carries: the row
    /// reads who sent it from the HTTP context, as <see cref="HttpAuditContextProvider" /> does in the app.
    /// </summary>
    private async Task<InstitutionDto> CreateInstitutionAsync(
        string schema, string name, string shortCode, ClaimsPrincipal principal)
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, ActorId), new Claim(ClaimTypes.Name, LongDisplay)],
                "IntegrationTest"))
        };
        httpContext.Request.Headers.UserAgent = LongUserAgent;
        var auditContext = new HttpAuditContextProvider(new HttpContextAccessor { HttpContext = httpContext });

        await using var db = NewContext(schema);
        var command = new CreateInstitutionCommand(name, shortCode, ContactEmail: null, principal);
        var handler = new CreateInstitutionCommandHandler(db);

        return await new AuditPipelineBehavior<CreateInstitutionCommand, InstitutionDto>(new AuditWriter(db), auditContext)
            .Handle(command, () => handler.Handle(command, CancellationToken.None), CancellationToken.None);
    }

    private static ClaimsPrincipal Administrator() => Principal(WombatRoles.Administrator);

    private static ClaimsPrincipal Trainee() => Principal(WombatRoles.Trainee);

    private static ClaimsPrincipal Principal(string role)
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, ActorId),
                new Claim(ClaimTypes.Role, role)
            ],
            "IntegrationTest",
            ClaimTypes.Name,
            ClaimTypes.Role));

    // ─── Schema helpers (as AuditOnRefusedSavePostgresTests) ─────────────────

    private async Task<string> MigratedSchemaAsync()
    {
        var schema = await _schemas.CreateAsync();

        await using var db = NewContext(schema);
        await db.Database.MigrateAsync();

        return schema;
    }

    private ApplicationDbContext NewContext(string schema)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(TestDatabase.SchemaConnectionString(schema)).Options);
}
