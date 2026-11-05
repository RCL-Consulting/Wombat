using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Persistence;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Institutions;

/// <summary>
/// A speciality's create and update say a name is taken only when it is (T394, flow 16, N1): the commands check the
/// College's other specialities before they stage anything, and a save the unique index on <c>(CollegeId, Name)</c> still
/// refuses (another save took the name in between) is the same refusal. Until T394 every refused save, a foreign key
/// included, was reported as a duplicate name.
/// </summary>
/// <remarks>
/// As <see cref="SubSpecialitySaveRefusal" /> does, which index refused is read back rather than parsed out of a provider
/// exception this layer cannot see, and a read-back that fails answers false, so the caller rethrows the database's own
/// refusal and the audit pipeline discards the refused save (T201, T254).
/// </remarks>
internal static class SpecialitySaveRefusal
{
    /// <summary>The refusal of a name another speciality of the same College already holds (the built words, N1).</summary>
    internal const string DuplicateName = "A speciality with the same name already exists for this college.";

    /// <summary>
    /// Whether a speciality of <paramref name="collegeId" /> other than <paramref name="specialityId" /> holds
    /// <paramref name="name" />, compared as the database compares, as the unique index does (N7).
    /// </summary>
    internal static Task<bool> IsTakenAsync(
        IApplicationDbContext dbContext, int? specialityId, int collegeId, string name, CancellationToken cancellationToken)
        => dbContext.Set<Speciality>()
            .AsNoTracking()
            .AnyAsync(other => other.Id != specialityId && other.CollegeId == collegeId && other.Name == name, cancellationToken);

    /// <summary>
    /// Whether the database refused the save because another speciality took the name after the check: a unique violation,
    /// and the name held now. False when anything else refused it, or when the read-back itself fails.
    /// </summary>
    internal static async Task<bool> WasTakenAsync(
        IApplicationDbContext dbContext,
        DbUpdateException exception,
        int? specialityId,
        int collegeId,
        string name,
        CancellationToken cancellationToken)
    {
        if (!PostgresErrors.IsUniqueViolation(exception))
        {
            return false;
        }

        try
        {
            return await IsTakenAsync(dbContext, specialityId, collegeId, name, cancellationToken);
        }
        catch (Exception)
        {
            return false;
        }
    }
}
