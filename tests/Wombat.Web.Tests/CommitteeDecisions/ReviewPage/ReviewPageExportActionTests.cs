using Bunit;
using FluentAssertions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;

namespace Wombat.Web.Tests.CommitteeDecisions.ReviewPage;

/// <summary>
/// T379 (flow 13, F1, M1, N5; R3-X01, X03, X05, X08–X10): "Export ‹name›'s portfolio" in the review page's header actions,
/// apart from the subtitle's progress link, only where <c>MayExport</c> (the export command's own check) says the export
/// would not be refused.
/// </summary>
public sealed class ReviewPageExportActionTests : ReviewPageTestBase
{
    [Theory]
    [InlineData(WombatRoles.CommitteeMember)]
    [InlineData(WombatRoles.Coordinator)]
    [InlineData(WombatRoles.SpecialityAdmin)]
    [InlineData(WombatRoles.SubSpecialityAdmin)]
    [InlineData(WombatRoles.InstitutionalAdmin)]
    [InlineData(WombatRoles.Administrator)]
    public void TheHeader_OffersTheExport_WhereMayExport(string role)
    {
        SignInAs(Mbatha, role);
        var cut = Render(Review(CommitteeReviewState.Decided, callerChairs: false) with { MayExport = true });

        var action = cut.Find(".header-container .actions-cell").Children.Should().ContainSingle().Which;
        action.ClassList.Should().BeEquivalentTo("btn", "btn-outline");
        Text(action).Should().Be("Export Lerato Molefe's portfolio");
        action.GetAttribute("href").Should().Be("/portfolio/export/molefe");
        cut.FindAll(".page-subtitle a[href^='/portfolio/export']").Should().BeEmpty("the action is the header's, not the subtitle's");
    }

    [Fact]
    public void TheHeader_OffersNothing_ForATraineeElsewhere()
    {
        // R3-X08: the review's trainee trains elsewhere now, so the export would be refused: no progress link, no action.
        var cut = Render(Review(CommitteeReviewState.Scheduled) with
        {
            MayExport = false,
            TraineeElsewhere = "The trainee no longer trains at this panel's institution."
        });

        cut.FindAll(".header-container .actions-cell").Should().BeEmpty();
        cut.FindAll("a.review-progress-link").Should().BeEmpty();
    }
}
