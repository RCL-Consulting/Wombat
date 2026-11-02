using Bunit;
using FluentAssertions;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using FakeSender = Wombat.Web.Tests.Admin.CurriculumItemsFakeSender;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T158, on the items list since T392 (flow 15; R3-B02): an item whose EPA is inactive is still listed, as a record, marked
/// "(inactive: not in force)" beside its title, and a standing notice says what that means. The item's page offers an
/// inactive EPA, labelled so (<c>CurriculumItemsEpaPickerTests</c>).
/// </summary>
public sealed class CurriculumItemsInactiveEpaTests : CurriculumItemsTestBase
{
    public CurriculumItemsInactiveEpaTests()
    {
        SignIn(WombatRoles.Administrator);
    }

    [Fact]
    public void AnItemWhoseEpaIsInactive_IsMarkedNotInForce_AndThePageSaysWhatThatMeans()
    {
        var cut = RenderList(new FakeSender(
        [
            FakeSender.Item(11, 1, 3, QuotaPeriod.Semester, null),
            FakeSender.Item(12, 2, 3, QuotaPeriod.AcademicYear, null, epaIsActive: false)
        ]) { EpaList = FakeSender.EpasWithInactive(2) });

        EpaCell(cut, "PAED-002").Should().Contain("(inactive: not in force)");
        EpaCell(cut, "PAED-001").Should().NotContain("inactive", "an item in force carries no marker");

        var notice = cut.FindAll(".alert.alert-info").Should().ContainSingle().Subject;
        notice.HasAttribute("role").Should().BeFalse("a standing notice is not a live region (N4)");
        Text(notice).Should().StartWith("PAED-002 is inactive, so its item is not in force:")
            .And.Contain("no trainee can choose it for a new activity, its credit is paused")
            .And.EndWith("Reactivating an EPA on its own page brings its item back, and credits what was completed against it meanwhile.");
    }

    // T199 item 1: Razor drops the whitespace before a code block, so the marker was printed against the title.
    [Fact]
    public void TheInactiveMarker_IsSetApartFromTheTitle_ByASpace()
    {
        var cut = RenderList(new FakeSender(
        [
            FakeSender.Item(12, 2, 3, QuotaPeriod.AcademicYear, null, epaIsActive: false)
        ]) { EpaList = FakeSender.EpasWithInactive(2) });

        cut.Find("tbody th[scope=row]").TextContent.Should().Contain("PAED-002 — PAED-002 title (inactive: not in force)",
            "the text as rendered, not collapsed");
    }

    [Fact]
    public void ACurriculumWhoseItemsAreAllInForce_HasNoNotice()
    {
        var cut = RenderList(new FakeSender());

        cut.FindAll(".alert.alert-info").Should().BeEmpty();
        cut.Markup.Should().NotContain("not in force");
    }

    [Fact]
    public void AnInactiveItemsRecord_SaysItIsNotInForce()
    {
        SignIn(WombatRoles.InstitutionalAdmin);
        var cut = RenderItem(new FakeSender([FakeSender.Item(12, 2, 3, QuotaPeriod.AcademicYear, null, epaIsActive: false, canEdit: false)])
        {
            CanEditCurriculum = false
        }, 12);

        Text(cut.FindAll("dl.detail-list dd").First()).Should().Be("PAED-002 — PAED-002 title (inactive: not in force)");
    }

    private static string EpaCell(IRenderedFragment cut, string epaCode) => Text(Row(cut, epaCode).QuerySelector("th")!);
}
