using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Users;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Institutions.Queries.GetAdoptedDisciplinesForInstitution;

/// <summary>
/// The specialities and sub-specialities one institution has adopted (T376, flow 12, S13): what the user page's Add a role…
/// offers, read for the PERSON's institution, never the caller's (P27: an Administrator adding a role to the Demo trainee
/// is offered the Demo Institution's). Empty for an institution outside the caller's scope, so an id names nothing.
/// </summary>
public sealed record GetAdoptedDisciplinesForInstitutionQuery(int InstitutionId, ClaimsPrincipal Principal)
    : IRequest<AdoptedDisciplinesDto>;

/// <summary>An institution's adopted disciplines, each list by name.</summary>
public sealed record AdoptedDisciplinesDto(
    IReadOnlyList<AdoptedSpecialityDto> Specialities,
    IReadOnlyList<AdoptedSubSpecialityDto> SubSpecialities)
{
    public static AdoptedDisciplinesDto None { get; } = new([], []);
}

public sealed record AdoptedSpecialityDto(int Id, string Name);

public sealed record AdoptedSubSpecialityDto(int Id, int SpecialityId, string Name);

public sealed class GetAdoptedDisciplinesForInstitutionQueryHandler
    : IRequestHandler<GetAdoptedDisciplinesForInstitutionQuery, AdoptedDisciplinesDto>
{
    private readonly IApplicationDbContext _dbContext;

    public GetAdoptedDisciplinesForInstitutionQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<AdoptedDisciplinesDto> Handle(GetAdoptedDisciplinesForInstitutionQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        // A trainee first (T278; T376 build review, B7): Add a role…'s offer is the Users pages', read by no one they refuse.
        UserAdministrationRules.DemandUserAdministration(request.Principal);

        return request.Principal.CanAccessInstitution(request.InstitutionId)
            ? AdoptedDisciplines.LoadAsync(_dbContext, request.InstitutionId, cancellationToken)
            : Task.FromResult(AdoptedDisciplinesDto.None);
    }
}

/// <summary>
/// The one reading of an institution's adopted disciplines (S13) that Add a role…'s offer and <c>AddRoleToUserCommand</c>'s
/// check share, so the command refuses exactly what the form does not offer.
/// </summary>
/// <remarks>
/// A sub-speciality counts when the institution holds an active adoption in it; a speciality when it holds one in any of
/// its sub-specialities (<see cref="AdoptedSpecialities.IdsAt" />, the predicate the speciality list and panel create read).
/// An inactive speciality or sub-speciality is left out, as Invite a person leaves it out.
/// </remarks>
public static class AdoptedDisciplines
{
    public static async Task<AdoptedDisciplinesDto> LoadAsync(
        IApplicationDbContext dbContext,
        int institutionId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        var specialityIds = AdoptedSpecialities.IdsAt(dbContext, institutionId);
        var specialities = await dbContext.Set<Speciality>()
            .AsNoTracking()
            .Where(entity => entity.IsActive && specialityIds.Contains(entity.Id))
            .OrderBy(entity => entity.Name)
            .ThenBy(entity => entity.Id)
            .Select(entity => new AdoptedSpecialityDto(entity.Id, entity.Name))
            .ToListAsync(cancellationToken);

        var subSpecialityIds = dbContext.Set<InstitutionCurriculumAdoption>()
            .Where(adoption => adoption.IsActive && adoption.InstitutionId == institutionId)
            .Select(adoption => adoption.SubSpecialityId);
        var subSpecialities = await dbContext.Set<SubSpeciality>()
            .AsNoTracking()
            .Where(entity => entity.IsActive && entity.Speciality.IsActive && subSpecialityIds.Contains(entity.Id))
            .OrderBy(entity => entity.Name)
            .ThenBy(entity => entity.Id)
            .Select(entity => new AdoptedSubSpecialityDto(entity.Id, entity.SpecialityId, entity.Name))
            .ToListAsync(cancellationToken);

        return new AdoptedDisciplinesDto(specialities, subSpecialities);
    }
}
