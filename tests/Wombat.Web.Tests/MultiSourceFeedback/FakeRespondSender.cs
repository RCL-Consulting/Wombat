using MediatR;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.MultiSourceFeedback;

/// <summary>
/// The respondent page's two requests, answered as a test says: the questionnaire (or a refusal of the link) and the
/// submit (recorded, or refused). (T205)
/// </summary>
internal sealed class FakeRespondSender : IScopedSender
{
    public const string TraineeName = "Thandi Nkosi";
    public const int ScaleQuestionId = 11;
    public const int CommentQuestionId = 12;
    public const string ScalePrompt = "Rates the trainee's overall professional performance.";
    public const string CommentPrompt = "What should the trainee keep doing or improve?";

    public static readonly DateOnly LastDay = new(2026, 10, 15);

    /// <summary>The questionnaire the link opens, until <see cref="FormFailureOnCall" /> says otherwise.</summary>
    public MsfResponseFormDto Form { get; set; } = SampleForm();

    /// <summary>What the query throws on its n-th call (1-based), or null to answer <see cref="Form" />.</summary>
    public Func<int, Exception?> FormFailureOnCall { get; set; } = _ => null;

    /// <summary>What the submit throws, or null to accept it.</summary>
    public Exception? SubmitFailure { get; set; }

    public List<string> QueriedTokens { get; } = [];

    public List<SubmitMsfResponseCommand> Submitted { get; } = [];

    public Exception? FormFailure
    {
        set => FormFailureOnCall = _ => value;
    }

    public static MsfResponseFormDto SampleForm(string? traineeName = TraineeName) => new(
        "Annual MSF",
        MsfTemplateKind.Msf,
        traineeName,
        LastDay,
        MsfRespondentCategory.Nurse,
        [
            new MsfResponsePromptDto(ScaleQuestionId, ScalePrompt, MsfQuestionType.Scale, true, MsfRatingScale.Default),
            new MsfResponsePromptDto(CommentQuestionId, CommentPrompt, MsfQuestionType.LongText, false, [])
        ]);

    /// <summary>A learner-feedback questionnaire (T164), as a learner the trainee taught is sent it.</summary>
    public static MsfResponseFormDto LearnerFeedbackForm() => new(
        "Learner feedback (interim questionnaire)",
        MsfTemplateKind.LearnerFeedback,
        TraineeName,
        LastDay,
        MsfRespondentCategory.Learner,
        [
            new MsfResponsePromptDto(ScaleQuestionId, "Rates the trainee's teaching overall.", MsfQuestionType.Scale, true, MsfRatingScale.Default),
            new MsfResponsePromptDto(CommentQuestionId, "What should the trainee keep doing or change in their teaching?", MsfQuestionType.LongText, false, [])
        ]);

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (request is not GetMsfResponseFormQuery query)
        {
            throw new InvalidOperationException($"The respondent page sent {request.GetType().Name}, which it has no business sending.");
        }

        QueriedTokens.Add(query.Token);
        if (FormFailureOnCall(QueriedTokens.Count) is { } failure)
        {
            return Task.FromException<TResponse>(failure);
        }

        return Task.FromResult((TResponse)(object)Form);
    }

    public Task Send(IRequest request, CancellationToken cancellationToken = default)
    {
        if (request is not SubmitMsfResponseCommand command)
        {
            throw new InvalidOperationException($"The respondent page sent {request.GetType().Name}, which it has no business sending.");
        }

        Submitted.Add(command);
        return SubmitFailure is null ? Task.CompletedTask : Task.FromException(SubmitFailure);
    }
}
