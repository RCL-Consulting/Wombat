using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Users;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Assessors;

/// <summary>One person's assessor training record, as the user page's Assessor training section reads it out (T04, T07).</summary>
public sealed record AssessorTrainingDto(
    int ProfileId,
    AssessorTrainingStatus TrainingStatus,
    DateOnly? TrainingCompletedOn,
    string Qualifications);

/// <summary>
/// The training record of one person (T376, flow 12, Q1, Q2, M1): what the user page's Assessor training section shows and
/// fills its form from. Null when none is kept, and for a person the caller may not see, so an id outside the caller's
/// scope reads as one with no record (the page itself reads it as not found). Read whether or not the person holds
/// Assessor now: the section decides what to show (Q18: a removed Assessor's record is kept, hidden).
/// </summary>
public sealed record GetAssessorTrainingQuery(string UserId, ClaimsPrincipal Principal) : IRequest<AssessorTrainingDto?>;

public sealed class GetAssessorTrainingQueryHandler : IRequestHandler<GetAssessorTrainingQuery, AssessorTrainingDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _userAdministrationService;

    public GetAssessorTrainingQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService userAdministrationService)
    {
        _dbContext = dbContext;
        _userAdministrationService = userAdministrationService;
    }

    public async Task<AssessorTrainingDto?> Handle(GetAssessorTrainingQuery request, CancellationToken cancellationToken)
    {
        UserAdministrationRules.DemandUserAdministration(request.Principal);

        if (!await AssessorScope.MaySeeAsync(_userAdministrationService, request.Principal, request.UserId, cancellationToken))
        {
            return null;
        }

        return await _dbContext.Set<AssessorProfile>()
            .AsNoTracking()
            .Where(profile => profile.UserId == request.UserId)
            .Select(profile => new AssessorTrainingDto(
                profile.Id, profile.TrainingStatus, profile.TrainingCompletedOn, profile.Qualifications))
            .SingleOrDefaultAsync(cancellationToken);
    }
}

/// <summary>
/// The person whose training record a profile id names (T376, flow 12, S10): what the old
/// <c>/admin/assessors/edit?id=</c> route follows to <c>/admin/users/{UserId}?section=training</c>. Null for an id that
/// names no profile, and for one whose person the caller may not see, alike (T194's rule: the page reads both as "User
/// unavailable").
/// </summary>
public sealed record GetAssessorProfileOwnerQuery(int ProfileId, ClaimsPrincipal Principal) : IRequest<string?>;

public sealed class GetAssessorProfileOwnerQueryHandler : IRequestHandler<GetAssessorProfileOwnerQuery, string?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _userAdministrationService;

    public GetAssessorProfileOwnerQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService userAdministrationService)
    {
        _dbContext = dbContext;
        _userAdministrationService = userAdministrationService;
    }

    public async Task<string?> Handle(GetAssessorProfileOwnerQuery request, CancellationToken cancellationToken)
    {
        UserAdministrationRules.DemandUserAdministration(request.Principal);

        var userId = await _dbContext.Set<AssessorProfile>()
            .AsNoTracking()
            .Where(profile => profile.Id == request.ProfileId)
            .Select(profile => profile.UserId)
            .SingleOrDefaultAsync(cancellationToken);

        return userId is not null
            && await AssessorScope.MaySeeAsync(_userAdministrationService, request.Principal, userId, cancellationToken)
                ? userId
                : null;
    }
}

/// <summary>
/// Whether the caller may see a person, by the person's own institution, as the user page judges it (T376): the
/// Administrator anyone who exists; an Institutional admin someone at her institution. The profile's own stamped
/// institution is never the test (F4: it follows the person at the next save).
/// </summary>
internal static class AssessorScope
{
    public static async Task<bool> MaySeeAsync(
        IUserAdministrationService users,
        ClaimsPrincipal principal,
        string userId,
        CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return false;
        }

        return principal.IsAdministrator()
            || (user.InstitutionId is int institutionId && principal.CanAccessInstitution(institutionId));
    }
}
