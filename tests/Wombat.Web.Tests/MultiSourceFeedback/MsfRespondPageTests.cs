using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Web.Components.Pages.MultiSourceFeedback;
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.MultiSourceFeedback;

/// <summary>
/// The states of the MSF respondent page: the questionnaire, the thanks, each dead link, and a submission to put right.
/// (T205)
/// </summary>
/// <remarks>
/// The page is served as static HTML (<c>Hosting/MsfRespondPageHostingTests</c> posts it over HTTP); bUnit renders it
/// interactively, which is how the page's own handlers are driven here. Both are the one component.
/// </remarks>
public sealed class MsfRespondPageTests : TestContext
{
    private const string Token = "link-token-1";

    [Fact]
    public void TheQuestionnaire_SaysWhomItIsFor_TheLastDay_AndEveryPointOfTheScale()
    {
        var cut = Render(new FakeRespondSender());

        cut.Find("h1").TextContent.Should().Be($"Feedback on {FakeRespondSender.TraineeName}");
        cut.FindAll("h1").Should().ContainSingle("the card's heading is its one h1 (T369, T280)");
        cut.Find("h1").HasAttribute("tabindex").Should().BeFalse("nothing moves the focus to it");
        cut.Find(".respond-meta").TextContent.Trim().Should().Be("Annual MSF · Last day to respond: 2026-10-15");
        Words(cut.Find(".account-form-container").TextContent).Should().Contain(
            $"You have been asked to give multi-source feedback on {FakeRespondSender.TraineeName}, a trainee you have " +
            "worked with.").And.NotContain("learners");
        var privacy = cut.Find(".respond-privacy");
        privacy.GetAttribute("role").Should().Be("group");
        cut.Find($"#{privacy.GetAttribute("aria-labelledby")}").TextContent.Should().Be("Who sees your feedback");
        privacy.QuerySelectorAll("li").Select(item => Words(item.TextContent).Trim()).Should().Equal(
            [
                $"Your name and email address are never shown to {FakeRespondSender.TraineeName}.",
                $"{FakeRespondSender.TraineeName} sees the feedback only after the request has closed and a coordinator " +
                "has reviewed and released it, grouped by respondent role."
            ],
            "the page promises what the invitation email promises, and no more (T202), before the first question (Q1)");
        privacy.CompareDocumentPosition(cut.Find("fieldset")).HasFlag(AngleSharp.Dom.DocumentPositions.Following)
            .Should().BeTrue("who sees the feedback is said before the rating");

        var legend = cut.Find("fieldset legend");
        legend.TextContent.Should().Contain(FakeRespondSender.ScalePrompt).And.Contain("required");
        cut.FindAll("fieldset input[type=radio]").Select(radio => cut.Find($"label[for='{radio.Id}']").TextContent.Trim())
            .Should().Equal(["Well below expectations", "Below expectations", "Meets expectations", "Above expectations", "Well above expectations"]);
        cut.FindAll("fieldset input[type=radio]").Should().OnlyContain(radio => radio.HasAttribute("required"));

        var comment = cut.Find($"textarea#msf-q{FakeRespondSender.CommentQuestionId}");
        comment.HasAttribute("maxlength").Should().BeFalse(
            "a paste over the limit is refused in words, never cut silently (DESIGN.md § Form system; build review D4)");
        comment.HasAttribute("required").Should().BeFalse("the comment question is optional");
        cut.Find($"label[for='msf-q{FakeRespondSender.CommentQuestionId}']").TextContent.Should().Contain(FakeRespondSender.CommentPrompt);
    }

    /// <summary>
    /// A learner is asked about the trainee's teaching, in the words of their invitation (T164), and is asked nothing
    /// about where they were taught: the coordinator recorded that on the invitation.
    /// </summary>
    [Fact]
    public void ALearnersQuestionnaire_AsksAboutTheTraineesTeaching_AsTheirInvitationDid()
    {
        var sender = new FakeRespondSender { Form = FakeRespondSender.LearnerFeedbackForm() };
        var cut = Render(sender);

        cut.Find("h1").TextContent.Should().Be($"Feedback on the teaching of {FakeRespondSender.TraineeName}");
        var words = Words(cut.Find(".account-form-container").TextContent);
        words.Should().Contain(
            $"You have been asked to give feedback on the teaching of {FakeRespondSender.TraineeName}, a trainee who has " +
            "taught you.");
        words.Should().Contain(
            $"Your name and email address are never shown to {FakeRespondSender.TraineeName}.").And.Contain(
            $"{FakeRespondSender.TraineeName} sees the feedback only after the request has closed and a coordinator has " +
            "reviewed and released it, together with the other learners' feedback.",
            "the page promises what the learner's invitation email promises, and no more (T164)");
        words.Should().Contain($"For each rating, think of the teaching you had from {FakeRespondSender.TraineeName}.");
        words.Should().NotContain("multi-source").And.NotContain("worked with").And.NotContain("respondent role")
            .And.NotContain("stage of training");

        cut.FindAll("fieldset input[type=radio]").Should().HaveCount(5);
        cut.FindAll("textarea").Should().ContainSingle("the questionnaire's one comment question, and no teaching context");
        cut.FindAll("input:not([type=radio]):not([type=hidden]), select").Should().BeEmpty("a learner is never asked where they were taught");

        cut.Find($"#msf-q{FakeRespondSender.ScaleQuestionId}-4").Change("4");
        cut.Find("form").Submit();

        sender.Submitted.Should().ContainSingle().Which.Answers.Should().BeEquivalentTo(
            [new SubmitMsfResponseAnswerItem(FakeRespondSender.ScaleQuestionId, 4, null)]);
        cut.Find(".alert-success").TextContent.Should().Be(
            $"Your feedback on the teaching of {FakeRespondSender.TraineeName} has been recorded.");
    }

    [Fact]
    public void ChoosingAPointAndWritingAComment_SendsBothThroughTheSubmitCommand_AndThanksTheRespondent()
    {
        var sender = new FakeRespondSender();
        var cut = Render(sender);

        cut.Find($"#msf-q{FakeRespondSender.ScaleQuestionId}-4").Change("4");
        cut.Find($"#msf-q{FakeRespondSender.CommentQuestionId}").Input("Calm with anxious parents.");
        cut.Find("form").Submit();

        var command = sender.Submitted.Should().ContainSingle().Which;
        command.Token.Should().Be(Token);
        command.Answers.Should().BeEquivalentTo(
            [
                new SubmitMsfResponseAnswerItem(FakeRespondSender.ScaleQuestionId, 4, null),
                new SubmitMsfResponseAnswerItem(FakeRespondSender.CommentQuestionId, null, "Calm with anxious parents.")
            ],
            options => options.WithStrictOrdering());

        cut.Find("h1").TextContent.Should().Be("Thank you");
        cut.Find(".alert-success").TextContent.Should().Be($"Your feedback on {FakeRespondSender.TraineeName} has been recorded.");
        Words(cut.Markup).Should().Contain("will not open the questionnaire again");
        cut.FindAll("input[type=radio], textarea, button[type=submit]").Should().BeEmpty();
    }

    [Theory]
    [InlineData(MsfResponseRefusal.LinkNotRecognised, "Feedback link not recognised")]
    [InlineData(MsfResponseRefusal.LinkExpired, "Feedback link expired")]
    [InlineData(MsfResponseRefusal.LinkRevoked, "Feedback link revoked")]
    [InlineData(MsfResponseRefusal.LinkUsed, "Feedback link already used")]
    [InlineData(MsfResponseRefusal.CampaignNotOpen, "Feedback request closed")]
    public void ADeadLink_ShowsItsTitleAndItsOwnMessage_AndNoQuestionnaire(MsfResponseRefusal reason, string title)
    {
        const string message = "The refusal's own message, written for the respondent.";
        var cut = Render(new FakeRespondSender { FormFailure = new MsfResponseRefusedException(reason, message) });

        cut.Find("h1").TextContent.Should().Be(title);
        var alert = cut.Find(".alert");
        alert.TextContent.Trim().Should().Be(message);
        alert.GetAttribute("role").Should().Be("alert");
        cut.FindAll("input[type=radio], textarea, button[type=submit]").Should().BeEmpty();
        cut.Markup.Should().NotContain(FakeRespondSender.TraineeName, "nothing of the campaign is shown through a dead link");
    }

    [Fact]
    public void ASubmitThatLosesTheRaceToAnotherThroughTheSameLink_IsToldTheLinkIsUsed()
    {
        var sender = new FakeRespondSender
        {
            SubmitFailure = new MsfResponseRefusedException(MsfResponseRefusal.LinkUsed, MsfCampaignRules.LinkUsedMessage)
        };
        var cut = Render(sender);

        cut.Find($"#msf-q{FakeRespondSender.ScaleQuestionId}-3").Change("3");
        cut.Find("form").Submit();

        cut.Find("h1").TextContent.Should().Be("Feedback link already used");
        cut.Find(".alert").TextContent.Trim().Should().Be(MsfCampaignRules.LinkUsedMessage);
        cut.FindAll("button[type=submit]").Should().BeEmpty();
    }

    [Fact]
    public void AnIncompleteSubmission_SaysWhatIsMissing_AndKeepsTheQuestionnaireAsTyped()
    {
        const string missing = "A response is required for 'Rates the trainee's overall professional performance.'.";
        var sender = new FakeRespondSender
        {
            SubmitFailure = new MsfResponseRefusedException(MsfResponseRefusal.AnswersIncomplete, missing)
        };
        var cut = Render(sender);

        cut.Find($"#msf-q{FakeRespondSender.CommentQuestionId}").Input("Kept after the refusal.");
        cut.Find("form").Submit();

        var alert = cut.Find(".alert-danger");
        Words(alert.TextContent).Trim().Should().Be($"Your feedback was not recorded. {missing}");
        alert.QuerySelector("strong")!.TextContent.Should().Be("Your feedback was not recorded.");
        alert.GetAttribute("role").Should().Be("alert");
        cut.Find($"#msf-q{FakeRespondSender.CommentQuestionId}").TextContent.Should().Be("Kept after the refusal.");
        cut.FindAll("button[type=submit]").Should().ContainSingle();

        // Put right, and sent again.
        sender.SubmitFailure = null;
        cut.Find($"#msf-q{FakeRespondSender.ScaleQuestionId}-5").Change("5");
        cut.Find("form").Submit();

        sender.Submitted.Should().HaveCount(2);
        cut.Find("h1").TextContent.Should().Be("Thank you");
    }

    /// <summary>
    /// The question a refusal is about points at the refusal, and is marked: the rating's group names the summary, and a
    /// comment box is invalid and described by its help text and the summary. Only that question. (T205 review)
    /// </summary>
    [Fact]
    public void AnIncompleteSubmission_MarksTheQuestionItIsAbout_AndNoOther()
    {
        var sender = new FakeRespondSender
        {
            SubmitFailure = new MsfResponseRefusedException(MsfResponseRefusal.AnswersIncomplete, "Too long.")
            {
                QuestionId = FakeRespondSender.CommentQuestionId
            }
        };
        var cut = Render(sender);
        var commentId = $"msf-q{FakeRespondSender.CommentQuestionId}";

        cut.Find($"#{commentId}").GetAttribute("aria-describedby").Should().Be(FieldHelp.Id(commentId), "guard: before");
        cut.Find($"#msf-q{FakeRespondSender.ScaleQuestionId}-3").Change("3");
        cut.Find("form").Submit();

        var comment = cut.Find($"#{commentId}");
        comment.GetAttribute("aria-invalid").Should().Be("true");
        comment.GetAttribute("aria-describedby").Should().Be($"{FieldHelp.Id(commentId)} {MsfRespond.ErrorSummaryId}");
        comment.ClassList.Should().Contain("input-validation-error");
        Words(cut.Find($"#{MsfRespond.ErrorSummaryId}").TextContent).Trim().Should().Be("Your feedback was not recorded. Too long.");
        cut.Find("fieldset").GetAttribute("aria-describedby").Should().Be(
            MsfRespond.RatingHelpId(FakeRespondSender.ScaleQuestionId), "the rating is not the question at fault: its help alone");
    }

    [Fact]
    public void ASubmissionWithNoAnswersAtAll_IsAskedForThem_InPlainWords()
    {
        // The command's validator refuses an empty answer list before its handler runs.
        var sender = new FakeRespondSender
        {
            SubmitFailure = new FluentValidation.ValidationException([new ValidationFailure("Answers", "'Answers' must not be empty.")])
        };
        var cut = Render(sender);

        cut.Find("form").Submit();

        Words(cut.Find(".alert-danger").TextContent).Trim().Should().Be(
            "Your feedback was not recorded. Answer the questions below before submitting.");
        cut.FindAll("fieldset input[type=radio]").Should().NotBeEmpty();
    }

    [Fact]
    public void ATraineeWithNoNameOnRecord_IsCalledWhatTheInvitationCalledThem_NeverAnId()
    {
        var cut = Render(new FakeRespondSender { Form = FakeRespondSender.SampleForm(traineeName: null) });

        cut.Find("h1").TextContent.Should().Be("Feedback on the trainee named in your invitation");
    }

    [Fact]
    public void AFault_SaysSomethingWentWrong_AndNotWhat()
    {
        const string detail = "fault-detail-for-developers-only";
        var cut = Render(new FakeRespondSender { FormFailure = new InvalidOperationException(detail) });

        cut.Find("h1").TextContent.Should().Be("Something went wrong");
        cut.Markup.Should().NotContain(detail);
        cut.FindAll("button[type=submit]").Should().BeEmpty();
    }

    // ─── T369 (flow 10, boards R3-Q1 to Q9) ──────────────────────────────────

    /// <summary>
    /// Each state keeps its Alert under its h1 (S9): info for a link already used, warning for a closed request and an
    /// expired, revoked or unrecognised link; each says the page can be closed.
    /// </summary>
    [Theory]
    [InlineData(MsfResponseRefusal.LinkUsed, "info")]
    [InlineData(MsfResponseRefusal.CampaignNotOpen, "warning")]
    [InlineData(MsfResponseRefusal.LinkExpired, "warning")]
    [InlineData(MsfResponseRefusal.LinkRevoked, "warning")]
    [InlineData(MsfResponseRefusal.LinkNotRecognised, "warning")]
    public void EachDeadLink_HasItsOwnAlertKind_UnderItsHeading(MsfResponseRefusal reason, string kind)
    {
        var cut = Render(new FakeRespondSender { FormFailure = new MsfResponseRefusedException(reason, "Its message.") });

        cut.FindAll("h1").Should().ContainSingle();
        var alert = cut.Find(".alert");
        alert.ClassList.Should().Contain($"alert-{kind}");
        alert.PreviousElementSibling!.TagName.Should().Be("H1", "the state's Alert sits under its heading");
        alert.NextElementSibling!.TextContent.Trim().Should().Be("You can close this page.");
    }

    /// <summary>The fault (Q8): a danger Alert under its h1, and "You can close this page." under it, as every dead link.</summary>
    [Fact]
    public void AFault_IsADangerAlert_AndSaysThePageCanBeClosed()
    {
        var cut = Render(new FakeRespondSender { FormFailure = new InvalidOperationException("fault") });

        cut.FindAll("h1").Should().ContainSingle();
        var alert = cut.Find(".alert");
        alert.ClassList.Should().Contain("alert-danger");
        alert.NextElementSibling!.TextContent.Trim().Should().Be("You can close this page.");
    }

    /// <summary>
    /// The questionnaire (Q1): the mark at 44px; the rating one fieldset, its legend the question with the required mark
    /// seen and "required" heard, its help under the legend and naming it; the optional comment says so, and its help
    /// reads "Up to 4,000 characters" (no counter: the page carries no script).
    /// </summary>
    [Fact]
    public void TheQuestionnaire_MarksWhatIsRequired_AndHelpsEachQuestion()
    {
        var cut = Render(new FakeRespondSender());

        var mark = cut.Find(".account-brand-mark");
        mark.GetAttribute("width").Should().Be("44");
        mark.GetAttribute("height").Should().Be("44");

        cut.Find(".respond-required").TextContent.Should().Be("Questions marked * must be answered.");
        var legend = cut.Find("fieldset legend");
        legend.QuerySelector(".required-mark")!.GetAttribute("aria-hidden").Should().Be("true");
        legend.QuerySelector(".visually-hidden")!.TextContent.Trim().Should().Be("required");

        var helpId = MsfRespond.RatingHelpId(FakeRespondSender.ScaleQuestionId);
        cut.Find("fieldset").GetAttribute("aria-describedby").Should().Be(helpId);
        cut.Find($"#{helpId}").TextContent.Should().Be(
            $"For each rating, compare {FakeRespondSender.TraineeName} with what you would expect of a trainee at their " +
            "stage of training.");

        var commentId = $"msf-q{FakeRespondSender.CommentQuestionId}";
        cut.Find($"label[for='{commentId}'] .respond-optional").TextContent.Trim().Should().Be("(optional)");
        cut.Find($"#{FieldHelp.Id(commentId)}").TextContent.Should().StartWith("Up to 4,000 characters.");
        cut.FindAll("script").Should().BeEmpty();
        cut.Find("form").ClassList.Should().Contain("respond-form");
        cut.Find("form > button[type=submit]").TextContent.Trim().Should().Be("Submit feedback");
    }

    /// <summary>Text with every run of whitespace, line breaks in the markup included, read as one space.</summary>
    private static string Words(string text) => System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");

    private IRenderedComponent<MsfRespond> Render(FakeRespondSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);
        Services.GetRequiredService<FakeNavigationManager>().NavigateTo($"/msf/respond?token={Token}");
        return RenderComponent<MsfRespond>();
    }
}
