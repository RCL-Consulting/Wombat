using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Epas;

namespace Wombat.Infrastructure.Activities;

/// <summary>
/// The one implementation of "which EPA is this activity evidence for". (T137)
/// </summary>
/// <remarks>
/// <para>
/// The sibling of <see cref="ObservationDateResolver" />, used by <see cref="ActivityService" /> wherever that one is.
/// It reads the field the PINNED schema's <c>evidence_epa_field</c> names, not the live schema's, for the reason T109
/// and T119 recorded: a binding must not drift under an activity that is already in flight.
/// </para>
/// <para>
/// The value is parsed by <see cref="CreditTargetResolver.TryGetInt32" />, the credit engine's own reader, so a value
/// credit resolves (<c>5</c> or <c>"5"</c>) is a value this resolves, and one credit cannot read is one this cannot
/// either. With <c>EvidenceEpa.EnsureCreditAgrees</c> holding the two properties to one field, the EPA stamped here
/// is the EPA credit matched.
/// </para>
/// <para>
/// Unlike the date, there is no fallback: an activity whose schema declares no pointer, whose field is empty, or whose
/// value is not an integer is about no EPA, and says so with null. An integer naming no EPA is null too, so the column
/// never carries an id a join cannot resolve. That check is a read, which is why the stamp is asynchronous and why
/// every caller awaits it before its request's first mutation (the audit trap).
/// </para>
/// </remarks>
internal static class EvidenceEpaResolver
{
    /// <summary>
    /// The EPA id the data states under the pinned schema's pointer, or null. Pure; does not check the id exists.
    /// </summary>
    public static int? Declared(FormSchema schema, string dataJson)
    {
        ArgumentNullException.ThrowIfNull(schema);

        if (string.IsNullOrWhiteSpace(schema.EvidenceEpaField) || string.IsNullOrWhiteSpace(dataJson))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(dataJson);
            return document.RootElement.ValueKind == JsonValueKind.Object &&
                   CreditTargetResolver.TryGetInt32(document.RootElement, schema.EvidenceEpaField, out var epaId)
                ? epaId
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// <see cref="Declared" />, kept only when an EPA with that id exists.
    /// </summary>
    public static async Task<int?> ResolveAsync(
        IApplicationDbContext dbContext,
        FormSchema schema,
        string dataJson,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        var declared = Declared(schema, dataJson);
        if (declared is not int epaId)
        {
            return null;
        }

        return await dbContext.Set<Epa>().AsNoTracking().AnyAsync(epa => epa.Id == epaId, cancellationToken)
            ? epaId
            : null;
    }
}
