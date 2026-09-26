using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.Activities.Queries;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Invitations;

namespace Wombat.Application.Features.Dashboards.Coordinator;

public sealed record GetCoordinatorDashboardSummaryQuery(ClaimsPrincipal Principal) : IRequest<CoordinatorDashboardSummaryDto>;

public sealed class GetCoordinatorDashboardSummaryQueryHandler
    : IRequestHandler<GetCoordinatorDashboardSummaryQuery, CoordinatorDashboardSummaryDto>
{
    /// <summary>How many stalled requests the card lists, oldest first.</summary>
    public const int StalledListed = 10;

    private readonly IApplicationDbContext _dbContext;
    private readonly DashboardThresholds _thresholds;
    private readonly IUserAdministrationService _users;

    public GetCoordinatorDashboardSummaryQueryHandler(
        IApplicationDbContext dbContext,
        IOptions<DashboardThresholds> thresholds,
        IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _thresholds = thresholds.Value;
        _users = users;
    }

    public async Task<CoordinatorDashboardSummaryDto> Handle(
        GetCoordinatorDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var institutionId = request.Principal.GetInstitutionId();
        var stallCutoff = DateTime.UtcNow.AddDays(-_thresholds.CoordinatorStallDays);
        var expiryCutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // T297: stalled is awaiting a reviewer, read from each activity's pinned workflow (ActivityWaiting), and untouched
        // for the stall days. Until T297 this read the state keyed "submitted", T074's choice for the old draft-to-submitted
        // shape: every rated CPSA instrument waits for its assessor in "requested", so no stalled Mini-CEX ever reached the
        // card, though the assessor nudge mailed about it (Step 3.30). The nudge reads the same predicate, so a request it
        // writes about is here once it has waited this long.
        var awaitingReviewer = await ActivityWaiting.LoadAwaitingReviewerAsync(_dbContext, cancellationToken);

        var stalledQuery = awaitingReviewer.Narrow(_dbContext.Set<Activity>()
                .AsNoTracking()
                .Where(a => a.UpdatedOn < stallCutoff))
            // The stall panel used to be unscoped while the invitation panel beside it was scoped,
            // so a coordinator saw every institution's stalled activities — ids and subject names,
            // each one a link to /activities/{id}. The same read rule that guards the activity
            // itself now decides what reaches this list. (T101)
            .WhereReadableBy(request.Principal);

        // Every candidate, then the exact test per pin, then the oldest: taking ten in SQL first could keep rows the exact
        // test drops and lose older ones it keeps.
        var stalledActivities = (await stalledQuery
                .Select(a => new
                {
                    a.Id,
                    a.ActivityTypeId,
                    a.SchemaVersion,
                    a.CurrentState,
                    TypeName = a.ActivityType.Name,
                    a.SubjectUserId,
                    a.UpdatedOn
                })
                .ToListAsync(cancellationToken))
            .Where(a => awaitingReviewer.Waits(a.ActivityTypeId, a.SchemaVersion, a.CurrentState))
            .OrderBy(a => a.UpdatedOn)
            .ThenBy(a => a.Id)
            .Take(StalledListed)
            .ToList();

        // Resolve the trainee's display name (the panel previously showed the raw UserId GUID).
        var nameCache = new Dictionary<string, string>(StringComparer.Ordinal);
        var stalled = new List<StalledRequestItem>(stalledActivities.Count);
        foreach (var activity in stalledActivities)
        {
            if (!nameCache.TryGetValue(activity.SubjectUserId, out var subjectName))
            {
                var details = await _users.GetByIdAsync(activity.SubjectUserId, cancellationToken);
                subjectName = FormatSubjectName(details, activity.SubjectUserId);
                nameCache[activity.SubjectUserId] = subjectName;
            }

            stalled.Add(new StalledRequestItem(activity.Id, activity.TypeName, subjectName, activity.UpdatedOn));
        }

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

    private static string FormatSubjectName(UserIdentityDetails? user, string fallbackUserId)
    {
        if (user is null)
        {
            return fallbackUserId;
        }

        var name = string.Join(" ", new[] { user.FirstName, user.LastName }
            .Where(part => !string.IsNullOrWhiteSpace(part)));
        return string.IsNullOrWhiteSpace(name) ? user.Email : name;
    }
}
