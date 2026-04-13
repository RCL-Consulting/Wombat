using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.MultiSourceFeedback;

public interface IMsfAggregationService
{
    MsfCampaignAggregateReportDto BuildReport(MsfCampaign campaign);
}

public sealed class MsfAggregationService : IMsfAggregationService
{
    public MsfCampaignAggregateReportDto BuildReport(MsfCampaign campaign)
    {
        ArgumentNullException.ThrowIfNull(campaign);

        var templateQuestions = campaign.Template.Questions
            .OrderBy(question => question.Order)
            .ToList();

        var categoryReports = campaign.Responses
            .GroupBy(response => response.Invitation.RespondentCategory)
            .OrderBy(group => group.Key)
            .Select(group =>
            {
                var responseCount = group.Count();
                if (responseCount < campaign.MinimumCategoryResponses)
                {
                    return new MsfCategoryAggregateDto(group.Key, responseCount, true, []);
                }

                var questionReports = templateQuestions
                    .Select(question =>
                    {
                        var answers = group.SelectMany(response => response.Answers)
                            .Where(answer => answer.QuestionId == question.Id)
                            .ToList();

                        if (question.Type == MsfQuestionType.Scale)
                        {
                            var scaleValues = answers
                                .Where(answer => answer.ScaleValue.HasValue)
                                .Select(answer => answer.ScaleValue!.Value)
                                .ToList();

                            var distribution = scaleValues
                                .GroupBy(value => value)
                                .OrderBy(grouping => grouping.Key)
                                .ToDictionary(grouping => grouping.Key, grouping => grouping.Count());

                            var scaleAggregate = new MsfScaleAggregateDto(
                                scaleValues.Count == 0 ? 0d : scaleValues.Average(),
                                scaleValues.Count,
                                distribution);

                            return new MsfQuestionAggregateDto(question.Id, question.Prompt, question.Type, scaleAggregate, []);
                        }

                        var comments = answers
                            .Where(answer => !string.IsNullOrWhiteSpace(answer.LongText))
                            .Select(answer => answer.LongText!.Trim())
                            .ToList();

                        return new MsfQuestionAggregateDto(question.Id, question.Prompt, question.Type, null, comments);
                    })
                    .ToList();

                return new MsfCategoryAggregateDto(group.Key, responseCount, false, questionReports);
            })
            .ToList();

        return new MsfCampaignAggregateReportDto(
            campaign.Id,
            campaign.SubjectUserId,
            campaign.Template.Name,
            campaign.State,
            campaign.MinimumResponses,
            campaign.MinimumCategoryResponses,
            campaign.Responses.Count,
            campaign.CoordinatorNarrative,
            campaign.Responses.Count >= campaign.MinimumResponses,
            categoryReports);
    }
}
