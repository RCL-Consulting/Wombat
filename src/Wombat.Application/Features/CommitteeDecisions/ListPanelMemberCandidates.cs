using System.Security.Claims;
using MediatR;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.CommitteeDecisions;

public sealed record PanelMemberCandidateDto(
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    int? InstitutionId);

public sealed record ListPanelMemberCandidatesQuery(ClaimsPrincipal Principal) : IRequest<IReadOnlyList<PanelMemberCandidateDto>>;

public sealed class ListPanelMemberCandidatesQueryHandler : IRequestHandler<ListPanelMemberCandidatesQuery, IReadOnlyList<PanelMemberCandidateDto>>
{
    private readonly IUserAdministrationService _userAdministrationService;

    public ListPanelMemberCandidatesQueryHandler(IUserAdministrationService userAdministrationService)
    {
        _userAdministrationService = userAdministrationService;
    }

    public async Task<IReadOnlyList<PanelMemberCandidateDto>> Handle(ListPanelMemberCandidatesQuery request, CancellationToken cancellationToken)
    {
        var users = await _userAdministrationService.ListUsersInRoleAsync(WombatRoles.CommitteeMember, cancellationToken);
        var query = users.AsEnumerable();

        if (!request.Principal.IsAdministrator())
        {
            var scopedInstitutionId = request.Principal.GetInstitutionId();
            if (!scopedInstitutionId.HasValue)
            {
                return Array.Empty<PanelMemberCandidateDto>();
            }
            query = query.Where(user => user.InstitutionId == scopedInstitutionId.Value);
        }

        return query
            .Select(user => new PanelMemberCandidateDto(user.UserId, user.Email, user.FirstName, user.LastName, user.InstitutionId))
            .OrderBy(user => user.LastName)
            .ThenBy(user => user.FirstName)
            .ToArray();
    }
}
