using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.MultiSourceFeedback;

/// <summary>
/// The campaign page reads a campaign's invitees as counts by respondent group, whatever its state, and whose campaign it
/// is by name. (T217)
/// </summary>
/// <remarks>
/// Until T217 the page showed no invitees at all, in any state. What it shows is a count per group, never a row per
/// invitee: the coordinator knows which row is whom, so a responded mark on one row would name the author of the comment
/// that has just appeared on the report (<see cref="MsfCampaignSetupDto.Invitees" />).
/// </remarks>
public sealed class MsfCampaignSetupInviteeTests
{
    private const int HostInstitution = 1;
    private const string SubjectUserId = "amara";

    [Fact]
    public async Task AnOpenCampaign_CountsItsInviteesAndResponsesByGroup_InTheCategorysOrder_AndNamesTheTrainee()
    {
        await using var db = NewDb();
        var campaignId = await SeedCampaignAsync(db, MsfCampaignState.Open,
            (MsfRespondentCategory.Nurse, true),
            (MsfRespondentCategory.PeerDoctor, true),
            (MsfRespondentCategory.Consultant, false),
            (MsfRespondentCategory.PeerDoctor, false),
            (MsfRespondentCategory.Nurse, false),
            (MsfRespondentCategory.PeerDoctor, true));
        var users = new FakeUserDirectory((SubjectUserId, "Amara Okafor"));

        var setup = await SetupAsync(db, users, campaignId);

        setup.Should().NotBeNull();
        setup!.State.Should().Be(MsfCampaignState.Open);
        setup.SubjectName.Should().Be("Amara Okafor");
        setup.OpensOn.Should().Be(new DateOnly(2026, 9, 1));
        setup.ClosesOn.Should().Be(new DateOnly(2026, 9, 15));
        setup.Invitees.Should().Equal(
            new MsfInviteeCountDto(MsfRespondentCategory.PeerDoctor, 3, 2),
            new MsfInviteeCountDto(MsfRespondentCategory.Consultant, 1, 0),
            new MsfInviteeCountDto(MsfRespondentCategory.Nurse, 2, 1));
        users.Lookups.Should().ContainSingle().Which.Should().Equal(SubjectUserId);
    }

    [Fact]
    public async Task TheSetup_CarriesNoRespondentsAddress()
    {
        await using var db = NewDb();
        var campaignId = await SeedCampaignAsync(db, MsfCampaignState.Open,
            (MsfRespondentCategory.PeerDoctor, true),
            (MsfRespondentCategory.Nurse, false));

        var setup = await SetupAsync(db, FakeUserDirectory.Empty, campaignId);

        var addresses = await db.MsfInvitations.Select(invitation => invitation.RespondentEmail!).ToListAsync();
        addresses.Should().HaveCount(2).And.OnlyContain(address => address.EndsWith("@example.test", StringComparison.Ordinal));
        JsonSerializer.Serialize(setup).Should().NotContain("@example.test");
    }

    [Fact]
    public async Task AClosedCampaign_StillCountsItsAnonymisedInvitees()
    {
        // Closing erases each address (MsfCampaign.Close), not the category, so the counts outlive it.
        await using var db = NewDb();
        var campaignId = await SeedCampaignAsync(db, MsfCampaignState.Open,
            (MsfRespondentCategory.PeerDoctor, true),
            (MsfRespondentCategory.Nurse, false));
        var campaign = await db.MsfCampaigns.Include(entity => entity.Invitations).SingleAsync(entity => entity.Id == campaignId);
        campaign.Close(DateTime.UtcNow);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var setup = await SetupAsync(db, FakeUserDirectory.Empty, campaignId);

        (await db.MsfInvitations.AllAsync(invitation => invitation.RespondentEmail == null)).Should().BeTrue();
        setup!.State.Should().Be(MsfCampaignState.UnderReview);
        setup.Invitees.Should().Equal(
            new MsfInviteeCountDto(MsfRespondentCategory.PeerDoctor, 1, 1),
            new MsfInviteeCountDto(MsfRespondentCategory.Nurse, 1, 0));
    }

    [Fact]
    public async Task ADraftWithNobodyInvited_HasNoInviteeCounts()
    {
        await using var db = NewDb();
        var campaignId = await SeedCampaignAsync(db, MsfCampaignState.Draft);

        var setup = await SetupAsync(db, FakeUserDirectory.Empty, campaignId);

        setup!.State.Should().Be(MsfCampaignState.Draft);
        setup.Invitees.Should().BeEmpty();
        setup.SubjectName.Should().Be(SubjectUserId, "a trainee with no name on record is shown by id");
    }

    // ─── A draft's addresses, for Remove (T247) ─────────────────────────────

    [Fact]
    public async Task ADraft_ListsEachInviteeByTheAddressTyped_InTheOrderAdded_BesideTheCounts()
    {
        await using var db = NewDb();
        var campaignId = await SeedCampaignAsync(db, MsfCampaignState.Draft,
            (MsfRespondentCategory.Nurse, false),
            (MsfRespondentCategory.PeerDoctor, false),
            (MsfRespondentCategory.Nurse, false));
        var ids = await db.MsfInvitations.OrderBy(invitation => invitation.Id).Select(invitation => invitation.Id).ToListAsync();

        var setup = await SetupAsync(db, FakeUserDirectory.Empty, campaignId);

        setup!.DraftInvitees.Should().Equal(
            new MsfDraftInviteeDto(ids[0], "respondent-1@example.test", MsfRespondentCategory.Nurse, null),
            new MsfDraftInviteeDto(ids[1], "respondent-2@example.test", MsfRespondentCategory.PeerDoctor, null),
            new MsfDraftInviteeDto(ids[2], "respondent-3@example.test", MsfRespondentCategory.Nurse, null));
        setup.Invitees.Should().Equal(
            new MsfInviteeCountDto(MsfRespondentCategory.PeerDoctor, 1, 0),
            new MsfInviteeCountDto(MsfRespondentCategory.Nurse, 2, 0));
    }

    [Fact]
    public async Task ALearnerFeedbackDraft_ListsWhereEachLearnerWasTaught()
    {
        await using var db = NewDb();
        var campaignId = await SeedCampaignAsync(db, MsfCampaignState.Draft, (MsfRespondentCategory.Learner, false));
        var invitation = await db.MsfInvitations.SingleAsync();
        invitation.TeachingContext = "Ward round";
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var setup = await SetupAsync(db, FakeUserDirectory.Empty, campaignId);

        setup!.DraftInvitees.Should().Equal(
            new MsfDraftInviteeDto(invitation.Id, "respondent-1@example.test", MsfRespondentCategory.Learner, "Ward round"));
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
    public async Task NoCampaignButADraft_ListsAnAddress_ThoughItsRowsStillHoldOne(MsfCampaignState state)
    {
        // Seeded with every address still held, which closing and withdrawing would have erased: the rule is the state,
        // not whether an address happens to be left. Once a campaign opens, its invitees are counted by group only. (T217)
        await using var db = NewDb();
        var campaignId = await SeedCampaignAsync(db, state,
            (MsfRespondentCategory.PeerDoctor, false),
            (MsfRespondentCategory.Nurse, false));

        var setup = await SetupAsync(db, FakeUserDirectory.Empty, campaignId);

        setup!.State.Should().Be(state);
        setup.DraftInvitees.Should().BeEmpty();
        JsonSerializer.Serialize(setup).Should().NotContain("@example.test");
        setup.Invitees.Sum(group => group.Invited).Should().Be(2, "the counts are there in every state");
    }

    private static Task<MsfCampaignSetupDto?> SetupAsync(ApplicationDbContext db, FakeUserDirectory users, int campaignId)
        => new GetMsfCampaignSetupQueryHandler(db, users).Handle(
            new GetMsfCampaignSetupQuery(campaignId, TestPrincipals.Coordinator(HostInstitution)), CancellationToken.None);

    private static async Task<int> SeedCampaignAsync(
        ApplicationDbContext db,
        MsfCampaignState state,
        params (MsfRespondentCategory Category, bool Responded)[] invitees)
    {
        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            UserId = SubjectUserId,
            InstitutionId = HostInstitution,
            CurriculumId = 1,
            ProgrammeStartDate = new DateOnly(2025, 1, 1),
            ExpectedCompletionDate = new DateOnly(2029, 1, 1),
            IsActive = true
        });

        var campaign = new MsfCampaign
        {
            SubjectUserId = SubjectUserId,
            CreatedByUserId = "coordinator-user",
            CreatedOn = DateTime.UtcNow,
            OpensOn = new DateOnly(2026, 9, 1),
            ClosesOn = new DateOnly(2026, 9, 15),
            State = state,
            Template = new MsfTemplate { Name = "Annual MSF" }
        };

        var number = 0;
        foreach (var (category, responded) in invitees)
        {
            var invitation = new MsfInvitation
            {
                RespondentEmail = $"respondent-{++number}@example.test",
                RespondentCategory = category,
                TokenHash = Guid.NewGuid().ToString("N"),
                IssuedOn = DateTime.UtcNow,
                ExpiresOn = new DateOnly(2026, 9, 22)
            };

            if (responded)
            {
                invitation.RecordResponse(DateTime.UtcNow);
                var response = new MsfResponse { SubmittedOn = DateTime.UtcNow, Campaign = campaign };
                invitation.Responses.Add(response);
                campaign.Responses.Add(response);
            }

            campaign.Invitations.Add(invitation);
        }

        db.MsfCampaigns.Add(campaign);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return campaign.Id;
    }

    private static ApplicationDbContext NewDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
