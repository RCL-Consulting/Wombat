using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.MultiSourceFeedback;

/// <summary>
/// The report page's one read and the release's mail (T369, flow 10, lane R): no group's results before the campaign has
/// closed (F1), whoever reads it; which named EPAs the release would record (M6); the campaign's facts and the trainee's
/// standing (build-contracts § 1); who closed it (F5); and the release mail, sent once after the save, never once her
/// programme has ended (F4), never for a refused release, and never at the cost of the release (Q8).
/// </summary>
public sealed partial class MsfEvidenceFanOutTests
{
    private const string TraineeName = "Lerato Molefe";
    private const string CoordinatorName = "Pieter Smit";

    // ─── F1: nothing by group until it has closed ────────────────────────────

    [Theory]
    [InlineData(MsfCampaignState.Draft, false)]
    [InlineData(MsfCampaignState.Open, false)]
    [InlineData(MsfCampaignState.Draft, true)]
    [InlineData(MsfCampaignState.Open, true)]
    public async Task TheReport_BeforeTheCampaignHasClosed_CarriesNoGroup_ButStillCountsHowFarItIs(
        MsfCampaignState state, bool asAdministrator)
    {
        await using var db = CreateDb();
        var campaign = Seed(db, [EpaOnCurriculum]);
        campaign.State = state;
        campaign.ClosedOn = null;
        await db.SaveChangesAsync();

        var report = (await ReportFor(db, campaign.Id, asAdministrator ? Administrator() : Coordinator()))!;

        report.Categories.Should().BeEmpty("the responses are read once it has closed and every address is gone (F1)");
        report.TotalResponses.Should().Be(4, "the checklist still says how many have come back");
        report.SurvivingCategoryCount.Should().Be(2, "and how many groups would report");
    }

    [Theory]
    [InlineData(MsfCampaignState.UnderReview, true, 2)]
    [InlineData(MsfCampaignState.Released, true, 2)]
    [InlineData(MsfCampaignState.Withdrawn, true, 2)]
    [InlineData(MsfCampaignState.Withdrawn, false, 0)]
    public async Task TheReport_ReadsTheGroups_OnlyOfACampaignThatClosed(MsfCampaignState state, bool closed, int groups)
    {
        await using var db = CreateDb();
        var campaign = Seed(db, [EpaOnCurriculum]);
        campaign.State = state;
        campaign.ClosedOn = closed ? DateTime.UtcNow.AddDays(-5) : null;
        await db.SaveChangesAsync();

        (await ReportFor(db, campaign.Id, Coordinator()))!.Categories.Should().HaveCount(groups,
            "a campaign withdrawn while open never closed, and its responses are never read");
    }

    // ─── R6: the trainee's copy carries no count ─────────────────────────────

    /// <summary>
    /// T369, build review R6 (F3): the copy the trainee reads holds no suppressed group, no group's or question's count, no
    /// total, invited count or count of groups reporting, and not who closed it; the page alone kept them out until R6. The
    /// coordinator's copy of the same campaign keeps them.
    /// </summary>
    [Fact]
    public async Task TheTraineesCopy_CarriesNoCount_AndNoSuppressedGroup()
    {
        await using var db = CreateDb();
        var campaign = Seed(db, [EpaOnCurriculum]);
        AddAnsweredInvitation(campaign, MsfRespondentCategory.PeerDoctor);
        campaign.ClosedByUserId = CoordinatorUserId;
        await db.SaveChangesAsync();
        await ReleaseAsync(db, campaign.Id, entrustmentLevel: null, narrative: null);

        db.ChangeTracker.Clear();
        var coordinators = (await ReportFor(db, campaign.Id, Coordinator()))!;
        coordinators.Categories.Should().HaveCount(3, "guard: the peer doctor's one answer is a suppressed group");
        coordinators.Categories.Should().ContainSingle(category => category.IsSuppressed && category.ResponseCount == 1);
        coordinators.TotalResponses.Should().Be(5);

        var trainees = (await ReportFor(db, campaign.Id, Principal(TraineeUserId)))!;
        trainees.IsSubjectsCopy.Should().BeTrue();
        trainees.Categories.Select(category => category.Category).Should().Equal(
            MsfRespondentCategory.Consultant, MsfRespondentCategory.Nurse);
        trainees.Categories.Should().OnlyContain(category => !category.IsSuppressed && category.ResponseCount == 0);
        trainees.Categories.SelectMany(category => category.Questions).Where(question => question.Scale != null)
            .Should().NotBeEmpty().And.OnlyContain(question => question.Scale!.ResponseCount == 0 && question.Scale.Average == 4);
        trainees.TotalResponses.Should().Be(0);
        trainees.InvitedTotal.Should().Be(0);
        trainees.SurvivingCategoryCount.Should().Be(0);
        trainees.ClosedByUserId.Should().BeNull();
    }

    private static void AddAnsweredInvitation(MsfCampaign campaign, MsfRespondentCategory category)
    {
        var invitation = new MsfInvitation
        {
            RespondentCategory = category,
            TokenHash = Guid.NewGuid().ToString("N"),
            IssuedOn = DateTime.UtcNow.AddDays(-20),
            ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-2),
            RespondedOn = DateTime.UtcNow.AddDays(-5),
            AnonymizedOn = DateTime.UtcNow.AddDays(-2)
        };
        var response = new MsfResponse
        {
            SubmittedOn = DateTime.UtcNow.AddDays(-5),
            Campaign = campaign,
            Answers = [new MsfResponseAnswer { QuestionId = 1, ScaleValue = 2 }]
        };
        invitation.Responses.Add(response);
        campaign.Invitations.Add(invitation);
        campaign.Responses.Add(response);
    }

    // ─── M6: what the release will record ────────────────────────────────────

    [Fact]
    public async Task TheReport_BeforeRelease_SaysWhichNamedEpasTheReleaseWillRecord_AndAfterItWhichItDid()
    {
        await using var db = CreateDb();
        var campaign = Seed(db, [EpaOnCurriculum, EpaOffCurriculum]);

        (await ReportAsync(db, campaign.Id)).CoveredEpas
            .Select(epa => (epa.EpaId, epa.WillBeRecorded, epa.Recorded))
            .Should().BeEquivalentTo([(EpaOnCurriculum, true, false), (EpaOffCurriculum, false, false)],
                "an EPA that has left her curriculum is dropped by the release, so it will not be recorded");

        await ReleaseAsync(db, campaign.Id, entrustmentLevel: null, narrative: null);

        (await ReportAsync(db, campaign.Id)).CoveredEpas
            .Select(epa => (epa.EpaId, epa.WillBeRecorded, epa.Recorded))
            .Should().BeEquivalentTo([(EpaOnCurriculum, true, true), (EpaOffCurriculum, false, false)],
                "released, what will be recorded is what was");
    }

    [Fact]
    public async Task TheReport_OfACampaignNamingNoEpa_ListsNone()
    {
        await using var db = CreateDb();
        var campaign = Seed(db, []);

        (await ReportAsync(db, campaign.Id)).CoveredEpas.Should().BeEmpty();
    }

    // ─── The campaign's facts and the trainee's standing ─────────────────────

    [Fact]
    public async Task TheReport_CarriesTheCampaignsFacts_AndTheTraineesStanding()
    {
        await using var db = CreateDb();
        var campaign = Seed(db, [EpaOnCurriculum]);
        campaign.ClosedByUserId = CoordinatorUserId;
        await db.SaveChangesAsync();

        var report = (await ReportFor(db, campaign.Id, Coordinator(), Directory(db)))!;

        report.SubjectName.Should().Be(TraineeName);
        report.OpensOn.Should().Be(campaign.OpensOn);
        report.ClosesOn.Should().Be(campaign.ClosesOn);
        report.InvitedTotal.Should().Be(4);
        report.ClosedOn.Should().Be(campaign.ClosedOn);
        report.ClosedByUserId.Should().Be(CoordinatorUserId);
        report.ReleasedOn.Should().BeNull();
        report.SubjectIsCurrent.Should().BeTrue();
        report.SubjectEndedOn.Should().BeNull();

        await ReleaseAsync(db, campaign.Id, entrustmentLevel: null, narrative: null);
        (await ReportFor(db, campaign.Id, Coordinator(), Directory(db)))!.ReleasedOn.Should().NotBeNull();
    }

    [Fact]
    public async Task TheReport_ClosedByTheJob_HasNoCloser()
    {
        await using var db = CreateDb();
        var campaign = Seed(db, [EpaOnCurriculum]);

        (await ReportAsync(db, campaign.Id)).ClosedByUserId.Should().BeNull("\"Closed by itself\" reads a null closer (F5)");
    }

    [Fact]
    public async Task TheReport_OfATraineeWhoseProgrammeEnded_SaysSo()
    {
        await using var db = CreateDb();
        var campaign = Seed(db, [EpaOnCurriculum]);
        var ended = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-10);
        EndProgramme(db, ended);

        var report = (await ReportFor(db, campaign.Id, Coordinator(), Directory(db)))!;

        report.SubjectIsCurrent.Should().BeFalse();
        report.SubjectEndedOn.Should().Be(ended);
    }

    // ─── F5: who closed it ───────────────────────────────────────────────────

    [Fact]
    public async Task Close_RecordsTheCoordinatorWhoClosedIt()
    {
        await using var db = CreateDb();
        var campaign = Seed(db, [EpaOnCurriculum]);
        campaign.State = MsfCampaignState.Open;
        campaign.ClosedOn = null;
        await db.SaveChangesAsync();

        var closed = await new CloseMsfCampaignCommandHandler(db, new MsfAggregationService())
            .Handle(new CloseMsfCampaignCommand(campaign.Id, Coordinator()), CancellationToken.None);

        closed.ClosedByUserId.Should().Be(CoordinatorUserId);
        closed.Categories.Should().NotBeEmpty("closed, its responses are read (F1)");
        (await db.MsfCampaigns.AsNoTracking().SingleAsync()).ClosedByUserId.Should().Be(CoordinatorUserId);
    }

    // ─── Q8, F4: the release mail ────────────────────────────────────────────

    [Fact]
    public async Task Release_MailsTheTraineeOnce_AfterTheSave_WithNoneOfTheFeedback()
    {
        await using var db = CreateDb();
        var campaign = Seed(db, [EpaOnCurriculum]);
        var mail = new ReleaseMail(db, campaign.Id);

        await ReleaseWithMailAsync(db, campaign.Id, Directory(db), mail, "Consistently reliable under pressure.");

        var message = mail.Sent.Should().ContainSingle().Which;
        message.To.Should().Be($"{TraineeUserId}@test");
        message.Subject.Should().Be("MSF report released: Annual MSF");
        message.TextBody.Should().Contain("Your multi-source feedback report, Annual MSF, was released to you on")
            .And.Contain($"by {CoordinatorName}.")
            .And.Contain("This email holds none of the feedback.")
            .And.Contain("https://wombat.test/msf/my-reports");
        foreach (var body in new[] { message.TextBody, message.HtmlBody })
        {
            body.Should().NotContain("Unfailingly patient", "no comment leaves Wombat by mail")
                .And.NotContain("Consistently reliable", "nor the narrative");
        }

        mail.ReleasedWhenSent.Should().Equal([true], "the mail goes once the release has been saved");
        ReleaseMsfCampaignCommandHandler.MyReportsPath.Should().Be("/msf/my-reports");
    }

    [Fact]
    public async Task Release_OfATraineeWhoseProgrammeEnded_IsTaken_AndSendsNoMail()
    {
        await using var db = CreateDb();
        var campaign = Seed(db, [EpaOnCurriculum]);
        EndProgramme(db, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-10));
        var mail = new ReleaseMail(db, campaign.Id);

        await ReleaseWithMailAsync(db, campaign.Id, Directory(db), mail);

        (await db.MsfCampaigns.AsNoTracking().SingleAsync()).State.Should().Be(MsfCampaignState.Released, "Q10: warn and keep the release");
        mail.Sent.Should().BeEmpty("she can no longer open MSF reports (F4)");
    }

    [Fact]
    public async Task ARefusedRelease_SendsNoMail()
    {
        await using var db = CreateDb();
        var campaign = Seed(db, [EpaOnCurriculum], respondingCategories: 1);
        var mail = new ReleaseMail(db, campaign.Id);

        var release = () => ReleaseWithMailAsync(db, campaign.Id, Directory(db), mail);

        await release.Should().ThrowAsync<InvalidOperationException>();
        mail.Sent.Should().BeEmpty();
    }

    [Fact]
    public async Task AFailedReleaseMail_DoesNotFailTheRelease_AndIsLogged()
    {
        await using var db = CreateDb();
        var campaign = Seed(db, [EpaOnCurriculum]);
        var mail = new ReleaseMail(db, campaign.Id) { FailWith = new InvalidOperationException("The mail server refused it.") };
        var logger = new CapturingNoticeLogger();

        await ReleaseWithMailAsync(db, campaign.Id, Directory(db), mail, logger: logger);

        (await db.MsfCampaigns.AsNoTracking().SingleAsync()).State.Should().Be(MsfCampaignState.Released);
        logger.Errors.Should().ContainSingle().Which.Should().Contain("msf-report-released");
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    /// <summary>The trainee and the coordinator by name, the trainee holding Trainee, as the account store would answer.</summary>
    private static FakeUserDirectory Directory(ApplicationDbContext db)
        => new FakeUserDirectory((TraineeUserId, TraineeName), (CoordinatorUserId, CoordinatorName)).WithTraineesOf(db);

    private static void EndProgramme(ApplicationDbContext db, DateOnly endedOn)
    {
        var profile = db.Set<TraineeProfile>().Single();
        profile.Complete(endedOn, DateOnly.FromDateTime(DateTime.UtcNow));
        db.SaveChanges();
    }

    private static Task<MsfCampaignAggregateReportDto?> ReportFor(
        ApplicationDbContext db, int campaignId, ClaimsPrincipal caller, IUserAdministrationService? users = null)
        => new GetCampaignAggregateReportQueryHandler(
                db, new MsfAggregationService(), users ?? FakeUserDirectory.Empty, new ActivityReferenceDataService(db))
            .Handle(new GetCampaignAggregateReportQuery(campaignId, caller), CancellationToken.None);

    private static async Task ReleaseWithMailAsync(
        ApplicationDbContext db,
        int campaignId,
        IUserAdministrationService users,
        IEmailSender sender,
        string? narrative = null,
        ILogger<AccountNotices>? logger = null)
    {
        var notices = new AccountNotices(
            users,
            sender,
            Options.Create(new WombatOptions { BaseUrl = "https://wombat.test" }),
            logger ?? NullLogger<AccountNotices>.Instance);

        await new ReleaseMsfCampaignCommandHandler(
                db,
                new MsfAggregationService(),
                BuildActivityService(db),
                new ActivityReferenceDataService(db),
                users,
                notices,
                NullLogger<ReleaseMsfCampaignCommandHandler>.Instance)
            .Handle(new ReleaseMsfCampaignCommand(campaignId, CoordinatorUserId, narrative, null, Coordinator()), CancellationToken.None);
    }

    private static ClaimsPrincipal Administrator()
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "admin-1"),
                new Claim(ClaimTypes.Role, WombatRoles.Administrator)
            ],
            "test",
            ClaimTypes.Name,
            ClaimTypes.Role));

    /// <summary>Captures each release mail, and whether the campaign was already stored released when it was sent.</summary>
    private sealed class ReleaseMail(ApplicationDbContext db, int campaignId) : IEmailSender
    {
        public List<EmailMessage> Sent { get; } = [];

        public List<bool> ReleasedWhenSent { get; } = [];

        public Exception? FailWith { get; init; }

        public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            if (FailWith is not null)
            {
                throw FailWith;
            }

            ReleasedWhenSent.Add(await db.MsfCampaigns.AsNoTracking()
                .AnyAsync(campaign => campaign.Id == campaignId && campaign.State == MsfCampaignState.Released, cancellationToken));
            Sent.Add(message);
        }
    }

    private sealed class CapturingNoticeLogger : ILogger<AccountNotices>
    {
        public List<string> Errors { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullLogger.Instance.BeginScope(state);

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.Error)
            {
                Errors.Add(formatter(state, exception));
            }
        }
    }
}
