using System.Reflection;
using FluentAssertions;
using Wombat.Infrastructure.DataRights;

namespace Wombat.Application.Tests.DataRights;

/// <summary>
/// Reflection-based coverage test that enumerates every Domain entity with a UserId-like
/// string property and verifies the ErasureExecutor source code handles it.
/// This is the primary defence against silent regressions when a new entity
/// with a UserId FK is added without updating erasure logic.
/// </summary>
public sealed class ErasureCoverageTests
{
    /// <summary>
    /// Properties matching these patterns are considered user-identifying references.
    /// </summary>
    private static readonly string[] UserIdPropertySuffixes =
    [
        "UserId",
    ];

    /// <summary>
    /// Entities/properties explicitly excluded from erasure because they are:
    /// - Already handled differently (e.g. audit entries are retained unchanged)
    /// - Self-referencing the erasure system itself
    /// - Not user PII (e.g. MsfInvitation.RespondentName is entered by the invitee, not the subject)
    /// </summary>
    private static readonly HashSet<string> ExcludedEntityProperties = new(StringComparer.Ordinal)
    {
        // Audit entries are retained unchanged under legitimate-interest basis.
        "AuditEntry.ActorUserId",
        "AuditEntryArchive.ActorUserId",

        // DataRights entities reference the requester/decider — retained for accountability.
        "DataRightsRequest.RequesterUserId",
        "DataRightsRequest.DecidedByUserId",
        "DataRightsErasureRecord.UserId",
    };

    [Fact]
    public void ErasureExecutor_HandlesAllUserIdProperties()
    {
        // Find all Domain entity types
        var domainAssembly = typeof(Wombat.Domain.DataRights.DataRightsRequest).Assembly;
        var entityTypes = domainAssembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && type.IsPublic && !type.IsNested)
            .Where(type => type.Namespace is not null && type.Namespace.StartsWith("Wombat.Domain"))
            .ToList();

        // Find all string properties ending in "UserId"
        var userIdProperties = new List<string>();
        foreach (var entityType in entityTypes)
        {
            var props = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(prop => prop.PropertyType == typeof(string) || prop.PropertyType == typeof(string))
                .Where(prop => UserIdPropertySuffixes.Any(suffix =>
                    prop.Name.EndsWith(suffix, StringComparison.Ordinal)));

            foreach (var prop in props)
            {
                var key = $"{entityType.Name}.{prop.Name}";
                if (!ExcludedEntityProperties.Contains(key))
                    userIdProperties.Add(key);
            }
        }

        // Read the ErasureExecutor source to check for coverage
        var executorSource = File.ReadAllText(
            Path.Combine(FindSolutionRoot(), "src", "Wombat.Infrastructure", "DataRights", "ErasureExecutor.cs"));

        var unhandled = new List<string>();
        foreach (var prop in userIdProperties)
        {
            var parts = prop.Split('.');
            var entityName = parts[0];
            var propertyName = parts[1];

            // Check if the executor references this entity type and property
            var entityReferenced = executorSource.Contains(entityName, StringComparison.Ordinal);
            var propertyReferenced = executorSource.Contains(propertyName, StringComparison.Ordinal);

            if (!entityReferenced || !propertyReferenced)
                unhandled.Add(prop);
        }

        unhandled.Should().BeEmpty(
            "Every entity with a UserId-like property must be handled in ErasureExecutor. " +
            "Unhandled: {0}. Add handling or exclude with a documented reason.",
            string.Join(", ", unhandled));
    }

    private static string FindSolutionRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (directory.GetFiles("Wombat.sln").Length > 0)
                return directory.FullName;
            directory = directory.Parent;
        }
        throw new InvalidOperationException("Could not find Wombat.sln from " + AppContext.BaseDirectory);
    }
}
