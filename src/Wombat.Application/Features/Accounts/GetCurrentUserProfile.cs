using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.Accounts;

/// <summary>
/// The signed-in user's own profile: whoever <paramref name="Principal" /> names, and nobody else. (T185)
/// </summary>
/// <remarks>
/// Until T185 this took a user id, which the profile page read off the principal, so the handler would answer for any
/// id it was handed. The id is now read here, from the caller, so there is no id to forge.
/// </remarks>
public sealed record GetCurrentUserProfileQuery(ClaimsPrincipal Principal) : IRequest<UserProfileDto>;

public sealed class GetCurrentUserProfileQueryValidator : AbstractValidator<GetCurrentUserProfileQuery>
{
    public GetCurrentUserProfileQueryValidator()
    {
        RuleFor(query => query.Principal).NotNull();
    }
}

public sealed class GetCurrentUserProfileQueryHandler : IRequestHandler<GetCurrentUserProfileQuery, UserProfileDto>
{
    private readonly IUserAdministrationService _userAdministrationService;
    private readonly IApplicationDbContext _dbContext;

    public GetCurrentUserProfileQueryHandler(IUserAdministrationService userAdministrationService, IApplicationDbContext dbContext)
    {
        _userAdministrationService = userAdministrationService;
        _dbContext = dbContext;
    }

    public async Task<UserProfileDto> Handle(GetCurrentUserProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = request.Principal.GetRequiredUserId();

        var user = await _userAdministrationService.GetByIdAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("The user profile could not be found.");

        // My account's Account and How you sign in cards (T339, flow 02): the institution by name, which an account with
        // none (the platform Administrator, a College admin) does not have (C4), and how the account signs in (T286).
        var institutionName = user.InstitutionId is { } institutionId
            ? await _dbContext.Set<Institution>()
                .AsNoTracking()
                .Where(institution => institution.Id == institutionId)
                .Select(institution => institution.Name)
                .SingleOrDefaultAsync(cancellationToken)
            : null;

        var signIns = await _userAdministrationService.GetSignInMethodsAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("The user profile could not be found.");

        return new UserProfileDto(user.UserId, user.Email, user.FirstName, user.LastName, user.Roles)
        {
            InstitutionName = institutionName,
            HasLocalPassword = signIns.HasLocalPassword,
            InstitutionalSignIns = signIns.InstitutionalSignIns
        };
    }
}
