using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Common.Users;
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
/// trainee's decisions in the country. <see cref="TraineeFilter" /> is text the caller types; it narrows only what the
/// scope has already let through, so one that names a trainee out of scope returns what a trainee with no decisions
/// returns, an empty list.
/// </para>
/// <para>
/// A decision carries no institution of its own; its trainee's preferred profile does. The query is narrowed in SQL to
/// the trainees the caller administers, through <see cref="TraineeScopeResolver.AdministeredProfiles" />, the query form
/// of <see cref="TraineeScopeResolver.IsAdministeredBy" />, so only the decisions shown leave the database: a
/// SpecialityAdmin reads their own speciality's trainees at their own institution and no one else's. Until T185 the
/// SQL narrowed only to the institution, and every decision there was loaded and then judged row by row in memory.
/// </para>
/// <para>
/// It is the rule revoking uses (<see cref="EntrustmentDecisionAuthorization.DemandRevocationAccessAsync" />), not the
/// wider <see cref="TraineeScopeResolver.IsOverseenBy" />, so every row the page offers a Revoke button on is one the
/// caller may revoke. A SpecialityAdmin who also sits on the committee oversees every trainee at their institution and
/// may read their decisions (<see cref="GetActiveDecisionsForTraineeQuery" />), but administers only their own
/// speciality's, and this list is the administrator's.
/// </para>
/// <para>
/// So it has revoking's trainee rung too (<see cref="TraineeScopeResolver.ActsAsTrainee" />): an admin, the
/// Administrator included, who also holds Trainee administers nobody's decisions and is shown none, as though there
/// were none to show. Until the T185 review it listed, and offered Revoke on, decisions whose certificate
/// <see cref="TraineeScopeResolver.MayReadAsync" /> refused them.
/// </para>
/// </remarks>
/// <param name="TraineeFilter">
/// Narrows the list to the trainees whose name contains this text, ignoring case, or whose user id is exactly it. The
/// page shows names, not ids (T142), so a filter that took only an id would ask for something the page never shows.
/// </param>
public sealed record ListEntrustmentDecisionsForAdminQuery(
    string? TraineeFilter,
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
    private readonly IUserAdministrationService _users;

    public ListEntrustmentDecisionsForAdminQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
    }

    public async Task<IReadOnlyList<EntrustmentDecisionDto>> Handle(ListEntrustmentDecisionsForAdminQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request.Principal);
        DemandAdminAccess(request.Principal);

        if (TraineeScopeResolver.ActsAsTrainee(request.Principal))
        {
            return [];
        }

        var query = _dbContext.Set<EntrustmentDecision>()
            .AsNoTracking()
            .Include(d => d.Epa)
            .Include(d => d.AuthorisedLevel)
            .Include(d => d.EvidenceLinks)
            .AsQueryable();

        if (!request.Principal.IsAdministrator())
        {
            var administered = TraineeScopeResolver.AdministeredProfiles(_dbContext, request.Principal);
            query = query.Where(d => administered.Any(profile => profile.UserId == d.TraineeUserId));
        }

        if (request.StatusFilter.HasValue)
        {
            query = query.Where(d => d.Status == request.StatusFilter.Value);
        }

        var decisions = await query
            .OrderByDescending(d => d.IssuedOn)
            .ThenByDescending(d => d.Id)
            .ToListAsync(cancellationToken);

        // T142. The Trainee column by name, in one lookup for the rows listed, and only for them: the names follow
        // whatever this handler's scope (T183, above) lets through. The trainee filter matches those names, so it runs
        // after the lookup and can only narrow what the scope has already let through.
        var names = await UserDisplayNames.ResolveAsync(
            _users, decisions.Select(d => d.TraineeUserId), cancellationToken);

        var rows = decisions.Select(d => d.ToDto() with { TraineeName = names.NameOf(d.TraineeUserId) });

        var trainee = request.TraineeFilter?.Trim();
        if (!string.IsNullOrEmpty(trainee))
        {
            rows = rows.Where(row =>
                row.TraineeName!.Contains(trainee, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(row.TraineeUserId, trainee, StringComparison.Ordinal));
        }

        return rows.ToArray();
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
