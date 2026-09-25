using System.Reflection;
using FluentAssertions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Wombat.Api.Endpoints;
using Wombat.Web.Services;

namespace Wombat.Architecture.Tests;

/// <summary>
/// The forcing function for T243: a foreign-key refusal is recognised only through <c>PostgresErrors</c>, never by
/// naming one of its codes.
/// </summary>
/// <remarks>
/// <para>
/// PostgreSQL 18, which production runs, reports a delete refused by an <c>ON DELETE RESTRICT</c> key (EF Core's
/// <c>DeleteBehavior.Restrict</c>) as <c>23001</c>; 16 and 17 report it as <c>23503</c>, and the dev server that the
/// default test run uses was 16.10. So a handler that catches <c>SqlState: PostgresErrorCodes.ForeignKeyViolation</c>
/// compiles, passes every test on dev, and misses exactly the refusal it was written for in production.
/// <c>PostgresErrors.IsForeignKeyViolation</c> accepts both.
/// </para>
/// <para>
/// Npgsql's <c>PostgresErrorCodes</c> and <c>PostgresErrors</c>' own constants are <c>const string</c>, so every use is
/// compiled into the caller as the literal itself. The scan therefore reads each method's string literals and each
/// type's constant fields, and finds a check on either code however it was spelled. A caller with a real reason to
/// name one code (none exists) belongs in <see cref="Exempt" /> with that reason.
/// </para>
/// </remarks>
public class ForeignKeyErrorCodeTests
{
    private const string Helper = "Wombat.Application.Common.Persistence.PostgresErrors";

    /// <summary>The two codes a foreign-key refusal can carry: <c>foreign_key_violation</c> and <c>restrict_violation</c>.</summary>
    private static readonly string[] ForeignKeyCodes = ["23503", "23001"];

    /// <summary>Top-level types that may name a foreign-key code, each with its reason.</summary>
    private static readonly Dictionary<string, string> Exempt = new(StringComparer.Ordinal)
    {
        [Helper] = "The one place the codes are named: IsForeignKeyViolation and ForeignKeyViolationStates accept both."
    };

    private static readonly Assembly[] Assemblies =
    [
        typeof(Wombat.Domain.Institutions.Institution).Assembly,
        typeof(Wombat.Application.DependencyInjection).Assembly,
        typeof(Wombat.Infrastructure.DependencyInjection).Assembly,
        typeof(MsfRespondEndpoint).Assembly,
        typeof(IScopedSender).Assembly
    ];

    [Fact]
    public void Only_PostgresErrors_names_a_foreign_key_code()
    {
        var namings = Namings()
            .Where(naming => !Exempt.ContainsKey(naming.TopLevelType))
            .Select(naming => $"{naming.Where} {naming.How} {naming.Code}")
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();

        // Every offender is named in the reason, because the assertion's own message shows only the first.
        namings.Should().BeEmpty(
            "a foreign-key refusal is 23001 on PostgreSQL 18 (production) and 23503 on 16 (dev) for the same " +
            "RESTRICT delete, so a check on one code passes on dev and misses the refusal in production. Ask " +
            "PostgresErrors.IsForeignKeyViolation, which accepts both (T243). Found: {0}",
            string.Join("; ", namings));
    }

    [Theory]
    [InlineData(Loads)]
    [InlineData(Declares)]
    public void The_scan_still_sees_the_helper_name_both_codes(string how)
    {
        // The guard for the test above: if either half of the scan found nothing anywhere, it would pass for the wrong
        // reason. PostgresErrors declares both codes as constants and loads both in IsForeignKeyViolation, so each half
        // must see both there.
        Namings()
            .Where(naming => string.Equals(naming.TopLevelType, Helper, StringComparison.Ordinal))
            .Where(naming => string.Equals(naming.How, how, StringComparison.Ordinal))
            .Select(naming => naming.Code)
            .Distinct(StringComparer.Ordinal)
            .Should().BeEquivalentTo(ForeignKeyCodes, $"guard: the scan must see PostgresErrors {how} both codes");
    }

    [Fact]
    public void Every_exemption_is_a_type_that_still_exists()
        => Exempt.Keys
            .Where(type => Assemblies.All(assembly => assembly.GetType(type) is null))
            .Should().BeEmpty("an exemption naming a type that is gone is a stale claim the next reader would trust");

    /// <summary>A method body loading the code as a string literal.</summary>
    private const string Loads = "loads";

    /// <summary>A type declaring the code as a string constant.</summary>
    private const string Declares = "declares";

    private static List<(string TopLevelType, string Where, string How, string Code)> Namings()
        => Assemblies.SelectMany(NamingsIn).ToList();

    private static List<(string TopLevelType, string Where, string How, string Code)> NamingsIn(Assembly assembly)
    {
        using var module = ModuleDefinition.ReadModule(assembly.Location);

        // Materialised before the module is disposed. Nested types too: an async method's body lives in its state
        // machine, and a lambda's in a display class.
        return module.Types
            .SelectMany(topLevel => Flatten(topLevel).Select(type => (TopLevel: topLevel, Type: type)))
            .SelectMany(pair => Literals(pair.Type)
                .Where(literal => ForeignKeyCodes.Contains(literal.Code, StringComparer.Ordinal))
                .Select(literal => (pair.TopLevel.FullName, $"{pair.Type.FullName}::{literal.Member}", literal.How, literal.Code)))
            .ToList();
    }

    /// <summary>Every string literal a method of <paramref name="type" /> loads, and every string constant it declares.</summary>
    private static IEnumerable<(string Member, string How, string Code)> Literals(TypeDefinition type)
        => type.Methods
            .Where(method => method.HasBody)
            .SelectMany(method => method.Body.Instructions
                .Where(instruction => instruction.OpCode == OpCodes.Ldstr)
                .Select(instruction => (method.Name, Loads, (string)instruction.Operand)))
            .Concat(type.Fields
                .Where(field => field.HasConstant && field.Constant is string)
                .Select(field => (field.Name, Declares, (string)field.Constant)));

    private static IEnumerable<TypeDefinition> Flatten(TypeDefinition type)
        => new[] { type }.Concat(type.NestedTypes.SelectMany(Flatten));
}
