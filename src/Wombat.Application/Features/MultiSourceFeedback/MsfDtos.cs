using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.MultiSourceFeedback;

public sealed record MsfQuestionDto(int Id, int Order, string Prompt, MsfQuestionType Type, int? ScaleId, bool Required);

public sealed record MsfTemplateDto(int Id, string Name, int? SpecialityId, bool AllowPatientResponses, bool IsActive, IReadOnlyList<MsfQuestionDto> Questions);

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
/// Whether this EPA's evidence activity was actually written. False on a released campaign means the
/// EPA had left the subject's curriculum by release day and was dropped - a terminal state, not a
/// pending one.
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
    DateTime? EvidenceRecordedOn);

public sealed record MsfResponsePromptDto(int QuestionId, string Prompt, MsfQuestionType Type, int? ScaleId, bool Required);

public sealed record MsfResponseFormDto(
    int CampaignId,
    string TemplateName,
    DateOnly ClosesOn,
    MsfRespondentCategory RespondentCategory,
    IReadOnlyList<MsfResponsePromptDto> Questions);
