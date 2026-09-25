using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.Epas.Commands.DeleteEntrustmentScale;

public sealed class DeleteEntrustmentScaleCommandHandler : IRequestHandler<DeleteEntrustmentScaleCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteEntrustmentScaleCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DeleteEntrustmentScaleCommand request, CancellationToken cancellationToken)
    {
        if (!request.Principal.IsAdministrator())
        {
            throw new UnauthorizedAccessException("Only global administrators may delete entrustment scales.");
        }

        var scale = await _dbContext.Set<EntrustmentScale>()
            .Include(entity => entity.Levels)
            .SingleOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException(NotFound(request.Id));

        if (await RefusalAsync(scale, cancellationToken) is { } refusal)
        {
            throw new InvalidOperationException(refusal);
        }

        foreach (var level in scale.Levels.ToList())
        {
            _dbContext.Set<EntrustmentLevel>().Remove(level);
        }

        _dbContext.Set<EntrustmentScale>().Remove(scale);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            // T254. The checks above are reads, and nothing holds what they read until the save: a sub-speciality's
            // default, a curriculum item's pin, a progress row or a decision can take the scale or one of its levels in
            // between, and the RESTRICT foreign key refuses the removal. So is a removal whose scale or level another
            // delete or save already took away. Before T254 the administrator read that as a raw DbUpdateException.
            // The same checks are asked again on a fresh read, and the first that now applies is the refusal, in the
            // words it would have had before the save. When none does, the refusal is something these checks do not
            // know about, and is left as it is.
            //
            // The database's refusal stays underneath either way, so the audit pipeline discards the refused removal
            // and writes its failure row alone (T201). The read-back runs outside any transaction: this save opened its
            // own, and the refusal rolled it back.
            var raced = await ReadBackAsync(scale, exception, cancellationToken);

            if (raced is null)
            {
                throw;
            }

            throw new InvalidOperationException(raced, exception);
        }
    }

    private static string NotFound(int scaleId) => $"Entrustment scale {scaleId} was not found.";

    /// <summary>
    /// Why the database refused this delete, in the words the checks would have used, read after the refusal; or null
    /// when the refusal is the database's own to report.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A refusal no check explains is a concurrency conflict when another save removed one of the scale's levels after
    /// this command loaded them (<c>UpdateEntrustmentScale</c> removes levels): the level's removal changed nothing, and
    /// EF refuses the whole save. Nothing is wrong with the delete itself, and a second attempt reads the levels afresh,
    /// so the administrator is told that, not EF's row counts (T254 review).
    /// </para>
    /// <para>
    /// Null, too, when the read-back itself fails. The refused removal is still tracked at that point, and an exception
    /// that did not carry the refusal would leave the audit pipeline to write its failure row the ordinary way, which
    /// sends the removal again: were the reference that refused it gone by then, the scale would be deleted under a row
    /// recording that it was not. Rethrowing the refusal keeps it the exception the pipeline sees, and it discards the
    /// removal (T201). What the read-back's own failure said is lost; it is not the reason the delete failed.
    /// </para>
    /// </remarks>
    private async Task<string?> ReadBackAsync(
        EntrustmentScale scale,
        DbUpdateException exception,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!await ScaleExistsAsync(scale.Id, cancellationToken))
            {
                return NotFound(scale.Id);
            }

            if (await RefusalAsync(scale, cancellationToken) is { } refusal)
            {
                return refusal;
            }
        }
        catch (Exception)
        {
            return null;
        }

        return exception is DbUpdateConcurrencyException
            ? "This entrustment scale was changed by another save while it was being deleted, so it has not been " +
              "deleted. Try again."
            : null;
    }

    private Task<bool> ScaleExistsAsync(int scaleId, CancellationToken cancellationToken)
        => _dbContext.Set<EntrustmentScale>().AsNoTracking().AnyAsync(entity => entity.Id == scaleId, cancellationToken);

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
    private async Task<string?> RefusalAsync(EntrustmentScale scale, CancellationToken cancellationToken)
    {
        var scaleId = scale.Id;

        if (await _dbContext.Set<MsfQuestion>().AnyAsync(question => question.ScaleId == scaleId, cancellationToken))
        {
            return "This entrustment scale is referenced by one or more MSF questions and cannot be deleted.";
        }

        // T109 pinned curriculum minima and scored progress rows to scales, both ON DELETE RESTRICT.
        // Without these two checks the delete reaches the database and comes back as a raw
        // DbUpdateException, which is a stack trace where an explanation belongs.
        if (await _dbContext.Set<CurriculumItem>().AnyAsync(item => item.ScaleId == scaleId, cancellationToken))
        {
            return "This entrustment scale is pinned to one or more curriculum items and cannot be deleted.";
        }

        if (await _dbContext.Set<CurriculumItemProgress>()
                .AnyAsync(progress => progress.MinimumLevelScaleId == scaleId, cancellationToken))
        {
            return "Trainee progress has been scored against this entrustment scale and it cannot be deleted.";
        }

        // A sub-speciality's default scale is the third ON DELETE RESTRICT reference (T076), and until T232 the one
        // this handler never asked about: the delete reached the database and came back as that same raw exception.
        if (await EntrustmentScaleReferences.DescribeDefaultOfASubSpecialityAsync(_dbContext, scaleId, cancellationToken)
            is { } subSpecialityDefault)
        {
            return subSpecialityDefault;
        }

        // A published schema that binds this scale, by the id the builder writes, the seed key the seeds write or a
        // name, would lose its ladder for good: the key stops resolving and the cross-scale refusal can never fire for
        // that type again (T109). Until T253 only a binding by name was asked about.
        if (await EntrustmentScaleReferences.DescribeBindingByAPublishedSchemaAsync(_dbContext, scale, cancellationToken)
            is { } binding)
        {
            return binding;
        }

        // Both decision tables hold their level ON DELETE RESTRICT.
        var pendingRefs = await _dbContext.Set<PendingEntrustmentDecision>()
            .AnyAsync(decision => decision.AuthorisedLevel.ScaleId == scaleId, cancellationToken);
        var issuedRefs = await _dbContext.Set<EntrustmentDecision>()
            .AnyAsync(decision => decision.AuthorisedLevel.ScaleId == scaleId, cancellationToken);
        if (pendingRefs || issuedRefs)
        {
            return "Levels of this entrustment scale are referenced by entrustment decisions and cannot be deleted.";
        }

        return null;
    }
}
