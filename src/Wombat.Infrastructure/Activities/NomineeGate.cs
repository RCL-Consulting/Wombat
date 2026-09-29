using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Identity;

namespace Wombat.Infrastructure.Activities;

/// <summary>
/// The write-path half of T102: a nominee field may name only someone <see cref="NomineeDirectory" /> lists for the
/// activity. Which fields are put to it on a given write is the caller's decision (see
/// <c>ActivityService.NomineeFieldsToJudge</c>); this class only judges them.
/// </summary>
/// <remarks>
/// <para>
/// Every read is awaited before the caller's first mutation, and the gate itself mutates nothing, so a refusal leaves
/// the request's DbContext clean. The audit pipeline saves that context from its catch; anything dirty would commit.
/// </para>
/// <para>
/// One refusal message for every failed condition. It does not say whether the person lacks the role, sits in another
/// institution, is deactivated or does not exist, so it tells a caller nothing the picker does not already show, and
/// it names the person only when they belong to the activity's own institution.
/// </para>
/// </remarks>
internal static class NomineeGate
{
    /// <summary>
    /// The nominee fields whose value differs between two payloads. A field absent from one side is compared as JSON
    /// null. Parsed values are compared, never the stored text: PostgreSQL re-renders <c>jsonb</c>.
    /// </summary>
    public static IReadOnlySet<string> ChangedFields(
        IEnumerable<string> nomineeFields,
        string storedDataJson,
        string newDataJson)
    {
        using var stored = JsonDocument.Parse(storedDataJson);
        using var updated = JsonDocument.Parse(newDataJson);

        var changed = new HashSet<string>(StringComparer.Ordinal);
        foreach (var field in nomineeFields)
        {
            if (!JsonElement.DeepEquals(ValueOrNull(stored.RootElement, field), ValueOrNull(updated.RootElement, field)))
            {
                changed.Add(field);
            }
        }

        return changed;
    }

    /// <summary>
    /// Whether the field names someone, as the actor grammar reads it: a non-empty JSON string.
    /// </summary>
    public static bool NamesSomeone(string dataJson, string field)
        => !string.IsNullOrEmpty(ActorRuleMatcher.ReadUserFieldValue(dataJson, field));

    public static async Task EnsurePermittedAsync(
        IApplicationDbContext dbContext,
        FormSchema schema,
        IReadOnlyDictionary<string, IReadOnlyList<string>> requiredRolesByField,
        IReadOnlySet<string> fieldsToJudge,
        string dataJson,
        int? institutionId,
        string subjectUserId,
        CancellationToken cancellationToken)
    {
        if (fieldsToJudge.Count == 0)
        {
            return;
        }

        using var document = JsonDocument.Parse(dataJson);

        // Schema order, so a payload with two bad nominees is always refused on the same one.
        foreach (var field in OrderBySchema(schema, fieldsToJudge))
        {
            var element = ValueOrNull(document.RootElement, field);
            if (element.ValueKind == JsonValueKind.Null ||
                (element.ValueKind == JsonValueKind.String && string.IsNullOrEmpty(element.GetString())))
            {
                // Nothing is being nominated. Whether the field may be empty is the schema validator's question.
                continue;
            }

            var label = schema.FieldLabel(field);
            var roles = requiredRolesByField.TryGetValue(field, out var declared) ? declared : [WombatRoles.Assessor];

            if (element.ValueKind != JsonValueKind.String)
            {
                // Hidden by show_if, a field skips the schema validator's type check, but the actor grammar still reads it.
                throw new ActivityFieldsRefusedException($"{label}: this must name a person, by choosing them from the list.", [field]);
            }

            var userId = element.GetString()!;
            if (await NomineeDirectory.IsEligibleAsync(dbContext, userId, institutionId, roles, subjectUserId, cancellationToken))
            {
                continue;
            }

            // T342 (C4, E2, G4): one neutral sentence for every refused nominee, "Assessor: Mohammed Patel cannot be named
            // as an assessor. Choose someone else." Until G4 a named person "can no longer" be named, which told a caller
            // that someone once could (a lockout) and was wrong for someone who never could. The person is named only
            // within the activity's institution, as before; anyone else is "that person".
            var name = await NameWithinInstitutionAsync(dbContext, userId, institutionId, cancellationToken);
            throw new ActivityFieldsRefusedException(
                $"{label}: {name ?? "that person"} cannot be named as {DescribeRoles(roles)}. Choose someone else.",
                [field]);
        }
    }

    private static async Task<string?> NameWithinInstitutionAsync(
        IApplicationDbContext dbContext,
        string userId,
        int? institutionId,
        CancellationToken cancellationToken)
    {
        if (institutionId is null)
        {
            return null;
        }

        var user = await dbContext.Set<WombatIdentityUser>()
            .AsNoTracking()
            .Where(entity => entity.Id == userId && entity.InstitutionId == institutionId.Value)
            .Select(entity => new { entity.FirstName, entity.LastName })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return null;
        }

        var name = string.Join(" ", new[] { user.FirstName, user.LastName }.Where(part => !string.IsNullOrWhiteSpace(part)));
        return string.IsNullOrWhiteSpace(name) ? null : name;
    }

    // The shared labels (T335): every role a field may require (WombatRoles.Nominable) reads as this gate's own map read it,
    // in running text since T342: "an assessor", "an assessor and a committee member".
    private static string DescribeRoles(IReadOnlyList<string> roles)
        => string.Join(" and ", roles.Select(role => WithArticle(WombatRoleLabels.For(role).ToLowerInvariant())));

    private static string WithArticle(string noun)
        => noun.Length > 0 && "aeiou".Contains(noun[0]) ? $"an {noun}" : $"a {noun}";

    private static IEnumerable<string> OrderBySchema(FormSchema schema, IReadOnlySet<string> fields)
    {
        var declared = schema.Sections
            .SelectMany(section => section.Fields)
            .Select(field => field.Key)
            .Where(fields.Contains)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        return declared.Concat(fields.Except(declared, StringComparer.Ordinal).Order(StringComparer.Ordinal));
    }

    private static JsonElement ValueOrNull(JsonElement root, string field)
        => root.ValueKind == JsonValueKind.Object && root.TryGetProperty(field, out var value)
            ? value
            : NullElement;

    private static readonly JsonElement NullElement = JsonDocument.Parse("null").RootElement.Clone();
}
