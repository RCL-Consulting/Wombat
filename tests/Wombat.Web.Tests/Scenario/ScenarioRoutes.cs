using System.Reflection;
using System.Text.RegularExpressions;
using Wombat.Web.Components.Layout;
using Wombat.Web.Tests.Navigation;

namespace Wombat.Web.Tests.Scenario;

/// <summary>A route pattern Wombat.Web maps outside the router, and where it is mapped.</summary>
internal sealed record MappedEndpoint(string File, int Line, string Method, string Pattern);

/// <summary>
/// What a scenario step's <c>Route:</c> may name: a routable component's template, or an endpoint Wombat.Web maps.
/// </summary>
internal static partial class ScenarioRoutes
{
    /// <summary>Every <c>@page</c> template, as the router finds them (<see cref="PageAccess.Pages" />).</summary>
    public static IReadOnlyList<string> ComponentTemplates()
        => PageAccess.Pages.Select(page => page.Template).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();

    /// <summary>
    /// The endpoints Wombat.Web maps outside the router: every <c>MapGet</c>, <c>MapPost</c>, <c>MapPut</c>,
    /// <c>MapDelete</c>, <c>MapPatch</c>, <c>MapMethods</c>, <c>MapHealthChecks</c> and <c>MapHub</c> call in its source,
    /// with its pattern. <paramref name="Problems" /> names each call the scan cannot read.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Read from the source, not from a running host. <c>Program.cs</c> maps them in its top-level statements, which only
    /// running <c>Program</c> executes, and <c>Program</c> migrates a PostgreSQL database before it starts; this suite has
    /// no database, and <c>AppTestHost</c> mirrors only the Razor-components and static-asset mappings. A hand-kept list
    /// would outlive an endpoint that is removed; the scan cannot.
    /// </para>
    /// <para>
    /// A pattern is the call's first argument: a string literal, or a constant named by <c>Type.Member</c>
    /// (<c>SessionEnd.Path</c>), read by reflection from the one type of that name in Wombat.Web. A <c>MapGroup</c>, whose
    /// prefix the scan does not compose, and a pattern it cannot resolve, are problems rather than guesses. Comment lines
    /// are skipped. The component routes (<c>MapWombatRazorComponents</c>) and the static assets are not endpoints a step
    /// names.
    /// </para>
    /// </remarks>
    public static (IReadOnlyList<MappedEndpoint> Endpoints, IReadOnlyList<string> Problems) MappedEndpoints()
    {
        var web = Path.Combine(PageAccess.SolutionRoot(), "src", "Wombat.Web");
        var endpoints = new List<MappedEndpoint>();
        var problems = new List<string>();

        var sources = Directory.EnumerateFiles(web, "*.cs", SearchOption.AllDirectories)
            .Where(path => !IsBuildOutput(Path.GetRelativePath(web, path)))
            .Order(StringComparer.Ordinal);

        foreach (var path in sources)
        {
            var file = Path.GetRelativePath(web, path).Replace('\\', '/');

            // Comment lines are blanked, not removed, so a match's line number is the file's.
            var text = string.Join(
                "\n",
                File.ReadAllText(path).Replace("\r\n", "\n").Split('\n')
                    .Select(line => line.TrimStart().StartsWith("//", StringComparison.Ordinal) ? string.Empty : line));

            foreach (Match call in MapCall().Matches(text))
            {
                var line = text[..call.Index].Count(c => c == '\n') + 1;
                var method = call.Groups["method"].Value;
                var argument = call.Groups["pattern"].Value;

                if (method == "Group")
                {
                    problems.Add($"{file}:{line}: MapGroup({argument}) — the scan does not compose a group's prefix; teach it");
                    continue;
                }

                if (Resolve(argument) is { } pattern)
                {
                    endpoints.Add(new MappedEndpoint(file, line, method, pattern.StartsWith('/') ? pattern : "/" + pattern));
                }
                else
                {
                    problems.Add($"{file}:{line}: Map{method}({argument}) — the pattern is neither a string literal nor a " +
                                 "Type.Member constant of Wombat.Web");
                }
            }
        }

        return (endpoints, problems);
    }

    private static bool IsBuildOutput(string relativePath)
    {
        var first = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)[0];
        return first is "bin" or "obj";
    }

    private static string? Resolve(string argument)
    {
        if (argument.StartsWith('"'))
        {
            return argument[1..^1];
        }

        var dot = argument.LastIndexOf('.');
        if (dot <= 0)
        {
            return null;
        }

        var typeName = argument[..dot].Split('.')[^1];
        var memberName = argument[(dot + 1)..];
        var types = typeof(NavMenu).Assembly.GetTypes().Where(type => type.Name == typeName).ToList();
        if (types.Count != 1)
        {
            return null;
        }

        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        return types[0].GetField(memberName, flags)?.GetValue(null) as string
            ?? types[0].GetProperty(memberName, flags)?.GetValue(null) as string;
    }

    /// <summary>
    /// <c>.Map&lt;Method&gt;(</c> and its first argument: a string literal, or a dotted name. <c>MapGroup</c> is matched so
    /// that it can be refused.
    /// </summary>
    [GeneratedRegex("""\.Map(?<method>Get|Post|Put|Delete|Patch|Methods|HealthChecks|Hub|Group)\s*(?:<[^>]*>)?\(\s*(?<pattern>"(?:[^"\\]|\\.)*"|[A-Za-z_][\w.]*)""")]
    private static partial Regex MapCall();
}
