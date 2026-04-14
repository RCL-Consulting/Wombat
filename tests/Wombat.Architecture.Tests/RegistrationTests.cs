using System.Reflection;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Wombat.Architecture.Tests;

/// <summary>
/// Verifies that all MediatR handlers in the Application assembly are registered in DI at boot.
/// Catches "forgot to register" regressions when a handler is accidentally placed outside
/// the assembly scanned by <c>RegisterServicesFromAssembly</c>.
/// </summary>
public class RegistrationTests
{
    private static readonly Assembly ApplicationAssembly =
        typeof(Wombat.Application.DependencyInjection).Assembly;

    [Fact]
    public void All_Application_handlers_are_registered_by_MediatR()
    {
        // Register only MediatR from the Application assembly (no real DbContext needed —
        // we check ServiceDescriptors, not resolve instances).
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(ApplicationAssembly));

        // Collect all concrete handler types in the Application assembly.
        var handlerTypes = ApplicationAssembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false, IsGenericTypeDefinition: false })
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType && i.Namespace == "MediatR" &&
                (i.Name == "IRequestHandler`2" || i.Name == "IRequestHandler`1")))
            .ToList();

        handlerTypes.Should().NotBeEmpty(
            because: "the Application assembly must contain at least one handler.");

        // Every handler type should appear as an ImplementationType in the service collection.
        var unregistered = handlerTypes
            .Where(ht => !services.Any(sd => sd.ImplementationType == ht))
            .Select(ht => ht.FullName!)
            .OrderBy(n => n)
            .ToList();

        unregistered.Should().BeEmpty(
            because: "all handler types in the Application assembly must be registered by " +
                     "RegisterServicesFromAssembly. If a handler is missing, ensure it is public, " +
                     "concrete, and non-generic, and lives in the Wombat.Application assembly.");
    }

    [Fact]
    public void All_Application_handlers_are_public_concrete_and_non_generic()
    {
        // MediatR's RegisterServicesFromAssembly only discovers public, concrete, non-generic types.
        // A handler that violates any of these constraints will be silently skipped.
        var incompatible = ApplicationAssembly.GetTypes()
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType && i.Namespace == "MediatR" &&
                (i.Name == "IRequestHandler`2" || i.Name == "IRequestHandler`1")))
            .Where(t => t.IsAbstract || t.IsGenericTypeDefinition || !t.IsPublic)
            .Select(t =>
                $"{t.FullName} " +
                $"(abstract={t.IsAbstract}, generic={t.IsGenericTypeDefinition}, public={t.IsPublic})")
            .OrderBy(n => n)
            .ToList();

        incompatible.Should().BeEmpty(
            because: "all IRequestHandler implementations must be public, concrete, and non-generic " +
                     "so that MediatR's assembly scanner can register them.");
    }
}
