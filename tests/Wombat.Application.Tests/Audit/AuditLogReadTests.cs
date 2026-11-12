using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Audit;
using Wombat.Application.Features.Audit.Queries.ListAuditEntries;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Audit;
using Wombat.Domain.DataRights;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Audit;

/// <summary>
/// T400 (flow 18, lane C): the audit log's read. The window as the caller fixed it, returned as read (T21); the
/// Institution filter's three kinds, ignored for an institution's administrator (K15, T101); each person by the one
/// names reader, an erased person by their pseudonym (Q15, M3); the institution's name per row (T19); who acted, by kind
/// (K8).
/// </summary>
public sealed class AuditLogReadTests
{
    private const int Kgk = 1;
    private const int Demo = 2;

    // 2026-11-10 19:30 SAST.
    private static readonly DateTime Now = new(2026, 11, 10, 17, 30, 0, DateTimeKind.Utc);

    [Fact]
    public async Task NoWindowGiven_IsTheLast24HoursFromTheClock_AndIsReturned()
    {
        await using var db = CreateDb();
        Add(db, Now.AddHours(-25), "TooOld");
        Add(db, Now.AddHours(-23), "Inside");
        Add(db, Now.AddMinutes(1), "AfterNow");
        await db.SaveChangesAsync();

        var result = await Handler(db).Handle(new ListAuditEntriesQuery(TestPrincipals.Administrator()), CancellationToken.None);

        result.From.Should().Be(Now.AddHours(-24));
        result.To.Should().Be(Now);
        result.Items.Select(row => row.Action).Should().Equal("Inside");
    }

    [Fact]
    public async Task AGivenWindow_IsReadAsGiven_AndReturned_WhateverTheClockSays()
    {
        await using var db = CreateDb();
        var from = new DateTime(2026, 11, 10, 16, 58, 0, DateTimeKind.Utc);
        var to = new DateTime(2026, 11, 10, 18, 0, 0, DateTimeKind.Utc);
        Add(db, from.AddSeconds(-1), "BeforeFrom");
        Add(db, from, "AtFrom");
        Add(db, to, "AtTo");
        Add(db, to.AddSeconds(1), "AfterTo", "recorded since the window was fixed (T277)");
        await db.SaveChangesAsync();

        var clock = new FixedClock(to.AddHours(5));
        var result = await new ListAuditEntriesQueryHandler(db, FakeUserDirectory.Empty, clock).Handle(
            new ListAuditEntriesQuery(TestPrincipals.Administrator(), From: from, To: to),
            CancellationToken.None);

        result.From.Should().Be(from);
        result.To.Should().Be(to);
        result.Items.Select(row => row.Action).Should().Equal("AtTo", "AtFrom");
    }

    [Fact]
    public async Task OnlyTo_Given_FromIs24HoursBeforeIt()
    {
        await using var db = CreateDb();
        var to = Now.AddHours(-2);

        var result = await Handler(db).Handle(
            new ListAuditEntriesQuery(TestPrincipals.Administrator(), To: to), CancellationToken.None);

        result.From.Should().Be(to.AddHours(-24));
        result.To.Should().Be(to);
    }

    [Fact]
    public async Task TheInstitutionFilter_HasThreeKinds_ForTheAdministrator()
    {
        await using var db = CreateDb();
        Add(db, Now.AddMinutes(-3), "AtKgk", institutionId: Kgk);
        Add(db, Now.AddMinutes(-2), "AtDemo", institutionId: Demo);
        Add(db, Now.AddMinutes(-1), "NoInstitution");
        await db.SaveChangesAsync();

        async Task<IEnumerable<string>> Read(AuditInstitutionFilter? filter)
            => (await Handler(db).Handle(
                new ListAuditEntriesQuery(TestPrincipals.Administrator(), Institution: filter), CancellationToken.None))
                .Items.Select(row => row.Action);

        (await Read(null)).Should().BeEquivalentTo("AtKgk", "AtDemo", "NoInstitution");
        (await Read(AuditInstitutionFilter.All)).Should().BeEquivalentTo("AtKgk", "AtDemo", "NoInstitution");
        (await Read(AuditInstitutionFilter.NoInstitution)).Should().Equal("NoInstitution");
        (await Read(AuditInstitutionFilter.One(Demo))).Should().Equal("AtDemo");
    }

    [Theory]
    [MemberData(nameof(EveryKind))]
    public async Task TheInstitutionFilter_IsIgnored_ForAnInstitutionsAdministrator_PinnedToTheirs(AuditInstitutionFilterKind kind)
    {
        await using var db = CreateDb();
        Add(db, Now.AddMinutes(-3), "AtKgk", institutionId: Kgk);
        Add(db, Now.AddMinutes(-2), "AtDemo", institutionId: Demo);
        Add(db, Now.AddMinutes(-1), "NoInstitution");
        await db.SaveChangesAsync();
        var filter = kind switch
        {
            AuditInstitutionFilterKind.None => AuditInstitutionFilter.NoInstitution,
            AuditInstitutionFilterKind.One => AuditInstitutionFilter.One(Demo),
            _ => AuditInstitutionFilter.All,
        };

        var result = await Handler(db).Handle(
            new ListAuditEntriesQuery(TestPrincipals.InstitutionalAdmin(Kgk), Institution: filter), CancellationToken.None);

        result.Items.Select(row => row.Action).Should().Equal(["AtKgk"], "T101's predicate stands, and no filter widens it");
        result.EveryInstitution.Should().BeFalse();
        result.Institutions.Should().BeEmpty("the filter is not offered, so no institution's name is read for it");
        result.ScopeName.Should().Be("Kgosi Kgari Teaching Hospital", "the subtitle names their own institution (T12)");
    }

    public static TheoryData<AuditInstitutionFilterKind> EveryKind
        => [AuditInstitutionFilterKind.All, AuditInstitutionFilterKind.None, AuditInstitutionFilterKind.One];

    [Fact]
    public async Task TheAdministrator_IsOfferedEveryInstitution_ByName()
    {
        await using var db = CreateDb();

        var result = await Handler(db).Handle(new ListAuditEntriesQuery(TestPrincipals.Administrator()), CancellationToken.None);

        result.EveryInstitution.Should().BeTrue();
        result.ScopeName.Should().BeNull();
        result.Institutions.Select(option => option.Name).Should().Equal("Demo Institution", "Kgosi Kgari Teaching Hospital");
    }

    [Fact]
    public async Task EachRow_NamesItsPersonLive_ItsInstitutionByName_AndWhoActedByKind()
    {
        await using var db = CreateDb();
        db.Set<DataRightsErasureRecord>().Add(DataRightsErasureRecord.Create(
            Guid.NewGuid(), "sipho", "deleted_user_0ae1d64b", Now.AddDays(-1), "[]"));
        Add(db, Now.AddMinutes(-5), "CreateActivityCommand", userId: "sipho", display: "Sipho Ndlovu", institutionId: Kgk);
        Add(db, Now.AddMinutes(-4), "Login", userId: "mbatha", display: "Prof Mbatha (stale)", institutionId: Kgk);
        Add(db, Now.AddMinutes(-3), "SubmitMsfResponseCommand");
        Add(db, Now.AddMinutes(-2), "LoginFailed");
        Add(db, Now.AddMinutes(-1), "Login", userId: "nameless", institutionId: Demo);
        await db.SaveChangesAsync();
        var users = new FakeUserDirectory(("mbatha", "Nolwazi Mbatha"), ("sipho", "Sipho Ndlovu"), ("nameless", " "));

        var result = await new ListAuditEntriesQueryHandler(db, users, new FixedClock(Now)).Handle(
            new ListAuditEntriesQuery(TestPrincipals.Administrator()), CancellationToken.None);

        result.Items.Select(row => (row.ActorKind, row.PersonName, row.InstitutionName)).Should().Equal(
            (AuditActorKind.Person, "nameless", "Demo Institution"),
            (AuditActorKind.None, null, null),
            (AuditActorKind.MsfRespondent, null, null),
            (AuditActorKind.Person, "Nolwazi Mbatha", "Kgosi Kgari Teaching Hospital"),
            (AuditActorKind.Person, "deleted_user_0ae1d64b", "Kgosi Kgari Teaching Hospital"));
        users.Lookups.Should().ContainSingle("one directory read for the page, never one a row");
        users.Lookups.Single().Should().NotContain("sipho", "an erased person is never asked about");
    }

    [Fact]
    public async Task ARowWithNoUserId_KeepsItsStoredName_ButNeverTheRespondents()
    {
        await using var db = CreateDb();
        Add(db, Now.AddMinutes(-2), "RebuildCurriculumProgressCommand", display: "Startup");
        Add(db, Now.AddMinutes(-1), "SubmitMsfResponseCommand", display: "should never show");
        await db.SaveChangesAsync();

        var result = await Handler(db).Handle(new ListAuditEntriesQuery(TestPrincipals.Administrator()), CancellationToken.None);

        result.Items.Select(row => (row.ActorKind, row.PersonName)).Should().Equal(
            (AuditActorKind.MsfRespondent, (string?)null),
            (AuditActorKind.None, "Startup"));
    }

    [Fact]
    public async Task APageSizeOutOfRange_IsKeptInside_PagerControlsSizes()
    {
        await using var db = CreateDb();

        var huge = await Handler(db).Handle(
            new ListAuditEntriesQuery(TestPrincipals.Administrator(), PageSize: 10_000), CancellationToken.None);
        var none = await Handler(db).Handle(
            new ListAuditEntriesQuery(TestPrincipals.Administrator(), Page: 0, PageSize: 0), CancellationToken.None);

        huge.PageSize.Should().Be(100);
        none.Page.Should().Be(1);
        none.PageSize.Should().Be(1);
    }

    private static ListAuditEntriesQueryHandler Handler(ApplicationDbContext db)
        => new(db, FakeUserDirectory.Empty, new FixedClock(Now));

    private static void Add(
        ApplicationDbContext db,
        DateTime occurredAt,
        string action,
        string? because = null,
        string? userId = null,
        string? display = null,
        int? institutionId = null)
    {
        _ = because;
        db.Set<AuditEntry>().Add(AuditEntry.Create(
            occurredAt,
            AuditCategory.Command,
            action,
            success: true,
            actorUserId: userId,
            actorDisplay: display,
            institutionId: institutionId));
    }

    private static ApplicationDbContext CreateDb()
    {
        var db = new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        db.Set<Institution>().Add(new Institution { Id = Kgk, Name = "Kgosi Kgari Teaching Hospital", ShortCode = "KGK" });
        db.Set<Institution>().Add(new Institution { Id = Demo, Name = "Demo Institution", ShortCode = "DEMO" });
        db.SaveChanges();
        return db;
    }

    private sealed class FixedClock(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow, TimeSpan.Zero);
    }
}
