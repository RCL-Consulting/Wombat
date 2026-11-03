using System.Globalization;
using Microsoft.AspNetCore.WebUtilities;

namespace Wombat.Web.Components.Shared;

/// <summary>
/// The catalogue pages' addresses (T392, flow 15, build-contracts § 0), so no page writes one by hand: Curricula, EPAs, an
/// EPA's page, a curriculum's items and an item's page of its own (Q14), and the one-shot tokens a result crosses a page
/// change by (F1, M5, M6), as <c>OnboardingLinks.Admitted</c> and <c>BackFromAdmit</c> do.
/// </summary>
/// <remarks>
/// <para>
/// A token carries an id only, never a name or a sentence. The arriving page reads its record again in the caller's scope,
/// writes the result itself (<c>"EPA saved."</c> for <see cref="EpaCreated" />, <c>"Curriculum item added."</c> for
/// <see cref="ItemAdded" />, <c>"Curriculum item saved."</c> for <see cref="ItemSaved" />), focuses it (for
/// <see cref="BackFromItem" />, no result: the row's Edit, or View on a row the caller may not change, takes the focus), and then replaces its address with
/// <see cref="WithoutTokens" />, so a reload says nothing. An item id the curriculum does not hold for the caller (stale, or
/// another institution's) writes nothing and focuses nothing.
/// </para>
/// <para>
/// <c>?created</c> has no value; read it with <see cref="HasToken" />. It survives the EPA page's <c>forceLoad</c> after a create
/// (<c>EpaEdit.razor</c>), which a scoped flash message would not.
/// </para>
/// </remarks>
public static class CatalogueLinks
{
    /// <summary>Curricula, where an institution adopts and keeps its curricula (Q1, Q18): <c>/admin/curricula</c>.</summary>
    public const string Curricula = "/admin/curricula";

    /// <summary>EPAs: <c>/admin/epas</c>.</summary>
    public const string Epas = "/admin/epas";

    /// <summary>Create EPA: <c>/admin/epas/new</c>.</summary>
    public const string NewEpa = "/admin/epas/new";

    /// <summary>The token after a create (M5): <c>created</c>.</summary>
    public const string CreatedToken = "created";

    /// <summary>The token after an item's add (M6): <c>added</c>.</summary>
    public const string AddedToken = "added";

    /// <summary>The token after an item's save (M6): <c>saved</c>.</summary>
    public const string SavedToken = "saved";

    /// <summary>The token after Cancel on an item's page (M6): <c>back</c>.</summary>
    public const string BackToken = "back";

    private static readonly string[] Tokens = [CreatedToken, AddedToken, SavedToken, BackToken];

    /// <summary>An EPA's page: <c>/admin/epas/17</c>.</summary>
    public static string Epa(int epaId) => Invariant($"{Epas}/{epaId}");

    /// <summary>An EPA's page after its create (F1, M5): <c>/admin/epas/17?created</c>.</summary>
    public static string EpaCreated(int epaId) => Invariant($"{Epas}/{epaId}?{CreatedToken}");

    /// <summary>A curriculum's items, the list that is the page (Q14): <c>/admin/curricula/4/items</c>.</summary>
    public static string Items(int curriculumId) => Invariant($"{Curricula}/{curriculumId}/items");

    /// <summary>Add an item, a page of its own (Q14, F3): <c>/admin/curricula/4/items/new</c>.</summary>
    public static string NewItem(int curriculumId) => Invariant($"{Items(curriculumId)}/new");

    /// <summary>An item's page of its own, to edit it (Q14, F3): <c>/admin/curricula/4/items/31</c>.</summary>
    public static string Item(int curriculumId, int itemId) => Invariant($"{Items(curriculumId)}/{itemId}");

    /// <summary>The items after an add (M6): <c>/admin/curricula/4/items?added=31</c>.</summary>
    public static string ItemAdded(int curriculumId, int itemId) => Invariant($"{Items(curriculumId)}?{AddedToken}={itemId}");

    /// <summary>The items after a save (M6): <c>/admin/curricula/4/items?saved=31</c>.</summary>
    public static string ItemSaved(int curriculumId, int itemId) => Invariant($"{Items(curriculumId)}?{SavedToken}={itemId}");

    /// <summary>The items after Cancel on an item's page, the row's Edit (or View) focused (M6): <c>/admin/curricula/4/items?back=31</c>.</summary>
    public static string BackFromItem(int curriculumId, int itemId) => Invariant($"{Items(curriculumId)}?{BackToken}={itemId}");

    /// <summary>Whether <paramref name="uri" /> carries the token <paramref name="name" />, with or without a value.</summary>
    public static bool HasToken(string uri, string name)
    {
        ArgumentNullException.ThrowIfNull(uri);
        return QueryHelpers.ParseQuery(QueryOf(uri)).ContainsKey(name);
    }

    /// <summary>The id the token <paramref name="name" /> carries in <paramref name="uri" />, or null when absent or not a number.</summary>
    public static int? TokenId(string uri, string name)
    {
        ArgumentNullException.ThrowIfNull(uri);
        return QueryHelpers.ParseQuery(QueryOf(uri)).TryGetValue(name, out var values)
            && int.TryParse(values.ToString(), NumberStyles.None, CultureInfo.InvariantCulture, out var id)
                ? id
                : null;
    }

    /// <summary>
    /// <paramref name="uri" /> without the four tokens, keeping its path and any other query part, for the page to replace
    /// its address with once it has written the result, so a reload says nothing (F1).
    /// </summary>
    public static string WithoutTokens(string uri)
    {
        ArgumentNullException.ThrowIfNull(uri);
        var queryStart = uri.IndexOf('?', StringComparison.Ordinal);
        if (queryStart < 0)
        {
            return uri;
        }

        var fragmentStart = uri.IndexOf('#', queryStart);
        var fragment = fragmentStart < 0 ? string.Empty : uri[fragmentStart..];
        var path = uri[..queryStart];
        var kept = QueryHelpers.ParseQuery(QueryOf(uri))
            .Where(pair => !Tokens.Contains(pair.Key, StringComparer.Ordinal))
            .SelectMany(pair => pair.Value.Count == 0
                ? [Uri.EscapeDataString(pair.Key)]
                : pair.Value.Select(value => $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(value ?? string.Empty)}"))
            .ToList();

        return kept.Count == 0 ? path + fragment : $"{path}?{string.Join('&', kept)}{fragment}";
    }

    private static string QueryOf(string uri)
    {
        var queryStart = uri.IndexOf('?', StringComparison.Ordinal);
        if (queryStart < 0)
        {
            return string.Empty;
        }

        var fragmentStart = uri.IndexOf('#', queryStart);
        return fragmentStart < 0 ? uri[queryStart..] : uri[queryStart..fragmentStart];
    }

    private static string Invariant(FormattableString value) => value.ToString(CultureInfo.InvariantCulture);
}
