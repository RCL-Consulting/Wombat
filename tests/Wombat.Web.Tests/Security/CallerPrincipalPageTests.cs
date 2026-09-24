using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Features.Trainees;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Components.Pages.MultiSourceFeedback;
using Wombat.Web.Components.Pages.Portfolio;
using Wombat.Web.Services;
using EntrustmentDecisionsAdmin = Wombat.Web.Components.Pages.Admin.EntrustmentDecisions.Index;
using Wombat.Web.Tests.Activities;

namespace Wombat.Web.Tests.Security;

/// <summary>
/// Every page whose handler authorises against the caller hands it the signed-in principal. (T113)
/// </summary>
/// <remarks>
/// <para>
/// Since T113 the trainee-id queries and the MSF commands and report decide from the principal on the request who may
/// see what. A page that passed anything else, an empty principal or someone else's, would still compile, and the
/// trainee or coordinator would silently see an empty list or "Report unavailable" rather than an error. These pin
/// the caller on every such request the pages send.
/// </para>
/// <para>
/// MyProgress is covered beside its rendering (<c>QuotaProgressRenderingTests</c>).
/// </para>
/// </remarks>
public sealed class CallerPrincipalPageTests : TestContext
{
    private const string TraineeUserId = "trainee-1";
    private const string CoordinatorUserId = "coordinator-1";
    private const string AdminUserId = "admin-1";
    private const int DecisionId = 41;
    private const int CampaignId = 5;

    private readonly TestAuthorizationContext _auth;
    private readonly RecordingSender _sender = new();
    private readonly RecordingReferenceData _referenceData = new();

    public CallerPrincipalPageTests()
    {
        _auth = this.AddTestAuthorization();
        Services.AddSingleton<IScopedSender>(_sender);
        Services.AddSingleton<IActivityReferenceDataService>(_referenceData);
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    // ─── The trainee's pages ─────────────────────────────────────────────────

    [Fact]
    public void MyReviews_AsksForTheSignedInTraineesReviews_AsThatTrainee()
    {
        SignIn(TraineeUserId, WombatRoles.Trainee);
        _sender.On<ListReviewsForTraineeQuery>(_ => Array.Empty<CommitteeReviewListItemDto>());

        var cut = RenderComponent<MyReviews>();
        cut.WaitForState(() => cut.Markup.Contains("No decisions yet"));

        var query = _sender.Single<ListReviewsForTraineeQuery>();
        query.TraineeUserId.Should().Be(TraineeUserId);
        CallerOf(query.Principal).Should().Be(TraineeUserId);
    }

    [Fact]
    public void MyAuthorisations_AsksForTheSignedInTraineesDecisions_AsThatTrainee()
    {
        SignIn(TraineeUserId, WombatRoles.Trainee);
        _sender.On<GetActiveDecisionsForTraineeQuery>(_ => Array.Empty<EntrustmentDecisionDto>());

        var cut = RenderComponent<MyAuthorisations>();
        cut.WaitForState(() => cut.Markup.Contains("No active authorisations yet"));

        var query = _sender.Single<GetActiveDecisionsForTraineeQuery>();
        query.TraineeUserId.Should().Be(TraineeUserId);
        CallerOf(query.Principal).Should().Be(TraineeUserId);
    }

    [Fact]
    public void MyMsfReports_ListsAndOpensAReport_AsTheSignedInTrainee()
    {
        SignIn(TraineeUserId, WombatRoles.Trainee);
        _sender
            .On<ListMsfCampaignsForTraineeQuery>(_ => new[] { Summary(TraineeUserId) })
            .On<GetCampaignAggregateReportQuery>(_ => Report(TraineeUserId, MsfCampaignState.Released));

        var cut = RenderComponent<MyMsfReports>(parameters => parameters.Add(page => page.CampaignId, CampaignId));
        cut.WaitForState(() => cut.Markup.Contains("Selected report"));

        var list = _sender.Single<ListMsfCampaignsForTraineeQuery>();
        list.SubjectUserId.Should().Be(TraineeUserId);
        CallerOf(list.Principal).Should().Be(TraineeUserId);

        var report = _sender.Single<GetCampaignAggregateReportQuery>();
        report.CampaignId.Should().Be(CampaignId);
        CallerOf(report.Principal).Should().Be(TraineeUserId);
    }

    [Fact]
    public void MyMsfReports_NeverShowsSomeoneElsesReport_ThoughTheHandlerHandsItToAnAdministrator()
    {
        // An Administrator may read any report, so the handler returns this one. The page is about the caller's OWN
        // reports: before T113 it raised its "not available" error but left the report assigned, and it rendered
        // under the error.
        SignIn("admin-1", WombatRoles.Administrator);
        _sender
            .On<ListMsfCampaignsForTraineeQuery>(_ => Array.Empty<MsfCampaignSummaryDto>())
            .On<GetCampaignAggregateReportQuery>(_ => Report("trainee-2", MsfCampaignState.Released));

        var cut = RenderComponent<MyMsfReports>(parameters => parameters.Add(page => page.CampaignId, CampaignId));
        cut.WaitForState(() => cut.Markup.Contains("not available to the current trainee"));

        cut.Markup.Should().NotContain("Selected report");
        cut.Markup.Should().NotContain(SecretNarrative);
    }

    [Fact]
    public void MyAuthorisations_SaysSo_WhenTheCertificateComesBackEmpty()
    {
        // The handler answers a certificate the caller may not have, and one that does not exist, with null. (T183)
        SignIn(TraineeUserId, WombatRoles.Trainee);
        _sender
            .On<GetActiveDecisionsForTraineeQuery>(_ => new[] { Decision(TraineeUserId) })
            .On<DownloadEntrustmentCertificateCommand>(_ => null);

        var cut = RenderComponent<MyAuthorisations>();
        cut.WaitForState(() => cut.FindAll("button").Any(button => button.TextContent.Contains("Download certificate")));

        cut.FindAll("button").Single(button => button.TextContent.Contains("Download certificate")).Click();
        cut.WaitForState(() => cut.Markup.Contains("The certificate could not be found."));

        var download = _sender.Single<DownloadEntrustmentCertificateCommand>();
        download.DecisionId.Should().Be(DecisionId);
        CallerOf(download.Principal).Should().Be(TraineeUserId);
        JSInterop.Invocations.Should().NotContain(invocation => invocation.Identifier == "wombatFileDownload");
    }

    // ─── The entrustment-decision admin page ─────────────────────────────────

    [Theory]
    [InlineData(WombatRoles.Administrator)]
    [InlineData(WombatRoles.InstitutionalAdmin)]
    [InlineData(WombatRoles.SpecialityAdmin)]
    public void EntrustmentDecisionsAdmin_ListsDownloadsAndRevokes_AsTheSignedInAdmin(string role)
    {
        // Each handler narrows to the trainees its caller oversees (T183), so each must be asked as that caller.
        SignIn(AdminUserId, role);
        _sender
            .On<ListEntrustmentDecisionsForAdminQuery>(_ => new[] { Decision(TraineeUserId) })
            .On<DownloadEntrustmentCertificateCommand>(_ => new EntrustmentCertificateResult([0x25, 0x50, 0x44, 0x46], "star.pdf", "hash"))
            .On<RevokeEntrustmentDecisionCommand>(_ => Decision(TraineeUserId));

        var cut = RenderComponent<EntrustmentDecisionsAdmin>();
        cut.WaitForState(() => cut.Markup.Contains("PAED-001"));

        cut.FindAll("button").Single(button => button.TextContent.Trim() == "Download").Click();
        cut.WaitForState(() => JSInterop.Invocations.Any(invocation => invocation.Identifier == "wombatFileDownload"));

        cut.FindAll("button").Single(button => button.TextContent.Trim() == "Revoke").Click();
        cut.Find("#revoke-reason").Change("Concern raised at the site.");
        cut.FindAll("button").Single(button => button.TextContent.Contains("Confirm revocation")).Click();
        cut.WaitForState(() => cut.Markup.Contains("revoked"));

        var lists = _sender.Received.OfType<ListEntrustmentDecisionsForAdminQuery>().ToList();
        lists.Should().NotBeEmpty();
        lists.Should().OnlyContain(query => CallerOf(query.Principal) == AdminUserId);

        var download = _sender.Single<DownloadEntrustmentCertificateCommand>();
        download.DecisionId.Should().Be(DecisionId);
        CallerOf(download.Principal).Should().Be(AdminUserId);

        var revoke = _sender.Single<RevokeEntrustmentDecisionCommand>();
        revoke.DecisionId.Should().Be(DecisionId);
        CallerOf(revoke.Principal).Should().Be(AdminUserId);
    }

    [Fact]
    public void EntrustmentDecisionsAdmin_SaysSo_WhenTheCertificateIsOutOfScope()
    {
        SignIn(AdminUserId, WombatRoles.InstitutionalAdmin);
        _sender
            .On<ListEntrustmentDecisionsForAdminQuery>(_ => new[] { Decision(TraineeUserId) })
            .On<DownloadEntrustmentCertificateCommand>(_ => null);

        var cut = RenderComponent<EntrustmentDecisionsAdmin>();
        cut.WaitForState(() => cut.Markup.Contains("PAED-001"));

        cut.FindAll("button").Single(button => button.TextContent.Trim() == "Download").Click();
        cut.WaitForState(() => cut.Markup.Contains("could not be found among the decisions you oversee"));

        JSInterop.Invocations.Should().NotContain(invocation => invocation.Identifier == "wombatFileDownload");
    }

    // ─── The coordinator's pages ─────────────────────────────────────────────

    [Fact]
    public void CampaignsList_AsksForTheCampaignsTheSignedInCoordinatorRuns()
    {
        SignIn(CoordinatorUserId, WombatRoles.Coordinator);
        _sender.On<ListMsfCampaignsForCoordinatorQuery>(_ => Array.Empty<MsfCampaignSummaryDto>());

        var cut = RenderComponent<CampaignsList>();
        cut.WaitForState(() => cut.Markup.Contains("No MSF campaigns"));

        CallerOf(_sender.Single<ListMsfCampaignsForCoordinatorQuery>().Principal).Should().Be(CoordinatorUserId);
    }

    [Fact]
    public void CampaignReport_ReadsAndClosesTheCampaign_AsTheSignedInCoordinator()
    {
        SignIn(CoordinatorUserId, WombatRoles.Coordinator);
        _sender
            .On<GetCampaignAggregateReportQuery>(_ => Report(TraineeUserId, MsfCampaignState.Open))
            .On<CloseMsfCampaignCommand>(_ => Report(TraineeUserId, MsfCampaignState.UnderReview));

        var cut = RenderComponent<CampaignReport>(parameters => parameters.Add(page => page.CampaignId, CampaignId));
        cut.WaitForState(() => cut.FindAll("button").Any(button => button.TextContent.Contains("Close campaign")));

        cut.FindAll("button").Single(button => button.TextContent.Contains("Close campaign")).Click();
        cut.WaitForState(() => cut.Markup.Contains("Campaign closed"));

        CallerOf(_sender.Single<GetCampaignAggregateReportQuery>().Principal).Should().Be(CoordinatorUserId);
        var close = _sender.Single<CloseMsfCampaignCommand>();
        close.CampaignId.Should().Be(CampaignId);
        CallerOf(close.Principal).Should().Be(CoordinatorUserId);
    }

    [Fact]
    public void CampaignReport_ReleasesTheCampaign_AsTheSignedInCoordinator()
    {
        SignIn(CoordinatorUserId, WombatRoles.Coordinator);
        _sender
            .On<GetCampaignAggregateReportQuery>(_ => Report(TraineeUserId, MsfCampaignState.UnderReview))
            .On<ReleaseMsfCampaignCommand>(_ => null);

        var cut = RenderComponent<CampaignReport>(parameters => parameters.Add(page => page.CampaignId, CampaignId));
        cut.WaitForState(() => cut.FindAll("#msf-narrative").Count > 0);

        cut.Find("form").Submit();
        cut.WaitForState(() => cut.Markup.Contains("Report released"));

        var release = _sender.Single<ReleaseMsfCampaignCommand>();
        release.CampaignId.Should().Be(CampaignId);
        release.ReviewerUserId.Should().Be(CoordinatorUserId);
        CallerOf(release.Principal).Should().Be(CoordinatorUserId);
    }

    [Fact]
    public void CampaignEdit_AddsAnInviteeAndOpensTheCampaign_AsTheSignedInCoordinator()
    {
        SignIn(CoordinatorUserId, WombatRoles.Coordinator);
        StubTheEditorsLists();
        _sender
            .On<AddMsfInvitationCommand>(_ => 17)
            .On<OpenMsfCampaignCommand>(_ => null);

        var cut = RenderComponent<CampaignEdit>(parameters => parameters.Add(page => page.CampaignId, CampaignId));

        cut.Find("#msf-respondent-email").Change("peer-1@example.test");
        cut.Find("#msf-respondent-email").Closest("form")!.Submit();
        cut.WaitForState(() => cut.Markup.Contains("Invitee added"));

        cut.FindAll("button").Single(button => button.TextContent.Contains("Open campaign")).Click();
        cut.WaitForState(() => cut.Markup.Contains("Campaign opened"));

        var add = _sender.Single<AddMsfInvitationCommand>();
        add.CampaignId.Should().Be(CampaignId);
        CallerOf(add.Principal).Should().Be(CoordinatorUserId);

        var open = _sender.Single<OpenMsfCampaignCommand>();
        open.CampaignId.Should().Be(CampaignId);
        CallerOf(open.Principal).Should().Be(CoordinatorUserId);
    }

    [Fact]
    public void CampaignEdit_ReadsTheCurriculumOnlyOfATraineeThePickerOffered()
    {
        // The select's value arrives from the browser. The reference service answers any id, so a value edited in the
        // page would read another institution's trainee's curriculum and confirm that the id names someone.
        SignIn(CoordinatorUserId, WombatRoles.Coordinator);
        StubTheEditorsLists();

        var cut = RenderComponent<CampaignEdit>();

        cut.Find("#msf-subject").Change("trainee-elsewhere");
        _referenceData.CurriculumAskedFor.Should().BeEmpty();
        cut.FindAll("input[type=checkbox]").Should().BeEmpty();

        cut.Find("#msf-subject").Change(TraineeUserId);
        cut.WaitForState(() => cut.FindAll("input[type=checkbox]").Count > 0);
        _referenceData.CurriculumAskedFor.Should().Equal(TraineeUserId);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private const string SecretNarrative = "Colleagues describe a careful, kind registrar.";

    private void SignIn(string userId, string role)
    {
        _auth.SetAuthorized($"{userId}@test");
        _auth.SetRoles(role);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, userId));
    }

    private void StubTheEditorsLists()
        => _sender
            .On<ListMsfTemplatesQuery>(_ => new[] { new MsfTemplateDto(1, "Default MSF", null, false, true, []) })
            .On<ListTraineesForSpecialityQuery>(_ => new[] { Trainee() });

    private static string? CallerOf(ClaimsPrincipal principal)
        => principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    private static TraineeProfileDto Trainee()
        => new(1, TraineeUserId, "trainee@test", "Thandi", "Mokoena", 2, "Paediatrics", "11.1", 3, "Paediatrics", 4,
            "General Paediatrics", new DateOnly(2026, 1, 1), new DateOnly(2029, 12, 31), true);

    private static EntrustmentDecisionDto Decision(string traineeUserId)
        => new(DecisionId, traineeUserId, 101, "PAED-001", "Resuscitate a critically ill child", 3, "3a", 3,
            new DateOnly(2026, 7, 1), null, 30, "chair-1", "Consistent across the period.", EntrustmentDecisionStatus.Active,
            null, null, null, null, []);

    private static MsfCampaignSummaryDto Summary(string subjectUserId)
        => new(CampaignId, subjectUserId, "Default MSF", new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 15), 8, 3,
            MsfCampaignState.Released, 9, 9, new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc));

    private static MsfCampaignAggregateReportDto Report(string subjectUserId, MsfCampaignState state)
        => new(CampaignId, subjectUserId, "Default MSF", state, 8, 3, 9, SecretNarrative, true, [], 2, 2,
            [new MsfCoveredEpaDto(101, "PAED-001", "Resuscitate a critically ill child", false)], null, null);

    private sealed class RecordingReferenceData : StubActivityReferenceDataService
    {
        public List<string> CurriculumAskedFor { get; } = [];

        public override Task<IReadOnlyList<ActivityCatalogueOption>> GetSubjectCurriculumEpaOptionsAsync(
            string subjectUserId, CancellationToken cancellationToken = default)
        {
            CurriculumAskedFor.Add(subjectUserId);
            return Task.FromResult<IReadOnlyList<ActivityCatalogueOption>>([new("101", "PAED-001 Resuscitate a critically ill child")]);
        }
    }

    /// <summary>Answers what a test registers, records every request, and refuses anything unregistered.</summary>
    private sealed class RecordingSender : IScopedSender
    {
        private readonly Dictionary<Type, Func<object, object?>> _answers = [];

        public List<object> Received { get; } = [];

        public RecordingSender On<TRequest>(Func<TRequest, object?> answer)
        {
            _answers[typeof(TRequest)] = request => answer((TRequest)request);
            return this;
        }

        public TRequest Single<TRequest>()
            => Received.OfType<TRequest>().Should().ContainSingle().Which;

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => Task.FromResult((TResponse)Answer(request)!);

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
        {
            Answer(request);
            return Task.CompletedTask;
        }

        private object? Answer(object request)
        {
            Received.Add(request);

            return _answers.TryGetValue(request.GetType(), out var answer)
                ? answer(request)
                : throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
        }
    }
}
