using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Curricula;
using Wombat.Web.Services;
using FakeSender = Wombat.Web.Tests.Admin.CurriculumItemsFakeSender;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T158: the curriculum item editor says which items are not in force.
/// </summary>
/// <remarks>
/// The editor reads items as records, so an item whose EPA is deactivated stays in its table. Since T158 that item is
/// on no progress page and its credit is paused (T196, D48). Unmarked, it reads as a live target that no trainee can see, and the admin
/// has nothing to tell them why.
/// </remarks>
public sealed class CurriculumItemsInactiveEpaTests : TestContext
{
    public CurriculumItemsInactiveEpaTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("admin@test");
        auth.SetRoles(WombatRoles.Administrator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "admin-1"));
    }

    [Fact]
    public void AnItemWhoseEpaIsInactive_IsMarkedNotInForce_AndThePageSaysWhatThatMeans()
    {
        var cut = RenderPage(new FakeSender(
        [
            FakeSender.Item(11, 1, 3, QuotaPeriod.Semester, null),
            FakeSender.Item(12, 2, 3, QuotaPeriod.AcademicYear, null, epaIsActive: false)
        ]) { EpaList = FakeSender.EpasWithInactive(2) });

        EpaCell(cut, "PAED-002").Should().Contain("(inactive: not in force)");
        EpaCell(cut, "PAED-001").Should().NotContain("inactive", "an item in force carries no marker");

        var notice = cut.FindAll(".alert.alert-info").Should().ContainSingle().Subject;
        Text(notice).Should().StartWith("PAED-002 is inactive, so its item is not in force:")
            .And.Contain("no trainee can choose it for a new activity, its credit is paused")
            .And.Contain("Reactivating an EPA on its own page brings its item back, and credits what was completed against it meanwhile.");
    }

    // T199 item 1: Razor drops the whitespace before a code block, so the marker was printed against the title,
    // "…PAED-002 title(inactive: not in force)".
    [Fact]
    public void TheInactiveMarker_IsSetApartFromTheTitle_ByASpace()
    {
        var cut = RenderPage(new FakeSender(
        [
            FakeSender.Item(12, 2, 3, QuotaPeriod.AcademicYear, null, epaIsActive: false)
        ]) { EpaList = FakeSender.EpasWithInactive(2) });

        var cell = cut.FindAll("tbody tr").Select(row => row.QuerySelector("td")!).First();
        cell.TextContent.Should().Contain("PAED-002 title (inactive: not in force)", "the text as rendered, not collapsed");
    }

    [Fact]
    public void ACurriculumWhoseItemsAreAllInForce_HasNoNotice()
    {
        var cut = RenderPage(new FakeSender());

        cut.FindAll(".alert.alert-info").Should().BeEmpty();
        cut.Markup.Should().NotContain("not in force");
    }

    [Fact]
    public void AnInactiveEpa_IsLabelledSoInTheAddFormsPicker_ButCanStillBeChosen()
    {
        var cut = RenderPage(new FakeSender { EpaList = FakeSender.EpasWithInactive(3) });

        cut.FindAll("#curriculum-item-epa option").Select(option => Text(option)).Should().Equal(
            "PAED-001 - PAED-001 title",
            "PAED-002 - PAED-002 title",
            "PAED-003 - PAED-003 title (inactive)");
    }

    private IRenderedComponent<CurriculumItemsEdit> RenderPage(FakeSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<CurriculumItemsEdit>(parameters => parameters.Add(page => page.Id, FakeSender.CurriculumId));
        cut.WaitForState(() => cut.FindAll("tbody tr").Count > 0);

        return cut;
    }

    private static string EpaCell(IRenderedComponent<CurriculumItemsEdit> cut, string epaCode)
        => Text(cut.FindAll("tbody tr")
            .Select(row => row.QuerySelector("td"))
            .First(cell => cell is not null && cell.TextContent.Trim().StartsWith(epaCode, StringComparison.Ordinal))!);

    private static string Text(IElement element)
        => System.Text.RegularExpressions.Regex.Replace(element.TextContent, @"\s+", " ").Trim();
}
