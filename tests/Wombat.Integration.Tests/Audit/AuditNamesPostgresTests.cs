using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Audit;
using Wombat.Application.Features.Audit.Queries.GetAuditEntryById;
using Wombat.Application.Features.Audit.Queries.ListAuditEntries;
using Wombat.Domain.Audit;
using Wombat.Domain.DataRights;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.Audit;

/// <summary>
/// T400 (flow 18, lane C) on a real PostgreSQL server: the audit log's names and filters as Npgsql translates them. Each
/// person by the one names reader (an erased person by their pseudonym, matched in <c>DataRightsErasureRecords</c> by the id
/// they had, Q15, M3), each row's institution by name (T19), the Institution filter's three kinds, among them
/// <c>InstitutionId IS NULL</c> (K15), ignored for an institution's administrator (T101), and an entry's speciality by
/// name. Isolated as <c>AuditEntryOrderPostgresTests</c> is: a migrated schema of its own, dropped in a finally and on
/// dispose.
/// </summary>
public sealed class AuditNamesPostgresTests : IAsyncLifetime
{
    private static readonly DateTime At = new(2026, 11, 10, 17, 0, 0, DateTimeKind.Utc);

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task TheLog_NamesPeopleAndInstitutions_AndFiltersByInstitutionInItsThreeKinds()
    {
        try
        {
            var schema = await MigratedSchemaAsync();
            int alpha, beta;
            Guid fromAlpha;
            await using (var arrange = NewContext(schema))
            {
                var a = new Institution { Name = "T400 Alpha Hospital", ShortCode = "T4A" };
                var b = new Institution { Name = "T400 Beta Hospital", ShortCode = "T4B" };
                arrange.Institutions.AddRange(a, b);
                await arrange.SaveChangesAsync();
                alpha = a.Id;
                beta = b.Id;

                // An erasure record belongs to the request it carried out (its foreign key).
                var request = DataRightsRequest.Create("t400-sipho", "Sipho Ndlovu", DataRightsRequestType.Erasure, "Leaving.", At);
                arrange.DataRightsRequests.Add(request);
                await arrange.SaveChangesAsync();
                arrange.DataRightsErasureRecords.Add(DataRightsErasureRecord.Create(
                    request.Id, "t400-sipho", "deleted_user_t400", At.AddDays(1), "[]"));
                var erased = AuditEntry.Create(
                    At.AddMinutes(-3), AuditCategory.Command, "CreateActivityCommand", false,
                    actorUserId: "t400-sipho", actorDisplay: "Sipho Ndlovu", institutionId: alpha,
                    errorMessage: "Date observed: The date cannot be after today (2026-11-10).");
                fromAlpha = erased.Id;
                arrange.AuditEntries.AddRange(
                    erased,
                    AuditEntry.Create(At.AddMinutes(-2), AuditCategory.Authentication, "Login", true,
                        actorUserId: "t400-mbatha", actorDisplay: "stale", institutionId: beta),
                    AuditEntry.Create(At.AddMinutes(-1), AuditCategory.Command, "SubmitMsfResponseCommand", true));
                await arrange.SaveChangesAsync();
            }

            var users = new FakeUserDirectory(("t400-mbatha", "Nolwazi Mbatha"), ("t400-sipho", "Sipho Ndlovu"));

            async Task<PagedAuditResult> ReadAsync(ClaimsPrincipal principal, AuditInstitutionFilter? filter)
            {
                await using var db = NewContext(schema);
                return await new ListAuditEntriesQueryHandler(db, users, TimeProvider.System).Handle(
                    new ListAuditEntriesQuery(principal, From: At.AddHours(-1), To: At, Institution: filter),
                    CancellationToken.None);
            }

            var all = await ReadAsync(Administrator(), null);
            all.Items.Select(row => (row.Action, row.ActorKind, row.PersonName, row.InstitutionName)).Should().Equal(
                ("SubmitMsfResponseCommand", AuditActorKind.MsfRespondent, (string?)null, (string?)null),
                ("Login", AuditActorKind.Person, "Nolwazi Mbatha", "T400 Beta Hospital"),
                ("CreateActivityCommand", AuditActorKind.Person, "deleted_user_t400", "T400 Alpha Hospital"));
            all.Items[2].ErrorMessage.Should().Be("Date observed: The date cannot be after today (2026-11-10).");
            all.Institutions.Select(option => option.Name).Should().Contain(["T400 Alpha Hospital", "T400 Beta Hospital"]);

            (await ReadAsync(Administrator(), AuditInstitutionFilter.NoInstitution)).Items.Select(row => row.Action)
                .Should().Equal("SubmitMsfResponseCommand");
            (await ReadAsync(Administrator(), AuditInstitutionFilter.One(beta))).Items.Select(row => row.Action)
                .Should().Equal("Login");
            (await ReadAsync(Administrator(), AuditInstitutionFilter.All)).TotalCount.Should().Be(3);

            var theirs = await ReadAsync(InstitutionalAdmin(alpha), AuditInstitutionFilter.One(beta));
            theirs.Items.Select(row => row.Action).Should().Equal(["CreateActivityCommand"], "pinned to theirs (T101)");
            theirs.ScopeName.Should().Be("T400 Alpha Hospital");

            await using (var db = NewContext(schema))
            {
                var entry = await new GetAuditEntryByIdQueryHandler(db, users).Handle(
                    new GetAuditEntryByIdQuery(fromAlpha, InstitutionalAdmin(alpha)), CancellationToken.None);
                entry!.PersonName.Should().Be("deleted_user_t400");
                entry.InstitutionName.Should().Be("T400 Alpha Hospital");
                entry.SummaryJson.Should().BeNull();

                (await new GetAuditEntryByIdQueryHandler(db, users).Handle(
                    new GetAuditEntryByIdQuery(fromAlpha, InstitutionalAdmin(beta)), CancellationToken.None))
                    .Should().BeNull();
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    private static ClaimsPrincipal Administrator()
        => Principal(WombatRoles.Administrator, null);

    private static ClaimsPrincipal InstitutionalAdmin(int institutionId)
        => Principal(WombatRoles.InstitutionalAdmin, institutionId);

    private static ClaimsPrincipal Principal(string role, int? institutionId)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, $"t400-{role}"), new(ClaimTypes.Role, role) };
        if (institutionId is int id)
        {
            claims.Add(new Claim(Wombat.Application.Common.Security.WombatClaimTypes.InstitutionId, id.ToString()));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "IntegrationTest", ClaimTypes.Name, ClaimTypes.Role));
    }

    private async Task<string> MigratedSchemaAsync()
    {
        var schema = await _schemas.CreateAsync();

        await using var db = NewContext(schema);
        await db.Database.MigrateAsync();

        return schema;
    }

    private static ApplicationDbContext NewContext(string schema)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(TestDatabase.SchemaConnectionString(schema)).Options);
}
