using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Domain.Audit;

namespace Wombat.Application.Audit;

/// <summary>
/// Outermost MediatR pipeline behaviour. Writes an AuditEntry for every command
/// (requests whose type name ends with "Command" or that implement IAuditedCommand).
/// Queries are passed through untouched.
///
/// Registration order matters: this behaviour must be registered FIRST so it wraps
/// all inner behaviours and sees the final outcome (including validation failures).
/// </summary>
public sealed class AuditPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IAuditWriter _auditWriter;
    private readonly IAuditContextProvider _contextProvider;

    public AuditPipelineBehavior(IAuditWriter auditWriter, IAuditContextProvider contextProvider)
    {
        _auditWriter = auditWriter;
        _contextProvider = contextProvider;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!IsCommand(request))
        {
            return await next();
        }

        var action = typeof(TRequest).Name;
        var occurredAt = DateTime.UtcNow;

        // Stamp the actor's institution on every command row. Command rows used to be written with
        // InstitutionId == null, and the audit queries treated null as "a global event anyone may
        // read" — so an InstitutionalAdmin in institution A could open institution B's command rows,
        // SummaryJson and all, at /admin/audit/{id}. The queries now fail closed on null, which means
        // a row without this stamp is Administrator-only and its institution's admins never see it. (T101)
        //
        // Resolved AFTER the handler runs, both here and in the catch: a handler whose caller is
        // anonymous declares the scope itself (IAuditContextProvider.DeclareInstitution), and a
        // handler that throws — a revoked invitation token being retried, say — has usually declared
        // before it threw, which is the row the issuing admin most needs to see.
        TResponse response;
        try
        {
            response = await next();
        }
        catch (Exception ex)
        {
            await WriteFailureAsync(request, action, occurredAt, ex, cancellationToken);
            throw;
        }

        // Outside the try: only the handler's own exception makes a failure row. Were this write refused, the
        // command's work has committed, and a Success=false row under its name would record a lock, a grant or an
        // admission that did happen as one that did not. Its exception reaches the caller untouched. (T201)
        var actor = ActorOf(request);
        await _auditWriter.WriteAsync(AuditEntry.Create(
            occurredAt: occurredAt,
            category: AuditCategory.Command,
            action: action,
            success: true,
            actorUserId: actor.UserId,
            actorDisplay: actor.Display,
            actorIpAddress: actor.IpAddress,
            actorUserAgent: actor.UserAgent,
            institutionId: actor.InstitutionId,
            summaryJson: AuditPayloadSerializer.Serialize(request)),
            cancellationToken);

        return response;
    }

    /// <summary>
    /// Writes the failed command's row without re-sending anything the database has refused, so the row is stored and
    /// the handler's exception, which the caller rethrows afterwards, is not replaced by one from this write.
    /// </summary>
    private async Task WriteFailureAsync(
        TRequest request,
        string action,
        DateTime occurredAt,
        Exception ex,
        CancellationToken cancellationToken)
    {
        var actor = ActorOf(request);
        var entry = AuditEntry.Create(
            occurredAt: occurredAt,
            category: AuditCategory.Command,
            action: action,
            success: false,
            actorUserId: actor.UserId,
            actorDisplay: actor.Display,
            actorIpAddress: actor.IpAddress,
            actorUserAgent: actor.UserAgent,
            institutionId: actor.InstitutionId,
            summaryJson: AuditPayloadSerializer.Serialize(request),
            errorMessage: ex.Message);

        // The writer saves through the handler's own DbContext, and that save flushes everything still tracked.
        //
        // When the handler's own save was refused (T201), the refused changes are still tracked: EF keeps them so a
        // caller could retry. Written the ordinary way, this row would send them again, be refused again, and be
        // lost, and EF's second exception would replace the handler's error (a translated "already exists" as much
        // as a concurrency conflict). So they are discarded and the row is written alone.
        //
        // Discarded here, not left to the fallback below: a refusal need not repeat. An insert whose clash has gone by
        // the time of this write would be accepted when re-sent, and committed under the row recording its failure.
        if (IsRefusedSave(ex))
        {
            await _auditWriter.WriteDiscardingPendingChangesAsync(entry, cancellationToken);
            return;
        }

        // Any other exception keeps the ordinary write, and with it the known trap: a handler that mutated a tracked
        // entity and then threw has that mutation committed here. Handlers are written against it (every check
        // before the first mutation; RebuildCurriculumProgress restores its context before rethrowing). Discarding
        // on every failure would retire the trap, but it changes what every handler's failure commits at once, and
        // that wants its own sweep of the handlers rather than riding on this fix.
        //
        // Unless the database refuses what the ordinary write carries. A refused save can arrive without a
        // DbUpdateException to show for it: ASP.NET Identity's user store catches a concurrency conflict itself and
        // returns a failed result, which UserAdministrationService throws as a plain InvalidOperationException. The
        // refused user update is still tracked, so the write sends it again. A refused write commits nothing, so
        // discarding what it carried and writing the row alone loses nothing the trap would have committed: only the
        // row is stored, and the handler's exception stays the one the caller sees.
        try
        {
            await _auditWriter.WriteAsync(entry, cancellationToken);
        }
        catch (DbUpdateException)
        {
            await _auditWriter.WriteDiscardingPendingChangesAsync(entry, cancellationToken);
        }
    }

    /// <summary>
    /// Who the row names as having sent the command. Nobody, for a command whose sender is promised anonymity
    /// (<see cref="IAnonymousAuditedCommand" />): only the truncated address is kept, whoever is signed in on the request
    /// and whatever a handler declared. (T205)
    /// </summary>
    private Actor ActorOf(TRequest request)
        => request is IAnonymousAuditedCommand
            ? new Actor(null, null, _contextProvider.IpAddress, null, null)
            : new Actor(
                _contextProvider.UserId,
                _contextProvider.UserDisplay,
                _contextProvider.IpAddress,
                _contextProvider.UserAgent,
                _contextProvider.InstitutionId);

    private readonly record struct Actor(
        string? UserId,
        string? Display,
        string? IpAddress,
        string? UserAgent,
        int? InstitutionId);

    private static bool IsCommand(TRequest request)
        => typeof(TRequest).Name.EndsWith("Command", StringComparison.Ordinal)
        || request is IAuditedCommand;

    /// <summary>
    /// A save the database refused: <see cref="DbUpdateException" /> (a concurrency conflict is one), thrown as is or
    /// inside the exception a handler translated it to. A dozen handlers turn a unique-index violation into an
    /// <see cref="InvalidOperationException" /> that carries it.
    /// </summary>
    private static bool IsRefusedSave(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is DbUpdateException)
            {
                return true;
            }
        }

        return false;
    }
}
