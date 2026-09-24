using Wombat.Domain.Activities.Schema;
using Wombat.Infrastructure.Activities;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.TestHelpers;

/// <summary>
/// What <c>ActivityService</c> stamps on <c>Activity.EpaId</c> (T137), for a fixture that adds an activity row directly
/// rather than through the service.
/// </summary>
/// <remarks>
/// The committee sampling report and the trajectory read an activity's EPA from that stamp alone (T135), so a fixture row
/// without it is a row about no EPA. This stamps it the way the product does: from the PINNED version's
/// <c>evidence_epa_field</c> (the type's own columns when the fixture wrote no version row, as
/// <c>RatedEvidenceProfiles</c> falls back), read by the product's one resolver, and kept only when that EPA exists. It
/// is not a second implementation of the rule: <see cref="EvidenceEpaResolver.Declared" /> is the resolver itself, and
/// the existence check is what <see cref="EvidenceEpaResolver.ResolveAsync" /> adds to it. A pinned schema that is
/// missing or does not parse stamps nothing, since the product could not have created a row against it.
/// </remarks>
internal static class EvidenceEpaStamp
{
    public static int? For(ApplicationDbContext dbContext, int activityTypeId, int schemaVersion, string dataJson)
    {
        var pinnedSchemaJson = dbContext.ActivityTypeVersions
            .Where(version => version.ActivityTypeId == activityTypeId && version.Version == schemaVersion)
            .Select(version => version.SchemaJson)
            .FirstOrDefault()
            ?? dbContext.ActivityTypes
                .Where(type => type.Id == activityTypeId)
                .Select(type => type.SchemaJson)
                .Single();

        if (string.IsNullOrWhiteSpace(pinnedSchemaJson))
        {
            return null;
        }

        FormSchema schema;
        try
        {
            schema = FormSchemaParser.Parse(pinnedSchemaJson);
        }
        catch (SchemaParseException)
        {
            return null;
        }

        return EvidenceEpaResolver.Declared(schema, dataJson) is int epaId && dbContext.Epas.Any(epa => epa.Id == epaId)
            ? epaId
            : null;
    }
}
