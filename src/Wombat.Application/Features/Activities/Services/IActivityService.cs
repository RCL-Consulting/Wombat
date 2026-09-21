using System.Security.Claims;
using Wombat.Application.Features.Activities.Dtos;

namespace Wombat.Application.Features.Activities.Services;

public interface IActivityService
{
    Task<ActivityDto> CreateDraftAsync(CreateActivityInput input, CancellationToken cancellationToken = default);
    Task<ActivityDto> UpdateDraftAsync(UpdateActivityDraftInput input, CancellationToken cancellationToken = default);
    Task<ActivityDto> TransitionAsync(TransitionActivityInput input, CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds a batch of activities, drives each straight to the terminal state named by a single
    /// transition, and <b>stages them in the caller's unit of work without saving</b>. Returns how many
    /// were staged. (T121)
    /// </summary>
    /// <remarks>
    /// <para>
    /// For evidence a feature outside the activity platform has already collected and closed, where
    /// there is no draft for anyone to fill in and no intermediate state to sit in. The multi-source
    /// feedback release is the first caller: one record per EPA the campaign covered.
    /// </para>
    /// <para>
    /// <b>Nothing touches the DbContext until every payload has passed.</b> That ordering is the whole
    /// contract, and it is not a tidiness preference. <c>AuditWriter</c> shares the request's scoped
    /// <c>IApplicationDbContext</c> and calls <c>SaveChangesAsync</c>, and
    /// <c>AuditPipelineBehavior</c> writes an audit row from its <c>catch</c> — so <b>any</b> exception
    /// thrown while the context holds a half-finished mutation flushes that mutation to the database on
    /// the way out. A release that failed validation would have committed "Released" with no evidence,
    /// and <c>MsfCampaign.Release</c> refuses a second attempt, so there would be no way back.
    /// </para>
    /// <para>
    /// It therefore does not save either: the caller mutates its own aggregate after this returns and
    /// commits everything in one <c>SaveChangesAsync</c>. Calling <c>CreateDraftAsync</c> and
    /// <c>TransitionAsync</c> in a loop instead would be wrong for the same reason — they each save, so
    /// the first iteration would commit the caller's pending work and expose the rest of the batch.
    /// </para>
    /// <para>
    /// <b>It refuses a type that declares credit</b>, and that refusal is deliberate rather than a gap.
    /// The credit key is <c>"{activity.Id}:{transitionKey}"</c>, so credit cannot be applied before the
    /// ids exist, which cannot happen before a save, which is exactly what this method does not do.
    /// A crediting system-written type needs a two-phase design; when one exists, it can be designed
    /// against a real caller instead of guessed at.
    /// </para>
    /// <para>
    /// Everything else is the ordinary path, deliberately: the same subject-scope stamp (T101), the same
    /// field-permission filter (T070), the same schema validation, the same observation-date stamp
    /// (T119), the same <c>ApplyTransition</c>.
    /// </para>
    /// </remarks>
    Task<int> StageCompletedAsync(
        RecordCompletedActivitiesInput input,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// The activity plus this actor's writable field set and available transitions (T070), or null
    /// when this actor may not read it (T101).
    /// </summary>
    /// <remarks>
    /// Null means either "no such activity" or "not yours" — deliberately the same answer, so that
    /// walking the id space discloses nothing. Callers must render it as not-found and must not
    /// translate it into a message that distinguishes the two cases.
    /// </remarks>
    Task<ActivityDetailDto?> GetDetailAsync(int activityId, ClaimsPrincipal principal, CancellationToken cancellationToken = default);
}
