using Bunit;
using FluentAssertions;
using Wombat.Application.Features.Curricula;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using FakeSender = Wombat.Web.Tests.Admin.CurriculumItemsFakeSender;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T211, T392 (flow 15, Q3, Q14, F3, F6, P2): the items list offers Edit and Remove… only on the items the caller may
/// change, as the query marks them (<see cref="CurriculumItemDto.CanEdit" />); a national row a caller may not change offers
/// View, its record, beside "Set by the College."; the words follow <see cref="CurriculumDto.CanEditCurriculum" />; the
/// add link only where <see cref="CurriculumDto.MayAddItem" /> holds and an EPA is left. The page never works the rule out
/// from the caller's roles: it reads the query's answer, so these tests give it the answer and check what it offers.
/// </summary>
public sealed class CurriculumItemsScopeTests : CurriculumItemsTestBase
{
    private const int InstitutionA = 40;

    [Fact]
    public void AnInstitution_IsOfferedEditAndRemove_OnlyOnItsOwnItem_AndViewOnTheNationalOnes()
    {
        SignIn(WombatRoles.InstitutionalAdmin);
        var cut = RenderList(InstitutionsView());

        foreach (var code in new[] { "PAED-001", "PAED-002" })
        {
            Actions(Row(cut, code)).Should().Equal(["View", "Set by the College."],
                $"{code} is the College's: its record, and why it offers no Edit (P2, M1)");
            Row(cut, code).QuerySelector("a")!.GetAttribute("href").Should().Be($"/admin/curricula/5/items/{(code == "PAED-001" ? 11 : 12)}");
            Row(cut, code).QuerySelector("a")!.GetAttribute("aria-label").Should().Be($"View {code}");
        }

        var own = Row(cut, "PAED-003");
        Actions(own).Should().Equal("Edit", "Remove…");
        own.QuerySelector("a")!.GetAttribute("href").Should().Be("/admin/curricula/5/items/13", "the item's page of its own (Q14)");
        own.QuerySelector("a")!.GetAttribute("aria-label").Should().Be("Edit PAED-003");
        own.QuerySelector("button")!.GetAttribute("aria-label").Should().Be("Remove PAED-003");
        own.QuerySelector("button")!.GetAttribute("aria-haspopup").Should().Be("dialog", "Remove… opens its dialog (N6)");
        Text(own.QuerySelector("th .muted.text-sm")!).Should().Be("Your institution's own item",
            "the query names no owner to an institution, every local item it reads being its own (T222)");
    }

    [Fact]
    public void AnAdministrator_IsToldWhoseOwnItemEachLocalItemIs_AndEachRowsActionsSayWhose()
    {
        // T222, R3-B17. An Administrator reads every institution's items; the query names each owner, and the row says whose.
        SignIn(WombatRoles.Administrator);
        var cut = RenderList(new FakeSender(
        [
            FakeSender.Item(11, 1, 3, QuotaPeriod.Semester, null),
            FakeSender.Item(12, 2, 3, QuotaPeriod.AcademicYear, null, owningInstitutionId: InstitutionA, owningInstitutionName: "Kgosi Kgari Teaching Hospital")
        ]));

        Row(cut, "PAED-001").QuerySelector("th .muted.text-sm").Should().BeNull("a national item is the College's, and says nothing");
        Text(Row(cut, "PAED-002").QuerySelector("th .muted.text-sm")!).Should().Be("Kgosi Kgari Teaching Hospital's own item");
        Row(cut, "PAED-002").QuerySelector("a")!.GetAttribute("aria-label").Should().Be("Edit PAED-002 (Kgosi Kgari Teaching Hospital's own item)");
        Row(cut, "PAED-002").QuerySelector("button")!.GetAttribute("aria-label").Should().Be("Remove PAED-002 (Kgosi Kgari Teaching Hospital's own item)");
    }

    [Fact]
    public void AnInstitution_ReadsItsNotice_AndItsAdoption_AndNoBackLinks()
    {
        SignIn(WombatRoles.InstitutionalAdmin);
        var cut = RenderList(InstitutionsView());

        cut.Find("h1").TextContent.Trim().Should().Be("Items of Paediatrics v11.1 (11.1)");
        cut.Find(".page-subtitle").TextContent.Trim().Should().Be(
            "CMSA · Paediatrics / General Paediatrics · effective 2026-01-01 · adopted by your institution on 2026-10-30");
        cut.FindAll(".alert.alert-warning").Should().ContainSingle()
            .Which.TextContent.Should().StartWith("The College sets this curriculum's national items, so they are read only here.");
        cut.FindAll("a").Select(link => Text(link)).Should().NotContain(["Back to curricula", "Back to curriculum"],
            "no Back links on the items page for anyone (T5); the trail goes back");
    }

    [Fact]
    public void TheCollege_ReadsItsNotice_ItsSubtitleWithoutAnInstitution_AndEditsEveryNationalItem()
    {
        // R3-B23: the College's words by CanEditCurriculum (F3).
        var cut = RenderList(new FakeSender());

        cut.Find(".page-subtitle").TextContent.Trim().Should().Be("CMSA · Paediatrics / General Paediatrics · effective 2026-01-01");
        cut.Find(".alert.alert-warning").TextContent.Trim().Should().StartWith("A target and its period are read live. Changing one changes what every trainee on this curriculum is measured against, in every institution that has adopted it");
        Actions(Row(cut, "PAED-001")).Should().Equal("Edit", "Remove…");
        Text(cut.Find(".form-section-head a")).Should().Be("Add item");
        cut.Find(".form-section-head a").GetAttribute("href").Should().Be("/admin/curricula/5/items/new");
    }

    [Fact]
    public void ACollegeAdmin_IsOfferedOnlyView_OnAnotherInstitutionsItem()
    {
        // The query does not show a College admin an institution's own item; were one handed to the page, marked as not
        // theirs to change, the page must still offer nothing that would change it.
        var cut = RenderList(new FakeSender(
        [
            FakeSender.Item(11, 1, 3, QuotaPeriod.Semester, null),
            FakeSender.Item(13, 3, 1, QuotaPeriod.AcademicYear, null, owningInstitutionId: InstitutionA, canEdit: false)
        ]));

        Actions(Row(cut, "PAED-003")).Should().Equal("View", "Another institution's item.");
    }

    [Fact]
    public void TheAddLink_IsOffered_OnlyWhereTheCallerMayAdd_AndAnEpaIsLeft()
    {
        SignIn(WombatRoles.InstitutionalAdmin);
        var offered = RenderList(InstitutionsView());
        Text(offered.Find(".form-section-head a")).Should().Be("Add your institution's own item");
        offered.Find(".form-section-head h2").TextContent.Trim().Should().Be("Items (3)");
    }

    [Fact]
    public void AnInstitutionWithNothingLeftToAdd_ReadsTheBuiltEmptyLine_InPlaceOfTheLink()
    {
        // R3-B01: no Add (no EPA it could take), the built empty line.
        SignIn(WombatRoles.InstitutionalAdmin);
        var cut = RenderList(new FakeSender(InstitutionsView().CurrentItems)
        {
            CanEditCurriculum = false,
            Institution = Adopted(),
            EpaList = []
        });

        cut.FindAll(".form-section-head a").Should().BeEmpty();
        Text(cut.Find(".curriculum-items-page > p.muted")).Should().Be(
            "Every EPA your institution could add is already on this curriculum: each national EPA of General Paediatrics, and each of your institution's own EPAs of General Paediatrics. To add an item of your own, first create a local EPA of General Paediatrics.");
    }

    [Fact]
    public void ASupersededVersion_SaysSoInItsSubtitle_OffersNoAdd_AndKeepsTheOwnItemsEditAndRemove()
    {
        // F6, S12 (R3-B21).
        SignIn(WombatRoles.InstitutionalAdmin);
        var sender = new FakeSender(InstitutionsView().CurrentItems)
        {
            CanEditCurriculum = false,
            MayAddItem = false,
            Institution = Adopted() with
            {
                IsActiveAdoption = false,
                SupersededOn = new DateOnly(2026, 10, 30),
                SupersededByName = "Paediatric EPA Curriculum",
                SupersededByVersion = "11.2",
                RegistrarCount = 2
            }
        };
        var cut = RenderList(sender);

        cut.Find(".page-subtitle").TextContent.Trim().Should().Be(
            "CMSA · Paediatrics / General Paediatrics · effective 2026-01-01 · superseded on 2026-10-30 by Paediatric EPA Curriculum (11.2); 2 registrars still on it");
        cut.FindAll(".form-section-head a").Should().BeEmpty();
        Text(cut.Find(".curriculum-items-page > p.muted")).Should().Be(
            "Your institution has superseded this version, so no item is added to it. Its own items stay for the registrars still on it.");
        Actions(Row(cut, "PAED-003")).Should().Equal("Edit", "Remove…");
        sender.EpaOptionQueries.Should().BeEmpty("a caller who may not add is not asked what she could add");
    }

    [Theory]
    [InlineData(1, "; 1 registrar still on it")]
    [InlineData(0, "")]
    public void ASupersededVersionsCount_HasItsSingular_AndNoneIsLeftOut(int registrars, string ending)
    {
        SignIn(WombatRoles.InstitutionalAdmin);
        var cut = RenderList(new FakeSender(InstitutionsView().CurrentItems)
        {
            CanEditCurriculum = false,
            MayAddItem = false,
            Institution = Adopted() with { IsActiveAdoption = false, SupersededOn = new DateOnly(2026, 10, 30), SupersededByName = "P", SupersededByVersion = "11.2", RegistrarCount = registrars }
        });

        cut.Find(".page-subtitle").TextContent.Trim().Should().EndWith($"superseded on 2026-10-30 by P (11.2){ending}");
    }

    [Fact]
    public void ANewVersion_WithoutTheInstitutionsOwnItems_SaysWhereTheyStay()
    {
        // R3-B07: 11.2 holds none of KGK's own items; 11.1 holds one.
        SignIn(WombatRoles.InstitutionalAdmin);
        var cut = RenderList(new FakeSender(
        [
            FakeSender.Item(11, 1, 3, QuotaPeriod.Semester, null, canEdit: false)
        ])
        {
            CanEditCurriculum = false,
            Institution = Adopted() with { OwnItemCount = 0, OwnItemsElsewhere = [new OwnItemsOnVersionDto(2, "11.1", 1)] }
        });

        Text(cut.Find(".form-section-head a")).Should().Be("Add your institution's own item");
        cut.FindAll(".curriculum-items-page > p.muted").Select(Text).Should().Equal(
            "A new version carries no institution's own items. Your item on 11.1 stays there.");
    }

    [Fact]
    public void ACurriculumOutOfScope_IsTheCurriculumUnavailablePage_NamingNoId()
    {
        // Q19 (R2-B16).
        SignIn(WombatRoles.InstitutionalAdmin);
        var cut = RenderList(new FakeSender { Unavailable = true });

        cut.Find("h1").TextContent.Trim().Should().Be("Curriculum unavailable");
        cut.Find(".state-panel-copy").TextContent.Trim().Should().Be("This curriculum does not exist, or you cannot open it.");
        var link = cut.Find(".detail-card--empty a");
        Text(link).Should().Be("Go to Curricula");
        link.GetAttribute("href").Should().Be("/admin/curricula");
        cut.FindAll("table").Should().BeEmpty();
    }

    /// <summary>
    /// What the query answers an Institutional admin of institution A on an adopted curriculum: two national items she may
    /// not change, and one of her own she may.
    /// </summary>
    private static FakeSender InstitutionsView()
        => new(
        [
            FakeSender.Item(11, 1, 3, QuotaPeriod.Semester, null, canEdit: false),
            FakeSender.Item(12, 2, 3, QuotaPeriod.AcademicYear, null, canEdit: false),
            FakeSender.Item(13, 3, 1, QuotaPeriod.AcademicYear, null, owningInstitutionId: InstitutionA)
        ])
        {
            CanEditCurriculum = false,
            Institution = Adopted(),
            EpaList = [FakeSender.Epa(41, "LOC-A01")]
        };

    private static CurriculumInstitutionStateDto Adopted()
        => new(InstitutionA, "Kgosi Kgari Teaching Hospital", true, new DateOnly(2026, 10, 30), null, null, null, 3, 2, 1, []);
}
