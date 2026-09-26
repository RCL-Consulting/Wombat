using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Queries.ListActivitiesByActorInbox;
using Wombat.Application.Features.Dashboards.Assessor;
using Wombat.Application.Features.Dashboards.Trainee;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Workflow;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.Dashboards;

/// <summary>
/// T297: the Assessor's and the Trainee's cards list what the Activity Inbox they link to lists, for every shipped
/// workflow and every caller: the trainee's card the inbox's first rows, the assessor's the inbox's rows less their own
/// portfolio. Before T297 each card selected by literal state keys and disagreed with the inbox on every rated CPSA
/// instrument.
/// </summary>
/// <remarks>
/// Each seed's type is filed in every state its workflow declares, three ways: a trainee's own naming the assessor, one
/// the assessor raised for the trainee, and one naming the assessor who is also a trainee; plus that dual user's own,
/// naming another assessor. Stamped to one institution, speciality and sub-speciality, so a <c>scope:</c> rule has
/// something to match. The callers are a trainee, an assessor, the dual user, a SpecialityAdmin and a Coordinator.
/// </remarks>
public sealed class DashboardInboxParityTests
{
    private const int InstitutionId = 10;
    private const int SpecialityId = 5;
    private const int SubSpecialityId = 6;
    private const int TypeId = 1;

    private const string TraineeId = "trainee";
    private const string AssessorId = "assessor";
    private const string DualId = "assessor-trainee";
    private const string OtherAssessorId = "other-assessor";

    private static readonly DateTime Clock = DateTime.UtcNow;

    public static TheoryData<string> SeedKeys() => new(ShippedSeeds.Keys());

    [Fact]
    public void EveryShippedSeed_IsPlayed()
    {
        // The theory below is driven by the seed folder in the test output; an empty or missing folder would pass it.
        ShippedSeeds.Keys().Should().Contain(["mini_cex_cpsa", "portfolio_review_cpsa", "reflective_exercise_cpsa", "teaching_session", "msf_cpsa"]);
    }

    [Theory]
    [MemberData(nameof(SeedKeys))]
    public async Task EachCard_ListsWhatTheInboxLists_ForEveryCaller(string seedKey)
    {
        await using var db = NewContext();
        var workflow = WorkflowParser.Parse(ShippedSeeds.Workflow(seedKey));
        ShippedSeeds.AddType(db, TypeId, seedKey, seedKey);
        var fields = FieldNames(workflow);

        var id = 0;
        foreach (var state in workflow.States.Select(state => state.Key))
        {
            Add(db, ++id, state, subject: TraineeId, creator: TraineeId, named: AssessorId, fields);
            Add(db, ++id, state, subject: TraineeId, creator: AssessorId, named: AssessorId, fields);
            Add(db, ++id, state, subject: TraineeId, creator: TraineeId, named: DualId, fields);
            Add(db, ++id, state, subject: DualId, creator: DualId, named: OtherAssessorId, fields);
        }

        await db.SaveChangesAsync();

        var callers = new Dictionary<string, ClaimsPrincipal>
        {
            ["trainee"] = TestPrincipals.InRoles([WombatRoles.Trainee], TraineeId, InstitutionId),
            ["assessor"] = TestPrincipals.InRoles([WombatRoles.Assessor], AssessorId, InstitutionId),
            ["assessor who is also a trainee"] = TestPrincipals.InRoles([WombatRoles.Assessor, WombatRoles.Trainee], DualId, InstitutionId),
            ["speciality admin"] = TestPrincipals.InRoles([WombatRoles.SpecialityAdmin], "speciality-admin", InstitutionId, SpecialityId, SubSpecialityId),
            ["coordinator"] = TestPrincipals.InRoles([WombatRoles.Coordinator], "coordinator", InstitutionId)
        };

        foreach (var (who, principal) in callers)
        {
            var callerId = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var inbox = await InboxAsync(db, principal);
            var trainee = await TraineeCardAsync(db, principal);
            var assessor = await AssessorCardAsync(db, principal);
            var notOwn = inbox.Where(row => row.SubjectUserId != callerId).ToList();

            trainee.Inbox.Select(item => item.ActivityId).Should().Equal(
                inbox.Take(5).Select(row => row.Id),
                $"{seedKey}: the {who}'s Activity inbox card is the first rows of the inbox it opens");
            trainee.Inbox.Select(item => item.CurrentStateLabel).Should().Equal(
                inbox.Take(5).Select(row => row.CurrentStateLabel),
                $"{seedKey}: the {who}'s card names each state as the inbox does");

            assessor.PendingRequestCount.Should().Be(
                notOwn.Count,
                $"{seedKey}: the {who}'s Pending requests counts the inbox's rows less their own portfolio");
            assessor.AwaitingReview.Select(item => item.ActivityId).Should().Equal(
                notOwn.OrderBy(row => row.UpdatedOn).ThenBy(row => row.Id).Take(10).Select(row => row.Id),
                $"{seedKey}: the {who}'s waiting list is those rows, oldest first");
        }

        // Not vacuous: wherever the workflow hands a move to a named assessor, the assessor has something waiting.
        if (workflow.Transitions.Any(transition => fields.Count > 0 && NamesAField(transition.Actor)))
        {
            (await AssessorCardAsync(db, callers["assessor"])).PendingRequestCount.Should().BePositive(
                $"{seedKey} hands a move to its named assessor");
        }
    }

    private static async Task<IReadOnlyList<ActivitySummaryDto>> InboxAsync(ApplicationDbContext db, ClaimsPrincipal principal)
        => await new ListActivitiesByActorInboxQueryHandler(db, new WorkflowEvaluator(), FakeUserDirectory.Empty)
            .Handle(new ListActivitiesByActorInboxQuery(principal), CancellationToken.None);

    private static async Task<TraineeDashboardSummaryDto> TraineeCardAsync(ApplicationDbContext db, ClaimsPrincipal principal)
        => await new GetTraineeDashboardSummaryQueryHandler(db, new WorkflowEvaluator(), FakeUserDirectory.Empty)
            .Handle(new GetTraineeDashboardSummaryQuery(principal, new DateOnly(2026, 9, 23)), CancellationToken.None);

    private static async Task<AssessorDashboardSummaryDto> AssessorCardAsync(ApplicationDbContext db, ClaimsPrincipal principal)
        => await new GetAssessorDashboardSummaryQueryHandler(
                db, new WorkflowEvaluator(), FakeUserDirectory.Empty, Options.Create(new DashboardThresholds()))
            .Handle(new GetAssessorDashboardSummaryQuery(principal), CancellationToken.None);

    private static void Add(
        ApplicationDbContext db, int id, string state, string subject, string creator, string named, IReadOnlySet<string> fields)
    {
        var data = fields.ToDictionary(field => field, _ => named, StringComparer.Ordinal);
        db.Activities.Add(new Activity
        {
            Id = id,
            ActivityTypeId = TypeId,
            SchemaVersion = 1,
            SubjectUserId = subject,
            CreatedByUserId = creator,
            CurrentState = state,
            DataJson = JsonSerializer.Serialize(data),
            InstitutionId = InstitutionId,
            SpecialityId = SpecialityId,
            SubSpecialityId = SubSpecialityId,
            CreatedOn = Clock.AddDays(-30),
            // Distinct, so the inbox's newest-first and the waiting list's oldest-first each have one order.
            UpdatedOn = Clock.AddMinutes(-id)
        });
    }

    /// <summary>Every field a <c>field:</c> rule on a transition names.</summary>
    private static IReadOnlySet<string> FieldNames(Workflow workflow)
    {
        var fields = new HashSet<string>(StringComparer.Ordinal);
        foreach (var transition in workflow.Transitions)
        {
            ActorFieldRules.CollectFieldNames(transition.Actor, fields);
        }

        return fields;
    }

    private static bool NamesAField(ActorRule rule) => rule switch
    {
        FieldUserActorRule => true,
        CombinedActorRule combined => combined.Rules.Any(NamesAField),
        _ => false
    };

    private static ApplicationDbContext NewContext()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
