using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Persistence;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.Epas;

/// <summary>
/// A scale's save that reads the unique name index's refusal back (T394 fix pass, code N-7), as a College's and a
/// speciality's do (<c>CollegeHolders.SaveAsync</c>): the create and the update refuse a taken name before they stage
/// anything, and a second save that took the name in between is refused under Name in the same words, never as the page's
/// fixed "could not be saved".
/// </summary>
/// <remarks>
/// Only when another scale holds the name now: the levels' unique index (scale, order) is the table's other one, and its
/// refusal, like any other, is rethrown as it is. The database's refusal stays underneath, so the audit pipeline discards
/// the refused save (T201). Read with no tracking, so the read sees the store, not this context's staging.
/// </remarks>
internal static class EntrustmentScaleNameRace
{
    /// <summary>The unique index on a scale's name, by EF Core's default name (<c>EntrustmentScaleConfiguration</c>).</summary>
    private const string NameIndex = "IX_EntrustmentScales_Name";

    public static async Task SaveAsync(
        IApplicationDbContext dbContext,
        int? exceptId,
        string name,
        string field,
        CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (PostgresErrors.IsUniqueViolation(exception))
        {
            // The refusal names its index (T397): a reorder saves twice in one transaction, which the refusal aborts, so
            // no read can follow it there. Only a refusal that names no index is read back.
            switch (PostgresErrors.ConstraintName(exception))
            {
                case NameIndex:
                    throw new FieldRefusalException(field, EntrustmentScaleBindings.NameTaken(name), exception);
                case not null:
                    throw;
            }

            var held = await dbContext.Set<EntrustmentScale>()
                .AsNoTracking()
                .AnyAsync(scale => (exceptId == null || scale.Id != exceptId) && scale.Name == name, cancellationToken);
            if (held)
            {
                throw new FieldRefusalException(field, EntrustmentScaleBindings.NameTaken(name), exception);
            }

            throw;
        }
    }
}
