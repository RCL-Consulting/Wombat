namespace Wombat.Application.Common.Interfaces;

/// <summary>
/// Serialises a completion's credit against the recording of its trainee's programme end (T281), so that neither can leave
/// credit for an encounter after the last day in the tally.
/// </summary>
/// <remarks>
/// <para>
/// An encounter observed after the programme's last day credits nothing on the profile (<c>TraineeProfile.IsAfterEnd</c>),
/// and recording the end takes back the credit such encounters already earned (<c>ProgrammeEndCredit</c>). Neither save
/// writes anything the other reads, so without a lock two races were open. A completion that read the profile before the
/// end committed credited an encounter after it; the end, which had already read that there was nothing to take back,
/// saved alone. And an end whose replay had read the trainee's progress rows before a completion opened a new semester row
/// could not see that row: neither the rows' xmin tokens nor the replay's check for a row that arrived mid-rebuild cover a
/// row it never read. Either way the credit stayed until an Administrator rebuild.
/// </para>
/// <para>
/// A hold is a transaction on the request's own context with row locks on <c>TraineeProfiles</c>, taken before the profile
/// is read and released when the caller's save commits, as <see cref="IEpaCreditLock" />'s are (T230). Credit holds every
/// profile of its trainee <em>shared</em>, so completions never wait for one another; recording an end holds the ended
/// profile against every completion's credit for its trainee. Whichever comes second waits for the first to commit and then
/// runs against what it wrote:
/// </para>
/// <list type="bullet">
/// <item>An end that waited for a completion reads the credit it saved, and takes it back if its encounter is after the
/// last day.</item>
/// <item>A completion that waited for an end reads the end, and credits nothing for an encounter after it.</item>
/// </list>
/// <para>
/// Credit takes it wherever credit is written live: the completion (<c>ActivityService</c>) and an EPA's reactivation,
/// which credits the completions filed during its pause (<c>UpdateEpaCommandHandler</c>). Not the Administrator's rebuild,
/// which replays every trainee: it recomputes the tallies from the stored ends, and a rebuild that overlaps an end's
/// replay is refused by the progress rows' xmin tokens.
/// </para>
/// <para>
/// Only on PostgreSQL. Every other provider (the unit suites' in-memory one) gets a hold that does nothing, because it has
/// no transactions to hold a lock in and no concurrent requests to hold it against.
/// </para>
/// </remarks>
public interface ITraineeCreditLock
{
    /// <summary>
    /// For recording a programme's end: holds the profile against every completion's credit for its trainee until the
    /// returned hold is committed or disposed. Take it before reading the profile. Anything that calls
    /// <c>TraineeProfile.Complete</c> or <c>TraineeProfile.Deactivate</c> takes it (<c>ProgrammeEndWritePathTests</c>).
    /// </summary>
    /// <exception cref="InvalidOperationException">The wait outlasted the command timeout; nothing was held.</exception>
    Task<ICreditHold> HoldForEndAsync(int traineeProfileId, CancellationToken cancellationToken);

    /// <summary>
    /// For credit: holds every profile of these trainees shared until the returned hold is committed or disposed. Take it
    /// before reading the profile credit accrues against. No trainees, no hold.
    /// </summary>
    /// <exception cref="InvalidOperationException">The wait outlasted the command timeout; nothing was held.</exception>
    Task<ICreditHold> HoldForCreditAsync(IReadOnlyCollection<string> traineeUserIds, CancellationToken cancellationToken);
}
