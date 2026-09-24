namespace Wombat.Domain.MultiSourceFeedback;

/// <remarks>
/// Stored as its number (<c>MsfInvitations.RespondentCategory</c> is an integer column), so a new member is appended and
/// nothing is renumbered.
/// </remarks>
public enum MsfRespondentCategory
{
    PeerDoctor = 0,
    Consultant = 1,
    Nurse = 2,
    Ahp = 3,
    Patient = 4,
    Other = 5,

    /// <summary>
    /// Anyone the trainee taught, in the session the invitation names: a student, a more junior trainee, or a member of
    /// the interprofessional team or a caregiver they taught (T164, D35). Invited only to a learner-feedback campaign,
    /// which invites nobody else (<see cref="MsfTemplate.Accepts" />).
    /// </summary>
    /// <remarks>
    /// Defined by the relationship, not by who the person is, because EPA 15 names learners across the team. Whether the
    /// College means learner feedback to be multi-source feedback from students, junior trainees and team members is its
    /// open question 9 (§ 3F); this category and <see cref="MsfTemplate.Accepts" /> are revisited when it answers.
    /// </remarks>
    Learner = 6
}
