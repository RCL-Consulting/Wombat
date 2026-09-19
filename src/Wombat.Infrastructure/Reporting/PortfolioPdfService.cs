using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Features.Reporting;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Institutions;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Application.Features.Activities.Queries;

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

    public PortfolioPdfService(IApplicationDbContext dbContext, IMsfAggregationService msfAggregationService)
    {
        _dbContext = dbContext;
        _msfAggregationService = msfAggregationService;
    }

    public async Task<PortfolioExportResult> GenerateAsync(PortfolioExportRequest request, CancellationToken cancellationToken)
    {
        var data = await LoadPortfolioDataAsync(request, cancellationToken);

        var document = Document.Create(container =>
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

        var pdfBytes = document.GeneratePdf();

        var hash = Convert.ToHexStringLower(SHA256.HashData(pdfBytes));

        var fileName = $"portfolio-{hash[..12]}.pdf";

        return new PortfolioExportResult(pdfBytes, fileName, hash);
    }

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

            if (data.CommitteeReviews.Count > 0)
            {
                column.Item().Element(e => CommitteeSectionComponent.Compose(e, data.CommitteeReviews));
            }

            if (data.ActivitiesByType.Count > 0)
            {
                column.Item().Element(e => ActivitiesSectionComponent.Compose(e, data.ActivitiesByType, data.SchemaVersions));
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

    private async Task<PortfolioData> LoadPortfolioDataAsync(PortfolioExportRequest request, CancellationToken cancellationToken)
    {
        var traineeProfile = await _dbContext.Set<Domain.Identity.TraineeProfile>()
            .AsNoTracking()
            .Include(profile => profile.Curriculum)
                .ThenInclude(curriculum => curriculum.SubSpeciality)
                    .ThenInclude(sub => sub.Speciality)
            .Where(profile => profile.UserId == request.TraineeUserId)
            // Same tie-break as ExportPortfolio.ResolveTraineeScopeAsync and
            // ActivityService.ResolveSubjectScopeAsync. Unordered, this picked an arbitrary profile, so
            // a trainee with two could get a PDF branded and headed by the institution that did NOT
            // grant the access. (T101)
            .OrderByDescending(profile => profile.IsActive)
            .ThenByDescending(profile => profile.Id)
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

        var activitiesByType = activities
            .GroupBy(activity => activity.ActivityType.Name)
            .ToDictionary(group => group.Key, group => group.ToList());

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
            .Where(campaign => campaign.SubjectUserId == request.TraineeUserId)
            .Where(campaign => campaign.State == MsfCampaignState.Released)
            .OrderByDescending(campaign => campaign.ReleasedOn)
            .ToListAsync(cancellationToken);

        var msfReports = msfCampaigns
            .Select(campaign => _msfAggregationService.BuildReport(campaign))
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
            SchemaVersions: schemaVersions,
            CommitteeReviews: committeeReviews,
            EntrustmentDecisions: entrustmentDecisions,
            MsfReports: msfReports,
            AuditEntries: auditEntries);
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
    Dictionary<(int ActivityTypeId, int Version), ActivityTypeVersion> SchemaVersions,
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
