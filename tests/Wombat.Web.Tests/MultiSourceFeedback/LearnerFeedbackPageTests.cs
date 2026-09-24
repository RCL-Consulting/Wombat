using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Features.Trainees;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Web.Components.Pages.MultiSourceFeedback;
using Wombat.Web.Services;
using Wombat.Web.Tests.Accessibility;
using Wombat.Web.Tests.Activities;

namespace Wombat.Web.Tests.MultiSourceFeedback;

/// <summary>
/// The MSF pages running learner feedback (T164, D35): a coordinator picks the template kind, a learner-feedback campaign
/// is offered only the EPAs whose list names it and only the Learner category, each learner is invited with the teaching
/// context they were taught in, and the report counts the contexts that answered and offers no level.
/// </summary>
public sealed class LearnerFeedbackPageTests : TestContext
{
    private const string TraineeUserId = "trainee-1";
    private const int MsfTemplateId = 1;
    private const int LearnerFeedbackTemplateId = 2;

    private static readonly IReadOnlyList<ActivityCatalogueOption> WholeCurriculum =
    [
        new("101", "PAED-001 — Resuscitate a critically ill child"),
        new("102", "PAED-002 — Manage a child with a chronic condition"),
        new("115", "PAED-015 — Teach and supervise")
    ];

    private static readonly IReadOnlyList<ActivityCatalogueOption> NamesLearnerFeedback = [new("115", "PAED-015 — Teach and supervise")];

    private readonly RecordingSender _sender = new();
    private readonly ReferenceData _referenceData = new();
    private readonly TestAuthorizationContext _auth;

    public LearnerFeedbackPageTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("coordinator@test");
        auth.SetRoles(WombatRoles.Coordinator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "coordinator-1"));
        _auth = auth;

        Services.AddSingleton<IScopedSender>(_sender);
        Services.AddSingleton<IActivityReferenceDataService>(_referenceData);
    }

    // ─── Creating a campaign ─────────────────────────────────────────────────

    [Fact]
    public void ChoosingALearnerFeedbackTemplate_OffersOnlyTheEpasItsListNames_AndOneRespondentGroup()
    {
        var cut = RenderComponent<CampaignEdit>();
        cut.Find("#msf-subject").Change(TraineeUserId);
        cut.WaitForState(() => cut.FindAll("input[type=checkbox]").Count == WholeCurriculum.Count);

        cut.Find("#msf-template-id").Change(LearnerFeedbackTemplateId.ToString());
        cut.WaitForState(() => cut.FindAll("input[type=checkbox]").Count == 1);

        // The same question the create command and the release ask: the curriculum, narrowed by the instrument.
        _referenceData.Asked.Last().Should().Be((TraineeUserId, "learner_feedback"));
        cut.Find("#msf-epa-115");
        cut.Find("#msf-epas-help").TextContent.Should().Contain("PAED-015");

        var categories = cut.Find("#msf-min-category-count");
        categories.GetAttribute("value").Should().Be("1");
        categories.HasAttribute("disabled").Should().BeTrue("learner feedback has one respondent group, learners");

        cut.Find("#msf-template-id").Change(MsfTemplateId.ToString());
        cut.WaitForState(() => cut.FindAll("input[type=checkbox]").Count == WholeCurriculum.Count);

        _referenceData.Asked.Last().Should().Be((TraineeUserId, (string?)null), "an MSF reads the curriculum unnarrowed, as before");
        cut.Find("#msf-min-category-count").GetAttribute("value").Should().Be("2");
        cut.Find("#msf-min-category-count").HasAttribute("disabled").Should().BeFalse();
    }

    /// <summary>
    /// T164 review: the response minimum follows the template while it is the other kind's default. Left at MSF's eight, a
    /// learner-feedback campaign of three learners could never be released. A minimum the coordinator typed is kept.
    /// </summary>
    [Fact]
    public void ChoosingALearnerFeedbackTemplate_StartsTheResponseMinimumAtThree_AndKeepsATypedOne()
    {
        var cut = RenderComponent<CampaignEdit>();
        cut.Find("#msf-min-responses").GetAttribute("value").Should().Be("8");

        cut.Find("#msf-template-id").Change(LearnerFeedbackTemplateId.ToString());
        cut.Find("#msf-min-responses").GetAttribute("value").Should().Be("3");

        cut.Find("#msf-template-id").Change(MsfTemplateId.ToString());
        cut.Find("#msf-min-responses").GetAttribute("value").Should().Be("8");

        cut.Find("#msf-min-responses").Change("5");
        cut.Find("#msf-template-id").Change(LearnerFeedbackTemplateId.ToString());
        cut.Find("#msf-min-responses").GetAttribute("value").Should().Be("5", "the coordinator typed it");
    }

    [Fact]
    public void TheTemplateList_MarksALearnerFeedbackTemplate()
    {
        var cut = RenderComponent<CampaignEdit>();

        cut.FindAll("#msf-template-id option").Select(option => option.TextContent.Trim())
            .Should().Equal("Select template", "Annual MSF", "Teaching feedback (Learner feedback)");
    }

    [Fact]
    public void TheQuickTemplateForm_CreatesALearnerFeedbackTemplate_WithWordingLabelledInterim()
    {
        _sender.On<CreateMsfTemplateCommand>(command => new MsfTemplateDto(3, command.Name, null, false, true, [], command.Kind));
        var cut = RenderComponent<CampaignEdit>();

        cut.Find("#msf-template-kind").Change(MsfTemplateKind.LearnerFeedback.ToString());

        cut.Find("#msf-template-name").GetAttribute("value").Should().Be("Learner feedback (interim questionnaire)");
        cut.Find("#msf-template-scale").GetAttribute("value").Should().Contain("teaching");

        cut.Find("#msf-template-name").Closest("form")!.Submit();
        cut.WaitForState(() => _sender.Received.OfType<CreateMsfTemplateCommand>().Any());

        var command = _sender.Received.OfType<CreateMsfTemplateCommand>().Single();
        command.Kind.Should().Be(MsfTemplateKind.LearnerFeedback);
        command.Name.Should().Be("Learner feedback (interim questionnaire)");
        command.AllowPatientResponses.Should().BeFalse();
    }

    // ─── Inviting ────────────────────────────────────────────────────────────

    [Fact]
    public void ALearnerFeedbackCampaign_OffersOnlyLearners_AndInvitesEachWithTheirTeachingContext()
    {
        _sender
            .On<GetMsfCampaignSetupQuery>(query => Setup(query.CampaignId, MsfTemplateKind.LearnerFeedback, [MsfRespondentCategory.Learner]))
            .On<AddMsfInvitationCommand>(_ => 17);
        var cut = RenderComponent<CampaignEdit>(parameters => parameters.Add(page => page.CampaignId, 9));
        cut.WaitForState(() => cut.FindAll("#msf-teaching-context").Count == 1);

        cut.FindAll("#msf-respondent-category option").Select(option => option.TextContent.Trim()).Should().Equal("Learner");
        cut.Markup.Should().Contain("Learner feedback");
        IdReferences.Broken(cut).Should().BeEmpty();

        cut.Find("#msf-respondent-email").Change("student-1@example.test");
        cut.Find("#msf-teaching-context").Change("Ward round");
        cut.Find("#msf-respondent-email").Closest("form")!.Submit();
        cut.WaitForState(() => cut.Markup.Contains("Invitee added"));

        var add = _sender.Received.OfType<AddMsfInvitationCommand>().Single();
        add.RespondentCategory.Should().Be(MsfRespondentCategory.Learner);
        add.TeachingContext.Should().Be("Ward round");

        // The next learner is usually from the same group.
        cut.Find("#msf-teaching-context").GetAttribute("value").Should().Be("Ward round");
    }

    [Fact]
    public void AnMsfCampaign_OffersNoLearner_AndAsksNoTeachingContext()
    {
        _sender
            .On<GetMsfCampaignSetupQuery>(query => Setup(
                query.CampaignId, MsfTemplateKind.Msf, [MsfRespondentCategory.PeerDoctor, MsfRespondentCategory.Nurse]))
            .On<AddMsfInvitationCommand>(_ => 17);
        var cut = RenderComponent<CampaignEdit>(parameters => parameters.Add(page => page.CampaignId, 9));
        cut.WaitForState(() => cut.FindAll("#msf-respondent-category option").Count == 2);

        cut.FindAll("#msf-respondent-category option").Select(option => option.TextContent.Trim()).Should().Equal("PeerDoctor", "Nurse");
        cut.FindAll("#msf-teaching-context").Should().BeEmpty();

        cut.Find("#msf-respondent-email").Change("nurse-1@example.test");
        cut.Find("#msf-respondent-email").Closest("form")!.Submit();
        cut.WaitForState(() => cut.Markup.Contains("Invitee added"));

        _sender.Received.OfType<AddMsfInvitationCommand>().Single().TeachingContext.Should().BeNull();
    }

    // ─── The report ──────────────────────────────────────────────────────────

    [Fact]
    public void ALearnerFeedbackReport_CountsTheContextsThatAnswered_AndOffersNoLevel()
    {
        _sender.On<GetCampaignAggregateReportQuery>(_ => Report(MsfTemplateKind.LearnerFeedback, ["Student tutorial", "Ward round"]));
        var cut = RenderComponent<CampaignReport>(parameters => parameters.Add(page => page.CampaignId, 9));
        cut.WaitForState(() => cut.FindAll("#msf-narrative").Count > 0);

        cut.Markup.Should().Contain("Teaching contexts that responded:</strong> 2 (Student tutorial, Ward round)");
        cut.Markup.Should().Contain("never their names");
        cut.Markup.Should().Contain("Kind:</strong> Learner feedback");
        cut.FindAll("#msf-level").Should().BeEmpty("learner_feedback_cpsa rates nothing, and its release refuses a level");
        _referenceData.RatedLevelsAskedFor.Should().Equal("learner_feedback_cpsa");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void AnMsfReport_StillOffersTheLevel_AndCountsNoContexts()
    {
        _sender.On<GetCampaignAggregateReportQuery>(_ => Report(MsfTemplateKind.Msf, null));
        var cut = RenderComponent<CampaignReport>(parameters => parameters.Add(page => page.CampaignId, 9));
        cut.WaitForState(() => cut.FindAll("#msf-level").Count > 0);

        cut.Markup.Should().NotContain("Teaching contexts");
        _referenceData.RatedLevelsAskedFor.Should().Equal("msf_cpsa");
    }

    /// <summary>
    /// T164 review: the trainee's own report page counts the teaching contexts and names none, whatever it is handed. The
    /// handler already leaves the names out of the trainee's copy; the page does not print them either.
    /// </summary>
    [Fact]
    public void TheTraineesReportPage_CountsTheTeachingContexts_AndNamesNone()
    {
        _auth.SetRoles(WombatRoles.Trainee);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, TraineeUserId));
        _sender
            .On<ListMsfCampaignsForTraineeQuery>(_ => Array.Empty<MsfCampaignSummaryDto>())
            .On<GetCampaignAggregateReportQuery>(_ => Report(MsfTemplateKind.LearnerFeedback, ["Neonatal night teaching", "Ward round"])
                with { State = MsfCampaignState.Released });
        var cut = RenderComponent<MyMsfReports>(parameters => parameters.Add(page => page.CampaignId, 9));
        cut.WaitForState(() => cut.Markup.Contains("Selected report"));

        cut.Markup.Should().Contain("Teaching contexts that responded:</strong> 2");
        cut.Markup.Should().NotContain("Neonatal night teaching").And.NotContain("Ward round");
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static MsfCampaignSetupDto Setup(int campaignId, MsfTemplateKind kind, IReadOnlyList<MsfRespondentCategory> categories)
        => new(campaignId, kind == MsfTemplateKind.LearnerFeedback ? "Teaching feedback" : "Annual MSF", kind, MsfCampaignState.Draft, categories);

    private static MsfCampaignAggregateReportDto Report(MsfTemplateKind kind, IReadOnlyList<string>? contexts)
        => new(9, TraineeUserId, "Teaching feedback", MsfCampaignState.UnderReview, 3, 3, 3, null, true, [], 1, 1,
            [new MsfCoveredEpaDto(115, "PAED-015", "Teach and supervise", false)], null, null, kind, contexts, contexts?.Count);

    private static TraineeProfileDto Trainee()
        => new(1, TraineeUserId, "trainee@test", "Thandi", "Mokoena", 2, "Paediatrics", "11.1", 3, "Paediatrics", 4,
            "General Paediatrics", new DateOnly(2026, 1, 1), new DateOnly(2029, 12, 31), true);

    private sealed class ReferenceData : StubActivityReferenceDataService
    {
        public List<(string SubjectUserId, string? PermittedToolKey)> Asked { get; } = [];

        public List<string> RatedLevelsAskedFor { get; } = [];

        public override Task<IReadOnlyList<ActivityCatalogueOption>> GetSubjectCurriculumEpaOptionsAsync(
            string subjectUserId, string? permittedToolKey = null, CancellationToken cancellationToken = default)
        {
            Asked.Add((subjectUserId, permittedToolKey));
            return Task.FromResult(permittedToolKey == "learner_feedback" ? NamesLearnerFeedback : WholeCurriculum);
        }

        public override Task<IReadOnlyList<ActivityCatalogueOption>> GetRatedLevelOptionsForActivityTypeAsync(
            string activityTypeKey, CancellationToken cancellationToken = default)
        {
            RatedLevelsAskedFor.Add(activityTypeKey);
            return Task.FromResult<IReadOnlyList<ActivityCatalogueOption>>(activityTypeKey == "msf_cpsa"
                ? [new("1", "1"), new("2", "2")]
                : []);
        }
    }

    /// <summary>Answers the editor's lists, whatever a test registers, and records every request.</summary>
    private sealed class RecordingSender : IScopedSender
    {
        private readonly Dictionary<Type, Func<object, object?>> _answers = [];

        public RecordingSender()
        {
            On<ListMsfTemplatesQuery>(_ => new MsfTemplateDto[]
            {
                new(MsfTemplateId, "Annual MSF", null, false, true, []),
                new(LearnerFeedbackTemplateId, "Teaching feedback", null, false, true, [], MsfTemplateKind.LearnerFeedback)
            });
            On<ListTraineesForSpecialityQuery>(_ => new[] { Trainee() });
        }

        public List<object> Received { get; } = [];

        public RecordingSender On<TRequest>(Func<TRequest, object?> answer)
        {
            _answers[typeof(TRequest)] = request => answer((TRequest)request);
            return this;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Received.Add(request);
            return _answers.TryGetValue(request.GetType(), out var answer)
                ? Task.FromResult((TResponse)answer(request)!)
                : throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
        {
            Received.Add(request);
            return _answers.ContainsKey(request.GetType())
                ? Task.CompletedTask
                : throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
        }
    }
}
