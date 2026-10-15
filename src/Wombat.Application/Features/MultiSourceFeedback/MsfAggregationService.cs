using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.MultiSourceFeedback;

public interface IMsfAggregationService
{
    /// <summary>A campaign's aggregate report, and what it was evidence for.</summary>
    /// <param name="recordedEpas">
    /// The EPAs the campaign's evidence rows carry, as <see cref="MsfCampaignCoverage.RecordedEpasAsync" /> reads them
    /// for it: the one source of whether a declared EPA was recorded (T186). Empty for a campaign that is not released,
    /// which has no evidence rows; the release gates are computed before the release writes any.
    /// </param>
    /// <param name="nameTeachingContexts">
    /// Whether the report names the teaching contexts a learner-feedback campaign's learners answered from, or only counts
    /// them (<see cref="MsfCampaignAggregateReportDto.TeachingContextsResponded" />). True only for the campaign's own
    /// report to whoever runs it; never for the trainee, the portfolio PDF or a committee snapshot. (T164)
    /// </param>
    MsfCampaignAggregateReportDto BuildReport(
        MsfCampaign campaign,
        IEnumerable<MsfRecordedEpa> recordedEpas,
        bool nameTeachingContexts = false);
}

public sealed class MsfAggregationService : IMsfAggregationService
{
    public MsfCampaignAggregateReportDto BuildReport(
        MsfCampaign campaign,
        IEnumerable<MsfRecordedEpa> recordedEpas,
        bool nameTeachingContexts = false)
    {
        ArgumentNullException.ThrowIfNull(campaign);
        ArgumentNullException.ThrowIfNull(recordedEpas);

        // Recorded is read from the evidence rows, never from the per-EPA stamp MsfCampaignEpa.RecordedOn, which a
        // campaign released before the stamp existed does not carry although its rows exist (T186).
        var recordedEpaIds = recordedEpas.Select(epa => epa.EpaId).ToHashSet();

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

                            // The mean and its count only: no per-point counts, which no page draws (T369, F2).
                            var scaleAggregate = new MsfScaleAggregateDto(
                                scaleValues.Count == 0 ? 0d : scaleValues.Average(),
                                scaleValues.Count);

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

        // College decision D11, 2026-09-20: multi-source feedback must actually be multi-source.
        // MinimumCategoryResponses only ever suppressed a thin category's data from the report; nothing
        // required that any category survived. A campaign answered by eight peer doctors therefore
        // cleared MinimumResponses, showed one category and five suppressed ones, and released as
        // "multi-source feedback" on one source. Both gates now, in the one place that computes either,
        // so the release handler and the disabled button cannot disagree about which applies. (T121)
        var survivingCategoryCount = categoryReports.Count(category => !category.IsSuppressed);

        // Learner feedback's teaching contexts: counted for every reader, named only for the campaign's runner (T164).
        var teachingContexts = campaign.Template.Kind == MsfTemplateKind.LearnerFeedback
            ? campaign.RespondedTeachingContexts()
            : null;

        var released = campaign.State == MsfCampaignState.Released;

        return new MsfCampaignAggregateReportDto(
            campaign.Id,
            campaign.SubjectUserId,
            campaign.Template.Name,
            campaign.State,
            campaign.MinimumResponses,
            campaign.MinimumCategoryResponses,
            campaign.Responses.Count,
            campaign.CoordinatorNarrative,
            campaign.Responses.Count >= campaign.MinimumResponses &&
                survivingCategoryCount >= campaign.MinimumRespondentCategories,
            // F1: no group's results until the campaign has closed, for every reader; the counts above still say how far
            // it is (T369).
            ResponsesAreRead(campaign) ? categoryReports : [],
            campaign.MinimumRespondentCategories,
            survivingCategoryCount,
            campaign.CoveredEpas
                .Where(covered => covered.Epa is not null)
                .Select(covered => new MsfCoveredEpaDto(
                    covered.EpaId,
                    covered.Epa.Code,
                    covered.Epa.Title,
                    recordedEpaIds.Contains(covered.EpaId))
                {
                    // Once released, what will be recorded is what was (T369, M6). Before, the report query asks the
                    // release's own predicate; nothing here can.
                    WillBeRecorded = released && recordedEpaIds.Contains(covered.EpaId)
                })
                .OrderBy(covered => covered.Code, StringComparer.Ordinal)
                .ToList(),
            campaign.ReviewerEntrustmentLevel,
            campaign.EvidenceRecordedOn,
            campaign.Template.Kind,
            nameTeachingContexts ? teachingContexts : null,
            teachingContexts?.Count)
        {
            // The campaign's own facts, for the report's header, status card and Close dialog (T369, build-contracts § 1).
            OpensOn = campaign.OpensOn,
            ClosesOn = campaign.ClosesOn,
            InvitedTotal = campaign.Invitations.Count,
            ClosedOn = campaign.ClosedOn,
            ClosedByUserId = campaign.ClosedByUserId,
            ReleasedOn = campaign.ReleasedOn
        };
    }

    /// <summary>
    /// Whether the campaign's responses may be read by group: once it has closed to responses and every address has been
    /// removed (T369, F1). Not while it is a draft or open; a campaign withdrawn while open never closed, and its responses
    /// are never read either. One withdrawn after closing was read before it was withdrawn, and still is.
    /// </summary>
    internal static bool ResponsesAreRead(MsfCampaign campaign)
        => campaign.State switch
        {
            MsfCampaignState.Closed or MsfCampaignState.UnderReview or MsfCampaignState.Released => true,
            MsfCampaignState.Withdrawn => campaign.ClosedOn is not null,
            _ => false
        };
}
