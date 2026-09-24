using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Features.Reporting;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Institutions;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Application.Features.Activities.Queries;
using Wombat.Application.Features.Activities.Queries.GetEpaTrajectoryForTrainee;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;

namespace Wombat.Infrastructure.Reporting;

internal sealed class PortfolioPdfService : IPortfolioPdfService
{
    // F-5-3 / T078: a fixed timestamp keeps the PDF metadata (and therefore the bytes) deterministic,
    // so the same data always produces the same content hash. Provenance is the content hash itself
    // (the file name + the /portfolio/verify surface), not a wall-clock generation time.
    internal static readonly DateTime DeterministicTimestamp = new(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    internal static DocumentMetadata DeterministicMetadata { get; } = new()
    {
        Title = "Portfolio Export",
        Author = "Wombat",
        Subject = "Trainee portfolio",
        Creator = "Wombat",
        Producer = "Wombat",
        CreationDate = DeterministicTimestamp,
        ModifiedDate = DeterministicTimestamp
    };

    private readonly IApplicationDbContext _dbContext;
    private readonly IMsfAggregationService _msfAggregationService;
    private readonly TimeProvider _timeProvider;

    /// <param name="timeProvider">
    /// "Today" for the per-EPA section of an open-ended export (T169), on the South African calendar. Defaults to the
    /// system clock; a test pins it.
    /// </param>
    public PortfolioPdfService(
        IApplicationDbContext dbContext,
        IMsfAggregationService msfAggregationService,
        TimeProvider? timeProvider = null)
    {
        _dbContext = dbContext;
        _msfAggregationService = msfAggregationService;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<PortfolioExportResult> GenerateAsync(PortfolioExportRequest request, CancellationToken cancellationToken)
    {
        var data = await LoadPortfolioDataAsync(request, cancellationToken);

        // One render at a time in the process, or the fonts can lose their text layer. (T200)
        var pdfBytes = await QuestPdfRenderer.GeneratePdfAsync(ComposeDocument(data), cancellationToken);

        var hash = Convert.ToHexStringLower(SHA256.HashData(pdfBytes));

        var fileName = $"portfolio-{hash[..12]}.pdf";

        return new PortfolioExportResult(pdfBytes, fileName, hash);
    }

    /// <summary>
    /// The portfolio as a document, before it is rendered. Internal so a test can render it as SVG, whose text elements
    /// are what a reader of the page reads (as <c>ActivitiesSectionEncounterDateTests</c> does, T161); the PDF's bytes
    /// carry the text only inside compressed font-encoded streams.
    /// </summary>
    internal static Document ComposeDocument(PortfolioData data)
        => Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(40);
                page.MarginVertical(35);
                page.DefaultTextStyle(style => style.FontSize(9));

                page.Header().Element(header => CoverPageComponent.Compose(header, data));
                page.Content().Element(content => ComposeContent(content, data));
                page.Footer().Element(footer => IntegrityFooterComponent.Compose(footer));
            });
        }).WithMetadata(DeterministicMetadata);

    private static void ComposeContent(IContainer container, PortfolioData data)
    {
        container.Column(column =>
        {
            column.Spacing(10);

            column.Item().Element(e => SummaryPageComponent.Compose(e, data));

            if (data.EntrustmentDecisions.Count > 0)
            {
                column.Item().Element(e => EntrustmentSummaryComponent.Compose(e, data.EntrustmentDecisions));
            }

            // After the STARs, which [T166] will set against each EPA's year target in this same section. (T169)
            column.Item().Element(e => EpaProgressSectionComponent.Compose(e, data.EpaProgress));

            if (data.CommitteeReviews.Count > 0)
            {
                column.Item().Element(e => CommitteeSectionComponent.Compose(e, data.CommitteeReviews));
            }

            if (data.ActivitiesByType.Count > 0)
            {
                column.Item().Element(e => ActivitiesSectionComponent.Compose(e, data.ActivitiesByType, data.SchemaVersions, data.RungLabels));
            }

            if (data.MsfReports.Count > 0)
            {
                column.Item().Element(e => MsfSectionComponent.Compose(e, data.MsfReports));
            }

            if (data.AuditEntries.Count > 0)
            {
                column.Item().Element(e => AuditAppendixComponent.Compose(e, data.AuditEntries));
            }
        });
    }

    // Internal so the tests can see WHICH profile headed the portfolio, which the rendered bytes do not show. (T113)
    internal async Task<PortfolioData> LoadPortfolioDataAsync(PortfolioExportRequest request, CancellationToken cancellationToken)
    {
        // The profile TraineeScopeResolver resolves, which is the one ExportPortfolio authorised the export
        // against and the one each activity's scope stamp was read from. Unordered, this picked an arbitrary
        // profile, so a trainee with two could get a PDF branded and headed by the institution that did NOT
        // grant the access. (T101; one definition since T113)
        var traineeProfile = await TraineeScopeResolver.PreferredProfiles(_dbContext)
            .AsNoTracking()
            .Include(profile => profile.Curriculum)
                .ThenInclude(curriculum => curriculum.SubSpeciality)
                    .ThenInclude(sub => sub.Speciality)
            .Where(profile => profile.UserId == request.TraineeUserId)
            .FirstOrDefaultAsync(cancellationToken);

        // The curriculum is national now (T091); the trainee's institution is held directly on the profile.
        var institution = traineeProfile is not null
            ? await _dbContext.Set<Domain.Institutions.Institution>()
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == traineeProfile.InstitutionId, cancellationToken)
            : null;
        var brand = institution is not null
            ? await _dbContext.Set<InstitutionBrand>()
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.InstitutionId == institution.Id, cancellationToken)
            : null;

        // WhereReadableBy makes the bundle exactly the union of the rows the caller could open one at a
        // time. Authorising the export from the trainee's CURRENT profile while selecting on subject
        // alone let the two disagree in both directions — see PortfolioExportRequest. (T101)
        var activitiesQuery = _dbContext.Set<Activity>()
            .AsNoTracking()
            .Include(activity => activity.ActivityType)
            .Include(activity => activity.Transitions)
            .Where(activity => activity.SubjectUserId == request.TraineeUserId)
            .WhereReadableBy(request.Principal);

        // A portfolio period is a clinical period, not a filing period, so the bundle is cut on the
        // ENCOUNTER date: an assessment of a March encounter belongs in a March-to-June portfolio even if
        // the paperwork landed in September. (T119, decision D2.)
        //
        // The bounds stay DateOnly against a DateOnly column rather than widening to UTC instants — it
        // keeps the index on ObservedOn usable, and TimeOnly.MaxValue was a day-boundary trap besides.
        if (request.FromDate.HasValue)
        {
            var fromDate = request.FromDate.Value;
            activitiesQuery = activitiesQuery.Where(activity => activity.ObservedOn >= fromDate);
        }

        if (request.ToDate.HasValue)
        {
            var toDate = request.ToDate.Value;
            activitiesQuery = activitiesQuery.Where(activity => activity.ObservedOn <= toDate);
        }

        var activities = await activitiesQuery
            .OrderBy(activity => activity.ActivityType.Name)
            // Newest encounter first, to agree with the date the PDF prints beside each activity. (T119)
            .ThenByDescending(activity => activity.ObservedOn)
            .ToListAsync(cancellationToken);

        var schemaVersionIds = activities
            .Select(activity => new { activity.ActivityTypeId, activity.SchemaVersion })
            .Distinct()
            .ToList();

        var schemaVersions = new Dictionary<(int ActivityTypeId, int Version), ActivityTypeVersion>();
        if (schemaVersionIds.Count > 0)
        {
            var versions = await _dbContext.Set<ActivityTypeVersion>()
                .AsNoTracking()
                .Where(version => schemaVersionIds
                    .Select(id => id.ActivityTypeId)
                    .Contains(version.ActivityTypeId))
                .ToListAsync(cancellationToken);

            foreach (var version in versions)
            {
                schemaVersions[(version.ActivityTypeId, version.Version)] = version;
            }
        }

        // T100: a Scale field stores an ordinal. On the CPSA ladder ordinal 5 is rung "4", so a PDF
        // printing the raw number tells the reader the wrong rung. Resolve every ladder the loaded
        // schemas name, once, and hand the map to the composer (which has no database).
        var scaleKeys = new List<string?>();
        foreach (var version in schemaVersions.Values)
        {
            FormSchema schema;
            try
            {
                schema = FormSchemaParser.Parse(version.SchemaJson);
            }
            catch
            {
                continue;
            }

            scaleKeys.AddRange(schema.Sections
                .SelectMany(section => section.Fields)
                .Where(field => field.Type == FieldType.Scale)
                .Select(field => field.ScaleKey));
        }

        var rungLabels = await EntrustmentRungLabels.LoadForScaleKeysAsync(
            _dbContext, scaleKeys, cancellationToken);

        var activitiesByType = activities
            .GroupBy(activity => activity.ActivityType.Name)
            .ToDictionary(group => group.Key, group => group.ToList());

        // T169: "complete" is a terminal state of the activity's PINNED workflow (D44), the point where credit fires,
        // as the sampling report and the trajectory already read it. The literal "completed" printed a trainee's
        // discussed reflective exercises, recorded MSF rows and logged procedures as never finished. The pin is the
        // version row the activity was filed against, else the type's own columns for a type whose version rows were
        // never written, as RatedEvidenceProfiles resolves it. Parsed once per pin.
        var finishedStatesByPin = new Dictionary<(int ActivityTypeId, int Version), IReadOnlySet<string>>();
        bool IsComplete(Activity activity)
        {
            var pin = (activity.ActivityTypeId, activity.SchemaVersion);
            if (!finishedStatesByPin.TryGetValue(pin, out var finished))
            {
                finished = ActivityCompletion.FinishedStates(schemaVersions.TryGetValue(pin, out var version)
                    ? version.WorkflowJson
                    : activity.ActivityType.WorkflowJson);
                finishedStatesByPin[pin] = finished;
            }

            return finished.Contains(activity.CurrentState);
        }

        var typeSummaries = activitiesByType
            // The order the activities section prints the same groups in.
            .OrderBy(pair => pair.Key)
            .Select(pair => new PortfolioTypeSummary(pair.Key, pair.Value.Count, pair.Value.Count(IsComplete)))
            .ToList();

        var epaProgress = await LoadEpaProgressAsync(request, traineeProfile, cancellationToken);

        var committeeReviews = await _dbContext.Set<CommitteeReview>()
            .AsNoTracking()
            .Include(review => review.Decisions)
            .Include(review => review.Panel)
            .Where(review => review.TraineeUserId == request.TraineeUserId)
            .Where(review => review.State == CommitteeReviewState.Ratified || review.State == CommitteeReviewState.Final)
            .OrderByDescending(review => review.ScheduledOn)
            .ToListAsync(cancellationToken);

        // F-5-2 / T077: the trainee's active STARs (entrustment authorisations). These are current
        // standing authorisations, so they are not constrained by the activity date filter.
        var entrustmentDecisions = await _dbContext.Set<EntrustmentDecision>()
            .AsNoTracking()
            .Include(decision => decision.Epa)
            .Include(decision => decision.AuthorisedLevel)
            .Where(decision => decision.TraineeUserId == request.TraineeUserId)
            .Where(decision => decision.Status == EntrustmentDecisionStatus.Active)
            .OrderBy(decision => decision.Epa.Code)
            .ToListAsync(cancellationToken);

        var msfCampaigns = await _dbContext.Set<MsfCampaign>()
            .AsNoTracking()
            .Include(campaign => campaign.Template)
                .ThenInclude(template => template.Questions)
            .Include(campaign => campaign.Invitations)
            .Include(campaign => campaign.Responses)
                .ThenInclude(response => response.Answers)
            // MsfAggregationService's very first act is to group responses by
            // `response.Invitation.RespondentCategory`, and this second Responses chain is what makes
            // that navigation non-null. It was missing, and the export threw a NullReferenceException
            // for any trainee with a released campaign — invisible until now only because no released
            // campaign existed on a database anyone exported from. Found browser-verifying T121; the
            // campaign a release creates is the first one most portfolios will ever hold.
            .Include(campaign => campaign.Responses)
                .ThenInclude(response => response.Invitation)
            // T121: MsfAggregationService reads the coverage to report what the campaign was evidence
            // for. Without this Include the navigation comes back empty and the printed portfolio
            // silently drops the EPA list while the activities section prints the per-EPA records.
            .Include(campaign => campaign.CoveredEpas)
                .ThenInclude(covered => covered.Epa)
            .Where(campaign => campaign.SubjectUserId == request.TraineeUserId)
            .Where(campaign => campaign.State == MsfCampaignState.Released)
            .OrderByDescending(campaign => campaign.ReleasedOn)
            .ToListAsync(cancellationToken);

        // Which declared EPAs each campaign recorded, from its evidence rows, as every other reader of coverage has it
        // (T186). Not the per-EPA stamp, which a campaign released before it existed does not carry.
        var recordedMsfEpas = await MsfCampaignCoverage.RecordedEpasAsync(
            _dbContext,
            request.TraineeUserId,
            msfCampaigns.Select(campaign => (campaign.Id, campaign.State)),
            MsfCampaignCoverage.MsfEvidenceTypeKey,
            cancellationToken);

        var msfReports = msfCampaigns
            .Select(campaign => _msfAggregationService.BuildReport(campaign, recordedMsfEpas[campaign.Id]))
            .ToList();

        var auditEntries = activities
            .SelectMany(activity => activity.Transitions.Select(transition => new AuditEntry(
                activity.ActivityType.Name,
                activity.Id,
                transition.FromState,
                transition.ToState,
                transition.TransitionKey,
                transition.ActorUserId,
                transition.OccurredOn)))
            .OrderBy(entry => entry.OccurredOn)
            .ToList();

        var traineeName = traineeProfile is not null
            ? $"Trainee {request.TraineeUserId}"
            : request.TraineeUserId;

        // Try to load the user's name
        var user = await _dbContext.Set<Infrastructure.Identity.WombatIdentityUser>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.TraineeUserId, cancellationToken);

        if (user is not null)
        {
            traineeName = $"{user.FirstName} {user.LastName}".Trim();
        }

        return new PortfolioData(
            TraineeName: traineeName,
            InstitutionName: institution?.Name ?? "Unknown Institution",
            ProgrammeName: traineeProfile?.Curriculum.Name ?? "Unknown Programme",
            SubSpecialityName: traineeProfile?.Curriculum.SubSpeciality.Name,
            SpecialityName: traineeProfile?.Curriculum.SubSpeciality.Speciality.Name,
            FromDate: request.FromDate,
            ToDate: request.ToDate,
            Brand: brand,
            Activities: activities,
            ActivitiesByType: activitiesByType,
            TypeSummaries: typeSummaries,
            SchemaVersions: schemaVersions,
            RungLabels: rungLabels,
            EpaProgress: epaProgress,
            CommitteeReviews: committeeReviews,
            EntrustmentDecisions: entrustmentDecisions,
            MsfReports: msfReports,
            AuditEntries: auditEntries);
    }

    /// <summary>
    /// The per-EPA section's data (T169), from the two reads behind the trainee's progress page: the quota reader for the
    /// targets and the trajectory for the rated observations. Neither is re-derived here.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Which programme.</b> <paramref name="profile" />, the one the cover names: <c>TraineeScopeResolver</c>'s
    /// preferred profile, which the export was authorised against. The progress page reads the active profile only, so a
    /// completed or deactivated trainee has no targets there. A graduation export is exactly the one that must show
    /// them, so the section reads the cover's profile whatever its state, and says what that state is.
    /// </para>
    /// <para>
    /// <b>Which day.</b> <see cref="PortfolioEpaProgress.ReadOn" />: the export's last day, or today on the South African
    /// calendar when the export is open-ended or ends in the future (a future day would read a period that has not
    /// happened), and never after a completed programme's completion day. Every window from that day's back to the one
    /// containing the export's first day is listed (the whole programme when there is none), so an annual export shows
    /// both of that year's semesters and a graduation export every one.
    /// </para>
    /// <para>
    /// <b>Scope.</b> The export was authorised for this trainee before this runs (<c>ExportPortfolio</c>, or the data
    /// subject's own access request). The quota reader reads progress rows, never an activity, and every figure on it is
    /// about this trainee, as on the progress page and the committee's dashboards. The trajectory reads activities
    /// through the caller's read scope and the export's dates, the same cut as the activity list below it, so every
    /// rated observation counted here is one the PDF lists.
    /// </para>
    /// </remarks>
    private async Task<PortfolioEpaProgress> LoadEpaProgressAsync(
        PortfolioExportRequest request,
        TraineeProfile? profile,
        CancellationToken cancellationToken)
    {
        var today = ProgrammeCalendar.DateOf(_timeProvider.GetUtcNow().UtcDateTime);
        var programme = PortfolioProgramme.Of(profile);
        var asOf = PortfolioEpaProgress.ReadOn(today, request.ToDate, programme);

        var targets = profile is null
            ? null
            : await TraineeQuotaProgressReader.ReadForProfileAsync(
                _dbContext,
                profile,
                asOf,
                periodsFrom: request.FromDate ?? DateOnly.MinValue,
                cancellationToken);

        // The handler itself, not a copy of it: the chart on the progress page and this section cannot disagree.
        var trajectories = await new GetEpaTrajectoryForTraineeQueryHandler(_dbContext).Handle(
            new GetEpaTrajectoryForTraineeQuery(request.TraineeUserId, request.Principal, request.FromDate, request.ToDate),
            cancellationToken);

        // Of the EPAs with rated evidence and no target, the ones this programme holds the trainee's item for whose EPA
        // is deactivated (T158). The reader's owner predicate: another institution's local item is not this trainee's.
        var targeted = targets?.Items.Select(item => item.EpaId).ToHashSet() ?? [];
        var untargeted = trajectories
            .Select(trajectory => trajectory.EpaId)
            .Where(epaId => !targeted.Contains(epaId))
            .ToArray();
        var deactivated = profile is null || untargeted.Length == 0
            ? []
            : await _dbContext.Set<CurriculumItem>()
                .AsNoTracking()
                .NotInForce()
                .Where(item => item.CurriculumId == profile.CurriculumId
                    && (item.OwningInstitutionId == null || item.OwningInstitutionId == profile.InstitutionId)
                    && untargeted.Contains(item.EpaId))
                .Select(item => item.EpaId)
                .ToListAsync(cancellationToken);

        return PortfolioEpaProgress.Build(
            asOf,
            today,
            request.FromDate,
            programme,
            targets,
            trajectories,
            deactivated.ToHashSet());
    }
}

internal sealed record PortfolioData(
    string TraineeName,
    string InstitutionName,
    string ProgrammeName,
    string? SubSpecialityName,
    string? SpecialityName,
    DateOnly? FromDate,
    DateOnly? ToDate,
    InstitutionBrand? Brand,
    List<Activity> Activities,
    Dictionary<string, List<Activity>> ActivitiesByType,
    IReadOnlyList<PortfolioTypeSummary> TypeSummaries,
    Dictionary<(int ActivityTypeId, int Version), ActivityTypeVersion> SchemaVersions,
    EntrustmentRungLookup RungLabels,
    PortfolioEpaProgress EpaProgress,
    List<CommitteeReview> CommitteeReviews,
    List<EntrustmentDecision> EntrustmentDecisions,
    List<MsfCampaignAggregateReportDto> MsfReports,
    List<AuditEntry> AuditEntries);

internal sealed record AuditEntry(
    string ActivityTypeName,
    int ActivityId,
    string FromState,
    string ToState,
    string TransitionKey,
    string ActorUserId,
    DateTime OccurredOn);
