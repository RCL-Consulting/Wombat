using System.Reflection;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Architecture.Tests;

/// <summary>
/// Verifies that EF Core entity configurations are correctly wired up.
/// Guards against the common mistake of adding a new entity to <see cref="ApplicationDbContext"/>
/// without creating a corresponding <see cref="IEntityTypeConfiguration{TEntity}"/> class.
/// </summary>
public class ModelConfigurationTests
{
    private static readonly Assembly InfrastructureAssembly =
        typeof(Wombat.Infrastructure.DependencyInjection).Assembly;
    private static readonly Assembly DomainAssembly =
        typeof(Institution).Assembly;

    [Fact]
    public void All_entity_configurations_are_EF_discoverable()
    {
        // ApplicationDbContext calls ApplyConfigurationsFromAssembly — EF requires each
        // IEntityTypeConfiguration<T> to be public, concrete, non-generic, and have a
        // parameterless constructor. Violating any of these means the config is silently skipped.
        var configTypes = GetEntityConfigurationTypes(InfrastructureAssembly);

        configTypes.Should().NotBeEmpty(
            because: "there must be at least one entity configuration in Infrastructure.");

        var undiscoverable = configTypes
            .Where(t => !t.IsPublic ||
                        t.IsAbstract ||
                        t.IsGenericTypeDefinition ||
                        t.GetConstructor(Type.EmptyTypes) is null)
            .Select(t =>
                $"{t.Name} " +
                $"(public={t.IsPublic}, abstract={t.IsAbstract}, " +
                $"generic={t.IsGenericTypeDefinition}, " +
                $"parameterlessCtor={t.GetConstructor(Type.EmptyTypes) is not null})")
            .ToList();

        undiscoverable.Should().BeEmpty(
            because: "all IEntityTypeConfiguration<T> classes must be public, concrete, " +
                     "non-generic, and have a parameterless constructor so EF discovers them via " +
                     "ApplyConfigurationsFromAssembly.");
    }

    [Fact]
    public void Every_Domain_entity_with_a_DbSet_has_a_configuration()
    {
        // ApplicationDbContext exposes DbSet<T> properties for every domain entity it owns.
        // Each of those entity types must have a corresponding IEntityTypeConfiguration<T>
        // in the Infrastructure assembly.
        //
        // Identity-owned types (WombatIdentityUser, scopes) are configured inline in
        // OnModelCreating rather than via standalone configs — they are excluded from this check
        // because they live in the Infrastructure namespace, not Domain.
        var domainEntityTypes = GetDbSetEntityTypes()
            .Where(t => t.Assembly == DomainAssembly)
            .ToList();

        var configuredTypes = GetEntityConfigurationTypes(InfrastructureAssembly)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
                .Select(i => i.GetGenericArguments()[0]))
            .ToHashSet();

        var missing = domainEntityTypes
            .Where(t => !configuredTypes.Contains(t))
            .Select(t => t.FullName!)
            .OrderBy(n => n)
            .ToList();

        missing.Should().BeEmpty(
            because: "every Domain entity exposed via a DbSet in ApplicationDbContext must have " +
                     "a corresponding IEntityTypeConfiguration<T> in the Infrastructure assembly. " +
                     "Add a configuration class or EF will silently use conventions only.");
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    /// <summary>Returns all concrete, non-generic IEntityTypeConfiguration implementations in the assembly.</summary>
    private static IReadOnlyList<Type> GetEntityConfigurationTypes(Assembly assembly) =>
        assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false, IsGenericTypeDefinition: false })
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)))
            .ToList();

    /// <summary>Returns the entity type argument for each DbSet property on ApplicationDbContext.</summary>
    private static IEnumerable<Type> GetDbSetEntityTypes()
    {
        var dbSetOpenType = typeof(DbSet<>);
        return typeof(ApplicationDbContext)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType.IsGenericType &&
                        p.PropertyType.GetGenericTypeDefinition() == dbSetOpenType)
            .Select(p => p.PropertyType.GetGenericArguments()[0]);
    }
}
