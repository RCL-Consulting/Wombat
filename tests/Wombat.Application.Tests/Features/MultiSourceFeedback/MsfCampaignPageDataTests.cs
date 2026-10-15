using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.MultiSourceFeedback;

/// <summary>
/// T369 (flow 10, lane K): what the campaign list and the campaign page read beyond T217's counts, and the create and add
/// rules their forms refuse by. <see cref="MsfCampaignSummaryDto.HasOpened" /> (the list's Responses "—"); the setup's
/// thresholds, EPAs, training year, closing and the trainee's standing (About this campaign, K13, K14); F2's floor of 3
/// for a respondent group, for both kinds; and the add form's words for a blank or malformed address (K5b, K5c).
/// </summary>
public sealed class MsfCampaignPageDataTests
{
    private const int HostInstitution = 1;
    private const string SubjectUserId = "lerato";

    // ─── The list: whether a campaign ever opened (T270) ─────────────────────

    public enum Story
    {
        Draft,
        Opened,
        WithdrawnAsADraft,
        WithdrawnOnceOpen
    }

    [Theory]
    [InlineData(Story.Draft, false)]
    [InlineData(Story.Opened, true)]
    [InlineData(Story.WithdrawnAsADraft, false)]
    [InlineData(Story.WithdrawnOnceOpen, true)]
    public async Task TheList_SaysWhetherEachCampaignEverOpened(Story story, bool hasOpened)
    {
        await using var db = NewDb();
        var campaign = SeedCampaign(db, story switch
        {
            Story.Opened => MsfCampaignState.Open,
            Story.Draft => MsfCampaignState.Draft,
            _ => MsfCampaignState.Withdrawn
        });
        if (story is Story.Opened or Story.WithdrawnOnceOpen)
        {
            campaign.OpenedOn = new DateTime(2026, 10, 14, 8, 0, 0, DateTimeKind.Utc);
        }

        await db.SaveChangesAsync();

        var rows = await new ListMsfCampaignsForCoordinatorQueryHandler(db, FakeUserDirectory.Empty)
            .Handle(new ListMsfCampaignsForCoordinatorQuery(TestPrincipals.Coordinator(HostInstitution)), CancellationToken.None);

        rows.Should().ContainSingle().Which.HasOpened.Should().Be(hasOpened);
    }

    // ─── The setup: About this campaign (T369) ───────────────────────────────

    [Fact]
    public async Task TheSetup_CarriesTheThresholds_TheEpasInCodeOrder_AndWhenItClosed()
    {
        await using var db = NewDb();
        db.Epas.AddRange(
            new Epa { Id = 12, SubSpecialityId = 1, Code = "PAED-012", Title = "Communicating with and counselling", IsActive = true },
            new Epa { Id = 10, SubSpecialityId = 1, Code = "PAED-010", Title = "Leading a clinical team", IsActive = true });
        var campaign = SeedCampaign(db, MsfCampaignState.UnderReview);
        campaign.MinimumResponses = 5;
        campaign.MinimumCategoryResponses = 3;
        campaign.MinimumRespondentCategories = 2;
        campaign.OpenedOn = new DateTime(2026, 10, 14, 8, 0, 0, DateTimeKind.Utc);
        campaign.ClosedOn = new DateTime(2026, 10, 14, 14, 20, 0, DateTimeKind.Utc);
        campaign.CoveredEpas = [new MsfCampaignEpa { EpaId = 12 }, new MsfCampaignEpa { EpaId = 10 }];
        await db.SaveChangesAsync();

        var setup = await SetupAsync(db, FakeUserDirectory.Empty, campaign.Id);

        setup!.MinimumResponses.Should().Be(5);
        setup.MinimumCategoryResponses.Should().Be(3);
        setup.MinimumRespondentCategories.Should().Be(2);
        setup.CoveredEpas.Should().Equal(
            new MsfCoveredEpaDto(10, "PAED-010", "Leading a clinical team", false),
            new MsfCoveredEpaDto(12, "PAED-012", "Communicating with and counselling", false));
        setup.HasOpened.Should().BeTrue();
        setup.ClosedOn.Should().Be(new DateTime(2026, 10, 14, 14, 20, 0, DateTimeKind.Utc));
    }

    [Fact]
    public async Task ADraft_HasNeverOpened_AndHasNotClosed()
    {
        await using var db = NewDb();
        var campaign = SeedCampaign(db, MsfCampaignState.Draft);
        await db.SaveChangesAsync();

        var setup = await SetupAsync(db, FakeUserDirectory.Empty, campaign.Id);

        setup!.HasOpened.Should().BeFalse();
        setup.ClosedOn.Should().BeNull();
    }

    [Theory]
    [InlineData(MsfSubjectStandingTests.Standing.Current, true, 4, null)]
    [InlineData(MsfSubjectStandingTests.Standing.Locked, false, null, null)]
    [InlineData(MsfSubjectStandingTests.Standing.Ended, false, null, "2026-09-30")]
    public async Task TheSetup_SaysWhetherTheTraineeIsCurrent_TheirTrainingYear_AndWhenTheirProgrammeEnded(
        MsfSubjectStandingTests.Standing standing, bool isCurrent, int? trainingYear, string? endedOn)
    {
        await using var db = NewDb();
        // Started 2023-01-01: 2026-10-14 is day 1382, the fourth year.
        var profile = new TraineeProfile
        {
            UserId = SubjectUserId, InstitutionId = HostInstitution, CurriculumId = 1,
            ProgrammeStartDate = new DateOnly(2023, 1, 1), ExpectedCompletionDate = new DateOnly(2027, 1, 1)
        };
        if (standing == MsfSubjectStandingTests.Standing.Ended)
        {
            profile.Deactivate(new DateOnly(2026, 9, 30), new DateOnly(2026, 10, 1));
        }

        db.Set<TraineeProfile>().Add(profile);
        var campaign = SeedCampaign(db, MsfCampaignState.UnderReview, addProfile: false);
        await db.SaveChangesAsync();

        var users = new FakeUserDirectory((SubjectUserId, "Lerato Molefe")).With(new UserIdentityDetails(
            SubjectUserId, "lerato@example.test", "Lerato", "Molefe", HostInstitution, [], [], [WombatRoles.Trainee],
            IsLockedOut: standing == MsfSubjectStandingTests.Standing.Locked,
            IsDeactivated: standing == MsfSubjectStandingTests.Standing.Locked));

        var setup = await SetupAsync(db, users, campaign.Id, Clock(new DateTimeOffset(2026, 10, 14, 10, 0, 0, TimeSpan.Zero)));

        setup!.SubjectIsCurrent.Should().Be(isCurrent);
        setup.TrainingYear.Should().Be(trainingYear, "only a current trainee's year is shown");
        setup.SubjectEndedOn.Should().Be(endedOn is null ? null : DateOnly.Parse(endedOn));
    }

    [Fact]
    public async Task TheTrainingYear_IsReadOnTheSouthAfricanCalendar()
    {
        // 2027-01-01 00:30 SAST is still 2026-12-31 22:30 UTC: on the South African calendar the fifth year has begun.
        await using var db = NewDb();
        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            UserId = SubjectUserId, InstitutionId = HostInstitution, CurriculumId = 1,
            ProgrammeStartDate = new DateOnly(2023, 1, 2), ExpectedCompletionDate = new DateOnly(2028, 1, 1)
        });
        var campaign = SeedCampaign(db, MsfCampaignState.Draft, addProfile: false);
        await db.SaveChangesAsync();
        var users = new FakeUserDirectory((SubjectUserId, "Lerato Molefe")).With(new UserIdentityDetails(
            SubjectUserId, "lerato@example.test", "Lerato", "Molefe", HostInstitution, [], [], [WombatRoles.Trainee]));

        var setup = await SetupAsync(db, users, campaign.Id, Clock(new DateTimeOffset(2026, 12, 31, 22, 30, 0, TimeSpan.Zero)));

        // 2023-01-02 to 2026-12-31 is 1459 days (year 4); to 2027-01-01, 1460 (year 5).
        setup!.TrainingYear.Should().Be(5);
    }

    // ─── F2: a respondent group's floor of 3, for both kinds ─────────────────

    [Theory]
    [InlineData(2, false)]
    [InlineData(3, true)]
    [InlineData(100, true)]
    [InlineData(101, false)]
    public void TheCreate_RefusesARespondentGroupMinimumBelowThree(int minimum, bool accepted)
    {
        var result = new CreateMsfCampaignCommandValidator().TestValidate(Create(minimum));

        if (accepted)
        {
            result.ShouldNotHaveValidationErrorFor(command => command.MinimumCategoryResponses);
        }
        else
        {
            result.ShouldHaveValidationErrorFor(command => command.MinimumCategoryResponses)
                .WithErrorMessage("Minimum responses in a respondent group: enter a number from 3 to 100.");
        }
    }

    [Fact]
    public void TheFloor_IsThree_AndTheEntitysDefaultMeetsIt()
    {
        MsfCampaign.MinimumCategoryResponsesFloor.Should().Be(3);
        new MsfCampaign().MinimumCategoryResponses.Should().Be(MsfCampaign.MinimumCategoryResponsesFloor);
    }

    [Fact]
    public void TheValidator_AsksNoKind_SoLearnerFeedbacksOneGroupIsHeldToTheFloorToo()
    {
        // The validator reads no template; the floor is the same rule whatever kind the template is (F2).
        var learner = Create(2) with { MinimumRespondentCategories = 1, MinimumResponses = 3 };

        new CreateMsfCampaignCommandValidator().TestValidate(learner)
            .ShouldHaveValidationErrorFor(command => command.MinimumCategoryResponses);
    }

    // ─── The add form's words (K5b, K5c, T324) ──────────────────────────────

    [Theory]
    [InlineData("", "Enter the respondent's email address.")]
    [InlineData("   ", "Enter the respondent's email address.")]
    [InlineData("naledi.sebego@kgk", "Enter a valid email address, for example name@example.org.")]
    [InlineData("naledi.sebego", "Enter a valid email address, for example name@example.org.")]
    [InlineData("@kgk.wombat.local", "Enter a valid email address, for example name@example.org.")]
    [InlineData("a b@kgk.wombat.local", "Enter a valid email address, for example name@example.org.")]
    [InlineData("a@@kgk.wombat.local", "Enter a valid email address, for example name@example.org.")]
    [InlineData("a@kgk..local", "Enter a valid email address, for example name@example.org.")]
    public void TheAdd_RefusesABlankOrMalformedAddress_InThePagesWords(string address, string refusal)
    {
        var result = new AddMsfInvitationCommandValidator().TestValidate(Add(address));

        result.ShouldHaveValidationErrorFor(command => command.RespondentEmail)
            .WithErrorMessage(refusal);
        result.Errors.Should().ContainSingle("one message for the field, never two");
    }

    [Theory]
    [InlineData("naledi.sebego@kgk.wombat.local")]
    [InlineData("  Grace.Mokwena@kgk.wombat.local ")]
    [InlineData("peer-1@example.test")]
    public void TheAdd_AcceptsAnAddress(string address)
        => new AddMsfInvitationCommandValidator().TestValidate(Add(address))
            .ShouldNotHaveValidationErrorFor(command => command.RespondentEmail);

    private static CreateMsfCampaignCommand Create(int minimumCategoryResponses) => new(
        SubjectUserId, 1, new DateOnly(2026, 10, 14), new DateOnly(2026, 10, 28), 5, minimumCategoryResponses, 2, [10],
        "coordinator-1", TestPrincipals.Coordinator(HostInstitution));

    private static AddMsfInvitationCommand Add(string address)
        => new(7, address, MsfRespondentCategory.Nurse, TestPrincipals.Coordinator(HostInstitution));

    private static Task<MsfCampaignSetupDto?> SetupAsync(
        ApplicationDbContext db, FakeUserDirectory users, int campaignId, TimeProvider? clock = null)
        => new GetMsfCampaignSetupQueryHandler(db, users, clock).Handle(
            new GetMsfCampaignSetupQuery(campaignId, TestPrincipals.Coordinator(HostInstitution)), CancellationToken.None);

    private static MsfCampaign SeedCampaign(ApplicationDbContext db, MsfCampaignState state, bool addProfile = true)
    {
        if (addProfile)
        {
            db.Set<TraineeProfile>().Add(new TraineeProfile
            {
                UserId = SubjectUserId, InstitutionId = HostInstitution, CurriculumId = 1,
                ProgrammeStartDate = new DateOnly(2025, 1, 1), ExpectedCompletionDate = new DateOnly(2029, 1, 1)
            });
        }

        var campaign = new MsfCampaign
        {
            SubjectUserId = SubjectUserId,
            CreatedByUserId = "coordinator-user",
            CreatedOn = DateTime.UtcNow,
            OpensOn = new DateOnly(2026, 10, 14),
            ClosesOn = new DateOnly(2026, 10, 28),
            State = state,
            Template = new MsfTemplate { Name = "Default MSF" }
        };
        db.MsfCampaigns.Add(campaign);
        return campaign;
    }

    private static TimeProvider Clock(DateTimeOffset now) => new FixedClock(now);

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private static ApplicationDbContext NewDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
