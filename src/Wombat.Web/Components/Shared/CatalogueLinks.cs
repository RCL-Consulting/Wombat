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
/// <c>?created</c> has no value on an EPA's, a College's, a scale's or a curriculum's page; read it with
/// <see cref="HasToken" />. On the College's tree it names the record, as <c>?saved</c> does there (<see cref="TreeRecordOf" />, T394, S13). It survives the EPA page's <c>forceLoad</c> after a create
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

    /// <summary>
    /// The token on a curriculum's page after a clone (T394, flow 16, Q5): <c>cloned=‹the source's id›</c>, so the arriving
    /// page names what it was cloned from ("… cloned from 11.1, with its 15 national items.").
    /// </summary>
    public const string ClonedToken = "cloned";

    private static readonly string[] Tokens = [CreatedToken, AddedToken, SavedToken, BackToken, ClonedToken];

    // Flow 18 (T400, lane A, K13): the institutions, the Administrator's.

    /// <summary>Institutions: <c>/admin/institutions</c>.</summary>
    public const string Institutions = "/admin/institutions";

    /// <summary>Create institution: <c>/admin/institutions/new</c>.</summary>
    public const string NewInstitution = "/admin/institutions/new";

    /// <summary>An institution's record: <c>/admin/institutions/7</c>.</summary>
    public static string Institution(int institutionId) => Invariant($"{Institutions}/{institutionId}");

    /// <summary>
    /// The record a create lands on, <c>/admin/institutions/7?created</c> (K13): the record reads the institution in the
    /// caller's scope, writes "‹name› created.", focuses it, and replaces its address without the token.
    /// </summary>
    public static string InstitutionCreated(int institutionId) => Invariant($"{Institution(institutionId)}?{CreatedToken}");

    // Flow 16 (T394, build-contracts § 0): the College's and the Administrator's pages.

    /// <summary>Colleges, the Administrator's: <c>/admin/colleges</c>.</summary>
    public const string Colleges = "/admin/colleges";

    /// <summary>Create college: <c>/admin/colleges/new</c>.</summary>
    public const string NewCollege = "/admin/colleges/new";

    /// <summary>
    /// The College admin's Specialities, the menu item's address: it sends them to their College's tree, or tells them
    /// their account names no College (S12). Never a link to Colleges (T291 item 2).
    /// </summary>
    public const string Specialities = "/admin/specialities";

    /// <summary>Entrustment scales (the Administrator's; read by the College and the institution, D57).</summary>
    public const string EntrustmentScales = "/admin/entrustment-scales";

    /// <summary>Create scale: <c>/admin/entrustment-scales/new</c>.</summary>
    public const string NewEntrustmentScale = "/admin/entrustment-scales/new";

    /// <summary>Create curriculum: <c>/admin/curricula/new</c>.</summary>
    public const string NewCurriculum = "/admin/curricula/new";

    /// <summary>A College's record: <c>/admin/colleges/4</c>.</summary>
    public static string College(int collegeId) => Invariant($"{Colleges}/{collegeId}");

    /// <summary>A College's record after its create (S13): <c>/admin/colleges/4?created</c>, "College created.".</summary>
    public static string CollegeCreated(int collegeId) => Invariant($"{College(collegeId)}?{CreatedToken}");

    /// <summary>
    /// The College's tree: its specialities, their sub-specialities and the ladder each uses (Q1):
    /// <c>/admin/colleges/2/specialities</c>.
    /// </summary>
    public static string CollegeTree(int collegeId) => Invariant($"{Colleges}/{collegeId}/specialities");

    /// <summary>
    /// The tree at one speciality's section, where the old Sub-specialities list now sends (Q1):
    /// <c>/admin/colleges/2/specialities#speciality-2</c>. The section's heading carries <see cref="TreeSectionId" />.
    /// </summary>
    public static string CollegeTreeAt(int collegeId, int specialityId)
        => Invariant($"{CollegeTree(collegeId)}#{TreeSectionId(specialityId)}");

    /// <summary>The id of a speciality's section heading on the tree: <c>speciality-2</c>.</summary>
    public static string TreeSectionId(int specialityId) => Invariant($"speciality-{specialityId}");

    /// <summary>The tree after a create, naming what was made (S13): <c>/admin/colleges/2/specialities?created=sub:4</c>.</summary>
    public static string TreeCreated(int collegeId, TreeRecord record) => Invariant($"{CollegeTree(collegeId)}?{CreatedToken}={record}");

    /// <summary>The tree after a save (S13): <c>/admin/colleges/2/specialities?saved=speciality:2</c>.</summary>
    public static string TreeSaved(int collegeId, TreeRecord record) => Invariant($"{CollegeTree(collegeId)}?{SavedToken}={record}");

    /// <summary>Create speciality: <c>/admin/colleges/4/specialities/new</c>.</summary>
    public static string NewSpeciality(int collegeId) => Invariant($"{CollegeTree(collegeId)}/new");

    /// <summary>A speciality's record: <c>/admin/colleges/2/specialities/2</c>.</summary>
    public static string Speciality(int collegeId, int specialityId) => Invariant($"{CollegeTree(collegeId)}/{specialityId}");

    /// <summary>
    /// A speciality's old Sub-specialities list: <c>/admin/specialities/2/sub-specialities</c>. Kept as an address that
    /// sends to <see cref="CollegeTreeAt" /> (Q1); no page links it.
    /// </summary>
    public static string SubSpecialities(int specialityId) => Invariant($"{Specialities}/{specialityId}/sub-specialities");

    /// <summary>Create sub-speciality: <c>/admin/specialities/2/sub-specialities/new</c>.</summary>
    public static string NewSubSpeciality(int specialityId) => Invariant($"{SubSpecialities(specialityId)}/new");

    /// <summary>A sub-speciality's record: <c>/admin/specialities/2/sub-specialities/4</c>.</summary>
    public static string SubSpeciality(int specialityId, int subSpecialityId)
        => Invariant($"{SubSpecialities(specialityId)}/{subSpecialityId}");

    /// <summary>A scale's page: <c>/admin/entrustment-scales/3</c>.</summary>
    public static string EntrustmentScale(int scaleId) => Invariant($"{EntrustmentScales}/{scaleId}");

    /// <summary>A scale's page after its create (S13): <c>/admin/entrustment-scales/3?created</c>, "Entrustment scale created.".</summary>
    public static string EntrustmentScaleCreated(int scaleId) => Invariant($"{EntrustmentScale(scaleId)}?{CreatedToken}");

    // Flow 17 (T398, build-contracts § 0): the activity types list and the builder.

    /// <summary>Activity types, the list: <c>/admin/activity-types</c>.</summary>
    public const string ActivityTypes = "/admin/activity-types";

    /// <summary>New activity type, the builder on a type not yet saved: <c>/admin/activity-types/new</c>.</summary>
    public const string NewActivityType = "/admin/activity-types/new";

    /// <summary>An activity type in the builder: <c>/admin/activity-types/23</c>.</summary>
    public static string ActivityType(int activityTypeId) => Invariant($"{ActivityTypes}/{activityTypeId}");

    /// <summary>
    /// An activity type after its first Save draft on <see cref="NewActivityType" /> (T291 item 3):
    /// <c>/admin/activity-types/23?created</c>, "Draft saved." on the arriving page.
    /// </summary>
    public static string ActivityTypeCreated(int activityTypeId) => Invariant($"{ActivityType(activityTypeId)}?{CreatedToken}");

    /// <summary>A curriculum's own page: <c>/admin/curricula/3</c>.</summary>
    public static string Curriculum(int curriculumId) => Invariant($"{Curricula}/{curriculumId}");

    /// <summary>A curriculum's page after its create (Q5): <c>/admin/curricula/3?created</c>.</summary>
    public static string CurriculumCreated(int curriculumId) => Invariant($"{Curriculum(curriculumId)}?{CreatedToken}");

    /// <summary>The new version's page after a clone (Q5): <c>/admin/curricula/4?cloned=2</c>, 2 the version cloned.</summary>
    public static string CurriculumCloned(int curriculumId, int fromCurriculumId)
        => Invariant($"{Curriculum(curriculumId)}?{ClonedToken}={fromCurriculumId}");

    /// <summary>
    /// The record a tree token names (<see cref="TreeCreated" />, <see cref="TreeSaved" />): <c>speciality:2</c> or
    /// <c>sub:4</c>; null when absent or malformed. The tree reads the record again in the caller's scope before it writes
    /// a result: a stale or foreign id writes nothing and focuses nothing (F1's rule).
    /// </summary>
    public static TreeRecord? TreeRecordOf(string uri, string name)
    {
        ArgumentNullException.ThrowIfNull(uri);
        if (!QueryHelpers.ParseQuery(QueryOf(uri)).TryGetValue(name, out var values))
        {
            return null;
        }

        var parts = values.ToString().Split(':');
        if (parts.Length != 2
            || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var id))
        {
            return null;
        }

        return parts[0] switch
        {
            TreeRecord.SpecialityPrefix => TreeRecord.OfSpeciality(id),
            TreeRecord.SubSpecialityPrefix => TreeRecord.OfSubSpeciality(id),
            _ => null
        };
    }

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
    /// <paramref name="uri" /> without the five tokens, keeping its path and any other query part, for the page to replace
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

/// <summary>What a tree token names: a speciality or a sub-speciality (T394, flow 16, S13).</summary>
public enum TreeRecordKind
{
    /// <summary>A speciality: <c>speciality:2</c>.</summary>
    Speciality,

    /// <summary>A sub-speciality: <c>sub:4</c>.</summary>
    SubSpeciality
}

/// <summary>A record on the College's tree, as a token carries it: <c>speciality:2</c>, <c>sub:4</c> (T394, S13).</summary>
public readonly record struct TreeRecord(TreeRecordKind Kind, int Id)
{
    /// <summary>A speciality's prefix.</summary>
    public const string SpecialityPrefix = "speciality";

    /// <summary>A sub-speciality's prefix.</summary>
    public const string SubSpecialityPrefix = "sub";

    /// <summary>A speciality.</summary>
    public static TreeRecord OfSpeciality(int id) => new(TreeRecordKind.Speciality, id);

    /// <summary>A sub-speciality.</summary>
    public static TreeRecord OfSubSpeciality(int id) => new(TreeRecordKind.SubSpeciality, id);

    /// <inheritdoc />
    public override string ToString()
        => string.Create(CultureInfo.InvariantCulture, $"{(Kind == TreeRecordKind.Speciality ? SpecialityPrefix : SubSpecialityPrefix)}:{Id}");
}
