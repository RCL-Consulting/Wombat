using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.DataRights;
using Wombat.Application.Features.DataRights.Queries;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.DataRights;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.DataRights;

/// <summary>
/// The erasure forecast (T387, lane D; Q3, F5, M2, S5): what Approve and erase… says it ends and keeps, read by the rules the
/// executor ends by, named only where the caller may see it (F5: a Coordinator's own institution's reviews named, the rest
/// counted; the Administrator's all named), and gated as the command.
/// </summary>
public sealed class GetErasureForecastQueryHandlerTests
{
    private const string PersonId = "person-1";
    private const int Home = 1;
    private const int Elsewhere = 2;

    private static ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static ClaimsPrincipal Admin()
        => new(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "admin-1"), new Claim(ClaimTypes.Role, WombatRoles.Administrator)],
            "test"));

    private static ClaimsPrincipal Coordinator(int institutionId, string userId = "coord-1")
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(WombatClaimTypes.InstitutionId, institutionId.ToString()),
                new Claim(ClaimTypes.Role, WombatRoles.Coordinator)
            ],
            "test"));

    private static FakeUserDirectory Directory(params string[] roles)
        => new FakeUserDirectory().With(new UserIdentityDetails(
            PersonId, "sipho@test.local", "Sipho", "Ndlovu", Home, [], [], roles));

    private static async Task<Guid> RequestAsync(
        ApplicationDbContext db,
        DataRightsRequestType type = DataRightsRequestType.Erasure,
        DataRightsRequestStatus status = DataRightsRequestStatus.Submitted,
        DateTime? requestedOn = null,
        string requesterId = PersonId)
    {
        var request = DataRightsRequest.Create(
            requesterId, "stored name", type, "reason", requestedOn ?? new DateTime(2026, 10, 27, 22, 4, 0, DateTimeKind.Utc), Home);
        request.Status = status;
        db.Set<DataRightsRequest>().Add(request);
        await db.SaveChangesAsync();
        return request.Id;
    }

    private static async Task<int> PanelAsync(ApplicationDbContext db, string name, int institutionId)
    {
        var panel = new DecisionPanel
        {
            Name = name, Scope = DecisionPanelScope.Institution, InstitutionId = institutionId, CreatedOn = DateTime.UtcNow
        };
        db.Set<DecisionPanel>().Add(panel);
        await db.SaveChangesAsync();
        return panel.Id;
    }

    private static async Task<int> ReviewAsync(
        ApplicationDbContext db,
        int panelId,
        CommitteeReviewState state,
        bool formative = false,
        string traineeId = PersonId)
    {
        var review = new CommitteeReview
        {
            AcademicYear = 2026,
            Semester = 1,
            TraineeUserId = traineeId,
            PanelId = panelId,
            IsFormative = formative,
            ReviewType = CommitteeReviewType.AnnualProgression,
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 6, 30),
            ScheduledOn = new DateOnly(2026, 11, 1)
        };
        db.Set<CommitteeReview>().Add(review);
        db.Entry(review).Property(entity => entity.State).CurrentValue = state;
        await db.SaveChangesAsync();
        return review.Id;
    }

    private static async Task<int> CampaignAsync(ApplicationDbContext db, MsfCampaignState state, string subjectId = PersonId)
    {
        var template = new MsfTemplate { Name = "Annual MSF" };
        db.Set<MsfTemplate>().Add(template);
        await db.SaveChangesAsync();
        var campaign = new MsfCampaign
        {
            SubjectUserId = subjectId,
            TemplateId = template.Id,
            CreatedByUserId = "coord-1",
            CreatedOn = DateTime.UtcNow,
            OpensOn = new DateOnly(2026, 10, 1),
            ClosesOn = new DateOnly(2026, 11, 15),
            State = state
        };
        db.Set<MsfCampaign>().Add(campaign);
        await db.SaveChangesAsync();
        return campaign.Id;
    }

    private static async Task ProfileAsync(ApplicationDbContext db, bool active, string name, string version)
    {
        var curriculum = new Curriculum { Name = name, Version = version, EffectiveFrom = new DateOnly(2025, 1, 1) };
        db.Set<Curriculum>().Add(curriculum);
        await db.SaveChangesAsync();
        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            UserId = PersonId,
            InstitutionId = Home,
            CurriculumId = curriculum.Id,
            ProgrammeStartDate = new DateOnly(2025, 1, 15),
            ExpectedCompletionDate = new DateOnly(2029, 1, 14),
            IsActive = active
        });
        await db.SaveChangesAsync();
    }

    private static Task<ErasureForecastDto?> ForecastAsync(
        ApplicationDbContext db, Guid id, ClaimsPrincipal caller, IUserAdministrationService? users = null)
        => new GetErasureForecastQueryHandler(db, users ?? Directory(WombatRoles.Trainee))
            .Handle(new GetErasureForecastQuery(id, caller), CancellationToken.None);

    /// <summary>
    /// F5 at two institutions: Pieter Smit, a Coordinator at the person's institution, reads the open review there by name
    /// and the one at another institution only as a count; the settled review elsewhere is left out (R3-D13).
    /// </summary>
    [Fact]
    public async Task ACoordinator_ReadsTheirInstitutionsReviewsByName_AndCountsTheRestElsewhere()
    {
        await using var db = CreateDb();
        var id = await RequestAsync(db);
        var homePanel = await PanelAsync(db, "Paed Annual Review Panel", Home);
        var otherPanel = await PanelAsync(db, "Elsewhere CCC", Elsewhere);
        var homeOpen = await ReviewAsync(db, homePanel, CommitteeReviewState.Scheduled, formative: true);
        await ReviewAsync(db, otherPanel, CommitteeReviewState.InProgress);
        var homeKept = await ReviewAsync(db, homePanel, CommitteeReviewState.Ratified);
        await ReviewAsync(db, otherPanel, CommitteeReviewState.UnderAppeal);
        await ReviewAsync(db, homePanel, CommitteeReviewState.Withdrawn);

        var forecast = await ForecastAsync(db, id, Coordinator(Home));

        forecast.Should().NotBeNull();
        forecast!.OpenReviews.Should().Equal(
            new ErasureForecastReview(homeOpen, "Formative check-in", "Paed Annual Review Panel", CommitteeReviewState.Scheduled));
        forecast.OpenReviewsElsewhere.Should().Be(1, "the review at another institution is counted, never named (F5)");
        forecast.KeptReviews.Should().Equal(
            new ErasureForecastReview(homeKept, "Annual progression review", "Paed Annual Review Panel", CommitteeReviewState.Ratified));
        forecast.OpenReviews.Concat(forecast.KeptReviews).Should().NotContain(review => review.PanelName == "Elsewhere CCC");
    }

    [Fact]
    public async Task TheAdministrator_ReadsEveryReviewByName()
    {
        await using var db = CreateDb();
        var id = await RequestAsync(db);
        var homePanel = await PanelAsync(db, "Paed Annual Review Panel", Home);
        var otherPanel = await PanelAsync(db, "Elsewhere CCC", Elsewhere);
        await ReviewAsync(db, homePanel, CommitteeReviewState.Scheduled);
        await ReviewAsync(db, otherPanel, CommitteeReviewState.Decided);
        await ReviewAsync(db, otherPanel, CommitteeReviewState.Final);

        var forecast = await ForecastAsync(db, id, Admin());

        forecast!.OpenReviews.Select(review => review.PanelName).Should().Equal("Paed Annual Review Panel", "Elsewhere CCC");
        forecast.OpenReviewsElsewhere.Should().Be(0);
        forecast.KeptReviews.Should().ContainSingle().Which.State.Should().Be(CommitteeReviewState.Final);
    }

    [Fact]
    public async Task TheForecast_NamesTheLiveName_TheStaffRoles_TheCampaigns_TheOtherOpenRequests_AndTheProgramme()
    {
        await using var db = CreateDb();
        var id = await RequestAsync(db);
        var draft = await CampaignAsync(db, MsfCampaignState.Draft);
        var underReview = await CampaignAsync(db, MsfCampaignState.UnderReview);
        await CampaignAsync(db, MsfCampaignState.Released);
        await CampaignAsync(db, MsfCampaignState.Withdrawn);
        await CampaignAsync(db, MsfCampaignState.Open, subjectId: "someone-else");
        var earlier = new DateTime(2026, 10, 27, 22, 1, 0, DateTimeKind.Utc);
        var submitted = await RequestAsync(db, DataRightsRequestType.Access, requestedOn: earlier);
        var approved = await RequestAsync(db, DataRightsRequestType.Rectification, DataRightsRequestStatus.Approved,
            requestedOn: earlier.AddMinutes(1));
        await RequestAsync(db, DataRightsRequestType.Export, DataRightsRequestStatus.Completed);
        await RequestAsync(db, DataRightsRequestType.Export, DataRightsRequestStatus.Withdrawn);
        await ProfileAsync(db, active: false, "Old Curriculum", "10.0");
        await ProfileAsync(db, active: true, "Paediatric EPA Curriculum", "11.2");
        var users = Directory(WombatRoles.Trainee, WombatRoles.Assessor, WombatRoles.CommitteeMember);

        var forecast = await ForecastAsync(db, id, Coordinator(Home), users);

        forecast!.PersonName.Should().Be("Sipho Ndlovu", "the live name, not the stored one (F1)");
        forecast.StaffRoleKeys.Should().Equal(WombatRoles.CommitteeMember, WombatRoles.Assessor);
        forecast.OpenCampaigns.Should().Equal(
            new ErasureForecastCampaign(draft, "Annual MSF (closing 2026-11-15)", MsfCampaignState.Draft),
            new ErasureForecastCampaign(underReview, "Annual MSF (closing 2026-11-15)", MsfCampaignState.UnderReview));
        forecast.OtherOpenRequests.Select(request => request.Id).Should().Equal(submitted, approved);
        forecast.OtherOpenRequests.Should().NotContain(request => request.Id == id, "this request is the one being approved");
        forecast.ProgrammeName.Should().Be("Paediatric EPA Curriculum (11.2)");
    }

    /// <summary>R3-D22: nothing else open. The lists are empty, nothing is counted, and no programme is named.</summary>
    [Fact]
    public async Task APersonWithNothingOpen_ReadsAnEmptyForecast()
    {
        await using var db = CreateDb();
        var id = await RequestAsync(db);

        var forecast = await ForecastAsync(db, id, Admin());

        forecast.Should().BeEquivalentTo(new ErasureForecastDto(
            "Sipho Ndlovu", [], [], 0, [], [], null, []));
    }

    [Fact]
    public async Task AnAccountThatNoLongerAnswers_IsNamedByTheStoredName_WithNoRoles()
    {
        await using var db = CreateDb();
        var id = await RequestAsync(db);

        var forecast = await ForecastAsync(db, id, Admin(), FakeUserDirectory.Empty);

        forecast!.PersonName.Should().Be("stored name");
        forecast.StaffRoleKeys.Should().BeEmpty();
    }

    [Fact]
    public async Task TheForecast_IsGatedAsTheCommand()
    {
        await using var db = CreateDb();
        var erasure = await RequestAsync(db);
        var export = await RequestAsync(db, DataRightsRequestType.Export);
        var withdrawn = await RequestAsync(db, status: DataRightsRequestStatus.Withdrawn);
        var own = await RequestAsync(db, requesterId: "coord-1");

        (await ForecastAsync(db, erasure, Coordinator(Elsewhere))).Should().BeNull("out of scope");
        (await ForecastAsync(db, Guid.NewGuid(), Admin())).Should().BeNull("an unknown id reads as one out of scope");
        (await ForecastAsync(db, export, Admin())).Should().BeNull("not an Erasure");
        (await ForecastAsync(db, withdrawn, Admin())).Should().BeNull("not Submitted");
        (await ForecastAsync(db, own, Coordinator(Home))).Should().BeNull("nobody decides their own request");
        (await ForecastAsync(db, erasure, Coordinator(Home))).Should().NotBeNull();
    }
}
