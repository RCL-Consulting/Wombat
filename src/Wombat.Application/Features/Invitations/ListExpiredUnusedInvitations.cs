using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Users;
using Wombat.Domain.Curricula;
using Wombat.Domain.Institutions;
using Wombat.Domain.Invitations;

namespace Wombat.Application.Features.Invitations;

/// <summary>
/// The invitations list's "Expired, not used" (T373, flow 11, Q9): invitations that passed their last day unused, newest
/// first, each with Invite again. In the caller's scope as the active list is.
/// </summary>
public sealed record ListExpiredUnusedInvitationsQuery(ClaimsPrincipal Principal) : IRequest<IReadOnlyList<ExpiredInvitationDto>>;

public sealed class ListExpiredUnusedInvitationsQueryHandler
    : IRequestHandler<ListExpiredUnusedInvitationsQuery, IReadOnlyList<ExpiredInvitationDto>>
{
    /// <summary>
    /// How long an expired invitation stays listed: it leaves on the 30th South African day after its last day (L14: a last
    /// day of 2026-11-02 is listed on dates before 2026-12-02).
    /// </summary>
    public const int ListedForDays = 30;

    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;
    private readonly TimeProvider _timeProvider;

    public ListExpiredUnusedInvitationsQueryHandler(
        IApplicationDbContext dbContext,
        IUserAdministrationService users,
        TimeProvider? timeProvider = null)
    {
        _dbContext = dbContext;
        _users = users;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <remarks>
    /// An invitation leaves the list before its 30 days once nothing is left to do about it (S4, the address compared
    /// trimmed and whatever its case): a newer invitation has gone to its address, at any institution or College, in any
    /// state (Invite again was pressed, or someone invited them anew), or an account now holds the address (they registered
    /// from another invitation, or an institution's sign-in made one). The days are counted on the South African calendar
    /// (<see cref="ProgrammeCalendar.DateOf" />), as the last day is (F7).
    /// </remarks>
    public async Task<IReadOnlyList<ExpiredInvitationDto>> Handle(
        ListExpiredUnusedInvitationsQuery request,
        CancellationToken cancellationToken)
    {
        UserAdministrationRules.DemandUserAdministration(request.Principal);

        var today = ProgrammeCalendar.DateOf(_timeProvider.GetUtcNow().UtcDateTime);
        var listedFrom = today.AddDays(-ListedForDays);

        var invitations = _dbContext.Set<Invitation>().AsNoTracking();
        if (!request.Principal.IsAdministrator())
        {
            if (request.Principal.GetInstitutionId() is not int scopedInstitutionId)
            {
                return Array.Empty<ExpiredInvitationDto>();
            }

            invitations = invitations.Where(entity => entity.InstitutionId == scopedInstitutionId);
        }

        var rows = await (
            from invitation in invitations
            join institution in _dbContext.Set<Institution>() on invitation.InstitutionId equals institution.Id into institutionGroup
            from institution in institutionGroup.DefaultIfEmpty()
            join college in _dbContext.Set<College>() on invitation.CollegeId equals college.Id into collegeGroup
            from college in collegeGroup.DefaultIfEmpty()
            join speciality in _dbContext.Set<Speciality>() on invitation.SpecialityId equals speciality.Id into specialityGroup
            from speciality in specialityGroup.DefaultIfEmpty()
            join subSpeciality in _dbContext.Set<SubSpeciality>() on invitation.SubSpecialityId equals subSpeciality.Id into subSpecialityGroup
            from subSpeciality in subSpecialityGroup.DefaultIfEmpty()
            where invitation.RevokedOn == null &&
                  invitation.UsedOn == null &&
                  invitation.ExpiresOn < today &&
                  invitation.ExpiresOn > listedFrom
            select new ExpiredInvitationDto(
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

        if (rows.Count == 0)
        {
            return rows;
        }

        // A newer invitation to the same address, anywhere: unfiltered, as Q10's check at issue is.
        var addresses = rows.Select(row => Invitation.NormaliseAddress(row.Email)).Distinct(StringComparer.Ordinal).ToArray();
        var toTheseAddresses = await _dbContext.Set<Invitation>()
            .AsNoTracking()
            .Where(entity => addresses.Contains(entity.Email.ToLower()))
            .Select(entity => new { entity.Id, entity.Email })
            .ToListAsync(cancellationToken);
        var newestByAddress = toTheseAddresses
            .GroupBy(entity => Invitation.NormaliseAddress(entity.Email), StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Max(entity => entity.Id), StringComparer.Ordinal);

        var held = new HashSet<string>(StringComparer.Ordinal);
        foreach (var address in addresses)
        {
            if (await _users.FindByEmailAsync(address, cancellationToken) is not null)
            {
                held.Add(address);
            }
        }

        return rows
            .Where(row =>
            {
                var address = Invitation.NormaliseAddress(row.Email);
                // The row's own invitation is missing from the second read when the database's lower() and
                // NormaliseAddress disagree on a character of the address (T373 fix pass, B4): it is then its own newest,
                // never a fault that takes the page with it.
                return !held.Contains(address) && newestByAddress.GetValueOrDefault(address, row.Id) == row.Id;
            })
            .OrderByDescending(row => row.ExpiresOn)
            .ThenByDescending(row => row.IssuedOn)
            .ThenByDescending(row => row.Id)
            .ToList();
    }
}
