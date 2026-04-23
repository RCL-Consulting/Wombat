using System.Reflection;
using System.Security.Claims;
using System.Text.Json;

namespace Wombat.Application.Audit;

/// <summary>
/// Serializes a command request to a compact JSON summary, redacting any properties
/// marked with <see cref="RedactAttribute"/>. Output is bounded to ~2 KB by design —
/// large payloads (file bytes, blob content) should be annotated with [Redact].
/// </summary>
public static class AuditPayloadSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
    };

    public static string Serialize(object request)
    {
        var type = request.GetType();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var dict = new Dictionary<string, object?>(properties.Length);

        foreach (var prop in properties)
        {
            if (prop.GetCustomAttribute<RedactAttribute>() is not null)
            {
                dict[prop.Name] = "[REDACTED]";
            }
            else if (typeof(ClaimsPrincipal).IsAssignableFrom(prop.PropertyType) ||
                     typeof(ClaimsIdentity).IsAssignableFrom(prop.PropertyType))
            {
                // ClaimsPrincipal / ClaimsIdentity have self-referencing Claim.Subject graphs
                // that blow up System.Text.Json's cycle detection. The actor's identity is
                // captured separately on AuditEntry (ActorUserId / ActorDisplay), so nothing
                // useful is lost by summarising it here.
                dict[prop.Name] = "[PRINCIPAL]";
            }
            else
            {
                try
                {
                    dict[prop.Name] = prop.GetValue(request);
                }
                catch
                {
                    dict[prop.Name] = "[ERROR_READING]";
                }
            }
        }

        return JsonSerializer.Serialize(dict, Options);
    }
}
