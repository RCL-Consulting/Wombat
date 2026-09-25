using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Common.Users;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.Institutions;

namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>
/// Where the entrustment decision on one EPA due in a period stands for one trainee: the decisions-due page's status.
/// "Missed" is computed when the page is read, never stored (Decision 6). (T131 slice 6)
/// </summary>
public enum EntrustmentDecisionDueStatus
{
    /// <summary>
    /// A STAR from a sitting in the window still decides it (<see cref="CommitteeAgendaStatus.StarDecides" />): Active,
    /// Expired (expiry is informational, Decision 10), or Superseded outside the window or by one that decides it.
    /// </summary>
    Decided = 1,

    /// <summary>It is on the agenda of an open review (scheduled, in progress, or decided and awaiting ratify).</summary>
    Scheduled = 2,

    /// <summary>
    /// A sitting deferred it, with a reason, and no later sitting has decided it since. A later sitting plans it again.
    /// </summary>
    Deferred = 3,

    /// <summary>The window's decision was revoked, and no sitting has deferred it since: it must be decided again.</summary>
    Revoked = 4,

    /// <summary>An annual EPA before its year's last semester: not scheduled yet, and decided by the year's last sitting.</summary>
    DueByYearEnd = 5,

    /// <summary>The trainee joined the window part-way through: optional, and never missed (Decision 9).</summary>
    PartialPeriod = 6,

    /// <summary>The College decides the EPA as opportunity allows (O7): optional, and never missed.</summary>
    AsOpportunityAllows = 7,

    /// <summary>Due in this period, not decided, and no open review holds it, while its window is still open.</summary>
    NotScheduled = 8,

    /// <summary>Its window has ended with nothing decided, deferred, or on an open review's agenda.</summary>
    Missed = 9
}

/// <summary>One EPA due for one trainee in the period, and where its decision stands. (T131 slice 6)</summary>
/// <param name="TraineeName">The trainee by name (T142), or their id where no name is on record.</param>
/// <param name="WindowLabel">"2026 S1" for a semester EPA, "2026" for an annual one.</param>
/// <param name="ReviewId">
/// The review the status is about: the one that issued the STAR for Decided and Revoked; the open review whose agenda
/// holds it for Scheduled; the one that deferred it for Deferred. Null otherwise.
/// </param>
/// <param name="MayOpenReview">Whether the caller passes that review's read ladder, so the page may link to it.</param>
/// <param name="EntrustmentDecisionId">The STAR a Decided or Revoked status is about.</param>
/// <param name="SchedulePanelId">
/// The panel the EPA's decision routes to for this trainee (<see cref="DecisionRouting" />), when the caller may schedule
/// the trainee before it: what the page's Schedule link fills in. Null when no panel at the institution covers the trainee.
/// </param>
/// <param name="SchedulePanelName">That panel's name.</param>
/// <param name="SchedulePeriodKey">
/// The period a sitting that decides the window sits for, as the scheduling form's period select says it ("2026-2"): the
/// window's last semester. For an annual EPA that is semester 2, whichever semester the page shows, since the year's last
/// sitting is the one it closes at (Decision 5).
/// </param>
/// <param name="SchedulePeriodLabel">That period: "2026 S2".</param>
/// <param name="HoldingReviewId">
/// The open binding review that already puts the trainee before <paramref name="SchedulePanelName" />'s seat for that
/// period, if any: scheduling another would be refused (one open binding review per trainee, period and seat), so the page
/// points to this one instead of offering Schedule.
/// </param>
/// <param name="MayOpenHoldingReview">Whether the caller passes that review's read ladder.</param>
public sealed record EntrustmentDecisionDueDto(
    string TraineeUserId,
    string TraineeName,
    int EpaId,
    string EpaCode,
    string EpaTitle,
    string WindowLabel,
    EntrustmentDecisionDueStatus Status,
    int? ReviewId,
    bool MayOpenReview,
    int? EntrustmentDecisionId,
    int? SchedulePanelId,
    string? SchedulePanelName,
    string SchedulePeriodKey,
    string SchedulePeriodLabel,
    int? HoldingReviewId,
    bool MayOpenHoldingReview)
{
    /// <summary>Whether nothing yet decides it: every status but Decided.</summary>
    public bool IsOutstanding => Status != EntrustmentDecisionDueStatus.Decided;
}

/// <summary>How the trainees stand on one EPA in the period: the page's summary row. (T131 slice 6)</summary>
/// <param name="Due">How many trainees owe a decision on the EPA in the period, whatever its status.</param>
/// <param name="ToSchedule">Not scheduled, due by year end, or revoked: no open review holds it yet.</param>
/// <param name="Optional">A partial period, or decided as opportunity allows: never missed.</param>
public sealed record EntrustmentDecisionDueEpaSummaryDto(
    int EpaId,
    string EpaCode,
    string EpaTitle,
    int Due,
    int Decided,
    int Scheduled,
    int Deferred,
    int ToSchedule,
    int Missed,
    int Optional);

/// <summary>The decisions due at one institution in one period. (T131 slice 6)</summary>
/// <param name="PeriodLabel">"2026 S1".</param>
/// <param name="Items">One row per trainee per EPA due, ordered by trainee name, then EPA code.</param>
public sealed record EntrustmentDecisionsDueDto(
    int InstitutionId,
    string InstitutionName,
    int AcademicYear,
    int Semester,
    string PeriodLabel,
    IReadOnlyList<EntrustmentDecisionDueDto> Items)
{
    /// <summary>How many trainees owe at least one decision in the period.</summary>
    public int TraineeCount => Items.Select(item => item.TraineeUserId).Distinct(StringComparer.Ordinal).Count();

    /// <summary>One row per EPA due, in code order.</summary>
    public IReadOnlyList<EntrustmentDecisionDueEpaSummaryDto> ByEpa
        => Items
            .GroupBy(item => (item.EpaId, item.EpaCode, item.EpaTitle))
            .Select(group => new EntrustmentDecisionDueEpaSummaryDto(
                group.Key.EpaId,
                group.Key.EpaCode,
                group.Key.EpaTitle,
                group.Count(),
                group.Count(item => item.Status == EntrustmentDecisionDueStatus.Decided),
                group.Count(item => item.Status == EntrustmentDecisionDueStatus.Scheduled),
                group.Count(item => item.Status == EntrustmentDecisionDueStatus.Deferred),
                group.Count(item => item.Status is EntrustmentDecisionDueStatus.NotScheduled
                    or EntrustmentDecisionDueStatus.DueByYearEnd
                    or EntrustmentDecisionDueStatus.Revoked),
                group.Count(item => item.Status == EntrustmentDecisionDueStatus.Missed),
                group.Count(item => item.Status is EntrustmentDecisionDueStatus.PartialPeriod
                    or EntrustmentDecisionDueStatus.AsOpportunityAllows)))
            .OrderBy(summary => summary.EpaCode, StringComparer.Ordinal)
            .ThenBy(summary => summary.EpaId)
            .ToArray();
}

/// <summary>
/// Which EPAs are due for decision in a period, per trainee at an institution, and where each decision stands: the
/// decisions-due page. (T131 slice 6)
/// </summary>
/// <remarks>
/// <para>
/// <b>Who and where.</b> An institution's trainees, so it is scoped like one (the T056 family). A global Administrator
/// must name the institution. Anyone else reads their own, whatever they name: institution B's query ignores
/// <c>InstitutionId = A</c>. The trainees are the current trainees whose active profile is at the institution
/// (<see cref="TraineeScopeResolver.ResolveAllCurrentAsync" />, T238), each kept only when the caller administers or
/// coordinates them (<see cref="TraineeScopeResolver.IsAdministeredOrCoordinatedBy" />): the reach of the roles that
/// schedule reviews, with no CommitteeMember arm. A trainee whose programme has ended owes the committee nothing, and is
/// not listed; nor is an erased trainee's pseudonym, whose profile stays active under an id no account holds, nor anyone
/// who no longer holds Trainee. The page's Schedule link is then offered only for a trainee scheduling accepts.
/// </para>
/// <para>
/// <b>A trainee first</b> (<see cref="TraineeScopeResolver.ActsAsTrainee" />, T185). A registrar who also coordinates, or
/// administers, the programme, the Administrator role included, is shown nobody's decisions here: this page names every
/// trainee's standing on every EPA, which is a peer's record. They get what a caller with nothing to see gets, null, asked
/// before any other role that would admit them.
/// </para>
/// <para>
/// <b>What is due.</b> The planner's rule, without a panel: each of the trainee's admitted curriculum items
/// (<see cref="StarCurriculum.AdmittedItems" />, scoped on <c>OwningInstitutionId</c>) that has a decision cadence, in its
/// cadence window holding the period (<see cref="QuotaWindow.For" />, read on the period's last day), when the trainee had
/// started by the period's end (<see cref="CommitteeAgendaLine.IsDueAt" />): someone who starts in semester 2 owes
/// nothing in semester 1, their annual EPAs included. No calendar is read but <see cref="AcademicPeriod" />'s.
/// </para>
/// <para>
/// <b>Where it stands</b> is <see cref="CommitteeAgendaStatus.DecisionDue" />, over the records the planner reads
/// (<see cref="DecisionWindowRecords" />). A STAR is in the window when the sitting that issued it sat for a period the
/// window holds, so a sitting held after its period ends still decides that period. A line still due or deferred at
/// another institution's panel (a review the trainee stranded when they moved) says nothing about where the decision
/// stands here; a Decided line, like a STAR, goes with the trainee.
/// </para>
/// <para>
/// <b>Schedule.</b> Each row names the panel a sitting that decides it would sit before, and the period it would sit for:
/// the window's last semester (<see cref="EntrustmentDecisionDueDto.SchedulePeriodKey" />). Where an open binding review
/// already holds the trainee's seat before that panel for that period, the row names it instead
/// (<see cref="EntrustmentDecisionDueDto.HoldingReviewId" />, <see cref="CommitteeReviewSeats" />), since scheduling
/// another would be refused.
/// </para>
/// <para>
/// Reads only. "Today" is the programme's (<see cref="ProgrammeCalendar.DateOf" />) unless a test pins it.
/// </para>
/// </remarks>
/// <param name="InstitutionId">The institution to read: required of an Administrator, ignored for anyone else.</param>
/// <param name="Today">The day "missed" is judged on; the programme's today when null.</param>
public sealed record GetEntrustmentDecisionsDueQuery(
    int AcademicYear,
    int Semester,
    int? InstitutionId,
    ClaimsPrincipal Principal,
    DateOnly? Today = null) : IRequest<EntrustmentDecisionsDueDto?>;

public sealed class GetEntrustmentDecisionsDueQueryValidator : AbstractValidator<GetEntrustmentDecisionsDueQuery>
{
    public GetEntrustmentDecisionsDueQueryValidator()
    {
        RuleFor(query => query.AcademicYear).InclusiveBetween(ReviewPeriodRules.FirstYear, ReviewPeriodRules.LastYear);
        RuleFor(query => query.Semester).InclusiveBetween(1, 2).WithMessage(ReviewPeriodRules.SemesterMessage);
        RuleFor(query => query.Principal).NotNull();
    }
}

public sealed class GetEntrustmentDecisionsDueQueryHandler
    : IRequestHandler<GetEntrustmentDecisionsDueQuery, EntrustmentDecisionsDueDto?>
{
    /// <summary>The refusal a global Administrator gets for not naming an institution.</summary>
    internal const string AdministratorMustNameInstitution = "Choose the institution whose decisions due to show.";

    private readonly IApplicationDbContext _dbContext;
    private readonly IUserAdministrationService _users;

    public GetEntrustmentDecisionsDueQueryHandler(IApplicationDbContext dbContext, IUserAdministrationService users)
    {
        _dbContext = dbContext;
        _users = users;
    }

    public async Task<EntrustmentDecisionsDueDto?> Handle(
        GetEntrustmentDecisionsDueQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request.Principal);
        var principal = request.Principal;

        // The trainee rung first (T185): a registrar with an admin or coordinating role reads no peer's record here.
        if (TraineeScopeResolver.ActsAsTrainee(principal))
        {
            return null;
        }

        if (ResolveInstitution(request) is not int institutionId)
        {
            return null;
        }

        var institutionName = await _dbContext.Set<Institution>()
            .AsNoTracking()
            .Where(institution => institution.Id == institutionId)
            .Select(institution => institution.Name)
            .FirstOrDefaultAsync(cancellationToken);

        if (institutionName is null)
        {
            return null;
        }

        var period = new AcademicPeriod(request.AcademicYear, request.Semester);
        var today = request.Today ?? ProgrammeCalendar.DateOf(DateTime.UtcNow);

        var trainees = await TraineesAsync(principal, institutionId, cancellationToken);
        if (trainees.Count == 0)
        {
            return new EntrustmentDecisionsDueDto(institutionId, institutionName, period.Year, period.Semester, period.ToString(), []);
        }

        var traineeIds = trainees.Select(trainee => trainee.UserId).ToArray();

        var itemsByCurriculum = new Dictionary<int, IReadOnlyList<DueItem>>();
        foreach (var curriculumId in trainees.Select(trainee => trainee.CurriculumId).Distinct())
        {
            itemsByCurriculum[curriculumId] = await StarCurriculum.AdmittedItems(_dbContext, curriculumId, institutionId)
                .Where(item => item.DecisionCadence != null)
                .Select(item => new DueItem(
                    item.EpaId,
                    item.Epa.Code,
                    item.Epa.Title,
                    item.DecisionCadence!.Value,
                    item.DecisionBodyKey,
                    item.DecisionIsOpportunistic))
                .ToListAsync(cancellationToken);
        }

        // Every window of the period lies in its academic year, a semester's or the whole year's, so the year bounds both.
        var records = await DecisionWindowRecords.LoadAsync(_dbContext, traineeIds, period.Year, cancellationToken);

        var panels = await _dbContext.Set<DecisionPanel>()
            .AsNoTracking()
            .Where(panel => panel.InstitutionId == institutionId)
            .ToListAsync(cancellationToken);

        // The open binding reviews that hold a seat before the institution's panels this year, the scheduling handler's
        // predicate (CommitteeReviewSeats): one query, matched in memory.
        var openReviews = (await _dbContext.Set<CommitteeReview>()
                .AsNoTracking()
                .OpenBinding()
                .Where(review => traineeIds.Contains(review.TraineeUserId) &&
                                 review.Panel.InstitutionId == institutionId &&
                                 review.AcademicYear == period.Year)
                .Select(review => new OpenSeatReview(
                    review.Id, review.TraineeUserId, review.Panel.DecisionBodyKey, review.AcademicYear, review.Semester))
                .ToListAsync(cancellationToken))
            .ToLookup(review => review.TraineeUserId, StringComparer.Ordinal);

        var names = await UserDisplayNames.ResolveAsync(_users, traineeIds, cancellationToken);

        var rows = new List<EntrustmentDecisionDueDto>();
        foreach (var trainee in trainees)
        {
            foreach (var item in itemsByCurriculum[trainee.CurriculumId])
            {
                var window = QuotaWindow.For(item.Cadence, period.End, trainee.ProgrammeStart);
                if (!CommitteeAgendaLine.IsDueAt(window, period))
                {
                    continue;
                }

                rows.Add(Row(
                    principal,
                    trainee,
                    names.NameOf(trainee.UserId),
                    item,
                    window,
                    period,
                    today,
                    records,
                    records.LinesIn(trainee.UserId, item.EpaId, window)
                        .Where(line => line.State == CommitteeAgendaLineState.Decided || line.PanelInstitutionId == institutionId)
                        .ToArray(),
                    panels,
                    openReviews[trainee.UserId]));
            }
        }

        rows = await LinkReviewsAsync(principal, rows, cancellationToken);

        return new EntrustmentDecisionsDueDto(
            institutionId,
            institutionName,
            period.Year,
            period.Semester,
            period.ToString(),
            rows
                .OrderBy(row => row.TraineeName, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(row => row.TraineeUserId, StringComparer.Ordinal)
                .ThenBy(row => row.EpaCode, StringComparer.Ordinal)
                .ThenBy(row => row.EpaId)
                .ToArray());
    }

    /// <summary>The institution to read, or null when the caller may read none.</summary>
    private static int? ResolveInstitution(GetEntrustmentDecisionsDueQuery request)
    {
        var principal = request.Principal;

        if (principal.IsAdministrator())
        {
            return request.InstitutionId ?? throw new InvalidOperationException(AdministratorMustNameInstitution);
        }

        // Their own institution, whatever they named. Who among its trainees they see is TraineesAsync's question.
        return principal.GetInstitutionId();
    }

    /// <summary>
    /// The current trainees at the institution (T238), each kept only if the caller administers or coordinates them; a
    /// global Administrator keeps every one.
    /// </summary>
    private async Task<IReadOnlyList<DueTrainee>> TraineesAsync(
        ClaimsPrincipal principal,
        int institutionId,
        CancellationToken cancellationToken)
    {
        // Current trainees only, by the rule scheduling resolves its trainee by, so every Schedule link this page offers
        // is one the handler accepts, and a profile that outlived its trainee is not listed (T238).
        var scopes = await TraineeScopeResolver.ResolveAllCurrentAsync(_dbContext, _users, institutionId, cancellationToken);
        var inScope = scopes
            .Where(pair => principal.IsAdministrator() || TraineeScopeResolver.IsAdministeredOrCoordinatedBy(pair.Value, principal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);

        if (inScope.Count == 0)
        {
            return [];
        }

        var userIds = inScope.Keys.ToArray();
        var profiles = await TraineeScopeResolver.ActiveProfiles(_dbContext)
            .AsNoTracking()
            .Where(profile => profile.InstitutionId == institutionId && userIds.Contains(profile.UserId))
            .Select(profile => new { profile.UserId, profile.CurriculumId, profile.ProgrammeStartDate })
            .ToListAsync(cancellationToken);

        return profiles
            .Select(profile => new DueTrainee(profile.UserId, inScope[profile.UserId], profile.CurriculumId, profile.ProgrammeStartDate))
            .ToArray();
    }

    private static EntrustmentDecisionDueDto Row(
        ClaimsPrincipal principal,
        DueTrainee trainee,
        string traineeName,
        DueItem item,
        QuotaWindow window,
        AcademicPeriod period,
        DateOnly today,
        DecisionWindowRecords records,
        IReadOnlyList<RecordedAgendaLine> lines,
        IReadOnlyList<DecisionPanel> panels,
        IEnumerable<OpenSeatReview> openReviews)
    {
        var stars = records.StarsIn(trainee.UserId, item.EpaId, window)
            .Select(star => (Star: star, Standing: records.Standing(star, window)))
            .ToArray();
        var standings = lines.Select(line => (Line: line, Standing: records.Standing(line, window))).ToArray();

        var status = CommitteeAgendaStatus.DecisionDue(
            standings.Select(line => line.Standing),
            stars.Select(star => star.Standing),
            item.IsOpportunistic,
            window,
            period,
            today);

        var (reviewId, starId) = ReferenceOf(status, stars, standings);

        // The panel the Schedule link fills in: the one routing gives the EPA to for this trainee, and only when the
        // scheduling rule would accept the caller putting the trainee before it (picker = gate).
        var schedulePanel = panels
            .Where(panel => DecisionRouting.RoutesTo(item.BodyKey, panel, trainee.Scope, panels))
            .OrderBy(panel => panel.Scope == DecisionPanelScope.Speciality ? 0 : 1)
            .ThenBy(panel => panel.Id)
            .FirstOrDefault(panel => CommitteeTraineeScope.MayScheduleFor(principal, panel, trainee.Scope));

        // The sitting that decides the window is one for its last semester: semester 2 for an annual EPA (Decision 5).
        var schedulePeriod = window.Semesters[^1];

        // One open binding review per trainee, period and seat: where one holds the seat, Schedule would be refused.
        var holding = schedulePanel is null
            ? null
            : openReviews
                .Where(review => review.AcademicYear == schedulePeriod.Year &&
                                 review.Semester == schedulePeriod.Semester &&
                                 CommitteeReviewSeats.SameSeat(review.BodyKey, schedulePanel.DecisionBodyKey))
                .MinBy(review => review.Id);

        return new EntrustmentDecisionDueDto(
            trainee.UserId,
            traineeName,
            item.EpaId,
            item.Code,
            item.Title,
            CommitteeAgendaLine.WindowLabelOf(window.AcademicYear, DecisionWindowRecords.WindowSemesterOf(window)),
            status,
            reviewId,
            MayOpenReview: false,
            starId,
            schedulePanel?.Id,
            schedulePanel?.Name,
            CommitteeReviewPeriods.KeyOf(schedulePeriod.Year, schedulePeriod.Semester),
            schedulePeriod.ToString(),
            holding?.Id,
            MayOpenHoldingReview: false);
    }

    /// <summary>
    /// The review a status is about, and the STAR, each the latest in sitting order: the STAR that decides the window for
    /// Decided, the one it lost for Revoked; the open review holding it for Scheduled; the review that deferred it for
    /// Deferred. Nothing otherwise.
    /// </summary>
    private static (int? ReviewId, int? StarId) ReferenceOf(
        EntrustmentDecisionDueStatus status,
        IReadOnlyList<(DecisionWindowStar Star, StarStanding Standing)> stars,
        IReadOnlyList<(RecordedAgendaLine Line, AgendaLineStanding Standing)> lines)
    {
        switch (status)
        {
            case EntrustmentDecisionDueStatus.Decided:
                var deciding = stars.Where(star => star.Standing.Decides).Select(star => star.Star).MaxBy(star => star.Sitting.Key);
                return deciding is not null
                    ? (deciding.Sitting.ReviewId, deciding.Id)
                    : (Latest(lines.Where(line => line.Standing.Decides)), null);
            case EntrustmentDecisionDueStatus.Revoked:
                var lapsed = stars.Where(star => !star.Standing.Decides).Select(star => star.Star).MaxBy(star => star.Sitting.Key);
                return lapsed is not null
                    ? (lapsed.Sitting.ReviewId, lapsed.Id)
                    : (Latest(lines.Where(line => line.Standing.Lapsed)), null);
            case EntrustmentDecisionDueStatus.Scheduled:
                return (Latest(lines.Where(line =>
                    line.Standing.State == CommitteeAgendaLineState.Due && IsOpen(line.Standing.Review))), null);
            case EntrustmentDecisionDueStatus.Deferred:
                return (Latest(lines.Where(line => line.Standing.State == CommitteeAgendaLineState.Deferred)), null);
            default:
                return (null, null);
        }
    }

    private static int? Latest(IEnumerable<(RecordedAgendaLine Line, AgendaLineStanding Standing)> lines)
        => lines.Select(line => (CommitteeSitting?)line.Line.Sitting).MaxBy(sitting => sitting?.Key)?.ReviewId;

    private static bool IsOpen(CommitteeReviewState state)
        => state is CommitteeReviewState.Scheduled or CommitteeReviewState.InProgress or CommitteeReviewState.Decided;

    /// <summary>
    /// Says, row by row, whether the caller may open the reviews a row names, the one its status is about and the one
    /// holding its seat: the review's own read ladder (<see cref="CommitteeDecisionAuthorization.MayReadReview" />),
    /// asked of each review rather than copied, so the page links only to a review that will open. One query for every
    /// review named.
    /// </summary>
    private async Task<List<EntrustmentDecisionDueDto>> LinkReviewsAsync(
        ClaimsPrincipal principal,
        List<EntrustmentDecisionDueDto> rows,
        CancellationToken cancellationToken)
    {
        var reviewIds = rows.Select(row => row.ReviewId)
            .Concat(rows.Select(row => row.HoldingReviewId))
            .OfType<int>()
            .Distinct()
            .ToArray();
        if (reviewIds.Length == 0)
        {
            return rows;
        }

        var reviews = await _dbContext.Set<CommitteeReview>()
            .AsNoTracking()
            .Include(review => review.Panel)
                .ThenInclude(panel => panel.Members)
            .Where(review => reviewIds.Contains(review.Id))
            .ToListAsync(cancellationToken);

        var readable = reviews
            .Where(review => CommitteeDecisionAuthorization.MayReadReview(principal, review))
            .Select(review => review.Id)
            .ToHashSet();

        return rows
            .Select(row => row with
            {
                MayOpenReview = row.ReviewId is int id && readable.Contains(id),
                MayOpenHoldingReview = row.HoldingReviewId is int holdingId && readable.Contains(holdingId)
            })
            .ToList();
    }

    private sealed record DueTrainee(string UserId, TraineeScope Scope, int CurriculumId, DateOnly ProgrammeStart);

    private sealed record DueItem(
        int EpaId,
        string Code,
        string Title,
        QuotaPeriod Cadence,
        string? BodyKey,
        bool IsOpportunistic);

    /// <summary>An open binding review of a trainee before one of the institution's panels, and the seat it holds.</summary>
    private sealed record OpenSeatReview(int Id, string TraineeUserId, string? BodyKey, int AcademicYear, int Semester);
}
