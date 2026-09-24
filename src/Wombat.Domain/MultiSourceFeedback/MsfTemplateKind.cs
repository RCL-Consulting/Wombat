namespace Wombat.Domain.MultiSourceFeedback;

/// <summary>
/// What a questionnaire collects, and so which evidence a released campaign run on it records. (T164, D35)
/// </summary>
/// <remarks>
/// <para>
/// Operator decision D35: learner feedback, EPA 15's "structured feedback from learners", is an MSF template of its own
/// kind rather than a separate token-based form. It reuses the campaign's invitations, single-use tokens, anonymising
/// close, suppression threshold and release step, which would otherwise have been rebuilt beside them.
/// </para>
/// <para>
/// The kind decides three things, and nothing else: who may be invited (<see cref="MsfTemplate.Accepts" />), which
/// evidence type a release writes (<c>msf_cpsa</c> or <c>learner_feedback_cpsa</c>), and which EPAs a campaign may be
/// declared evidence for. Stored as its number; append new members, never renumber.
/// </para>
/// </remarks>
public enum MsfTemplateKind
{
    /// <summary>Multi-source feedback from colleagues, patients and others: the College's MSF (T121).</summary>
    Msf = 0,

    /// <summary>Feedback on the trainee's teaching from the people they taught (EPA 15). Only learners answer it.</summary>
    LearnerFeedback = 1
}
