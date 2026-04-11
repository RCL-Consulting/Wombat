using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.Epas;

public sealed record DeactivateEpaCommand(int Id) : IRequest;
public sealed record ListEpasForSubSpecialityQuery(int? SubSpecialityId = null) : IRequest<IReadOnlyList<EpaDto>>;
public sealed record GetEpaByIdQuery(int Id) : IRequest<EpaDto?>;
public sealed record GetEntrustmentScalesListQuery() : IRequest<IReadOnlyList<EntrustmentScaleDto>>;

public sealed class DeactivateEpaCommandHandler : IRequestHandler<DeactivateEpaCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public DeactivateEpaCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(DeactivateEpaCommand request, CancellationToken cancellationToken)
    {
        var epa = await _dbContext.Set<Epa>().SingleOrDefaultAsync(entity => entity.Id == request.Id, cancellationToken);

        if (epa is null)
        {
            throw new InvalidOperationException("The requested EPA was not found.");
        }

        epa.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

public sealed class ListEpasForSubSpecialityQueryHandler : IRequestHandler<ListEpasForSubSpecialityQuery, IReadOnlyList<EpaDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public ListEpasForSubSpecialityQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EpaDto>> Handle(ListEpasForSubSpecialityQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Set<Epa>().AsQueryable();

        if (request.SubSpecialityId.HasValue)
        {
            query = query.Where(entity => entity.SubSpecialityId == request.SubSpecialityId.Value);
        }

        return await query
            .OrderBy(entity => entity.SubSpeciality.Speciality.Institution.Name)
            .ThenBy(entity => entity.SubSpeciality.Speciality.Name)
            .ThenBy(entity => entity.SubSpeciality.Name)
            .ThenBy(entity => entity.Code)
            .Select(entity => new EpaDto(
                entity.Id,
                entity.SubSpecialityId,
                entity.SubSpeciality.Name,
                entity.Code,
                entity.Title,
                entity.Description,
                entity.RequiredKnowledgeSkills,
                entity.IsActive,
                entity.CreatedOn))
            .ToListAsync(cancellationToken);
    }
}

public sealed class GetEpaByIdQueryHandler : IRequestHandler<GetEpaByIdQuery, EpaDto?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetEpaByIdQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<EpaDto?> Handle(GetEpaByIdQuery request, CancellationToken cancellationToken)
        => await _dbContext.Set<Epa>()
            .Where(entity => entity.Id == request.Id)
            .Select(entity => new EpaDto(
                entity.Id,
                entity.SubSpecialityId,
                entity.SubSpeciality.Name,
                entity.Code,
                entity.Title,
                entity.Description,
                entity.RequiredKnowledgeSkills,
                entity.IsActive,
                entity.CreatedOn))
            .SingleOrDefaultAsync(cancellationToken);
}

public sealed class GetEntrustmentScalesListQueryHandler : IRequestHandler<GetEntrustmentScalesListQuery, IReadOnlyList<EntrustmentScaleDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetEntrustmentScalesListQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EntrustmentScaleDto>> Handle(GetEntrustmentScalesListQuery request, CancellationToken cancellationToken)
        => await _dbContext.Set<EntrustmentScale>()
            .OrderBy(entity => entity.Name)
            .Select(entity => new EntrustmentScaleDto(
                entity.Id,
                entity.Name,
                entity.Description,
                entity.Levels
                    .OrderBy(level => level.Order)
                    .Select(level => new EntrustmentLevelDto(level.Id, level.Order, level.Label, level.Description))
                    .ToList()))
            .ToListAsync(cancellationToken);
}
