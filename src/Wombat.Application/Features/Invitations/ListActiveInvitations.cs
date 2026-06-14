using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Institutions;
using Wombat.Domain.Invitations;

namespace Wombat.Application.Features.Invitations;

public sealed record ListActiveInvitationsQuery(ClaimsPrincipal Principal) : IRequest<IReadOnlyList<ActiveInvitationDto>>;

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
        var colleges = _dbContext.Set<College>();
        var specialities = _dbContext.Set<Speciality>();
        var subSpecialities = _dbContext.Set<SubSpeciality>();

        var invitations = _dbContext.Set<Invitation>().AsQueryable();

        if (!request.Principal.IsAdministrator())
        {
            var scopedInstitutionId = request.Principal.GetInstitutionId();
            if (!scopedInstitutionId.HasValue)
            {
                return Array.Empty<ActiveInvitationDto>();
            }
            invitations = invitations.Where(entity => entity.InstitutionId == scopedInstitutionId.Value);
        }

        return await (
            from invitation in invitations
            join institution in institutions on invitation.InstitutionId equals institution.Id into institutionGroup
            from institution in institutionGroup.DefaultIfEmpty()
            join college in colleges on invitation.CollegeId equals college.Id into collegeGroup
            from college in collegeGroup.DefaultIfEmpty()
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
                institution != null ? institution.Name : null,
                invitation.CollegeId,
                college != null ? college.Name : null,
                invitation.SpecialityId,
                speciality != null ? speciality.Name : null,
                invitation.SubSpecialityId,
                subSpeciality != null ? subSpeciality.Name : null,
                invitation.IssuedOn,
                invitation.ExpiresOn))
            .ToListAsync(cancellationToken);
    }
}
