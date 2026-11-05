using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Domain.Curricula;
using FakeSender = Wombat.Web.Tests.Admin.CurriculumItemsFakeSender;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// The items table (T392, flow 15, T10, F7, decision 1, decision 3, M1; R3-B01, B06): six columns, compact, stacked from
/// 900px down, captioned without repeating the heading, its row header the EPA, every text cell labelled, the actions in a
/// <c>div.actions-cell</c> inside a plain <c>td</c>, and no row edited in place (Q14, T323). The order is the query's
/// (national items in EPA order, then each institution's own, § 0), which the page keeps.
/// </summary>
public sealed class CurriculumItemsEditLayoutTests : CurriculumItemsTestBase
{
    [Fact]
    public void TheTable_HasSixColumns_NoWindowAndWeight()
    {
        var cut = RenderList(Catalogue());

        cut.FindAll("thead th").Select(Text).Should().Equal(
            "EPA", "Target and decision", "Minimum level", "Minimum by training year", "Tools", "Actions");
        cut.FindAll("thead th").Last().QuerySelector(".visually-hidden").Should().NotBeNull("the Actions header is named, not shown");
    }

    [Fact]
    public void TheTable_IsCompact_StackedWide_AndCaptioned()
    {
        var cut = RenderList(Catalogue());

        var table = cut.Find("table");
        table.ClassList.Should().Contain(["clinic-table", "clinic-table--compact", "clinic-table--stack", "clinic-table--stack-wide"]);
        Text(cut.Find("caption")).Should().Be("Paediatrics v11.1 (11.1)'s items, in EPA order");
        Text(cut.Find(".form-section-head h2")).Should().Be("Items (3)");
        cut.Find(".curriculum-items-page").ClassList.Should().Contain("people-page");
    }

    [Fact]
    public void EachRow_IsHeadedByItsEpa_WithTheEmDash_AndLabelsEveryTextCell()
    {
        var cut = RenderList(Catalogue());

        var row = Row(cut, "PAED-001");
        Text(row.QuerySelector("th[scope=row]")!).Should().Be("PAED-001 — PAED-001 title", "S10: EpaOptionLabel's em dash");
        row.QuerySelectorAll("td").Select(cell => cell.GetAttribute("data-label")).Should().Equal(
            "Target and decision", "Minimum level", "Minimum by training year", "Tools", null);
    }

    [Fact]
    public void TheActions_AreADivInAPlainCell_AndNoRowIsEditedInPlace()
    {
        var cut = RenderList(Catalogue());

        cut.FindAll("td.actions-cell").Should().BeEmpty("M1: a plain td holding one div.actions-cell");
        cut.FindAll("td > div.actions-cell").Should().HaveCount(3);
        cut.FindAll("td[colspan]").Should().BeEmpty("Q14, T323: the item's form is a page of its own");
        cut.FindAll("form, input, select").Should().BeEmpty("the list is the page; nothing on it is a form");
    }

    [Fact]
    public void TheMinimumLevel_ShowsItsRung_OverItsScale()
    {
        var cut = RenderList(Catalogue());

        Text(Cell(cut, "PAED-001", "Minimum level")).Should().Be("5 CPSA Paediatric Entrustment Scale v11.1");
        Text(Cell(cut, "PAED-002", "Minimum level")).Should().Be("3 Not pinned");
    }

    [Theory]
    [InlineData("""{"1":3,"2":4,"3":5,"4":6}""", "3a / 3b / 4 / 5")]
    [InlineData("""{"3":5,"4":6}""", "Year 3: 4 / Year 4: 5")]
    [InlineData(null, "—")]
    public void TheMinimumByTrainingYear_IsFolded_TheYearsNamedOnlyWhereOneIsMissing(string? stages, string expected)
    {
        // T10: "3a / 3b / 4 / 5"; the year labels are on the item's page.
        var cut = RenderList(new FakeSender([FakeSender.Item(11, 1, 3, QuotaPeriod.Semester, null, minimumLevelOrder: 6, stageMinimaJson: stages, scale: FakeSender.CpsaScale)])
        {
            Scales = [FakeSender.CpsaScale]
        });

        Text(Cell(cut, "PAED-001", "Minimum by training year")).Should().Be(expected);
    }

    [Fact]
    public void TheRows_KeepTheQuerysOrder_OwnItemsLast()
    {
        // Decision 3 (built by § 0): the national items, then each institution's own.
        var cut = RenderList(new FakeSender(
        [
            FakeSender.Item(11, 2, 3, QuotaPeriod.Semester, null),
            FakeSender.Item(12, 3, 3, QuotaPeriod.Semester, null),
            FakeSender.Item(13, 1, 1, QuotaPeriod.AcademicYear, null, owningInstitutionId: 40, owningInstitutionName: "Kgosi Kgari Teaching Hospital")
        ]));

        cut.FindAll("tbody th[scope=row]").Select(cell => Text(cell)[..8]).Should().Equal("PAED-002", "PAED-003", "PAED-001");
    }

    [Fact]
    public void TheStandingNotices_AreNotLiveRegions()
    {
        // N4.
        var cut = RenderList(new FakeSender([FakeSender.Item(11, 1, 3, QuotaPeriod.Semester, null, epaIsActive: false)]));

        cut.FindAll(".alert-warning, .alert-info").Should().HaveCount(2)
            .And.OnlyContain(alert => !alert.HasAttribute("role"));
    }

    [Fact]
    public void WhileTheCurriculumIsRead_ThePageIsCurriculumItems_WithItsLoadingLine_AndNoAction()
    {
        // T7, S19 (R3-B18).
        Services.AddSingleton<Wombat.Web.Services.IScopedSender>(new HeldSender());
        var cut = RenderComponent<Wombat.Web.Components.Pages.Admin.Curricula.CurriculumItemsEdit>(
            parameters => parameters.Add(page => page.Id, FakeSender.CurriculumId));

        Text(cut.Find("h1")).Should().Be("Curriculum items");
        Text(cut.Find("p.loading-line[role=status]")).Should().Be("Loading this curriculum's items.");
        cut.FindAll("a.btn, button").Where(action => action.Closest("dialog") is null).Should().BeEmpty(
            "no action is offered until the read is in (the Remove dialog's own buttons are in its closed dialog)");
    }

    [Fact]
    public void ALoadFailure_IsFixedWords_WithTryAgain_AndNoNoticeOrTableUnderIt()
    {
        // S7, T329 (R3-B19).
        var sender = Catalogue();
        sender.ReadFailure = new InvalidOperationException("Npgsql: relation does not exist");
        sender.ReadFailures = 1;
        var cut = RenderList(sender);

        Text(cut.Find("h1")).Should().Be("Curriculum items");
        Text(cut.Find(".action-result .alert-danger .alert-row-text")).Should().Be(
            "Could not load this curriculum's items. Nothing has changed. Try again, or come back in a few minutes.");
        cut.Markup.Should().NotContain("Npgsql");
        cut.FindAll("table, .alert-warning").Should().BeEmpty();

        cut.Find(".action-result button").Click();
        cut.WaitForAssertion(() => cut.FindAll("tbody tr").Should().HaveCount(3));
        Text(cut.Find("h1")).Should().Be("Items of Paediatrics v11.1 (11.1)");
    }

    // ---- T394 (flow 16, lane D): the items as the College keeps them (R3-F05–F09, F18) ----

    [Fact]
    public void TheCollegesNotice_OpensWithTheReach_Bold_ThenTheBuiltNotice()
    {
        var sender = new FakeSender
        {
            Reach = new Wombat.Application.Features.Curricula.CurriculumReachDto(["Kgosi Kgari Teaching Hospital"], 3, [], 0)
        };
        var cut = RenderList(sender);

        var notice = cut.Find(".alert-warning");
        notice.HasAttribute("role").Should().BeFalse("a standing notice is no live region");
        Text(notice.QuerySelector("strong")!).Should().Be("Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training on it.");
        Text(notice).Should().Be(
            "Adopted by 1 institution, Kgosi Kgari Teaching Hospital; 3 registrars in training on it. A target and its period are read live. Changing one changes what every trainee on this curriculum is measured against, in every institution that has adopted it, including periods that have already closed. A tool list is checked when an activity is filed and submitted, and never takes back credit already earned.");
        notice.QuerySelectorAll("strong").Should().ContainSingle("only the first sentence is bold");
    }

    [Fact]
    public void AVersionNoInstitutionHasAdopted_SaysSo_Bold()
    {
        var cut = RenderList(new FakeSender { Reach = Wombat.Application.Features.Curricula.CurriculumReachDto.None });

        Text(cut.Find(".alert-warning strong")).Should().Be("No institution has adopted this version yet.");
    }

    [Fact]
    public void TheHeader_OffersEditThisVersion_ToTheCollege_NamedForTheVersion()
    {
        var cut = RenderList(new FakeSender());

        var link = cut.Find(".header-container .actions-cell a");
        Text(link).Should().Be("Edit this version");
        link.GetAttribute("aria-label").Should().Be("Edit this version of Paediatrics v11.1 (11.1)");
        link.GetAttribute("href").Should().Be("/admin/curricula/5");
        link.ClassList.Should().Contain("btn-outline");
    }

    [Fact]
    public void AnInstitution_IsOfferedNoEditThisVersion_AndNoReach()
    {
        SignIn(Wombat.Domain.Identity.WombatRoles.InstitutionalAdmin);
        var cut = RenderList(new FakeSender { CanEditCurriculum = false, MayAddItem = false });

        cut.FindAll(".header-container .actions-cell a").Should().BeEmpty();
        cut.FindAll(".alert-warning strong").Should().BeEmpty("flow 15's notice, unchanged");
    }

    [Fact]
    public void ADisciplineWithNoNationalEpa_SaysSo_InPlaceOfTheAdd_WithGoToEpas_AndNoTable()
    {
        // Q14 (T326, R3-F06): until T394 the line said every national EPA was already on the curriculum.
        var cut = RenderList(new FakeSender([]) { HasNationalEpas = false, EpaList = [] });

        Text(cut.Find(".form-section-head h2")).Should().Be("Items (0)");
        cut.FindAll(".form-section-head a").Should().BeEmpty("nothing to add");
        Text(cut.Find(".detail-card--empty .state-panel-copy")).Should().Be(
            "General Paediatrics has no national EPA yet. Add one under EPAs, then come back to add it here.");
        var go = cut.Find(".detail-card--empty a.btn-primary");
        Text(go).Should().Be("Go to EPAs");
        go.GetAttribute("href").Should().Be("/admin/epas");
        cut.FindAll("table").Should().BeEmpty("Items (0) draws no table");
        cut.Markup.Should().NotContain("is already on this curriculum");
    }

    [Fact]
    public void TheHeading_CountsTheRowsItsReaderReads()
    {
        // S9 (R3-F18): the Administrator reads an institution's own item too, so her heading counts it.
        SignIn(Wombat.Domain.Identity.WombatRoles.Administrator);
        var sender = new FakeSender(
        [
            FakeSender.Item(11, 1, 3, QuotaPeriod.Semester, null),
            FakeSender.Item(12, 2, 3, QuotaPeriod.AcademicYear, null),
            FakeSender.Item(20, 4, 1, QuotaPeriod.AcademicYear, null, owningInstitutionId: 3, owningInstitutionName: "Kgosi Kgari Teaching Hospital")
        ]);
        var cut = RenderList(sender);

        Text(cut.Find(".form-section-head h2")).Should().Be("Items (3)");
        cut.FindAll("tbody tr").Should().HaveCount(3);
    }

    private static FakeSender Catalogue()
        => new(
        [
            FakeSender.Item(11, 1, 3, QuotaPeriod.Semester, null, minimumLevelOrder: 6, scale: FakeSender.CpsaScale),
            FakeSender.Item(12, 2, 3, QuotaPeriod.AcademicYear, null),
            FakeSender.Item(13, 3, 2, QuotaPeriod.Semester, null)
        ])
        {
            Scales = [FakeSender.CpsaScale]
        };

    /// <summary>A server that never answers: the page as it reads.</summary>
    private sealed class HeldSender : Wombat.Web.Services.IScopedSender
    {
        public Task<TResponse> Send<TResponse>(MediatR.IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => new TaskCompletionSource<TResponse>().Task;

        public Task Send(MediatR.IRequest request, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
