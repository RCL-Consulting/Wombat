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
    DateTime? ReleasedOn);

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
    IReadOnlyList<MsfCategoryAggregateDto> Categories);

public sealed record MsfResponsePromptDto(int QuestionId, string Prompt, MsfQuestionType Type, int? ScaleId, bool Required);

public sealed record MsfResponseFormDto(
    int CampaignId,
    string TemplateName,
    DateOnly ClosesOn,
    MsfRespondentCategory RespondentCategory,
    IReadOnlyList<MsfResponsePromptDto> Questions);
