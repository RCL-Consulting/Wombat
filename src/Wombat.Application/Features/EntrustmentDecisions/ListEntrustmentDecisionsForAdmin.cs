using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.EntrustmentDecisions;

/// <summary>
/// The entrustment decisions an administrator administers: every decision for a global Administrator, and for an
/// InstitutionalAdmin, SpecialityAdmin or SubSpecialityAdmin the decisions about trainees they administer
/// (<see cref="TraineeScopeResolver.IsAdministeredBy" />). (T183)
/// </summary>
/// <remarks>
/// <para>
/// Until T183 this checked the role and nothing else, so every hospital's admin saw, and could filter to, every
/// trainee's decisions in the country. <see cref="TraineeUserIdFilter" /> is a raw id the caller types; one that names
/// a trainee out of scope returns what a trainee with no decisions returns, an empty list.
/// </para>
/// <para>
/// A decision carries no institution of its own; its trainee's preferred profile does. The query is narrowed to the
/// caller's institution in SQL, through the same preferred-profile set <see cref="TraineeScopeResolver.ResolveAsync" />
/// reads, so no other institution's decisions leave the database. What survives is then judged row by row by
/// <see cref="TraineeScopeResolver.IsAdministeredBy" /> itself, not by a copy of it: a SpecialityAdmin sees their own
/// speciality's trainees at their own institution and no one else's.
/// </para>
/// <para>
/// It is the rule revoking uses (<see cref="EntrustmentDecisionAuthorization.DemandRevocationAccessAsync" />), not the
/// wider <see cref="TraineeScopeResolver.IsOverseenBy" />, so every row the page offers a Revoke button on is one the
/// caller may revoke. A SpecialityAdmin who also sits on the committee oversees every trainee at their institution and
/// may read their decisions (<see cref="GetActiveDecisionsForTraineeQuery" />), but administers only their own
/// speciality's, and this list is the administrator's.
/// </para>
/// </remarks>
public sealed record ListEntrustmentDecisionsForAdminQuery(
    string? TraineeUserIdFilter,
    EntrustmentDecisionStatus? StatusFilter,
    ClaimsPrincipal Principal) : IRequest<IReadOnlyList<EntrustmentDecisionDto>>;

public sealed class ListEntrustmentDecisionsForAdminQueryValidator : AbstractValidator<ListEntrustmentDecisionsForAdminQuery>
{
    public ListEntrustmentDecisionsForAdminQueryValidator()
    {
        RuleFor(query => query.Principal).NotNull();
    }
}

public sealed class ListEntrustmentDecisionsForAdminQueryHandler
    : IRequestHandler<ListEntrustmentDecisionsForAdminQuery, IReadOnlyList<EntrustmentDecisionDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public ListEntrustmentDecisionsForAdminQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EntrustmentDecisionDto>> Handle(ListEntrustmentDecisionsForAdminQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request.Principal);
        DemandAdminAccess(request.Principal);

        var query = _dbContext.Set<EntrustmentDecision>()
            .AsNoTracking()
            .Include(d => d.Epa)
            .Include(d => d.AuthorisedLevel)
            .Include(d => d.EvidenceLinks)
            .AsQueryable();

        if (!request.Principal.IsAdministrator())
        {
            if (request.Principal.GetInstitutionId() is not int institutionId)
            {
                return [];
            }

            var preferredProfiles = TraineeScopeResolver.PreferredProfiles(_dbContext);
            query = query.Where(d => preferredProfiles.Any(profile =>
                profile.UserId == d.TraineeUserId &&
                profile.InstitutionId == institutionId));
        }

        if (!string.IsNullOrWhiteSpace(request.TraineeUserIdFilter))
        {
            query = query.Where(d => d.TraineeUserId == request.TraineeUserIdFilter);
        }

        if (request.StatusFilter.HasValue)
        {
            query = query.Where(d => d.Status == request.StatusFilter.Value);
        }

        var decisions = await query
            .OrderByDescending(d => d.IssuedOn)
            .ThenByDescending(d => d.Id)
            .ToListAsync(cancellationToken);

        if (!request.Principal.IsAdministrator())
        {
            var scopes = await TraineeScopeResolver.ResolveManyAsync(
                _dbContext, decisions.Select(d => d.TraineeUserId), cancellationToken);

            decisions = decisions
                .Where(d => scopes.TryGetValue(d.TraineeUserId, out var scope) &&
                            TraineeScopeResolver.IsAdministeredBy(scope, request.Principal))
                .ToList();
        }

        return decisions.Select(d => d.ToDto()).ToArray();
    }

    private static void DemandAdminAccess(ClaimsPrincipal principal)
    {
        if (principal.IsInRole(WombatRoles.Administrator) ||
            principal.IsInRole(WombatRoles.InstitutionalAdmin) ||
            principal.IsInRole(WombatRoles.SpecialityAdmin) ||
            principal.IsInRole(WombatRoles.SubSpecialityAdmin))
        {
            return;
        }

        throw new UnauthorizedAccessException("Only institutional administrators can list entrustment decisions.");
    }
}
