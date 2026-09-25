using System.Linq.Expressions;
using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Curricula;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Curricula;

/// <summary>The curricula the caller may open (<see cref="CurriculumAdminScope.Openable" />), each cut to what they read.</summary>
public sealed record GetCurriculaListQuery(ClaimsPrincipal Principal) : IRequest<IReadOnlyList<CurriculumDto>>;

/// <summary>
/// One curriculum, cut to what the caller reads, or null when they may not open it (T056: not found, not forbidden).
/// </summary>
public sealed record GetCurriculumByIdQuery(int Id, ClaimsPrincipal Principal) : IRequest<CurriculumDto?>;

public sealed class GetCurriculaListQueryHandler : IRequestHandler<GetCurriculaListQuery, IReadOnlyList<CurriculumDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetCurriculaListQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CurriculumDto>> Handle(GetCurriculaListQuery request, CancellationToken cancellationToken)
    {
        // The same rule as GetCurriculumByIdQuery, so every row the list shows is one the caller can open (T211).
        var rows = await CurriculumAdminScope.Openable(_dbContext, request.Principal)
            .OrderBy(entity => entity.SubSpeciality.Speciality.College.Name)
            .ThenBy(entity => entity.SubSpeciality.Speciality.Name)
            .ThenBy(entity => entity.SubSpeciality.Name)
            .ThenBy(entity => entity.Name)
            .ThenByDescending(entity => entity.EffectiveFrom)
            .Select(CurriculumRow.Projection(_dbContext.Set<Institution>()))
            .ToListAsync(cancellationToken);

        return rows.Select(row => CurriculumAdminScope.ForCaller(row.Curriculum, row.CollegeId, request.Principal)).ToList();
    }
}

public sealed class GetCurriculumByIdQueryHandler : IRequestHandler<GetCurriculumByIdQuery, CurriculumDto?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetCurriculumByIdQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CurriculumDto?> Handle(GetCurriculumByIdQuery request, CancellationToken cancellationToken)
    {
        var row = await CurriculumAdminScope.Openable(_dbContext, request.Principal)
            .Where(entity => entity.Id == request.Id)
            .Select(CurriculumRow.Projection(_dbContext.Set<Institution>()))
            .SingleOrDefaultAsync(cancellationToken);

        return row is null ? null : CurriculumAdminScope.ForCaller(row.Curriculum, row.CollegeId, request.Principal);
    }
}

/// <summary>
/// A curriculum as read for the admin pages, with every item and the College it belongs to, before
/// <see cref="CurriculumAdminScope.ForCaller" /> cuts it to the caller. The list and the one-curriculum read share it.
/// </summary>
internal sealed record CurriculumRow(CurriculumDto Curriculum, int CollegeId)
{
    /// <summary>The projection, with each local item's owner looked up by name in <paramref name="institutions" /> (T222).</summary>
    /// <param name="institutions">
    /// The institutions set. A curriculum item has no navigation to its institution, so its name is a correlated scalar
    /// subquery inside the item projection: the curriculum and its items are still read in one statement.
    /// </param>
    public static Expression<Func<Curriculum, CurriculumRow>> Projection(IQueryable<Institution> institutions) => entity => new CurriculumRow(
        new CurriculumDto(
            entity.Id,
            entity.SubSpeciality.SpecialityId,
            entity.SubSpecialityId,
            entity.SubSpeciality.Speciality.Name,
            entity.SubSpeciality.Name,
            entity.SubSpeciality.Speciality.College.Name,
            entity.Name,
            entity.Version,
            entity.EffectiveFrom,
            entity.EffectiveTo,
            entity.IsActive,
            true,
            entity.Items
                .OrderBy(item => item.Epa.Code)
                .Select(item => new CurriculumItemDto(item.Id, item.EpaId, item.Epa.Code, item.Epa.Title, item.RequiredCount, item.QuotaPeriod, item.MinimumLevelOrder, item.WindowMonths, item.Weight, item.MinimumLevelByStageJson, item.PermittedToolsJson, item.Epa.IsActive, item.OwningInstitutionId,
                    institutions.Where(institution => institution.Id == item.OwningInstitutionId).Select(institution => (string?)institution.Name).FirstOrDefault(),
                    item.DecisionCadence, item.DecisionBodyKey, item.DecisionBody == null ? null : item.DecisionBody.Name, item.DecisionIsOpportunistic, item.ScaleId, item.Scale == null ? null : item.Scale.Name))
                .ToList(),
            entity.SubSpeciality.DefaultEntrustmentScaleId),
        entity.SubSpeciality.Speciality.CollegeId);
}
