using Bunit;
using FluentAssertions;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Web.Components.Pages.MultiSourceFeedback;
using Wombat.Web.Components.Shared;
using static Wombat.Web.Tests.MultiSourceFeedback.MsfReportFixtures;

namespace Wombat.Web.Tests.MultiSourceFeedback;

/// <summary>
/// The respondent groups' cards (T369, build-contracts § 1; round 3 M2, S7; T249, T270), which the coordinator's report and
/// her own report both mount.
/// </summary>
public sealed class MsfGroupCardsTests : TestContext
{
    [Fact]
    public void TheCoordinatorsCards_AreTheGroupsShown_InTheOrderGiven_EachWithItsAverageAndCount()
    {
        var cut = Cards(MolefeGroups(), forTrainee: false);

        cut.FindAll("section.detail-card.group-card h3").Select(Text).Should().Equal(
            "Consultant · 4.33 average rating, from 3 responses", "Nurse · 4.67 average rating, from 3 responses");
        cut.Markup.Should().NotContain("Peer doctor").And.NotContain("Allied health professional",
            "nothing about a suppressed group: its line is the page's");
        cut.FindAll(".group-question").Select(Text).Should().Equal(Question, Question);
        cut.FindAll(".group-comments").First().QuerySelectorAll("li").Select(Text).Should().Equal(
            "Sound judgement in clinic; teaches the interns well",
            "Ready to run a ward; should document escalation decisions more fully",
            "Keeps the team calm");
        cut.Markup.Should().NotContain(".:", "the question once, never \"Prompt.: 4.00\" (T270)");
        foreach (var section in cut.FindAll("section.group-card"))
        {
            section.GetAttribute("aria-labelledby").Should().Be(section.QuerySelector("h3")!.Id);
        }
    }

    [Fact]
    public void HerCards_CarryNoCount()
    {
        var cut = Cards(MolefeGroups(), forTrainee: true);

        cut.FindAll("h3").Select(Text).Should().Equal("Consultant · 4.33 average rating", "Nurse · 4.67 average rating");
        cut.Markup.Should().NotContain("responses");
    }

    [Fact]
    public void AGroupWithNoScaleQuestion_IsHeadedByItsNameAlone()
    {
        var group = new MsfCategoryAggregateDto(MsfRespondentCategory.Consultant, 3, false,
            [new MsfQuestionAggregateDto(2, Question, MsfQuestionType.LongText, null, ["one", "two", "three"])]);

        var cut = Cards([group], forTrainee: false);

        Text(cut.Find("h3")).Should().Be("Consultant");
        cut.FindAll(".group-avg").Should().BeEmpty();
    }

    /// <summary>S7: several scale questions, the group's name alone, then each question's average under it.</summary>
    [Fact]
    public void AGroupWithSeveralScaleQuestions_ListsEachAverageUnderItsName()
    {
        var group = new MsfCategoryAggregateDto(MsfRespondentCategory.Nurse, 3, false,
        [
            new MsfQuestionAggregateDto(1, "Clinical care", MsfQuestionType.Scale, new MsfScaleAggregateDto(4.0, 3), []),
            new MsfQuestionAggregateDto(3, "Communication", MsfQuestionType.Scale, new MsfScaleAggregateDto(3.5, 2), []),
            new MsfQuestionAggregateDto(2, Question, MsfQuestionType.LongText, null, [])
        ]);

        var cut = Cards([group], forTrainee: false);

        Text(cut.Find("h3")).Should().Be("Nurse");
        cut.FindAll(".group-card p").Select(Text).Should().Equal(
            "Clinical care", "4.00 average rating, from 3 responses",
            "Communication", "3.50 average rating, from 2 responses");
        cut.Markup.Should().NotContain(Question, "a question nobody commented on is left out");
    }

    [Fact]
    public void TheHeadingLevel_CanBeAnother()
    {
        var cut = RenderComponent<MsfGroupCards>(parameters => parameters
            .Add(cards => cards.Categories, MolefeGroups())
            .Add(cards => cards.HeadingLevel, "h2"));

        cut.FindAll("h2").Should().HaveCount(2);
        cut.FindAll("h3").Should().BeEmpty();
    }

    [Fact]
    public void TheWordsHerReportUses_AreTheReportsOwn()
    {
        MsfReportText.Responses(1).Should().Be("1 response");
        MsfReportText.Responses(3).Should().Be("3 responses");
        MsfReportText.RatingScaleNote.Should().Be(
            "Ratings are averaged from 1, Well below expectations, to 5, Well above expectations.");
    }

    /// <summary>The release mail links where her reports are (build-contracts § 3).</summary>
    [Fact]
    public void TheReleaseMailsLink_IsMsfReports()
        => ReleaseMsfCampaignCommandHandler.MyReportsPath.Should().Be(MsfLinks.MyReports);

    private IRenderedComponent<MsfGroupCards> Cards(IReadOnlyList<MsfCategoryAggregateDto> groups, bool forTrainee)
        => RenderComponent<MsfGroupCards>(parameters => parameters
            .Add(cards => cards.Categories, groups)
            .Add(cards => cards.ForTrainee, forTrainee));
}
