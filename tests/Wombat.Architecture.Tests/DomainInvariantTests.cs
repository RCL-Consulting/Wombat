using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using Wombat.Domain.Institutions;

namespace Wombat.Architecture.Tests;

/// <summary>
/// Verifies structural invariants for the Domain layer.
/// These tests protect the design conventions documented in ARCHITECTURE.md.
/// </summary>
public class DomainInvariantTests
{
    private static readonly Assembly DomainAssembly = typeof(Institution).Assembly;

    [Fact]
    public void All_concrete_domain_classes_should_be_sealed()
    {
        // ARCHITECTURE.md: "sealed on concrete classes by default."
        // Every non-abstract, non-static class in Domain must be sealed.
        // This prevents accidental inheritance from escaping the domain model.
        var result = Types
            .InAssembly(DomainAssembly)
            .That()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: $"all concrete Domain classes must be sealed (ARCHITECTURE.md). " +
                     $"Unsealed types: {Join(result.FailingTypeNames)}");
    }

    // NOTE: A stricter "No_public_setters_on_domain_entities" invariant is intentionally deferred.
    // The codebase uses EF-friendly public setters across all domain entities — a deliberate
    // trade-off for EF Core compatibility. Enforcing private setters project-wide would require
    // migrating ~380 properties and is tracked as a future task. The ActivityType aggregate (which
    // has business methods controlling mutation) is the canonical pattern to migrate toward.

    private static string Join(IEnumerable<string>? names) =>
        names is null ? "(none)" : string.Join(", ", names);
}
