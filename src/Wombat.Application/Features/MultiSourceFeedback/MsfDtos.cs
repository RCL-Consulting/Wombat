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
}

public sealed record MsfScaleAggregateDto(double Average, int ResponseCount, IReadOnlyDictionary<int, int> Distribution);

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
public sealed record MsfCoveredEpaDto(int EpaId, string Code, string Title, bool Recorded);

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
    bool IsSubjectsCopy = false);

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
