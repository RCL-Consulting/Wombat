using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Web.Components.Pages.MultiSourceFeedback;
using Wombat.Web.Services;
using Wombat.Web.Tests.Activities;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.MultiSourceFeedback;

/// <summary>
/// The trainee a campaign is about reads its released report on their own page, <c>/msf/my-reports/{id}</c>, never on
/// the coordinator's (<c>/msf/reports/{id}</c>), whatever role brought them there; and neither page names a teaching
/// context to them (T164). (T269)
/// </summary>
/// <remarks>
/// A trainee who also coordinates is admitted to the coordinator's page by role, and the handler admits the subject to
/// their released report, so until T269 they read it there: the coordinator's actions card, the release gates and every
/// group's card, suppressed ones included. The handler marks their copy (<c>IsSubjectsCopy</c>) by the answer that also
/// leaves the teaching contexts unnamed, and both pages read that mark.
/// </remarks>
public sealed class SubjectReadsOwnReportTests : WombatTestContext
{
    private const int CampaignId = 5;
    private const string SubjectUserId = "trainee-1";
    private const string ContextName = "Neonatal night teaching";
    private const string Narrative = "Learners describe clear, well paced teaching.";

    private readonly TestAuthorizationContext _auth;
    private readonly RecordingReferenceData _referenceData = new();

    public SubjectReadsOwnReportTests()
    {
        _auth = this.AddTestAuthorization();
        Services.AddSingleton<IActivityReferenceDataService>(_referenceData);
    }

    private FakeNavigationManager Navigation => Services.GetRequiredService<FakeNavigationManager>();

    /// <summary>
    /// The subject is sent to their own copy, replacing the coordinator's address in the history, and the
    /// coordinator's page shows nothing of the report meanwhile: no Summary, no actions card, no group, no teaching
    /// context, even when the copy it is handed names one. It reads no supervision levels either, which only its
    /// release form offers.
    /// </summary>
    /// <remarks>
    /// The page reads no role, so the three rows pass alike; they are there so that it never starts to. A redirect kept
    /// for a Trainee (<c>IsInRole</c>) would leave a former trainee who now holds only Coordinator, and an Administrator
    /// who is the subject, on the coordinator's page, and the second and third rows would fail.
    /// </remarks>
    [Theory]
    [InlineData(WombatRoles.Trainee, WombatRoles.Coordinator)]
    [InlineData(WombatRoles.Coordinator)]
    [InlineData(WombatRoles.Administrator)]
    public void TheSubject_OnTheCoordinatorsPage_IsSentToTheirOwnCopy_AndShownNothingOfIt(params string[] roles)
    {
        SignInAsTheSubject(roles);
        Services.AddSingleton<IScopedSender>(
            new ReportSender(SubjectsCopy() with { TeachingContextsResponded = [ContextName] }));

        var cut = RenderComponent<CampaignReport>(parameters => parameters.Add(page => page.CampaignId, CampaignId));

        var navigation = Navigation.History.Should().ContainSingle().Subject;
        navigation.Uri.Should().Be($"/msf/my-reports/{CampaignId}");
        navigation.Options.ReplaceHistoryEntry.Should().BeTrue(
            "Back must not return to a page that sends them on again");

        cut.FindAll("h3").Should().BeEmpty("nothing of the report is rendered on the coordinator's page");
        cut.Markup.Should().NotContain("Coordinator actions")
            .And.NotContain("Teaching contexts")
            .And.NotContain(ContextName)
            .And.NotContain(Narrative)
            .And.NotContain("Report unavailable", "the subject is being sent on, not refused");
        cut.FindAll("form").Should().BeEmpty();
        _referenceData.RatedLevelsAskedFor.Should().BeEmpty();
    }

    /// <summary>
    /// Where the subject lands: their own copy, with the count of teaching contexts and no name, and no coordinator
    /// card. Rendered as the page the coordinator's page sent them to, by the same sender.
    /// </summary>
    /// <remarks>
    /// The copy handed in names a context, which the handler never gives the subject, so that the page's own leaving
    /// it out is what the name's absence shows (T269 review): the handler's half is the Application tests'.
    /// </remarks>
    [Fact]
    public void TheSubject_LandsOnMyMsfReports_WithTheCountOfTeachingContexts_AndNoCoordinatorCard()
    {
        SignInAsTheSubject([WombatRoles.Trainee, WombatRoles.Coordinator]);
        Services.AddSingleton<IScopedSender>(
            new ReportSender(SubjectsCopy() with { TeachingContextsResponded = [ContextName] }));

        RenderComponent<CampaignReport>(parameters => parameters.Add(page => page.CampaignId, CampaignId));
        Navigation.Uri.Should().EndWith($"/msf/my-reports/{CampaignId}");

        var landed = RenderComponent<MyMsfReports>(parameters => parameters.Add(page => page.CampaignId, CampaignId));
        landed.WaitForState(() => landed.Markup.Contains("Selected report"));

        landed.Markup.Should().Contain("Teaching contexts that responded:</strong> 2")
            .And.Contain(Narrative)
            .And.NotContain(ContextName)
            .And.NotContain("Coordinator actions")
            .And.NotContain("Release to trainee");
        landed.FindAll(".alert-danger").Should().BeEmpty();
        landed.FindAll("form").Should().BeEmpty();
    }

    /// <summary>
    /// Whoever runs the campaign still reads it on the coordinator's page: a copy that is not the subject's sends
    /// nobody anywhere, and names the contexts to the coordinator who typed them.
    /// </summary>
    [Fact]
    public void TheCoordinator_StillReadsTheReportOnTheirPage_AndIsSentNowhere()
    {
        _auth.SetAuthorized("coordinator@test");
        _auth.SetRoles(WombatRoles.Coordinator);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "coordinator-1"));
        Services.AddSingleton<IScopedSender>(new ReportSender(SubjectsCopy() with
        {
            IsSubjectsCopy = false,
            TeachingContextsResponded = [ContextName, "Ward round"]
        }));

        var cut = RenderComponent<CampaignReport>(parameters => parameters.Add(page => page.CampaignId, CampaignId));
        cut.WaitForState(() => cut.Markup.Contains("Coordinator actions"));

        Navigation.History.Should().BeEmpty();
        cut.Markup.Should().Contain($"2 ({ContextName}, Ward round)");
    }

    /// <summary>
    /// The trainee's page asks the handler's answer whether a copy is the caller's own, not its own comparison of ids,
    /// so the two pages and the handler cannot disagree about who the subject is (<c>MsfCampaignRules.IsCaller</c>
    /// trims a stored id, T224 review). A copy marked the subject's is shown, however its id was stored.
    /// </summary>
    [Fact]
    public void TheTraineesPage_AsksTheHandlersAnswer_WhetherTheCopyIsTheCallers()
    {
        SignInAsTheSubject([WombatRoles.Trainee]);
        Services.AddSingleton<IScopedSender>(
            new ReportSender(SubjectsCopy() with { SubjectUserId = $" {SubjectUserId} " }));

        var cut = RenderComponent<MyMsfReports>(parameters => parameters.Add(page => page.CampaignId, CampaignId));
        cut.WaitForState(() => cut.Markup.Contains("Selected report"));
        cut.FindAll(".alert-danger").Should().BeEmpty();
    }

    /// <summary>A copy not marked the caller's is refused, even when its id is the caller's.</summary>
    [Fact]
    public void TheTraineesPage_RefusesACopyNotMarkedTheirs_ThoughItNamesThem()
    {
        SignInAsTheSubject([WombatRoles.Trainee, WombatRoles.Coordinator]);
        Services.AddSingleton<IScopedSender>(new ReportSender(SubjectsCopy() with
        {
            IsSubjectsCopy = false,
            TeachingContextsResponded = [ContextName]
        }));

        var cut = RenderComponent<MyMsfReports>(parameters => parameters.Add(page => page.CampaignId, CampaignId));
        cut.WaitForState(() => cut.Markup.Contains("not available to the current trainee"));

        cut.Markup.Should().NotContain("Selected report").And.NotContain(ContextName).And.NotContain(Narrative);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private void SignInAsTheSubject(string[] roles)
    {
        _auth.SetAuthorized("trainee@test");
        _auth.SetRoles(roles);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, SubjectUserId));
    }

    /// <summary>A released learner-feedback report as the handler hands it to its subject: counted, unnamed.</summary>
    private static MsfCampaignAggregateReportDto SubjectsCopy()
        => new(CampaignId, SubjectUserId, "Teaching feedback", MsfCampaignState.Released, 3, 3, 3, Narrative, true,
            [
                new MsfCategoryAggregateDto(MsfRespondentCategory.Learner, 3, false,
                    [new MsfQuestionAggregateDto(1, "Teaching overall", MsfQuestionType.Scale,
                        new MsfScaleAggregateDto(4, 3, new Dictionary<int, int> { [4] = 3 }), [])])
            ],
            1, 1, [new MsfCoveredEpaDto(115, "PAED-015", "Teach and supervise", true)], null, DateTime.UtcNow,
            MsfTemplateKind.LearnerFeedback,
            TeachingContextsResponded: null,
            TeachingContextCount: 2,
            IsSubjectsCopy: true);

    private sealed class RecordingReferenceData : StubActivityReferenceDataService
    {
        public List<string> RatedLevelsAskedFor { get; } = [];

        public override Task<IReadOnlyList<ActivityCatalogueOption>> GetRatedLevelOptionsForActivityTypeAsync(
            string activityTypeKey, CancellationToken cancellationToken = default)
        {
            RatedLevelsAskedFor.Add(activityTypeKey);
            return Task.FromResult<IReadOnlyList<ActivityCatalogueOption>>([]);
        }
    }

    /// <summary>Answers the report and the trainee's list; any command is a failure of the test.</summary>
    private sealed class ReportSender(MsfCampaignAggregateReportDto report) : IScopedSender
    {
        public Task<TResponse> Send<TResponse>(
            IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => request switch
            {
                GetCampaignAggregateReportQuery => Task.FromResult((TResponse)(object)report),
                ListMsfCampaignsForTraineeQuery =>
                    Task.FromResult((TResponse)(object)(IReadOnlyList<MsfCampaignSummaryDto>)[]),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
    }
}
