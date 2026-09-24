using System.Reflection;
using FluentAssertions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Wombat.Api.Endpoints;
using Wombat.Web.Services;

namespace Wombat.Architecture.Tests;

/// <summary>
/// The forcing function for T102's write paths: an activity's data is written by <c>ActivityService</c> and nothing
/// else, through a method that runs the guard sequence.
/// </summary>
/// <remarks>
/// <para>
/// T102 deleted <c>IActivityService.UpdateDraftAsync</c> and <c>UpdateActivityDraftCommand</c>. They had no production
/// caller, replaced the whole payload in any non-terminal state, and skipped field ownership (T070), the state gate,
/// the self-nomination guard and the nominee gate. Nothing in the type system stops them coming back: a new "save the
/// draft" method compiles, passes its own tests and silently bypasses every check the three guarded paths make.
/// </para>
/// <para>
/// A post-creation save must be a self-transition through <c>TransitionAsync</c> (T106 item 1, T127), or a new method
/// that runs the same sequence in the same order: field ownership, the self-nomination guard, schema validation, the
/// EPA→tool gate, the nominee gate, all before the first mutation. Either way it has to be added here deliberately.
/// </para>
/// </remarks>
public class ActivityWritePathTests
{
    private static readonly Assembly ApplicationAssembly = typeof(Wombat.Application.DependencyInjection).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(Wombat.Infrastructure.DependencyInjection).Assembly;
    private static readonly Assembly WebAssembly = typeof(IScopedSender).Assembly;
    private static readonly Assembly ApiAssembly = typeof(MsfRespondEndpoint).Assembly;

    private const string ActivityEntity = "Wombat.Domain.Activities.Activity";
    private const string ActivityServiceType = "Wombat.Infrastructure.Activities.ActivityService";
    private const string ActivityServiceInterface = "Wombat.Application.Features.Activities.Services.IActivityService";

    /// <summary>
    /// The members of <c>Activity</c> that replace its data: the <c>DataJson</c> setter, and <c>ApplyTransition</c>,
    /// which writes the new payload with the move.
    /// </summary>
    private static readonly string[] DataWritingMembers = ["set_DataJson", "ApplyTransition"];

    /// <summary>
    /// Every method <c>IActivityService</c> offers, and what each one is. A write method is one of the three guarded
    /// paths; adding a fourth means adding it here, with the guard sequence it runs.
    /// </summary>
    private static readonly Dictionary<string, string> ActivityServiceSurface = new(StringComparer.Ordinal)
    {
        ["CreateDraftAsync"] = "Write: builds the draft through BuildDraftActivity (T070 filter, self-nomination guard, validation), then the encounter-date bounds (T160), the EPA→tool gate and the nominee gate, before Add.",
        ["TransitionAsync"] = "Write: the actor gate, the T070 merge, the self-nomination guard, Submit validation, the encounter-date bounds (T160), the EPA→tool gate and the nominee gate, before ApplyTransition.",
        ["StageCompletedAsync"] = "Write: BuildDraftActivity per row, the encounter-date future check (T160), the nominee gate, the actor gate and validation per row, and AddRange only after the whole batch has passed.",
        ["GetDetailAsync"] = "Read: behind the T101 read gate. Writes nothing."
    };

    /// <summary>
    /// Methods outside <c>ActivityService</c> that set <c>DataJson</c> on an <c>Activity</c>, each with the reason it is
    /// not a write path. An entry here is a claim that the entity is a detached probe no context ever sees, not
    /// permission to persist one.
    /// </summary>
    private static readonly Dictionary<string, string> ProbeExemptMethods = new(StringComparer.Ordinal)
    {
        ["Wombat.Web.Components.Pages.Activities.NewActivity::BuildCandidate"] =
            "The activity /activities/new would create, built only to ask WorkflowEvaluator and FieldPermissionEvaluator " +
            "what the creator could do. Never attached to a DbContext; the page files the real one through " +
            "CreateActivityCommand, which reaches CreateDraftAsync."
    };

    [Fact]
    public void The_whole_payload_draft_update_stays_deleted()
    {
        var serviceInterface = ApplicationAssembly.GetType(ActivityServiceInterface, throwOnError: true)!;

        serviceInterface.GetMethod("UpdateDraftAsync").Should().BeNull(
            "UpdateDraftAsync replaced the whole payload and bypassed field ownership, the self-nomination guard and " +
            "the nominee gate; a post-creation save is a self-transition through TransitionAsync. (T102)");

        ApplicationAssembly.GetTypes()
            .Where(type => type.Name.StartsWith("UpdateActivityDraft", StringComparison.Ordinal))
            .Select(type => type.FullName)
            .Should().BeEmpty("the command, its handler and its input were deleted with UpdateDraftAsync. (T102)");
    }

    [Fact]
    public void The_activity_service_offers_only_the_guarded_write_paths()
    {
        var serviceInterface = ApplicationAssembly.GetType(ActivityServiceInterface, throwOnError: true)!;

        serviceInterface.GetMethods()
            .Select(method => method.Name)
            .Distinct(StringComparer.Ordinal)
            .Should().BeEquivalentTo(
                ActivityServiceSurface.Keys,
                "a new IActivityService method is a new way in, and must be added to ActivityServiceSurface with the " +
                "guard sequence it runs (T102). A save is a self-transition through TransitionAsync.");
    }

    [Fact]
    public void Only_the_activity_service_writes_an_activitys_data()
    {
        var writers = OuterWriters()
            .Where(writer => !string.Equals(writer.TopLevelType, ActivityServiceType, StringComparison.Ordinal))
            .Where(writer => !ProbeExemptMethods.ContainsKey(writer.Method))
            .Select(writer => $"{writer.Method} calls Activity.{writer.Member}")
            .Distinct(StringComparer.Ordinal)
            .OrderBy(entry => entry, StringComparer.Ordinal)
            .ToList();

        writers.Should().BeEmpty(
            "an activity's data may be replaced only inside ActivityService, whose write methods run field ownership, " +
            "the self-nomination guard, the EPA→tool gate and the nominee gate before the first mutation. Anything " +
            "else writing DataJson or calling ApplyTransition bypasses all of them; a detached probe that is never " +
            "persisted belongs in ProbeExemptMethods with its reason. (T102)");
    }

    [Fact]
    public void Every_probe_exemption_still_describes_a_real_probe()
    {
        // As with the T101 read-scope exemptions: an entry naming a method that no longer sets DataJson is a stale
        // claim the next reader would trust, and one that calls ApplyTransition is no probe at all.
        var byMethod = OuterWriters()
            .GroupBy(writer => writer.Method, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Select(writer => writer.Member).ToHashSet(StringComparer.Ordinal));

        ProbeExemptMethods.Keys
            .Where(method => !byMethod.TryGetValue(method, out var members) ||
                             members.Contains("ApplyTransition") ||
                             !members.Contains("set_DataJson"))
            .Should().BeEmpty("an exemption that no longer applies must be deleted. (T102)");
    }

    [Fact]
    public void The_activity_service_is_still_where_the_data_is_written()
    {
        // The guard for the test above: if the scan found nothing anywhere, it would pass for the wrong reason.
        DataWritersIn(InfrastructureAssembly)
            .Where(writer => string.Equals(writer.TopLevelType, ActivityServiceType, StringComparison.Ordinal))
            .Select(writer => writer.Member)
            .Distinct(StringComparer.Ordinal)
            .Should().BeEquivalentTo(DataWritingMembers, "guard: the IL scan must see ActivityService's own writes");
    }

    // ─── Helpers: Cecil ──────────────────────────────────────────────────────

    private static IEnumerable<(string TopLevelType, string Method, string Member)> OuterWriters()
        => new[] { ApplicationAssembly, InfrastructureAssembly, ApiAssembly, WebAssembly }.SelectMany(DataWritersIn);

    private static IEnumerable<(string TopLevelType, string Method, string Member)> DataWritersIn(Assembly assembly)
    {
        using var module = ModuleDefinition.ReadModule(assembly.Location);

        // Materialised before the module is disposed.
        return module.Types
            .SelectMany(topLevel => Flatten(topLevel).Select(type => (TopLevel: topLevel, Type: type)))
            .SelectMany(pair => pair.Type.Methods
                .Where(method => method.HasBody)
                .SelectMany(method => method.Body.Instructions
                    .Select(instruction => DataWritingMember(instruction))
                    .Where(member => member is not null)
                    .Select(member => (pair.TopLevel.FullName, $"{pair.Type.FullName}::{method.Name}", member!))))
            .ToList();
    }

    /// <summary>
    /// Every type nested in <paramref name="type" />, because an <c>async</c> method's body lives in a
    /// compiler-generated state machine nested inside its declaring type, and a lambda's in a display class.
    /// </summary>
    private static IEnumerable<TypeDefinition> Flatten(TypeDefinition type)
        => new[] { type }.Concat(type.NestedTypes.SelectMany(Flatten));

    private static string? DataWritingMember(Instruction instruction)
        => instruction.Operand is MethodReference call &&
           call.DeclaringType?.FullName == ActivityEntity &&
           DataWritingMembers.Contains(call.Name, StringComparer.Ordinal)
            ? call.Name
            : null;
}
