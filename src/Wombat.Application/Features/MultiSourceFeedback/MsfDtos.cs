using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.MultiSourceFeedback;

public sealed record MsfQuestionDto(int Id, int Order, string Prompt, MsfQuestionType Type, int? ScaleId, bool Required);

/// <param name="Kind">What the questionnaire collects (T164): multi-source feedback or learner feedback.</param>
public sealed record MsfTemplateDto(
    int Id,
    string Name,
    int? SpecialityId,
    bool AllowPatientResponses,
    bool IsActive,
    IReadOnlyList<MsfQuestionDto> Questions,
    MsfTemplateKind Kind = MsfTemplateKind.Msf);

public sealed record MsfCampaignSummaryDto(
    int Id,
    string SubjectUserId,
    string TemplateName,
    DateOnly OpensOn,
    DateOnly ClosesOn,
    int MinimumResponses,
    int MinimumCategoryResponses,
    MsfCampaignState State,
    int InvitationCount,
    int ResponseCount,
    DateTime? ReleasedOn)
{
    /// <summary>
    /// Whose feedback it is, by name (<see cref="Wombat.Application.Common.Users.UserDisplayNames.NameOf" />): the
    /// campaign list's Subject column (T142). Filled by <c>ListMsfCampaignsForCoordinatorQuery</c> in one lookup for the
    /// page. Null from the trainee's own list and from create, which do not show it.
    /// </summary>
    public string? SubjectName { get; init; }

    /// <summary>What the campaign's questionnaire collects (T164). Filled by both campaign lists.</summary>
    public MsfTemplateKind Kind { get; init; } = MsfTemplateKind.Msf;

    /// <summary>
    /// Whether the campaign was ever opened (<c>MsfCampaign.OpenedOn</c> is set): the coordinator's list counts its
    /// responses only then, and shows "—" for one that never opened, a draft or a draft withdrawn, which could have none
    /// (T369, T270). Filled by <c>ListMsfCampaignsForCoordinatorQuery</c>.
    /// </summary>
    public bool HasOpened { get; init; }
}

/// <summary>
/// One released report on the registrar's own list, MSF reports (T369, F3; build-contracts § 2): which questionnaire, what
/// it collects and when it was released, and nothing else.
/// </summary>
/// <remarks>
/// No count of any kind. The campaign's total took in the suppressed groups, so "6" beside a report showing two groups of
/// two told her that two others answered, against the release's "not even that they answered" (T249). Her own copy of the
/// report carries no group's count either (<c>MsfGroupCards</c>' <c>ForTrainee</c>).
/// </remarks>
/// <param name="ReleasedOn">When the report was released, UTC; the page prints it in SAST (T325).</param>
public sealed record MsfReleasedReportDto(int CampaignId, string TemplateName, MsfTemplateKind Kind, DateTime ReleasedOn);

/// <summary>One scale question's answers in one respondent group: their mean, and how many answered it.</summary>
/// <remarks>
/// No per-point counts (T369, F2, round 3 check item 3): no page or report draws a rating's spread, and with a group as small
/// as the threshold of 3 a spread such as "one 2, two 5s" says more about each answer than the mean does.
/// </remarks>
public sealed record MsfScaleAggregateDto(double Average, int ResponseCount);

public sealed record MsfQuestionAggregateDto(
    int QuestionId,
    string Prompt,
    MsfQuestionType Type,
    MsfScaleAggregateDto? Scale,
    IReadOnlyList<string> Comments);

public sealed record MsfCategoryAggregateDto(
    MsfRespondentCategory Category,
    int ResponseCount,
    bool IsSuppressed,
    IReadOnlyList<MsfQuestionAggregateDto> Questions);

/// <summary>One EPA a campaign is declared to be evidence for. (T121)</summary>
/// <param name="Recorded">
/// Whether this EPA's evidence activity exists: read from the campaign's evidence rows
/// (<see cref="MsfCampaignCoverage" />, T186), never from the per-EPA stamp. False on a released campaign
/// is terminal, not pending, and says nothing about why: the release drops an EPA that has left the
/// subject's curriculum, but a missing row does not show that that was the reason.
/// </param>
public sealed record MsfCoveredEpaDto(int EpaId, string Code, string Title, bool Recorded)
{
    /// <summary>
    /// Whether the release records this EPA (T369, M6): before release, whether it is still one the campaign may cover for
    /// its trainee (<see cref="MsfCampaignRules.CoverableEpaIdsAsync" />, the predicate the release drops an EPA by); once
    /// released, <see cref="Recorded" />. Filled by <c>GetCampaignAggregateReportQuery</c> for the report's records line and
    /// the release dialog ("PAED-012: not recorded, no longer on Lerato Molefe's curriculum"); false elsewhere.
    /// </summary>
    public bool WillBeRecorded { get; init; }
}

/// <param name="ReadyForRelease">
/// Both release gates at once: enough responses in total, AND enough respondent categories surviving
/// suppression (College decision D11). The two numbers beside it exist so a blocked release can say
/// which gate it is blocked on rather than leaving a disabled button unexplained.
/// </param>
/// <param name="SurvivingCategoryCount">
/// How many categories cleared <see cref="MinimumCategoryResponses" /> and therefore actually report
/// anything. A campaign answered entirely by eight peer doctors scores 1 here.
/// </param>
/// <param name="CoveredEpas">
/// What the released report will become evidence for. Empty is legal and means the release will record
/// no evidence at all.
/// </param>
/// <param name="Kind">What the campaign's questionnaire collects (T164): multi-source feedback or learner feedback.</param>
/// <param name="TeachingContextsResponded">
/// For learner feedback, the names of the distinct teaching contexts whose learners answered
/// (<c>MsfCampaign.RespondedTeachingContexts</c>), in reading order; and only for a report built for whoever runs the
/// campaign (<see cref="IMsfAggregationService.BuildReport" />'s <c>nameTeachingContexts</c>). Null for multi-source
/// feedback, and null in every report the trainee reads or that is printed or frozen, because a context is a finer
/// breakdown than the respondent group the suppression threshold protects: "Neonatal night teaching" beside four answers
/// tells the trainee that the one intern taught there answered, and the name is the coordinator's free text. (T164)
/// </param>
/// <param name="TeachingContextCount">
/// For learner feedback, how many distinct teaching contexts the learners who answered were taught in: what EPA 15's
/// "at least two teaching contexts" is about. Reported, never enforced (§ 3F question 9), and carried in every report of
/// a learner-feedback campaign, whoever reads it. Null for multi-source feedback, whose respondents are counted by group
/// instead.
/// </param>
/// <param name="Categories">
/// Each respondent group that answered, in the enum's order (Peer doctor, Consultant, Nurse, Allied health professional,
/// Other; M2), a group under the threshold marked suppressed and holding nothing. <b>Empty while the campaign has not
/// closed</b> (T369, F1): the responses are read once it has closed and every address has been removed, whoever reads it.
/// The counts beside it (<see cref="TotalResponses" />, <see cref="SurvivingCategoryCount" />) still say how far it is.
/// </param>
/// <param name="IsSubjectsCopy">
/// Whether the trainee the campaign is about read this copy: <c>MsfCampaignRules.IsCaller</c>'s answer, the one that
/// also leaves the teaching contexts unnamed, so the two cannot disagree. Such a copy is always a released report
/// (<see cref="MsfCampaignRules.CanReadReportAsync" />). The coordinator's report page (<c>/msf/reports/{id}</c>) does
/// not show it and sends its reader to their own copy (<c>/msf/my-reports/{id}</c>), whatever other role brought them
/// there: the subject reads the report as a trainee, never beside the coordinator's actions card and the per-group
/// detail. False in every report built for anyone else, and in the ones that are printed or frozen. (T269)
/// <para>
/// It says who asked through <see cref="GetCampaignAggregateReportQuery" />, and so which page shows the copy; it is not
/// what the copy may show. False is not "whoever runs the campaign": the portfolio PDF's copy is false, and the trainee
/// reads it. So never name a teaching context because this is false. What a copy may name is decided where it is built,
/// and <see cref="MsfCampaignAggregateReportDto.TeachingContextsResponded" /> is null in every copy the trainee can be
/// given, this one included.
/// </para>
/// </param>
public sealed record MsfCampaignAggregateReportDto(
    int CampaignId,
    string SubjectUserId,
    string TemplateName,
    MsfCampaignState State,
    int MinimumResponses,
    int MinimumCategoryResponses,
    int TotalResponses,
    string? CoordinatorNarrative,
    bool ReadyForRelease,
    IReadOnlyList<MsfCategoryAggregateDto> Categories,
    int MinimumRespondentCategories,
    int SurvivingCategoryCount,
    IReadOnlyList<MsfCoveredEpaDto> CoveredEpas,
    int? ReviewerEntrustmentLevel,
    DateTime? EvidenceRecordedOn,
    MsfTemplateKind Kind = MsfTemplateKind.Msf,
    IReadOnlyList<string>? TeachingContextsResponded = null,
    int? TeachingContextCount = null,
    bool IsSubjectsCopy = false)
{
    /// <summary>
    /// The trainee by name (<see cref="Wombat.Application.Common.Users.UserDisplayNames.NameOf" />): the report's h1, "MSF
    /// report for Lerato Molefe", and every sentence that names the trainee (T369, D3: never a pronoun). Filled by
    /// <c>GetCampaignAggregateReportQuery</c>; null from the other builders, which do not show it.
    /// </summary>
    public string? SubjectName { get; init; }

    /// <summary>The response window's first day (T369): the report's subtitle.</summary>
    public DateOnly OpensOn { get; init; }

    /// <summary>The response window's last day (T369): the subtitle, and "It closes by itself once 2026-10-28 has passed".</summary>
    public DateOnly ClosesOn { get; init; }

    /// <summary>
    /// How many were invited (T369): the Close dialog's unused links are this less <see cref="TotalResponses" />, a count
    /// and never whose. An invitee removed from a draft is not counted; one anonymised by the close is.
    /// </summary>
    public int InvitedTotal { get; init; }

    /// <summary>When the campaign closed to responses, or null while it has not (T369).</summary>
    public DateTime? ClosedOn { get; init; }

    /// <summary>
    /// Who closed it, or null when the hourly job did or it has not closed (T369, F5): "Closed by itself on …" reads a
    /// closed campaign's null here.
    /// </summary>
    public string? ClosedByUserId { get; init; }

    /// <summary>When the report was released to the trainee, or null (T369): the released status line, in SAST.</summary>
    public DateTime? ReleasedOn { get; init; }

    /// <summary>
    /// Whether the trainee is current (<see cref="MsfSubjectStanding.IsCurrent" />, T369): once not, the report says she
    /// would have read it, and the release sends no mail (F4).
    /// </summary>
    public bool SubjectIsCurrent { get; init; }

    /// <summary>
    /// The day the trainee's programme ended, once none is running (<see cref="MsfSubjectStanding.EndedOn" />, T369, R10).
    /// </summary>
    public DateOnly? SubjectEndedOn { get; init; }
}

/// <summary>One point a respondent can choose on a scale question: the value stored, and what it means. (T205)</summary>
public sealed record MsfScalePointDto(int Value, string Label, string? Description);

/// <summary>One question of the questionnaire a respondent's link opens. (T021, T205)</summary>
/// <param name="ScalePoints">
/// The points a scale question is answered on, lowest first (<see cref="MsfRatingScale" />): the only values the submit
/// accepts for it. Empty for a comment question.
/// </param>
public sealed record MsfResponsePromptDto(
    int QuestionId,
    string Prompt,
    MsfQuestionType Type,
    bool Required,
    IReadOnlyList<MsfScalePointDto> ScalePoints);

/// <summary>
/// What a respondent's link opens: whom the feedback is for, the last day to give it, and the questions. Nothing about
/// any other respondent or response. (T021, T202, T205)
/// </summary>
/// <param name="TraineeName">
/// The trainee the feedback is about, by name, as the invitation email names them (T202). Null only when the trainee has
/// no name on record any more; the page then points the respondent at their invitation, which named them.
/// </param>
/// <param name="Kind">
/// What the questionnaire collects (T164): the page asks a learner about the trainee's teaching, as their invitation
/// did, and asks everyone else for multi-source feedback on a colleague. The questionnaire's, not the respondent's: it
/// says nothing about who else was asked.
/// </param>
/// <param name="LastDayToRespond">
/// The one deadline the invitation gave (<see cref="MsfInvitation.LastDayToRespond" />), not the campaign's closing date:
/// the invitation's own expiry can come first.
/// </param>
/// <remarks>
/// No campaign id: neither the page nor the Api's JSON needs one, and a sequential internal id is one more thing a link
/// would disclose (T205). No teaching context either: a learner's is the coordinator's record of where they were taught
/// (<see cref="MsfInvitation.TeachingContext" />), never asked of the learner, and not the page's to show.
/// </remarks>
public sealed record MsfResponseFormDto(
    string TemplateName,
    MsfTemplateKind Kind,
    string? TraineeName,
    DateOnly LastDayToRespond,
    MsfRespondentCategory RespondentCategory,
    IReadOnlyList<MsfResponsePromptDto> Questions);
