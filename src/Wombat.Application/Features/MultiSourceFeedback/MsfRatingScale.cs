using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Epas;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.MultiSourceFeedback;

/// <summary>
/// The points each scale question of a questionnaire is answered on: what the respondent's page offers and the only
/// values the submit accepts. (T205)
/// </summary>
/// <remarks>
/// <para>
/// A question that names a scale (<see cref="MsfQuestion.ScaleId" />, an entrustment scale: <c>DeleteEntrustmentScale</c>
/// counts it as a reference, T054) is answered on that scale's levels, each stored as its order, as every rating in the
/// product is. No question the product creates names one: the campaign editor's quick template passes none, and D10 keeps
/// respondents off the entrustment ladder. Those are answered on <see cref="Default" />.
/// </para>
/// <para>
/// Before T205 nothing said what a scale question's values were. The Api accepted any integer, and an answer of 999
/// would have entered the category means the coordinator reviews. The page could not be built without a scale to show,
/// and the submit now refuses a value that is not one of the question's points.
/// </para>
/// </remarks>
public static class MsfRatingScale
{
    /// <summary>
    /// The five-point scale a question is answered on when it names none: how the trainee compares with what the
    /// respondent expects of a trainee at their stage. Needs confirmation by the operator (T205): the product had no
    /// scale before, so these labels are new.
    /// </summary>
    public static IReadOnlyList<MsfScalePointDto> Default { get; } =
    [
        new(1, "Well below expectations", null),
        new(2, "Below expectations", null),
        new(3, "Meets expectations", null),
        new(4, "Above expectations", null),
        new(5, "Well above expectations", null)
    ];

    /// <summary>
    /// The points of every scale question on <paramref name="template" />, by question id. A comment question has none
    /// and is absent.
    /// </summary>
    public static async Task<IReadOnlyDictionary<int, IReadOnlyList<MsfScalePointDto>>> ResolveAsync(
        IApplicationDbContext dbContext,
        MsfTemplate template,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(template);

        var scaleQuestions = template.Questions.Where(question => question.Type == MsfQuestionType.Scale).ToList();
        var namedScaleIds = scaleQuestions
            .Where(question => question.ScaleId is not null)
            .Select(question => question.ScaleId!.Value)
            .Distinct()
            .ToList();

        var levelsByScale = namedScaleIds.Count == 0
            ? []
            : (await dbContext.Set<EntrustmentLevel>()
                    .AsNoTracking()
                    .Where(level => namedScaleIds.Contains(level.ScaleId))
                    .Select(level => new { level.ScaleId, level.Order, level.Label, level.Description })
                    .ToListAsync(cancellationToken))
                .GroupBy(level => level.ScaleId)
                .ToDictionary(
                    group => group.Key,
                    group => (IReadOnlyList<MsfScalePointDto>)group
                        .OrderBy(level => level.Order)
                        .Select(level => new MsfScalePointDto(level.Order, level.Label, level.Description))
                        .ToList());

        // A named scale with no levels offers nothing to choose, so the question falls back to the default rather than
        // becoming one no respondent could answer.
        return scaleQuestions.ToDictionary(
            question => question.Id,
            question => question.ScaleId is int scaleId && levelsByScale.TryGetValue(scaleId, out var levels) && levels.Count > 0
                ? levels
                : Default);
    }
}
