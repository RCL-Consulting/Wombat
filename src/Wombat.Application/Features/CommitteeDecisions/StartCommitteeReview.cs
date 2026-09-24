using System.Globalization;
using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Activities;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Epas;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.CommitteeDecisions;

public sealed record StartCommitteeReviewCommand(int ReviewId, ClaimsPrincipal Principal) : IRequest<CommitteeReviewDetailDto>;

public sealed class StartCommitteeReviewCommandValidator : AbstractValidator<StartCommitteeReviewCommand>
{
    public StartCommitteeReviewCommandValidator()
    {
        RuleFor(command => command.ReviewId).GreaterThan(0);
        RuleFor(command => command.Principal).NotNull();
    }
}

public sealed class StartCommitteeReviewCommandHandler : IRequestHandler<StartCommitteeReviewCommand, CommitteeReviewDetailDto>
{
    private readonly IApplicationDbContext _dbContext;

    public StartCommitteeReviewCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CommitteeReviewDetailDto> Handle(StartCommitteeReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _dbContext.Set<CommitteeReview>()
            .Include(entity => entity.Panel)
                .ThenInclude(panel => panel.Members)
            .Include(entity => entity.Decisions)
            .Include(entity => entity.Appeals)
            .Include(entity => entity.EvidenceItems)
            .SingleOrDefaultAsync(entity => entity.Id == request.ReviewId, cancellationToken)
            ?? throw new InvalidOperationException("The committee review could not be found.");

        CommitteeDecisionAuthorization.DemandPanelAccess(request.Principal, review.Panel);
        await CommitteeTraineeScope.DemandTraineeAtPanelInstitutionAsync(_dbContext, request.Principal, review, cancellationToken);

        var actorUserId = CommitteeDecisionAuthorization.GetRequiredUserId(request.Principal);
        var evidenceItems = await BuildEvidenceSnapshotAsync(review, cancellationToken);
        review.Start(evidenceItems, actorUserId, DateTime.UtcNow);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return review.ToDetailDto();
    }

    private async Task<IReadOnlyList<CommitteeEvidence>> BuildEvidenceSnapshotAsync(CommitteeReview review, CancellationToken cancellationToken)
    {
        // A review period is a clinical period: the panel is judging what the trainee DID between these
        // dates, so evidence falls in the window by its encounter date, not by when the form was filed.
        // An encounter in March that reached the system in September belongs to the March review. (T119,
        // decision D2 — and note this is a visible change to what a panel is shown.)
        //
        // Both bounds are DateOnly against a DateOnly column, inclusive at each end, which is what the
        // AddDays(1)-exclusive instant was emulating. MSF below windows on an instant column, so it turns
        // the same inclusive days into a half-open UTC range.
        var fromDate = review.ReviewPeriodFrom;
        var toDate = review.ReviewPeriodTo;

        // No state filter here, deliberately. A declined, cancelled or still-draft WBA is evidence about
        // the trainee's progress in the window - a run of declines is exactly what a panel should see - and
        // each row prints its state. MSF below is the opposite: a campaign is evidence only once released,
        // so do not "fix" this asymmetry by filtering activities to match. (T138)
        //
        // The sampling report on the same page (GetSamplingConcentrationWarnings) shares these date bounds
        // but NOT this row set: it samples only rated activities in a terminal state of their pinned workflow
        // (D44), and it is computed live while this list is frozen here. So it leaves out rows listed here,
        // and it can count rows this list never held (a WBA observed in the window and completed after
        // Start). Do not "fix" that either; the snapshot labels each row's state, and the report's
        // arithmetic has no label to carry a declined rating. (T135)
        var activities = await _dbContext.Set<Activity>()
            .AsNoTracking()
            .Include(activity => activity.ActivityType)
            .Where(activity =>
                activity.SubjectUserId == review.TraineeUserId &&
                activity.ObservedOn >= fromDate &&
                activity.ObservedOn <= toDate)
            .OrderByDescending(activity => activity.UpdatedOn)
            .ToListAsync(cancellationToken);

        // A campaign falls in the window by the UTC day it actually closed (T138); MsfCampaignReviewWindow says
        // why, and is shared with the live notice that counts the campaigns this leaves out for being unreleased
        // at Start (T173). Every campaign here is released, and a released one always has ClosedOn: Release requires
        // UnderReview, which only Close produces, and Close stamps it.
        var msfCampaigns = await _dbContext.Set<MsfCampaign>()
            .AsNoTracking()
            .Include(campaign => campaign.Template)
            .Include(campaign => campaign.Responses)
            // T121: so the campaign row can say what its evidence activities are FOR. A released
            // campaign now contributes both this row and one Activity row per covered EPA, and without
            // the coverage on the parent a panel sees one campaign and N unexplained siblings dated the
            // same minute. They are not duplicates; they are a report and its per-EPA claims, and the
            // summary below is where that is said.
            .Include(campaign => campaign.CoveredEpas)
                .ThenInclude(covered => covered.Epa)
            // Released only (T138), the same rule as PortfolioPdfService and ListMsfCampaignsForTraineeQuery.
            // Before release the trainee has not seen the report and the coordinator may still withdraw it;
            // a withdrawn campaign was deliberately retracted. Neither is evidence a panel should weigh.
            //
            // The cost: a campaign still under review when the chair starts is in no snapshot, ever. The
            // snapshot is frozen here and the release comes later, and the next review's window does not
            // cover the day it closed; the per-EPA activities the release creates miss both the same way,
            // exactly as a WBA observed in the window but filed after Start does. The panel is told instead
            // by a live count on the review (CountMsfCampaignsOutsideSnapshotQuery, T173), which is not
            // evidence and is not frozen here, and which still counts the campaign once it is released.
            .ClosedInWindowOf(review)
            .Where(campaign => campaign.State == MsfCampaignState.Released)
            .OrderByDescending(campaign => campaign.ClosedOn)
            .ToListAsync(cancellationToken);

        var describe = await ActivityDescriber.LoadAsync(_dbContext, activities, cancellationToken);
        var activityEvidence = activities.Select(describe.Describe);

        var msfEvidence = msfCampaigns.Select(campaign => new CommitteeEvidence
        {
            SourceType = CommitteeEvidenceSourceType.MsfCampaign,
            MsfCampaignId = campaign.Id,
            SourceLabel = $"{campaign.Template.Name} #{campaign.Id}",
            Summary = $"State: {campaign.State}; responses {campaign.Responses.Count}; " +
                      $"closed {campaign.ClosedOn:yyyy-MM-dd}.{DescribeCoverage(campaign)}",
            SourceRecordedOn = campaign.ReleasedOn ?? campaign.ClosedOn ?? campaign.OpenedOn ?? campaign.CreatedOn,
            // A campaign is a report across the EPAs it covers, not evidence for one: it has no EPA, instrument, rating
            // or encounter of its own. Its per-EPA claims are ordinary activities above, each under its EPA. (T167)
            SourceState = campaign.State.ToString()
        });

        return activityEvidence.Concat(msfEvidence).ToArray();
    }

    /// <summary>
    /// What an activity's snapshot line says about it: its EPA, instrument, rating, encounter date and state. (T167)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Built once per snapshot from lookups bounded by the window's activities, never per row.
    /// </para>
    /// <list type="bullet">
    ///   <item><b>The EPA</b> is the stamped <c>Activity.EpaId</c> (T137), with its code and title. It is not re-read out
    ///   of <c>DataJson</c>; the stamp is the EPA the activity's list shows and its credit went to.</item>
    ///   <item><b>The instrument</b> is the type's <c>WbaToolKey</c> (T122) by the College's name for it, and the type's
    ///   own name when it declares none or the key names no instrument.</item>
    ///   <item><b>The rating</b> is the pinned version's <c>rated_level_field</c>, read by <see cref="RatedEvidenceProfile" />
    ///   (T135), the reader the sampling report and the trajectory share, and labelled as a rung on the ladder that field
    ///   names, falling back to the ordinal. It is read in every state and printed beside the state: a snapshot shows a
    ///   declined rating as declined, where the sampling report leaves it out (D44).</item>
    ///   <item><b>The encounter date</b> is <c>Activity.ObservedOn</c> (T119) with its source, so a date nobody stated is
    ///   shown as the filing day it is (T161), not as a clinical fact.</item>
    /// </list>
    /// <para>
    /// Each is also written into <see cref="CommitteeEvidence.Summary" />, so the line reads on its own wherever the
    /// summary is quoted (an evidence link on a STAR, T131).
    /// </para>
    /// </remarks>
    private sealed class ActivityDescriber
    {
        private readonly IReadOnlyDictionary<(int ActivityTypeId, int Version), RatedEvidenceProfile> _profiles;
        private readonly EntrustmentRungLookup _ladders;
        private readonly IReadOnlyDictionary<int, (string Code, string Title)> _epas;
        private readonly IReadOnlyDictionary<string, string> _instrumentNames;

        private ActivityDescriber(
            IReadOnlyDictionary<(int ActivityTypeId, int Version), RatedEvidenceProfile> profiles,
            EntrustmentRungLookup ladders,
            IReadOnlyDictionary<int, (string Code, string Title)> epas,
            IReadOnlyDictionary<string, string> instrumentNames)
        {
            _profiles = profiles;
            _ladders = ladders;
            _epas = epas;
            _instrumentNames = instrumentNames;
        }

        public static async Task<ActivityDescriber> LoadAsync(
            IApplicationDbContext dbContext,
            IReadOnlyCollection<Activity> activities,
            CancellationToken cancellationToken)
        {
            var profiles = await RatedEvidenceProfiles.LoadAsync(
                dbContext,
                activities.Select(activity => (activity.ActivityTypeId, activity.SchemaVersion)),
                cancellationToken);

            var ladders = await EntrustmentRungLabels.LoadForScaleKeysAsync(
                dbContext,
                profiles.Values.Select(profile => profile.RatedScaleKey),
                cancellationToken);

            var epaIds = activities.Select(activity => activity.EpaId).OfType<int>().Distinct().ToArray();
            var epas = epaIds.Length == 0
                ? new Dictionary<int, (string Code, string Title)>()
                : (await dbContext.Set<Epa>()
                    .AsNoTracking()
                    .Where(epa => epaIds.Contains(epa.Id))
                    .Select(epa => new { epa.Id, epa.Code, epa.Title })
                    .ToListAsync(cancellationToken))
                    .ToDictionary(epa => epa.Id, epa => (epa.Code, epa.Title));

            var toolKeys = activities
                .Select(activity => WbaTool.NormalizeKey(activity.ActivityType.WbaToolKey))
                .OfType<string>()
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            var instrumentNames = toolKeys.Length == 0
                ? new Dictionary<string, string>(StringComparer.Ordinal)
                : await dbContext.Set<WbaTool>()
                    .AsNoTracking()
                    .Where(tool => toolKeys.Contains(tool.Key))
                    .ToDictionaryAsync(tool => tool.Key, tool => tool.Name, StringComparer.Ordinal, cancellationToken);

            return new ActivityDescriber(profiles, ladders, epas, instrumentNames);
        }

        public CommitteeEvidence Describe(Activity activity)
        {
            var profile = _profiles[(activity.ActivityTypeId, activity.SchemaVersion)];

            (string Code, string Title)? epa = activity.EpaId is int epaId && _epas.TryGetValue(epaId, out var found)
                ? found
                : null;

            var instrumentKey = WbaTool.NormalizeKey(activity.ActivityType.WbaToolKey);
            var instrumentName = instrumentKey is not null && _instrumentNames.TryGetValue(instrumentKey, out var toolName)
                ? toolName
                : activity.ActivityType.Name;

            var rating = profile.ReadRating(activity.DataJson);
            var ratingLabel = rating is int order ? _ladders.FormatByScaleKey(profile.RatedScaleKey, order) : null;
            var declared = activity.ObservedOnSource == ObservationDateSource.Declared;

            var summary = new List<string>
            {
                $"State: {activity.CurrentState}",
                epa is { } described ? $"EPA {described.Code}" : "about no EPA",
                instrumentName,
                ratingLabel is not null
                    ? $"rated {ratingLabel}"
                    : profile.IsRatedInstrument ? "no rating recorded" : "unrated",
                // The one wording of an encounter date (T161, D28), so the summary says what the page's column does.
                $"encounter {EncounterDate.Label(activity.ObservedOn, declared)}",
                Invariant($"updated {activity.UpdatedOn:yyyy-MM-dd HH:mm} UTC")
            };

            return new CommitteeEvidence
            {
                SourceType = CommitteeEvidenceSourceType.Activity,
                ActivityId = activity.Id,
                SourceLabel = $"{activity.ActivityType.Name} #{activity.Id}",
                Summary = string.Join("; ", summary) + ".",
                SourceRecordedOn = activity.UpdatedOn,
                EpaId = epa is null ? null : activity.EpaId,
                EpaCode = epa?.Code,
                EpaTitle = epa?.Title,
                InstrumentKey = instrumentKey,
                InstrumentName = instrumentName,
                IsRatedInstrument = profile.IsRatedInstrument,
                RatingOrder = rating,
                RatingLabel = ratingLabel,
                ObservedOn = activity.ObservedOn,
                ObservedOnSource = activity.ObservedOnSource,
                SourceState = activity.CurrentState
            };
        }

        private static string Invariant(FormattableString value) => value.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// What a campaign declared itself evidence for, and whether that reached the portfolio. (T121)
    /// </summary>
    /// <remarks>
    /// A released campaign appears in this snapshot twice over: once here, and once per covered EPA as
    /// an ordinary <c>msf_cpsa</c> activity. That is deliberate — the panel wants both the report and
    /// the per-EPA claims — but it is only readable if the parent names the children, which is what this
    /// sentence does. An empty coverage set is worth printing too: it is the one case where a released
    /// campaign left no evidence at all. Only released campaigns reach the snapshot (T138), so there is
    /// no "to be recorded on release" case to describe.
    /// </remarks>
    private static string DescribeCoverage(MsfCampaign campaign)
    {
        var covered = campaign.CoveredEpas
            .Where(entry => entry.Epa is not null)
            .OrderBy(entry => entry.Epa.Code, StringComparer.Ordinal)
            .ToArray();

        if (covered.Length == 0)
        {
            return " Names no EPA, so it recorded no evidence.";
        }

        // Per EPA, never per campaign. A release records evidence for the EPAs still on the trainee's
        // curriculum and drops the rest, so a campaign can be half recorded - and a panel told
        // "recorded as one activity each" about an EPA whose activity was never written would go
        // looking for a record that does not exist. Released with nothing recorded is terminal, not
        // pending: Release refuses a second attempt.
        var recorded = covered.Where(entry => entry.RecordedOn is not null).Select(entry => entry.Epa.Code).ToArray();
        var missing = covered.Where(entry => entry.RecordedOn is null).Select(entry => entry.Epa.Code).ToArray();

        var sentence = recorded.Length > 0
            ? $" Evidence recorded for {string.Join(", ", recorded)}, one activity each."
            : string.Empty;

        return missing.Length > 0
            ? sentence + $" Also declared {string.Join(", ", missing)}, no longer on the trainee's " +
              "curriculum, so nothing was recorded for those."
            : sentence;
    }
}
