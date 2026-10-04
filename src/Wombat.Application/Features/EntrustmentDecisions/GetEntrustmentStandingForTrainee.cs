using System.Security.Claims;
using FluentValidation;
using MediatR;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.Curricula;

namespace Wombat.Application.Features.EntrustmentDecisions;

/// <summary>
/// Where a trainee stands on each EPA of their curriculum: the active STAR decision against Annexure A's target for their
/// training year and against the exit rule, beside the latest rating a named assessor gave. For the committee's review
/// page and the trainee's own progress page. (T166)
/// </summary>
/// <remarks>
/// <para>
/// <b>Read-only, and gates nothing.</b> Whether recording a Graduate decision, or completing a programme, should be
/// refused or need a recorded reason when the exit rule is unmet is an operator decision T166 left open. The
/// recommendation is to keep this informational until the College confirms the exit rule; nothing consults it.
/// </para>
/// <para>
/// <b>Who may ask:</b> the trainee themselves, a global Administrator, or someone who oversees the trainee's programme
/// (<see cref="TraineeScopeResolver.MayReadAsync" />, T113). Anyone else gets null, which is also the answer for a
/// trainee with no profile, so the answer never confirms that the id names somebody. The latest rating is read from the
/// activities the caller may read (<c>WhereReadableBy</c>, T101), exactly as the trajectory chart beside it is: an
/// overseer at the trainee's current institution is not thereby shown evidence stamped to another.
/// </para>
/// <para>
/// <b>Which EPAs:</b> the curriculum on the trainee's preferred profile (<see cref="TraineeScopeResolver.PreferredProfiles" />:
/// the active one, else the most recent, so a graduate's standing still reads), national core items plus the trainee's
/// own institution's local items only (a curriculum row is shared by every adopting institution), and only items in
/// force (<see cref="CurriculumItemsInForce" />, T158).
/// </para>
/// <para>
/// <b>The year target</b> is the item's per-stage map (<c>MinimumLevelByStageJson</c>, Annexure A's <c>y1</c> to
/// <c>y4</c>), read by <see cref="CurriculumItem.GetMinimumLevelForStage" />, the same reader the credit engine uses for
/// its per-encounter minimum. There is deliberately no second copy of Annexure A: two would drift. Where the map names
/// no level for the year (an item with no map, or a trainee past the years it names), that reader gives the exit level,
/// and the row says so (<see cref="EpaStandingDto.YearTargetIsExitLevel" />) rather than passing it off as Annexure A's.
/// </para>
/// <para>
/// <b>The exit rule</b> is the College's, so it is counted over the national core items only. An institution's own local
/// items are rows of the table with their own exit level, but they are not part of "Level 5 in 9 EPAs and Level 4 in the
/// remaining 6".
/// </para>
/// <para>
/// <b>Levels are compared only on the item's pinned ladder</b> (<see cref="EntrustmentStanding.Judge" />, T109).
/// </para>
/// </remarks>
/// <param name="AsOf">
/// The day the training year is read for; nothing else is read as of it. Defaults to today in South Africa. The committee
/// review page passes the last day of the review period once that has passed, so a panel judging a year sees that year's
/// targets; decisions and ratings are still read as they stand, because the decisions a review issues postdate its
/// period.
/// </param>
public sealed record GetEntrustmentStandingForTraineeQuery(
    string TraineeUserId,
    ClaimsPrincipal Principal,
    DateOnly? AsOf = null)
    : IRequest<EntrustmentStandingDto?>;

public sealed class GetEntrustmentStandingForTraineeQueryValidator
    : AbstractValidator<GetEntrustmentStandingForTraineeQuery>
{
    public GetEntrustmentStandingForTraineeQueryValidator()
    {
        RuleFor(query => query.TraineeUserId).NotEmpty();
        RuleFor(query => query.Principal).NotNull();
    }
}

public sealed class GetEntrustmentStandingForTraineeQueryHandler
    : IRequestHandler<GetEntrustmentStandingForTraineeQuery, EntrustmentStandingDto?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetEntrustmentStandingForTraineeQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<EntrustmentStandingDto?> Handle(
        GetEntrustmentStandingForTraineeQuery request,
        CancellationToken cancellationToken)
    {
        // Trimmed once, so the id that is authorised is the id that is read. The read itself is EntrustmentStandingReader's,
        // which Home shares in summary mode (T355, note 3); nothing this returns changed when it moved.
        var traineeUserId = request.TraineeUserId.Trim();

        if (!await TraineeScopeResolver.MayReadAsync(_dbContext, request.Principal, traineeUserId, cancellationToken))
        {
            return null;
        }

        return await EntrustmentStandingReader.ReadAsync(
            _dbContext,
            request.Principal,
            traineeUserId,
            request.AsOf ?? QuotaCalendar.Today(),
            withLatestRatings: true,
            cancellationToken);
    }
}
