using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Epas;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Institutions;

/// <summary>
/// What a sub-speciality's create or update tells the administrator when the database refuses its save (T254).
/// </summary>
/// <remarks>
/// <para>
/// Both commands used to report every refused save as a duplicate name. Only one refusal means that: the unique index on
/// <c>(SpecialityId, Name)</c>. The others are a foreign key whose row went away after the command checked it, the
/// speciality or the default entrustment scale, and, for an update, the sub-speciality itself being deleted meanwhile.
/// Each gets the words the command's own check would have used had it run after the other save.
/// </para>
/// <para>
/// Which one it was is read back, as <c>UpdateEpa</c> and <c>ScheduleCommitteeReview</c> read back which index refused
/// them, rather than parsed out of a provider exception this layer cannot see. The SQLSTATE is read through
/// <see cref="DbException.SqlState" />, as those handlers do: Application does not reference Npgsql.
/// </para>
/// </remarks>
internal static class SubSpecialitySaveRefusal
{
    /// <summary>The refusal of a name another sub-speciality of the same speciality already holds.</summary>
    internal const string DuplicateName = "A sub-speciality with the same name already exists for this speciality.";

    /// <summary>PostgreSQL's unique_violation.</summary>
    private const string UniqueViolation = "23505";

    /// <summary>
    /// Why the database refused the save, in the command's own words, read now; or null when none of the refusals this
    /// command knows about applies, or the read-back itself fails, and the caller rethrows what the database said.
    /// </summary>
    /// <remarks>
    /// A failed read-back is null, not its own exception, because the refused save is still tracked when it runs. An
    /// exception that did not carry the refusal would leave the audit pipeline to write its failure row the ordinary way,
    /// which sends the save again: were the clash gone by then (the sibling renamed back, say), the save would commit
    /// under a row recording that it failed. Rethrowing the refusal keeps it the exception the pipeline sees, and it
    /// discards the save (T201). What the read-back's own failure said is lost; it is not the reason the save failed.
    /// </remarks>
    /// <param name="subSpecialityId">The sub-speciality being updated; null for a create.</param>
    /// <param name="specialityId">The speciality the save puts it in.</param>
    /// <param name="name">The name the save gives it, trimmed as saved.</param>
    /// <param name="defaultEntrustmentScaleId">The default scale the save gives it; null for none, and for a create.</param>
    internal static async Task<string?> ReadBackAsync(
        IApplicationDbContext dbContext,
        DbUpdateException exception,
        int? subSpecialityId,
        int specialityId,
        string name,
        int? defaultEntrustmentScaleId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await ReadAsync(
                dbContext, exception, subSpecialityId, specialityId, name, defaultEntrustmentScaleId, cancellationToken);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static async Task<string?> ReadAsync(
        IApplicationDbContext dbContext,
        DbUpdateException exception,
        int? subSpecialityId,
        int specialityId,
        string name,
        int? defaultEntrustmentScaleId,
        CancellationToken cancellationToken)
    {
        // Only the unique index names a duplicate, and only when another sub-speciality of that speciality holds the
        // name now. The table's only other unique index is on the seed key, which neither command writes.
        if (exception.InnerException is DbException { SqlState: UniqueViolation }
            && await dbContext.Set<SubSpeciality>()
                .AsNoTracking()
                .AnyAsync(other => other.Id != subSpecialityId
                    && other.SpecialityId == specialityId
                    && other.Name == name, cancellationToken))
        {
            return DuplicateName;
        }

        // An update of a row deleted since it was read changes nothing, which EF reports as a concurrency conflict.
        if (subSpecialityId is int id
            && !await dbContext.Set<SubSpeciality>().AsNoTracking().AnyAsync(entity => entity.Id == id, cancellationToken))
        {
            return $"Sub-speciality {id} was not found.";
        }

        if (!await dbContext.Set<Speciality>().AsNoTracking().AnyAsync(entity => entity.Id == specialityId, cancellationToken))
        {
            return $"Speciality {specialityId} was not found.";
        }

        if (defaultEntrustmentScaleId is int scaleId
            && !await dbContext.Set<EntrustmentScale>().AsNoTracking().AnyAsync(entity => entity.Id == scaleId, cancellationToken))
        {
            return $"Entrustment scale {scaleId} was not found.";
        }

        return null;
    }
}
