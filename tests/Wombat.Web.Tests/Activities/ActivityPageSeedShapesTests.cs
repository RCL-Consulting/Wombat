using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Commands.TransitionActivity;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityById;
using Wombat.Application.Features.Activities.Services;
using Wombat.Web.Components.Pages.Activities;
using Wombat.Web.Components.Shared.Activities;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// T342 step 6, the build review's fix pass: the activity page on the seeds that are not the CPSA Mini-CEX, each on its own
/// form and workflow. G1: Cancel offered to whoever the server offers it (the demo types' <c>subject|field:assessor_user_id</c>).
/// G3: a create that is itself the filing (a born-requested demo request, a born-logged journal club). D5: the subtitles as
/// drawn. And the status card's moves in words ("record the discussion", the runbook lane's finding).
/// </summary>
public sealed class ActivityPageSeedShapesTests : TestContext
{
    private const string Subject = "trainee-1";
    private const string Assessor = "assessor-1";

    private static readonly DateTime Created = new(2026, 9, 29, 7, 10, 0, DateTimeKind.Utc);
    private static readonly DateTime Moved = new(2026, 9, 29, 9, 5, 0, DateTimeKind.Utc);

    private readonly TestAuthorizationContext _auth;

    public ActivityPageSeedShapesTests()
    {
        _auth = this.AddTestAuthorization();
        SignIn(Subject);
        Services.AddSingleton<IActivityReferenceDataService, StubActivityReferenceDataService>();
        Services.AddScoped<ActivityNotices>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    // ---- G1: the demo Mini-CEX, born requested, whose cancel is subject|field:assessor_user_id ----

    [Fact]
    public void TheAssessorOfADemoRequest_IsOfferedCancel_LastInTheBar_InTheRequestsWords()
    {
        SignIn(Assessor);
        var detail = DemoRequest(
            holder: new ActivityHolderDto(ActivityHolderKind.Person, Assessor, "Fatima Khumalo", true, Created),
            actions: [Action("accept"), Action("decline", requiresNote: true), Action("cancel")]);
        var sender = new PageSender(detail, detail with { Activity = detail.Activity with { CurrentState = "cancelled", CurrentStateLabel = "Cancelled" } });
        var cut = Render(sender);

        var bar = cut.FindAll(".activity-moves > .form-actions--moves > button").Select(button => button.TextContent.Trim()).ToList();
        bar.Should().Equal("Accept", "Decline", "Cancel request…");
        cut.FindAll(".activity-status-action").Should().BeEmpty("the viewer holds it: Cancel is in their bar, not on the card");

        var dialog = cut.Find("dialog");
        dialog.QuerySelector("h2")!.TextContent.Should().Be("Cancel this request?");
        dialog.QuerySelector("p")!.TextContent.Should().Be(
            "It leaves your Activity inbox. A cancelled request cannot be reopened, and it credits nothing.");

        cut.FindAll("button").First(button => button.TextContent.Trim() == "Cancel request…").Click();
        cut.Find("dialog .btn-danger").Click();

        sender.Transitions.Should().ContainSingle().Which.TransitionKey.Should().Be("cancel");
    }

    [Fact]
    public void ItsRegistrar_WhileTheAssessorHoldsIt_HasCancelOnTheCard()
    {
        var model = ActivityPageModel.From(DemoRequest(
            holder: new ActivityHolderDto(ActivityHolderKind.Person, Assessor, "Fatima Khumalo", false, Created),
            actions: [Action("cancel")]), Subject);

        model.CancelOnCard.Should().BeTrue();
        model.BarCancel.Should().BeNull();
        model.Status(null).Action.Should().Be(ActivityStatusAction.Cancel);
        model.CancelLabel.Should().Be("Cancel request…", "a request born requested is not a draft");
    }

    // ---- G3: a create that is itself the filing ----

    [Fact]
    public void ADemoRequest_WasFiledByItsCreate_SoItsLatenessIsShown_AndCancelledItWasSubmitted()
    {
        var requested = ActivityPageModel.From(DemoRequest(
            holder: new ActivityHolderDto(ActivityHolderKind.Person, Assessor, "Fatima Khumalo", false, Created),
            actions: [], createDaysAfter: 20), Subject);

        requested.Filing!.TransitionKey.Should().Be("create");
        requested.Status(null).Meta.Should().Be("Filed 20 days after the encounter: recorded as late.");
        requested.About().Single(row => row.Label == "Filed").Value.Should().Be("2026-09-29, 20 days after the encounter (late)");
        requested.Subtitle.Should().Be("Sipho Ndlovu's request to Fatima Khumalo");

        var cancelled = ActivityPageModel.From(DemoRequest(
            state: "cancelled", stateLabel: "Cancelled",
            holder: new ActivityHolderDto(ActivityHolderKind.Closed, null, null, false, Moved),
            actions: [],
            extra: [Row(2, "requested", "cancelled", "cancel", "Requested", "Cancelled", Subject, "Sipho Ndlovu", Moved)]), Subject);

        cancelled.Status(null).Body.Should().Be("It credits nothing, and nothing more can happen to it.", "it was submitted: its create filed it");
        cancelled.Subtitle.Should().Be("Sipho Ndlovu's request to Fatima Khumalo");
    }

    [Fact]
    public void ABornLoggedJournalClub_IsNotADraft()
    {
        var model = ActivityPageModel.From(Detail(
            "journal_club", "Journal Club", "logged", "Logged",
            new ActivityHolderDto(ActivityHolderKind.Done, null, null, false, Created),
            [CreateRow("logged", "Logged")], [],
            """{"session_date":"2026-09-20"}""", nominee: null), Subject);

        model.Filing!.TransitionKey.Should().Be("create");
        model.Subtitle.Should().Be("Sipho Ndlovu's journal club");
    }

    // ---- D5: the subtitles as drawn ----

    [Fact]
    public void ACancelledDraft_IsTheDraft_Cancelled()
    {
        var model = ActivityPageModel.From(Detail(
            "dops_cpsa", "DOPS (Paediatrics)", "cancelled", "Cancelled",
            new ActivityHolderDto(ActivityHolderKind.Closed, null, null, false, Moved),
            [CreateRow("draft", "Draft"), Row(2, "draft", "cancelled", "cancel", "Draft", "Cancelled", Subject, "Sipho Ndlovu", Moved)],
            [], "{}", nominee: null), Subject);

        model.Subtitle.Should().Be("Sipho Ndlovu's draft, cancelled");
        model.Status(null).Body.Should().Be("It was never submitted, and it credits nothing.");
    }

    [Fact]
    public void AReflection_IsForDiscussion_AndAReview_IsARequest()
    {
        var reflection = ActivityPageModel.From(Detail(
            "reflective_exercise_cpsa", "Reflective Exercise (Paediatrics)", "submitted", "Awaiting discussion",
            new ActivityHolderDto(ActivityHolderKind.Person, "sup-1", "Sarah Botha", false, Moved),
            [CreateRow("draft", "Draft"), Row(2, "draft", "submitted", "submit", "Draft", "Awaiting discussion", Subject, "Sipho Ndlovu", Moved)],
            [], "{}", nominee: "Sarah Botha"), Subject);
        var review = ActivityPageModel.From(Detail(
            "portfolio_review_cpsa", "Portfolio and Logbook Review (Paediatrics)", "submitted", "Awaiting review",
            new ActivityHolderDto(ActivityHolderKind.Person, "sup-2", "Mohammed Patel", false, Moved),
            [CreateRow("draft", "Draft"), Row(2, "draft", "submitted", "submit", "Draft", "Awaiting review", Subject, "Sipho Ndlovu", Moved)],
            [], "{}", nominee: "Mohammed Patel"), Subject);

        reflection.Subtitle.Should().Be("Sipho Ndlovu's reflection, for discussion with Sarah Botha");
        review.Subtitle.Should().Be("Sipho Ndlovu's request to Mohammed Patel");
    }

    // ---- the status card's moves, each a verb phrase ----

    [Fact]
    public void TheSupervisorsMoves_ReadAsVerbs_RecordTheDiscussion_OrReturnIt()
    {
        var model = ActivityPageModel.From(Detail(
            "reflective_exercise_cpsa", "Reflective Exercise (Paediatrics)", "submitted", "Awaiting discussion",
            new ActivityHolderDto(ActivityHolderKind.Person, "sup-1", "Sarah Botha", true, Moved),
            [CreateRow("draft", "Draft"), Row(2, "draft", "submitted", "submit", "Draft", "Awaiting discussion", Subject, "Sipho Ndlovu", Moved)],
            [Action("record_discussion"), Action("return", requiresNote: true), Action("cancel")], "{}", nominee: "Sarah Botha"), "sup-1");

        model.Status(null).Body.Should().Be("Record the discussion, or return it with a note Sipho Ndlovu will read.");
    }

    [Theory]
    [InlineData("complete", "complete it")]
    [InlineData("sign_off", "sign it off")]
    [InlineData("record_discussion", "record the discussion")]
    [InlineData("a_builders_own_move", "act on it")]
    public void MovePhrase_NamesEachSeededMove_AndFallsBackForAnyOther(string key, string phrase)
        => ActivityPageModel.MovePhrase(key).Should().Be(phrase);

    // ---- helpers ----

    private void SignIn(string userId)
    {
        _auth.SetAuthorized(userId);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, userId));
    }

    private IRenderedComponent<ActivityView> Render(PageSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<ActivityView>(parameters => parameters.Add(page => page.ActivityId, 7));
        cut.WaitForState(() => cut.Markup.Contains("Who has it now"));
        return cut;
    }

    private static ActivityActionDto Action(string key, bool requiresNote = false)
        => new(key, requiresNote) { TargetStateLabel = char.ToUpperInvariant(key[0]) + key[1..] };

    private static ActivityTransitionDto Row(int id, string from, string to, string key, string fromLabel, string toLabel,
        string actor, string actorName, DateTime at, int? daysAfter = null)
        => new(id, from, to, key, fromLabel, toLabel, key == "create" ? "Create" : char.ToUpperInvariant(key[0]) + key[1..],
            actor, at, null, "{}", null, null, daysAfter) { ActorName = actorName };

    private static ActivityTransitionDto CreateRow(string state, string label, int? daysAfter = null)
        => Row(1, state, state, "create", label, label, Subject, "Sipho Ndlovu", Created, daysAfter);

    private static ActivityDetailDto DemoRequest(
        ActivityHolderDto holder,
        IReadOnlyList<ActivityActionDto> actions,
        string state = "requested",
        string stateLabel = "Requested",
        int? createDaysAfter = null,
        IReadOnlyList<ActivityTransitionDto>? extra = null)
        => Detail("mini_cex", "Mini-CEX", state, stateLabel, holder,
            [CreateRow("requested", "Requested", createDaysAfter), .. extra ?? []], actions,
            """{"epa_id":"12","assessor_user_id":"assessor-1","observed_on":"2026-09-09"}""", nominee: "Fatima Khumalo");

    private static ActivityDetailDto Detail(
        string seed, string typeName, string state, string stateLabel, ActivityHolderDto holder,
        IReadOnlyList<ActivityTransitionDto> transitions, IReadOnlyList<ActivityActionDto> actions, string dataJson,
        string? nominee)
    {
        var activity = new ActivityDto(
            7, 2, seed, typeName, null, 1,
            SeedSchemas.Schema(seed), SeedSchemas.Workflow(seed), "[]", SeedSchemas.CreditRules(seed) ?? "{}",
            Subject, 1, Subject, state, stateLabel, dataJson, null, null,
            new DateOnly(2026, 9, 9), true, Created, Moved, transitions);

        return new ActivityDetailDto(activity, [], actions)
        {
            Holder = holder,
            NomineeName = nominee,
            DisplayName = $"{typeName} · 2026-09-09",
            SubjectName = "Sipho Ndlovu"
        };
    }

    private sealed class PageSender(params ActivityDetailDto[] details) : IScopedSender
    {
        private int _loads;

        public List<TransitionActivityCommand> Transitions { get; } = [];

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case GetActivityByIdQuery:
                    var detail = details[Math.Min(_loads, details.Length - 1)];
                    _loads++;
                    return Task.FromResult((TResponse)(object)detail);
                case TransitionActivityCommand transition:
                    Transitions.Add(transition);
                    return Task.FromResult((TResponse)(object)details[^1].Activity);
                default:
                    return Task.FromResult(default(TResponse)!);
            }
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
