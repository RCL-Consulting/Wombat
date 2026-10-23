using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.Users.Queries.GetRequestsWaitingForUser;

/// <summary>
/// The requests that wait on one person as their nominee (T376, flow 12, F6): what the user page's Lock out… and Remove…
/// on Assessor dialogs warn about, read when the dialog opens, never on a page view. They stay with the person whatever the
/// dialog does: nothing moves them to anyone else, and a locked account is sent no reminder.
/// </summary>
public sealed record GetRequestsWaitingForUserQuery(string UserId, ClaimsPrincipal Principal) : IRequest<WaitingRequestsDto>;

/// <summary>How many requests wait on the person, since when the oldest, and the oldest five.</summary>
/// <param name="OldestWaitingSince">When the oldest began to wait (its last change, as every waiting list reads it); null for none.</param>
public sealed record WaitingRequestsDto(int Count, DateTimeOffset? OldestWaitingSince, IReadOnlyList<WaitingRow> FirstFive)
{
    public static WaitingRequestsDto None { get; } = new(0, null, []);
}

/// <summary>One waiting request, as the dialog lists it: "Pieter du Plessis · Portfolio and Logbook Review (Paediatrics) · PAED-015 · waiting since 2026-10-12 12:35 SAST · overdue".</summary>
/// <param name="Reference">The EPA the request is about, by its code; null for one about none.</param>
/// <param name="IsOverdue">It has waited <see cref="DashboardThresholds.AssessorDueDays" /> days or more (the assessor's inbox's rule).</param>
public sealed record WaitingRow(
    string TraineeName,
    string ActivityTypeName,
    string? Reference,
    DateTimeOffset WaitingSince,
    bool IsOverdue);

public sealed class GetRequestsWaitingForUserQueryHandler : IRequestHandler<GetRequestsWaitingForUserQuery, WaitingRequestsDto>
{
    /// <summary>How many rows the dialog lists before "and N more." (P32).</summary>
    public const int Listed = 5;

    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;
    private readonly DashboardThresholds _thresholds;
    private readonly TimeProvider _clock;

    public GetRequestsWaitingForUserQueryHandler(
        IApplicationDbContext dbContext,
        IUserAdministrationService users,
        IOptions<DashboardThresholds> thresholds,
        TimeProvider clock)
    {
        _dbContext = dbContext;
        _users = users;
        _thresholds = thresholds.Value;
        _clock = clock;
    }

    /// <remarks>
    /// The person is read through the Users pages' rules (<see cref="UserAdministrationRules.DemandUserAdministration" />,
    /// then <see cref="UserAdministrationRules.DemandInScope" />), so an id outside the caller's scope, or naming nobody, is
    /// refused with one sentence and never answered with a count. The rows are <see cref="AwaitingNominees" />'s, the
    /// nightly nudge's own reading, over every activity, not only those older than the nudge's days.
    /// </remarks>
    public async Task<WaitingRequestsDto> Handle(GetRequestsWaitingForUserQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        UserAdministrationRules.DemandUserAdministration(request.Principal);
        var person = UserAdministrationRules.DemandInScope(
            request.Principal,
            await _users.GetByIdAsync(request.UserId, cancellationToken),
            "You do not have permission to view this user.");

        // An Institutional admin reads only her institution's requests (T376 build review, B1): an assessor moved from
        // another institution may still be named on requests filed there, and neither their count nor a registrar's name
        // crosses to the new institution's admin. The Administrator reads every one; the nudge keeps its own reading.
        var activities = _dbContext.Set<Activity>().AsQueryable();
        if (!request.Principal.IsAdministrator())
        {
            var institutionId = person.InstitutionId;
            activities = activities.Where(activity => activity.InstitutionId == institutionId);
        }

        var waiting = await AwaitingNominees.LoadForAsync(_dbContext, activities, person.UserId, cancellationToken);
        if (waiting.Count == 0)
        {
            return WaitingRequestsDto.None;
        }

        var shown = waiting.Take(Listed).ToList();
        var names = await _users.GetDisplayNamesAsync(
            shown.Select(row => row.Activity.SubjectUserId).Distinct(StringComparer.Ordinal).ToArray(), cancellationToken);

        var epaIds = shown.Where(row => row.Activity.EpaId.HasValue).Select(row => row.Activity.EpaId!.Value).Distinct().ToArray();
        var codes = epaIds.Length == 0
            ? new Dictionary<int, string>()
            : await _dbContext.Set<Epa>()
                .AsNoTracking()
                .Where(epa => epaIds.Contains(epa.Id))
                .ToDictionaryAsync(epa => epa.Id, epa => epa.Code, cancellationToken);

        var now = _clock.GetUtcNow().UtcDateTime;
        var due = TimeSpan.FromDays(_thresholds.AssessorDueDays);
        var rows = shown
            .Select(row => new WaitingRow(
                names.TryGetValue(row.Activity.SubjectUserId, out var name) ? name : "Unknown trainee",
                row.Activity.ActivityType.Name,
                row.Activity.EpaId is int epaId && codes.TryGetValue(epaId, out var code) ? code : null,
                Utc(row.Activity.UpdatedOn),
                now - row.Activity.UpdatedOn >= due))
            .ToList();

        return new WaitingRequestsDto(waiting.Count, Utc(waiting[0].Activity.UpdatedOn), rows);
    }

    private static DateTimeOffset Utc(DateTime value)
        => new(DateTime.SpecifyKind(value, DateTimeKind.Utc));
}
