using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Users;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Trainees;

/// <summary>
/// One row of Trainees' Completed and closed (T379, flow 13, S11, F7; boards C01–C07): the person, the curriculum, the
/// programme's dates over the training year it ended in, and how it ended.
/// </summary>
/// <param name="Name">"Lerato Molefe", or an erased person's pseudonym (<see cref="IsErased" />).</param>
/// <param name="Email">The account's address, which tells two people with one name apart (C05); empty for an erased person.</param>
/// <param name="Ended">How and when the programme ended (<c>EndedProgrammes.Of</c>); null for an end recorded before T209
/// kept the day, which the list says as "Withdrawn" with no day.</param>
/// <param name="InstitutionName">The profile's institution, which the Administrator's list shows under each name (F7, C06).</param>
public sealed record EndedProfileRowDto(
    int ProfileId,
    string UserId,
    string Name,
    string Email,
    string CurriculumName,
    string CurriculumVersion,
    string SubSpecialityName,
    DateOnly ProgrammeStartDate,
    EndedProgrammeDto? Ended,
    string? InstitutionName,
    bool IsErased);

/// <summary>A page of Completed and closed, with the count of every ended programme in the caller's scope (the pager's).</summary>
public sealed record EndedProfilesPageDto(IReadOnlyList<EndedProfileRowDto> Rows, int TotalCount, int Page, int PageSize);

/// <summary>
/// Trainees' Completed and closed (T379, flow 13, S11, Q8 as Q22 corrects it): every ended programme in the caller's scope,
/// the latest end first then the latest profile, a page at a time, paged in SQL. Until T379 the page filtered Current
/// trainees' read for the inactive profiles, unpaged and in Current's order, and an erased person's profile was dropped.
/// </summary>
/// <remarks>
/// An Institutional admin lists her own institution's; the Administrator every institution's, each row naming its
/// institution (F7). An erased person is listed under the pseudonym their profile now holds (T258), as the end it recorded
/// (a withdrawal on the erasure day, for one still running then). A Trainee-holding admin is refused before anything is
/// read (T278, T290; the page shows its note instead).
/// </remarks>
public sealed record ListEndedProfilesQuery(int Page, int PageSize, ClaimsPrincipal Principal) : IRequest<EndedProfilesPageDto>
{
    /// <summary>A page of 20 (Q22).</summary>
    public const int DefaultPageSize = 20;

    /// <summary>The pager's sizes (<c>PagerControls</c>); any other asked is the default.</summary>
    public static readonly IReadOnlyList<int> PageSizes = [10, 20, 50, 100];
}

public sealed class ListEndedProfilesQueryHandler : IRequestHandler<ListEndedProfilesQuery, EndedProfilesPageDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _userAdministrationService;

    public ListEndedProfilesQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService userAdministrationService)
    {
        _dbContext = dbContext;
        _userAdministrationService = userAdministrationService;
    }

    public async Task<EndedProfilesPageDto> Handle(ListEndedProfilesQuery request, CancellationToken cancellationToken)
    {
        TraineeAdministrationRules.DemandTraineeAdministration(request.Principal);

        var pageSize = ListEndedProfilesQuery.PageSizes.Contains(request.PageSize) ? request.PageSize : ListEndedProfilesQuery.DefaultPageSize;

        var query = _dbContext.Set<TraineeProfile>().AsNoTracking().Where(profile => !profile.IsActive);
        if (!request.Principal.IsAdministrator())
        {
            if (request.Principal.GetInstitutionId() is not { } institutionId)
            {
                return new EndedProfilesPageDto([], 0, 1, pageSize);
            }

            query = query.Where(profile => profile.InstitutionId == institutionId);
        }

        var total = await query.CountAsync(cancellationToken);
        var lastPage = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
        var page = Math.Clamp(request.Page, 1, lastPage);

        // The latest end first (an end with no day recorded, from before T209, last), then the latest profile, so a page's
        // rows are fixed whatever ties.
        var profiles = await query
            .Include(profile => profile.Curriculum)
                .ThenInclude(curriculum => curriculum.SubSpeciality)
            .OrderByDescending(profile => profile.CompletedOn ?? profile.DeactivatedOn ?? DateOnly.MinValue)
            .ThenByDescending(profile => profile.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // The page's people at once (no read per row); a profile no account holds is an erased person's.
        var contacts = await _userAdministrationService.GetContactsAsync(
            profiles.Select(profile => profile.UserId).Distinct(StringComparer.Ordinal).ToArray(), cancellationToken);

        IReadOnlyDictionary<int, string> institutionNames = new Dictionary<int, string>();
        if (request.Principal.IsAdministrator())
        {
            var institutionIds = profiles.Select(profile => profile.InstitutionId).Distinct().ToArray();
            institutionNames = await _dbContext.Set<Institution>()
                .AsNoTracking()
                .Where(institution => institutionIds.Contains(institution.Id))
                .ToDictionaryAsync(institution => institution.Id, institution => institution.Name, cancellationToken);
        }

        var rows = profiles
            .Select(profile =>
            {
                var contact = contacts.GetValueOrDefault(profile.UserId);
                return new EndedProfileRowDto(
                    profile.Id,
                    profile.UserId,
                    contact is null ? profile.UserId : $"{contact.FirstName} {contact.LastName}".Trim(),
                    contact?.Email ?? string.Empty,
                    profile.Curriculum.Name,
                    profile.Curriculum.Version,
                    profile.Curriculum.SubSpeciality.Name,
                    profile.ProgrammeStartDate,
                    EndedProgrammes.Of(profile),
                    institutionNames.GetValueOrDefault(profile.InstitutionId),
                    contact is null);
            })
            .ToList();

        return new EndedProfilesPageDto(rows, total, page, pageSize);
    }
}
