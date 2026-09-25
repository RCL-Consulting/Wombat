using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.MultiSourceFeedback;

/// <summary>
/// Opening a draft campaign, and inviting to one, is new work: it asks for a current trainee, as create does. A campaign
/// already open carries on. (T284)
/// </summary>
/// <remarks>
/// <para>
/// Until T284 only create asked (<see cref="MsfCampaignRules.MayStartCampaignAboutAsync" />, T238, T268). A draft written
/// before its trainee was locked, withdrew, lost the Trainee role or was erased could still be opened, and every
/// respondent on it was mailed a request for feedback on someone no longer in a programme.
/// </para>
/// <para>
/// A refusal must leave the store as it was even after a save, because the audit pipeline saves the request's context
/// from its catch. Every refusal here is followed by that save, the tracker is cleared, and the campaign is read back
/// through a second context.
/// </para>
/// </remarks>
public sealed class MsfNewWorkNeedsCurrentTraineeTests
{
    private const int HostInstitution = 1;
    private const int OtherInstitution = 2;
    private const string SubjectUserId = "trainee-1";
    private const string SubjectName = "Thandi Nkosi";
    private const string RespondUrl = "https://wombat.example/msf/respond";

    /// <summary><c>MsfCampaignRules.SubjectNotCurrentTrainee</c>: the refusal create gives an Administrator.</summary>
    private const string SubjectNotCurrent =
        "A multi-source feedback campaign can only be run for a trainee in a programme now: someone whose trainee " +
        "profile is active, who still holds the Trainee role, and who has not been locked out by an administrator.";

    /// <summary><c>MsfCampaignRules.CampaignNotRunByCaller</c>.</summary>
    private const string CampaignNotRunByCaller = "The MSF campaign could not be found among the campaigns you run.";

    private readonly string _databaseName = Guid.NewGuid().ToString();
    private readonly CapturingEmailSender _emailSender = new();

    /// <summary>Each way a trainee stops being current (TraineeScopeResolver, T238, T268).</summary>
    public enum Standing
    {
        Locked,
        ProgrammeEnded,
        RoleRemoved,
        NoAccount
    }

    public static TheoryData<Standing, string> NotCurrentByCaller()
    {
        var data = new TheoryData<Standing, string>();
        foreach (var standing in Enum.GetValues<Standing>())
        {
            data.Add(standing, "Coordinator");
            data.Add(standing, "Administrator");
        }

        return data;
    }

    // ─── Open ────────────────────────────────────────────────────────────────

    [Theory]
    [MemberData(nameof(NotCurrentByCaller))]
    public async Task OpeningADraft_AboutATraineeWhoIsNotCurrent_IsRefused_BeforeAnyMail_AndNothingChanges(
        Standing standing, string caller)
    {
        await using var db = CreateDb();
        var campaignId = await SeedCampaignAsync(db, MsfCampaignState.Draft, profileActive: standing != Standing.ProgrammeEnded);
        var before = await SnapshotAsync(campaignId);

        var open = () => OpenAsync(db, campaignId, CallerFor(caller), DirectoryFor(standing));

        (await open.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message.Should().Be(SubjectNotCurrent);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        (await SnapshotAsync(campaignId)).Should().BeEquivalentTo(before);
        _emailSender.Sent.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(NotCurrentByCaller))]
    public async Task InvitingToADraft_AboutATraineeWhoIsNotCurrent_IsRefused_AndNothingIsAdded(Standing standing, string caller)
    {
        await using var db = CreateDb();
        var campaignId = await SeedCampaignAsync(db, MsfCampaignState.Draft, profileActive: standing != Standing.ProgrammeEnded);
        var before = await SnapshotAsync(campaignId);

        var add = () => AddAsync(db, campaignId, CallerFor(caller), DirectoryFor(standing));

        (await add.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message.Should().Be(SubjectNotCurrent);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        (await SnapshotAsync(campaignId)).Should().BeEquivalentTo(before);
    }

    [Theory]
    [InlineData("Coordinator")]
    [InlineData("Administrator")]
    public async Task TheDraftOfACurrentTrainee_IsInvitedTo_AndOpened(string caller)
    {
        await using var db = CreateDb();
        var campaignId = await SeedCampaignAsync(db, MsfCampaignState.Draft);
        var directory = DirectoryFor(standing: null);

        await AddAsync(db, campaignId, CallerFor(caller), directory);
        await OpenAsync(db, campaignId, CallerFor(caller), directory);

        var after = await SnapshotAsync(campaignId);
        after.State.Should().Be(MsfCampaignState.Open);
        after.Invitations.Should().HaveCount(2);
        _emailSender.Sent.Select(message => message.To)
            .Should().BeEquivalentTo(["nurse-1@example.test", "peer-9@example.test"]);
    }

    [Fact]
    public async Task ATraineeLockedOutForMinutesByWrongPasswords_IsStillCurrent_SoTheirDraftOpens()
    {
        // A brute-force lockout lifts itself; treating it as a lock would let anyone stop a trainee's feedback by typing
        // wrong passwords at their account (T268).
        await using var db = CreateDb();
        var campaignId = await SeedCampaignAsync(db, MsfCampaignState.Draft);
        var directory = new FakeUserDirectory((SubjectUserId, SubjectName)).With(
            Account(roles: [WombatRoles.Trainee], lockedOut: true, deactivated: false));

        await OpenAsync(db, campaignId, TestPrincipals.Coordinator(HostInstitution), directory);

        (await SnapshotAsync(campaignId)).State.Should().Be(MsfCampaignState.Open);
    }

    [Theory]
    [InlineData("Open")]
    [InlineData("Add")]
    public async Task AnotherInstitutionsCoordinator_IsRefusedAsBefore_AndLearnsNothingOfTheTrainee(string command)
    {
        // The scope refusal comes first and is the one every outsider gets, so it says nothing about whether the trainee
        // is current (T113).
        await using var db = CreateDb();
        var campaignId = await SeedCampaignAsync(db, MsfCampaignState.Draft);
        var outsider = TestPrincipals.Coordinator(OtherInstitution, "coordinator-elsewhere");

        var run = command == "Open"
            ? () => OpenAsync(db, campaignId, outsider, DirectoryFor(Standing.Locked))
            : (Func<Task>)(() => AddAsync(db, campaignId, outsider, DirectoryFor(Standing.Locked)));

        (await run.Should().ThrowAsync<UnauthorizedAccessException>()).Which.Message.Should().Be(CampaignNotRunByCaller);
    }

    // ─── Work under way carries on ───────────────────────────────────────────

    [Fact]
    public async Task AnOpenCampaign_AboutATraineeLockedSince_StillResendsTheLinksNotDelivered()
    {
        await using var db = CreateDb();
        var campaignId = await SeedCampaignAsync(db, MsfCampaignState.Open);

        var sent = await new ResendMsfLinksCommandHandler(
                db,
                _emailSender,
                new InvitationTokenService(),
                DirectoryFor(Standing.Locked),
                Options.Create(new WombatOptions { MsfRespondUrl = RespondUrl }))
            .Handle(new ResendMsfLinksCommand(campaignId, TestPrincipals.Coordinator(HostInstitution)), CancellationToken.None);

        sent.Should().Be(1);
        _emailSender.Sent.Should().ContainSingle().Which.To.Should().Be("nurse-1@example.test");
    }

    [Fact]
    public async Task ADraft_AboutATraineeLockedSince_CanStillLoseAnInvitee_AndBeWithdrawn()
    {
        // Neither mails anyone: removing an invitee and withdrawing are not new work.
        await using var db = CreateDb();
        var campaignId = await SeedCampaignAsync(db, MsfCampaignState.Draft);
        var coordinator = TestPrincipals.Coordinator(HostInstitution);
        var invitationId = await db.MsfInvitations.Where(invitation => invitation.CampaignId == campaignId)
            .Select(invitation => invitation.Id).SingleAsync();

        await new RemoveMsfInvitationCommandHandler(db)
            .Handle(new RemoveMsfInvitationCommand(campaignId, invitationId, coordinator), CancellationToken.None);
        await new WithdrawMsfCampaignCommandHandler(db)
            .Handle(new WithdrawMsfCampaignCommand(campaignId, MsfCampaignState.Draft, coordinator), CancellationToken.None);

        var after = await SnapshotAsync(campaignId);
        after.State.Should().Be(MsfCampaignState.Withdrawn);
        after.Invitations.Should().BeEmpty();
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static ClaimsPrincipal CallerFor(string caller)
        => caller == "Administrator" ? TestPrincipals.Administrator() : TestPrincipals.Coordinator(HostInstitution);

    /// <summary>
    /// The user store as it answers for the subject: a current trainee when <paramref name="standing" /> is null. The
    /// subject is always named, so a refusal is never the open's "no name" one.
    /// </summary>
    private static FakeUserDirectory DirectoryFor(Standing? standing)
    {
        var directory = new FakeUserDirectory((SubjectUserId, SubjectName));
        return standing switch
        {
            null or Standing.ProgrammeEnded => directory.With(Account(roles: [WombatRoles.Trainee])),
            Standing.Locked => directory.With(Account(roles: [WombatRoles.Trainee], lockedOut: true, deactivated: true)),
            Standing.RoleRemoved => directory.With(Account(roles: [WombatRoles.Assessor])),
            Standing.NoAccount => directory,
            _ => throw new ArgumentOutOfRangeException(nameof(standing), standing, null)
        };
    }

    private static UserIdentityDetails Account(string[] roles, bool lockedOut = false, bool deactivated = false)
        => new(SubjectUserId, "thandi@example.test", "Thandi", "Nkosi", HostInstitution, [], [], roles, lockedOut, deactivated);

    private Task OpenAsync(ApplicationDbContext db, int campaignId, ClaimsPrincipal principal, IUserAdministrationService users)
        => new OpenMsfCampaignCommandHandler(
                db,
                _emailSender,
                new InvitationTokenService(),
                users,
                Options.Create(new WombatOptions { MsfRespondUrl = RespondUrl }))
            .Handle(new OpenMsfCampaignCommand(campaignId, principal), CancellationToken.None);

    private static Task<int> AddAsync(ApplicationDbContext db, int campaignId, ClaimsPrincipal principal, IUserAdministrationService users)
        => new AddMsfInvitationCommandHandler(db, new InvitationTokenService(), users)
            .Handle(
                new AddMsfInvitationCommand(campaignId, "peer-9@example.test", MsfRespondentCategory.PeerDoctor, principal),
                CancellationToken.None);

    /// <summary>
    /// A campaign about trainee-1, whose profile is at <see cref="HostInstitution" /> (ended when
    /// <paramref name="profileActive" /> is false), with one invitee. An open campaign's invitee holds a link whose mail
    /// was dropped, so a resend has one to send (T251).
    /// </summary>
    private static async Task<int> SeedCampaignAsync(ApplicationDbContext db, MsfCampaignState state, bool profileActive = true)
    {
        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            UserId = SubjectUserId, InstitutionId = HostInstitution, CurriculumId = 1,
            ProgrammeStartDate = new DateOnly(2025, 1, 1), ExpectedCompletionDate = new DateOnly(2029, 1, 1),
            IsActive = profileActive
        });

        var tokens = new InvitationTokenService();
        var open = state == MsfCampaignState.Open;
        var campaign = new MsfCampaign
        {
            SubjectUserId = SubjectUserId,
            CreatedByUserId = "coordinator-user",
            CreatedOn = DateTime.UtcNow,
            OpensOn = DateOnly.FromDateTime(DateTime.UtcNow),
            ClosesOn = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(14),
            MinimumResponses = 1,
            MinimumCategoryResponses = 1,
            MinimumRespondentCategories = 1,
            State = state,
            OpenedOn = open ? DateTime.UtcNow.AddDays(-1) : null,
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
                    IssuedOn = DateTime.UtcNow.AddDays(-1),
                    ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(21),
                    TokenSelector = open ? "seeded-selector" : null,
                    DeliveryLinkSelector = open ? "seeded-selector" : null,
                    DeliveryFailedOn = open ? DateTime.UtcNow : null
                }
            ]
        };

        db.MsfCampaigns.Add(campaign);
        await db.SaveChangesAsync();
        return campaign.Id;
    }

    /// <summary>What an open or an add could change, read through a second context so nothing tracked can mask it.</summary>
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
            campaign.Invitations
                .OrderBy(invitation => invitation.Id)
                .Select(invitation => $"{invitation.RespondentEmail}|{invitation.TokenHash}|{invitation.TokenSelector}")
                .ToArray());
    }

    private sealed record CampaignSnapshot(MsfCampaignState State, DateTime? OpenedOn, string[] Invitations);

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
