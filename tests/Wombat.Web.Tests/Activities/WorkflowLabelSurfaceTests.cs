using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.GetActivityById;
using Wombat.Application.Features.Activities.Queries.ListWaitingForYou;
using Wombat.Application.Features.Activities.Queries.ListActivitiesBySubject;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.Dashboards.Assessor;
using Wombat.Application.Features.Dashboards.Trainee;
using Wombat.Web.Components.Pages.Activities;
using Wombat.Web.Components.Pages.Dashboards;
using Wombat.Web.Services;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// T220: every page that shows an activity's workflow state or a recorded move shows it by the label the query carried
/// from the activity's PINNED workflow, never by the stored key, so a page names a state as the refusals and notices on
/// it do (T189). Each DTO here carries a key and a label that differ, as <c>clinical_audit_cpsa</c>'s <c>submitted</c>
/// ("Awaiting supervisor") does, so a surface that printed the key would show it.
/// </summary>
public sealed class WorkflowLabelSurfaceTests : TestContext
{
    private const string SchemaJson = """
        {
          "version": 1,
          "sections": [
            { "key": "s", "title": "Audit", "fields": [ { "key": "audit_title", "type": "text", "label": "Audit title" } ] }
          ]
        }
        """;

    private const string WorkflowJson = """
        {
          "version": 1,
          "initial_state": "draft",
          "states": [
            { "key": "draft", "label": "Draft" },
            { "key": "submitted", "label": "Awaiting supervisor" },
            { "key": "signed_off", "label": "Signed off", "terminal": true }
          ],
          "transitions": [
            { "key": "submit", "from": "draft", "to": "submitted", "actor": "subject" },
            { "key": "sign_off", "from": "submitted", "to": "signed_off", "actor": "role:Assessor" }
          ]
        }
        """;

    private readonly TestAuthorizationContext _auth;

    public WorkflowLabelSurfaceTests()
    {
        _auth = this.AddTestAuthorization();
        _auth.SetAuthorized("trainee@test");
        _auth.SetRoles("Trainee");
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "trainee-1"));

        Services.AddSingleton<IActivityReferenceDataService, StubActivityReferenceDataService>();
        Services.AddScoped<ActivityNotices>();
    }

    // ---- the activity's own page -------------------------------------------------------------------------------------

    [Fact]
    public void TheActivityPage_NamesItsStateByItsLabel_InTheHeaderAndTheSummary()
    {
        var cut = RenderActivity(SubmittedAudit());

        // T342 (Q3): the status card's badge says the state, by its label; the header's "State:" line is gone.
        Text(cut.Find(".activity-status .badge")).Should().Be("Awaiting supervisor");
        Text(cut.Find(".page-subtitle")).Should().NotContain("State:");
    }

    /// <summary>
    /// T190: the page and its tab are named for the activity's type, in the same words. Every activity's tab read just
    /// "Activity" before.
    /// </summary>
    [Fact]
    public void TheActivityPage_IsNamedForItsType_InItsHeadingAndItsTab()
    {
        var cut = RenderActivity(SubmittedAudit());

        Text(cut.Find("h1")).Should().Be("Clinical Audit (Paediatrics)");
        TabTitle.Of(this, cut).Should().Be("Clinical Audit (Paediatrics) · Wombat");
    }

    [Fact]
    public void TheActivityPage_NamesEachRecordedMoveAndItsStatesByTheirLabels()
    {
        var cut = RenderActivity(SubmittedAudit());

        var history = cut.Find("table.history-table");
        var rows = history.QuerySelectorAll("tbody tr");
        Cells(history, rows, "Move").Should().Equal("Create", "Submit");
        // T342 (R3): the create row comes from nothing.
        Cells(history, rows, "From → to").Should().Equal("— → Draft", "Draft → Awaiting supervisor");
    }

    [Fact]
    public void TheActivityPage_NeverPrintsTheStateOrMoveKeys()
    {
        var cut = RenderActivity(SubmittedAudit());

        var visible = cut.Find(".header-container").TextContent + string.Concat(cut.FindAll(".detail-card").Select(card => card.TextContent));
        visible.Should().NotContain("submitted").And.NotContain("draft →").And.NotContain("→ draft");
    }

    // ---- the lists ---------------------------------------------------------------------------------------------------

    [Fact]
    public void MyActivities_ShowsTheStateByItsLabel()
    {
        Services.AddSingleton<IScopedSender>(new FakeSender()
            .On<ListActivitiesBySubjectQuery>(_ => new ActivityListPageDto(ListRows(), 1, 20, ListRows().Count))
            .On<Wombat.Application.Features.Activities.Queries.ListNeedsYou.ListNeedsYouQuery>(_ => (IReadOnlyList<ActivitySummaryDto>)[]));
        var cut = RenderComponent<MyActivities>();
        cut.WaitForState(() => cut.FindAll("tbody tr").Count == 1);

        StateColumn(cut).Should().Equal("Awaiting supervisor");
    }

    [Fact]
    public void TheInbox_ShowsTheStateByItsLabel()
    {
        Services.AddSingleton<IScopedSender>(new FakeSender()
            .On<ListWaitingForYouQuery>(_ => new WaitingForYouDto(ListRows(), 0, 7))
            .On<Wombat.Application.Features.Activities.Queries.ListDecidedByYou.ListDecidedByYouQuery>(_ => new ActivityListPageDto([], 1, 20, 0)));
        var cut = RenderComponent<ActivityInbox>();
        cut.WaitForState(() => cut.FindAll("tbody tr").Count == 1);

        StateColumn(cut).Should().Equal("Awaiting supervisor");
    }

    // ---- the dashboards ----------------------------------------------------------------------------------------------

    [Fact]
    public void TheTraineeDashboard_BadgesEachStateByItsLabel_ColouredByItsKey()
    {
        Services.AddSingleton<IScopedSender>(new FakeSender().On<GetTraineeDashboardSummaryQuery>(_ => new TraineeDashboardSummaryDto(
            null,
            // T342: Home's card is Needs you, the rows ListNeedsYouQuery reads.
            [TestSupport.ActivityRows.Row(41, typeName: "Clinical Audit (Paediatrics)")],
            // T355: Recent decisions, in place of Recent activities (Q3).
            [
                ActivityRows.Decided(42, "Clinical Audit (Paediatrics)", "Thandi Nkosi", "signed_off", "Signed off", isFinished: true, new DateTime(2026, 3, 20, 8, 0, 0, DateTimeKind.Utc)),
                ActivityRows.Decided(44, "Teaching session", "Thandi Nkosi", "accepted", "Accepted", isFinished: true, new DateTime(2026, 3, 19, 8, 0, 0, DateTimeKind.Utc))
            ],
            null,
            IsPendingTrainee: false)));

        var cut = RenderComponent<TraineeDashboard>();
        cut.WaitForState(() => cut.FindAll(".badge").Count >= 3);

        var inbox = BadgeFor(cut, 41);
        Text(inbox).Should().Be("Draft");
        inbox.ClassList.Should().Contain("badge-draft");

        var recent = BadgeFor(cut, 42);
        Text(recent).Should().Be("Signed off");
        recent.ClassList.Should().Contain("badge-completed", "a finished state is green, whatever its key (BadgeFor, T266)");

        // A teaching session finishes in "accepted": done, so green, not a supervisor's work in hand (D44, T266 review).
        BadgeFor(cut, 44).ClassList.Should().Contain("badge-completed").And.NotContain("badge-accepted");
    }

    [Fact]
    public void TheAssessorDashboard_BadgesEachDecisionByItsLabel()
    {
        _auth.SetRoles("Assessor");
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "assessor-1"));
        Services.AddSingleton<IScopedSender>(new FakeSender().On<GetAssessorDashboardSummaryQuery>(_ => ActivityRows.AssessorHome(
            [],
            [
                ActivityRows.Decided(43, "Clinical Audit (Paediatrics)", "Thandi Nkosi", "signed_off", "Signed off", isFinished: true, new DateTime(2026, 3, 21, 8, 0, 0, DateTimeKind.Utc)),
                ActivityRows.Decided(45, "Teaching session", "Thandi Nkosi", "accepted", "Accepted", isFinished: true, new DateTime(2026, 3, 20, 8, 0, 0, DateTimeKind.Utc)),
                ActivityRows.Decided(46, "Mini-CEX", "Thandi Nkosi", "declined", "Declined", isFinished: false, new DateTime(2026, 3, 19, 8, 0, 0, DateTimeKind.Utc))
            ])));

        var cut = RenderComponent<AssessorDashboard>();
        cut.WaitForState(() => cut.FindAll(".badge").Count >= 3);

        var badge = BadgeFor(cut, 43);
        Text(badge).Should().Be("Signed off");
        // One of the badges app.css defines (BadgeFor, T266): badge-signed_off, which it does not, was an untinted pill.
        badge.ClassList.Should().Contain("badge-completed").And.NotContain("badge-signed_off");

        // Done is the pinned workflow's terminal state, not the key's name (D44): a finished teaching session, which ends
        // in "accepted", was amber, the tint of work in hand (T266 review). A declined request is red.
        BadgeFor(cut, 45).ClassList.Should().Contain("badge-completed").And.NotContain("badge-accepted");
        BadgeFor(cut, 46).ClassList.Should().Contain("badge-declined");
    }

    /// <summary>
    /// The card of what waits on the assessor printed the key <c>accepted</c> as its badge's words (T220 review). It prints
    /// the state's label, coloured by the state's key since T297 put every waiting state on it, and says in words when the
    /// work is overdue: since T350 (note 14) in a badge of its own beside the state's, never in its place.
    /// </summary>
    [Fact]
    public void TheAssessorDashboard_BadgesAWaitingAssessmentByItsLabel_AndSaysWhenItIsOverdue()
    {
        _auth.SetRoles("Assessor");
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "assessor-1"));
        Services.AddSingleton<IScopedSender>(new FakeSender().On<GetAssessorDashboardSummaryQuery>(_ => ActivityRows.AssessorHome(
            [
                ActivityRows.Waiting(45, "Mini-CEX", "Thandi Nkosi", "requested", "Requested", overdue: true, since: new DateTime(2026, 1, 2, 8, 0, 0, DateTimeKind.Utc)),
                ActivityRows.Waiting(44, "Mini-CEX", "Thandi Nkosi", "requested", "Requested", since: new DateTime(2026, 3, 21, 8, 0, 0, DateTimeKind.Utc)),
                ActivityRows.Waiting(46, "Clinical Audit (Paediatrics)", "Thandi Nkosi", "submitted", "Awaiting supervisor", since: new DateTime(2026, 3, 22, 8, 0, 0, DateTimeKind.Utc))
            ],
            [])));

        var cut = RenderComponent<AssessorDashboard>();
        cut.WaitForState(() => cut.FindAll(".badge").Count >= 3);

        var onTime = BadgeFor(cut, 44);
        Text(onTime).Should().Be("Requested");
        onTime.ClassList.Should().Contain("badge-submitted", "a request waiting on its assessor is blue");
        var audit = BadgeFor(cut, 46);
        Text(audit).Should().Be("Awaiting supervisor");
        audit.ClassList.Should().Contain("badge-submitted");

        var overdue = BadgesFor(cut, 45);
        overdue.Select(Text).Should().Equal(["Requested", "Overdue"], "Overdue stands beside the state, never in its place");
        overdue[0].ClassList.Should().Contain("badge-submitted");
        overdue[1].ClassList.Should().Contain("badge-overdue", "overdue work wants attention, in the warning tint");
    }

    // ---- helpers -----------------------------------------------------------------------------------------------------

    private static IReadOnlyList<ActivitySummaryDto> ListRows() =>
    [
        new ActivitySummaryDto(
            42,
            2,
            "clinical_audit_cpsa",
            "Clinical Audit (Paediatrics)",
            "trainee-1",
            "submitted",
            "Awaiting supervisor",
            new DateTime(2026, 3, 20, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 20, 9, 0, 0, DateTimeKind.Utc),
            5000,
            "PAED-001",
            "Quality improvement",
            true,
            new DateOnly(2026, 3, 10),
            true,
            null)
        {
            SubjectName = "Thandi Nkosi"
        }
    ];

    /// <summary>A clinical audit its author has submitted: the create row and the submit, as the service maps them.</summary>
    private static ActivityDetailDto SubmittedAudit()
    {
        var created = new DateTime(2026, 3, 20, 8, 0, 0, DateTimeKind.Utc);
        var activity = new ActivityDto(
            42,
            2,
            "clinical_audit_cpsa",
            "Clinical Audit (Paediatrics)",
            "clinical_audit",
            1,
            SchemaJson,
            WorkflowJson,
            "[]",
            """{ "counts_for": [] }""",
            "trainee-1",
            1,
            "trainee-1",
            "submitted",
            "Awaiting supervisor",
            """{ "audit_title": "Hand hygiene before resuscitation" }""",
            null,
            null,
            new DateOnly(2026, 3, 10),
            true,
            created,
            created.AddHours(1),
            [
                new ActivityTransitionDto(1, "draft", "draft", "create", "Draft", "Draft", "Create", "trainee-1", created, null, "{}", null, null, null)
                    { ActorName = "Thandi Nkosi" },
                new ActivityTransitionDto(2, "draft", "submitted", "submit", "Draft", "Awaiting supervisor", "Submit", "trainee-1", created.AddHours(1), null, "{}", null, null, null)
                    { ActorName = "Thandi Nkosi" }
            ]);

        // Submitted and waiting on the supervisor: nothing for its author to write or do. The read-only branch.
        return new ActivityDetailDto(activity, [], []);
    }

    private IRenderedComponent<ActivityView> RenderActivity(ActivityDetailDto detail)
    {
        Services.AddSingleton<IScopedSender>(new FakeSender().On<GetActivityByIdQuery>(_ => detail));
        var cut = RenderComponent<ActivityView>(parameters => parameters.Add(page => page.ActivityId, detail.Activity.Id));
        cut.WaitForState(() => cut.Markup.Contains("Who has it now"));
        return cut;
    }

    private static IReadOnlyList<string> StateColumn<T>(IRenderedComponent<T> cut) where T : Microsoft.AspNetCore.Components.IComponent
    {
        var table = cut.Find("table");
        return Cells(table, table.QuerySelectorAll("tbody tr"), "State");
    }

    /// <summary>The text of one column, found by its header, so a reordered table cannot pass by accident.</summary>
    private static IReadOnlyList<string> Cells(IElement table, IEnumerable<IElement> rows, string header)
    {
        var index = table.QuerySelectorAll("thead th").Select(cell => cell.TextContent.Trim()).ToList().IndexOf(header);
        index.Should().BeGreaterThanOrEqualTo(0, "the table must have a '{0}' column", header);
        return rows.Select(row => Text(row.QuerySelectorAll("td")[index])).ToList();
    }

    /// <summary>The badge on the list item that links to activity <paramref name="activityId" />.</summary>
    private static IElement BadgeFor(IRenderedFragment cut, int activityId)
        => cut.FindAll("li").Single(item => item.QuerySelector($"a[href='/activities/{activityId}']") is not null)
            .QuerySelector(".badge")!;

    /// <summary>Every badge on the list item that links to activity <paramref name="activityId" />, in order.</summary>
    private static List<IElement> BadgesFor(IRenderedFragment cut, int activityId)
        => cut.FindAll("li").Single(item => item.QuerySelector($"a[href='/activities/{activityId}']") is not null)
            .QuerySelectorAll(".badge").ToList();

    private static string Text(IElement element)
        => string.Join(" ", element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private sealed class FakeSender : IScopedSender
    {
        private readonly Dictionary<Type, Func<object, object?>> _answers = [];

        public FakeSender On<TRequest>(Func<TRequest, object?> answer)
        {
            _answers[typeof(TRequest)] = request => answer((TRequest)request);
            return this;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => _answers.TryGetValue(request.GetType(), out var answer)
                ? Task.FromResult((TResponse)answer(request)!)
                : throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
