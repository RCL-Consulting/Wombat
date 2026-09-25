using System.Reflection;
using FluentAssertions;
using Mono.Cecil;
using Wombat.Api.Endpoints;
using Wombat.Web.Services;

namespace Wombat.Architecture.Tests;

/// <summary>
/// The forcing function for T230: whatever pauses or resumes an EPA's credit holds the EPA against the completions whose
/// credit it changes.
/// </summary>
/// <remarks>
/// <para>
/// A completion credits an EPA's items only while the EPA is in force at its moment, and a reactivation credits what was
/// completed during the pause. T230 serialised the two on the EPA's row (<c>IEpaCreditLock</c>): a completion holds it
/// shared from before it reads whether the EPA is in force until its save commits, and a deactivation or a reactivation
/// holds it for a change from before it reads the EPA. A new caller of <c>Epa.Deactivate</c> or <c>Epa.Reactivate</c>
/// without the hold (a bulk retirement, say, or a seeder retiring an EPA) compiles, passes its own tests and silently
/// reopens both races, because nothing it does is wrong in a single request.
/// </para>
/// <para>
/// The check is per top-level type: one that calls either method must also call
/// <c>IEpaCreditLock.HoldForChangeAsync</c>. It cannot see whether the hold comes first, or whether the clock is read
/// after it; those are on the caller, and <c>EpaCreditRacePostgresTests</c> drives both handlers that exist.
/// </para>
/// </remarks>
public class EpaPauseWritePathTests
{
    private static readonly Assembly ApplicationAssembly = typeof(Wombat.Application.DependencyInjection).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(Wombat.Infrastructure.DependencyInjection).Assembly;
    private static readonly Assembly WebAssembly = typeof(IScopedSender).Assembly;
    private static readonly Assembly ApiAssembly = typeof(MsfRespondEndpoint).Assembly;

    private const string EpaEntity = "Wombat.Domain.Epas.Epa";
    private const string EpaCreditLock = "Wombat.Application.Common.Interfaces.IEpaCreditLock";
    private const string HoldForChange = "HoldForChangeAsync";

    /// <summary>The members of <c>Epa</c> that pause or resume its credit.</summary>
    private static readonly string[] PauseMembers = ["Deactivate", "Reactivate"];

    /// <summary>The two that exist today: the Deactivate button, and saving the edit form with Active ticked or not.</summary>
    private static readonly string[] KnownPausers =
    [
        "Wombat.Application.Features.Epas.DeactivateEpaCommandHandler",
        "Wombat.Application.Features.Epas.UpdateEpaCommandHandler"
    ];

    [Fact]
    public void Whatever_pauses_or_resumes_an_epa_holds_it_for_a_change()
    {
        Scan()
            .Where(type => type.Pauses.Count > 0 && !type.HoldsForChange)
            .Select(type => $"{type.TopLevelType} calls Epa.{string.Join("/", type.Pauses)} without IEpaCreditLock.{HoldForChange}")
            .Should().BeEmpty(
                "a deactivation or a reactivation must hold the EPA for a change from before it reads the EPA until its save " +
                "commits, reading the deactivation's moment only once it holds it; without the hold a completion in flight " +
                "keeps credit a rebuild takes away, or is missed by the reactivation and stays uncredited. (T230)");
    }

    [Fact]
    public void The_scan_still_sees_the_known_pausers_and_their_holds()
    {
        // The guard for the test above: if the scan found no caller anywhere, or no hold, it would pass for the wrong reason.
        Scan()
            .Where(type => type.Pauses.Count > 0 && type.HoldsForChange)
            .Select(type => type.TopLevelType)
            .Should().BeEquivalentTo(KnownPausers, "guard: the IL scan must see both EPA handlers, each calling the hold");
    }

    // ─── Helpers: Cecil ──────────────────────────────────────────────────────

    private sealed record ScannedType(string TopLevelType, IReadOnlySet<string> Pauses, bool HoldsForChange);

    private static List<ScannedType> Scan()
        => new[] { ApplicationAssembly, InfrastructureAssembly, ApiAssembly, WebAssembly }.SelectMany(ScanAssembly).ToList();

    private static List<ScannedType> ScanAssembly(Assembly assembly)
    {
        using var module = ModuleDefinition.ReadModule(assembly.Location);

        // Materialised before the module is disposed.
        return module.Types
            .Select(topLevel =>
            {
                var calls = Flatten(topLevel)
                    .SelectMany(type => type.Methods)
                    .Where(method => method.HasBody)
                    .SelectMany(method => method.Body.Instructions)
                    .Select(instruction => instruction.Operand as MethodReference)
                    .Where(call => call is not null)
                    .ToList();

                var pauses = calls
                    .Where(call => call!.DeclaringType?.FullName == EpaEntity && PauseMembers.Contains(call.Name, StringComparer.Ordinal))
                    .Select(call => call!.Name)
                    .ToHashSet(StringComparer.Ordinal);

                var holds = calls.Any(call => call!.DeclaringType?.FullName == EpaCreditLock &&
                                              string.Equals(call.Name, HoldForChange, StringComparison.Ordinal));

                return new ScannedType(topLevel.FullName, pauses, holds);
            })
            .ToList();
    }

    /// <summary>
    /// Every type nested in <paramref name="type" />, because an <c>async</c> method's body lives in a compiler-generated
    /// state machine nested inside its declaring type, and a lambda's in a display class.
    /// </summary>
    private static IEnumerable<TypeDefinition> Flatten(TypeDefinition type)
        => new[] { type }.Concat(type.NestedTypes.SelectMany(Flatten));
}
