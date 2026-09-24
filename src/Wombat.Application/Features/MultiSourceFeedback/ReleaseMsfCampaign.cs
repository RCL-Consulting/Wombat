using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Wombat.Application.Audit;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.MultiSourceFeedback;

/// <remarks>
/// <c>Narrative</c> is redacted from the audit summary. The AuditPipelineBehavior audits every
/// request whose type name ends in "Command" and AuditPayloadSerializer writes its properties into
/// SummaryJson; this is the reviewer's written summary of what colleagues said about a named
/// trainee, up to 4000 characters of it, and it does not belong in an admin-searchable table. The
/// campaign and reviewer ids stay in the clear — who released what, and when. (T101)
/// <para>
/// <c>EntrustmentLevel</c> is NOT redacted, and deliberately: it is a single ordinal a named clinician
/// puts their name to, it is released to the trainee, and who asserted what level and when is exactly
/// the kind of thing an audit trail is for.
/// </para>
/// </remarks>
public sealed record ReleaseMsfCampaignCommand(
    int CampaignId,
    string ReviewerUserId,
    [property: Redact] string? Narrative,
    int? EntrustmentLevel,
    ClaimsPrincipal Principal) : IRequest;

public sealed class ReleaseMsfCampaignCommandValidator : AbstractValidator<ReleaseMsfCampaignCommand>
{
    public ReleaseMsfCampaignCommandValidator()
    {
        RuleFor(command => command.CampaignId).GreaterThan(0);
        RuleFor(command => command.ReviewerUserId).NotEmpty();
        RuleFor(command => command.Narrative).MaximumLength(4000);

        // A bound, not a ladder. The rung count is data — an entrustment scale is a seeded table — so
        // the real check is the pinned schema's min/max on `overall_level`, which SchemaValidator applies
        // when the evidence activity is built. This only rejects a value that cannot be an ordinal.
        RuleFor(command => command.EntrustmentLevel)
            .GreaterThanOrEqualTo(1)
            .When(command => command.EntrustmentLevel.HasValue);
    }
}

public sealed class ReleaseMsfCampaignCommandHandler : IRequestHandler<ReleaseMsfCampaignCommand>
{
    /// <summary>
    /// The seeded activity type that carries a released campaign's evidence, one row per covered EPA.
    /// </summary>
    private const string MsfActivityTypeKey = "msf_cpsa";

    /// <summary>The single transition out of that type's draft state, into its terminal one.</summary>
    private const string RecordTransitionKey = "record";

    private readonly IApplicationDbContext _dbContext;
    private readonly IMsfAggregationService _aggregationService;
    private readonly IActivityService _activityService;
    private readonly IActivityReferenceDataService _referenceDataService;
    private readonly ILogger<ReleaseMsfCampaignCommandHandler> _logger;

    public ReleaseMsfCampaignCommandHandler(
        IApplicationDbContext dbContext,
        IMsfAggregationService aggregationService,
        IActivityService activityService,
        IActivityReferenceDataService referenceDataService,
        ILogger<ReleaseMsfCampaignCommandHandler> logger)
    {
        _dbContext = dbContext;
        _aggregationService = aggregationService;
        _activityService = activityService;
        _referenceDataService = referenceDataService;
        _logger = logger;
    }

    public async Task Handle(ReleaseMsfCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await MsfCampaignRules.GetCampaignGraphAsync(_dbContext, request.CampaignId, cancellationToken);

        await MsfCampaignRules.EnsureSubjectIsInScopeAsync(
            _dbContext, request.Principal, campaign.SubjectUserId, cancellationToken);

        var report = _aggregationService.BuildReport(campaign);
        if (!report.ReadyForRelease)
        {
            throw new InvalidOperationException(
                report.TotalResponses < report.MinimumResponses
                    ? "The campaign cannot be released until the minimum response count is met."
                    : $"The campaign cannot be released until at least {report.MinimumRespondentCategories} " +
                      $"respondent categories report; only {report.SurvivingCategoryCount} did.");
        }

        // EVERYTHING that can throw happens before the campaign is touched, and that ordering is the
        // point rather than a style. AuditWriter shares this scoped DbContext and saves, and
        // AuditPipelineBehavior writes an audit row from its catch - so an exception raised while
        // `State = Released` is pending on the tracked aggregate would COMMIT that release on the way
        // out, leaving a campaign released to the trainee with no evidence and no second attempt
        // possible, because MsfCampaign.Release refuses anything but UnderReview.
        var staged = await StageEvidenceAsync(campaign, request, cancellationToken);

        var utcNow = DateTime.UtcNow;
        campaign.Release(request.ReviewerUserId, request.Narrative, request.EntrustmentLevel, utcNow);

        if (staged > 0)
        {
            campaign.EvidenceRecordedOn = utcNow;
        }

        // One commit: the release, and the evidence it produced, or neither.
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Stages one terminal <c>msf_cpsa</c> activity per covered EPA that is still on the subject's
    /// curriculum, without saving. Returns how many. (T121)
    /// </summary>
    /// <remarks>
    /// <para>
    /// One activity per EPA rather than one naming many, because <c>curriculum_item_match</c> reads a
    /// single integer out of <c>DataJson</c>. A list would need a new match rule - a DSL change carrying
    /// the Parse-and-Serialize trap - for no gain, and per-EPA rows are what a committee wants anyway:
    /// each EPA's evidence standing on its own.
    /// </para>
    /// <para>
    /// Nothing respondent-identifying goes into <c>DataJson</c>, ever. No email, no per-respondent
    /// comment, no per-category breakdown. <c>AccessReportBuilder</c> puts an activity's whole
    /// <c>DataJson</c> into the subject's data-subject access report (T112), and the portfolio PDF
    /// prints it. The payload is therefore built from campaign fields and from the request, never from
    /// the aggregate report this handler is also holding - that object contains every verbatim comment.
    /// </para>
    /// <para>
    /// Per College decision D8 the seed ships <c>"counts_for": []</c>: MSF is required evidence tracked
    /// in its own right and consumes none of Annexure A's 55 encounters. So no
    /// <c>CurriculumItemProgress</c> row moves and <c>ActivityTransition.CreditedItemCount</c> stays
    /// null - T108's three-valued contract for "credit was never evaluated".
    /// </para>
    /// </remarks>
    private async Task<int> StageEvidenceAsync(
        MsfCampaign campaign,
        ReleaseMsfCampaignCommand request,
        CancellationToken cancellationToken)
    {
        if (campaign.EvidenceRecordedOn is not null)
        {
            _logger.LogInformation(
                "MSF campaign {CampaignId} already recorded its evidence on {RecordedOn}; not repeating the fan-out.",
                campaign.Id,
                campaign.EvidenceRecordedOn);
            return 0;
        }

        // Re-validated rather than trusted from creation: a trainee may have been moved between curricula
        // while the response window was open. An EPA that has left the curriculum is DROPPED with a log
        // line, not thrown on - a release must not fail because an administrator moved someone, and the
        // feedback is still released to the trainee either way.
        var onCurriculum = (await _referenceDataService
                .GetSubjectCurriculumEpaOptionsAsync(campaign.SubjectUserId, cancellationToken))
            .Select(option => int.Parse(option.Value, CultureInfo.InvariantCulture))
            .ToHashSet();

        var declared = campaign.CoveredEpas.Select(covered => covered.EpaId).Distinct().OrderBy(id => id).ToArray();
        var covered = declared.Where(onCurriculum.Contains).ToArray();

        foreach (var dropped in declared.Except(covered))
        {
            _logger.LogWarning(
                "MSF campaign {CampaignId} declared EPA {EpaId} but it is no longer on the curriculum of " +
                "{SubjectUserId}; no evidence recorded for it.",
                campaign.Id,
                dropped,
                campaign.SubjectUserId);
        }

        if (covered.Length == 0)
        {
            _logger.LogWarning(
                "MSF campaign {CampaignId} released with no coverable EPA, so it recorded no evidence.",
                campaign.Id);
            return 0;
        }

        var payloads = covered
            .Select(epaId => BuildEvidenceDataJson(campaign, request, epaId))
            .ToArray();

        var staged = await _activityService.StageCompletedAsync(
            new RecordCompletedActivitiesInput(
                MsfActivityTypeKey,
                campaign.SubjectUserId,
                request.ReviewerUserId,
                RecordTransitionKey,
                payloads,
                request.Principal),
            cancellationToken);

        // Marked per EPA, not only on the campaign: declaring coverage and recording evidence are
        // different facts, and a release honours the first for EPAs it cannot honour the second for.
        // Without this the committee snapshot would list every declared EPA as "recorded as one
        // activity each", including the ones that were dropped.
        var recordedOn = DateTime.UtcNow;
        foreach (var epa in campaign.CoveredEpas.Where(entry => covered.Contains(entry.EpaId)))
        {
            epa.RecordedOn = recordedOn;
        }

        return staged;
    }

    /// <summary>
    /// The one evidence payload for one EPA. Reads the narrative and the level from the REQUEST
    /// rather than from the campaign, because the campaign has deliberately not been released yet
    /// when this runs.
    /// </summary>
    private static string BuildEvidenceDataJson(
        MsfCampaign campaign,
        ReleaseMsfCampaignCommand request,
        int epaId)
    {
        var data = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["epa_id"] = epaId,
            ["campaign_id"] = campaign.Id,

            // The day the window actually shut, not the release date: that is when the evidence was
            // complete. It is what Activity.ObservedOn is stamped from (T119) and what [T130]'s period
            // resolver will bucket on, so it has to be the clinical date rather than the paperwork one.
            ["observed_on"] = EvidenceCompleteOn(campaign).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            ["respondent_count"] = campaign.Responses.Count
        };

        if (request.EntrustmentLevel is int level)
        {
            data["overall_level"] = level;
        }

        if (!string.IsNullOrWhiteSpace(request.Narrative))
        {
            data["summary"] = request.Narrative.Trim();
        }

        return JsonSerializer.Serialize(data);
    }

    /// <summary>
    /// The day the response window actually shut.
    /// </summary>
    /// <remarks>
    /// <c>ClosedOn</c> rather than <c>ClosesOn</c>, because a coordinator can close a campaign early
    /// and <c>ClosesOn</c> is only the date it was scheduled to shut. Dating the evidence from the
    /// schedule would stamp a FUTURE <c>ObservedOn</c> on a campaign closed ahead of time, which would
    /// put it outside the committee review window it belongs to and into the wrong period for
    /// [T130]'s quota. The fallback cannot be reached today - release refuses anything but
    /// <c>UnderReview</c>, which only <c>Close</c> produces - and is here so that stops being an
    /// assumption. <c>StartCommitteeReview</c> windows the campaign itself by the same UTC date, so a
    /// panel sees the report and these activities together; change one and you must change the other.
    /// </remarks>
    private static DateOnly EvidenceCompleteOn(MsfCampaign campaign)
        => campaign.ClosedOn is DateTime closedOn
            ? DateOnly.FromDateTime(closedOn)
            : campaign.ClosesOn;
}
