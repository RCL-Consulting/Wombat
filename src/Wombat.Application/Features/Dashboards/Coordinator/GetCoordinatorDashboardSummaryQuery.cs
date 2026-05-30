using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Domain.Activities;
using Wombat.Domain.Invitations;

namespace Wombat.Application.Features.Dashboards.Coordinator;

public sealed record GetCoordinatorDashboardSummaryQuery(ClaimsPrincipal Principal) : IRequest<CoordinatorDashboardSummaryDto>;

public sealed class GetCoordinatorDashboardSummaryQueryHandler
    : IRequestHandler<GetCoordinatorDashboardSummaryQuery, CoordinatorDashboardSummaryDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly DashboardThresholds _thresholds;

    public GetCoordinatorDashboardSummaryQueryHandler(
        IApplicationDbContext dbContext,
        IOptions<DashboardThresholds> thresholds)
    {
        _dbContext = dbContext;
        _thresholds = thresholds.Value;
    }

    public async Task<CoordinatorDashboardSummaryDto> Handle(
        GetCoordinatorDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var institutionId = request.Principal.GetInstitutionId();
        var stallCutoff = DateTime.UtcNow.AddDays(-_thresholds.CoordinatorStallDays);
        var expiryCutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var stalledQuery = _dbContext.Set<Activity>()
            .AsNoTracking()
            .Include(a => a.ActivityType)
            .Where(a => a.CurrentState == "submitted" && a.UpdatedOn < stallCutoff);

        var stalled = await stalledQuery
            .OrderBy(a => a.UpdatedOn)
            .Take(10)
            .Select(a => new StalledRequestItem(
                a.Id, a.ActivityType.Name, a.SubjectUserId, a.UpdatedOn))
            .ToListAsync(cancellationToken);

        var expiringQuery = _dbContext.Set<Invitation>()
            .AsNoTracking()
            .Where(i => i.UsedOn == null && i.RevokedOn == null &&
                        i.ExpiresOn >= today && i.ExpiresOn <= expiryCutoff);

        if (institutionId.HasValue)
        {
            expiringQuery = expiringQuery.Where(i => i.InstitutionId == institutionId.Value);
        }

        var expiring = await expiringQuery
            .OrderBy(i => i.ExpiresOn)
            .Take(10)
            .Select(i => new ExpiringInvitationItem(
                i.Id, i.Email, i.TargetRole, i.ExpiresOn))
            .ToListAsync(cancellationToken);

        return new CoordinatorDashboardSummaryDto(stalled, expiring);
    }
}
