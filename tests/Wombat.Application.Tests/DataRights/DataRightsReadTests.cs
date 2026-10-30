using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.DataRights;
using Wombat.Application.Features.DataRights.Queries;
using Wombat.Domain.DataRights;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.DataRights;

/// <summary>
/// The three data-rights reads as T387 fills them (lane K; build-contracts § 2): the live name and address, the erased
/// requester's pseudonym and day, the institution, the decider and the recorders by name, the corrections as typed, the
/// programme-start record and the institutional sign-in. <see cref="DataRightsQueryHandlerTests" /> keeps the scope rules.
/// </summary>
public sealed class DataRightsReadTests
{
    private const string RequesterId = "nomsa";
    private const string PieterId = "pieter";

    /// <summary>
    /// T387 (F1, M1, S1): the request page names the requester live, so a name corrected on My account shows at once,
    /// with the live address; until T387 it printed the name stored at submission (the sign-in claim, an address).
    /// </summary>
    [Fact]
    public async Task GetById_NamesTheRequesterLive_AfterANameChange()
    {
        await using var db = CreateDb();
        AddInstitutions(db);
        var entity = DataRightsRequest.Create(
            RequesterId, "Nomsa Mahlngu", DataRightsRequestType.Rectification, "My surname", DateTime.UtcNow, institutionId: 1);
        db.Set<DataRightsRequest>().Add(entity);
        await db.SaveChangesAsync();

        var users = new FakeUserDirectory().With(Account(RequesterId, "Nomsa", "Mahlangu", "mahlangu@kgk.wombat.local"));
        var result = await new GetDataRightsRequestByIdQueryHandler(db, WithSignIns(users, null)).Handle(
            new GetDataRightsRequestByIdQuery(entity.Id, Coordinator(institutionId: 1)), CancellationToken.None);

        result!.RequesterName.Should().Be("Nomsa Mahlangu");
        result.RequesterEmail.Should().Be("mahlangu@kgk.wombat.local");
        result.ErasedOn.Should().BeNull();
        result.InstitutionName.Should().Be("Kgosi Kgari Teaching Hospital");
        result.DecidedByName.Should().BeNull("nobody has decided it");
    }

    /// <summary>
    /// T387 (N5, N13, K01): once erased the requester is the pseudonym, with no address, and the erasure's South African
    /// day: an erasure at 22:30 UTC on 27 October is 28 October in South Africa. The directory is never asked for them.
    /// </summary>
    [Fact]
    public async Task GetById_AnErasedRequester_IsThePseudonymWithTheErasureDay_AndNoAddress()
    {
        await using var db = CreateDb();
        var erasedAt = new DateTime(2026, 10, 27, 22, 30, 0, DateTimeKind.Utc);
        var entity = DataRightsRequest.Create(
            "sipho", "deleted_user_0ae1d64b", DataRightsRequestType.Erasure, "Leaving", erasedAt.AddHours(-1), institutionId: 1);
        entity.Approve("admin-1", "Approved.", erasedAt);
        db.Set<DataRightsRequest>().Add(entity);
        db.Set<DataRightsErasureRecord>().Add(
            DataRightsErasureRecord.Create(entity.Id, "sipho", "deleted_user_0ae1d64b", erasedAt, "[]"));
        await db.SaveChangesAsync();

        var users = new Mock<IUserAdministrationService>(MockBehavior.Strict);
        users.Setup(u => u.GetDisplayNamesAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, string> { ["admin-1"] = "Dev Admin" });

        var result = await new GetDataRightsRequestByIdQueryHandler(db, users.Object).Handle(
            new GetDataRightsRequestByIdQuery(entity.Id, Administrator()), CancellationToken.None);

        result!.RequesterName.Should().Be("deleted_user_0ae1d64b");
        result.RequesterEmail.Should().BeNull();
        result.ErasedOn.Should().Be(new DateOnly(2026, 10, 28));
        result.DecidedByName.Should().Be("Dev Admin");
        users.Verify(u => u.GetContactsAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// T387 (S11, F3): the corrections in the order recorded, each value read back through the parser (Postgres re-renders
    /// jsonb, so the stored text need not be the serializer's: here "ü" unescaped, as Postgres renders it, where the
    /// serializer writes ü), each recorder by name, and an erased decider or recorder by the pseudonym the erasure wrote
    /// over their id. The decider and the recorders are named in one directory read.
    /// </summary>
    [Fact]
    public async Task GetById_ReadsTheCorrectionsInOrder_TheValuesAsTyped_AndEachRecorderByName()
    {
        await using var db = CreateDb();
        var decidedAt = new DateTime(2026, 10, 28, 8, 0, 0, DateTimeKind.Utc);
        var entity = DataRightsRequest.Create(
            RequesterId, "Nomsa Mahlangu", DataRightsRequestType.Rectification, "My surname", decidedAt.AddHours(-1), institutionId: 1);
        entity.Approve("deleted_user_11aa22bb", "Approved.", decidedAt);
        db.Set<DataRightsRequest>().Add(entity);
        db.Set<DataRightsErasureRecord>().Add(DataRightsErasureRecord.Create(
            Guid.NewGuid(), "former-coordinator", "deleted_user_11aa22bb", decidedAt.AddDays(1), "[]"));
        var second = DataRightsRectification.Create(
            entity.Id, "Other:Home address", "\"1 Old Road\"", "\"2 New Road\"", PieterId, decidedAt.AddMinutes(20));
        var first = DataRightsRectification.Create(
            entity.Id, DataRightsRecords.AccountLastName, "\"Müller\"", "\"Mahlangu\"", "deleted_user_11aa22bb", decidedAt.AddMinutes(10));
        db.Set<DataRightsRectification>().AddRange(second, first);
        await db.SaveChangesAsync();

        var users = new FakeUserDirectory((PieterId, "Pieter Smit"))
            .With(Account(RequesterId, "Nomsa", "Mahlangu", "mahlangu@kgk.wombat.local"));
        var result = await new GetDataRightsRequestByIdQueryHandler(db, WithSignIns(users, null)).Handle(
            new GetDataRightsRequestByIdQuery(entity.Id, Administrator()), CancellationToken.None);

        result!.DecidedByName.Should().Be("deleted_user_11aa22bb", "the decider was erased after deciding");
        result.Corrections.Select(c => c.TargetType).Should().Equal(DataRightsRecords.AccountLastName, "Other:Home address");
        result.Corrections[0].FromValue.Should().Be("Müller");
        result.Corrections[0].ToValue.Should().Be("Mahlangu");
        result.Corrections[0].AppliedByName.Should().Be("deleted_user_11aa22bb");
        result.Corrections[1].AppliedByName.Should().Be("Pieter Smit");
        result.Corrections[1].FromValue.Should().Be("1 Old Road");
        users.Lookups.Should().ContainSingle("the decider and the recorders are named in one read")
            .Which.Should().Equal(PieterId);
    }

    /// <summary>
    /// T387 (S2, C02, C07): the programme-start record is the preferred profile, which the Administrator and an
    /// Institutional admin of its institution may open, and a Coordinator may not (R3-C02's "you cannot").
    /// </summary>
    [Theory]
    [InlineData("administrator", true)]
    [InlineData("coordinator", false)]
    [InlineData("coordinator-and-institutional-admin-here", true)]
    public async Task GetById_TheProgrammeStartRecord_OpensForTheAdministratorOrAnInstitutionalAdminThere(string caller, bool mayOpen)
    {
        await using var db = CreateDb();
        var entity = AddRectification(db);
        db.Set<TraineeProfile>().Add(Profile(4, active: false));
        db.Set<TraineeProfile>().Add(Profile(5, active: true));
        await db.SaveChangesAsync();

        var principal = caller switch
        {
            "administrator" => Administrator(),
            "coordinator" => Coordinator(institutionId: 1),
            _ => Build("ia-1", 1, [WombatRoles.Coordinator, WombatRoles.InstitutionalAdmin])
        };

        var result = await new GetDataRightsRequestByIdQueryHandler(db, WithSignIns(FakeUserDirectory.Empty, null)).Handle(
            new GetDataRightsRequestByIdQuery(entity.Id, principal), CancellationToken.None);

        result!.ProgrammeStart.Should().Be(new ProgrammeStartRecordDto(5, Ended: false, MayOpen: mayOpen),
            "the active profile is the preferred one (TraineeScopeResolver.PreferredProfiles)");
    }

    /// <summary>
    /// T387 (S2, C02): an Institutional admin of another institution than the profile's may not open it. The rule itself,
    /// since such a caller reads the request only with a Coordinator's scope there.
    /// </summary>
    [Fact]
    public async Task GetById_TheProgrammeStartRecord_DoesNotOpenForAnInstitutionalAdminElsewhere()
    {
        await using var db = CreateDb();
        var entity = DataRightsRequest.Create(
            RequesterId, "Nomsa Mahlangu", DataRightsRequestType.Rectification, "My surname", DateTime.UtcNow, institutionId: 2);
        db.Set<DataRightsRequest>().Add(entity);
        db.Set<TraineeProfile>().Add(Profile(5, active: true));
        await db.SaveChangesAsync();

        var result = await new GetDataRightsRequestByIdQueryHandler(db, WithSignIns(FakeUserDirectory.Empty, null)).Handle(
            new GetDataRightsRequestByIdQuery(entity.Id, Build("ia-2", 2, [WombatRoles.Coordinator, WombatRoles.InstitutionalAdmin])),
            CancellationToken.None);

        result!.ProgrammeStart!.MayOpen.Should().BeFalse("the profile is at institution 1, the caller's institution is 2");
    }

    /// <summary>T387 (S2, C08): an ended programme's record reads as ended (flow 13's archived line); with no profile, none.</summary>
    [Fact]
    public async Task GetById_TheProgrammeStartRecord_IsEndedForAnEndedProgramme_AndAbsentWithNoProfile()
    {
        await using var db = CreateDb();
        var entity = AddRectification(db);
        await db.SaveChangesAsync();
        var handler = new GetDataRightsRequestByIdQueryHandler(db, WithSignIns(FakeUserDirectory.Empty, null));

        (await handler.Handle(new GetDataRightsRequestByIdQuery(entity.Id, Administrator()), CancellationToken.None))!
            .ProgrammeStart.Should().BeNull("the requester holds no trainee profile, so the radio is not offered");

        var ended = Profile(7, active: true);
        ended.Deactivate(new DateOnly(2026, 10, 20), new DateOnly(2026, 10, 28));
        db.Set<TraineeProfile>().Add(ended);
        await db.SaveChangesAsync();

        (await handler.Handle(new GetDataRightsRequestByIdQuery(entity.Id, Administrator()), CancellationToken.None))!
            .ProgrammeStart.Should().Be(new ProgrammeStartRecordDto(7, Ended: true, MayOpen: true));
    }

    /// <summary>
    /// T387 (F6, S7, C11): a requester who signs in through an institution has their name set by it; the page names the
    /// first institutional sign-in. Read only for a Rectification, the one request with a Correction section.
    /// </summary>
    [Fact]
    public async Task GetById_NameFromSignIn_IsTheFirstInstitutionalSignIn_ForARectificationOnly()
    {
        await using var db = CreateDb();
        var rectification = AddRectification(db);
        var access = DataRightsRequest.Create(RequesterId, "Nomsa Mahlangu", DataRightsRequestType.Access, "Mine", DateTime.UtcNow, 1);
        db.Set<DataRightsRequest>().Add(access);
        await db.SaveChangesAsync();

        var signedInByInstitution = new AccountSignInMethods(
            false, [new InstitutionalSignIn("kgk-oidc", "Kgosi Kgari Teaching Hospital"), new InstitutionalSignIn("z", "Zeta")]);
        var handler = new GetDataRightsRequestByIdQueryHandler(db, WithSignIns(FakeUserDirectory.Empty, signedInByInstitution));

        (await handler.Handle(new GetDataRightsRequestByIdQuery(rectification.Id, Administrator()), CancellationToken.None))!
            .NameFromSignIn.Should().Be("Kgosi Kgari Teaching Hospital");
        (await handler.Handle(new GetDataRightsRequestByIdQuery(access.Id, Administrator()), CancellationToken.None))!
            .NameFromSignIn.Should().BeNull("an Access request has no Correction section");

        var withPasswordOnly = new GetDataRightsRequestByIdQueryHandler(
            db, WithSignIns(FakeUserDirectory.Empty, new AccountSignInMethods(true, [])));
        (await withPasswordOnly.Handle(new GetDataRightsRequestByIdQuery(rectification.Id, Administrator()), CancellationToken.None))!
            .NameFromSignIn.Should().BeNull();
    }

    /// <summary>
    /// T387 (F1, S1, N13, Q23): each row names the requester live with the address, or by the pseudonym with the erasure's
    /// day and no address; the Administrator's rows carry the institution ("None" is the page's); the page's living people
    /// are read in one directory call, the erased never.
    /// </summary>
    [Fact]
    public async Task ListRequests_NamesEachRequesterLive_OrByThePseudonym_InOneRead()
    {
        await using var db = CreateDb();
        AddInstitutions(db);
        var now = new DateTime(2026, 10, 28, 6, 0, 0, DateTimeKind.Utc);
        db.Set<DataRightsRequest>().Add(DataRightsRequest.Create("anele", "anele@kgk.wombat.local", DataRightsRequestType.Export, "a", now, 1));
        db.Set<DataRightsRequest>().Add(DataRightsRequest.Create("thabo", "Thabo Old", DataRightsRequestType.Access, "b", now.AddMinutes(-1), 2));
        db.Set<DataRightsRequest>().Add(DataRightsRequest.Create("sipho", "deleted_user_0ae1d64b", DataRightsRequestType.Erasure, "c", now.AddMinutes(-2), 1));
        db.Set<DataRightsRequest>().Add(DataRightsRequest.Create("devadmin", "Dev Admin", DataRightsRequestType.Access, "d", now.AddMinutes(-3)));
        db.Set<DataRightsErasureRecord>().Add(DataRightsErasureRecord.Create(Guid.NewGuid(), "sipho", "deleted_user_0ae1d64b", now, "[]"));
        await db.SaveChangesAsync();

        var users = new Mock<IUserAdministrationService>(MockBehavior.Strict);
        users.Setup(u => u.GetContactsAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, UserContact>
            {
                ["anele"] = new("anele", "Anele", "Dlamini", "anele@kgk.wombat.local"),
                ["thabo"] = new("thabo", "Thabo", "Nkosi", "thabo@urh.wombat.local"),
                ["devadmin"] = new("devadmin", "Dev", "Admin", "devadmin@wombat.local"),
            });

        var result = await new ListDataRightsRequestsQueryHandler(db, users.Object).Handle(
            new ListDataRightsRequestsQuery(null, null, null, Administrator()), CancellationToken.None);

        result.Items.Select(i => (i.RequesterName, i.RequesterEmail, i.ErasedOn, i.InstitutionName)).Should().Equal(
            ("Anele Dlamini", "anele@kgk.wombat.local", (DateOnly?)null, "Kgosi Kgari Teaching Hospital"),
            ("Thabo Nkosi", "thabo@urh.wombat.local", (DateOnly?)null, "Umlazi Regional Hospital"),
            ("deleted_user_0ae1d64b", (string?)null, new DateOnly(2026, 10, 28), "Kgosi Kgari Teaching Hospital"),
            ("Dev Admin", "devadmin@wombat.local", (DateOnly?)null, (string?)null));
        result.EveryInstitution.Should().BeTrue();
        result.InstitutionName.Should().BeNull();
        users.Verify(u => u.GetContactsAsync(
            It.Is<IReadOnlyCollection<string>>(ids => !ids.Contains("sipho") && ids.Count == 3), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// T387 (Q23, S4): a Coordinator's rows carry no institution (they see one), and the result names theirs; a Coordinator
    /// set to no institution has none named, and sees nothing.
    /// </summary>
    [Fact]
    public async Task ListRequests_AScopedCoordinatorsResultNamesTheirInstitution_AndNoRowCarriesOne()
    {
        await using var db = CreateDb();
        AddInstitutions(db);
        db.Set<DataRightsRequest>().Add(DataRightsRequest.Create("anele", "Anele Dlamini", DataRightsRequestType.Export, "a", DateTime.UtcNow, 1));
        await db.SaveChangesAsync();
        var users = new FakeUserDirectory().With(Account("anele", "Anele", "Dlamini", "anele@kgk.wombat.local"));
        var handler = new ListDataRightsRequestsQueryHandler(db, users);

        var scoped = await handler.Handle(new ListDataRightsRequestsQuery(null, null, null, Coordinator(1)), CancellationToken.None);
        var nowhere = await handler.Handle(new ListDataRightsRequestsQuery(null, null, null, Coordinator(null)), CancellationToken.None);

        scoped.EveryInstitution.Should().BeFalse();
        scoped.InstitutionName.Should().Be("Kgosi Kgari Teaching Hospital");
        scoped.Items.Should().ContainSingle().Which.InstitutionName.Should().BeNull();
        scoped.Items[0].RequesterEmail.Should().Be("anele@kgk.wombat.local");
        nowhere.InstitutionName.Should().BeNull();
        nowhere.Items.Should().BeEmpty();
    }

    /// <summary>
    /// T387 (S3, S10, M09, M18): My data rights names the caller's institution, or none, and each Rectification's corrected
    /// records, each once, in the order first recorded.
    /// </summary>
    [Fact]
    public async Task GetMyRequests_NamesTheInstitution_AndEachRectificationsCorrectedRecords()
    {
        await using var db = CreateDb();
        AddInstitutions(db);
        var now = new DateTime(2026, 10, 28, 6, 0, 0, DateTimeKind.Utc);
        var rectification = DataRightsRequest.Create(RequesterId, "Nomsa Mahlangu", DataRightsRequestType.Rectification, "a", now, 1);
        var access = DataRightsRequest.Create(RequesterId, "Nomsa Mahlangu", DataRightsRequestType.Access, "b", now.AddMinutes(-5), 1);
        db.Set<DataRightsRequest>().AddRange(rectification, access);
        db.Set<DataRightsRectification>().AddRange(
            DataRightsRectification.Create(rectification.Id, DataRightsRecords.ProgrammeStart, "\"a\"", "\"b\"", PieterId, now.AddMinutes(30)),
            DataRightsRectification.Create(rectification.Id, DataRightsRecords.AccountLastName, "\"a\"", "\"b\"", PieterId, now.AddMinutes(10)),
            DataRightsRectification.Create(rectification.Id, DataRightsRecords.AccountLastName, "\"b\"", "\"c\"", PieterId, now.AddMinutes(40)));
        await db.SaveChangesAsync();
        var handler = new GetMyDataRightsRequestsQueryHandler(db);

        var mine = await handler.Handle(new GetMyDataRightsRequestsQuery(Build(RequesterId, 1, [WombatRoles.Trainee])), CancellationToken.None);
        var nowhere = await handler.Handle(new GetMyDataRightsRequestsQuery(Build(RequesterId, null, [WombatRoles.Trainee])), CancellationToken.None);

        mine.InstitutionName.Should().Be("Kgosi Kgari Teaching Hospital");
        mine.Requests.Select(r => r.Id).Should().Equal(rectification.Id, access.Id);
        mine.Requests[0].CorrectedRecords.Should().Equal(DataRightsRecords.AccountLastName, DataRightsRecords.ProgrammeStart);
        mine.Requests[1].CorrectedRecords.Should().BeEmpty();
        nowhere.InstitutionName.Should().BeNull();
    }

    private static ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static void AddInstitutions(ApplicationDbContext db)
    {
        db.Set<Institution>().Add(new Institution { Id = 1, Name = "Kgosi Kgari Teaching Hospital", ShortCode = "KGK" });
        db.Set<Institution>().Add(new Institution { Id = 2, Name = "Umlazi Regional Hospital", ShortCode = "URH" });
    }

    private static DataRightsRequest AddRectification(ApplicationDbContext db)
    {
        var entity = DataRightsRequest.Create(
            RequesterId, "Nomsa Mahlangu", DataRightsRequestType.Rectification, "My surname", DateTime.UtcNow, institutionId: 1);
        db.Set<DataRightsRequest>().Add(entity);
        return entity;
    }

    private static TraineeProfile Profile(int id, bool active) => new()
    {
        Id = id,
        UserId = RequesterId,
        InstitutionId = 1,
        CurriculumId = 1,
        ProgrammeStartDate = new DateOnly(2026, 1, 12),
        ExpectedCompletionDate = new DateOnly(2030, 1, 12),
        IsActive = active
    };

    private static UserIdentityDetails Account(string userId, string first, string last, string email, int? institutionId = 1)
        => new(userId, email, first, last, institutionId, [], [], []);

    /// <summary>
    /// <paramref name="inner" /> with its sign-in methods answered: <see cref="FakeUserDirectory" /> leaves them to the
    /// interface's default, which throws, as a directory that never shows My account may.
    /// </summary>
    private static IUserAdministrationService WithSignIns(FakeUserDirectory inner, AccountSignInMethods? methods)
    {
        var users = new Mock<IUserAdministrationService>(MockBehavior.Strict);
        users.Setup(u => u.GetContactsAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .Returns((IReadOnlyCollection<string> ids, CancellationToken token) => inner.GetContactsAsync(ids, token));
        users.Setup(u => u.GetDisplayNamesAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .Returns((IReadOnlyCollection<string> ids, CancellationToken token) => inner.GetDisplayNamesAsync(ids, token));
        users.Setup(u => u.GetSignInMethodsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(methods);
        return users.Object;
    }

    private static ClaimsPrincipal Administrator() => Build("admin-1", null, [WombatRoles.Administrator]);

    private static ClaimsPrincipal Coordinator(int? institutionId) => Build("coord-1", institutionId, [WombatRoles.Coordinator]);

    private static ClaimsPrincipal Build(string userId, int? institutionId, string[] roles)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
        if (institutionId.HasValue)
        {
            claims.Add(new Claim(WombatClaimTypes.InstitutionId, institutionId.Value.ToString()));
        }

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }
}
