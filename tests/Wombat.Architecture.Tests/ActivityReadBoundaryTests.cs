using System.Reflection;
using System.Security.Claims;
using FluentAssertions;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Wombat.Architecture.Tests;

/// <summary>
/// The forcing function for the T101 read boundary.
/// </summary>
/// <remarks>
/// <para>
/// T101 closed a hole in which <c>GetActivityByIdQuery</c>, <c>ListActivitiesBySubjectQuery</c>,
/// <c>GetEpaTrajectoryForTraineeQuery</c>, <c>GetSamplingConcentrationWarningsQuery</c> and the
/// coordinator dashboard all returned another trainee's clinical data to any authenticated caller.
/// Nothing in the type system stops that being reintroduced: a new query over
/// <c>Set&lt;Activity&gt;()</c> compiles, passes its unit tests and ships wide open. These tests are
/// the guard rails.
/// </para>
/// <para>
/// They are deliberately separate tests, because carrying a principal and USING it are different
/// properties and only the second is the boundary. <c>GetActivityByIdQuery</c> carried a
/// <see cref="ClaimsPrincipal" /> throughout T070 while performing no read authorization at all —
/// its own XML comment said so — so the shape test below would NOT have caught the defect T101 is
/// named for. The IL test would: it asks whether the rows are actually put through
/// <c>ActivityReadScope.WhereReadableBy</c>.
/// </para>
/// </remarks>
public class ActivityReadBoundaryTests
{
    private static readonly Assembly ApplicationAssembly =
        typeof(Wombat.Application.DependencyInjection).Assembly;

    private const string ActivityEntity = "Wombat.Domain.Activities.Activity";
    private const string ReadScopeType = "ActivityReadScope";
    private const string ReadScopeMethod = "WhereReadableBy";

    /// <summary>
    /// Every DTO that carries one trainee's activity data out of the Application layer. A request
    /// that can return one of these is answering "show me this trainee's assessments", which is a
    /// question that cannot be answered without knowing who is asking.
    /// </summary>
    private static readonly string[] ActivityDataDtoNames =
    [
        "ActivityDto",
        "ActivityDetailDto",
        "ActivitySummaryDto",
        // T342: My activities' page of rows, which holds ActivitySummaryDto rows in a record the unwrap cannot see into.
        "ActivityListPageDto",
        "EpaTrajectoryDto",
        "TrajectoryPointDto"
    ];

    /// <summary>
    /// Handlers that read <c>Set&lt;Activity&gt;()</c> without <c>WhereReadableBy</c>, each with the
    /// reason the read rule does not apply. An entry here is a claim that the rows are already
    /// confined by something at least as narrow as the read gate — not permission to skip it.
    /// </summary>
    private static readonly Dictionary<string, string> ScopeExemptHandlers = new(StringComparer.Ordinal)
    {
        ["Wombat.Application.Features.Activities.Commands.RebuildCurriculumProgress.RebuildCurriculumProgressCommandHandler"] =
            "Replays every activity to rebuild curriculum credit. Returns a count; no activity row reaches a caller.",

        ["Wombat.Application.Features.Trainees.CompleteTraineeProfileCommandHandler"] =
            "Recording the completion day takes back credit earned by encounters after it (T281): reads one trainee's credited activities' encounter dates, then replays that trainee's credit as the rebuild does (ProgrammeEndCredit), after the institution scope check (CanAccessInstitution on the profile). The replay is the rebuild's for one trainee: it re-scores their whole history against today's curriculum and re-stamps their completions wherever they were filed, other institutions included, and runs only when the end takes credit back. Returns nothing; no activity row reaches a caller.",

        ["Wombat.Application.Features.Trainees.DeactivateTraineeProfileCommandHandler"] =
            "Recording the last day takes back credit earned by encounters after it (T281): reads one trainee's credited activities' encounter dates, then replays that trainee's credit as the rebuild does (ProgrammeEndCredit), after the institution scope check (CanAccessInstitution on the profile). The replay is the rebuild's for one trainee: it re-scores their whole history against today's curriculum and re-stamps their completions wherever they were filed, other institutions included, and runs only when the end takes credit back. Returns nothing; no activity row reaches a caller.",

        ["Wombat.Application.Features.Epas.UpdateEpaCommandHandler"] =
            "Reactivating an EPA credits the completions filed while it was inactive (T196, D48), as the rebuild would. Reads activities only on reactivation, after the EPA-owner check (CollegeAdmin or owning InstitutionalAdmin), and only completions that could hold paused credit: pinned to rules that credit, in a terminal state, with a transition since the pause began (ResumedEpaCredit.LoadCandidatesAsync). Returns the EPA and a count; no activity row reaches a caller.",

        ["Wombat.Application.Features.Activities.Queries.ListActivitiesByActorInbox.ListActivitiesByActorInboxQueryHandler"] =
            "Filters every row through IWorkflowEvaluator, i.e. the ACT gate, which IsReadableBy is a superset of (ActivityWaiting.LoadActionableAsync, T297). Narrower than the read rule by construction.",

        ["Wombat.Application.Features.CommitteeDecisions.StartCommitteeReviewCommandHandler"] =
            "Builds the review evidence snapshot after CommitteeDecisionAuthorization.DemandStartableReview, which admits a member of this panel, a Coordinator of the institution this panel belongs to, or a global Administrator - it waived every Coordinator anywhere until T101 finding E, which is what made this reason worth stating - and after CommitteeTraineeScope.DemandTraineeAtPanelInstitutionAsync, which refuses anyone but an Administrator a review whose trainee does not train at the panel's institution (T182). Stores labels and dates, never DataJson.",

        ["Wombat.Application.Features.Dashboards.Assessor.GetAssessorDashboardSummaryQueryHandler"] =
            "Its waiting card is the inbox's rows, filtered through IWorkflowEvaluator, the ACT gate, as ListActivitiesByActorInboxQueryHandler's are (ActivityWaiting.LoadActionableAsync, T297). Its decisions are rows the caller moved last - the past-actor arm of IsReadableBy, which the list filter deliberately omits.",

        ["Wombat.Application.Features.Dashboards.Trainee.GetTraineeDashboardSummaryQueryHandler"] =
            "Its own reads are confined to SubjectUserId == the signed-in user, which is the subject arm of the read rule. Its Activity inbox card is the inbox's rows, filtered through IWorkflowEvaluator, the ACT gate, as ListActivitiesByActorInboxQueryHandler's are (ActivityWaiting.LoadActionableAsync, T297).",

        ["Wombat.Application.Features.Dashboards.SpecialityAdmin.GetSpecialityAdminDashboardSummaryQueryHandler"] =
            "T101 REVIEW FINDING, not an approval: hand-rolls the SpecialityId stamp comparison instead of calling WhereReadableBy, making it a fourth copy of the oversight rule. It already differs - no Administrator arm, and no IsInRole(SpecialityAdmin) gate on the speciality claim - and is safe only because SpecialityAdminDashboard.razor is role-gated, which nothing here states. Replace the inline Where with WhereReadableBy and delete this entry.",

        ["Wombat.Application.Features.Dashboards.SubSpecialityAdmin.GetSubSpecialityAdminDashboardSummaryQueryHandler"] =
            "T101 REVIEW FINDING, not an approval: same hand-rolled stamp comparison as the SpecialityAdmin dashboard, and on the same dashboard its trainee tiles derive scope live from Curriculum.SubSpecialityId while this tile reads the frozen stamp, so the two disagree about a transferred trainee. Replace the inline Where with WhereReadableBy and delete this entry."
    };

    // ─── The boundary itself ─────────────────────────────────────────────────

    /// <summary>
    /// Any Application handler that queries activity rows must put them through the one read rule,
    /// or appear in <see cref="ScopeExemptHandlers" /> with a reason.
    /// </summary>
    [Fact]
    public void Handlers_that_query_activity_rows_apply_the_read_scope()
    {
        using var module = ModuleDefinition.ReadModule(ApplicationAssembly.Location);

        var violations = AllTypes(module)
            .Where(IsRequestHandler)
            .Where(type => !ScopeExemptHandlers.ContainsKey(type.FullName))
            .Where(type => CallsAny(type, IsActivityDbSetCall))
            .Where(type => !CallsAny(type, IsReadScopeCall))
            .Select(type => type.FullName)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToList();

        violations.Should().BeEmpty(
            because: "a handler that reads Set<Activity>() must confine the rows with " +
                     "ActivityReadScope.WhereReadableBy, or be listed in ScopeExemptHandlers with " +
                     "the reason its rows are already confined more narrowly. (T101)");
    }

    /// <summary>
    /// The exemption list must not rot: an entry naming a type that no longer exists, or one that
    /// has since been scoped properly, is a stale claim the next reader would trust.
    /// </summary>
    [Fact]
    public void Every_scope_exemption_still_describes_a_real_unscoped_handler()
    {
        using var module = ModuleDefinition.ReadModule(ApplicationAssembly.Location);
        var byName = AllTypes(module).ToDictionary(type => type.FullName, StringComparer.Ordinal);

        var stale = ScopeExemptHandlers.Keys
            .Where(name => !byName.TryGetValue(name, out var type) ||
                           !CallsAny(type, IsActivityDbSetCall) ||
                           CallsAny(type, IsReadScopeCall))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToList();

        stale.Should().BeEmpty(
            because: "an exemption that no longer applies must be deleted, not left standing as a " +
                     "licence for whatever takes that type's name next. (T101)");
    }

    // ─── The shape ───────────────────────────────────────────────────────────

    /// <summary>
    /// A request that can hand back one trainee's activity data must carry the caller. Weaker than
    /// the test above — carrying a principal is not using it — but it catches the whole class of
    /// "query keyed on a caller-supplied trainee id and nothing else" at the signature, which is
    /// where ListActivitiesBySubjectQuery and GetEpaTrajectoryForTraineeQuery went wrong.
    /// </summary>
    [Fact]
    public void Requests_that_return_activity_data_carry_the_caller()
    {
        var violations = ApplicationAssembly.GetTypes()
            .Where(IsConcreteRequest)
            .Where(ReturnsActivityData)
            .Where(type => !CarriesClaimsPrincipal(type))
            .Select(type => type.FullName!)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToList();

        violations.Should().BeEmpty(
            because: "a request whose response contains activity data must take a ClaimsPrincipal, " +
                     "so the handler has something to authorize against. (T101)");
    }

    /// <summary>
    /// The other half of the shape rule, on the service seam rather than the MediatR seam.
    /// </summary>
    /// <remarks>
    /// <c>IActivityService.GetAsync(int activityId, CancellationToken)</c> is the method T101
    /// deleted: principal-less, zero callers, and a ready-made bypass of the gate its sibling
    /// <c>GetDetailAsync</c> enforces. Nothing stopped the next person adding it back. A service
    /// method that hands an activity DTO to a caller has to know who the caller is.
    /// </remarks>
    [Fact]
    public void Activity_service_methods_that_return_activity_data_take_the_caller()
    {
        var serviceInterface = ApplicationAssembly.GetType(
            "Wombat.Application.Features.Activities.Services.IActivityService",
            throwOnError: true)!;

        var violations = serviceInterface.GetMethods()
            .Where(method => Unwrap(method.ReturnType)
                .Any(t => ActivityDataDtoNames.Contains(t.Name, StringComparer.Ordinal)))
            .Where(method => !method.GetParameters().Any(parameter =>
                typeof(ClaimsPrincipal).IsAssignableFrom(parameter.ParameterType) ||
                // The write methods take the caller inside their input record (CreateActivityInput
                // and friends) rather than as a bare parameter.
                CarriesClaimsPrincipal(parameter.ParameterType)))
            .Select(method => $"{serviceInterface.Name}.{method.Name}")
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToList();

        violations.Should().BeEmpty(
            because: "a service method that returns activity data must take a ClaimsPrincipal; a " +
                     "principal-less overload beside a gated one is a bypass waiting to be called. (T101)");
    }

    // ─── Helpers: Cecil ──────────────────────────────────────────────────────

    private static IEnumerable<TypeDefinition> AllTypes(ModuleDefinition module)
        => module.Types.SelectMany(Flatten);

    private static IEnumerable<TypeDefinition> Flatten(TypeDefinition type)
        => new[] { type }.Concat(type.NestedTypes.SelectMany(Flatten));

    private static bool IsRequestHandler(TypeDefinition type)
        => !type.IsAbstract &&
           !type.IsInterface &&
           type.Interfaces.Any(i => i.InterfaceType.Name.StartsWith("IRequestHandler", StringComparison.Ordinal));

    /// <summary>
    /// Scans the type's own methods AND its nested types', because an <c>async</c> handler's body
    /// lives in a compiler-generated state machine nested inside it — scanning only the declared
    /// methods would find nothing at all in this codebase.
    /// </summary>
    private static bool CallsAny(TypeDefinition type, Func<Instruction, bool> predicate)
        => Flatten(type)
            .SelectMany(t => t.Methods)
            .Where(method => method.HasBody)
            .SelectMany(method => method.Body.Instructions)
            .Any(predicate);

    private static bool IsActivityDbSetCall(Instruction instruction)
        => instruction.Operand is GenericInstanceMethod call &&
           call.ElementMethod.Name == "Set" &&
           call.GenericArguments.Count == 1 &&
           call.GenericArguments[0].FullName == ActivityEntity;

    private static bool IsReadScopeCall(Instruction instruction)
        => instruction.Operand is MethodReference call &&
           call.Name == ReadScopeMethod &&
           call.DeclaringType?.Name == ReadScopeType;

    // ─── Helpers: reflection ─────────────────────────────────────────────────

    private static bool IsConcreteRequest(Type type)
    {
        if (type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition)
        {
            return false;
        }

        return type.GetInterfaces().Any(i =>
            i.Namespace == "MediatR" && (i.Name == "IRequest" || i.Name == "IRequest`1"));
    }

    private static bool ReturnsActivityData(Type requestType)
        => requestType.GetInterfaces()
            .Where(i => i.Namespace == "MediatR" && i.Name == "IRequest`1")
            .SelectMany(i => Unwrap(i.GetGenericArguments()[0]))
            .Any(t => ActivityDataDtoNames.Contains(t.Name, StringComparer.Ordinal));

    /// <summary>The response type and, for a collection or a nullable, what it is made of.</summary>
    private static IEnumerable<Type> Unwrap(Type type)
    {
        yield return type;

        if (!type.IsGenericType)
        {
            yield break;
        }

        foreach (var argument in type.GetGenericArguments())
        {
            foreach (var inner in Unwrap(argument))
            {
                yield return inner;
            }
        }
    }

    private static bool CarriesClaimsPrincipal(Type requestType)
        => requestType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Any(property => typeof(ClaimsPrincipal).IsAssignableFrom(property.PropertyType));
}
