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
public sealed record EndedProgrammeDto(DateOnly EndedOn, bool Completed);

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
