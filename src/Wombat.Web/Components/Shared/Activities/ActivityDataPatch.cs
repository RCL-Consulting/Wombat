using System.Text.Json.Nodes;

namespace Wombat.Web.Components.Shared.Activities;

/// <summary>
/// Builds the data patch a page sends alongside a workflow transition (T070).
/// </summary>
/// <remarks>
/// <para>
/// The patch is a diff, not a post-back of the whole form. It carries only writable keys whose
/// value actually changed, which is what keeps <c>ActivityService.MergeWritableKeys</c> from
/// rejecting the transition: that merge throws on any patched key the actor does not own unless
/// the value is byte-for-byte what is already stored.
/// </para>
/// <para>
/// This lives in a plain <c>.cs</c> file rather than a component so it can be unit-tested without
/// a renderer.
/// </para>
/// </remarks>
public static class ActivityDataPatch
{
    /// <summary>
    /// Returns the JSON object to send as <c>DataPatchJson</c>, or <c>null</c> when there is
    /// nothing to send — no writable keys, or no writable key whose value differs. A writable key
    /// the actor cleared is emitted as an explicit JSON <c>null</c>, which
    /// <c>SchemaValidator</c> treats as absent.
    /// </summary>
    /// <param name="storedJson">The activity's persisted <c>DataJson</c>.</param>
    /// <param name="workingJson">The form's current data, as <c>ActivityForm</c> last emitted it.</param>
    /// <param name="writableFieldKeys">The keys this actor may write in the current state.</param>
    /// <exception cref="InvalidOperationException">Either document is not a JSON object.</exception>
    public static string? Build(string? storedJson, string? workingJson, IReadOnlySet<string>? writableFieldKeys)
    {
        if (writableFieldKeys is null || writableFieldKeys.Count == 0)
        {
            return null;
        }

        var stored = ParseObject(storedJson, nameof(storedJson));
        var working = ParseObject(workingJson, nameof(workingJson));

        // Iterating the documents rather than the (unordered) key set keeps the patch deterministic:
        // written keys in form order first, then cleared keys in stored order.
        var patch = new JsonObject();

        foreach (var property in working)
        {
            if (!writableFieldKeys.Contains(property.Key))
            {
                continue;
            }

            stored.TryGetPropertyValue(property.Key, out var storedValue);
            if (JsonNode.DeepEquals(storedValue, property.Value))
            {
                continue;
            }

            patch[property.Key] = property.Value?.DeepClone();
        }

        foreach (var property in stored)
        {
            if (!writableFieldKeys.Contains(property.Key) ||
                property.Value is null ||
                patch.ContainsKey(property.Key))
            {
                continue;
            }

            if (!working.TryGetPropertyValue(property.Key, out var workingValue) || workingValue is null)
            {
                // Present before, gone now: the actor cleared it. Send null so the merge overwrites
                // the stored value — omitting the key would silently leave the old value in place.
                patch[property.Key] = null;
            }
        }

        return patch.Count == 0 ? null : patch.ToJsonString();
    }

    private static JsonObject ParseObject(string? json, string parameterName)
    {
        var node = JsonNode.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);

        return node as JsonObject
            ?? throw new InvalidOperationException($"Activity data '{parameterName}' must be a JSON object.");
    }
}
