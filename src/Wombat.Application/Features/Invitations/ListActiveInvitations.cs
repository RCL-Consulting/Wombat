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
    private readonly TimeProvider _timeProvider;

    public ListActiveInvitationsQueryHandler(IApplicationDbContext dbContext, TimeProvider? timeProvider = null)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<IReadOnlyList<ActiveInvitationDto>> Handle(ListActiveInvitationsQuery request, CancellationToken cancellationToken)
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var today = DateOnly.FromDateTime(utcNow);

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

        var rows = await (
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
            select new
            {
                invitation.Id,
                invitation.Email,
                invitation.TargetRole,
                invitation.InstitutionId,
                InstitutionName = institution != null ? institution.Name : null,
                invitation.CollegeId,
                CollegeName = college != null ? college.Name : null,
                invitation.SpecialityId,
                SpecialityName = speciality != null ? speciality.Name : null,
                invitation.SubSpecialityId,
                SubSpecialityName = subSpeciality != null ? subSpeciality.Name : null,
                invitation.IssuedOn,
                invitation.ExpiresOn,
                invitation.SentOn,
                invitation.DeliveryFailedOn,
                invitation.DeliveryFailures
            })
            .ToListAsync(cancellationToken);

        // What became of each link's mail is the Domain's one rule (T283), which the resend asks too.
        return rows
            .Select(row =>
            {
                var delivery = Invitation.DeliveryOf(row.SentOn, row.DeliveryFailedOn, row.IssuedOn, utcNow);
                return new ActiveInvitationDto(
                    row.Id,
                    row.Email,
                    row.TargetRole,
                    row.InstitutionId,
                    row.InstitutionName,
                    row.CollegeId,
                    row.CollegeName,
                    row.SpecialityId,
                    row.SpecialityName,
                    row.SubSpecialityId,
                    row.SubSpecialityName,
                    row.IssuedOn,
                    row.ExpiresOn,
                    delivery,
                    row.DeliveryFailures,
                    Invitation.SuggestsCheckingAddress(delivery, row.DeliveryFailures));
            })
            .ToList();
    }
}
