using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.Users;

/// <summary>
/// How a registrar's programme ended, for a person who now holds no role (T376, flow 12, S2, Q13; D54: a withdrawn or
/// completed registrar holds no role). Users' No role cell says only that one exists; the user page says how and when:
/// "they completed the programme on 2026-10-20", "they left the programme on 2026-10-18".
/// </summary>
/// <param name="EndedOn">The programme's last day (<see cref="TraineeProfile.EndedOn" />).</param>
/// <param name="Completed">True when it was completed (<see cref="TraineeProfile.CompletedOn" />), false when the registrar left it.</param>
public sealed record EndedProgrammeDto(DateOnly EndedOn, bool Completed)
{
    /// <summary>
    /// The training year the programme ended in (T379, flow 13, S4 and S8): <see cref="TraineeProfile.StageOn" /> on the last
    /// day, as the former trainee's Home, the profile's ended record and the staff export say it ("Training year 4"). Null
    /// for a last day before the programme start (an erasure before it began), and from <see cref="EndedProgrammes.LatestForAsync" />,
    /// which flow 12's pages read and which needs none; <see cref="EndedProgrammes.Of" /> fills it.
    /// </summary>
    public int? TrainingYearAtEnd { get; init; }
}

/// <summary>
/// The latest ended trainee profile of each person asked about, in one query (T376, flow 12, S2; build-contracts § 0): Users'
/// page of rows (lane U) and the user page (lane P) read it, so they agree on who has one.
/// </summary>
/// <remarks>
/// A profile ended before T209 recorded no day (<see cref="TraineeProfile.DeactivatedOn" /> null) has no
/// <see cref="TraineeProfile.EndedOn" />, and is left out: the page then reads as a person with no profile. Several ended
/// profiles (a registrar who left and was admitted again, T374) give the latest by its end, then by id. A person with a
/// running programme holds Trainee, so a caller asks this only for the people who hold no role.
/// </remarks>
public static class EndedProgrammes
{
    /// <summary>
    /// How <paramref name="profile" /> ended, with the training year it ended in, or null while it runs or for a profile that
    /// ended before Wombat recorded the day (T379, flow 13, build-contracts § 0). One reading for every page of flow 13 that
    /// names the end: the former trainee's Home ("You completed your programme on 2026-10-26." / "Your programme ended on
    /// 2026-10-24."), the profile's ended record and the closed list, and the staff export's last day. An erased person's
    /// profile reads as a withdrawal on the erasure day (<see cref="TraineeProfile.Erase" />).
    /// </summary>
    public static EndedProgrammeDto? Of(TraineeProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        return profile.IsActive || profile.EndedOn is not { } endedOn
            ? null
            : new EndedProgrammeDto(endedOn, profile.CompletedOn is not null)
            {
                TrainingYearAtEnd = TraineeProfile.StageOn(profile.ProgrammeStartDate, endedOn),
            };
    }

    public static async Task<IReadOnlyDictionary<string, EndedProgrammeDto>> LatestForAsync(
        IApplicationDbContext dbContext,
        IReadOnlyCollection<string> userIds,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(userIds);

        if (userIds.Count == 0)
        {
            return new Dictionary<string, EndedProgrammeDto>(StringComparer.Ordinal);
        }

        var ids = userIds.Distinct(StringComparer.Ordinal).ToArray();
        var ended = await dbContext.Set<TraineeProfile>()
            .AsNoTracking()
            .Where(profile => ids.Contains(profile.UserId) && (profile.CompletedOn != null || profile.DeactivatedOn != null))
            .Select(profile => new { profile.Id, profile.UserId, profile.CompletedOn, profile.DeactivatedOn })
            .ToListAsync(cancellationToken);

        return ended
            .GroupBy(profile => profile.UserId, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group =>
                {
                    var latest = group
                        .OrderByDescending(profile => profile.CompletedOn ?? profile.DeactivatedOn)
                        .ThenByDescending(profile => profile.Id)
                        .First();
                    return new EndedProgrammeDto(
                        (latest.CompletedOn ?? latest.DeactivatedOn)!.Value,
                        latest.CompletedOn.HasValue);
                },
                StringComparer.Ordinal);
    }
}
