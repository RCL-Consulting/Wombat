using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Epas;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Epas;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Identity;

namespace Wombat.Infrastructure.Reporting;

/// <summary>
/// What the reference fields of the exported activities name, by the value each stores, so the activities section
/// prints what the activity's own page shows and never an internal id (T199).
/// </summary>
/// <remarks>
/// <para>
/// Three kinds of field store an id:
/// </para>
/// <list type="bullet">
/// <item>An <c>epa</c> field stores the EPA's id. It prints as the activity's EPA picker labels it:
/// "PAED-003 — Title", with "(no longer in use)" after an EPA that is not in force (<see cref="EpaOptionLabel" />).
/// The export printed "EPA: 3".</item>
/// <item>A <c>user</c> field stores the person's account id. It prints as their name, as the committee section names
/// the people present (T142), and never their email address: the export is a document that leaves the application.</item>
/// <item>A campaign's evidence record (<see cref="MsfEvidenceKinds.IsEvidenceType" />) names its campaign under
/// <see cref="MsfCampaignCoverage.CampaignIdField" />, a number field. It prints as the MSF section of the same export
/// heads the campaign's report, "Annual MSF (Campaign #3)", so a reader can find the one from the other. The export
/// printed "MSF campaign: 3".</item>
/// </list>
/// <para>
/// A value that names nothing that can be found prints as the page would show it: an EPA id as stored (a value no option
/// declares prints as stored, T191), a campaign as "Campaign #3", and a person as "Unknown person", the words the
/// nominee picker uses, because an account id is never words.
/// </para>
/// <para>
/// <b>Scope.</b> Only values stored on the exported activities are looked up, and those activities are the ones the
/// caller may read (<c>WhereReadableBy</c>, T101), whose pages show the same names. A campaign is looked up only among
/// the trainee's own.
/// </para>
/// </remarks>
internal sealed record PortfolioFieldReferences(
    IReadOnlyDictionary<int, string> EpaLabels,
    IReadOnlyDictionary<string, string> PersonNames,
    IReadOnlyDictionary<int, string> CampaignLabels)
{
    /// <summary>What a person field prints when its account cannot be found. The nominee picker's words.</summary>
    public const string UnknownPerson = "Unknown person";

    public static PortfolioFieldReferences Empty { get; } = new(
        new Dictionary<int, string>(),
        new Dictionary<string, string>(StringComparer.Ordinal),
        new Dictionary<int, string>());

    /// <summary>
    /// Whether this field of an activity of this type stores an id that <see cref="Label" /> names: every <c>epa</c> and
    /// <c>user</c> field, and the campaign field of a campaign's evidence record.
    /// </summary>
    public static bool IsReference(FormField field, string? activityTypeKey)
        => field.Type is FieldType.Epa or FieldType.User || NamesCampaign(field, activityTypeKey);

    /// <summary>What a reference field's stored value prints as. See the remarks for each kind and its fallback.</summary>
    public string Label(FormField field, string? activityTypeKey, string storedValue)
    {
        if (field.Type == FieldType.User)
        {
            return PersonNames.TryGetValue(storedValue, out var name) ? name : UnknownPerson;
        }

        if (field.Type == FieldType.Epa)
        {
            return TryParseId(storedValue) is int epaId && EpaLabels.TryGetValue(epaId, out var epa) ? epa : storedValue;
        }

        if (NamesCampaign(field, activityTypeKey))
        {
            return TryParseId(storedValue) is int campaignId
                ? CampaignLabels.TryGetValue(campaignId, out var campaign) ? campaign : CampaignLabel(null, campaignId)
                : storedValue;
        }

        return storedValue;
    }

    /// <summary>A campaign as the MSF section heads its report: "Annual MSF (Campaign #3)".</summary>
    public static string CampaignLabel(string? templateName, int campaignId)
        => string.IsNullOrWhiteSpace(templateName)
            ? $"Campaign #{campaignId.ToString(CultureInfo.InvariantCulture)}"
            : $"{templateName} (Campaign #{campaignId.ToString(CultureInfo.InvariantCulture)})";

    /// <summary>
    /// Every EPA, person and campaign the exported activities' reference fields store, named in three reads, one per
    /// kind, and none when no activity stores one.
    /// </summary>
    /// <param name="schemas">Each pin's schema, or no entry where it has none that parses; those activities print raw.</param>
    /// <param name="subjectUserId">The trainee the export is about. A campaign is looked up only among theirs.</param>
    public static async Task<PortfolioFieldReferences> LoadAsync(
        IApplicationDbContext dbContext,
        IEnumerable<Activity> activities,
        IReadOnlyDictionary<(int ActivityTypeId, int Version), FormSchema> schemas,
        string subjectUserId,
        CancellationToken cancellationToken)
    {
        var epaIds = new HashSet<int>();
        var userIds = new HashSet<string>(StringComparer.Ordinal);
        var campaignIds = new HashSet<int>();

        foreach (var activity in activities)
        {
            if (!schemas.TryGetValue((activity.ActivityTypeId, activity.SchemaVersion), out var schema))
            {
                continue;
            }

            var typeKey = activity.ActivityType?.Key;
            var references = schema.Sections
                .SelectMany(section => section.Fields)
                .Where(field => IsReference(field, typeKey))
                .ToList();
            if (references.Count == 0)
            {
                continue;
            }

            using var data = TryParse(activity.DataJson);
            if (data is null || data.RootElement.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            foreach (var field in references)
            {
                if (!data.RootElement.TryGetProperty(field.Key, out var element) || StoredText(element) is not { } stored)
                {
                    continue;
                }

                if (field.Type == FieldType.User)
                {
                    userIds.Add(stored);
                }
                else if (TryParseId(stored) is int id)
                {
                    (field.Type == FieldType.Epa ? epaIds : campaignIds).Add(id);
                }
            }
        }

        var epaLabels = epaIds.Count == 0
            ? new Dictionary<int, string>()
            : (await dbContext.Set<Epa>()
                .AsNoTracking()
                .Where(epa => epaIds.Contains(epa.Id))
                .Select(epa => new { epa.Id, epa.Code, epa.Title, epa.IsActive })
                .ToListAsync(cancellationToken))
                .ToDictionary(epa => epa.Id, epa => EpaOptionLabel.For(epa.Code, epa.Title, epa.IsActive));

        var personNames = userIds.Count == 0
            ? new Dictionary<string, string>(StringComparer.Ordinal)
            : (await dbContext.Set<WombatIdentityUser>()
                .AsNoTracking()
                .Where(user => userIds.Contains(user.Id))
                .Select(user => new { user.Id, user.FirstName, user.LastName })
                .ToListAsync(cancellationToken))
                .Select(user => (user.Id, Name: $"{user.FirstName} {user.LastName}".Trim()))
                .Where(user => user.Name.Length > 0)
                .ToDictionary(user => user.Id, user => user.Name, StringComparer.Ordinal);

        var campaignLabels = campaignIds.Count == 0
            ? new Dictionary<int, string>()
            : (await dbContext.Set<MsfCampaign>()
                .AsNoTracking()
                .Where(campaign => campaignIds.Contains(campaign.Id) && campaign.SubjectUserId == subjectUserId)
                .Select(campaign => new { campaign.Id, TemplateName = campaign.Template.Name })
                .ToListAsync(cancellationToken))
                .ToDictionary(campaign => campaign.Id, campaign => CampaignLabel(campaign.TemplateName, campaign.Id));

        return new PortfolioFieldReferences(epaLabels, personNames, campaignLabels);
    }

    private static bool NamesCampaign(FormField field, string? activityTypeKey)
        => MsfEvidenceKinds.IsEvidenceType(activityTypeKey) &&
           string.Equals(field.Key, MsfCampaignCoverage.CampaignIdField, StringComparison.Ordinal);

    /// <summary>
    /// The stored value as text, or null when there is none to name: a string as it is, a number as written (the release
    /// writes the campaign as one).
    /// </summary>
    internal static string? StoredText(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.String when !string.IsNullOrWhiteSpace(element.GetString()) => element.GetString()!.Trim(),
        JsonValueKind.Number => element.GetRawText(),
        _ => null
    };

    private static int? TryParseId(string value)
        => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) ? id : null;

    private static JsonDocument? TryParse(string json)
    {
        try
        {
            return JsonDocument.Parse(json);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
