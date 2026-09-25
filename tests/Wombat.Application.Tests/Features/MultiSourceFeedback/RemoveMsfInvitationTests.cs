using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Audit;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Audit;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.MultiSourceFeedback;

/// <summary>
/// An invitee added to a draft campaign by mistake can be removed; an invitee of a campaign that has opened cannot. The
/// scope check comes first, every refusal leaves nothing behind, and no audit row names an address. (T247)
/// </summary>
/// <remarks>
/// <para>
/// Until T247 the only remedy for an invitee added by mistake was to withdraw the campaign and create it again (the T228
/// review, finding 2).
/// </para>
/// <para>
/// Each remove runs inside the real <see cref="AuditPipelineBehavior{TRequest,TResponse}" /> writing through the real
/// <see cref="AuditWriter" /> on the handler's own context, as a request does, because that is where the audit trap
/// springs: the failure row's save flushes whatever the handler left tracked. Every refusal is followed by a further save
/// and a tracker clear, and read back through a second context. The in-memory provider has no <c>xmin</c>; the races with
/// an open are <c>MsfRemoveInviteePostgresTests</c>.
/// </para>
/// </remarks>
public sealed class RemoveMsfInvitationTests
{
    private const int HostInstitution = 1;
    private const int OtherInstitution = 2;
    private const string SubjectUserId = "trainee-1";
    private const string Peer = "peer-1@example.test";
    private const string Nurse = "nurse-1@example.test";

    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task ADraftsInvitee_IsRemoved_TheOthersStay_AndTheAuditRowNamesNoAddress()
    {
        var (campaignId, invitations) = await SeedCampaignAsync(MsfCampaignState.Draft, Peer, Nurse);

        await using (var db = CreateDb())
        {
            await RemoveThroughTheAuditPipelineAsync(db, campaignId, invitations[Peer], TestPrincipals.Coordinator(HostInstitution));
        }

        await using var read = CreateDb();
        (await read.MsfInvitations.AsNoTracking().Where(invitation => invitation.CampaignId == campaignId)
                .Select(invitation => invitation.RespondentEmail).ToListAsync())
            .Should().Equal([Nurse], "only the invitee named was removed");
        (await read.MsfCampaigns.AsNoTracking().SingleAsync(campaign => campaign.Id == campaignId)).State
            .Should().Be(MsfCampaignState.Draft, "removing an invitee changes nothing else about the campaign");

        var row = await read.AuditEntries.AsNoTracking().SingleAsync();
        row.Action.Should().Be(nameof(RemoveMsfInvitationCommand));
        row.Success.Should().BeTrue();
        ShouldNameNoAddress(row.SummaryJson, row.ErrorMessage);
        var summary = JsonDocument.Parse(row.SummaryJson).RootElement;
        summary.GetProperty("campaignId").GetInt32().Should().Be(campaignId, "the row still says which campaign");
        summary.GetProperty("invitationId").GetInt32().Should().Be(invitations[Peer]);
    }

    [Fact]
    public async Task ARemovedAddress_CanBeInvitedAgain_WhileTheCampaignIsADraft()
    {
        // What the dialog promises: a remove is undone by adding the address again.
        var (campaignId, invitations) = await SeedCampaignAsync(MsfCampaignState.Draft, Peer);

        await using (var db = CreateDb())
        {
            await RemoveThroughTheAuditPipelineAsync(db, campaignId, invitations[Peer], TestPrincipals.Coordinator(HostInstitution));
        }

        await using (var db = CreateDb())
        {
            await new AddMsfInvitationCommandHandler(db, new InvitationTokenService(), FakeUserDirectory.Trainees(SubjectUserId)).Handle(
                new AddMsfInvitationCommand(campaignId, Peer, MsfRespondentCategory.Consultant, TestPrincipals.Coordinator(HostInstitution)),
                CancellationToken.None);
        }

        await using var read = CreateDb();
        var stored = await read.MsfInvitations.AsNoTracking().SingleAsync(invitation => invitation.CampaignId == campaignId);
        stored.RespondentEmail.Should().Be(Peer);
        stored.RespondentCategory.Should().Be(MsfRespondentCategory.Consultant);
    }

    public static TheoryData<MsfCampaignState> NotADraft => new()
    {
        MsfCampaignState.Open,
        MsfCampaignState.Closed,
        MsfCampaignState.UnderReview,
        MsfCampaignState.Released,
        MsfCampaignState.Withdrawn
    };

    [Theory]
    [MemberData(nameof(NotADraft))]
    public async Task AnInviteeOfACampaignNoLongerADraft_IsNotRemoved_AndNothingIsWritten(MsfCampaignState state)
    {
        // An open campaign has mailed each invitee a link, and may hold their answer, which goes with the invitation
        // (MsfResponseConfiguration). The state is refused before the invitee is looked at.
        var (campaignId, invitations) = await SeedCampaignAsync(state, Peer, Nurse);

        await using (var db = CreateDb())
        {
            var remove = () => RemoveThroughTheAuditPipelineAsync(
                db, campaignId, invitations[Peer], TestPrincipals.Coordinator(HostInstitution));
            (await remove.Should().ThrowAsync<InvalidOperationException>())
                .WithMessage(NotADraftRefusal(state))
                .Which.InnerException.Should().BeNull("refused by the check, before anything reached the database");

            await ShouldHaveLeftNothingAsync(db);
        }

        await ShouldStillInviteAsync(campaignId, Peer, Nurse);
        await ShouldHaveOneFailureRowNamingNoAddressAsync();
    }

    [Theory]
    [MemberData(nameof(NotADraft))]
    public async Task OnACampaignNoLongerADraft_AnIdItDoesNotHold_IsRefusedForTheState_NotForTheId(MsfCampaignState state)
    {
        // The state is asked before the invitee (T247 review, finding 3). Asked the other way round, an id that names
        // nothing, or another campaign's invitee, would be told NotInvited, and the campaign's own ids OnlyFromADraft: a
        // difference that tells which invitation ids an opened campaign holds.
        var (campaignId, _) = await SeedCampaignAsync(state, Peer);
        var (_, otherInvitations) = await SeedCampaignAsync(MsfCampaignState.Draft, Nurse);

        var nothing = await RefusalInItsOwnContextAsync(campaignId, 999_999, TestPrincipals.Coordinator(HostInstitution));
        var anotherCampaigns = await RefusalInItsOwnContextAsync(
            campaignId, otherInvitations[Nurse], TestPrincipals.Coordinator(HostInstitution));

        foreach (var refusal in new[] { nothing, anotherCampaigns })
        {
            refusal.Should().BeOfType<InvalidOperationException>().Which.Message.Should().Be(NotADraftRefusal(state));
        }
    }

    [Fact]
    public void TheRefusalOfAWithdrawnCampaign_SaysNothingOfLinks_WhichADraftWithdrawnNeverMailed()
    {
        // A campaign withdrawn while still a draft mailed nobody (T247 review, finding 2).
        RemoveMsfInvitationCommandHandler.OnlyFromADraftWithdrawn.Should().NotContainAny("link", "emailed");
        RemoveMsfInvitationCommandHandler.OnlyFromADraft.Should().Contain("has been opened");
    }

    [Fact]
    public void TheRefusalOfAnOpenedCampaign_DoesNotSayEachInviteeWasEmailed_WhichTheOpenCannotKnow()
    {
        // The mail leaves after the open, and whether it arrived is what the campaign page counts (T251 review, finding 4).
        RemoveMsfInvitationCommandHandler.OnlyFromADraft.Should().NotContainEquivalentOf("has been emailed")
            .And.Contain("is sent a link");
    }

    private static string NotADraftRefusal(MsfCampaignState state)
        => state == MsfCampaignState.Withdrawn
            ? RemoveMsfInvitationCommandHandler.OnlyFromADraftWithdrawn
            : RemoveMsfInvitationCommandHandler.OnlyFromADraft;

    [Fact]
    public async Task AnIdThatNamesAnotherCampaignsInvitee_IsRefusedInTheWordsOfAnIdThatNamesNothing()
    {
        // Asked of this campaign's invitations only: the other campaign's invitee is untouched, and the refusal says
        // nothing about it.
        var (campaignId, _) = await SeedCampaignAsync(MsfCampaignState.Draft, Peer);
        var (otherCampaignId, otherInvitations) = await SeedCampaignAsync(MsfCampaignState.Draft, Nurse);

        Exception otherCampaigns;
        await using (var db = CreateDb())
        {
            otherCampaigns = await RefusalAsync(() => RemoveThroughTheAuditPipelineAsync(
                db, campaignId, otherInvitations[Nurse], TestPrincipals.Coordinator(HostInstitution)));
            await ShouldHaveLeftNothingAsync(db);
        }

        var nothing = await RefusalInItsOwnContextAsync(campaignId, 999_999, TestPrincipals.Coordinator(HostInstitution));

        otherCampaigns.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Be(RemoveMsfInvitationCommandHandler.NotInvited);
        nothing.Should().BeOfType<InvalidOperationException>().Which.Message.Should().Be(otherCampaigns.Message);

        await ShouldStillInviteAsync(campaignId, Peer);
        await ShouldStillInviteAsync(otherCampaignId, Nurse);
    }

    [Fact]
    public async Task SomeoneWhoDoesNotRunTheCampaign_IsRefusedForTheCampaign_BeforeItsStateOrItsInviteeIsAsked()
    {
        // The scope check comes first (T113). A coordinator elsewhere is told what a campaign id that names nothing
        // gets, whether the campaign is a draft or open and whether the invitation id is real, so the refusal confirms
        // neither the campaign nor its invitees.
        var (draftId, draftInvitations) = await SeedCampaignAsync(MsfCampaignState.Draft, Peer);
        var (openId, openInvitations) = await SeedCampaignAsync(MsfCampaignState.Open, Nurse);
        var outsider = TestPrincipals.Coordinator(OtherInstitution, "coordinator-elsewhere");

        var onADraft = await RefusalInItsOwnContextAsync(draftId, draftInvitations[Peer], outsider);
        var onAnOpenCampaign = await RefusalInItsOwnContextAsync(openId, openInvitations[Nurse], outsider);
        var withABogusInvitee = await RefusalInItsOwnContextAsync(draftId, 999_999, outsider);
        var noSuchCampaign = await RefusalInItsOwnContextAsync(draftId + 1000, draftInvitations[Peer], outsider);

        onADraft.Should().BeOfType<UnauthorizedAccessException>();
        foreach (var refusal in new[] { onAnOpenCampaign, withABogusInvitee, noSuchCampaign })
        {
            refusal.Should().BeOfType<UnauthorizedAccessException>().Which.Message.Should().Be(onADraft.Message);
        }

        await ShouldStillInviteAsync(draftId, Peer);
        await ShouldStillInviteAsync(openId, Nurse);
    }

    public static TheoryData<string> KeptOut => new() { "Subject as Coordinator", "Coordinator+Trainee", "Trainee", "InstitutionalAdmin" };

    [Theory]
    [MemberData(nameof(KeptOut))]
    public async Task NobodyWhoDoesNotRunTheCampaign_RemovesAnInvitee(string caller)
    {
        // The trainee it is about, whatever else they hold (T224); a peer who holds Trainee (T185's rung); a trainee at
        // the same institution; an InstitutionalAdmin, who runs no campaign.
        var (campaignId, invitations) = await SeedCampaignAsync(MsfCampaignState.Draft, Peer);
        var principal = caller switch
        {
            "Subject as Coordinator" => TestPrincipals.Coordinator(HostInstitution, SubjectUserId),
            "Coordinator+Trainee" => TestPrincipals.InRoles([WombatRoles.Coordinator, WombatRoles.Trainee], "trainee-2", HostInstitution),
            "Trainee" => TestPrincipals.Trainee("trainee-2", HostInstitution),
            "InstitutionalAdmin" => TestPrincipals.InstitutionalAdmin(HostInstitution),
            _ => throw new ArgumentOutOfRangeException(nameof(caller), caller, null)
        };

        var refusal = await RefusalInItsOwnContextAsync(campaignId, invitations[Peer], principal);

        refusal.Should().BeOfType<UnauthorizedAccessException>();
        await ShouldStillInviteAsync(campaignId, Peer);
    }

    [Fact]
    public async Task AnAdministrator_RemovesAnInviteeAnywhere()
    {
        var (campaignId, invitations) = await SeedCampaignAsync(MsfCampaignState.Draft, Peer, Nurse);

        await using (var db = CreateDb())
        {
            await RemoveThroughTheAuditPipelineAsync(db, campaignId, invitations[Nurse], TestPrincipals.Administrator());
        }

        await ShouldStillInviteAsync(campaignId, Peer);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(-1, 1)]
    public void TheValidator_RefusesAnIdThatCannotNameARow(int campaignId, int invitationId)
    {
        new RemoveMsfInvitationCommandValidator()
            .Validate(new RemoveMsfInvitationCommand(campaignId, invitationId, TestPrincipals.Administrator()))
            .IsValid.Should().BeFalse();
        new RemoveMsfInvitationCommandValidator()
            .Validate(new RemoveMsfInvitationCommand(1, 1, TestPrincipals.Administrator()))
            .IsValid.Should().BeTrue();
    }

    [Fact]
    public void NoRefusal_NamesAnAddress()
    {
        // The audit row keeps a refusal's message (T184, T205).
        foreach (var refusal in new[]
                 {
                     RemoveMsfInvitationCommandHandler.OnlyFromADraft,
                     RemoveMsfInvitationCommandHandler.OnlyFromADraftWithdrawn,
                     RemoveMsfInvitationCommandHandler.NotInvited,
                     RemoveMsfInvitationCommandHandler.CampaignChanged
                 })
        {
            refusal.Should().NotContain("@");
        }
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    /// <summary>The audit behaviour outermost, then the handler: the app's order.</summary>
    private static Task RemoveThroughTheAuditPipelineAsync(
        ApplicationDbContext db, int campaignId, int invitationId, ClaimsPrincipal principal)
    {
        var command = new RemoveMsfInvitationCommand(campaignId, invitationId, principal);
        var handler = new RemoveMsfInvitationCommandHandler(db);

        return new AuditPipelineBehavior<RemoveMsfInvitationCommand, Unit>(new AuditWriter(db), new FixedAuditContext())
            .Handle(
                command,
                async () =>
                {
                    await handler.Handle(command, CancellationToken.None);
                    return Unit.Value;
                },
                CancellationToken.None);
    }

    private static async Task<Exception> RefusalAsync(Func<Task> act)
    {
        var thrown = await Record.ExceptionAsync(act);
        thrown.Should().NotBeNull("the remove must be refused");
        return thrown!;
    }

    /// <summary>What a remove refuses with, run in a context of its own as a request would be, and what it left.</summary>
    private async Task<Exception> RefusalInItsOwnContextAsync(int campaignId, int invitationId, ClaimsPrincipal principal)
    {
        await using var db = CreateDb();
        var refusal = await RefusalAsync(() => RemoveThroughTheAuditPipelineAsync(db, campaignId, invitationId, principal));
        await ShouldHaveLeftNothingAsync(db);
        return refusal;
    }

    /// <summary>
    /// The audit trap: the failure row was saved through this context, and a further save commits whatever is still
    /// tracked. Nothing may be.
    /// </summary>
    private static async Task ShouldHaveLeftNothingAsync(ApplicationDbContext db)
    {
        db.ChangeTracker.Entries().Where(entry => entry.State != EntityState.Unchanged).Should().BeEmpty();
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }

    private async Task ShouldStillInviteAsync(int campaignId, params string[] addresses)
    {
        await using var read = CreateDb();
        (await read.MsfInvitations.AsNoTracking()
                .Where(invitation => invitation.CampaignId == campaignId)
                .Select(invitation => invitation.RespondentEmail)
                .ToListAsync())
            .Should().BeEquivalentTo(addresses);
    }

    private async Task ShouldHaveOneFailureRowNamingNoAddressAsync()
    {
        await using var read = CreateDb();
        var row = await read.AuditEntries.AsNoTracking().SingleAsync();
        row.Action.Should().Be(nameof(RemoveMsfInvitationCommand));
        row.Success.Should().BeFalse();
        ShouldNameNoAddress(row.SummaryJson, row.ErrorMessage);
    }

    private static void ShouldNameNoAddress(string summaryJson, string? errorMessage)
    {
        foreach (var address in new[] { Peer, Nurse, "@" })
        {
            summaryJson.Should().NotContain(address);
            (errorMessage ?? string.Empty).Should().NotContain(address);
        }
    }

    /// <summary>A campaign about trainee-1, who trains at <see cref="HostInstitution" />, inviting each address.</summary>
    private async Task<(int CampaignId, Dictionary<string, int> Invitations)> SeedCampaignAsync(
        MsfCampaignState state, params string[] addresses)
    {
        await using var db = CreateDb();
        if (!await db.Set<TraineeProfile>().AnyAsync(profile => profile.UserId == SubjectUserId))
        {
            db.Set<TraineeProfile>().Add(new TraineeProfile
            {
                UserId = SubjectUserId, InstitutionId = HostInstitution, CurriculumId = 1,
                ProgrammeStartDate = new DateOnly(2025, 1, 1), ExpectedCompletionDate = new DateOnly(2029, 1, 1),
                IsActive = true
            });
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var campaign = new MsfCampaign
        {
            SubjectUserId = SubjectUserId,
            CreatedByUserId = "coordinator-user",
            CreatedOn = DateTime.UtcNow,
            OpensOn = today,
            ClosesOn = today.AddDays(14),
            State = state,
            Template = new MsfTemplate
            {
                Name = "Annual MSF",
                Questions = [new MsfQuestion { Order = 1, Prompt = "Overall", Type = MsfQuestionType.Scale, Required = true }]
            },
            Invitations = addresses
                .Select(address => new MsfInvitation
                {
                    RespondentEmail = address,
                    RespondentCategory = address.StartsWith("nurse", StringComparison.Ordinal)
                        ? MsfRespondentCategory.Nurse
                        : MsfRespondentCategory.PeerDoctor,
                    TokenHash = Guid.NewGuid().ToString("N"),
                    IssuedOn = DateTime.UtcNow,
                    ExpiresOn = today.AddDays(21)
                })
                .ToList()
        };

        db.MsfCampaigns.Add(campaign);
        await db.SaveChangesAsync();
        return (campaign.Id, campaign.Invitations.ToDictionary(invitation => invitation.RespondentEmail!, invitation => invitation.Id));
    }

    private ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options);

    private sealed class FixedAuditContext : IAuditContextProvider
    {
        public string? UserId => "coordinator-user";
        public string? UserDisplay => "Coordinator";
        public string? IpAddress => "10.0.0.0/24";
        public string? UserAgent => "Test/1.0";
        public int? InstitutionId => HostInstitution;
        public void DeclareInstitution(int institutionId) { }
        public void DeclareActor(string userId, string display) { }
    }
}
