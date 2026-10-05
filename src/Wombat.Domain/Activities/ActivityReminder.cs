using Wombat.Domain.Curricula;

namespace Wombat.Domain.Activities;

/// <summary>
/// One reminder a member of staff sent the assessor a waiting request names: who sent it, when, and to whom (T358, flow 06;
/// Q4, C4). It is the record the waiting list reads its "Reminded 2026-10-04 by Pieter Smit" line from, and the same-day
/// block is its unique index: one reminder per request per South African day, whoever sends it.
/// </summary>
/// <remarks>
/// <para>
/// A reminder moves nothing (C4c): it is not a move of the request, so it is not an <see cref="ActivityTransition" />, and
/// it never touches the activity's state, its transitions or <see cref="Activity.UpdatedOn" />, from which the wait is
/// counted. So it carries the activity's id and no navigation to it: a save that adds a reminder can never reach the
/// activity row, and loading an activity never loads its reminders.
/// </para>
/// <para>
/// <see cref="SentByUserId" /> and <see cref="AssessorUserId" /> name people, and an erasure pseudonymises both, as it does
/// <see cref="ActivityTransition.ActorUserId" /> (T358, review 1).
/// </para>
/// </remarks>
public sealed class ActivityReminder
{
    public int Id { get; set; }

    /// <summary>The waiting request the reminder was about.</summary>
    public int ActivityId { get; set; }

    /// <summary>The member of staff who sent it.</summary>
    public string SentByUserId { get; set; } = string.Empty;

    /// <summary>The assessor it was sent to: the person the request's next move named when it was sent.</summary>
    public string AssessorUserId { get; set; } = string.Empty;

    /// <summary>When it was sent (UTC).</summary>
    public DateTime SentOn { get; set; }

    /// <summary>
    /// The South African day <see cref="SentOn" /> fell on (<see cref="ProgrammeCalendar.DateOf" />): the day the same-day
    /// block counts by, stored so the unique index can hold it (T358, round 3's settled same-day rule).
    /// </summary>
    public DateOnly SentOnDay { get; set; }

    /// <summary>
    /// A reminder about <paramref name="activityId" />, sent by <paramref name="sentByUserId" /> to
    /// <paramref name="assessorUserId" /> at <paramref name="sentOn" />, its day read on the South African calendar, so the
    /// day and the moment cannot disagree.
    /// </summary>
    public static ActivityReminder Record(int activityId, string sentByUserId, string assessorUserId, DateTime sentOn)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sentByUserId);
        ArgumentException.ThrowIfNullOrWhiteSpace(assessorUserId);
        if (sentOn.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("A reminder's moment is UTC.", nameof(sentOn));
        }

        return new ActivityReminder
        {
            ActivityId = activityId,
            SentByUserId = sentByUserId,
            AssessorUserId = assessorUserId,
            SentOn = sentOn,
            SentOnDay = ProgrammeCalendar.DateOf(sentOn)
        };
    }
}
