using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.MultiSourceFeedback;

/// <summary>
/// The MSF commands, the aggregate report and the coordinator's list answer only the people who run campaigns for the
/// campaign's subject: a Coordinator at the institution the subject trains at, or an Administrator; never the subject
/// themselves, and nobody who holds Trainee. (T113, T224)
/// </summary>
/// <remarks>
/// <para>
/// Before T113 open, close, withdraw and add-invitation took a campaign id and nothing else, the report likewise, and
/// the list took nothing at all. The only gate was the page's role check, so a Coordinator at any hospital could open,
/// close, invite to or read the report of any other hospital's campaign by route id, and saw every campaign in the
/// country on the list.
/// </para>
/// <para>
/// A refused command must leave the store exactly as it was even after a save, because the audit pipeline saves the
/// request's DbContext from its catch: a mutation staged before the check would be committed by the refusal itself.
/// Every refusal below is followed by that save and read back through a second context.
/// </para>
/// </remarks>
public sealed class MsfCampaignScopeTests
{
    private const int HostInstitution = 1;
    private const int OtherInstitution = 2;
    private const string SubjectUserId = "trainee-1";

    private readonly string _databaseName = Guid.NewGuid().ToString();
    private readonly CapturingEmailSender _emailSender = new();

    // ─── The commands ────────────────────────────────────────────────────────

    public static TheoryData<string> Commands => new() { "Open", "Close", "Withdraw", "AddInvitation", "RemoveInvitation", "Resend" };

    [Theory]
    [MemberData(nameof(Commands))]
    public async Task ACoordinatorFromAnotherInstitution_IsRefused_AndNothingChanges(string command)
    {
        await using var db = CreateDb();
        var campaignId = await SeedCampaignAsync(db, StateFor(command));
        var before = await SnapshotAsync(campaignId);

        var act = () => RunAsync(command, db, campaignId, TestPrincipals.Coordinator(OtherInstitution));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        await db.SaveChangesAsync();
        (await SnapshotAsync(campaignId)).Should().BeEquivalentTo(before);
        _emailSender.Sent.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(Commands))]
    public async Task AClassmateOfTheSubject_IsRefused_ThoughTheirInstitutionMatches(string command)
    {
        // Every user carries an institution claim. Running campaigns takes the Coordinator role as well.
        await using var db = CreateDb();
        var campaignId = await SeedCampaignAsync(db, StateFor(command));
        var before = await SnapshotAsync(campaignId);

        var act = () => RunAsync(command, db, campaignId, TestPrincipals.Trainee("trainee-2", HostInstitution));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        await db.SaveChangesAsync();
        (await SnapshotAsync(campaignId)).Should().BeEquivalentTo(before);
    }

    [Theory]
    [MemberData(nameof(Commands))]
    public async Task TheSubjectsOwnCoordinator_IsNotRefused(string command)
    {
        await using var db = CreateDb();
        var campaignId = await SeedCampaignAsync(db, StateFor(command));
        var before = await SnapshotAsync(campaignId);

        await RunAsync(command, db, campaignId, TestPrincipals.Coordinator(HostInstitution));

        (await SnapshotAsync(campaignId)).Should().NotBeEquivalentTo(before, "the command did its work");
    }

    [Theory]
    [MemberData(nameof(Commands))]
    public async Task AnAdministrator_IsNotRefused(string command)
    {
        await using var db = CreateDb();
        var campaignId = await SeedCampaignAsync(db, StateFor(command));

        var act = () => RunAsync(command, db, campaignId, TestPrincipals.Administrator());

        await act.Should().NotThrowAsync();
    }

    /// <summary>
    /// The campaign editor prints a command's refusal, so "could not be found" for a missing id beside a scope refusal
    /// for a real one let a Coordinator walk the ids and learn which campaigns other institutions run. The report
    /// already returns null for both; the commands now refuse both alike. (T113 review)
    /// </summary>
    [Theory]
    [MemberData(nameof(CampaignIdCommands))]
    public async Task ACampaignIdThatNamesNothing_IsRefusedExactlyAsAnotherInstitutionsCampaignIs(string command)
    {
        await using var db = CreateDb();
        var campaignId = await SeedCampaignAsync(db, StateFor(command));
        var outsider = TestPrincipals.Coordinator(OtherInstitution);

        var outOfScope = await RefusalAsync(() => RunAsync(command, db, campaignId, outsider));
        var missing = await RefusalAsync(() => RunAsync(command, db, campaignId + 1000, outsider));

        outOfScope.Should().BeOfType<UnauthorizedAccessException>();
        missing.Should().BeOfType(outOfScope.GetType());
        missing.Message.Should().Be(outOfScope.Message);

        // And the subject's own coordinator, asking for an id that names nothing, is told nothing more than that.
        var ownMissing = await RefusalAsync(
            () => RunAsync(command, db, campaignId + 1000, TestPrincipals.Coordinator(HostInstitution)));
        ownMissing.Should().BeOfType<UnauthorizedAccessException>().Which.Message.Should().Be(outOfScope.Message);
    }

    [Theory]
    [MemberData(nameof(CampaignIdCommands))]
    public async Task AnAdministrator_IsToldPlainlyThatACampaignDoesNotExist(string command)
    {
        // An Administrator runs every campaign, so there is no census to deny them.
        await using var db = CreateDb();
        var campaignId = await SeedCampaignAsync(db, StateFor(command));

        var missing = await RefusalAsync(() => RunAsync(command, db, campaignId + 1000, TestPrincipals.Administrator()));

        missing.Should().BeOfType<InvalidOperationException>().Which.Message.Should().Be("The MSF campaign could not be found.");
    }

    [Fact]
    public async Task TheCommands_FollowTheSharedTieBreak_ForATraineeWithTwoPastProfiles()
    {
        // A trainee with two past profiles and no current one resolves, for MSF, where their activities are stamped:
        // the highest-id profile. The MSF copy of the resolver used to pick the later programme start, i.e. the OTHER
        // institution.
        await using var db = CreateDb();
        var campaignId = await SeedCampaignAsync(db, MsfCampaignState.Open, secondProfileAt: OtherInstitution);

        var host = () => RunAsync("Close", db, campaignId, TestPrincipals.Coordinator(HostInstitution));
        await host.Should().ThrowAsync<UnauthorizedAccessException>();

        await RunAsync("Close", db, campaignId, TestPrincipals.Coordinator(OtherInstitution));
        (await SnapshotAsync(campaignId)).State.Should().Be(MsfCampaignState.UnderReview);
    }

    // ─── The report ──────────────────────────────────────────────────────────

    [Fact]
    public async Task TheReport_IsNullForACoordinatorFromAnotherInstitution_AsForACampaignThatDoesNotExist()
    {
        await using var db = CreateDb();
        var campaignId = await SeedCampaignAsync(db, MsfCampaignState.UnderReview);

        (await ReportAsync(db, campaignId, TestPrincipals.Coordinator(OtherInstitution))).Should().BeNull();
        (await ReportAsync(db, campaignId + 1000, TestPrincipals.Coordinator(HostInstitution))).Should().BeNull();
    }

    [Fact]
    public async Task TheReport_IsReadByTheSubjectsOwnCoordinator_AndAnAdministrator_BeforeRelease()
    {
        await using var db = CreateDb();
        var campaignId = await SeedCampaignAsync(db, MsfCampaignState.UnderReview);

        (await ReportAsync(db, campaignId, TestPrincipals.Coordinator(HostInstitution)))!.SubjectUserId.Should().Be(SubjectUserId);
        (await ReportAsync(db, campaignId, TestPrincipals.Administrator())).Should().NotBeNull();
    }

    [Fact]
    public async Task TheSubject_ReadsTheirReportOnlyOnceItIsReleased_WhateverElseTheyHold()
    {
        await using var db = CreateDb();
        var campaignId = await SeedCampaignAsync(db, MsfCampaignState.UnderReview);

        var subject = TestPrincipals.Trainee(SubjectUserId, HostInstitution);
        var subjectWhoCoordinates = TestPrincipals.InRole(WombatRoles.Coordinator, SubjectUserId, HostInstitution);

        (await ReportAsync(db, campaignId, subject)).Should().BeNull("the working copy is not the trainee's");
        (await ReportAsync(db, campaignId, subjectWhoCoordinates)).Should().BeNull(
            "the subject arm comes first, so a second role does not reach the working copy of one's own feedback");

        await ReleaseAsync(campaignId);

        // A fresh context, as a new request would have: the seeding one still tracks the pre-release campaign.
        await using var afterRelease = CreateDb();
        (await ReportAsync(afterRelease, campaignId, subject))!.State.Should().Be(MsfCampaignState.Released);
    }

    [Fact]
    public async Task AClassmate_NeverReadsTheReport_EvenReleased()
    {
        await using var db = CreateDb();
        var campaignId = await SeedCampaignAsync(db, MsfCampaignState.Released);

        (await ReportAsync(db, campaignId, TestPrincipals.Trainee("trainee-2", HostInstitution))).Should().BeNull();
    }

    // ─── The coordinator's list ──────────────────────────────────────────────

    [Fact]
    public async Task TheCoordinatorList_HoldsOnlyTheCampaignsAboutTheirOwnInstitutionsTrainees()
    {
        await using var db = CreateDb();
        var hostCampaign = await SeedCampaignAsync(db, MsfCampaignState.Draft);
        var otherCampaign = await SeedCampaignAsync(db, MsfCampaignState.Draft, subjectUserId: "trainee-9", subjectInstitution: OtherInstitution);
        var unadmittedCampaign = await SeedCampaignAsync(db, MsfCampaignState.Draft, subjectUserId: "no-profile", subjectInstitution: null);

        (await ListAsync(db, TestPrincipals.Coordinator(HostInstitution))).Should().Equal(hostCampaign);
        (await ListAsync(db, TestPrincipals.Coordinator(OtherInstitution))).Should().Equal(otherCampaign);
        (await ListAsync(db, TestPrincipals.Administrator()))
            .Should().BeEquivalentTo([hostCampaign, otherCampaign, unadmittedCampaign]);
    }

    [Fact]
    public async Task TheCoordinatorList_IsEmptyForAnyoneWhoDoesNotRunCampaigns()
    {
        await using var db = CreateDb();
        await SeedCampaignAsync(db, MsfCampaignState.Draft);

        (await ListAsync(db, TestPrincipals.Trainee(SubjectUserId, HostInstitution))).Should().BeEmpty();
        (await ListAsync(db, TestPrincipals.InstitutionalAdmin(HostInstitution))).Should().BeEmpty();
        (await ListAsync(db, TestPrincipals.InRole(WombatRoles.Coordinator, "coordinator-nowhere", institutionId: null)))
            .Should().BeEmpty();
    }

    [Fact]
    public async Task TheCoordinatorList_AndTheSingleCampaignCheck_AgreeAboutATraineeWithTwoProfiles()
    {
        await using var db = CreateDb();
        var campaignId = await SeedCampaignAsync(db, MsfCampaignState.UnderReview, secondProfileAt: OtherInstitution);

        (await ListAsync(db, TestPrincipals.Coordinator(OtherInstitution))).Should().Equal(campaignId);
        (await ReportAsync(db, campaignId, TestPrincipals.Coordinator(OtherInstitution))).Should().NotBeNull();

        (await ListAsync(db, TestPrincipals.Coordinator(HostInstitution))).Should().BeEmpty();
        (await ReportAsync(db, campaignId, TestPrincipals.Coordinator(HostInstitution))).Should().BeNull();
    }

    // ─── The subject, and anyone who holds Trainee (T224) ────────────────────
    //
    // Whoever runs a campaign adds its invitees, opens it, watches each group's responses come in before release, closes
    // it and writes the narrative its trainee is released. Until T224 only the report refused the trainee it is about;
    // a subject who also coordinated, or administered, ran their own campaign, and a registrar who coordinated ran their
    // peers'. Each role is given alone first, so that each exclusion is shown to hold by itself.

    /// <summary>The trainee the campaign is about, holding a role that would otherwise run it.</summary>
    public static TheoryData<string> SubjectCallers => new() { "Coordinator", "Administrator", "Coordinator+Trainee" };

    private static ClaimsPrincipal SubjectAs(string roles) => roles switch
    {
        "Coordinator" => TestPrincipals.Coordinator(HostInstitution, userId: SubjectUserId),
        "Administrator" => TestPrincipals.Administrator(userId: SubjectUserId),
        "Coordinator+Trainee" => TestPrincipals.InRoles([WombatRoles.Coordinator, WombatRoles.Trainee], SubjectUserId, HostInstitution),
        _ => throw new ArgumentOutOfRangeException(nameof(roles), roles, null)
    };

    /// <summary>A classmate of the subject who also holds a role that would otherwise run the campaign.</summary>
    private static ClaimsPrincipal PeerWhoAlsoHolds(string role)
        => TestPrincipals.InRoles([role, WombatRoles.Trainee], "trainee-2", role == WombatRoles.Administrator ? null : HostInstitution);

    public static TheoryData<string, string> CampaignIdCommandsBySubjectCaller()
    {
        var data = new TheoryData<string, string>();
        foreach (var command in new[] { "Open", "Close", "Withdraw", "AddInvitation", "RemoveInvitation", "Release" })
        {
            foreach (var roles in new[] { "Coordinator", "Administrator", "Coordinator+Trainee" })
            {
                data.Add(command, roles);
            }
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(CampaignIdCommandsBySubjectCaller))]
    public async Task TheSubject_IsRefusedEveryCommandOnTheirOwnCampaign_WhateverRoleTheyHold_AndNothingChanges(
        string command, string roles)
    {
        await using var db = CreateDb();
        var campaignId = await SeedCampaignAsync(db, StateFor(command));
        var before = await SnapshotAsync(campaignId);
        var outsiders = await RefusalAsync(() => RunAsync(command, db, campaignId, TestPrincipals.Coordinator(OtherInstitution)));

        var refusal = await RefusalAsync(() => RunAsync(command, db, campaignId, SubjectAs(roles)));

        // The refusal of a campaign the caller does not run, word for word: it says nothing the subject did not know.
        refusal.Should().BeOfType<UnauthorizedAccessException>().Which.Message.Should().Be(outsiders.Message);
        await db.SaveChangesAsync();
        (await SnapshotAsync(campaignId)).Should().BeEquivalentTo(before);
        _emailSender.Sent.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(CampaignIdCommands))]
    public async Task ACoordinatorWhoHoldsTrainee_IsRefusedEveryCommandOnAPeersCampaign_AndNothingChanges(string command)
    {
        await using var db = CreateDb();
        var campaignId = await SeedCampaignAsync(db, StateFor(command));
        var before = await SnapshotAsync(campaignId);

        var act = () => RunAsync(command, db, campaignId, PeerWhoAlsoHolds(WombatRoles.Coordinator));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        await db.SaveChangesAsync();
        (await SnapshotAsync(campaignId)).Should().BeEquivalentTo(before);
        _emailSender.Sent.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(CampaignIdCommands))]
    public async Task AnAdministratorWhoHoldsTrainee_IsRefusedEveryCommand_AndAMissingIdTheSame(string command)
    {
        // They run no campaign, so, like a Coordinator, they are not told which ids name one.
        await using var db = CreateDb();
        var campaignId = await SeedCampaignAsync(db, StateFor(command));
        var before = await SnapshotAsync(campaignId);
        var caller = PeerWhoAlsoHolds(WombatRoles.Administrator);

        var existing = await RefusalAsync(() => RunAsync(command, db, campaignId, caller));
        var missing = await RefusalAsync(() => RunAsync(command, db, campaignId + 1000, caller));

        existing.Should().BeOfType<UnauthorizedAccessException>();
        missing.Should().BeOfType<UnauthorizedAccessException>().Which.Message.Should().Be(existing.Message);
        await db.SaveChangesAsync();
        (await SnapshotAsync(campaignId)).Should().BeEquivalentTo(before);
    }

    [Theory]
    [MemberData(nameof(SubjectCallers))]
    public async Task TheSubject_CannotCreateACampaignAboutThemselves_AndNothingIsStored(string roles)
    {
        await using var db = CreateDb();
        var seeded = await SeedCampaignAsync(db, MsfCampaignState.Released);
        var templateId = await TemplateOfAsync(seeded);

        var act = () => CreateAsync(db, templateId, SubjectUserId, SubjectAs(roles));

        (await act.Should().ThrowAsync<UnauthorizedAccessException>())
            .Which.Message.Should().StartWith("A multi-source feedback campaign cannot be run by the trainee it is about.");
        await db.SaveChangesAsync();
        (await CampaignIdsAsync()).Should().Equal(seeded);
    }

    [Theory]
    [InlineData(WombatRoles.Coordinator)]
    [InlineData(WombatRoles.Administrator)]
    public async Task SomeoneWhoHoldsTrainee_CannotCreateACampaignAboutAPeer_AndNothingIsStored(string role)
    {
        await using var db = CreateDb();
        var seeded = await SeedCampaignAsync(db, MsfCampaignState.Released);
        var templateId = await TemplateOfAsync(seeded);

        var act = () => CreateAsync(db, templateId, SubjectUserId, PeerWhoAlsoHolds(role));

        (await act.Should().ThrowAsync<UnauthorizedAccessException>())
            .Which.Message.Should().StartWith("You hold the Trainee role");
        await db.SaveChangesAsync();
        (await CampaignIdsAsync()).Should().Equal(seeded);

        // The subject's own coordinator, who holds no Trainee, creates it.
        var created = await CreateAsync(db, templateId, SubjectUserId, TestPrincipals.Coordinator(HostInstitution));
        (await CampaignIdsAsync()).Should().BeEquivalentTo([seeded, created.Id]);
    }

    /// <summary>
    /// Create asks who may run the campaign before it reads the template (T224 review), so a caller who may not is told
    /// that whatever template id they send, and learns nothing from the id. Only someone who runs the trainee's campaigns
    /// is told the template is missing.
    /// </summary>
    [Fact]
    public async Task ACallerWhoMayNotCreateTheCampaign_IsToldSo_WhateverTemplateIdTheySend()
    {
        await using var db = CreateDb();
        var seeded = await SeedCampaignAsync(db, MsfCampaignState.Released);
        const int noSuchTemplate = 999_999;

        (await RefusalAsync(() => CreateAsync(db, noSuchTemplate, SubjectUserId, SubjectAs("Coordinator"))))
            .Should().BeOfType<UnauthorizedAccessException>()
            .Which.Message.Should().StartWith("A multi-source feedback campaign cannot be run by the trainee it is about.");
        (await RefusalAsync(() => CreateAsync(db, noSuchTemplate, SubjectUserId, PeerWhoAlsoHolds(WombatRoles.Coordinator))))
            .Should().BeOfType<UnauthorizedAccessException>()
            .Which.Message.Should().Be(MsfCampaignRules.TraineeRunsNoCampaigns);
        (await RefusalAsync(() => CreateAsync(db, noSuchTemplate, SubjectUserId, TestPrincipals.Coordinator(OtherInstitution))))
            .Should().BeOfType<UnauthorizedAccessException>();

        (await RefusalAsync(() => CreateAsync(db, noSuchTemplate, SubjectUserId, TestPrincipals.Coordinator(HostInstitution))))
            .Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Be("The selected MSF template could not be found.");

        await db.SaveChangesAsync();
        (await CampaignIdsAsync()).Should().Equal(seeded);
    }

    [Fact]
    public async Task TheCampaignPage_IsNullForTheSubject_WhateverRoleTheyHold_AndForAnyoneWhoHoldsTrainee()
    {
        await using var db = CreateDb();
        var campaignId = await SeedCampaignAsync(db, MsfCampaignState.Open);

        foreach (var roles in new[] { "Coordinator", "Administrator", "Coordinator+Trainee" })
        {
            (await SetupAsync(db, campaignId, SubjectAs(roles))).Should().BeNull(
                $"the subject as {roles} would otherwise watch each group's responses come in");
        }

        (await SetupAsync(db, campaignId, PeerWhoAlsoHolds(WombatRoles.Coordinator))).Should().BeNull();
        (await SetupAsync(db, campaignId, PeerWhoAlsoHolds(WombatRoles.Administrator))).Should().BeNull();

        (await SetupAsync(db, campaignId, TestPrincipals.Coordinator(HostInstitution)))!.CampaignId.Should().Be(campaignId);
        (await SetupAsync(db, campaignId, TestPrincipals.Administrator())).Should().NotBeNull();
    }

    [Fact]
    public async Task TheCoordinatorList_NeverHoldsACampaignAboutTheCaller_AndNothingForAnyoneWhoHoldsTrainee()
    {
        await using var db = CreateDb();
        var own = await SeedCampaignAsync(db, MsfCampaignState.Open);
        var peers = await SeedCampaignAsync(db, MsfCampaignState.Open, subjectUserId: "trainee-3");

        (await ListAsync(db, SubjectAs("Coordinator"))).Should().Equal(peers);
        (await ListAsync(db, SubjectAs("Administrator"))).Should().Equal(peers);
        (await ListAsync(db, SubjectAs("Coordinator+Trainee"))).Should().BeEmpty();
        (await ListAsync(db, PeerWhoAlsoHolds(WombatRoles.Coordinator))).Should().BeEmpty();
        (await ListAsync(db, PeerWhoAlsoHolds(WombatRoles.Administrator))).Should().BeEmpty();

        (await ListAsync(db, TestPrincipals.Coordinator(HostInstitution))).Should().BeEquivalentTo([own, peers]);
    }

    [Fact]
    public async Task SomeoneWhoHoldsTrainee_NeverReadsAPeersReport_EvenReleased_ButReadsTheirOwnOnceReleased()
    {
        await using var db = CreateDb();
        var underReview = await SeedCampaignAsync(db, MsfCampaignState.UnderReview);
        var released = await SeedCampaignAsync(db, MsfCampaignState.Released);

        foreach (var role in new[] { WombatRoles.Coordinator, WombatRoles.Administrator })
        {
            (await ReportAsync(db, underReview, PeerWhoAlsoHolds(role))).Should().BeNull();
            (await ReportAsync(db, released, PeerWhoAlsoHolds(role))).Should().BeNull();
        }

        (await ReportAsync(db, underReview, SubjectAs("Coordinator+Trainee"))).Should().BeNull();
        (await ReportAsync(db, released, SubjectAs("Coordinator+Trainee")))!.State.Should().Be(MsfCampaignState.Released);
        (await ReportAsync(db, underReview, SubjectAs("Administrator"))).Should().BeNull();
    }

    private static Task<MsfCampaignSetupDto?> SetupAsync(ApplicationDbContext db, int campaignId, ClaimsPrincipal principal)
        => new GetMsfCampaignSetupQueryHandler(db, FakeUserDirectory.Empty)
            .Handle(new GetMsfCampaignSetupQuery(campaignId, principal), CancellationToken.None);

    private static Task<MsfCampaignSummaryDto> CreateAsync(
        ApplicationDbContext db, int templateId, string subjectUserId, ClaimsPrincipal principal)
        => new CreateMsfCampaignCommandHandler(db, new ActivityReferenceDataService(db), FakeUserDirectory.TraineesOf(db)).Handle(
            new CreateMsfCampaignCommand(
                subjectUserId,
                templateId,
                DateOnly.FromDateTime(DateTime.UtcNow),
                DateOnly.FromDateTime(DateTime.UtcNow).AddDays(14),
                MinimumResponses: 1,
                MinimumCategoryResponses: 1,
                MinimumRespondentCategories: 1,
                EpaIds: [],
                CreatedByUserId: principal.FindFirst(ClaimTypes.NameIdentifier)!.Value,
                principal),
            CancellationToken.None);

    private async Task<int> TemplateOfAsync(int campaignId)
    {
        await using var db = CreateDb();
        return await db.MsfCampaigns.Where(entity => entity.Id == campaignId).Select(entity => entity.TemplateId).SingleAsync();
    }

    /// <summary>Every campaign stored, read through a second context so nothing tracked can mask a refused create.</summary>
    private async Task<int[]> CampaignIdsAsync()
    {
        await using var db = CreateDb();
        return await db.MsfCampaigns.AsNoTracking().OrderBy(entity => entity.Id).Select(entity => entity.Id).ToArrayAsync();
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    /// <summary>Every command addressed by campaign id; Release is refused before its evidence setup is reached.</summary>
    public static TheoryData<string> CampaignIdCommands => new() { "Open", "Close", "Withdraw", "AddInvitation", "RemoveInvitation", "Release", "Resend" };

    private static MsfCampaignState StateFor(string command) => command switch
    {
        "Close" or "Resend" => MsfCampaignState.Open,
        "Release" => MsfCampaignState.UnderReview,
        _ => MsfCampaignState.Draft
    };

    private static async Task<Exception> RefusalAsync(Func<Task> act)
    {
        var thrown = await Record.ExceptionAsync(act);
        thrown.Should().NotBeNull("the command must refuse");
        return thrown!;
    }

    private async Task RunAsync(string command, ApplicationDbContext db, int campaignId, ClaimsPrincipal principal)
    {
        switch (command)
        {
            case "Open":
                await new OpenMsfCampaignCommandHandler(
                        db,
                        _emailSender,
                        new InvitationTokenService(),
                        new FakeUserDirectory((SubjectUserId, "Thandi Nkosi")).WithTrainees(SubjectUserId),
                        Options.Create(new WombatOptions { MsfRespondUrl = "https://wombat.example/msf/respond" }))
                    .Handle(new OpenMsfCampaignCommand(campaignId, principal), CancellationToken.None);
                break;
            case "Close":
                await new CloseMsfCampaignCommandHandler(db, new MsfAggregationService())
                    .Handle(new CloseMsfCampaignCommand(campaignId, principal), CancellationToken.None);
                break;
            case "Withdraw":
                await new WithdrawMsfCampaignCommandHandler(db)
                    .Handle(new WithdrawMsfCampaignCommand(campaignId, StateFor(command), principal), CancellationToken.None);
                break;
            case "AddInvitation":
                await new AddMsfInvitationCommandHandler(db, new InvitationTokenService(), FakeUserDirectory.Trainees(SubjectUserId))
                    .Handle(
                        new AddMsfInvitationCommand(campaignId, "peer-9@example.test", MsfRespondentCategory.PeerDoctor, principal),
                        CancellationToken.None);
                break;
            case "RemoveInvitation":
                // The campaign's own invitee; for an id that names no campaign, whatever id: the scope check refuses first.
                var invitationId = await db.MsfInvitations.AsNoTracking()
                    .Where(invitation => invitation.CampaignId == campaignId)
                    .Select(invitation => invitation.Id)
                    .FirstOrDefaultAsync();
                await new RemoveMsfInvitationCommandHandler(db)
                    .Handle(new RemoveMsfInvitationCommand(campaignId, invitationId > 0 ? invitationId : 1, principal), CancellationToken.None);
                break;
            case "Resend":
                // T251: sends a new link to each respondent whose link was not delivered, so it is refused as the open is.
                await new ResendMsfLinksCommandHandler(
                        db,
                        _emailSender,
                        new InvitationTokenService(),
                        new FakeUserDirectory((SubjectUserId, "Thandi Nkosi")),
                        Options.Create(new WombatOptions { MsfRespondUrl = "https://wombat.example/msf/respond" }))
                    .Handle(new ResendMsfLinksCommand(campaignId, principal), CancellationToken.None);
                break;
            case "Release":
                await new ReleaseMsfCampaignCommandHandler(
                        db,
                        new MsfAggregationService(),
                        new ActivityService(db, new SchemaValidator(), new WorkflowEvaluator(), new CreditApplier(db), new FieldPermissionEvaluator()),
                        new ActivityReferenceDataService(db),
                        NullLogger<ReleaseMsfCampaignCommandHandler>.Instance)
                    .Handle(
                        new ReleaseMsfCampaignCommand(campaignId, "coordinator-user", null, null, principal),
                        CancellationToken.None);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(command), command, null);
        }
    }

    private static Task<MsfCampaignAggregateReportDto?> ReportAsync(ApplicationDbContext db, int campaignId, ClaimsPrincipal principal)
        => new GetCampaignAggregateReportQueryHandler(db, new MsfAggregationService())
            .Handle(new GetCampaignAggregateReportQuery(campaignId, principal), CancellationToken.None);

    private static async Task<int[]> ListAsync(ApplicationDbContext db, ClaimsPrincipal principal)
        => (await new ListMsfCampaignsForCoordinatorQueryHandler(db, FakeUserDirectory.Empty)
                .Handle(new ListMsfCampaignsForCoordinatorQuery(principal), CancellationToken.None))
            .Select(summary => summary.Id)
            .ToArray();

    private async Task ReleaseAsync(int campaignId)
    {
        await using var db = CreateDb();
        var campaign = await db.MsfCampaigns.SingleAsync(entity => entity.Id == campaignId);
        campaign.Release("coordinator-user", "Released.", null, DateTime.UtcNow);
        await db.SaveChangesAsync();
    }

    /// <summary>What a command could change, read through a second context so nothing tracked can mask it.</summary>
    private async Task<CampaignSnapshot> SnapshotAsync(int campaignId)
    {
        await using var db = CreateDb();
        var campaign = await db.MsfCampaigns
            .AsNoTracking()
            .Include(entity => entity.Invitations)
            .SingleAsync(entity => entity.Id == campaignId);

        return new CampaignSnapshot(
            campaign.State,
            campaign.OpenedOn,
            campaign.ClosedOn,
            campaign.WithdrawnOn,
            campaign.Invitations
                .OrderBy(invitation => invitation.Id)
                .Select(invitation => $"{invitation.RespondentEmail}|{invitation.TokenHash}|{invitation.AnonymizedOn:O}")
                .ToArray());
    }

    private sealed record CampaignSnapshot(
        MsfCampaignState State,
        DateTime? OpenedOn,
        DateTime? ClosedOn,
        DateTime? WithdrawnOn,
        string[] Invitations);

    /// <summary>
    /// A campaign about <paramref name="subjectUserId" />, admitted at <paramref name="subjectInstitution" /> (null: no
    /// profile at all). <paramref name="secondProfileAt" /> gives the subject a second profile there, created later (a
    /// higher id) but with an EARLIER programme start, and makes neither profile current - the case the MSF resolver
    /// used to disagree about. (The database allows one ACTIVE profile per trainee, so two tie only when both are past.)
    /// </summary>
    private async Task<int> SeedCampaignAsync(
        ApplicationDbContext db,
        MsfCampaignState state,
        string subjectUserId = SubjectUserId,
        int? subjectInstitution = HostInstitution,
        int? secondProfileAt = null)
    {
        if (subjectInstitution is int institutionId)
        {
            db.Set<TraineeProfile>().Add(new TraineeProfile
            {
                UserId = subjectUserId, InstitutionId = institutionId, CurriculumId = 1,
                ProgrammeStartDate = new DateOnly(2025, 1, 1), ExpectedCompletionDate = new DateOnly(2029, 1, 1),
                IsActive = secondProfileAt is null
            });
            await db.SaveChangesAsync();
        }

        if (secondProfileAt is int secondInstitutionId)
        {
            db.Set<TraineeProfile>().Add(new TraineeProfile
            {
                UserId = subjectUserId, InstitutionId = secondInstitutionId, CurriculumId = 1,
                ProgrammeStartDate = new DateOnly(2023, 1, 1), ExpectedCompletionDate = new DateOnly(2027, 1, 1),
                IsActive = false
            });
            await db.SaveChangesAsync();
        }

        var tokens = new InvitationTokenService();
        var campaign = new MsfCampaign
        {
            SubjectUserId = subjectUserId,
            CreatedByUserId = "coordinator-user",
            CreatedOn = DateTime.UtcNow,
            OpensOn = DateOnly.FromDateTime(DateTime.UtcNow),
            ClosesOn = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(14),
            MinimumResponses = 1,
            MinimumCategoryResponses = 1,
            MinimumRespondentCategories = 1,
            State = state,
            Template = new MsfTemplate
            {
                Name = "Annual MSF",
                Questions = [new MsfQuestion { Order = 1, Prompt = "Overall", Type = MsfQuestionType.Scale, Required = true }]
            },
            Invitations =
            [
                new MsfInvitation
                {
                    RespondentEmail = "nurse-1@example.test",
                    RespondentCategory = MsfRespondentCategory.Nurse,
                    TokenHash = tokens.HashToken(tokens.GenerateToken()),
                    IssuedOn = DateTime.UtcNow,
                    ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(21),
                    // An open campaign's invitee holds a link whose mail was dropped, so a resend has one to send (T251).
                    TokenSelector = state == MsfCampaignState.Open ? "seeded-selector" : null,
                    DeliveryLinkSelector = state == MsfCampaignState.Open ? "seeded-selector" : null,
                    DeliveryFailedOn = state == MsfCampaignState.Open ? DateTime.UtcNow : null
                }
            ]
        };

        db.MsfCampaigns.Add(campaign);
        await db.SaveChangesAsync();
        return campaign.Id;
    }

    private ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options);

    private sealed class CapturingEmailSender : IEmailSender
    {
        public List<EmailMessage> Sent { get; } = [];

        public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            Sent.Add(message);
            return Task.CompletedTask;
        }
    }
}
