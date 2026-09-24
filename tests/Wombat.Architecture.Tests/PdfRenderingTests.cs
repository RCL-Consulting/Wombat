using System.Reflection;
using FluentAssertions;
using Mono.Cecil;
using Wombat.Api.Endpoints;
using Wombat.Web.Services;

namespace Wombat.Architecture.Tests;

/// <summary>
/// T200: every document is rendered through <c>QuestPdfRenderer</c>, which lets one render run at a time in the process.
/// </summary>
/// <remarks>
/// Two QuestPDF renders that overlap can each lose their text layer: the fonts' ToUnicode maps come out mapping every
/// glyph but one to U+0000. Nothing in the type system stops a new export (a clinical-audit PDF, a committee pack) from
/// calling <c>GeneratePdf</c> itself, and it would pass its own tests and corrupt the exports it happens to overlap.
/// </remarks>
public class PdfRenderingTests
{
    private const string Renderer = "Wombat.Infrastructure.Reporting.QuestPdfRenderer";

    /// <summary>
    /// QuestPDF's render entry points: <c>GeneratePdf</c>, <c>GenerateImages</c>, <c>GenerateSvg</c>, <c>GenerateXps</c> and
    /// their "and show" forms, and the Companion previewer.
    /// </summary>
    private static readonly string[] RenderingTypes =
    [
        "QuestPDF.Fluent.GenerateExtensions",
        "QuestPDF.Companion.CompanionExtensions"
    ];

    private static readonly Assembly[] Assemblies =
    [
        typeof(Wombat.Application.DependencyInjection).Assembly,
        typeof(Wombat.Infrastructure.DependencyInjection).Assembly,
        typeof(MsfRespondEndpoint).Assembly,
        typeof(IScopedSender).Assembly
    ];

    [Fact]
    public void Only_the_shared_renderer_renders_a_document()
    {
        Renders()
            .Where(render => !string.Equals(render.TopLevelType, Renderer, StringComparison.Ordinal))
            .Select(render => $"{render.Method} calls {render.Call}")
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .Should().BeEmpty(
                "a document is rendered only through QuestPdfRenderer, which lets one render run at a time in the " +
                "process; two that overlap can lose their text layer (T200)");
    }

    [Fact]
    public void The_shared_renderer_is_still_where_documents_are_rendered()
    {
        // The guard for the test above: if the scan found no render anywhere, it would pass for the wrong reason.
        Renders()
            .Where(render => string.Equals(render.TopLevelType, Renderer, StringComparison.Ordinal))
            .Select(render => render.Call)
            .Should().Contain("QuestPDF.Fluent.GenerateExtensions::GeneratePdf", "guard: the IL scan must see the renderer's own call");
    }

    private static List<(string TopLevelType, string Method, string Call)> Renders()
        => Assemblies.SelectMany(RendersIn).ToList();

    private static List<(string TopLevelType, string Method, string Call)> RendersIn(Assembly assembly)
    {
        using var module = ModuleDefinition.ReadModule(assembly.Location);

        // Materialised before the module is disposed. Nested types too: an async method's body lives in its state
        // machine, and a lambda's in a display class.
        return module.Types
            .SelectMany(topLevel => Flatten(topLevel).Select(type => (TopLevel: topLevel, Type: type)))
            .SelectMany(pair => pair.Type.Methods
                .Where(method => method.HasBody)
                .SelectMany(method => method.Body.Instructions
                    .Select(instruction => instruction.Operand as MethodReference)
                    .Where(call => call?.DeclaringType is not null && RenderingTypes.Contains(call.DeclaringType.FullName))
                    .Select(call => (
                        pair.TopLevel.FullName,
                        $"{pair.Type.FullName}::{method.Name}",
                        $"{call!.DeclaringType.FullName}::{call.Name}"))))
            .ToList();
    }

    private static IEnumerable<TypeDefinition> Flatten(TypeDefinition type)
        => new[] { type }.Concat(type.NestedTypes.SelectMany(Flatten));
}
