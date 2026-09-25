using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Activities.Services;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Activities;
using Wombat.Domain.Curricula;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Activities.Queries.GetEpaTrajectoryForTrainee;

/// <summary>
/// One trainee's entrustment trajectory: extracted ratings, each carrying the activity id it came
/// from and the assessor who gave it.
/// </summary>
/// <remarks>
/// <see cref="Principal" /> sits second rather than last because the optional date bounds must keep
/// their defaults, and a parameter that decides what a caller may see is not one to leave optional.
/// Without it this query answered on a caller-supplied trainee id alone — ratings and deep-linkable
/// activity ids for any trainee named. (T101)
/// </remarks>
public sealed record GetEpaTrajectoryForTraineeQuery(
    string TraineeUserId,
    ClaimsPrincipal Principal,
    DateOnly? From = null,
    DateOnly? To = null) : IRequest<IReadOnlyList<EpaTrajectoryDto>>;

public sealed class GetEpaTrajectoryForTraineeQueryValidator
    : AbstractValidator<GetEpaTrajectoryForTraineeQuery>
{
    public GetEpaTrajectoryForTraineeQueryValidator()
    {
        RuleFor(query => query.TraineeUserId).NotEmpty();
        RuleFor(query => query)
            .Must(query => !query.From.HasValue || !query.To.HasValue || query.From.Value <= query.To.Value)
            .WithMessage("From must be on or before To.");
    }
}

public sealed record EpaTrajectoryDto(
    int EpaId,
    string EpaCode,
    string EpaTitle,
    /// <summary>
    /// The ladder this EPA's ratings are read against — the scale the trainee's curriculum item for it
    /// is pinned to (T109). Null when the item is unpinned, which is a permanent and meaningful state.
    /// </summary>
    int? ScaleId,
    string? ScaleName,
    /// <summary>
    /// The rungs of the pinned scale, in order. Empty when nothing resolves, in which case the chart
    /// falls back to the numeric axis it drew before T123.
    /// </summary>
    IReadOnlyList<TrajectoryRungDto> Rungs,
    IReadOnlyList<TrajectoryPointDto> Points);

/// <summary>
/// One rung of the ladder a trajectory is plotted against. Order is the stored ordinal and the position
/// on the axis; Label is the rung as the College prints it. On the CPSA v11.1 ladder they are different
/// numbers — Order 5 is rung "4" — which is why the chart cannot label its own axis from ordinals. (T100)
/// </summary>
public sealed record TrajectoryRungDto(int Order, string Label);

public sealed record TrajectoryPointDto(
    int ActivityId,
    DateOnly ObservedOn,
    /// <summary>
    /// False when nobody stated when the encounter happened and <see cref="ObservedOn" /> is only the day the activity
    /// was created (<c>ObservedOnSource == CreatedOn</c>). The point still sits there, the same date the window selects
    /// on, but the chart must not present it as a clinical date (T161, D28; <c>EncounterDate.Label</c>). No default,
    /// so a new call site cannot silently report every point as dated.
    /// </summary>
    bool ObservedOnDeclared,
    int Rating,
    /// <summary>
    /// Rating rendered as a rung on the EPA's pinned scale, or the bare ordinal when it resolves to
    /// nothing — including when the ordinal is not a rung on that ladder at all, which is how a rating
    /// recorded against a different scale shows up here.
    /// </summary>
    string RatingLabel,
    string Source,
    string AssessorUserId,
    /// <summary>
    /// The rating was recorded against a DIFFERENT ladder from the one this EPA's axis is drawn from,
    /// so its ordinal does not mean on this chart what it meant on the form. Drawn hollow and left out
    /// of the polyline. (T123 D30, made computable by T126)
    /// </summary>
    /// <remarks>
    /// False is not "on the ladder" — it is "no disagreement established". It covers a point whose own
    /// ladder is unknown, which is the common case while four generic seeds still declare the
    /// unresolvable <c>or_scale</c> (T110) and while types published before T126 carry no
    /// <c>rated_level_field</c>. Marking those would assert a conflict nothing has shown.
    /// </remarks>
    bool OffLadder = false);

public sealed class GetEpaTrajectoryForTraineeQueryHandler
    : IRequestHandler<GetEpaTrajectoryForTraineeQuery, IReadOnlyList<EpaTrajectoryDto>>
{
    // Which types are rated, and what evidence they are, now has ONE answer for the whole
    // repository: RatedActivityTypes (T134). This file used to keep its own family map and the
    // committee sampling report kept a second, EXACT-key one — which is why every seeded CPSA tool
    // charted here and counted as no evidence at all there. Both maps are gone; the surviving one
    // moved into the shared classifier unchanged.
    //
    // The KNOWN LIMITATION this carried for months — "an institution that builds its own rated tool
    // under an unfamiliar key will not chart" — is CLOSED. The gate is now the type's own declared
    // rated field (T126), so any rated tool charts whatever it is called. Its evidence SOURCE is the
    // category of the instrument it declares (WbaToolKey, T144), so a builder-made Mini-CEX under an
    // unfamiliar key reads as Direct observation. A type reads as its raw key only when its instrument
    // has no category, or when it declares none and its key matches no known family.

    private readonly IApplicationDbContext _dbContext;

    public GetEpaTrajectoryForTraineeQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EpaTrajectoryDto>> Handle(
        GetEpaTrajectoryForTraineeQuery request,
        CancellationToken cancellationToken)
    {
        var traineeUserId = request.TraineeUserId.Trim();

        var query = _dbContext.Set<Activity>()
            .AsNoTracking()
            .Where(activity => activity.SubjectUserId == traineeUserId)
            // The trainee id is whatever the caller asked about, so the rows are cut down to what
            // this caller may read: a trainee asking about themselves matches on SubjectUserId,
            // anyone else must oversee the programme each activity is stamped to. Out of scope
            // draws an empty chart rather than raising — a refusal would confirm the trainee. (T101)
            .WhereReadableBy(request.Principal);

        // The window selects on the ENCOUNTER date, not the filing date, so an assessment of a March
        // encounter filed in September is inside a March window and outside a September one. (T119)
        //
        // The bounds stay DateOnly and compare against a DateOnly column rather than being widened to
        // UTC instants: it keeps the index on ObservedOn usable, and it removes the boundary defect the
        // instants had — an activity created 00:30 SAST fell on the previous UTC day and dropped out of
        // a window that began that morning.
        if (request.From.HasValue)
        {
            var from = request.From.Value;
            query = query.Where(activity => activity.ObservedOn >= from);
        }

        if (request.To.HasValue)
        {
            var to = request.To.Value;
            query = query.Where(activity => activity.ObservedOn <= to);
        }

        // What a rating is, and which rows hold one, is AttributedRatings' answer, shared with the entrustment
        // standing (T166): a terminal state of the pinned workflow (D44), the pinned schema's declared rated field, the
        // stamped EPA (T137), and the assessor RatedEvidenceProfile names (T135, T150). The rows it reads are the ones
        // this handler confined above, so the read rule is applied here, where the boundary test can see it.
        var ratings = await AttributedRatings.ReadAsync(_dbContext, query, cancellationToken);

        // The x-axis is the encounter date the clinician stated, which is what this chart has always claimed to plot
        // and never did: it plotted CreatedOn, the audit clock. (T119) Where nobody stated one (ObservedOnSource ==
        // CreatedOn) the point still sits on the day it was created, and says so: the chart's tooltip and table mark it
        // as undated evidence (T161, D28, T119 D4). RatingLabel is filled in below, once the EPA's pinned ladder is known.
        var rawPoints = ratings
            .Select(rating => (rating.EpaId, Point: new TrajectoryPointDto(
                rating.ActivityId,
                rating.ObservedOn,
                rating.ObservedOnDeclared,
                rating.Rating,
                rating.Rating.ToString(),
                rating.Source,
                rating.AssessorUserId)))
            .ToList();
        var ratedScaleIdByActivity = ratings
            .Where(rating => rating.RatedScaleId.HasValue)
            .ToDictionary(rating => rating.ActivityId, rating => rating.RatedScaleId!.Value);

        if (rawPoints.Count == 0)
        {
            return Array.Empty<EpaTrajectoryDto>();
        }

        var epaIds = rawPoints.Select(entry => entry.EpaId).Distinct().ToArray();
        var epas = await _dbContext.Set<Epa>()
            .AsNoTracking()
            .Where(epa => epaIds.Contains(epa.Id))
            .ToDictionaryAsync(epa => epa.Id, cancellationToken);

        var ladders = await ResolvePinnedLaddersAsync(traineeUserId, epaIds, cancellationToken);

        return rawPoints
            .GroupBy(entry => entry.EpaId)
            .Where(group => epas.ContainsKey(group.Key))
            .Select(group =>
            {
                var epa = epas[group.Key];
                ladders.ScaleIdByEpa.TryGetValue(epa.Id, out var scaleId);
                var points = group
                    .Select(entry =>
                    {
                        // Off-ladder is asserted only when BOTH sides are known and they disagree. An
                        // unresolvable point key (or_scale, T110) or a type predating T126 leaves it
                        // alone: a hollow dot says "this was measured on something else", and saying so
                        // without evidence is worse than the numeric axis this chart already drew.
                        var offLadder = scaleId is not null
                            && ratedScaleIdByActivity.TryGetValue(entry.Point.ActivityId, out var ownScaleId)
                            && ownScaleId != scaleId.Value;

                        return entry.Point with
                        {
                            RatingLabel = ladders.Rungs.Format(scaleId, entry.Point.Rating),
                            OffLadder = offLadder
                        };
                    })
                    .OrderBy(point => point.ObservedOn)
                    .ThenBy(point => point.ActivityId)
                    .ToArray();

                var rungs = ladders.Rungs.RungsOf(scaleId)
                    .Select(rung => new TrajectoryRungDto(rung.Order, rung.Label))
                    .ToArray();

                return new EpaTrajectoryDto(
                    epa.Id,
                    epa.Code,
                    epa.Title,
                    scaleId,
                    scaleId.HasValue && ladders.ScaleNameById.TryGetValue(scaleId.Value, out var name)
                        ? name
                        : null,
                    rungs,
                    points);
            })
            .OrderBy(dto => dto.EpaCode, StringComparer.Ordinal)
            .ToArray();
    }

    private sealed record PinnedLadders(
        Dictionary<int, int?> ScaleIdByEpa,
        Dictionary<int, string> ScaleNameById,
        EntrustmentRungLookup Rungs);

    /// <summary>
    /// The entrustment ladder each EPA's ratings should be read against, for this trainee.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The source is <c>CurriculumItem.ScaleId</c> — T109's per-item pin, which is literally "the scale
    /// this EPA's minima are expressed on" for this trainee's curriculum. It is deliberately NOT
    /// <c>Curriculum.SubSpeciality.DefaultEntrustmentScaleId</c>, which two neighbouring queries do read
    /// and which <c>CurriculumItem</c>'s own remarks reject as a source of pins, because it is a
    /// programme-wide committee-picker default an administrator may change at any time (T187), not the
    /// ladder any one EPA's minima were written on.
    /// </para>
    /// <para>
    /// The profile is the trainee's preferred one (<see cref="TraineeScopeResolver.PreferredProfiles" />:
    /// the active one, else the highest id), NOT filtered on <c>IsActive</c> — the one pick the credit
    /// engine, the EPA picker and the export make (T185), so the chart and credit cannot disagree about
    /// which profile row is in force, and a graduated trainee still has a trajectory worth reading. Until
    /// T185 this broke ties by the latest <c>ProgrammeStartDate</c> with no final tie-break at all.
    /// <c>GetCurriculumProgressForTrainee</c>, the other query on the same page, reads the same profile
    /// but only while it is active: it reads today's targets, and a completed programme has none.
    /// </para>
    /// <para>
    /// Every step is permissive: no profile, no curriculum item for the EPA, or an unpinned item all
    /// yield no ladder, and the chart then renders exactly as it did before T123.
    /// </para>
    /// <para>
    /// What this does NOT do is resolve the ladder each plotted ACTIVITY was recorded against, which is
    /// what lets a point on a different scale be drawn as such. That is
    /// <see cref="AttributedRating.RatedScaleId" /> (T126), from the pinned version's declared rated field, the
    /// same field the rating itself is read from (<see cref="RatedEvidenceProfile" />, T135).
    /// </para>
    /// </remarks>
    private async Task<PinnedLadders> ResolvePinnedLaddersAsync(
        string traineeUserId,
        int[] epaIds,
        CancellationToken cancellationToken)
    {
        var none = new PinnedLadders([], [], EntrustmentRungLookup.Empty);

        var profile = await TraineeScopeResolver.PreferredProfiles(_dbContext)
            .AsNoTracking()
            .Where(entity => entity.UserId == traineeUserId)
            .Select(entity => new { entity.CurriculumId, entity.InstitutionId })
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
        {
            return none;
        }

        // The owner predicate is not optional. A national curriculum row is SHARED by every adopting
        // institution (AdoptCurriculumCommandHandler never clones it), and a national item never shares
        // an EPA with an institution's own (T223), so an institution-local item added by institution B
        // has no national row beside it for that EPA. Without this, a trainee at institution A would take B's ladder as
        // their chart's axis — and then have their own correctly-rated observations marked "not a rung
        // on this scale" against a ladder that was never theirs.
        var pins = await _dbContext.Set<CurriculumItem>()
            .AsNoTracking()
            .Where(item => item.CurriculumId == profile.CurriculumId
                && (item.OwningInstitutionId == null || item.OwningInstitutionId == profile.InstitutionId)
                && epaIds.Contains(item.EpaId))
            .Select(item => new { item.EpaId, item.ScaleId })
            .ToListAsync(cancellationToken);

        if (pins.Count == 0)
        {
            return none;
        }

        var scaleIdByEpa = new Dictionary<int, int?>(pins.Count);
        foreach (var pin in pins)
        {
            scaleIdByEpa[pin.EpaId] = pin.ScaleId;
        }

        var rungs = await EntrustmentRungLabels.LoadAsync(
            _dbContext, scaleIdByEpa.Values, cancellationToken);

        var pinnedScaleIds = scaleIdByEpa.Values
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToArray();

        var scaleNameById = pinnedScaleIds.Length == 0
            ? []
            : await _dbContext.Set<EntrustmentScale>()
                .AsNoTracking()
                .Where(scale => pinnedScaleIds.Contains(scale.Id))
                .ToDictionaryAsync(scale => scale.Id, scale => scale.Name, cancellationToken);

        return new PinnedLadders(scaleIdByEpa, scaleNameById, rungs);
    }
}
