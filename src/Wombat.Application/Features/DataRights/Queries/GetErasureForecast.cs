using System.Globalization;
using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.DataRights;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.DataRights.Queries;

/// <summary>
/// What approving an Erasure would end and keep (T387, Q3, F5, M2, S5), for the dialog Approve and erase… opens. Gated as
/// <see cref="Commands.ApproveDataRightsRequestCommand" /> is: null when the caller may not decide the request, or it is
/// not a submitted Erasure. The handler is lane D's (build-contracts § 2).
/// </summary>
public sealed record GetErasureForecastQuery(
    Guid RequestId,
    ClaimsPrincipal Principal) : IRequest<ErasureForecastDto?>;

public sealed class GetErasureForecastQueryValidator : AbstractValidator<GetErasureForecastQuery>
{
    public GetErasureForecastQueryValidator()
    {
        RuleFor(query => query.RequestId).NotEmpty();
        RuleFor(query => query.Principal).NotNull();
    }
}

/// <remarks>
/// <para>
/// Read with the rules <c>ErasureExecutor</c> ends by, so the dialog says what the erasure does: every open committee
/// review of the person (<see cref="CommitteeReview.OpenStates" />), at any institution, is withdrawn; every campaign about
/// them not Released or Withdrawn is withdrawn; their other open requests (Submitted, or an approved Rectification) are
/// withdrawn; their active profile ends. The settled reviews (Ratified, UnderAppeal, Final) are kept.
/// </para>
/// <para>
/// F5: a review is named where the caller may see it, at the caller's institution (its panel's), and every one for the
/// Administrator; an open review elsewhere is counted, never named, so a Coordinator learns no other institution's panel
/// or review. A kept review elsewhere is left out: nothing about it changes. The campaigns are named everywhere (the
/// operator's approval, 2026-10-30): a campaign is about the person at their institution.
/// </para>
/// <para>
/// Nothing here is stored or staged: a query, read untracked.
/// </para>
/// </remarks>
public sealed class GetErasureForecastQueryHandler : IRequestHandler<GetErasureForecastQuery, ErasureForecastDto?>
{
    /// <summary>The settled states an erasure keeps under the pseudonym (T258): a review ratified, under appeal, or final.</summary>
    private static readonly CommitteeReviewState[] KeptStates =
        [CommitteeReviewState.Ratified, CommitteeReviewState.UnderAppeal, CommitteeReviewState.Final];

    /// <summary>The roles that are not a staff role: the dialog's "The roles ‹roles› and their scopes" leaves them out.</summary>
    private static readonly string[] NotStaffRoles = [WombatRoles.Trainee, WombatRoles.PendingTrainee];

    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;

    public GetErasureForecastQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
    }

    public async Task<ErasureForecastDto?> Handle(GetErasureForecastQuery request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Set<DataRightsRequest>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.RequestId, cancellationToken);

        // Gated as the command: a caller who may not decide it, the requester included, reads nothing, and an unknown id
        // reads as one out of scope (S14). So does a request that is not a submitted Erasure: there is nothing to confirm.
        if (entity is null ||
            !DataRightsAuthorization.CanReview(request.Principal, entity) ||
            entity.Type != DataRightsRequestType.Erasure ||
            entity.Status != DataRightsRequestStatus.Submitted)
        {
            return null;
        }

        var personId = entity.RequesterUserId;
        var namesEveryInstitution = request.Principal.IsAdministrator();
        var callerInstitutionId = request.Principal.GetInstitutionId();

        // F1: the live name; the stored one only for an account that no longer answers.
        var contacts = await _users.GetContactsAsync([personId], cancellationToken);
        var personName = contacts.TryGetValue(personId, out var contact) &&
                         !string.IsNullOrWhiteSpace($"{contact.FirstName} {contact.LastName}".Trim())
            ? $"{contact.FirstName} {contact.LastName}".Trim()
            : entity.RequesterDisplayName;

        var account = await _users.GetByIdAsync(personId, cancellationToken);
        var staffRoleKeys = account is null
            ? []
            : WombatRoles.All
                .Where(role => !NotStaffRoles.Contains(role) && account.Roles.Contains(role, StringComparer.Ordinal))
                .ToArray();

        var reviews = await _dbContext.Set<CommitteeReview>()
            .AsNoTracking()
            .Where(review => review.TraineeUserId == personId)
            .OrderBy(review => review.Id)
            .Select(review => new
            {
                review.Id,
                review.ReviewType,
                review.IsFormative,
                review.State,
                PanelName = review.Panel.Name,
                PanelInstitutionId = review.Panel.InstitutionId
            })
            .ToListAsync(cancellationToken);

        bool MayName(int panelInstitutionId) => namesEveryInstitution || panelInstitutionId == callerInstitutionId;

        var open = reviews.Where(review => CommitteeReview.OpenStates.Contains(review.State)).ToList();
        var openNamed = open
            .Where(review => MayName(review.PanelInstitutionId))
            .Select(review => new ErasureForecastReview(
                review.Id,
                CommitteeDecisionWording.ReviewTypeLabel(review.ReviewType, review.IsFormative),
                review.PanelName,
                review.State))
            .ToArray();
        var openElsewhere = open.Count - openNamed.Length;

        var kept = reviews
            .Where(review => KeptStates.Contains(review.State) && MayName(review.PanelInstitutionId))
            .Select(review => new ErasureForecastReview(
                review.Id,
                CommitteeDecisionWording.ReviewTypeLabel(review.ReviewType, review.IsFormative),
                review.PanelName,
                review.State))
            .ToArray();

        // The states the executor withdraws: every one but Released and Withdrawn.
        var campaigns = await _dbContext.Set<MsfCampaign>()
            .AsNoTracking()
            .Where(campaign => campaign.SubjectUserId == personId &&
                               campaign.State != MsfCampaignState.Released &&
                               campaign.State != MsfCampaignState.Withdrawn)
            .OrderBy(campaign => campaign.Id)
            .Select(campaign => new { campaign.Id, TemplateName = campaign.Template.Name, campaign.ClosesOn, campaign.State })
            .ToListAsync(cancellationToken);

        var otherRequests = await _dbContext.Set<DataRightsRequest>()
            .AsNoTracking()
            .Where(other => other.RequesterUserId == personId &&
                            other.Id != entity.Id &&
                            (other.Status == DataRightsRequestStatus.Submitted ||
                             other.Status == DataRightsRequestStatus.Approved))
            .OrderBy(other => other.RequestedOn)
            .Select(other => new ErasureForecastRequest(other.Id, other.Type, other.RequestedOn))
            .ToListAsync(cancellationToken);

        var programme = await _dbContext.Set<TraineeProfile>()
            .AsNoTracking()
            .Where(profile => profile.UserId == personId && profile.IsActive)
            .OrderBy(profile => profile.Id)
            .Select(profile => new { profile.Curriculum.Name, profile.Curriculum.Version })
            .FirstOrDefaultAsync(cancellationToken);

        return new ErasureForecastDto(
            personName,
            staffRoleKeys,
            openNamed,
            openElsewhere,
            campaigns
                .Select(campaign => new ErasureForecastCampaign(
                    campaign.Id, CampaignName(campaign.TemplateName, campaign.ClosesOn), campaign.State))
                .ToArray(),
            otherRequests,
            programme is null ? null : $"{programme.Name} ({programme.Version})",
            kept);
    }

    /// <summary>
    /// A campaign about the person as the MSF pages tell it from the others, without the subject the dialog already names:
    /// "Annual MSF (closing 2029-03-21)", the parenthesis of flow 10's <c>MsfCampaignText.Describe</c>.
    /// </summary>
    public static string CampaignName(string templateName, DateOnly closesOn)
        => $"{templateName} (closing {closesOn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)})";
}
