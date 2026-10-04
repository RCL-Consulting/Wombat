using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.Activities;

namespace Wombat.Application.Features.Activities.Queries.GetActivityCountLine;

/// <summary>
/// The count a completed activity made, for its own status card (T355, C5; E5): "PAED-001: 1 of 3 this semester." after
/// the built credit sentence, read by <see cref="EpaCountLines" />, the reader Home's Recent decisions shares, so the card
/// and Home say the same count of the same activity.
/// </summary>
/// <remarks>
/// Null unless the caller is the activity's subject (a registrar's own count, never shown to her assessor or an overseer,
/// note 4), the activity is about an EPA, and that EPA is an in-force item of her curriculum. An unknown id reads as null,
/// as one she cannot read does, so the answer never says the id exists.
/// </remarks>
public sealed record GetActivityCountLineQuery(ClaimsPrincipal Principal, int ActivityId) : IRequest<EpaCountLineDto?>;

public sealed class GetActivityCountLineQueryHandler : IRequestHandler<GetActivityCountLineQuery, EpaCountLineDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly TimeProvider _clock;

    public GetActivityCountLineQueryHandler(IApplicationDbContext dbContext, TimeProvider clock)
    {
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task<EpaCountLineDto?> Handle(GetActivityCountLineQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request.Principal);

        var callerUserId = request.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(callerUserId))
        {
            return null;
        }

        var activity = await _dbContext.Set<Activity>()
            .AsNoTracking()
            .Where(entity => entity.Id == request.ActivityId && entity.SubjectUserId == callerUserId)
            .WhereReadableBy(request.Principal)
            .Select(entity => new { entity.EpaId, entity.ObservedOn })
            .FirstOrDefaultAsync(cancellationToken);

        if (activity?.EpaId is not int epaId)
        {
            return null;
        }

        var key = (epaId, activity.ObservedOn);
        var lines = await EpaCountLines.ReadAsync(
            _dbContext, callerUserId, [key], QuotaCalendar.Today(_clock), cancellationToken);

        return lines.TryGetValue(key, out var line) ? line : null;
    }
}
