using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Audit;
using Wombat.Application.Features.Audit.Queries.GetAuditEntryById;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Audit;
using Wombat.Domain.DataRights;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Audit;

/// <summary>
/// T400 (flow 18, lane C, Q9, T4): one audit entry as its page reads it. Who by the one names reader (an erased person by
/// their pseudonym, Q15, M3); the institution and the speciality by name; who acted by kind (K8); the payload for the
/// Administrator alone (R2-E01); null for an id unknown or out of scope (T101).
/// </summary>
public sealed class AuditEntryReadTests
{
    private const int Kgk = 1;
    private const int Paediatrics = 4;
    private static readonly DateTime At = new(2026, 11, 10, 17, 1, 41, 861, DateTimeKind.Utc);

    [Fact]
    public async Task AnErasedPerson_IsNamedByTheirPseudonym_NeverByTheStoredName()
    {
        await using var db = CreateDb();
        db.Set<DataRightsErasureRecord>().Add(DataRightsErasureRecord.Create(
            Guid.NewGuid(), "c22b8779", "deleted_user_0ae1d64b", At.AddDays(1), "[]"));
        var entry = Add(db, "CreateActivityCommand", userId: "c22b8779", display: "Sipho Ndlovu", institutionId: Kgk);
        await db.SaveChangesAsync();

        var read = await Read(db, entry.Id, TestPrincipals.InstitutionalAdmin(Kgk), new FakeUserDirectory(("c22b8779", "Sipho Ndlovu")));

        read!.ActorKind.Should().Be(AuditActorKind.Person);
        read.PersonName.Should().Be("deleted_user_0ae1d64b");
        read.ActorUserId.Should().Be("c22b8779", "the entry keeps the id the row stores (R3-E09)");
        read.InstitutionName.Should().Be("Kgosi Kgari Teaching Hospital");
    }

    [Fact]
    public async Task ALivingPerson_IsTheirLiveName()
    {
        await using var db = CreateDb();
        var entry = Add(db, "TransitionActivityCommand", userId: "naidoo", display: "Dr D Naidoo", institutionId: Kgk);
        await db.SaveChangesAsync();

        var read = await Read(db, entry.Id, TestPrincipals.Administrator(), new FakeUserDirectory(("naidoo", "David Naidoo")));

        read!.PersonName.Should().Be("David Naidoo");
    }

    [Fact]
    public async Task TheRespondentsSubmission_IsTheMsfRespondent_WithNoInstitution()
    {
        await using var db = CreateDb();
        var entry = Add(db, "SubmitMsfResponseCommand", summaryJson: """{"token":"[REDACTED]","answers":"[REDACTED]"}""");
        await db.SaveChangesAsync();

        var read = await Read(db, entry.Id, TestPrincipals.Administrator(), FakeUserDirectory.Empty);

        read!.ActorKind.Should().Be(AuditActorKind.MsfRespondent);
        read.PersonName.Should().BeNull();
        read.InstitutionName.Should().BeNull();
        read.SummaryJson.Should().Contain("token");
    }

    [Fact]
    public async Task ASubject_IsReadWithItsInstitutionAndSpecialityByName()
    {
        await using var db = CreateDb();
        var subject = Guid.NewGuid();
        var entry = AuditEntry.Create(
            At, AuditCategory.Committee, "RecordCommitteeDecision", true,
            actorUserId: "zulu", subjectType: "CommitteeReview", subjectId: subject, institutionId: Kgk, specialityId: Paediatrics);
        db.Set<AuditEntry>().Add(entry);
        await db.SaveChangesAsync();

        var read = await Read(db, entry.Id, TestPrincipals.Administrator(), new FakeUserDirectory(("zulu", "Thandi Zulu")));

        read!.SubjectType.Should().Be("CommitteeReview");
        read.SubjectId.Should().Be(subject);
        read.InstitutionName.Should().Be("Kgosi Kgari Teaching Hospital");
        read.SpecialityName.Should().Be("Paediatrics");
    }

    [Fact]
    public async Task ThePayload_IsTheAdministratorsAlone()
    {
        await using var db = CreateDb();
        var entry = Add(db, "RecordCommitteeDecisionCommand", userId: "zulu", institutionId: Kgk, summaryJson: """{"rationale":"x"}""");
        await db.SaveChangesAsync();

        var administrator = await Read(db, entry.Id, TestPrincipals.Administrator(), FakeUserDirectory.Empty);
        var theirs = await Read(db, entry.Id, TestPrincipals.InstitutionalAdmin(Kgk), FakeUserDirectory.Empty);

        administrator!.SummaryJson.Should().Be("""{"rationale":"x"}""");
        theirs!.SummaryJson.Should().BeNull("an institution's administrator reads no payload (R2-E01)");
    }

    [Fact]
    public async Task AnEntryOutOfScope_OrUnknown_IsNull_AndNoNameIsRead()
    {
        await using var db = CreateDb();
        var other = Add(db, "RunScheduledJobNowCommand", userId: "admin-1");
        await db.SaveChangesAsync();
        var users = new FakeUserDirectory(("admin-1", "Demo Administrator"));

        (await Read(db, other.Id, TestPrincipals.InstitutionalAdmin(Kgk), users)).Should().BeNull();
        (await Read(db, Guid.NewGuid(), TestPrincipals.Administrator(), users)).Should().BeNull();
        users.Lookups.Should().BeEmpty("a name is read only for a row the caller may read (T101)");
    }

    private static Task<AuditEntryDetailDto?> Read(
        ApplicationDbContext db, Guid id, System.Security.Claims.ClaimsPrincipal principal, FakeUserDirectory users)
        => new GetAuditEntryByIdQueryHandler(db, users).Handle(new GetAuditEntryByIdQuery(id, principal), CancellationToken.None);

    private static AuditEntry Add(
        ApplicationDbContext db,
        string action,
        string? userId = null,
        string? display = null,
        int? institutionId = null,
        string? summaryJson = null)
    {
        var entry = AuditEntry.Create(
            At, AuditCategory.Command, action, success: true,
            actorUserId: userId, actorDisplay: display, institutionId: institutionId, summaryJson: summaryJson ?? "{}");
        db.Set<AuditEntry>().Add(entry);
        return entry;
    }

    private static ApplicationDbContext CreateDb()
    {
        var db = new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        db.Set<Institution>().Add(new Institution { Id = Kgk, Name = "Kgosi Kgari Teaching Hospital", ShortCode = "KGK" });
        db.Set<Speciality>().Add(new Speciality { Id = Paediatrics, CollegeId = 1, Name = "Paediatrics" });
        db.SaveChanges();
        return db;
    }
}
