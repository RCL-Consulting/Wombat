using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <param name="InstitutionId">
/// A global Administrator's choice of institution (T364, round 3 F2): an Administrator belongs to none, so Decision panels
/// lists the panels of the one they chose with Show. Null lists every panel, as before. Ignored for anyone else, whose
/// list is their own institution's panels and the panels they sit on.
/// </param>
/// <param name="ForScheduling">
/// True for the scheduling page's panel picker: only the panels this caller may put a trainee before
/// (<see cref="CommitteeTraineeScope.ListSchedulablePanelsAsync" />, T194). False for the panels page, which lists every
/// panel the caller may read.
/// </param>
public sealed record ListDecisionPanelsQuery(ClaimsPrincipal Principal, bool ForScheduling = false, int? InstitutionId = null)
    : IRequest<IReadOnlyList<DecisionPanelSummaryDto>>;

public sealed class ListDecisionPanelsQueryHandler : IRequestHandler<ListDecisionPanelsQuery, IReadOnlyList<DecisionPanelSummaryDto>>
{
    /// <summary>A member whose account no longer exists, so has no name to show (T364).</summary>
    internal const string UnnamedMember = "A member with no account";

    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;

    public ListDecisionPanelsQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
    }

    /// <remarks>
    /// <para>
    /// A global Administrator sees every panel. Everyone else sees the panels their own institution runs, and any
    /// panel they sit on (an External member may come from elsewhere). Until T182 only an InstitutionalAdmin was
    /// filtered (T063), so a Coordinator, SpecialityAdmin or CommitteeMember at one hospital listed every panel in the
    /// country, and the scheduling page offered them panels its handler would refuse.
    /// </para>
    /// <para>
    /// The scheduling page asks for <see cref="ListDecisionPanelsQuery.ForScheduling" />, which keeps only the panels on
    /// which the scheduling rule accepts at least one trainee from this caller that the trainee picker can name: the panel
    /// picker offers a panel exactly when its trainee picker would offer someone. Until T194 it read the whole list, so an
    /// External member from another institution was offered that panel with no trainee to choose, and a SpecialityAdmin
    /// another speciality's panel.
    /// </para>
    /// </remarks>
    public async Task<IReadOnlyList<DecisionPanelSummaryDto>> Handle(ListDecisionPanelsQuery request, CancellationToken cancellationToken)
    {
        var panels = _dbContext.Set<DecisionPanel>().AsNoTracking();

        if (request.Principal.IsAdministrator())
        {
            if (request.InstitutionId is int chosen)
            {
                panels = panels.Where(panel => panel.InstitutionId == chosen);
            }
        }
        else
        {
            var institutionId = request.Principal.GetInstitutionId();
            var userId = request.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            panels = panels.Where(panel =>
                (institutionId != null && panel.InstitutionId == institutionId) ||
                (userId != null && panel.Members.Any(member => member.UserId == userId)));
        }

        if (request.ForScheduling)
        {
            var candidates = await panels.ToListAsync(cancellationToken);
            var schedulableIds = (await CommitteeTraineeScope.ListSchedulablePanelsAsync(
                    _dbContext, _users, request.Principal, candidates, cancellationToken))
                .Select(panel => panel.Id)
                .ToArray();

            if (schedulableIds.Length == 0)
            {
                return [];
            }

            panels = panels.Where(panel => schedulableIds.Contains(panel.Id));
        }

        var rows = await panels
            .OrderBy(panel => panel.Name)
            .ThenBy(panel => panel.Id)
            .Select(panel => new
            {
                Summary = new DecisionPanelSummaryDto(
                    panel.Id,
                    panel.Name,
                    panel.Scope,
                    panel.InstitutionId,
                    panel.SpecialityId,
                    panel.Members.Count,
                    panel.DecisionBodyKey,
                    panel.DecisionBody == null ? null : panel.DecisionBody.Name),
                Members = panel.Members
                    .Select(member => new { member.Id, member.UserId, member.Role })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        // The scheduling picker shows a panel's name only, so it reads no other name (T364, build review R6).
        if (request.ForScheduling)
        {
            return await WithReachAsync(rows.Select(row => row.Summary).ToList(), request.Principal, cancellationToken);
        }

        // The list names what each panel covers and who sits on it (T364, round 3 M8; Q8): the institution and speciality
        // by name, and every member by name as stored, read once for the page.
        var institutionIds = rows.Select(row => row.Summary.InstitutionId).OfType<int>().Distinct().ToArray();
        var institutionNames = await _dbContext.Set<Institution>()
            .AsNoTracking()
            .Where(institution => institutionIds.Contains(institution.Id))
            .ToDictionaryAsync(institution => institution.Id, institution => institution.Name, cancellationToken);
        var specialityIds = rows.Select(row => row.Summary.SpecialityId).OfType<int>().Distinct().ToArray();
        var specialityNames = await _dbContext.Set<Speciality>()
            .AsNoTracking()
            .Where(speciality => specialityIds.Contains(speciality.Id))
            .ToDictionaryAsync(speciality => speciality.Id, speciality => speciality.Name, cancellationToken);
        var userIds = rows.SelectMany(row => row.Members.Select(member => member.UserId)).Distinct(StringComparer.Ordinal).ToArray();
        var names = userIds.Length == 0
            ? new Dictionary<string, string>(StringComparer.Ordinal)
            : await _users.GetDisplayNamesAsync(userIds, cancellationToken);

        var summaries = rows
            .Select(row => row.Summary with
            {
                InstitutionName = row.Summary.InstitutionId is int institutionId &&
                                  institutionNames.TryGetValue(institutionId, out var institutionName)
                    ? institutionName
                    : null,
                SpecialityName = row.Summary.SpecialityId is int specialityId &&
                                 specialityNames.TryGetValue(specialityId, out var specialityName)
                    ? specialityName
                    : null,
                MemberNames = row.Members
                    .OrderBy(member => member.Role)
                    .ThenBy(member => member.Id)
                    .Select(member => new DecisionPanelMemberNameDto(
                        names.TryGetValue(member.UserId, out var name) ? name : UnnamedMember, member.Role))
                    .ToArray()
            })
            .ToList();

        return await WithReachAsync(summaries, request.Principal, cancellationToken);
    }

    /// <summary>
    /// Which of them the caller may open to change, by the rule the panel form's read and panel update demand, so the
    /// list offers Edit exactly where the form opens: never to someone who holds Trainee (T256), a committee member or a
    /// coordinator, and to a Speciality or SubSpecialityAdmin only on their own speciality's panels.
    /// </summary>
    private async Task<IReadOnlyList<DecisionPanelSummaryDto>> WithReachAsync(
        IReadOnlyList<DecisionPanelSummaryDto> summaries, ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var reach = await CommitteeDecisionAuthorization.PanelReachAsync(_dbContext, principal, cancellationToken);
        return summaries
            .Select(summary => summary with
            {
                CallerMayManage = summary.InstitutionId is int institutionId &&
                                  reach.Admits(institutionId, summary.Scope, summary.SpecialityId)
            })
            .ToArray();
    }
}
