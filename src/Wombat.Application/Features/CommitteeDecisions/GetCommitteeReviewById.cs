using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Users;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <param name="Today">The day the agenda's "missed" is judged on; the programme's today when null. (T131)</param>
public sealed record GetCommitteeReviewByIdQuery(int ReviewId, ClaimsPrincipal Principal, DateOnly? Today = null)
    : IRequest<CommitteeReviewDetailDto>;

public sealed class GetCommitteeReviewByIdQueryHandler : IRequestHandler<GetCommitteeReviewByIdQuery, CommitteeReviewDetailDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;

    public GetCommitteeReviewByIdQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
    }

    public async Task<CommitteeReviewDetailDto> Handle(GetCommitteeReviewByIdQuery request, CancellationToken cancellationToken)
    {
        var review = await _dbContext.Set<CommitteeReview>()
            .AsNoTracking()
            .Include(entity => entity.Panel)
                .ThenInclude(panel => panel.Members)
            .Include(entity => entity.Decisions)
                .ThenInclude(decision => decision.Attendees)
            .Include(entity => entity.Appeals)
            .Include(entity => entity.EvidenceItems)
            .Include(entity => entity.AgendaLines)
            .SingleOrDefaultAsync(entity => entity.Id == request.ReviewId, cancellationToken);

        // The ladder this handler used to spell out inline now lives beside the other committee
        // guards, because the two sibling queries on the same page have to climb the identical one.
        // The panel carries its own institution regardless of scope; the discipline is national
        // now (T091), so no further lookup is needed to place a review. (T101 finding E)
        // One refusal for an unknown review and one out of reach, before anything about it is said (T194 item 1).
        review = CommitteeDecisionAuthorization.DemandReviewAccess(request.Principal, review);

        // T142. The trainee by name, looked up only once the caller has passed the review ladder above. T165 adds the
        // panel's members and those recorded as present at each decision, in the same one lookup.
        var detail = review.ToDetailDto();
        var names = await UserDisplayNames.ResolveAsync(
            _users,
            detail.PanelMembers
                .Concat(detail.Decisions.SelectMany(decision => decision.Attendees))
                .Select(person => person.UserId)
                .Prepend(review.TraineeUserId),
            cancellationToken);

        // Who may sit on the panel now (PanelSeat), read once and only where the page uses it: the present list while the
        // review can still take a decision, the appeal body's note under appeal, and whether a caller seated on the appeal
        // body may act from the seat, which resolving demands (T237).
        var holdsAppealSeat = CommitteeDecisionAuthorization.ResolvesAppeals(request.Principal, review.Panel);
        var eligible = MayStillDecide(review) || review.State == CommitteeReviewState.UnderAppeal || holdsAppealSeat
            ? await PanelSeat.EligibleAsync(_users, review.Panel.InstitutionId, cancellationToken)
            : null;
        var seated = Seated(review, request.Principal, eligible);
        var callerUserId = request.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // T213 review. Starting the review and every chair's action also demand that its trainee still trains at the
        // panel's institution, so the page is told when that fails for this caller, by the same predicate, and offers none
        // of them. Asked only in the states where one of them is open.
        var actionOpen = review.State is CommitteeReviewState.Scheduled
            or CommitteeReviewState.InProgress
            or CommitteeReviewState.Decided;
        var traineeElsewhere =
            actionOpen && !await CommitteeTraineeScope.MayActOnTraineeAsync(_dbContext, request.Principal, review, cancellationToken)
                ? CommitteeTraineeScope.TraineeNotAtPanelInstitution
                : null;

        // T131 slice 4. The agenda, read by the reader GetCommitteeAgendaQuery shares, past the same ladder.
        var agenda = await CommitteeAgendaReader.ReadAsync(
            _dbContext, review, request.Today ?? ProgrammeCalendar.DateOf(DateTime.UtcNow), cancellationToken);

        return detail with
        {
            TraineeName = names.NameOf(review.TraineeUserId),
            Agenda = agenda,
            // T213: what the caller may do here, by the predicates the handlers demand, so the page offers each control to
            // exactly the people its handler lets use it.
            CallerChairs = CommitteeDecisionAuthorization.Chairs(request.Principal, review.Panel),
            CallerMayStart = CommitteeDecisionAuthorization.WorksOnPanel(request.Principal, review.Panel),
            // The seat, and acting from it now: the two checks the resolve handler demands, in its order (T237).
            CallerResolvesAppeals = holdsAppealSeat &&
                                    eligible is not null &&
                                    PanelSeat.AppealBodyAt(review, eligible).Any(member =>
                                        string.Equals(member.UserId, callerUserId, StringComparison.Ordinal)),
            TraineeElsewhere = traineeElsewhere,
            AppealBody = AppealBodyOf(review, eligible)
                .Select(person => person with { Name = names.NameOf(person.UserId) })
                .ToArray(),
            PanelMembers = detail.PanelMembers
                .Select(person => person with
                {
                    Name = names.NameOf(person.UserId),
                    MaySit = seated.Contains(person.UserId)
                })
                .ToArray(),
            Decisions = detail.Decisions
                .Select(decision => decision with
                {
                    Attendees = decision.Attendees.Select(person => person with { Name = names.NameOf(person.UserId) }).ToArray()
                })
                .ToArray()
        };
    }

    /// <summary>
    /// Whether the review can still take a decision: a summative review not yet decided, or under appeal, whose remit
    /// records a replacement.
    /// </summary>
    private static bool MayStillDecide(CommitteeReview review)
        => !review.IsFormative &&
           review.State is CommitteeReviewState.Scheduled
               or CommitteeReviewState.InProgress
               or CommitteeReviewState.UnderAppeal;

    /// <summary>
    /// The panel members who may be recorded as present at this review now (<see cref="PanelSeat" />, T165): the ones
    /// the record-decision and remit forms offer, by the rule recording enforces. Asked only while the review can still
    /// take a decision, a summative review not yet decided or under appeal, and never for the trainee whose review it
    /// is: who may sit on their panel is the panel's business.
    /// </summary>
    private static IReadOnlySet<string> Seated(
        CommitteeReview review,
        System.Security.Claims.ClaimsPrincipal principal,
        IReadOnlyDictionary<string, Common.Interfaces.UserIdentityDetails>? eligible)
    {
        var askedByTheTrainee = string.Equals(
            principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            review.TraineeUserId,
            StringComparison.Ordinal);

        if (eligible is null || !MayStillDecide(review) || askedByTheTrainee)
        {
            return new HashSet<string>(StringComparer.Ordinal);
        }

        return PanelSeat.SittingAt(review, eligible)
            .Select(member => member.UserId)
            .ToHashSet(StringComparer.Ordinal);
    }

    /// <summary>
    /// Who can resolve the appeal now (<see cref="CommitteeReviewDetailDto.AppealBody" />, T237): the appeal body that can
    /// act (<see cref="PanelSeat.AppealBodyAt" />), the one list the resolve handler also demands the caller is on. Only
    /// under appeal, and for every reader: the note it feeds tells the trainee too who resolves their appeal, and must name
    /// nobody who cannot.
    /// </summary>
    private static IReadOnlyList<CommitteePersonDto> AppealBodyOf(
        CommitteeReview review,
        IReadOnlyDictionary<string, Common.Interfaces.UserIdentityDetails>? eligible)
        => eligible is null || review.State != CommitteeReviewState.UnderAppeal
            ? []
            : PanelSeat.AppealBodyAt(review, eligible)
                .Select(member => new CommitteePersonDto(member.UserId, member.Role))
                .ToArray();
}
