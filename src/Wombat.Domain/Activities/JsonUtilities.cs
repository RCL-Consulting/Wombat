using System.Text.Json;

namespace Wombat.Domain.Activities;

internal static class JsonUtilities
{
    public static string Normalize(string json)
    {
        using var document = JsonDocument.Parse(json);
        return Normalize(document.RootElement);
    }

    public static string Normalize(JsonElement element)
    {
        return JsonSerializer.Serialize(element);
    }
}
