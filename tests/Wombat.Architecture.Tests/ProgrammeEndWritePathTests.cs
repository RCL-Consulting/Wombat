using System.Reflection;
using FluentAssertions;
using Mono.Cecil;
using Wombat.Api.Endpoints;
using Wombat.Web.Services;

namespace Wombat.Architecture.Tests;

/// <summary>
/// The forcing function for T281's lock: whatever records a trainee's programme end holds the profile against the
/// completions whose credit it judges.
/// </summary>
/// <remarks>
/// <para>
/// An encounter observed after the programme's last day credits nothing on the profile, and recording the end takes back
/// the credit such encounters already earned (<c>ProgrammeEndCredit</c>). The review of T281 serialised the two on the
/// profile's row (<c>ITraineeCreditLock</c>): a completion holds its trainee's profiles shared from before it reads the end
/// until its save commits, and recording an end holds the profile from before it reads it. A new caller of
/// <c>TraineeProfile.Complete</c> or <c>TraineeProfile.Deactivate</c> without the hold (a bulk withdrawal, say, or an
/// import of graduations) compiles, passes its own tests and silently reopens the race, because nothing it does is wrong
/// in a single request.
/// </para>
/// <para>
/// The check is per top-level type: one that calls either method must also call <c>ITraineeCreditLock.HoldForEndAsync</c>.
/// It cannot see whether the hold comes first; that is on the caller, and <c>ProgrammeEndCreditRacePostgresTests</c> drives
/// the handler that exists. <c>TraineeProfile.Erase</c> is not scanned: it ends a profile on the erasure day, which is
/// today, and no encounter can be dated after today (<c>EncounterDateGate</c>), so it takes nothing back.
/// </para>
/// </remarks>
public class ProgrammeEndWritePathTests
{
    private static readonly Assembly ApplicationAssembly = typeof(Wombat.Application.DependencyInjection).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(Wombat.Infrastructure.DependencyInjection).Assembly;
    private static readonly Assembly WebAssembly = typeof(IScopedSender).Assembly;
    private static readonly Assembly ApiAssembly = typeof(MsfRespondEndpoint).Assembly;

    private const string ProfileEntity = "Wombat.Domain.Identity.TraineeProfile";
    private const string TraineeCreditLock = "Wombat.Application.Common.Interfaces.ITraineeCreditLock";
    private const string HoldForEnd = "HoldForEndAsync";

    /// <summary>The members of <c>TraineeProfile</c> that record the programme's end.</summary>
    private static readonly string[] EndMembers = ["Complete", "Deactivate"];

    /// <summary>The two that exist today: Mark complete and Deactivate on the trainee profile page.</summary>
    private static readonly string[] KnownEnders =
    [
        "Wombat.Application.Features.Trainees.CompleteTraineeProfileCommandHandler",
        "Wombat.Application.Features.Trainees.DeactivateTraineeProfileCommandHandler"
    ];

    [Fact]
    public void Whatever_records_a_programmes_end_holds_the_profile_for_it()
    {
        Scan()
            .Where(type => type.Ends.Count > 0 && !type.HoldsForEnd)
            .Select(type => $"{type.TopLevelType} calls TraineeProfile.{string.Join("/", type.Ends)} without ITraineeCreditLock.{HoldForEnd}")
            .Should().BeEmpty(
                "recording a programme's end must hold the profile from before it reads it until its save commits; without " +
                "the hold a completion in flight credits an encounter after the last day that the end neither sees nor takes " +
                "back. (T281)");
    }

    [Fact]
    public void The_scan_still_sees_the_known_enders_and_their_holds()
    {
        // The guard for the test above: if the scan found no caller anywhere, or no hold, it would pass for the wrong reason.
        Scan()
            .Where(type => type.Ends.Count > 0 && type.HoldsForEnd)
            .Select(type => type.TopLevelType)
            .Should().BeEquivalentTo(KnownEnders, "guard: the IL scan must see both end handlers, each calling the hold");
    }

    // ─── Helpers: Cecil ──────────────────────────────────────────────────────

    private sealed record ScannedType(string TopLevelType, IReadOnlySet<string> Ends, bool HoldsForEnd);

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

                var ends = calls
                    .Where(call => call!.DeclaringType?.FullName == ProfileEntity && EndMembers.Contains(call.Name, StringComparer.Ordinal))
                    .Select(call => call!.Name)
                    .ToHashSet(StringComparer.Ordinal);

                var holds = calls.Any(call => call!.DeclaringType?.FullName == TraineeCreditLock &&
                                              string.Equals(call.Name, HoldForEnd, StringComparison.Ordinal));

                return new ScannedType(topLevel.FullName, ends, holds);
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
