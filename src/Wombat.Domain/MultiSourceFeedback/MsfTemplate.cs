namespace Wombat.Domain.MultiSourceFeedback;

public sealed class MsfTemplate
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? SpecialityId { get; set; }
    public bool AllowPatientResponses { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>What this questionnaire collects (T164, D35). Set at creation and never changed.</summary>
    public MsfTemplateKind Kind { get; set; } = MsfTemplateKind.Msf;

    public ICollection<MsfQuestion> Questions { get; set; } = [];

    /// <summary>
    /// Whether someone of this category may be invited to answer this questionnaire. (T164)
    /// </summary>
    /// <remarks>
    /// <para>
    /// A learner-feedback questionnaire is answered by learners and by nobody else, and a learner answers nothing else.
    /// Each half matters. A learner counted into an MSF would be a respondent group College decision D11 never had in
    /// mind, and would help an MSF clear its two-group rule. A colleague counted into learner feedback would be feedback
    /// on the trainee's teaching from someone who was not taught.
    /// </para>
    /// <para>
    /// A patient is invited only where the template allows it, as before T164.
    /// </para>
    /// <para>
    /// This settles, for now, the first half of the College's open question 9 (§ 3F): is learner feedback multi-source
    /// feedback from students, junior trainees and team members? It is answered here as "no: its own instrument, from
    /// whoever the trainee taught", and is revisited when the College answers.
    /// </para>
    /// </remarks>
    public bool Accepts(MsfRespondentCategory category)
        => Kind switch
        {
            MsfTemplateKind.LearnerFeedback => category == MsfRespondentCategory.Learner,
            _ => category != MsfRespondentCategory.Learner &&
                 (category != MsfRespondentCategory.Patient || AllowPatientResponses)
        };

    /// <summary>Every category <see cref="Accepts" /> admits, in the enum's order: what the invitee picker offers.</summary>
    public IReadOnlyList<MsfRespondentCategory> AcceptedCategories()
        => Enum.GetValues<MsfRespondentCategory>().Where(Accepts).ToArray();
}
