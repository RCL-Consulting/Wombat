using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Epas.Commands.DeleteEntrustmentScale;
using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.Epas;

/// <summary>
/// What uses an entrustment scale (T394, flow 16, Q16, T395; build-contracts § 0), one read for the three that ask: the
/// delete, which refuses a scale in use with the first use's sentence (moved here unchanged from
/// <c>DeleteEntrustmentScaleCommandHandler</c>); the update, which refuses a reorder or a removal on a scale in use (Q16,
/// after Q18's per-item checks); and the scale's page, which offers Up, Down and Remove only on a scale nothing uses
/// (<see cref="EntrustmentScaleDto.InUse" />). The page and the handlers cannot disagree about "in use".
/// </summary>
/// <remarks>
/// In use is any of the delete's refusals, in its order: an MSF question, a pinned curriculum item, an item pinned on an
/// ended programme's record (T312), scored progress, a sub-speciality's default, a published form's binding, an entrustment
/// decision on one of its levels. Each is a reason a rung's rank means something to a stored row, so a reorder would change
/// what that row means (T395: nine items' minima on the CPSA ladder).
/// </remarks>
internal static class EntrustmentScaleUses
{
    /// <summary>
    /// The first use's refusal as the delete throws it: its sentence, and when the use is a sub-speciality's default, each
    /// such sub-speciality for the list's links (T394, flow 16, S6). Null when nothing uses the scale.
    /// </summary>
    public static async Task<EntrustmentScaleDeleteRefusedException?> DeleteRefusalAsync(
        IApplicationDbContext dbContext, EntrustmentScale scale, Exception? inner, CancellationToken cancellationToken)
    {
        if (await FirstUseAsync(dbContext, scale, cancellationToken) is not { } use)
        {
            return null;
        }

        var holders = use.IsSubSpecialityDefault
            ? await EntrustmentScaleReferences.DefaultHoldersAsync(dbContext, scale.Id, cancellationToken)
            : [];
        return new EntrustmentScaleDeleteRefusedException(use.Sentence, inner) { Holders = holders };
    }

    /// <summary>Whether anything uses the scale (<see cref="FirstUseAsync" /> found a use).</summary>
    public static async Task<bool> IsInUseAsync(IApplicationDbContext dbContext, EntrustmentScale scale, CancellationToken cancellationToken)
        => await FirstUseAsync(dbContext, scale, cancellationToken) is not null;

    /// <summary>
    /// Why this scale may not be deleted, read from the database now; or null when nothing refers to it. The first
    /// reference found is the one named.
    /// </summary>
    /// <remarks>
    /// Asked before the removal and again when the database refuses it (T254), so both give the same words. Every
    /// question is a query, never the change tracker: after a refused save the scale and its levels are still tracked as
    /// deleted, and the levels asked about are the ones stored now, not the ones this command loaded, so a level another
    /// save has added since is asked about too.
    /// </remarks>
    public static async Task<EntrustmentScaleUse?> FirstUseAsync(
        IApplicationDbContext dbContext, EntrustmentScale scale, CancellationToken cancellationToken)
    {
        var scaleId = scale.Id;

        if (await dbContext.Set<MsfQuestion>().AnyAsync(question => question.ScaleId == scaleId, cancellationToken))
        {
            return new EntrustmentScaleUse("This entrustment scale is referenced by one or more MSF questions and cannot be deleted.");
        }

        // T109 pinned curriculum minima and scored progress rows to scales, both ON DELETE RESTRICT.
        // Without these two checks the delete reaches the database and comes back as a raw
        // DbUpdateException, which is a stack trace where an explanation belongs.
        if (await dbContext.Set<CurriculumItem>().AnyAsync(item => item.ScaleId == scaleId, cancellationToken))
        {
            return new EntrustmentScaleUse("This entrustment scale is pinned to one or more curriculum items and cannot be deleted.");
        }

        // T312 (review B2): the list an ended programme ended with keeps each item's pin as it was, so the scale an item
        // was pinned to then is the closed record's, whatever the item is pinned to now. No foreign key knows of it.
        if (await EntrustmentScaleReferences.IsPinnedOnAnEndedRecordAsync(dbContext, scaleId, cancellationToken))
        {
            return new EntrustmentScaleUse(
                "This entrustment scale is pinned to a curriculum item on the record of a programme that has ended, " +
                "which keeps the targets it ended with, so it cannot be deleted.");
        }

        if (await dbContext.Set<CurriculumItemProgress>()
                .AnyAsync(progress => progress.MinimumLevelScaleId == scaleId, cancellationToken))
        {
            return new EntrustmentScaleUse("Trainee progress has been scored against this entrustment scale and it cannot be deleted.");
        }

        // A sub-speciality's default scale is the third ON DELETE RESTRICT reference (T076), and until T232 the one
        // this handler never asked about: the delete reached the database and came back as that same raw exception.
        if (await EntrustmentScaleReferences.DescribeDefaultOfASubSpecialityAsync(dbContext, scaleId, cancellationToken)
            is { } subSpecialityDefault)
        {
            return new EntrustmentScaleUse(subSpecialityDefault, IsSubSpecialityDefault: true);
        }

        // A published schema that binds this scale, by the id the builder writes, the seed key the seeds write or a
        // name, would lose its ladder for good: the key stops resolving and the cross-scale refusal can never fire for
        // that type again (T109). Until T253 only a binding by name was asked about.
        if (await EntrustmentScaleReferences.DescribeBindingByAPublishedSchemaAsync(dbContext, scale, cancellationToken)
            is { } binding)
        {
            return new EntrustmentScaleUse(binding);
        }

        // Both decision tables hold their level ON DELETE RESTRICT.
        var pendingRefs = await dbContext.Set<PendingEntrustmentDecision>()
            .AnyAsync(decision => decision.AuthorisedLevel.ScaleId == scaleId, cancellationToken);
        var issuedRefs = await dbContext.Set<EntrustmentDecision>()
            .AnyAsync(decision => decision.AuthorisedLevel.ScaleId == scaleId, cancellationToken);
        if (pendingRefs || issuedRefs)
        {
            return new EntrustmentScaleUse("Levels of this entrustment scale are referenced by entrustment decisions and cannot be deleted.");
        }

        return null;
    }
}

/// <summary>
/// The first use <see cref="EntrustmentScaleUses.FirstUseAsync" /> found: the delete's sentence for it, and whether it is a
/// sub-speciality's default, whose refusal links each such sub-speciality (T394 fix pass, code N-4: the kind is carried,
/// not found again by comparing sentences).
/// </summary>
internal sealed record EntrustmentScaleUse(string Sentence, bool IsSubSpecialityDefault = false);
