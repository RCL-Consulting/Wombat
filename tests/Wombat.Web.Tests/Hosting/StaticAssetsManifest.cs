using System.Text.Json;
using Wombat.Web.Components;

namespace Wombat.Web.Tests.Hosting;

/// <summary>
/// One uncompressed route in Wombat.Web's static-assets endpoint manifest, the file <c>MapStaticAssets()</c> builds its
/// endpoints from.
/// </summary>
/// <param name="Route">The URL path, without its leading slash: <c>app.css</c> or <c>app.wtr6tzkipp.css</c>.</param>
/// <param name="Label">
/// The file's own path, on a fingerprinted route only; <c>@Assets[label]</c> resolves to that route. Null on the bare
/// route.
/// </param>
/// <param name="CacheControl">The Cache-Control the endpoint answers with.</param>
internal sealed record StaticAssetRoute(string Route, string? Label, string? CacheControl);

internal static class StaticAssetsManifest
{
    /// <summary>
    /// Every uncompressed route the build declared. A compressed variant shares its route and differs only by a
    /// Content-Encoding selector, so it adds nothing here.
    /// </summary>
    public static IReadOnlyDictionary<string, StaticAssetRoute> Load()
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            $"{typeof(App).Assembly.GetName().Name}.staticwebassets.endpoints.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));

        var routes = new Dictionary<string, StaticAssetRoute>(StringComparer.Ordinal);
        foreach (var endpoint in document.RootElement.GetProperty("Endpoints").EnumerateArray())
        {
            if (endpoint.GetProperty("Selectors").GetArrayLength() > 0)
            {
                continue;
            }

            var route = endpoint.GetProperty("Route").GetString()!;
            routes[route] = new StaticAssetRoute(
                route,
                Named(endpoint.GetProperty("EndpointProperties"), "label"),
                Named(endpoint.GetProperty("ResponseHeaders"), "Cache-Control"));
        }

        return routes;
    }

    private static string? Named(JsonElement pairs, string name)
        => pairs.EnumerateArray()
            .Where(pair => pair.GetProperty("Name").GetString() == name)
            .Select(pair => pair.GetProperty("Value").GetString())
            .FirstOrDefault();
}
