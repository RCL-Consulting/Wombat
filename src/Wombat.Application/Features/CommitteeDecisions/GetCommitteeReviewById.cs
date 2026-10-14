using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Common.Users;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;

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
        var review = await LoadAsync(_dbContext, request.ReviewId, cancellationToken);

        // The ladder this handler used to spell out inline now lives beside the other committee
        // guards, because the two sibling queries on the same page have to climb the identical one.
        // The panel carries its own institution regardless of scope; the discipline is national
        // now (T091), so no further lookup is needed to place a review. (T101 finding E) Only a speciality
        // administrator's rung also reads where the trainee trains (T218).
        // One refusal for an unknown review and one out of reach, before anything about it is said (T194 item 1).
        review = await CommitteeDecisionAuthorization.DemandReviewAccessAsync(
            _dbContext, _users, request.Principal, review, cancellationToken);

        return await DescribeAsync(_dbContext, _users, review, request.Principal, request.Today, forItsTrainee: false, cancellationToken);
    }

    /// <summary>
    /// The review with everything the review pages read about it (T366: shared with <see cref="GetMyCommitteeReviewQuery" />,
    /// the trainee's own page, so the two cannot drift). Null when no review has the id. Read only.
    /// </summary>
    internal static Task<CommitteeReview?> LoadAsync(IApplicationDbContext dbContext, int reviewId, CancellationToken cancellationToken)
        => dbContext.Set<CommitteeReview>()
            .AsNoTracking()
            .Include(entity => entity.Panel)
                .ThenInclude(panel => panel.Members)
            .Include(entity => entity.Decisions)
                .ThenInclude(decision => decision.Attendees)
            .Include(entity => entity.Appeals)
            .Include(entity => entity.EvidenceItems)
            .Include(entity => entity.AgendaLines)
            .SingleOrDefaultAsync(entity => entity.Id == reviewId, cancellationToken);

    /// <summary>
    /// The review as a page reads it, for a caller who has already passed its own read check: the committee's ladder
    /// (<see cref="CommitteeDecisionAuthorization.DemandReviewAccessAsync" />), or the trainee's own-review check
    /// (<see cref="GetMyCommitteeReviewQueryHandler" />, T366, F8).
    /// </summary>
    /// <param name="forItsTrainee">
    /// The read is the trainee's own page (T366): the appeal body is read while the review is still Ratified too, for the
    /// lodge block's "What happens next", and the panel's institution is named (round 2's data rows).
    /// </param>
    internal static async Task<CommitteeReviewDetailDto> DescribeAsync(
        IApplicationDbContext dbContext,
        IUserAdministrationService users,
        CommitteeReview review,
        ClaimsPrincipal principal,
        DateOnly? today,
        bool forItsTrainee,
        CancellationToken cancellationToken)
    {
        // T142. The trainee by name, looked up only once the caller has passed the review ladder above. T165 adds the
        // panel's members and those recorded as present at each decision, in the same one lookup.
        var detail = review.ToDetailDto();
        var names = await UserDisplayNames.ResolveAsync(
            users,
            detail.PanelMembers
                .Concat(detail.Decisions.SelectMany(decision => decision.Attendees))
                .Select(person => person.UserId)
                // T361 (round 3 items 23, 30): the snapshot names each line's assessor, in the same one lookup.
                .Concat(detail.EvidenceItems.Select(item => item.AssessorUserId).OfType<string>())
                // T366 (T308): who lodged and who resolved each appeal, in the same one lookup.
                .Concat(detail.Appeals.Select(appeal => appeal.LodgedByUserId))
                .Concat(detail.Appeals.Select(appeal => appeal.ResolvedByUserId).OfType<string>())
                .Prepend(review.TraineeUserId),
            cancellationToken);

        // T213 review. Starting the review and every chair's action also demand that its trainee still trains at the
        // panel's institution, so the page is told when that fails for this caller, by the same predicate, and offers none
        // of them. Asked only in the states where one of them is open.
        var actionOpen = review.State is CommitteeReviewState.Scheduled
            or CommitteeReviewState.InProgress
            or CommitteeReviewState.Decided;

        // Who may sit on the panel now (PanelSeat), read once and only where the page uses it: the present list while the
        // review can still take a decision, the appeal body's note under appeal, whether a caller seated on the appeal
        // body may act from the seat, which resolving demands (T237), and whether the panel's chair may take the chair's
        // actions, which each of them demands (T256), in every state one of them is open or about to be; and whether a
        // member may start it, in the one state Start takes (T279).
        var holdsAppealSeat = CommitteeDecisionAuthorization.ResolvesAppeals(principal, review.Panel);
        var appealBodyRead = AppealBodyRead(review, forItsTrainee);
        var eligible = MayStillDecide(review) || appealBodyRead || holdsAppealSeat || actionOpen
            ? await PanelSeat.EligibleAsync(users, review.Panel.InstitutionId, cancellationToken)
            : null;
        var seated = Seated(review, principal, eligible);
        var callerUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var traineeElsewhere =
            actionOpen && !await CommitteeTraineeScope.MayActOnTraineeAsync(dbContext, principal, review, cancellationToken)
                ? CommitteeTraineeScope.TraineeNotAtPanelInstitution
                : null;

        // T131 slice 4. The agenda, read by the reader GetCommitteeAgendaQuery shares, past the same ladder.
        var agenda = await CommitteeAgendaReader.ReadAsync(
            dbContext, review, today ?? ProgrammeCalendar.DateOf(DateTime.UtcNow), cancellationToken);

        // T361 (round 2 review 10, E6): the registrar page's id and the training year the page's subtitle names.
        var (traineeProfileId, trainingYear) = await TraineeOfAsync(dbContext, review, cancellationToken);

        // T366 (round 2's data row): the panel's institution by name, whose staff read an appeal's reason. Her page only.
        var institutionName = forItsTrainee
            ? await dbContext.Set<Institution>()
                .AsNoTracking()
                .Where(institution => institution.Id == review.Panel.InstitutionId)
                .Select(institution => institution.Name)
                .SingleOrDefaultAsync(cancellationToken)
            : null;

        return detail with
        {
            TraineeName = names.NameOf(review.TraineeUserId),
            Agenda = agenda,
            // T213: what the caller may do here, by the predicates the handlers demand, so the page offers each control to
            // exactly the people its handler lets use it. The chair's: the seat, and sitting at the review now (T256).
            CallerChairs = eligible is not null && CommitteeDecisionAuthorization.Chairs(principal, review, eligible),
            ChairCannotAct = actionOpen && eligible is not null
                ? ChairCannotActOf(review, principal, eligible, names)
                : null,
            // Start's own predicate, and only in the one state it takes: a member of the panel only while they may sit (T279).
            CallerMayStart = review.State == CommitteeReviewState.Scheduled &&
                             eligible is not null &&
                             CommitteeDecisionAuthorization.WorksOnReview(principal, review, eligible),
            // The seat, and acting from it now: the two checks the resolve handler demands, in its order (T237).
            CallerResolvesAppeals = holdsAppealSeat &&
                                    eligible is not null &&
                                    PanelSeat.AppealBodyAt(review, eligible).Any(member =>
                                        string.Equals(member.UserId, callerUserId, StringComparison.Ordinal)),
            TraineeElsewhere = traineeElsewhere,
            TraineeProfileId = traineeProfileId,
            TrainingYear = trainingYear,
            AppealBody = (appealBodyRead ? AppealBodyOf(review, eligible) : [])
                .Select(person => person with { Name = names.NameOf(person.UserId) })
                .ToArray(),
            PanelInstitutionName = institutionName,
            Appeals = detail.Appeals
                .Select(appeal => appeal with
                {
                    LodgedByName = names.NameOf(appeal.LodgedByUserId),
                    ResolvedByName = appeal.ResolvedByUserId is { } resolver ? names.NameOf(resolver) : null
                })
                .ToArray(),
            PanelMembers = detail.PanelMembers
                .Select(person => person with
                {
                    Name = names.NameOf(person.UserId),
                    MaySit = seated.Contains(person.UserId)
                })
                .ToArray(),
            EvidenceItems = detail.EvidenceItems
                .Select(item => item.AssessorUserId is { } assessor ? item with { AssessorName = names.NameOf(assessor) } : item)
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
    /// The trainee's registrar-page id and training year, for the review page (T361, round 2 review 10, E6).
    /// </summary>
    /// <remarks>
    /// <para>
    /// The id is the preferred profile's (<see cref="TraineeScopeResolver.PreferredProfiles" />), the one profile the
    /// registrar page opens (<c>GetProgrammeTraineeQuery</c>), and only while it is at the panel's institution: once the
    /// trainee trains elsewhere that page is not this panel's staff's to open.
    /// </para>
    /// <para>
    /// The year is read from the programme the review sat for: the preferred profile when it is at the panel's
    /// institution, else the latest profile there, so a review of a trainee who has since moved keeps the year it sat
    /// in. It is the year on the period's last day, or on the programme's when it ended sooner, as an ended programme is
    /// read everywhere (T252); null before the programme starts and for a trainee with no profile there.
    /// </para>
    /// </remarks>
    private static async Task<(int? ProfileId, int? TrainingYear)> TraineeOfAsync(
        IApplicationDbContext dbContext,
        CommitteeReview review,
        CancellationToken cancellationToken)
    {
        var institutionId = review.Panel.InstitutionId;
        var preferred = await TraineeScopeResolver.PreferredProfiles(dbContext)
            .AsNoTracking()
            .Where(profile => profile.UserId == review.TraineeUserId)
            .SingleOrDefaultAsync(cancellationToken);

        var atPanel = preferred is not null && preferred.InstitutionId == institutionId
            ? preferred
            : await dbContext.Set<TraineeProfile>()
                .AsNoTracking()
                .Where(profile => profile.UserId == review.TraineeUserId && profile.InstitutionId == institutionId)
                .OrderByDescending(profile => profile.Id)
                .FirstOrDefaultAsync(cancellationToken);

        if (atPanel is null)
        {
            return (null, null);
        }

        var day = atPanel.EndedOn is { } ended && ended < review.ReviewPeriodTo ? ended : review.ReviewPeriodTo;
        return (ReferenceEquals(atPanel, preferred) ? atPanel.Id : null, atPanel.GetStage(day));
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
    /// Why nobody can take the chair's actions at this review now, or null when its chair may (T256): the panel's chair
    /// may not sit at it (<see cref="PanelSeat.SittingAt" />), the rule every chair's action demands
    /// (<see cref="CommitteeDecisionAuthorization.Chairs" />). Said to the Chair seat's holder in the words their click
    /// would be refused with, and to everyone else naming the chair. Start refuses with the same sentence while it is not
    /// null (T361, Q11, E2), so the page's reason and the server's refusal are one.
    /// </summary>
    internal static string? ChairCannotActOf(
        CommitteeReview review,
        ClaimsPrincipal principal,
        IReadOnlyDictionary<string, Common.Interfaces.UserIdentityDetails> eligible,
        UserDisplayNames names)
    {
        if (PanelSeat.SittingAt(review, eligible).Any(member => member.Role == DecisionPanelMemberRole.Chair))
        {
            return null;
        }

        if (CommitteeDecisionAuthorization.HoldsChairSeat(principal, review.Panel))
        {
            return PanelSeat.MayNotChairFromSeat;
        }

        var chair = review.Panel.Members.FirstOrDefault(member => member.Role == DecisionPanelMemberRole.Chair);
        return PanelSeat.ChairCannotAct(chair is null ? null : names.NameOf(chair.UserId));
    }

    /// <summary>
    /// Whether the appeal body is read (<see cref="AppealBodyOf" />): under appeal, for every reader; and on the trainee's
    /// own page while the review is Ratified too, where "What happens next" names who would hear an appeal before she
    /// lodges one (T366, round 2's data row: until then it was read under appeal only).
    /// </summary>
    private static bool AppealBodyRead(CommitteeReview review, bool forItsTrainee)
        => review.State == CommitteeReviewState.UnderAppeal ||
           (forItsTrainee && review.State == CommitteeReviewState.Ratified);

    /// <summary>
    /// Who can resolve the appeal now (<see cref="CommitteeReviewDetailDto.AppealBody" />, T237): the appeal body that can
    /// act (<see cref="PanelSeat.AppealBodyAt" />), the one list the resolve handler also demands the caller is on. Read
    /// only where <see cref="AppealBodyRead" /> says, and for every reader: the note it feeds tells the trainee too who
    /// resolves their appeal, and must name nobody who cannot.
    /// </summary>
    private static IReadOnlyList<CommitteePersonDto> AppealBodyOf(
        CommitteeReview review,
        IReadOnlyDictionary<string, Common.Interfaces.UserIdentityDetails>? eligible)
        => eligible is null
            ? []
            : PanelSeat.AppealBodyAt(review, eligible)
                .Select(member => new CommitteePersonDto(member.UserId, member.Role))
                .ToArray();
}
