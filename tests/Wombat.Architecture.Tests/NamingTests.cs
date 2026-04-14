using System.Reflection;
using FluentAssertions;
using Wombat.Application.Common;

namespace Wombat.Architecture.Tests;

/// <summary>
/// Enforces the CQRS naming conventions from ARCHITECTURE.md:
/// Commands end in "Command", Queries end in "Query", Handlers end in "Handler".
/// Every command must have a FluentValidation validator or be explicitly opted out via
/// <see cref="NoValidatorAttribute"/>.
/// </summary>
public class NamingTests
{
    private static readonly Assembly ApplicationAssembly =
        typeof(Wombat.Application.DependencyInjection).Assembly;

    // ─── Commands ────────────────────────────────────────────────────────────

    [Fact]
    public void Commands_should_end_with_Command()
    {
        // Any concrete type in Application that implements IRequest or IRequest<T>
        // and does not end in "Command" or "Query" is a naming violation.
        var violations = ApplicationAssembly.GetTypes()
            .Where(IsConcreteRequest)
            .Where(t => !t.Name.EndsWith("Command", StringComparison.Ordinal) &&
                        !t.Name.EndsWith("Query", StringComparison.Ordinal))
            .Select(t => t.FullName!)
            .ToList();

        violations.Should().BeEmpty(
            because: "all MediatR request types in Application must end in 'Command' or 'Query'.");
    }

    [Fact]
    public void Queries_should_end_with_Query()
    {
        // Query handlers reference a Query type. Verify all types named *Query implement IRequest<T>.
        var badNames = ApplicationAssembly.GetTypes()
            .Where(t => t.Name.EndsWith("Query", StringComparison.Ordinal) &&
                        t is { IsAbstract: false, IsInterface: false, IsGenericTypeDefinition: false } &&
                        !IsConcreteRequest(t))
            .Select(t => t.FullName!)
            .ToList();

        badNames.Should().BeEmpty(
            because: "types named *Query must implement IRequest or IRequest<T>.");
    }

    [Fact]
    public void CommandHandlers_should_end_with_Handler()
    {
        var violations = ApplicationAssembly.GetTypes()
            .Where(IsConcreteRequestHandler)
            .Where(t => !t.Name.EndsWith("Handler", StringComparison.Ordinal))
            .Select(t => t.FullName!)
            .ToList();

        violations.Should().BeEmpty(
            because: "all MediatR handler types must end in 'Handler'.");
    }

    // ─── Validator coverage ──────────────────────────────────────────────────

    [Fact]
    public void Every_command_should_have_a_validator_or_be_opted_out()
    {
        // Find all concrete commands (types ending in "Command" that implement IRequest).
        var commands = ApplicationAssembly.GetTypes()
            .Where(t => t.Name.EndsWith("Command", StringComparison.Ordinal) &&
                        t is { IsAbstract: false, IsInterface: false, IsGenericTypeDefinition: false } &&
                        IsConcreteRequest(t))
            .ToList();

        // Build set of command types that have an AbstractValidator<TCommand> in Application.
        var validatedCommandTypes = ApplicationAssembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition)
            .Select(GetAbstractValidatorArgument)
            .Where(t => t is not null)
            .ToHashSet()!;

        var missingValidators = commands
            .Where(cmd => !validatedCommandTypes.Contains(cmd) &&
                          cmd.GetCustomAttribute<NoValidatorAttribute>() is null)
            .Select(cmd => cmd.FullName!)
            .OrderBy(name => name)
            .ToList();

        missingValidators.Should().BeEmpty(
            because: "every command must have a FluentValidation validator or be marked with " +
                     "[NoValidator] (with an XML comment explaining why).");
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    /// <summary>Returns true if the type is a concrete (non-abstract) MediatR request.</summary>
    private static bool IsConcreteRequest(Type type)
    {
        if (type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition)
            return false;

        return type.GetInterfaces().Any(i =>
            i.Namespace == "MediatR" &&
            (i.Name == "IRequest" || i.Name == "IRequest`1"));
    }

    /// <summary>Returns true if the type is a concrete MediatR request handler.</summary>
    private static bool IsConcreteRequestHandler(Type type)
    {
        if (type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition)
            return false;

        return type.GetInterfaces().Any(i =>
            i.IsGenericType && i.Namespace == "MediatR" &&
            (i.Name == "IRequestHandler`2" || i.Name == "IRequestHandler`1"));
    }

    /// <summary>
    /// If <paramref name="type"/> directly extends <c>AbstractValidator&lt;T&gt;</c> (or a subclass
    /// of it), returns T; otherwise returns null.
    /// </summary>
    private static Type? GetAbstractValidatorArgument(Type type)
    {
        var baseType = type.BaseType;
        while (baseType is not null)
        {
            if (baseType.IsGenericType &&
                baseType.Name.StartsWith("AbstractValidator`1", StringComparison.Ordinal))
            {
                return baseType.GetGenericArguments()[0];
            }

            baseType = baseType.BaseType;
        }

        return null;
    }
}
