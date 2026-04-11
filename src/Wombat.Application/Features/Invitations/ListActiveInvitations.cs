using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Institutions;
using Wombat.Domain.Invitations;

namespace Wombat.Application.Features.Invitations;

public sealed record ListActiveInvitationsQuery() : IRequest<IReadOnlyList<ActiveInvitationDto>>;

public sealed class ListActiveInvitationsQueryHandler : IRequestHandler<ListActiveInvitationsQuery, IReadOnlyList<ActiveInvitationDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public ListActiveInvitationsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ActiveInvitationDto>> Handle(ListActiveInvitationsQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var institutions = _dbContext.Set<Institution>();
        var specialities = _dbContext.Set<Speciality>();
        var subSpecialities = _dbContext.Set<SubSpeciality>();

        return await (
            from invitation in _dbContext.Set<Invitation>()
            join institution in institutions on invitation.InstitutionId equals institution.Id
            join speciality in specialities on invitation.SpecialityId equals speciality.Id into specialityGroup
            from speciality in specialityGroup.DefaultIfEmpty()
            join subSpeciality in subSpecialities on invitation.SubSpecialityId equals subSpeciality.Id into subSpecialityGroup
            from subSpeciality in subSpecialityGroup.DefaultIfEmpty()
            where invitation.RevokedOn == null &&
                  invitation.UsedOn == null &&
                  invitation.ExpiresOn >= today
            orderby invitation.IssuedOn descending
            select new ActiveInvitationDto(
                invitation.Id,
                invitation.Email,
                invitation.TargetRole,
                invitation.InstitutionId,
                institution.Name,
                invitation.SpecialityId,
                speciality != null ? speciality.Name : null,
                invitation.SubSpecialityId,
                subSpeciality != null ? subSpeciality.Name : null,
                invitation.IssuedOn,
                invitation.ExpiresOn))
            .ToListAsync(cancellationToken);
    }
}
