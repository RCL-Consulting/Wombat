using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Curricula;
using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.EntrustmentDecisions;

/// <summary>
/// One EPA a STAR may be granted on for a trainee, and the ladder its level must be a rung of. (T167)
/// </summary>
/// <param name="EpaId">The EPA.</param>
/// <param name="Code">Its code, "PAED-001".</param>
/// <param name="Title">Its title.</param>
/// <param name="ScaleId">
/// The entrustment scale the level must belong to: the curriculum item's pinned scale (T109), else the programme's
/// default scale (T076), else null, which accepts a level on any scale.
/// </param>
/// <param name="ScaleName">That scale's name, or null when <paramref name="ScaleId" /> is null.</param>
public sealed record StarEpaOptionDto(int EpaId, string Code, string Title, int? ScaleId, string? ScaleName);

/// <summary>
/// Which EPAs a Statement of Awarded Responsibility may be granted on for a trainee, and on which ladder: the one
/// answer the committee's STAR picker lists and the staging and ratifying handlers enforce. (T167)
/// </summary>
/// <remarks>
/// <para>
/// <b>The EPAs</b> are the items of the curriculum on the trainee's preferred profile
/// (<see cref="TraineeScopeResolver.PreferredProfiles" />: the active one, else the most recent). The national core and
/// the trainee's OWN institution's local extras, never another institution's: a curriculum row is shared by every
/// institution that adopted it, and <c>CurriculumItems</c> is unique on (curriculum, EPA), so another institution's
/// local item is the only row for its EPA and would otherwise be offered here as if it were this trainee's. A trainee
/// with no profile has no curriculum and no EPA a STAR may be granted on. Only items in force count
/// (<see cref="CurriculumItemsInForce" />, T158): a deactivated EPA is no longer a target, so no new STAR is granted on
/// it, as no new credit lands on it.
/// </para>
/// <para>
/// Before T167 the picker listed the EPAs in the CHAIR's scope (<c>ListEpasForSubSpecialityQuery</c>) and staging
/// checked only that the EPA existed, so a STAR could be granted on an EPA the trainee's programme does not contain.
/// An <c>Activity.EpaId</c> stamp (T137) is no substitute: it says what an activity was evidence for, not that the EPA
/// is on the trainee's curriculum.
/// </para>
/// <para>
/// <b>The ladder</b> is the item's pinned scale where it has one, which is the scale its minima are written on and its
/// evidence rated on. An unpinned item (a permanent, meaningful state, T109) falls back to the programme's default
/// scale, which is the T076 rule this replaces for every item; with neither, any scale is accepted, as before T076.
/// </para>
/// <para>
/// The picker and the gate call the same <see cref="ListAsync" />, so the picker cannot offer an EPA or a level the
/// gate refuses, nor hide one it accepts. The gate runs where a STAR is staged, and again at ratification (<see cref="DemandStagedAsync" />), which is when a staged decision becomes a STAR: between staging
/// and ratifying, the item can be removed, its ladder re-pinned, its EPA deactivated, or the trainee moved to another
/// curriculum.
/// </para>
/// </remarks>
public static class StarCurriculum
{
    /// <summary>
    /// The EPAs a STAR may be granted on for this trainee, ordered by code. Empty when the trainee holds no profile.
    /// </summary>
    public static async Task<IReadOnlyList<StarEpaOptionDto>> ListAsync(
        IApplicationDbContext dbContext,
        string traineeUserId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        if (string.IsNullOrWhiteSpace(traineeUserId))
        {
            return [];
        }

        var profile = await TraineeScopeResolver.PreferredProfiles(dbContext)
            .Where(entity => entity.UserId == traineeUserId)
            .Select(entity => new { entity.CurriculumId, entity.InstitutionId })
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
        {
            return [];
        }

        // One level at a time, as TraineeScopeResolver does: a required navigation in one projection is an INNER join.
        var programmeScaleId = await dbContext.Set<Curriculum>()
            .Where(entity => entity.Id == profile.CurriculumId)
            .Select(entity => entity.SubSpeciality.DefaultEntrustmentScaleId)
            .FirstOrDefaultAsync(cancellationToken);

        var items = await dbContext.Set<CurriculumItem>()
            .AsNoTracking()
            .InForce()
            .Where(item => item.CurriculumId == profile.CurriculumId &&
                           (item.OwningInstitutionId == null || item.OwningInstitutionId == profile.InstitutionId))
            .Select(item => new { item.EpaId, item.Epa.Code, item.Epa.Title, item.ScaleId })
            .ToListAsync(cancellationToken);

        var scaleIds = items
            .Select(item => item.ScaleId ?? programmeScaleId)
            .OfType<int>()
            .Distinct()
            .ToArray();

        var scaleNames = scaleIds.Length == 0
            ? new Dictionary<int, string>()
            : await dbContext.Set<EntrustmentScale>()
                .AsNoTracking()
                .Where(scale => scaleIds.Contains(scale.Id))
                .ToDictionaryAsync(scale => scale.Id, scale => scale.Name, cancellationToken);

        return items
            .Select(item =>
            {
                var scaleId = item.ScaleId ?? programmeScaleId;
                var scaleName = scaleId is int id && scaleNames.TryGetValue(id, out var name) ? name : null;
                return new StarEpaOptionDto(item.EpaId, item.Code, item.Title, scaleId, scaleName);
            })
            .OrderBy(option => option.Code, StringComparer.Ordinal)
            .ThenBy(option => option.EpaId)
            .ToArray();
    }

    /// <summary>
    /// Refuses a STAR on an EPA that is not on the trainee's curriculum, or at a level that is not a rung of that EPA's
    /// ladder. Reads only: a caller runs it before its first mutation, so a refusal leaves nothing for the audit
    /// pipeline's save to commit.
    /// </summary>
    /// <exception cref="InvalidOperationException">The EPA or the level is refused.</exception>
    public static async Task<StarEpaOptionDto> DemandAsync(
        IApplicationDbContext dbContext,
        string traineeUserId,
        Epa epa,
        EntrustmentLevel level,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(epa);
        ArgumentNullException.ThrowIfNull(level);

        var options = await ListAsync(dbContext, traineeUserId, cancellationToken);
        var refusal = RefusalFor(options, epa.Id, epa.Code, level.ScaleId);
        if (refusal is not null)
        {
            throw new InvalidOperationException(refusal);
        }

        return options.First(candidate => candidate.EpaId == epa.Id);
    }

    /// <summary>
    /// Refuses to ratify while any decision staged at the review would no longer be admitted: its EPA has left the
    /// trainee's curriculum or been deactivated, or its level is no longer a rung of the item's ladder. Ratifying issues
    /// every staged decision as a STAR and supersedes the trainee's active one on the same EPA, so it is held to the rule
    /// staging was. Every stale decision is named, so the chair can remove them and stage again. Reads only: the ratify
    /// handler runs it before its first mutation.
    /// </summary>
    /// <exception cref="InvalidOperationException">At least one staged decision is refused.</exception>
    public static async Task DemandStagedAsync(
        IApplicationDbContext dbContext,
        string traineeUserId,
        IReadOnlyCollection<PendingEntrustmentDecision> staged,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(staged);

        if (staged.Count == 0)
        {
            return;
        }

        var options = await ListAsync(dbContext, traineeUserId, cancellationToken);
        var epaIds = staged.Select(decision => decision.EpaId).Distinct().ToArray();
        var levelIds = staged.Select(decision => decision.AuthorisedLevelId).Distinct().ToArray();
        var codes = await dbContext.Set<Epa>()
            .AsNoTracking()
            .Where(epa => epaIds.Contains(epa.Id))
            .ToDictionaryAsync(epa => epa.Id, epa => epa.Code, cancellationToken);
        var ladders = await dbContext.Set<EntrustmentLevel>()
            .AsNoTracking()
            .Where(level => levelIds.Contains(level.Id))
            .ToDictionaryAsync(level => level.Id, level => level.ScaleId, cancellationToken);

        var refusals = staged
            .Select(decision =>
            {
                var code = codes.TryGetValue(decision.EpaId, out var found) ? found : $"EPA {decision.EpaId}";
                int? ladder = ladders.TryGetValue(decision.AuthorisedLevelId, out var scaleId) ? scaleId : null;
                return (Code: code, Refusal: RefusalFor(options, decision.EpaId, code, ladder));
            })
            .Where(judged => judged.Refusal is not null)
            .OrderBy(judged => judged.Code, StringComparer.Ordinal)
            .Select(judged => judged.Refusal!)
            .ToArray();

        if (refusals.Length > 0)
        {
            throw new InvalidOperationException(
                $"This review cannot be ratified: {refusals.Length} staged entrustment " +
                $"{(refusals.Length == 1 ? "decision no longer fits" : "decisions no longer fit")} the trainee's curriculum. " +
                string.Join(" ", refusals) +
                $" Remove {(refusals.Length == 1 ? "it" : "them")} and stage again, then ratify.");
        }
    }

    /// <summary>
    /// Why a STAR on this EPA, at a level on this ladder, is refused for a trainee whose admitted EPAs are
    /// <paramref name="options" />; null when it is admitted. The one wording staging and ratifying share.
    /// </summary>
    /// <param name="options">What <see cref="ListAsync" /> admits for the trainee.</param>
    /// <param name="epaId">The EPA.</param>
    /// <param name="epaCode">Its code, for the refusal.</param>
    /// <param name="levelScaleId">The scale the level is a rung of; null when the level no longer exists.</param>
    private static string? RefusalFor(
        IReadOnlyList<StarEpaOptionDto> options,
        int epaId,
        string epaCode,
        int? levelScaleId)
    {
        var option = options.FirstOrDefault(candidate => candidate.EpaId == epaId);
        if (option is null)
        {
            return $"{epaCode} is not on this trainee's curriculum, or its EPA is deactivated, so a STAR cannot be " +
                   "granted on it.";
        }

        if (option.ScaleId is int scaleId && levelScaleId != scaleId)
        {
            return $"The authorised level must be a rung of {option.ScaleName ?? "the entrustment scale"}, the ladder " +
                   $"{option.Code} is assessed on for this trainee (the curriculum item's pinned scale, else the " +
                   "programme's entrustment scale).";
        }

        return null;
    }
}
