using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wombat.Application.Audit;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.Activities.Commands.RebuildCurriculumProgress;
using Wombat.Domain.Activities;
using Wombat.Domain.Identity;

namespace Wombat.Infrastructure.Persistence;

/// <summary>
/// Rebuilds curriculum progress once at startup when the progress table is empty but completions have
/// credited (T130).
/// </summary>
/// <remarks>
/// <para>
/// <b>Why it exists.</b> The T130 migration empties <c>CurriculumItemProgresses</c> on every existing
/// database. A lifetime row cannot be split into semester buckets: <c>MinimumLevelReachedCount</c> does not
/// decompose. Without this, every trainee's progress page would read zero, and every committee dashboard
/// nought per cent, from the deploy until somebody found the rebuild button. Meanwhile every
/// <c>ActivityTransition.CreditedItemCount</c> would still say those completions credited, so the two
/// surfaces would contradict each other.
/// </para>
/// <para>
/// T219's migration empties the table again, for the same reason: its new column (whether each row's last encounter
/// date was stated) can only be computed by replaying the credit, and this is what replays it.
/// </para>
/// <para>
/// <b>When it fires.</b> Only when the table holds no rows AND some transition records that it credited at
/// least one item. In normal operation that state is unreachable: once anything has credited, there is a
/// row. The one other way to reach it is deleting every curriculum item a trainee was credited against.
/// Then the rebuild finds nothing to write, logs that it wrote nothing, and runs again on the next boot.
/// That costs one replay per boot and changes nothing.
/// </para>
/// <para>
/// It goes through MediatR, so the rebuild is audited like any other command, under a system principal holding
/// the Administrator role, because the rebuild refuses anyone else. There is no HTTP request at startup, so the
/// actor is declared to the audit context explicitly; without that, the row would name nobody. It never fails startup: a rebuild
/// that throws has rolled itself back, and the page still offers the manual rebuild. The switch is
/// <c>Wombat__RebuildEmptyCurriculumProgress=false</c>. When it is off, this logs what it would have done
/// and does nothing.
/// </para>
/// </remarks>
public sealed class CurriculumProgressBootstrapper
{
    /// <summary>The name the system principal carries, and the actor its audit row records (declared, see <see cref="RunAsync" />).</summary>
    public const string SystemActorName = "system:curriculum-progress-bootstrap";

    private readonly ApplicationDbContext _dbContext;
    private readonly ISender _sender;
    private readonly IAuditContextProvider _auditContext;
    private readonly IOptions<WombatOptions> _options;
    private readonly ILogger<CurriculumProgressBootstrapper> _logger;

    public CurriculumProgressBootstrapper(
        ApplicationDbContext dbContext,
        ISender sender,
        IAuditContextProvider auditContext,
        IOptions<WombatOptions> options,
        ILogger<CurriculumProgressBootstrapper> logger)
    {
        _dbContext = dbContext;
        _sender = sender;
        _auditContext = auditContext;
        _options = options;
        _logger = logger;
    }

    /// <summary>The rebuild's result when one ran, otherwise null.</summary>
    public async Task<RebuildCurriculumProgressResult?> RunAsync(CancellationToken cancellationToken = default)
    {
        if (await _dbContext.CurriculumItemProgresses.AnyAsync(cancellationToken))
        {
            return null;
        }

        var creditedTransitions = await _dbContext.Set<ActivityTransition>()
            .CountAsync(transition => transition.CreditedItemCount > 0, cancellationToken);

        if (creditedTransitions == 0)
        {
            return null;
        }

        if (!_options.Value.RebuildEmptyCurriculumProgress)
        {
            _logger.LogWarning(
                "Curriculum progress is empty but {Count} completions record credit. NOT rebuilt: the startup rebuild is disabled (set Wombat__RebuildEmptyCurriculumProgress=true, or use /admin/curriculum-progress).",
                creditedTransitions);
            return null;
        }

        try
        {
            // The audit pipeline resolves this same scoped provider for the dispatch below.
            _auditContext.DeclareActor(SystemActorName, SystemActorName);
            var result = await _sender.Send(new RebuildCurriculumProgressCommand(SystemAdministrator()), cancellationToken);

            _logger.LogInformation(
                "Curriculum progress was empty and {Count} completions record credit, so it was rebuilt: {Replayed} activities replayed, {Written} semester tallies written, {Stamped} transitions re-stamped.",
                creditedTransitions,
                result.ActivitiesReplayed,
                result.ProgressRowsWritten,
                result.TransitionsStamped);

            return result;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(
                exception,
                "The startup rebuild of curriculum progress failed and was rolled back. Progress stays empty until a rebuild succeeds; run it from /admin/curriculum-progress.");
            return null;
        }
    }

    private static ClaimsPrincipal SystemAdministrator()
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, SystemActorName),
                new Claim(ClaimTypes.Role, WombatRoles.Administrator)
            ],
            authenticationType: "system"));
}
