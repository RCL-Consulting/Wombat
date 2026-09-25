using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Institutions;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>
/// What the new-panel form may offer this caller: the scopes, and for a Speciality-scoped panel the specialities, that
/// creating a panel would accept. (T194)
/// </summary>
/// <param name="MayCreateInstitutionWide">Whether the institution-wide scope is theirs to create.</param>
/// <param name="Specialities">
/// The specialities a Speciality-scoped panel of theirs may cover, when that is only some (a Speciality or
/// SubSpecialityAdmin's own); null when it is any (an InstitutionalAdmin or a global Administrator), for whom the form
/// offers the speciality list it reads for every caller. Empty when it is none.
/// </param>
public sealed record DecisionPanelFormOptionsDto(bool MayCreateInstitutionWide, IReadOnlyList<SpecialityDto>? Specialities)
{
    /// <summary>Whether this caller may create any panel at all.</summary>
    public bool MayCreateAny => MayCreateInstitutionWide || Specialities is not { Count: 0 };
}

/// <summary>
/// The panel form's offer, read from the rule creating a panel demands
/// (<see cref="CommitteeDecisionAuthorization.PanelReachAsync" />), so the form offers a scope and a speciality exactly
/// when <see cref="CreateDecisionPanelCommand" /> would accept them. (T194)
/// </summary>
/// <remarks>
/// Before T194 the form decided from the caller's roles. A Speciality or SubSpecialityAdmin was offered only the
/// Speciality scope, rightly, but the speciality list came from <c>GetSpecialitiesListQuery</c>, which lists a College's
/// specialities, or the ones an InstitutionalAdmin's institution has adopted, and nothing to anyone else: the form offered
/// them a scope with no speciality to choose, and they could create no panel at all.
/// </remarks>
public sealed record GetDecisionPanelFormOptionsQuery(ClaimsPrincipal Principal) : IRequest<DecisionPanelFormOptionsDto>;

public sealed class GetDecisionPanelFormOptionsQueryHandler
    : IRequestHandler<GetDecisionPanelFormOptionsQuery, DecisionPanelFormOptionsDto>
{
    private readonly IApplicationDbContext _dbContext;

    public GetDecisionPanelFormOptionsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DecisionPanelFormOptionsDto> Handle(
        GetDecisionPanelFormOptionsQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request.Principal);

        var reach = await CommitteeDecisionAuthorization.PanelReachAsync(_dbContext, request.Principal, cancellationToken);
        if (reach.ManagesEveryPanel)
        {
            return new DecisionPanelFormOptionsDto(MayCreateInstitutionWide: true, Specialities: null);
        }

        if (reach.InstitutionId is null || reach.SpecialityIds.Count == 0)
        {
            return new DecisionPanelFormOptionsDto(MayCreateInstitutionWide: false, Specialities: []);
        }

        var specialityIds = reach.SpecialityIds.ToArray();
        var specialities = await _dbContext.Set<Speciality>()
            .AsNoTracking()
            .Where(speciality => specialityIds.Contains(speciality.Id))
            .OrderBy(speciality => speciality.Name)
            .Select(speciality => new SpecialityDto(
                speciality.Id, speciality.CollegeId, speciality.Name, speciality.Description, speciality.IsActive))
            .ToListAsync(cancellationToken);

        return new DecisionPanelFormOptionsDto(MayCreateInstitutionWide: false, Specialities: specialities);
    }
}
