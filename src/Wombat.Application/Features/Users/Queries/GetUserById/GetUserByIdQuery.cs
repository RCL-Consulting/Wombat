using System.Security.Claims;
using MediatR;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;

namespace Wombat.Application.Features.Users.Queries.GetUserById;

public sealed record GetUserByIdQuery(string UserId, ClaimsPrincipal Principal) : IRequest<UserDetailDto?>;

public sealed class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDetailDto?>
{
    private readonly IUserAdministrationService _userAdministrationService;
    private readonly IApplicationDbContext _dbContext;

    public GetUserByIdQueryHandler(IUserAdministrationService userAdministrationService, IApplicationDbContext dbContext)
    {
        _userAdministrationService = userAdministrationService;
        _dbContext = dbContext;
    }

    /// <remarks>
    /// <para>
    /// Someone who holds Trainee opens nobody, themselves included, whatever other role they hold, and is refused before
    /// the id is looked up (T278, <see cref="UserAdministrationRules.DemandUserAdministration" />), so the refusal says
    /// nothing about the id. The user page asks the same rule first and sends nothing.
    /// </para>
    /// <para>
    /// What the page of sections reads (T376, flow 12, "DATA the build must add"): the scope's names, once, through
    /// <see cref="PersonScopeNames" /> (S1); for a person who holds no role, their latest ended programme (S2); and how the
    /// account signs in (F3, Q10), which the directory reads. Q9: no invitations are read.
    /// </para>
    /// </remarks>
    public async Task<UserDetailDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        UserAdministrationRules.DemandUserAdministration(request.Principal);

        var user = await _userAdministrationService.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        // Scope guard: out-of-scope users are reported as 404, not 403, to avoid leaking id existence.
        // Unscoped (global) accounts such as Administrators are visible only to a global Administrator,
        // never to a tenant-level InstitutionalAdmin.
        if (!request.Principal.IsAdministrator()
            && (!user.InstitutionId.HasValue || !request.Principal.CanAccessInstitution(user.InstitutionId.Value)))
        {
            return null;
        }

        var names = await PersonScopeNames.LoadAsync(_dbContext, [user], cancellationToken);

        EndedProgrammeDto? ended = null;
        if (user.Roles.Count == 0)
        {
            ended = (await EndedProgrammes.LatestForAsync(_dbContext, [user.UserId], cancellationToken))
                .GetValueOrDefault(user.UserId);
        }

        // An account deleted between the two reads signs in by nothing: the page then says so as for a password-only one.
        var signIn = await _userAdministrationService.GetSignInMethodsAsync(user.UserId, cancellationToken)
                     ?? new AccountSignInMethods(false, [], true);

        return new UserDetailDto(
            user.UserId,
            user.Email,
            user.FirstName,
            user.LastName,
            user.InstitutionId,
            names.InstitutionName(user),
            user.SpecialityIds.ToArray(),
            user.SubSpecialityIds.ToArray(),
            user.Roles.ToArray(),
            // The administrative lock, never a brute-force lockout (build-contracts § 1).
            user.IsDeactivated)
        {
            Scope = names.Scope(user),
            SpecialityNames = names.SpecialityNames(user),
            SubSpecialityNames = names.SubSpecialityNames(user),
            CollegeId = user.CollegeId,
            CollegeName = names.CollegeName(user),
            EndedProgramme = ended,
            AllowsLocalPassword = signIn.AllowsLocalPassword,
            HasLocalPassword = signIn.HasLocalPassword,
            InstitutionalSignIns = signIn.InstitutionalSignIns
        };
    }
}
