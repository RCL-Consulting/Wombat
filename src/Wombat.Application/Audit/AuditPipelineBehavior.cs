using MediatR;
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
        try
        {
            var response = await next();

            await _auditWriter.WriteAsync(AuditEntry.Create(
                occurredAt: occurredAt,
                category: AuditCategory.Command,
                action: action,
                success: true,
                actorUserId: _contextProvider.UserId,
                actorDisplay: _contextProvider.UserDisplay,
                actorIpAddress: _contextProvider.IpAddress,
                actorUserAgent: _contextProvider.UserAgent,
                institutionId: _contextProvider.InstitutionId,
                summaryJson: AuditPayloadSerializer.Serialize(request)),
                cancellationToken);

            return response;
        }
        catch (Exception ex)
        {
            await _auditWriter.WriteAsync(AuditEntry.Create(
                occurredAt: occurredAt,
                category: AuditCategory.Command,
                action: action,
                success: false,
                actorUserId: _contextProvider.UserId,
                actorDisplay: _contextProvider.UserDisplay,
                actorIpAddress: _contextProvider.IpAddress,
                actorUserAgent: _contextProvider.UserAgent,
                institutionId: _contextProvider.InstitutionId,
                summaryJson: AuditPayloadSerializer.Serialize(request),
                errorMessage: ex.Message),
                cancellationToken);

            throw;
        }
    }

    private static bool IsCommand(TRequest request)
        => typeof(TRequest).Name.EndsWith("Command", StringComparison.Ordinal)
        || request is IAuditedCommand;
}
