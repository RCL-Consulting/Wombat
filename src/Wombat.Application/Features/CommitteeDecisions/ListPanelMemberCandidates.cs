using System.Security.Claims;
using MediatR;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;

namespace Wombat.Application.Features.CommitteeDecisions;

public sealed record PanelMemberCandidateDto(
    string UserId,
    string Email,
    string FirstName,
    string LastName,
    int? InstitutionId);

/// <summary>
/// The people a panel at <paramref name="InstitutionId" /> may seat: the panel form's picker. (T165)
/// </summary>
/// <param name="InstitutionId">
/// The panel's institution. Anyone but a global Administrator manages panels at their own institution only, so for them
/// it is their own whatever is asked; an Administrator names it, and is offered nobody until they have.
/// </param>
public sealed record ListPanelMemberCandidatesQuery(ClaimsPrincipal Principal, int? InstitutionId = null)
    : IRequest<IReadOnlyList<PanelMemberCandidateDto>>;

/// <remarks>
/// Lists by <see cref="PanelSeat" />, the rule panel create and update enforce, so the picker cannot offer someone the
/// save refuses. Before T165 it listed every CommitteeMember holder, deactivated accounts included, and an
/// Administrator was offered every institution's. Before T237 it listed a committee member who also holds Trainee, for
/// any seat, the Chair's included, where since T194 they cannot act.
/// </remarks>
public sealed class ListPanelMemberCandidatesQueryHandler : IRequestHandler<ListPanelMemberCandidatesQuery, IReadOnlyList<PanelMemberCandidateDto>>
{
    private readonly IUserAdministrationService _userAdministrationService;

    public ListPanelMemberCandidatesQueryHandler(IUserAdministrationService userAdministrationService)
    {
        _userAdministrationService = userAdministrationService;
    }

    public async Task<IReadOnlyList<PanelMemberCandidateDto>> Handle(ListPanelMemberCandidatesQuery request, CancellationToken cancellationToken)
    {
        var institutionId = request.Principal.IsAdministrator()
            ? request.InstitutionId
            : request.Principal.GetInstitutionId();

        if (institutionId is not int panelInstitutionId)
        {
            return Array.Empty<PanelMemberCandidateDto>();
        }

        var eligible = await PanelSeat.EligibleAsync(_userAdministrationService, panelInstitutionId, cancellationToken);

        return eligible.Values
            .Select(user => new PanelMemberCandidateDto(user.UserId, user.Email, user.FirstName, user.LastName, user.InstitutionId))
            .OrderBy(user => user.LastName)
            .ThenBy(user => user.FirstName)
            .ThenBy(user => user.UserId, StringComparer.Ordinal)
            .ToArray();
    }
}
