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
/// The specialities a Speciality-scoped panel of theirs may cover
/// (<c>CommitteeDecisionAuthorization.CreatableSpecialityIdsAsync</c>, T245): the ones the panel's institution has adopted,
/// all of them for an InstitutionalAdmin or a global Administrator and only their own for a Speciality or
/// SubSpecialityAdmin. Null when it depends on an institution not yet chosen: a global Administrator's, asked without
/// <see cref="GetDecisionPanelFormOptionsQuery.InstitutionId" />, who may create a speciality panel at an institution
/// once it is chosen. Empty when it is none.
/// </param>
public sealed record DecisionPanelFormOptionsDto(bool MayCreateInstitutionWide, IReadOnlyList<SpecialityDto>? Specialities)
{
    /// <summary>Whether this caller may create any panel at all.</summary>
    public bool MayCreateAny => MayCreateInstitutionWide || Specialities is not { Count: 0 };

    /// <summary>
    /// Why the panel pages offer this caller no panel to create or change, when the reason is the Trainee role beside a
    /// role that manages panels (<c>CommitteeDecisionAuthorization.TraineeNoteOnPanelPages</c>, T256); null for anyone
    /// else. The sentence panel create and update refuse such a caller with, before any panel is looked up, so the panel
    /// list and the panel form say it in place of what they would offer, whichever panel is asked for.
    /// </summary>
    public string? TraineeNote { get; init; }
}

/// <summary>
/// The panel form's offer, read from the rule creating a panel demands
/// (<see cref="CommitteeDecisionAuthorization.PanelReachAsync" />, and for a speciality
/// <see cref="CommitteeDecisionAuthorization.CreatableSpecialityIdsAsync" />, T245), so the form offers a scope and a
/// speciality exactly when <see cref="CreateDecisionPanelCommand" /> would accept them. (T194) The panel list reads it too, to offer New panel
/// only when creating one would be accepted, and both panel pages read its <see cref="DecisionPanelFormOptionsDto.TraineeNote" />
/// to say why they offer someone who holds Trainee nothing (T256).
/// </summary>
/// <param name="InstitutionId">
/// The institution a global Administrator has chosen for the new panel, whose adopted specialities are then offered; null
/// until they choose one. Ignored for anyone else, whose panels run at their own institution. (T245 review)
/// </param>
/// <remarks>
/// Before T194 the form decided from the caller's roles. A Speciality or SubSpecialityAdmin was offered only the
/// Speciality scope, rightly, but the speciality list came from <c>GetSpecialitiesListQuery</c>, which lists a College's
/// specialities, or the ones an InstitutionalAdmin's institution has adopted, and nothing to anyone else: the form offered
/// them a scope with no speciality to choose, and they could create no panel at all. Until T245 the offer was every
/// speciality their claims named, adopted or not, an InstitutionalAdmin's was the speciality list, and an Administrator's
/// every speciality whichever institution they chose, while create accepted any speciality from each of them.
/// </remarks>
public sealed record GetDecisionPanelFormOptionsQuery(ClaimsPrincipal Principal, int? InstitutionId = null)
    : IRequest<DecisionPanelFormOptionsDto>;

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

        // T256: someone who holds Trainee beside a role that manages panels reaches none (PanelReachAsync asks it first),
        // and is told why.
        if (CommitteeDecisionAuthorization.TraineeNoteOnPanelPages(request.Principal) is { } traineeNote)
        {
            return new DecisionPanelFormOptionsDto(MayCreateInstitutionWide: false, Specialities: []) { TraineeNote = traineeNote };
        }

        var reach = await CommitteeDecisionAuthorization.PanelReachAsync(_dbContext, request.Principal, cancellationToken);

        // The panel's institution: the one a global Administrator chose, and anyone else's own.
        var institutionId = reach.EveryInstitution ? request.InstitutionId : reach.InstitutionId;
        if (institutionId is not int institution)
        {
            // An Administrator who has not chosen one yet may create either scope, and is offered the specialities once
            // they do; anyone else with no institution of their own may create nothing.
            return reach.EveryInstitution
                ? new DecisionPanelFormOptionsDto(MayCreateInstitutionWide: true, Specialities: null)
                : new DecisionPanelFormOptionsDto(MayCreateInstitutionWide: false, Specialities: []);
        }

        // The specialities panel create accepts from them there (T245): those of their reach the institution has adopted.
        var creatable = await CommitteeDecisionAuthorization.CreatableSpecialityIdsAsync(
            _dbContext, reach, institution, cancellationToken);
        if (creatable.Count == 0)
        {
            return new DecisionPanelFormOptionsDto(MayCreateInstitutionWide: reach.ManagesEveryPanel, Specialities: []);
        }

        var specialityIds = creatable.ToArray();
        var specialities = await _dbContext.Set<Speciality>()
            .AsNoTracking()
            .Where(speciality => specialityIds.Contains(speciality.Id))
            .OrderBy(speciality => speciality.Name)
            .Select(speciality => new SpecialityDto(
                speciality.Id, speciality.CollegeId, speciality.Name, speciality.Description, speciality.IsActive))
            .ToListAsync(cancellationToken);

        return new DecisionPanelFormOptionsDto(MayCreateInstitutionWide: reach.ManagesEveryPanel, Specialities: specialities);
    }
}
