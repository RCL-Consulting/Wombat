using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Curricula;

namespace Wombat.Application.Features.Curricula;

/// <summary>
/// The EPAs the curriculum item editor's picker offers (T195): for the Add form when <paramref name="ItemId" /> is
/// null, else for that item's edit row. Exactly what the Add and Update handlers accept: an EPA the item may name
/// (<see cref="CurriculumItemEpas.Nameable" />) that the curriculum does not already hold, apart from the edited item's
/// own (<see cref="CurriculumAdminScope.HoldsItsEpaAgainst" />, T222). Empty when every such EPA is already on it.
/// </summary>
/// <remarks>
/// It replaces <c>ListEpasForSubSpecialityQuery</c> on that page, which listed the EPAs in the CALLER's scope: every
/// institution's local EPAs to an Administrator, whatever the item's owner. It authorizes as the command the picker
/// feeds does, so a caller who could not save the item is told so rather than offered a list.
/// </remarks>
public sealed record ListCurriculumItemEpaOptionsQuery(int CurriculumId, int? ItemId, ClaimsPrincipal Principal)
    : IRequest<IReadOnlyList<EpaDto>>;

public sealed class ListCurriculumItemEpaOptionsQueryValidator : AbstractValidator<ListCurriculumItemEpaOptionsQuery>
{
    public ListCurriculumItemEpaOptionsQueryValidator()
    {
        RuleFor(query => query.CurriculumId).GreaterThan(0);
        RuleFor(query => query.ItemId).GreaterThan(0).When(query => query.ItemId.HasValue);
        RuleFor(query => query.Principal).NotNull();
    }
}

public sealed class ListCurriculumItemEpaOptionsQueryHandler
    : IRequestHandler<ListCurriculumItemEpaOptionsQuery, IReadOnlyList<EpaDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public ListCurriculumItemEpaOptionsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EpaDto>> Handle(ListCurriculumItemEpaOptionsQuery request, CancellationToken cancellationToken)
    {
        var curriculum = await _dbContext.Set<Curriculum>()
            .AsNoTracking()
            .Where(entity => entity.Id == request.CurriculumId)
            .Select(entity => new { entity.SubSpecialityId, entity.SubSpeciality.Speciality.CollegeId })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("The requested curriculum was not found.");

        int? owningInstitutionId;
        if (request.ItemId is int itemId)
        {
            // The Update handler's order: the coarse gate before the lookup, so an item's existence is not told to a
            // caller who could touch no item of this curriculum.
            if (!request.Principal.CanAccessCollege(curriculum.CollegeId) && !request.Principal.IsInstitutionalAdmin())
            {
                throw new UnauthorizedAccessException("You do not have permission to modify this curriculum.");
            }

            var item = await _dbContext.Set<CurriculumItem>()
                .AsNoTracking()
                .Where(entity => entity.Id == itemId && entity.CurriculumId == request.CurriculumId)
                .Select(entity => new { entity.OwningInstitutionId })
                .SingleOrDefaultAsync(cancellationToken)
                ?? throw new InvalidOperationException("The requested curriculum item was not found.");
            owningInstitutionId = item.OwningInstitutionId;
        }
        else
        {
            owningInstitutionId = CurriculumItemEpas.OwnerOfNewItem(request.Principal, curriculum.CollegeId);
        }

        if (!CurriculumItemEpas.MayWrite(request.Principal, curriculum.CollegeId, owningInstitutionId))
        {
            throw new UnauthorizedAccessException("You do not have permission to modify this curriculum.");
        }

        return await CurriculumItemEpas.ListAsync(
            _dbContext, request.CurriculumId, curriculum.SubSpecialityId, owningInstitutionId, request.ItemId, cancellationToken);
    }
}
