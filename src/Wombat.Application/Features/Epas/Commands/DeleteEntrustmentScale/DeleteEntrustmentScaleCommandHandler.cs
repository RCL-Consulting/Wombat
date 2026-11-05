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
            ?? throw new EntrustmentScaleDeleteRefusedException(NotFound(request.Id));

        if (await EntrustmentScaleUses.DeleteRefusalAsync(_dbContext, scale, null, cancellationToken) is { } refusal)
        {
            throw refusal;
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

            // S6 (T394): a sub-speciality that took the scale as its default meanwhile is linked, as before the save. Asked
            // as the read-back is, so a failure of its own leaves the refusal as it is (no links), never another exception.
            var holders = await HoldersAfterTheRefusalAsync(scale.Id, raced, cancellationToken);
            throw new EntrustmentScaleDeleteRefusedException(raced, exception) { Holders = holders };
        }
    }

    private async Task<IReadOnlyList<EntrustmentScaleHolder>> HoldersAfterTheRefusalAsync(
        int scaleId, string refusal, CancellationToken cancellationToken)
    {
        try
        {
            return string.Equals(
                refusal,
                await EntrustmentScaleReferences.DescribeDefaultOfASubSpecialityAsync(_dbContext, scaleId, cancellationToken),
                StringComparison.Ordinal)
                ? await EntrustmentScaleReferences.DefaultHoldersAsync(_dbContext, scaleId, cancellationToken)
                : [];
        }
        catch (Exception)
        {
            return [];
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

    /// <summary>Why this scale may not be deleted now (<see cref="EntrustmentScaleUses.FirstUseAsync" />, T394 § 0).</summary>
    private async Task<string?> RefusalAsync(EntrustmentScale scale, CancellationToken cancellationToken)
        => (await EntrustmentScaleUses.FirstUseAsync(_dbContext, scale, cancellationToken))?.Sentence;
}
